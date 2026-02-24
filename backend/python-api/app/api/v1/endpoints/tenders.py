"""
Tender management endpoints
"""
from fastapi import APIRouter, Depends, HTTPException, Query
from sqlalchemy.orm import Session
from sqlalchemy import select, func, delete
from typing import Optional
import uuid

from app.core.database import get_db
from app.core.security import get_current_user, TokenData
from app.models.tender import Tender, TenderDocument
from app.schemas.tender import (
    TenderCreate, TenderUpdate, TenderResponse, TenderListResponse
)

router = APIRouter()


@router.post("", response_model=TenderResponse, status_code=201)
def create_tender(
    tender_data: TenderCreate,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """Create a new tender"""
    tender = Tender(
        **tender_data.model_dump(),
        tenant_id=uuid.UUID(current_user.tenant_id),
        created_by=uuid.UUID(current_user.user_id)
    )

    db.add(tender)
    db.commit()
    db.refresh(tender)

    response = TenderResponse.model_validate(tender)
    response.document_count = 0
    return response


@router.get("", response_model=TenderListResponse)
def list_tenders(
    page: int = Query(1, ge=1),
    page_size: int = Query(50, ge=1, le=100),
    status: Optional[str] = None,
    search: Optional[str] = None,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """List tenders with pagination and filters"""
    query = select(Tender).where(Tender.tenant_id == uuid.UUID(current_user.tenant_id))

    # Apply filters
    if status:
        query = query.where(Tender.status == status)

    if search:
        search_term = f"%{search}%"
        query = query.where(
            (Tender.title.ilike(search_term)) |
            (Tender.reference_number.ilike(search_term)) |
            (Tender.issuing_authority.ilike(search_term))
        )

    # Get total count
    count_query = select(func.count()).select_from(query.subquery())
    total_result = db.execute(count_query)
    total = total_result.scalar()

    # Apply pagination
    query = query.order_by(Tender.created_at.desc())
    query = query.offset((page - 1) * page_size).limit(page_size)

    # Execute query
    result = db.execute(query)
    tenders = result.scalars().all()

    # Get document counts for each tender
    tender_responses = []
    for tender in tenders:
        doc_count_query = select(func.count()).select_from(TenderDocument).where(
            TenderDocument.tender_id == tender.id
        )
        doc_count_result = db.execute(doc_count_query)
        doc_count = doc_count_result.scalar() or 0

        tender_response = TenderResponse.model_validate(tender)
        tender_response.document_count = doc_count
        tender_responses.append(tender_response)

    return TenderListResponse(
        items=tender_responses,
        total=total,
        page=page,
        page_size=page_size
    )


@router.get("/{tender_id}", response_model=TenderResponse)
def get_tender(
    tender_id: uuid.UUID,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """Get tender by ID"""
    query = select(Tender).where(
        Tender.id == tender_id,
        Tender.tenant_id == uuid.UUID(current_user.tenant_id)
    )

    result = db.execute(query)
    tender = result.scalar_one_or_none()

    if not tender:
        raise HTTPException(status_code=404, detail="Tender not found")

    # Get document count
    doc_count_query = select(func.count()).select_from(TenderDocument).where(
        TenderDocument.tender_id == tender.id
    )
    doc_count_result = db.execute(doc_count_query)
    doc_count = doc_count_result.scalar() or 0

    tender_response = TenderResponse.model_validate(tender)
    tender_response.document_count = doc_count
    return tender_response


@router.put("/{tender_id}", response_model=TenderResponse)
def update_tender(
    tender_id: uuid.UUID,
    tender_data: TenderUpdate,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """Update tender"""
    query = select(Tender).where(
        Tender.id == tender_id,
        Tender.tenant_id == uuid.UUID(current_user.tenant_id)
    )

    result = db.execute(query)
    tender = result.scalar_one_or_none()

    if not tender:
        raise HTTPException(status_code=404, detail="Tender not found")

    # Update fields
    update_data = tender_data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(tender, field, value)

    db.commit()
    db.refresh(tender)

    # Get document count
    doc_count_query = select(func.count()).select_from(TenderDocument).where(
        TenderDocument.tender_id == tender.id
    )
    doc_count_result = db.execute(doc_count_query)
    doc_count = doc_count_result.scalar() or 0

    tender_response = TenderResponse.model_validate(tender)
    tender_response.document_count = doc_count
    return tender_response


@router.delete("/{tender_id}", status_code=204)
def delete_tender(
    tender_id: uuid.UUID,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """Delete tender"""
    query = select(Tender).where(
        Tender.id == tender_id,
        Tender.tenant_id == uuid.UUID(current_user.tenant_id)
    )

    result = db.execute(query)
    tender = result.scalar_one_or_none()

    if not tender:
        raise HTTPException(status_code=404, detail="Tender not found")

    # Delete associated documents
    db.execute(delete(TenderDocument).where(TenderDocument.tender_id == tender_id))

    # Delete tender
    db.delete(tender)
    db.commit()

    return None
