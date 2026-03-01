"""Tender model."""
import uuid
from datetime import datetime
from typing import Optional
from decimal import Decimal

from sqlalchemy import Column, String, Text, DateTime, Numeric, ForeignKey
from sqlalchemy.dialects.postgresql import UUID, JSONB
from sqlalchemy.orm import relationship

from app.core.db import Base


class Tender(Base):
    """Tender entity - represents a tender/RFP that the company is responding to."""

    __tablename__ = "tenders"

    # Primary Key
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)

    # Multi-tenancy (foreign keys to existing tables)
    tenant_id = Column(UUID(as_uuid=True), nullable=False, index=True)
    created_by = Column(UUID(as_uuid=True), nullable=False)

    # Tender Information
    title = Column(String(500), nullable=False)
    reference_number = Column(String(100), nullable=True)
    issuing_authority = Column(String(300), nullable=True)
    portal_name = Column(String(100), nullable=True)  # Manual entry for MVP
    portal_url = Column(Text, nullable=True)
    tender_url = Column(Text, nullable=True)

    # Dates and Value
    deadline = Column(DateTime(timezone=True), nullable=True, index=True)
    estimated_value = Column(Numeric(15, 2), nullable=True)

    # Flexible storage
    requirements = Column(JSONB, nullable=True)  # Store structured requirements
    notes = Column(Text, nullable=True)

    # Status
    status = Column(String(50), nullable=False, default="draft", index=True)
    # Status values: draft, in_progress, submitted, won, lost

    # Audit fields
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)

    # Relationships
    # Note: These will link to the TenderDocument join table
    # documents = relationship("TenderDocument", back_populates="tender", cascade="all, delete-orphan")

    def __repr__(self):
        return f"<Tender(id={self.id}, title='{self.title}', status='{self.status}')>"
