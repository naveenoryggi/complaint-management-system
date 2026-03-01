"""Pydantic schemas for AI generation."""
from typing import Optional, Dict, Any, List
from datetime import datetime
from uuid import UUID
from pydantic import BaseModel, Field
from enum import Enum


class GenerationType(str, Enum):
    """Types of documents that can be generated."""
    TECHNICAL_SOLUTION = "technical_solution"
    COMPLIANCE_DECLARATION = "compliance_declaration"
    COVERING_LETTER = "covering_letter"
    EXECUTIVE_SUMMARY = "executive_summary"
    PROPOSAL = "proposal"
    METHODOLOGY = "methodology"


class GenerationContext(BaseModel):
    """Context data for AI generation."""
    tender_title: Optional[str] = Field(None, description="Title of the tender")
    tender_reference: Optional[str] = Field(None, description="Tender reference number")
    issuing_authority: Optional[str] = Field(None, description="Organization issuing the tender")
    requirements: Optional[List[str]] = Field(None, description="List of key requirements")
    scope_of_work: Optional[str] = Field(None, description="Scope of work description")

    # Company information
    company_name: Optional[str] = Field(None, description="Company name")
    company_profile: Optional[str] = Field(None, description="Brief company profile")
    experience: Optional[str] = Field(None, description="Relevant experience")

    # Additional context
    custom_context: Optional[Dict[str, Any]] = Field(None, description="Any additional context")


class AIGenerateRequest(BaseModel):
    """Request schema for AI document generation."""
    generation_type: GenerationType = Field(..., description="Type of document to generate")
    template_id: Optional[UUID] = Field(None, description="Optional template to use")
    context: GenerationContext = Field(..., description="Context for generation")

    # Generation parameters
    model: Optional[str] = Field("claude-opus-4", description="Claude model to use")
    max_tokens: Optional[int] = Field(4000, ge=100, le=8000, description="Maximum tokens to generate")
    temperature: Optional[float] = Field(0.7, ge=0.0, le=1.0, description="Generation temperature")

    # Output options
    save_as_document: bool = Field(True, description="Save output as document")
    document_name: Optional[str] = Field(None, description="Name for saved document")


class AIRegenerateRequest(BaseModel):
    """Request schema for regenerating with modifications."""
    generation_id: UUID = Field(..., description="ID of previous generation")
    modifications: str = Field(..., description="What to change in regeneration")
    context_updates: Optional[GenerationContext] = Field(None, description="Updated context")


class AIGenerationResponse(BaseModel):
    """Response schema for AI generation."""
    id: UUID
    generation_type: str
    content: str
    tokens_used: Optional[int]
    model_used: str
    document_id: Optional[UUID] = None
    created_at: datetime

    class Config:
        from_attributes = True


class PromptTemplateResponse(BaseModel):
    """Schema for prompt template."""
    id: UUID
    name: str
    description: Optional[str]
    generation_type: str
    template_text: str
    is_active: bool
    created_at: datetime

    class Config:
        from_attributes = True


class AIUsageStats(BaseModel):
    """AI usage statistics for tenant."""
    total_generations: int
    total_tokens_used: int
    estimated_cost_usd: float
    generations_by_type: Dict[str, int]
    current_month_tokens: int
    current_month_cost_usd: float
