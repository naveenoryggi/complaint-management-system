"""Tender result model — post-submission outcome tracking."""
import uuid
from datetime import datetime

from sqlalchemy import Column, String, DateTime, Uuid, ForeignKey, Text, Numeric, UniqueConstraint

from app.core.db import Base


class TenderResult(Base):
    __tablename__ = "tender_results"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)
    tenant_id = Column(Uuid, nullable=False, index=True)
    result = Column(String(20), nullable=False)  # won/lost/cancelled
    awarded_value = Column(Numeric(15, 2), nullable=True)
    award_date = Column(DateTime(timezone=True), nullable=True)
    winning_bidder = Column(String(500), nullable=True)
    loss_reason = Column(Text, nullable=True)
    lessons_learned = Column(Text, nullable=True)
    contract_number = Column(String(200), nullable=True)
    contract_start = Column(DateTime(timezone=True), nullable=True)
    contract_end = Column(DateTime(timezone=True), nullable=True)
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)

    __table_args__ = (
        UniqueConstraint("tender_id", name="uq_tender_result"),
    )
