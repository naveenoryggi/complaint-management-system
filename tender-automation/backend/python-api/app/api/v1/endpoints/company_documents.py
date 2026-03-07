"""Company Document Library API — upload, manage, and auto-link company-wide documents."""
from typing import Optional
from uuid import UUID
from datetime import date

from fastapi import APIRouter, Depends, UploadFile, File, Form, HTTPException, Query, status
from pydantic import BaseModel
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.models.document import Document
from app.services.company_document_service import (
    list_company_documents,
    get_category_summary,
    get_expiring_documents,
    auto_link_to_checklist,
    CATEGORY_LABELS,
    _doc_to_dict,
)
from app.core.storage import file_storage

router = APIRouter()


# ---------------------------------------------------------------------------
# Pydantic Schemas
# ---------------------------------------------------------------------------

class CompanyDocUpdate(BaseModel):
    name: str | None = None
    description: str | None = None
    company_doc_category: str | None = None
    expiry_date: str | None = None  # ISO date string


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------

@router.get("")
async def list_docs(
    category: Optional[str] = Query(None),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all company documents, optionally filtered by category."""
    docs = await list_company_documents(
        db, UUID(current_user.tenant_id), category=category
    )
    return docs


@router.post("/upload", status_code=status.HTTP_201_CREATED)
async def upload_company_document(
    file: UploadFile = File(...),
    name: str = Form(...),
    company_doc_category: str = Form(...),
    description: str = Form(None),
    expiry_date: str = Form(None),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Upload a new company document."""
    # Save file to disk
    file_path, file_size = await file_storage.save_file(file, subfolder="company_docs")
    mime_type = file_storage.get_mime_type(file.filename)

    parsed_expiry = None
    if expiry_date:
        try:
            parsed_expiry = date.fromisoformat(expiry_date)
        except ValueError:
            raise HTTPException(status_code=400, detail="Invalid expiry_date format. Use YYYY-MM-DD.")

    doc = Document(
        tenant_id=UUID(current_user.tenant_id),
        created_by=UUID(current_user.user_id),
        name=name,
        description=description,
        file_path=file_path,
        file_size=file_size,
        mime_type=mime_type,
        document_type="company_document",
        company_doc_category=company_doc_category,
        expiry_date=parsed_expiry,
        is_company_document=True,
        tags=[company_doc_category],
    )
    db.add(doc)
    await db.commit()
    await db.refresh(doc)

    return _doc_to_dict(doc)


@router.get("/{doc_id}")
async def get_company_document(
    doc_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get a single company document by ID."""
    result = await db.execute(
        select(Document).where(
            Document.id == doc_id,
            Document.tenant_id == UUID(current_user.tenant_id),
            Document.is_company_document == True,
        )
    )
    doc = result.scalar_one_or_none()
    if not doc:
        raise HTTPException(status_code=404, detail="Company document not found")
    return _doc_to_dict(doc)


@router.put("/{doc_id}")
async def update_company_document(
    doc_id: UUID,
    body: CompanyDocUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update metadata for a company document."""
    result = await db.execute(
        select(Document).where(
            Document.id == doc_id,
            Document.tenant_id == UUID(current_user.tenant_id),
            Document.is_company_document == True,
        )
    )
    doc = result.scalar_one_or_none()
    if not doc:
        raise HTTPException(status_code=404, detail="Company document not found")

    if body.name is not None:
        doc.name = body.name
    if body.description is not None:
        doc.description = body.description
    if body.company_doc_category is not None:
        doc.company_doc_category = body.company_doc_category
    if body.expiry_date is not None:
        try:
            doc.expiry_date = date.fromisoformat(body.expiry_date) if body.expiry_date else None
        except ValueError:
            raise HTTPException(status_code=400, detail="Invalid expiry_date format")

    await db.commit()
    await db.refresh(doc)
    return _doc_to_dict(doc)


@router.delete("/{doc_id}")
async def delete_company_document(
    doc_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a company document."""
    result = await db.execute(
        select(Document).where(
            Document.id == doc_id,
            Document.tenant_id == UUID(current_user.tenant_id),
            Document.is_company_document == True,
        )
    )
    doc = result.scalar_one_or_none()
    if not doc:
        raise HTTPException(status_code=404, detail="Company document not found")

    await db.delete(doc)
    await db.commit()
    return {"detail": "Deleted"}


@router.get("/categories")
async def get_categories(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get category list with upload counts and expiry status."""
    return await get_category_summary(db, UUID(current_user.tenant_id))


@router.post("/auto-link/{tender_id}")
async def auto_link(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Auto-link company documents to a tender's submission checklist items."""
    result = await auto_link_to_checklist(db, tender_id, UUID(current_user.tenant_id))
    await db.commit()
    return result
