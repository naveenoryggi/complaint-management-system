"""AI Generation tracking model."""
import uuid
from datetime import datetime
from sqlalchemy import Column, String, Text, DateTime, Integer, ForeignKey, Uuid, JSON

from app.core.db import Base


class AIGeneration(Base):
    """Track AI document generations for cost monitoring and audit."""

    __tablename__ = "ai_generations"

    # Primary Key
    id = Column(Uuid, primary_key=True, default=uuid.uuid4)

    # Multi-tenancy
    tenant_id = Column(Uuid, nullable=False, index=True)
    created_by = Column(Uuid, nullable=False)

    # Link to saved document (if saved)
    document_id = Column(Uuid, ForeignKey("documents.id"), nullable=True)

    # Generation Info
    prompt = Column(Text, nullable=False)
    model_used = Column(String(50), nullable=False, default="claude-opus-4-5")
    tokens_used = Column(Integer, nullable=True)  # For cost tracking

    generation_type = Column(String(50), nullable=True)
    # Types: solution, declaration, proposal, covering_letter, etc.

    # Input/Output
    input_context = Column(JSON, nullable=True)  # Store any input data
    output_content = Column(Text, nullable=True)  # Generated content

    # Audit
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)

    def __repr__(self):
        return f"<AIGeneration(id={self.id}, type='{self.generation_type}', tokens={self.tokens_used})>"
