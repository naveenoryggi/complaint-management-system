"""Submission checklist model — tracks document submission across online/offline modes."""
import uuid
from datetime import datetime

from sqlalchemy import Column, String, Text, DateTime, Date, Integer, Boolean, ForeignKey, Uuid

from app.core.db import Base


class SubmissionChecklistItem(Base):
    """Document submission checklist with dual-mode (online/offline) tracking."""
    __tablename__ = "submission_checklist_items"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    document_name = Column(String(500), nullable=False)
    document_category = Column(String(50), default="other")
    # technical, financial, emd, oem, declaration, certificate, legal, company_profile, experience, other

    submission_mode = Column(String(20), default="online")  # online, offline, both
    is_critical = Column(Boolean, default=False)

    envelope = Column(String(30), nullable=True)  # envelope_a, envelope_b, superscription
    cover_name = Column(String(200), nullable=True)  # Custom cover name e.g. "Cover 1 - Technical Bid"

    # Online status: not_started -> in_progress -> uploaded -> digitally_signed -> verified
    online_status = Column(String(30), default="not_started")
    # Offline status: not_started -> prepared -> dispatched -> received -> verified
    offline_status = Column(String(30), default="not_started")

    # Notarization tracking
    notarization_required = Column(Boolean, default=False)
    notarization_type = Column(String(30), nullable=True)  # judicial, non_judicial, None
    notarization_status = Column(String(30), default="not_required")
    # not_required, pending, in_progress, completed, verified

    # Document origin — tells user where to get / how to prepare this document
    document_origin = Column(String(30), default="self_generated")
    # tender_provided: format/annexure given in tender PDF (fill and submit)
    # self_generated: bidder creates (our platform can help generate)
    # pre_existing: already have or obtain from third party (bank, CA, govt)
    format_reference = Column(String(300), nullable=True)  # e.g., "Annexure-III", "Section 5 Format A"
    obtaining_source = Column(String(300), nullable=True)  # e.g., "Bank", "Chartered Accountant", "Udyam Portal"
    can_auto_generate = Column(Boolean, default=False)  # True if platform can generate a draft

    linked_document_id = Column(Uuid, ForeignKey("documents.id", ondelete="SET NULL"), nullable=True)
    due_date = Column(Date, nullable=True)
    notes = Column(Text, nullable=True)
    sort_order = Column(Integer, default=0)
    source = Column(String(30), default="manual")  # manual, template, ai_extracted

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)
