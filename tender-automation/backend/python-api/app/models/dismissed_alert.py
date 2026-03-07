"""Dismissed alert model for user-specific alert dismissals."""
import uuid
from datetime import datetime

from sqlalchemy import Column, String, DateTime, Uuid, UniqueConstraint

from app.core.db import Base


class DismissedAlert(Base):
    __tablename__ = "dismissed_alerts"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tenant_id = Column(Uuid, nullable=False, index=True)
    user_id = Column(Uuid, nullable=False, index=True)
    alert_key = Column(String(200), nullable=False)
    dismissed_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)

    __table_args__ = (
        UniqueConstraint("tenant_id", "user_id", "alert_key", name="uq_dismissed_alert"),
    )
