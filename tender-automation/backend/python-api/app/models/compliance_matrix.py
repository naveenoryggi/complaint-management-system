"""Technical compliance matrix, BoQ, milestones, corrigendum models."""
import uuid
from datetime import datetime, date

from sqlalchemy import Column, String, Text, DateTime, Date, Float, Integer, Boolean, ForeignKey, Uuid, JSON, Numeric

from app.core.db import Base


class TechnicalComplianceItem(Base):
    """Clause-by-clause technical compliance tracking."""
    __tablename__ = "technical_compliance_items"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    # Clause reference from tender document
    clause_number = Column(String(50), nullable=False)  # "3.2.1", "Annexure-A Item 5"
    clause_title = Column(String(500), nullable=False)
    clause_description = Column(Text, nullable=True)
    section = Column(String(200), nullable=True)  # "Technical Specifications", "Eligibility"

    # Compliance response
    compliance_status = Column(String(30), nullable=False, default="pending")
    # Values: complied, not_complied, partially_complied, pending, na
    deviation_remarks = Column(Text, nullable=True)  # Explain deviations
    page_reference = Column(String(100), nullable=True)  # "Page 42, Section 5.1"
    supporting_document_id = Column(Uuid, nullable=True)  # Reference to uploaded doc

    # Metadata
    is_mandatory = Column(Boolean, nullable=False, default=True)
    is_critical = Column(Boolean, nullable=False, default=False)  # Technical knock-out
    ai_extracted = Column(Boolean, nullable=False, default=False)
    sort_order = Column(Integer, nullable=False, default=0)

    # Clause interpretation (AI-powered)
    risk_score = Column(Float, nullable=True)  # 0-10 risk score
    risk_category = Column(String(30), nullable=True)  # low, medium, high, critical
    risk_reason = Column(Text, nullable=True)  # Why this clause is risky
    interpretation = Column(Text, nullable=True)  # AI-generated plain-English interpretation
    cross_references = Column(JSON, nullable=True)  # [{"clause": "3.2", "relationship": "depends_on"}]

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class BoQLineItem(Base):
    """Bill of Quantities / Price Schedule line items."""
    __tablename__ = "boq_line_items"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    # Item details
    item_number = Column(String(50), nullable=False)  # "1", "1.1", "2"
    description = Column(Text, nullable=False)
    unit = Column(String(50), nullable=True)  # "Nos", "Lot", "Per Month", "Lump Sum"
    quantity = Column(Float, nullable=False, default=1)

    # Pricing
    unit_rate = Column(Numeric(15, 2), nullable=True)
    total_before_tax = Column(Numeric(15, 2), nullable=True)
    gst_percentage = Column(Float, nullable=True, default=18.0)
    gst_amount = Column(Numeric(15, 2), nullable=True)
    total_with_tax = Column(Numeric(15, 2), nullable=True)

    # Classification
    category = Column(String(100), nullable=True)  # "Hardware", "Software", "Services", "AMC"
    is_optional = Column(Boolean, nullable=False, default=False)
    make_model = Column(String(300), nullable=True)  # OEM product reference
    oem_id = Column(Uuid, nullable=True)  # Link to OEM master

    sort_order = Column(Integer, nullable=False, default=0)
    notes = Column(Text, nullable=True)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class TenderMilestone(Base):
    """Important dates and timeline events for a tender."""
    __tablename__ = "tender_milestones"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    milestone_type = Column(String(50), nullable=False)
    # Types: publish_date, prebid_meeting, corrigendum_deadline, bid_submission,
    #        technical_opening, financial_opening, presentation, award_date,
    #        contract_signing, work_start, custom
    label = Column(String(300), nullable=False)
    event_date = Column(DateTime(timezone=True), nullable=True)
    event_time = Column(String(20), nullable=True)  # "14:00" for time-specific events
    location = Column(String(500), nullable=True)  # Venue for physical meetings
    is_completed = Column(Boolean, nullable=False, default=False)
    notes = Column(Text, nullable=True)

    # Alert settings
    alert_days_before = Column(Integer, nullable=True, default=2)
    sort_order = Column(Integer, nullable=False, default=0)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class TenderCorrigendum(Base):
    """Corrigendum / addendum tracking for tender amendments."""
    __tablename__ = "tender_corrigendums"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    corrigendum_number = Column(Integer, nullable=False)  # 1, 2, 3...
    issue_date = Column(Date, nullable=True)
    title = Column(String(500), nullable=False)
    summary = Column(Text, nullable=True)

    # What changed
    changes = Column(JSON, nullable=True)
    # [{"field": "deadline", "old": "2026-03-15", "new": "2026-03-25"},
    #  {"field": "emd_amount", "old": "500000", "new": "400000"},
    #  {"clause": "3.2", "change": "Relaxed minimum turnover from 50Cr to 30Cr"}]

    document_path = Column(String(500), nullable=True)  # Uploaded corrigendum PDF
    is_acknowledged = Column(Boolean, nullable=False, default=False)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)
