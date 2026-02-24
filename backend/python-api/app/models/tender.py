"""
Database models for Tender Management
"""
from sqlalchemy import Column, String, DateTime, Text, Integer, Boolean, DECIMAL, Index
from sqlalchemy.dialects.mssql import UNIQUEIDENTIFIER
from sqlalchemy.sql import func
import uuid

from app.core.database import Base


class Tender(Base):
    """Tender model"""
    __tablename__ = "tenders"

    id = Column(UNIQUEIDENTIFIER, primary_key=True, default=uuid.uuid4)
    tenant_id = Column(UNIQUEIDENTIFIER, nullable=False)
    created_by = Column(UNIQUEIDENTIFIER, nullable=False)

    title = Column(String(500), nullable=False)
    reference_number = Column(String(100))
    issuing_authority = Column(String(300))
    portal_name = Column(String(100))
    portal_url = Column(Text)
    deadline = Column(DateTime)
    estimated_value = Column(DECIMAL(15, 2))

    requirements = Column(Text, default='{}')
    notes = Column(Text)
    status = Column(String(50), default="draft")

    created_at = Column(DateTime, server_default=func.getdate())
    updated_at = Column(DateTime, server_default=func.getdate())

    __table_args__ = (
        Index("idx_tenders_tenant", "tenant_id"),
        Index("idx_tenders_deadline", "deadline"),
        Index("idx_tenders_status", "status"),
    )


class Document(Base):
    """Document model"""
    __tablename__ = "documents"

    id = Column(UNIQUEIDENTIFIER, primary_key=True, default=uuid.uuid4)
    tenant_id = Column(UNIQUEIDENTIFIER, nullable=False)
    created_by = Column(UNIQUEIDENTIFIER, nullable=False)

    name = Column(String(255), nullable=False)
    description = Column(Text)
    file_path = Column(String(500), nullable=False)
    file_size = Column(Integer, nullable=False)
    mime_type = Column(String(100), nullable=False)

    document_type = Column(String(50))
    tags = Column(Text, default='')
    doc_metadata = Column("metadata", Text, default='{}')

    is_template = Column(Boolean, default=False)
    version = Column(Integer, default=1)

    created_at = Column(DateTime, server_default=func.getdate())
    updated_at = Column(DateTime, server_default=func.getdate())

    __table_args__ = (
        Index("idx_documents_tenant", "tenant_id"),
        Index("idx_documents_type", "document_type"),
    )


class TenderDocument(Base):
    """Tender-Document association model"""
    __tablename__ = "tender_documents"

    id = Column(UNIQUEIDENTIFIER, primary_key=True, default=uuid.uuid4)
    tender_id = Column(UNIQUEIDENTIFIER, nullable=False)
    document_id = Column(UNIQUEIDENTIFIER, nullable=False)

    document_order = Column(Integer, default=0)
    is_generated = Column(Boolean, default=False)
    generation_prompt = Column(Text)

    created_at = Column(DateTime, server_default=func.getdate())

    __table_args__ = (
        Index("idx_tender_docs_tender", "tender_id"),
    )


class AIGeneration(Base):
    """AI Generation tracking model"""
    __tablename__ = "ai_generations"

    id = Column(UNIQUEIDENTIFIER, primary_key=True, default=uuid.uuid4)
    tenant_id = Column(UNIQUEIDENTIFIER, nullable=False)
    created_by = Column(UNIQUEIDENTIFIER, nullable=False)
    document_id = Column(UNIQUEIDENTIFIER)

    prompt = Column(Text, nullable=False)
    model_used = Column(String(50), default="claude-sonnet-4-5")
    tokens_used = Column(Integer)
    generation_type = Column(String(50))
    input_context = Column(Text)
    output_content = Column(Text)

    created_at = Column(DateTime, server_default=func.getdate())

    __table_args__ = (
        Index("idx_ai_generations_tenant", "tenant_id"),
    )
