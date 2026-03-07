"""AI Provider configuration model — stores encrypted API keys per tenant."""
import uuid
from datetime import datetime, timezone

from sqlalchemy import Column, String, Boolean, Integer, Text, Index
from sqlalchemy.dialects.mssql import UNIQUEIDENTIFIER, NVARCHAR, BIT, DATETIMEOFFSET

from app.core.db import Base


class AIProviderConfig(Base):
    """Multi-provider AI configuration stored per tenant."""

    __tablename__ = "ai_provider_configs"

    id = Column(UNIQUEIDENTIFIER, primary_key=True, default=uuid.uuid4)
    tenant_id = Column(UNIQUEIDENTIFIER, nullable=False, index=True)

    provider_name = Column(NVARCHAR(50), nullable=False)  # anthropic | openai | google | azure_openai
    display_name = Column(NVARCHAR(200), nullable=False)
    api_key_encrypted = Column(NVARCHAR(None), nullable=False)  # Fernet-encrypted
    endpoint_url = Column(NVARCHAR(500), nullable=True)  # Azure endpoint
    deployment_name = Column(NVARCHAR(200), nullable=True)  # Azure deployment
    api_version = Column(NVARCHAR(50), nullable=True)  # Azure API version
    default_model = Column(NVARCHAR(100), nullable=False)

    # JSON string: {"extraction":true,"declarations":true,...}
    feature_mapping = Column(NVARCHAR(None), nullable=True)

    is_active = Column(BIT, default=True, nullable=False)
    is_default = Column(BIT, default=False, nullable=False)

    # Usage tracking
    total_tokens_used = Column(Integer, default=0, nullable=False)
    total_requests = Column(Integer, default=0, nullable=False)
    last_used_at = Column(DATETIMEOFFSET, nullable=True)
    last_test_at = Column(DATETIMEOFFSET, nullable=True)
    last_test_success = Column(BIT, nullable=True)

    created_by = Column(UNIQUEIDENTIFIER, nullable=True)
    created_at = Column(DATETIMEOFFSET, default=lambda: datetime.now(timezone.utc), nullable=False)
    updated_at = Column(DATETIMEOFFSET, default=lambda: datetime.now(timezone.utc),
                        onupdate=lambda: datetime.now(timezone.utc), nullable=False)

    __table_args__ = (
        Index("ix_ai_provider_configs_tenant", "tenant_id"),
    )
