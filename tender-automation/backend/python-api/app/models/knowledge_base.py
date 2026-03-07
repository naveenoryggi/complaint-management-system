"""Knowledge base models — reusable content entries, versioning, and tender usage links."""
import uuid
from datetime import datetime

from sqlalchemy import Column, String, Text, DateTime, Date, Integer, Float, Boolean, ForeignKey, UniqueConstraint, Uuid, JSON

from app.core.db import Base


class KnowledgeBaseEntry(Base):
    """Reusable knowledge base entry — company snippets, financial data, technical responses, etc."""
    __tablename__ = "knowledge_base_entries"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tenant_id = Column(Uuid, nullable=False, index=True)

    # Content
    title = Column(String(500), nullable=False)
    content = Column(Text, nullable=False)
    content_format = Column(String(20), nullable=False, default="text")  # text, json, markdown

    # Classification
    category = Column(String(50), nullable=False, index=True)
    # Values: company_snippet, financial_data, experience_summary, declaration_text,
    #         technical_response, personnel_bio, pricing_data, standard_clause, submission_note
    subcategory = Column(String(100), nullable=True)
    # e.g. turnover, net_worth, compliance_matrix_response, methodology, etc.

    tags = Column(JSON, nullable=True)       # ["networking", "switch", "PoE"]
    keywords = Column(JSON, nullable=True)   # ["48-port", "managed", "L3"]

    # Source tracking
    source_type = Column(String(30), nullable=False, default="manual")
    # Values: manual, auto_harvested, ai_generated
    source_tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="SET NULL"), nullable=True)
    source_description = Column(String(500), nullable=True)

    # Freshness / validity
    valid_from = Column(Date, nullable=True)
    valid_until = Column(Date, nullable=True)
    fiscal_year = Column(String(20), nullable=True)  # "2024-25", "2023-24"

    # Status and usage
    is_current = Column(Boolean, nullable=False, default=True)
    usage_count = Column(Integer, nullable=False, default=0)
    last_used_at = Column(DateTime(timezone=True), nullable=True)
    last_used_tender_id = Column(Uuid, nullable=True)

    # Quality
    confidence_score = Column(Float, nullable=True)  # 0.0 - 1.0
    is_verified = Column(Boolean, nullable=False, default=False)
    is_archived = Column(Boolean, nullable=False, default=False)

    # Audit
    created_by = Column(Uuid, nullable=True)
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class KBEntryVersion(Base):
    """Version history for knowledge base entries — tracks content changes over time."""
    __tablename__ = "kb_entry_versions"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    entry_id = Column(Uuid, ForeignKey("knowledge_base_entries.id", ondelete="CASCADE"), nullable=False, index=True)

    version_number = Column(Integer, nullable=False)
    content = Column(Text, nullable=False)
    change_reason = Column(String(500), nullable=True)
    changed_by = Column(Uuid, nullable=True)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)

    __table_args__ = (
        UniqueConstraint("entry_id", "version_number", name="uq_kb_entry_version"),
    )


class KBTenderLink(Base):
    """Junction table — tracks which KB entries were used in which tenders."""
    __tablename__ = "kb_tender_links"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    entry_id = Column(Uuid, ForeignKey("knowledge_base_entries.id", ondelete="CASCADE"), nullable=False, index=True)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    usage_context = Column(String(200), nullable=True)  # "compliance_response", "bid_cover_letter", etc.
    used_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    used_by = Column(Uuid, nullable=True)

    __table_args__ = (
        UniqueConstraint("entry_id", "tender_id", "usage_context", name="uq_kb_tender_usage"),
    )
