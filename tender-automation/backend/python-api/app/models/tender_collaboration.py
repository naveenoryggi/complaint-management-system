"""Tender collaboration models — assignments and comments."""
import uuid
from datetime import datetime

from sqlalchemy import Column, String, DateTime, Uuid, ForeignKey, Text

from app.core.db import Base


class TenderAssignment(Base):
    __tablename__ = "tender_assignments"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)
    tenant_id = Column(Uuid, nullable=False, index=True)
    user_id = Column(Uuid, nullable=False)
    user_name = Column(String(200), nullable=False)
    role = Column(String(50), nullable=False, default="member")  # lead/member/reviewer
    assigned_by = Column(Uuid, nullable=False)
    assigned_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)


class TenderComment(Base):
    __tablename__ = "tender_comments"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)
    tenant_id = Column(Uuid, nullable=False, index=True)
    user_id = Column(Uuid, nullable=False)
    user_name = Column(String(200), nullable=False)
    comment_text = Column(Text, nullable=False)
    parent_id = Column(Uuid, nullable=True)  # For threaded replies
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)
