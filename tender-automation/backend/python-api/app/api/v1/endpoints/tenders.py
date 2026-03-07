"""Tender management API endpoints."""
from typing import List, Optional
from uuid import UUID
from datetime import datetime, timezone
from fastapi import APIRouter, Depends, HTTPException, status, Query
from pydantic import BaseModel, Field, field_validator
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, func, or_

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.models.tender import Tender
from app.models.document import Document
from app.models.document import TenderDocument

router = APIRouter()


def _strip_tz(dt: Optional[datetime]) -> Optional[datetime]:
    """Convert timezone-aware datetime to naive UTC datetime for SQL Server compatibility."""
    if dt is None:
        return None
    if dt.tzinfo is not None:
        # Convert to UTC then remove timezone info
        dt = dt.astimezone(timezone.utc).replace(tzinfo=None)
    # Remove microseconds to avoid SQL Server precision issues
    return dt.replace(microsecond=0)


# Request/Response schemas
class TenderCreate(BaseModel):
    """Schema for creating a tender."""
    title: str = Field(..., min_length=1, max_length=500)
    reference_number: Optional[str] = Field(None, max_length=100)
    issuing_authority: Optional[str] = Field(None, max_length=300)
    portal_name: Optional[str] = Field(None, max_length=100)
    portal_url: Optional[str] = None
    deadline: Optional[datetime] = None
    estimated_value: Optional[float] = None
    requirements: Optional[dict] = None
    notes: Optional[str] = None
    status: str = Field("draft", max_length=50)

    @field_validator("deadline", mode="before")
    @classmethod
    def normalize_deadline(cls, v):
        if v is None:
            return None
        if isinstance(v, str):
            # Parse ISO string
            try:
                v = datetime.fromisoformat(v.replace("Z", "+00:00"))
            except ValueError:
                return None
        if isinstance(v, datetime):
            return _strip_tz(v)
        return v


class TenderUpdate(BaseModel):
    """Schema for updating a tender."""
    title: Optional[str] = Field(None, min_length=1, max_length=500)
    reference_number: Optional[str] = Field(None, max_length=100)
    issuing_authority: Optional[str] = Field(None, max_length=300)
    portal_name: Optional[str] = Field(None, max_length=100)
    portal_url: Optional[str] = None
    deadline: Optional[datetime] = None
    estimated_value: Optional[float] = None
    requirements: Optional[dict] = None
    notes: Optional[str] = None
    status: Optional[str] = Field(None, max_length=50)

    @field_validator("deadline", mode="before")
    @classmethod
    def normalize_deadline(cls, v):
        if v is None:
            return None
        if isinstance(v, str):
            try:
                v = datetime.fromisoformat(v.replace("Z", "+00:00"))
            except ValueError:
                return None
        if isinstance(v, datetime):
            return _strip_tz(v)
        return v


class TenderResponse(BaseModel):
    """Schema for tender response."""
    id: UUID
    tenant_id: UUID
    created_by: UUID
    title: str
    reference_number: Optional[str]
    issuing_authority: Optional[str]
    portal_name: Optional[str]
    portal_url: Optional[str]
    deadline: Optional[datetime]
    estimated_value: Optional[float]
    requirements: Optional[dict]
    notes: Optional[str]
    status: str
    created_at: datetime
    updated_at: datetime
    document_count: int = 0

    class Config:
        from_attributes = True


class TenderListResponse(BaseModel):
    """Schema for paginated tender list."""
    items: List[TenderResponse]
    total: int
    page: int
    page_size: int
    total_pages: int


class TenderDocumentAssociation(BaseModel):
    """Schema for associating document with tender."""
    document_id: UUID
    document_order: int = 0
    is_generated: bool = False
    generation_prompt: Optional[str] = None


class TenderDocumentResponse(BaseModel):
    """Schema for tender document association response."""
    id: UUID
    tender_id: UUID
    document_id: UUID
    document_order: int
    is_generated: bool
    generation_prompt: Optional[str]
    created_at: datetime
    document: Optional[dict] = None

    class Config:
        from_attributes = True


@router.post("/", response_model=TenderResponse, status_code=status.HTTP_201_CREATED)
async def create_tender(
    tender: TenderCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Create a new tender.

    **Status Values:**
    - `draft`: Initial state
    - `in_progress`: Work in progress
    - `submitted`: Submitted to authority
    - `won`: Tender awarded
    - `lost`: Tender not awarded
    - `cancelled`: Tender cancelled

    Requires authentication.
    """
    new_tender = Tender(
        tenant_id=UUID(current_user.tenant_id),
        created_by=UUID(current_user.user_id),
        **tender.model_dump()
    )

    db.add(new_tender)
    await db.commit()
    await db.refresh(new_tender)

    return TenderResponse(**new_tender.__dict__, document_count=0)


@router.get("/", response_model=TenderListResponse)
async def list_tenders(
    page: int = Query(1, ge=1),
    page_size: int = Query(50, ge=1, le=500),
    status_filter: Optional[str] = Query(None, alias="status"),
    search: Optional[str] = None,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get list of tenders with pagination and filtering.

    **Filters:**
    - status: Filter by status (draft, in_progress, submitted, won, lost, cancelled)
    - search: Search in title, reference, or authority name

    Requires authentication.
    """
    # Base query
    query = select(Tender).where(Tender.tenant_id == UUID(current_user.tenant_id))

    # Apply status filter
    if status_filter:
        query = query.where(Tender.status == status_filter)

    # Apply search filter
    if search:
        search_pattern = f"%{search}%"
        query = query.where(
            or_(
                Tender.title.ilike(search_pattern),
                Tender.reference_number.ilike(search_pattern),
                Tender.issuing_authority.ilike(search_pattern)
            )
        )

    # Get total count
    count_query = select(func.count()).select_from(query.subquery())
    total_result = await db.execute(count_query)
    total = total_result.scalar()

    # Apply pagination
    query = query.order_by(Tender.created_at.desc())
    query = query.offset((page - 1) * page_size).limit(page_size)

    # Execute query
    result = await db.execute(query)
    tenders = result.scalars().all()

    # Get document counts for each tender
    tender_responses = []
    for tender in tenders:
        doc_count_query = select(func.count()).where(
            TenderDocument.tender_id == tender.id
        )
        doc_count_result = await db.execute(doc_count_query)
        doc_count = doc_count_result.scalar()

        tender_responses.append(
            TenderResponse(**tender.__dict__, document_count=doc_count)
        )

    total_pages = (total + page_size - 1) // page_size

    return TenderListResponse(
        items=tender_responses,
        total=total,
        page=page,
        page_size=page_size,
        total_pages=total_pages
    )


@router.get("/upcoming", response_model=List[TenderResponse])
async def get_upcoming_tenders(
    days: int = Query(7, ge=1, le=90, description="Number of days to look ahead"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get tenders with upcoming deadlines.

    Returns tenders with deadlines in the next N days (default: 7).

    Requires authentication.
    """
    from datetime import timedelta

    end_date = datetime.utcnow() + timedelta(days=days)

    query = select(Tender).where(
        Tender.tenant_id == UUID(current_user.tenant_id),
        Tender.deadline.isnot(None),
        Tender.deadline >= datetime.utcnow(),
        Tender.deadline <= end_date,
        Tender.status.in_(['draft', 'in_progress'])
    ).order_by(Tender.deadline)

    result = await db.execute(query)
    tenders = result.scalars().all()

    # Get document counts
    tender_responses = []
    for tender in tenders:
        doc_count_query = select(func.count()).where(
            TenderDocument.tender_id == tender.id
        )
        doc_count_result = await db.execute(doc_count_query)
        doc_count = doc_count_result.scalar()

        tender_responses.append(
            TenderResponse(**tender.__dict__, document_count=doc_count)
        )

    return tender_responses


@router.get("/{tender_id}", response_model=TenderResponse)
async def get_tender(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get tender by ID.

    Requires authentication.
    """
    result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id)
        )
    )
    tender = result.scalar_one_or_none()

    if not tender:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found"
        )

    # Get document count
    doc_count_query = select(func.count()).where(
        TenderDocument.tender_id == tender.id
    )
    doc_count_result = await db.execute(doc_count_query)
    doc_count = doc_count_result.scalar()

    return TenderResponse(**tender.__dict__, document_count=doc_count)


@router.put("/{tender_id}", response_model=TenderResponse)
async def update_tender(
    tender_id: UUID,
    tender_update: TenderUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Update tender by ID.

    Only updates fields that are provided (partial update).

    Requires authentication.
    """
    result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id)
        )
    )
    tender = result.scalar_one_or_none()

    if not tender:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found"
        )

    # Update only provided fields
    update_data = tender_update.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(tender, field, value)

    tender.updated_at = datetime.utcnow()

    await db.commit()
    await db.refresh(tender)

    # Get document count
    doc_count_query = select(func.count()).where(
        TenderDocument.tender_id == tender.id
    )
    doc_count_result = await db.execute(doc_count_query)
    doc_count = doc_count_result.scalar()

    return TenderResponse(**tender.__dict__, document_count=doc_count)


@router.delete("/{tender_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_tender(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Delete tender by ID.

    This also deletes all tender-document associations (cascade).

    Requires authentication.
    """
    result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id)
        )
    )
    tender = result.scalar_one_or_none()

    if not tender:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found"
        )

    await db.delete(tender)
    await db.commit()


@router.post("/{tender_id}/documents", response_model=TenderDocumentResponse, status_code=status.HTTP_201_CREATED)
async def associate_document(
    tender_id: UUID,
    association: TenderDocumentAssociation,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Associate a document with a tender.

    **Fields:**
    - document_id: ID of the document to associate
    - document_order: Display order (default: 0)
    - is_generated: Whether this is an AI-generated document
    - generation_prompt: Prompt used for AI generation (if applicable)

    Requires authentication.
    """
    # Verify tender exists and belongs to user's tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id)
        )
    )
    tender = tender_result.scalar_one_or_none()

    if not tender:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found"
        )

    # Verify document exists and belongs to user's tenant
    doc_result = await db.execute(
        select(Document).where(
            Document.id == association.document_id,
            Document.tenant_id == UUID(current_user.tenant_id)
        )
    )
    document = doc_result.scalar_one_or_none()

    if not document:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Document not found"
        )

    # Check if association already exists
    existing_result = await db.execute(
        select(TenderDocument).where(
            TenderDocument.tender_id == tender_id,
            TenderDocument.document_id == association.document_id
        )
    )
    existing = existing_result.scalar_one_or_none()

    if existing:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Document is already associated with this tender"
        )

    # Create association
    tender_doc = TenderDocument(
        tender_id=tender_id,
        document_id=association.document_id,
        document_order=association.document_order,
        is_generated=association.is_generated,
        generation_prompt=association.generation_prompt
    )

    db.add(tender_doc)
    await db.commit()
    await db.refresh(tender_doc)

    return TenderDocumentResponse(**tender_doc.__dict__)


@router.get("/{tender_id}/documents", response_model=List[TenderDocumentResponse])
async def get_tender_documents(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get all documents associated with a tender.

    Returns documents in order (by document_order field).

    Requires authentication.
    """
    # Verify tender exists and belongs to user's tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id)
        )
    )
    tender = tender_result.scalar_one_or_none()

    if not tender:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found"
        )

    # Get tender documents with document details
    result = await db.execute(
        select(TenderDocument, Document)
        .join(Document, TenderDocument.document_id == Document.id)
        .where(TenderDocument.tender_id == tender_id)
        .order_by(TenderDocument.document_order)
    )
    rows = result.all()

    responses = []
    for tender_doc, document in rows:
        doc_dict = {
            "id": document.id,
            "name": document.name,
            "file_path": document.file_path,
            "mime_type": document.mime_type,
            "file_size": document.file_size,
            "document_type": document.document_type,
            "tags": document.tags,
            "created_at": document.created_at
        }

        responses.append(
            TenderDocumentResponse(
                **tender_doc.__dict__,
                document=doc_dict
            )
        )

    return responses


class ReorderDocumentsRequest(BaseModel):
    """Schema for reordering tender documents."""
    document_ids: List[UUID]


@router.put("/{tender_id}/documents/reorder")
async def reorder_documents(
    tender_id: UUID,
    request: ReorderDocumentsRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Reorder documents within a tender.

    Accepts an ordered list of document_ids. Each document's order
    is updated to match its position in the list (0-based).

    Requires authentication.
    """
    # Verify tender exists and belongs to user's tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id)
        )
    )
    tender = tender_result.scalar_one_or_none()

    if not tender:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found"
        )

    # Update document_order for each document
    for order, doc_id in enumerate(request.document_ids):
        result = await db.execute(
            select(TenderDocument).where(
                TenderDocument.tender_id == tender_id,
                TenderDocument.document_id == doc_id
            )
        )
        tender_doc = result.scalar_one_or_none()
        if tender_doc:
            tender_doc.document_order = order

    await db.commit()

    return {"message": f"Reordered {len(request.document_ids)} documents"}


@router.delete("/{tender_id}/documents/{document_id}", status_code=status.HTTP_204_NO_CONTENT)
async def remove_document_association(
    tender_id: UUID,
    document_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Remove document association from tender.

    Note: This only removes the association, not the document itself.

    Requires authentication.
    """
    # Verify tender exists and belongs to user's tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id)
        )
    )
    tender = tender_result.scalar_one_or_none()

    if not tender:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found"
        )

    # Find and delete association
    result = await db.execute(
        select(TenderDocument).where(
            TenderDocument.tender_id == tender_id,
            TenderDocument.document_id == document_id
        )
    )
    tender_doc = result.scalar_one_or_none()

    if not tender_doc:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Document association not found"
        )

    await db.delete(tender_doc)
    await db.commit()
