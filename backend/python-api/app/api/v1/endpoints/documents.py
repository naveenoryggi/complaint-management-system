"""
Document management endpoints
"""
from fastapi import APIRouter, Depends, HTTPException, UploadFile, File, Query
from sqlalchemy.orm import Session
from sqlalchemy import select, func
from typing import Optional, List
import uuid
import os
import shutil
from pathlib import Path

from app.core.database import get_db
from app.core.security import get_current_user, TokenData
from app.core.config import settings
from app.models.tender import Document
from app.schemas.tender import DocumentCreate, DocumentUpdate, DocumentResponse

router = APIRouter()

# Ensure upload directory exists
Path(settings.UPLOAD_DIR).mkdir(parents=True, exist_ok=True)


@router.post("/upload", response_model=DocumentResponse, status_code=201)
def upload_document(
    file: UploadFile = File(...),
    name: Optional[str] = None,
    description: Optional[str] = None,
    document_type: Optional[str] = None,
    tags: str = "",  # Comma-separated tags
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """Upload a document"""
    # Validate file extension
    file_ext = os.path.splitext(file.filename)[1].lower()
    if file_ext not in settings.ALLOWED_EXTENSIONS:
        raise HTTPException(
            status_code=400,
            detail=f"File type {file_ext} not allowed. Allowed: {settings.ALLOWED_EXTENSIONS}"
        )

    # Create unique filename
    file_id = uuid.uuid4()
    filename = f"{file_id}{file_ext}"
    file_path = os.path.join(settings.UPLOAD_DIR, filename)

    # Save file
    try:
        with open(file_path, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to save file: {str(e)}")

    # Get file size
    file_size = os.path.getsize(file_path)

    # Create document record
    document = Document(
        tenant_id=uuid.UUID(current_user.tenant_id),
        created_by=uuid.UUID(current_user.user_id),
        name=name or file.filename,
        description=description,
        file_path=file_path,
        file_size=file_size,
        mime_type=file.content_type or "application/octet-stream",
        document_type=document_type,
        tags=[t.strip() for t in tags.split(",") if t.strip()]
    )

    db.add(document)
    db.commit()
    db.refresh(document)

    return DocumentResponse.model_validate(document)


@router.get("", response_model=List[DocumentResponse])
def list_documents(
    skip: int = Query(0, ge=0),
    limit: int = Query(100, ge=1, le=100),
    document_type: Optional[str] = None,
    search: Optional[str] = None,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """List documents"""
    query = select(Document).where(Document.tenant_id == uuid.UUID(current_user.tenant_id))

    if document_type:
        query = query.where(Document.document_type == document_type)

    if search:
        search_term = f"%{search}%"
        query = query.where(
            (Document.name.ilike(search_term)) |
            (Document.description.ilike(search_term))
        )

    query = query.order_by(Document.created_at.desc()).offset(skip).limit(limit)

    result = db.execute(query)
    documents = result.scalars().all()

    return [DocumentResponse.model_validate(doc) for doc in documents]


@router.get("/{document_id}", response_model=DocumentResponse)
def get_document(
    document_id: uuid.UUID,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """Get document by ID"""
    query = select(Document).where(
        Document.id == document_id,
        Document.tenant_id == uuid.UUID(current_user.tenant_id)
    )

    result = db.execute(query)
    document = result.scalar_one_or_none()

    if not document:
        raise HTTPException(status_code=404, detail="Document not found")

    return DocumentResponse.model_validate(document)


@router.delete("/{document_id}", status_code=204)
def delete_document(
    document_id: uuid.UUID,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """Delete document"""
    query = select(Document).where(
        Document.id == document_id,
        Document.tenant_id == uuid.UUID(current_user.tenant_id)
    )

    result = db.execute(query)
    document = result.scalar_one_or_none()

    if not document:
        raise HTTPException(status_code=404, detail="Document not found")

    # Delete file from disk
    try:
        if os.path.exists(document.file_path):
            os.remove(document.file_path)
    except Exception as e:
        print(f"Failed to delete file: {e}")

    # Delete from database
    db.delete(document)
    db.commit()

    return None
