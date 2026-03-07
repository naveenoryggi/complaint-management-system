"""Tender chat message model — per-tender AI conversation history."""
import uuid
from datetime import datetime

from sqlalchemy import Column, String, Text, DateTime, Integer, ForeignKey, Uuid

from app.core.db import Base


class TenderChatMessage(Base):
    """Stores individual chat messages for tender AI assistant conversations."""

    __tablename__ = "tender_chat_messages"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)
    tenant_id = Column(Uuid, nullable=False, index=True)
    role = Column(String(20), nullable=False)  # "user" or "assistant"
    content = Column(Text, nullable=False)
    tokens_used = Column(Integer, nullable=True)
    model_used = Column(String(100), nullable=True)
    user_id = Column(Uuid, nullable=True)
    user_name = Column(String(200), nullable=True)
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)

    def __repr__(self):
        return f"<TenderChatMessage(id={self.id}, tender_id={self.tender_id}, role='{self.role}')>"
