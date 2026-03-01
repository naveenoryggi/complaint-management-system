"""Pydantic schemas for document requests and responses."""
from typing import Optional, List, Dict, Any
from datetime import datetime
from uuid import UUID
from pydantic import BaseModel, Field, field_validator


class DocumentBase(BaseModel):
    """Base document schema with common fields."""
    name: str = Field(..., min_length=1, max_length=255, description="Document name")
    description: Optional[str] = Field(None, max_length=2000, description="Document description")
    document_type: Optional[str] = Field(None, max_length=50, description="Document type (certificate, financial, etc.)")
    tags: List[str] = Field(default_factory=list, description="Document tags")
    is_template: bool = Field(default=False, description="Whether this is a template document")


class DocumentCreate(DocumentBase):
    """Schema for creating a document (metadata only, file uploaded separately)."""
    pass


class DocumentUpdate(BaseModel):
    """Schema for updating document metadata."""
    name: Optional[str] = Field(None, min_length=1, max_length=255)
    description: Optional[str] = None
    document_type: Optional[str] = Field(None, max_length=50)
    tags: Optional[List[str]] = None
    is_template: Optional[bool] = None


class DocumentResponse(DocumentBase):
    """Schema for document response."""
    id: UUID
    tenant_id: UUID
    created_by: UUID
    file_path: str
    file_size: int
    mime_type: str
    version: int
    metadata: Dict[str, Any] = Field(default_factory=dict)
    created_at: datetime
    updated_at: datetime

    class Config:
        from_attributes = True


class DocumentListResponse(BaseModel):
    """Schema for paginated document list."""
    items: List[DocumentResponse]
    total: int
    page: int
    page_size: int
    total_pages: int


class DocumentSearchRequest(BaseModel):
    """Schema for document search request."""
    query: Optional[str] = Field(None, description="Search query (searches name and description)")
    document_type: Optional[str] = Field(None, description="Filter by document type")
    tags: Optional[List[str]] = Field(None, description="Filter by tags (any match)")
    is_template: Optional[bool] = Field(None, description="Filter templates")
    page: int = Field(default=1, ge=1, description="Page number")
    page_size: int = Field(default=20, ge=1, le=100, description="Items per page")


class DocumentUploadResponse(BaseModel):
    """Schema for file upload response."""
    id: UUID
    name: str
    file_path: str
    file_size: int
    mime_type: str
    message: str = "Document uploaded successfully"

    class Config:
        from_attributes = True
