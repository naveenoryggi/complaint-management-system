"""Document models."""
import uuid
from datetime import datetime
from sqlalchemy import Column, String, Text, DateTime, BigInteger, Boolean, Integer, ForeignKey, ARRAY
from sqlalchemy.dialects.postgresql import UUID, JSONB
from sqlalchemy.orm import relationship

from app.core.db import Base


class Document(Base):
    """Document entity - company documents library."""

    __tablename__ = "documents"

    # Primary Key
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)

    # Multi-tenancy
    tenant_id = Column(UUID(as_uuid=True), nullable=False, index=True)
    created_by = Column(UUID(as_uuid=True), nullable=False)

    # Document Info
    name = Column(String(255), nullable=False)
    description = Column(Text, nullable=True)

    # File Storage
    file_path = Column(String(500), nullable=False)  # Path or S3 key
    file_size = Column(BigInteger, nullable=False)  # Size in bytes
    mime_type = Column(String(100), nullable=False)

    # Categorization
    document_type = Column(String(50), nullable=True, index=True)
    # Types: certificate, financial, technical, proposal, legal, etc.

    tags = Column(ARRAY(String), nullable=False, server_default='{}')
    metadata = Column(JSONB, nullable=False, server_default='{}')

    # Template & Versioning
    is_template = Column(Boolean, nullable=False, default=False)
    version = Column(Integer, nullable=False, default=1)

    # Audit fields
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)

    def __repr__(self):
        return f"<Document(id={self.id}, name='{self.name}', type='{self.document_type}')>"


class TenderDocument(Base):
    """Join table linking tenders to documents."""

    __tablename__ = "tender_documents"

    # Primary Key
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)

    # Foreign Keys
    tender_id = Column(UUID(as_uuid=True), ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)
    document_id = Column(UUID(as_uuid=True), ForeignKey("documents.id", ondelete="CASCADE"), nullable=False, index=True)

    # Ordering
    document_order = Column(Integer, nullable=False, default=0)

    # AI Generation Tracking
    is_generated = Column(Boolean, nullable=False, default=False)
    generation_prompt = Column(Text, nullable=True)

    # Audit
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)

    # Relationships
    # tender = relationship("Tender", back_populates="documents")
    # document = relationship("Document")

    def __repr__(self):
        return f"<TenderDocument(tender_id={self.tender_id}, document_id={self.document_id})>"
