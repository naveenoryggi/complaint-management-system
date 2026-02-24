"""
Pydantic schemas for Tender Management
"""
from pydantic import BaseModel, Field, ConfigDict
from typing import Optional, List, Dict, Any
from datetime import datetime
from decimal import Decimal
import uuid


class TenderBase(BaseModel):
    """Base tender schema"""
    title: str = Field(..., max_length=500)
    reference_number: Optional[str] = Field(None, max_length=100)
    issuing_authority: Optional[str] = Field(None, max_length=300)
    portal_name: Optional[str] = Field(None, max_length=100)
    portal_url: Optional[str] = None
    deadline: Optional[datetime] = None
    estimated_value: Optional[Decimal] = None
    requirements: Optional[Dict[str, Any]] = {}
    notes: Optional[str] = None
    status: str = Field("draft", max_length=50)


class TenderCreate(TenderBase):
    """Schema for creating tender"""
    pass


class TenderUpdate(BaseModel):
    """Schema for updating tender"""
    title: Optional[str] = Field(None, max_length=500)
    reference_number: Optional[str] = Field(None, max_length=100)
    issuing_authority: Optional[str] = Field(None, max_length=300)
    portal_name: Optional[str] = Field(None, max_length=100)
    portal_url: Optional[str] = None
    deadline: Optional[datetime] = None
    estimated_value: Optional[Decimal] = None
    requirements: Optional[Dict[str, Any]] = None
    notes: Optional[str] = None
    status: Optional[str] = Field(None, max_length=50)


class TenderResponse(TenderBase):
    """Schema for tender response"""
    id: uuid.UUID
    tenant_id: uuid.UUID
    created_by: uuid.UUID
    created_at: datetime
    updated_at: datetime
    document_count: int = 0

    model_config = ConfigDict(from_attributes=True)


class TenderListResponse(BaseModel):
    """Schema for tender list response"""
    items: List[TenderResponse]
    total: int
    page: int
    page_size: int


class DocumentBase(BaseModel):
    """Base document schema"""
    name: str = Field(..., max_length=255)
    description: Optional[str] = None
    document_type: Optional[str] = Field(None, max_length=50)
    tags: List[str] = []
    is_template: bool = False


class DocumentCreate(DocumentBase):
    """Schema for creating document"""
    pass


class DocumentUpdate(BaseModel):
    """Schema for updating document"""
    name: Optional[str] = Field(None, max_length=255)
    description: Optional[str] = None
    document_type: Optional[str] = Field(None, max_length=50)
    tags: Optional[List[str]] = None


class DocumentResponse(DocumentBase):
    """Schema for document response"""
    id: uuid.UUID
    tenant_id: uuid.UUID
    created_by: uuid.UUID
    file_path: str
    file_size: int
    mime_type: str
    doc_metadata: Dict[str, Any]
    version: int
    created_at: datetime
    updated_at: datetime

    model_config = ConfigDict(from_attributes=True)


class AIGenerateRequest(BaseModel):
    """Schema for AI generation request"""
    generation_type: str = Field(..., description="Type: solution, declaration, proposal")
    context: Dict[str, Any] = Field(..., description="Context for generation")
    tender_id: Optional[uuid.UUID] = None
    save_as_document: bool = True
    document_name: Optional[str] = None


class AIGenerateResponse(BaseModel):
    """Schema for AI generation response"""
    id: uuid.UUID
    content: str
    tokens_used: int
    model_used: str
    document_id: Optional[uuid.UUID] = None
    created_at: datetime


class AssemblyRequest(BaseModel):
    """Schema for document assembly request"""
    document_ids: List[uuid.UUID] = Field(..., min_length=1)
    add_cover: bool = True
    cover_title: Optional[str] = None
    add_letterhead: bool = True
    output_format: str = Field("pdf", pattern="^(pdf|zip)$")


class AssemblyResponse(BaseModel):
    """Schema for assembly response"""
    file_path: str
    file_size: int
    document_count: int
