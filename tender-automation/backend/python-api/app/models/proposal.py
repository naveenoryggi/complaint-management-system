"""Proposal generation models — tender sections, proposal sections, generated content."""
import uuid
from datetime import datetime

from sqlalchemy import Column, String, Text, DateTime, Integer, Float, Boolean, ForeignKey, Uuid, JSON

from app.core.db import Base


class TenderSection(Base):
    """Parsed section from a tender document (Scope of Work, Technical Specs, etc.)."""
    __tablename__ = "tender_sections"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    section_name = Column(String(500), nullable=False)
    section_description = Column(Text, nullable=True)
    section_text = Column(Text, nullable=True)
    page_reference = Column(String(100), nullable=True)
    sort_order = Column(Integer, nullable=False, default=0)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)


class ProposalSection(Base):
    """Generated proposal TOC section — hierarchical structure for the response document."""
    __tablename__ = "proposal_sections"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    section_number = Column(String(20), nullable=False)  # "1", "1.1", "6.2.1"
    section_title = Column(String(500), nullable=False)
    parent_section_id = Column(Uuid, ForeignKey("proposal_sections.id", ondelete="SET NULL"), nullable=True)

    # Mapped requirement IDs from TechnicalComplianceItem
    mapped_requirement_ids = Column(JSON, nullable=True)  # ["uuid1", "uuid2"]

    # Generation status
    status = Column(String(30), nullable=False, default="pending")
    # Values: pending, generating, generated, reviewed, approved

    sort_order = Column(Integer, nullable=False, default=0)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class GeneratedContent(Base):
    """AI-generated content for a proposal section — supports versioning."""
    __tablename__ = "generated_content"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    proposal_section_id = Column(Uuid, ForeignKey("proposal_sections.id", ondelete="CASCADE"), nullable=False, index=True)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    # Content
    content = Column(Text, nullable=False)
    content_format = Column(String(20), nullable=False, default="markdown")  # markdown, html

    # Version tracking
    version = Column(Integer, nullable=False, default=1)
    is_current = Column(Boolean, nullable=False, default=True)

    # AI metadata
    model_used = Column(String(100), nullable=True)
    tokens_used = Column(Integer, nullable=True)
    generation_prompt = Column(Text, nullable=True)

    # Diagram suggestions for this section
    diagram_suggestions = Column(JSON, nullable=True)
    # [{"type": "architecture", "description": "System architecture diagram", "placeholder": "[Insert...]"}]

    # Quality
    quality_score = Column(Float, nullable=True)  # 0-100 from evaluation simulation

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)


class ProposalGeneration(Base):
    """Tracks a full proposal generation run — one per tender per attempt."""
    __tablename__ = "proposal_generations"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)
    tenant_id = Column(Uuid, nullable=False, index=True)

    # Pipeline status
    status = Column(String(30), nullable=False, default="started")
    # Values: started, analyzing, generating_toc, generating_sections, optimizing,
    #         gap_analysis, evaluation, completed, failed

    current_step = Column(String(100), nullable=True)
    total_steps = Column(Integer, nullable=False, default=0)
    completed_steps = Column(Integer, nullable=False, default=0)

    # Results
    total_sections = Column(Integer, nullable=True)
    total_requirements = Column(Integer, nullable=True)
    coverage_percentage = Column(Float, nullable=True)  # % of requirements covered
    estimated_score = Column(Float, nullable=True)  # from evaluation simulation

    # Gap analysis results
    gap_analysis = Column(JSON, nullable=True)
    # [{"requirement_id": "...", "issue": "...", "risk_level": "high", "recommendation": "..."}]

    # Evaluation simulation results
    evaluation_results = Column(JSON, nullable=True)
    # [{"criteria": "...", "max_marks": 25, "estimated_score": 20, "comments": "..."}]

    # Diagram suggestions
    diagram_suggestions = Column(JSON, nullable=True)

    # Timing
    started_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    completed_at = Column(DateTime(timezone=True), nullable=True)

    # Error tracking
    error_message = Column(Text, nullable=True)
