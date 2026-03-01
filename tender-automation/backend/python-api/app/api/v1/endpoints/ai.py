"""AI generation API endpoints."""
from typing import List, Dict
from uuid import UUID
from fastapi import APIRouter, Depends, HTTPException, status, Query
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.schemas.ai import (
    AIGenerateRequest,
    AIGenerationResponse,
    AIUsageStats,
    GenerationType,
)
from app.services.ai_service import ai_service
from app.prompts.templates import TEMPLATES

router = APIRouter()


@router.post("/generate", response_model=AIGenerationResponse, status_code=status.HTTP_201_CREATED)
async def generate_document(
    request: AIGenerateRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Generate a document using AI (Claude API).

    **Generation Types:**
    - `technical_solution`: Technical solution & approach
    - `compliance_declaration`: Compliance declarations
    - `covering_letter`: Covering letter
    - `executive_summary`: Executive summary
    - `proposal`: Proposal document
    - `methodology`: Project methodology

    **Context Fields:**
    - tender_title: Title of the tender
    - tender_reference: Reference number
    - issuing_authority: Organization issuing tender
    - requirements: List of key requirements
    - scope_of_work: Scope description
    - company_name: Your company name
    - company_profile: Brief company profile
    - experience: Relevant experience

    **Models Available:**
    - `claude-opus-4`: Most capable, higher cost
    - `claude-sonnet-4`: Balanced performance and cost (recommended)
    - `claude-opus-4-5`: Latest Opus model

    Requires authentication.
    """
    return await ai_service.generate_document(
        db=db,
        request=request,
        current_user=current_user,
    )


@router.get("/history", response_model=List[AIGenerationResponse])
async def get_generation_history(
    limit: int = Query(50, ge=1, le=100, description="Maximum number of records"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get AI generation history for current tenant.

    Returns up to 100 most recent generations.

    Requires authentication.
    """
    return await ai_service.get_generation_history(
        db=db,
        current_user=current_user,
        limit=limit,
    )


@router.get("/usage", response_model=AIUsageStats)
async def get_usage_statistics(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get AI usage statistics for current tenant.

    Includes:
    - Total generations
    - Total tokens used
    - Estimated cost (USD)
    - Generations by type
    - Current month usage

    Requires authentication.
    """
    return await ai_service.get_usage_stats(
        db=db,
        current_user=current_user,
    )


@router.get("/templates", response_model=Dict[str, str])
async def list_templates(
    current_user: TokenData = Depends(get_current_user),
):
    """
    List available prompt templates.

    Returns a dictionary mapping generation types to template descriptions.

    Requires authentication.
    """
    return {
        "technical_solution": "Comprehensive technical solution and approach document (1500-2000 words)",
        "compliance_declaration": "Formal compliance declaration with standard clauses (300-500 words)",
        "covering_letter": "Professional covering letter for tender submission (300-400 words)",
        "executive_summary": "Compelling executive summary highlighting value proposition (400-600 words)",
        "methodology": "Detailed project methodology with phase-wise breakdown (800-1200 words)",
        "proposal": "Complete proposal document combining multiple sections",
    }


@router.get("/generation-types", response_model=List[Dict[str, str]])
async def list_generation_types(
    current_user: TokenData = Depends(get_current_user),
):
    """
    List all available generation types with descriptions.

    Requires authentication.
    """
    return [
        {
            "value": GenerationType.TECHNICAL_SOLUTION.value,
            "label": "Technical Solution & Approach",
            "description": "Comprehensive technical solution addressing all requirements with methodology",
        },
        {
            "value": GenerationType.COMPLIANCE_DECLARATION.value,
            "label": "Compliance Declaration",
            "description": "Formal legal declaration of compliance, eligibility, and undertakings",
        },
        {
            "value": GenerationType.COVERING_LETTER.value,
            "label": "Covering Letter",
            "description": "Professional covering letter introducing your company and proposal",
        },
        {
            "value": GenerationType.EXECUTIVE_SUMMARY.value,
            "label": "Executive Summary",
            "description": "High-level summary highlighting key points and value proposition",
        },
        {
            "value": GenerationType.METHODOLOGY.value,
            "label": "Project Methodology",
            "description": "Detailed methodology with phases, deliverables, and governance structure",
        },
        {
            "value": GenerationType.PROPOSAL.value,
            "label": "Complete Proposal",
            "description": "Full proposal document combining technical and commercial aspects",
        },
    ]


@router.get("/template/{generation_type}", response_model=Dict[str, str])
async def get_template_details(
    generation_type: str,
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get detailed information about a specific template.

    Returns the raw template text and required context fields.

    Requires authentication.
    """
    from app.prompts.templates import get_template

    try:
        template_text = get_template(generation_type)

        # Extract required context fields from template
        import re
        context_fields = re.findall(r'\{(\w+)\}', template_text)
        unique_fields = list(dict.fromkeys(context_fields))  # Remove duplicates, preserve order

        return {
            "generation_type": generation_type,
            "template": template_text,
            "required_fields": ", ".join(unique_fields),
            "field_count": str(len(unique_fields)),
        }
    except KeyError:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Template not found for generation type: {generation_type}"
        )


@router.post("/preview", response_model=Dict[str, str])
async def preview_prompt(
    generation_type: str,
    context: Dict[str, str],
    current_user: TokenData = Depends(get_current_user),
):
    """
    Preview the formatted prompt that will be sent to Claude API.

    This allows users to see exactly what the AI will receive before generating.

    Requires authentication.
    """
    from app.prompts.templates import format_template

    try:
        formatted_prompt = format_template(generation_type, context)

        return {
            "generation_type": generation_type,
            "formatted_prompt": formatted_prompt,
            "estimated_input_tokens": str(len(formatted_prompt.split()) * 1.3),  # Rough estimate
        }
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Failed to format template: {str(e)}"
        )
