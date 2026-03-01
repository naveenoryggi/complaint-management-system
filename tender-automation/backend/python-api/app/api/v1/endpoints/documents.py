"""Document API endpoints."""
from typing import List
from uuid import UUID
from fastapi import APIRouter, Depends, UploadFile, File, Form, HTTPException, status, Query
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.schemas.document import (
    DocumentCreate,
    DocumentUpdate,
    DocumentResponse,
    DocumentListResponse,
    DocumentSearchRequest,
    DocumentUploadResponse,
)
from app.services.document_service import document_service

router = APIRouter()


@router.post("/upload", response_model=DocumentUploadResponse, status_code=status.HTTP_201_CREATED)
async def upload_document(
    file: UploadFile = File(..., description="File to upload"),
    name: str = Form(..., description="Document name"),
    description: str = Form(None, description="Document description"),
    document_type: str = Form(None, description="Document type"),
    tags: str = Form("", description="Comma-separated tags"),
    is_template: bool = Form(False, description="Is this a template?"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Upload a new document.

    Requires authentication.
    """
    # Parse tags
    tag_list = [tag.strip() for tag in tags.split(",") if tag.strip()]

    # Create document data
    document_data = DocumentCreate(
        name=name,
        description=description,
        document_type=document_type,
        tags=tag_list,
        is_template=is_template,
    )

    # Upload document
    document = await document_service.upload_document(
        db=db,
        file=file,
        document_data=document_data,
        current_user=current_user,
    )

    return DocumentUploadResponse(
        id=document.id,
        name=document.name,
        file_path=document.file_path,
        file_size=document.file_size,
        mime_type=document.mime_type,
    )


@router.get("", response_model=DocumentListResponse)
async def list_documents(
    page: int = Query(1, ge=1, description="Page number"),
    page_size: int = Query(20, ge=1, le=100, description="Items per page"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    List all documents for current tenant (paginated).

    Requires authentication.
    """
    return await document_service.list_documents(
        db=db,
        current_user=current_user,
        page=page,
        page_size=page_size,
    )


@router.get("/search", response_model=DocumentListResponse)
async def search_documents(
    query: str = Query(None, description="Search query"),
    document_type: str = Query(None, description="Filter by type"),
    tags: List[str] = Query(None, description="Filter by tags"),
    is_template: bool = Query(None, description="Filter templates"),
    page: int = Query(1, ge=1, description="Page number"),
    page_size: int = Query(20, ge=1, le=100, description="Items per page"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Search documents with filters.

    Requires authentication.
    """
    search_request = DocumentSearchRequest(
        query=query,
        document_type=document_type,
        tags=tags,
        is_template=is_template,
        page=page,
        page_size=page_size,
    )

    return await document_service.search_documents(
        db=db,
        search_request=search_request,
        current_user=current_user,
    )


@router.get("/{document_id}", response_model=DocumentResponse)
async def get_document(
    document_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get document by ID.

    Requires authentication.
    """
    document = await document_service.get_document(
        db=db,
        document_id=document_id,
        current_user=current_user,
    )

    if not document:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Document not found",
        )

    return DocumentResponse.model_validate(document)


@router.put("/{document_id}", response_model=DocumentResponse)
async def update_document(
    document_id: UUID,
    update_data: DocumentUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Update document metadata.

    Requires authentication.
    """
    document = await document_service.update_document(
        db=db,
        document_id=document_id,
        update_data=update_data,
        current_user=current_user,
    )

    if not document:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Document not found",
        )

    return DocumentResponse.model_validate(document)


@router.delete("/{document_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_document(
    document_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Delete document (both record and file).

    Requires authentication.
    """
    deleted = await document_service.delete_document(
        db=db,
        document_id=document_id,
        current_user=current_user,
    )

    if not deleted:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Document not found",
        )

    return None
