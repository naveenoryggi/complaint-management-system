"""Reference bundle, document, criteria, and assignment models."""
import uuid
from datetime import datetime, date

from sqlalchemy import Column, String, Text, DateTime, Date, Integer, Float, Boolean, ForeignKey, ARRAY, UniqueConstraint
from sqlalchemy.dialects.postgresql import UUID, JSONB
from sqlalchemy.orm import relationship

from app.core.db import Base


class ReferenceBundle(Base):
    __tablename__ = "reference_bundles"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    tenant_id = Column(UUID(as_uuid=True), nullable=False, index=True)

    bundle_name = Column(String(500), nullable=False)
    client_name = Column(String(500), nullable=False)
    client_short_name = Column(String(100), nullable=True)
    client_type = Column(String(50), nullable=True)
    project_name = Column(String(500), nullable=True)
    work_order_number = Column(String(100), nullable=True)
    work_order_date = Column(Date, nullable=True)
    contract_value = Column(Float, nullable=True)
    gst_value = Column(Float, nullable=True)
    total_value = Column(Float, nullable=True)
    duration_months = Column(Integer, nullable=True)
    start_date = Column(Date, nullable=True)
    actual_completion_date = Column(Date, nullable=True)
    status = Column(String(50), nullable=False, default="completed")
    scope_description = Column(Text, nullable=True)

    value_bands = Column(ARRAY(String), nullable=True)
    client_type_tags = Column(ARRAY(String), nullable=True)
    work_type_tags = Column(ARRAY(String), nullable=True)
    completeness_score = Column(Integer, nullable=False, default=0)
    missing_documents = Column(ARRAY(String), nullable=True)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)

    bundle_documents = relationship("BundleDocument", back_populates="bundle", cascade="all, delete-orphan")


class BundleDocument(Base):
    __tablename__ = "bundle_documents"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    bundle_id = Column(UUID(as_uuid=True), ForeignKey("reference_bundles.id", ondelete="CASCADE"), nullable=False, index=True)

    tier = Column(String(50), nullable=False)  # contractual/financial/completion/supporting
    doc_subtype = Column(String(100), nullable=False)  # work_order/invoice/cc/performance_cert/reference_letter
    document_path = Column(String(500), nullable=True)
    file_name = Column(String(300), nullable=True)
    invoice_number = Column(String(100), nullable=True)
    invoice_date = Column(Date, nullable=True)
    invoice_amount = Column(Float, nullable=True)
    signatory_name = Column(String(200), nullable=True)
    signatory_designation = Column(String(200), nullable=True)
    is_verified = Column(Boolean, nullable=False, default=False)
    notes = Column(Text, nullable=True)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)

    bundle = relationship("ReferenceBundle", back_populates="bundle_documents")


class TenderCriteria(Base):
    __tablename__ = "tender_criteria"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    tender_id = Column(UUID(as_uuid=True), ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    criteria_code = Column(String(50), nullable=False)
    description = Column(Text, nullable=True)
    stage = Column(String(20), nullable=False, default="marking")  # pq/marking
    max_marks = Column(Float, nullable=False, default=0)
    qualifying_marks = Column(Float, nullable=True)
    scoring_rules = Column(JSONB, nullable=True)
    evidence_required = Column(ARRAY(String), nullable=True)
    can_reuse_across_tenders = Column(Boolean, nullable=False, default=True)
    max_reuse_count = Column(Integer, nullable=True)
    ai_extracted = Column(Boolean, nullable=False, default=False)
    source_document_id = Column(UUID(as_uuid=True), nullable=True)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class BundleCriteriaAssignment(Base):
    __tablename__ = "bundle_criteria_assignments"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    bundle_id = Column(UUID(as_uuid=True), ForeignKey("reference_bundles.id", ondelete="CASCADE"), nullable=False, index=True)
    criteria_id = Column(UUID(as_uuid=True), ForeignKey("tender_criteria.id", ondelete="CASCADE"), nullable=False, index=True)

    predicted_marks = Column(Float, nullable=True)
    actual_marks = Column(Float, nullable=True)
    submitted_doc_types = Column(ARRAY(String), nullable=True)
    notes = Column(Text, nullable=True)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)

    __table_args__ = (UniqueConstraint('bundle_id', 'criteria_id', name='uq_bundle_criteria'),)
