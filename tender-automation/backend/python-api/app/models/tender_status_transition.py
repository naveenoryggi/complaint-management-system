"""Tender status transition model for workflow audit trail."""
import uuid
from datetime import datetime

from sqlalchemy import Column, String, DateTime, Uuid, ForeignKey, Text

from app.core.db import Base


class TenderStatusTransition(Base):
    __tablename__ = "tender_status_transitions"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)
    tenant_id = Column(Uuid, nullable=False, index=True)
    from_status = Column(String(50), nullable=False)
    to_status = Column(String(50), nullable=False)
    changed_by = Column(Uuid, nullable=False)
    change_reason = Column(String(500), nullable=True)
    changed_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
