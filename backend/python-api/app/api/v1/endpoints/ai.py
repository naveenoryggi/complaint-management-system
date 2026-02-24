"""
AI generation endpoints using Claude API
"""
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
import uuid
from datetime import datetime

from app.core.database import get_db
from app.core.security import get_current_user, TokenData
from app.core.config import settings
from app.models.tender import AIGeneration, Document
from app.schemas.tender import AIGenerateRequest, AIGenerateResponse

router = APIRouter()


# Prompt templates for different generation types
PROMPTS = {
    "solution": """You are an expert technical writer specializing in tender responses.

Generate a comprehensive technical solution writeup based on the following context:

{context}

Requirements:
- Write in a professional, formal tone
- Include technical details and implementation approach
- Address all requirements mentioned in the context
- Structure the response with clear headings
- Be specific and detailed (minimum 500 words)

Generate the technical solution:""",

    "declaration": """You are a legal compliance writer for tender documentation.

Generate a formal compliance declaration based on the following context:

{context}

Requirements:
- Use formal legal language
- Include all necessary compliance statements
- Reference relevant standards and regulations
- Structure with numbered clauses
- Include signature placeholder

Generate the compliance declaration:""",

    "proposal": """You are a business proposal writer for tender submissions.

Generate a professional tender proposal based on the following context:

{context}

Requirements:
- Write persuasively highlighting company strengths
- Include executive summary
- Address all tender requirements
- Include project timeline and deliverables
- Use professional business language

Generate the tender proposal:"""
}


@router.post("/generate", response_model=AIGenerateResponse)
def generate_content(
    request: AIGenerateRequest,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """Generate content using Claude API"""

    # Validate generation type
    if request.generation_type not in PROMPTS:
        raise HTTPException(
            status_code=400,
            detail=f"Invalid generation type. Must be one of: {list(PROMPTS.keys())}"
        )

    # Check if Anthropic API key is configured
    if not settings.ANTHROPIC_API_KEY:
        raise HTTPException(
            status_code=500,
            detail="Anthropic API key not configured. Please set ANTHROPIC_API_KEY environment variable."
        )

    # Build prompt
    prompt_template = PROMPTS[request.generation_type]
    context_str = "\n".join([f"{k}: {v}" for k, v in request.context.items()])
    full_prompt = prompt_template.format(context=context_str)

    # For MVP, we'll return a placeholder response
    # In production, this would call the Anthropic API
    generated_content = f"""[AI-Generated {request.generation_type.title()}]

Based on the provided context, here is a professional {request.generation_type} for your tender submission.

{context_str}

--- Generated Content ---

This is a placeholder for AI-generated content.

To enable real AI generation:
1. Set ANTHROPIC_API_KEY environment variable
2. Install anthropic package: pip install anthropic
3. The system will automatically use Claude API

For now, you can manually edit this placeholder content in the document editor.

--- End of Generated Content ---

Note: This is a development placeholder. Production version will use Claude Sonnet 4.5 for high-quality generation."""

    # Save AI generation record
    ai_generation = AIGeneration(
        tenant_id=uuid.UUID(current_user.tenant_id),
        created_by=uuid.UUID(current_user.user_id),
        prompt=full_prompt,
        model_used=settings.ANTHROPIC_MODEL,
        tokens_used=len(generated_content.split()),  # Approximate
        generation_type=request.generation_type,
        input_context=request.context,
        output_content=generated_content
    )

    db.add(ai_generation)
    db.commit()
    db.refresh(ai_generation)

    # Optionally save as document
    document_id = None
    if request.save_as_document:
        document_name = request.document_name or f"{request.generation_type}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

        # For now, we'll create a document record without saving to disk
        # In production, this would save as DOCX
        document = Document(
            tenant_id=uuid.UUID(current_user.tenant_id),
            created_by=uuid.UUID(current_user.user_id),
            name=document_name,
            description=f"AI generated {request.generation_type}",
            file_path=f"placeholder_{ai_generation.id}.txt",
            file_size=len(generated_content),
            mime_type="text/plain",
            document_type=request.generation_type,
            tags=["ai-generated"],
            metadata={"generation_id": str(ai_generation.id)}
        )

        db.add(document)
        db.commit()
        db.refresh(document)
        document_id = document.id

        # Update AI generation with document reference
        ai_generation.document_id = document_id
        db.commit()

    return AIGenerateResponse(
        id=ai_generation.id,
        content=generated_content,
        tokens_used=ai_generation.tokens_used,
        model_used=ai_generation.model_used,
        document_id=document_id,
        created_at=ai_generation.created_at
    )


@router.get("/templates")
def list_templates():
    """List available generation templates"""
    return {
        "templates": [
            {
                "id": "solution",
                "name": "Technical Solution",
                "description": "Generate technical solution writeups for tender requirements"
            },
            {
                "id": "declaration",
                "name": "Compliance Declaration",
                "description": "Generate formal compliance declarations"
            },
            {
                "id": "proposal",
                "name": "Tender Proposal",
                "description": "Generate complete tender proposal documents"
            }
        ]
    }
