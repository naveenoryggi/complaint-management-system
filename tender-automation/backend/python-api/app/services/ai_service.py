"""AI service - Multi-provider AI integration for document generation."""
from typing import Optional, Dict, Any
from uuid import UUID
from datetime import datetime
from sqlalchemy import select, func
from sqlalchemy.ext.asyncio import AsyncSession
from fastapi import HTTPException, status
import aiofiles

from app.core.config import settings
from app.models.ai_generation import AIGeneration
from app.models.document import Document
from app.schemas.ai import (
    AIGenerateRequest,
    AIGenerationResponse,
    AIUsageStats,
    GenerationContext,
)
from app.prompts.templates import format_template
from app.core.security import TokenData
from app.services.docx_service import docx_service
from app.services.ai_provider_service import send_message


class AIService:
    """Service for AI document generation using multi-provider factory."""

    def __init__(self):
        """Initialize pricing lookup (no hardcoded client)."""
        self.pricing = {
            "claude-opus-4": {"input": 15.0, "output": 75.0},
            "claude-opus-4-5": {"input": 15.0, "output": 75.0},
            "claude-sonnet-3-5": {"input": 3.0, "output": 15.0},
            "claude-sonnet-4": {"input": 3.0, "output": 15.0},
        }

    def _estimate_cost(
        self,
        model: str,
        input_tokens: int,
        output_tokens: int
    ) -> float:
        """
        Estimate generation cost in USD.

        Args:
            model: Claude model name
            input_tokens: Number of input tokens
            output_tokens: Number of output tokens

        Returns:
            Estimated cost in USD
        """
        pricing = self.pricing.get(model, self.pricing["claude-sonnet-4"])

        input_cost = (input_tokens / 1_000_000) * pricing["input"]
        output_cost = (output_tokens / 1_000_000) * pricing["output"]

        return input_cost + output_cost

    async def generate_document(
        self,
        db: AsyncSession,
        request: AIGenerateRequest,
        current_user: TokenData,
    ) -> AIGenerationResponse:
        """
        Generate document using Claude API.

        Args:
            db: Database session
            request: Generation request
            current_user: Current authenticated user

        Returns:
            Generated content and metadata
        """
        try:
            # Format prompt with context
            context_dict = request.context.model_dump()
            prompt = format_template(request.generation_type.value, context_dict)

            # Call AI via provider factory
            content, total_tokens, model_used = await send_message(
                db=db,
                tenant_id=current_user.tenant_id,
                prompt=prompt,
                feature="document_generation",
                model=request.model,
                max_tokens=request.max_tokens,
                temperature=request.temperature,
            )

            # Create AI generation record
            ai_generation = AIGeneration(
                tenant_id=UUID(current_user.tenant_id),
                created_by=UUID(current_user.user_id),
                prompt=prompt,
                model_used=model_used,
                tokens_used=total_tokens,
                generation_type=request.generation_type.value,
                input_context=context_dict,
                output_content=content,
            )

            db.add(ai_generation)
            await db.flush()  # Get the ID without committing yet

            # Save as document if requested
            document_id = None
            if request.save_as_document:
                document_name = request.document_name or f"{request.generation_type.value.replace('_', ' ').title()} - {datetime.utcnow().strftime('%Y%m%d_%H%M%S')}"

                # Generate DOCX file
                company_name = context_dict.get("company_name", "Your Company")

                # Use specialized formatting for certain document types
                if request.generation_type.value == "covering_letter":
                    docx_buffer = docx_service.format_covering_letter(
                        content=content,
                        company_name=company_name,
                        address=context_dict.get("company_address")
                    )
                elif request.generation_type.value == "compliance_declaration":
                    docx_buffer = docx_service.format_compliance_declaration(
                        content=content,
                        company_name=company_name
                    )
                else:
                    docx_buffer = docx_service.create_document_from_text(
                        content=content,
                        title=document_name,
                        generation_type=request.generation_type.value,
                        company_name=company_name,
                        add_letterhead=True
                    )

                # Save DOCX file to storage
                import os
                storage_dir = os.path.join(settings.upload_dir, "ai_generated")
                os.makedirs(storage_dir, exist_ok=True)

                file_path = os.path.join(storage_dir, f"{ai_generation.id}.docx")

                # Write DOCX to file
                async with aiofiles.open(file_path, 'wb') as f:
                    await f.write(docx_buffer.getvalue())

                file_size = len(docx_buffer.getvalue())

                # Create document record
                document = Document(
                    tenant_id=UUID(current_user.tenant_id),
                    created_by=UUID(current_user.user_id),
                    name=document_name,
                    description=f"AI-generated {request.generation_type.value.replace('_', ' ')}",
                    file_path=f"ai_generated/{ai_generation.id}.docx",
                    file_size=file_size,
                    mime_type="application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    document_type="ai_generated",
                    tags=[request.generation_type.value, "ai_generated"],
                    metadata={
                        "ai_generation_id": str(ai_generation.id),
                        "model": request.model,
                        "tokens_used": total_tokens,
                        "generation_type": request.generation_type.value,
                    }
                )

                db.add(document)
                await db.flush()

                # Update AI generation with document reference
                ai_generation.document_id = document.id
                document_id = document.id

            await db.commit()
            await db.refresh(ai_generation)

            return AIGenerationResponse(
                id=ai_generation.id,
                generation_type=ai_generation.generation_type,
                content=content,
                tokens_used=total_tokens,
                model_used=model_used,
                document_id=document_id,
                created_at=ai_generation.created_at,
            )

        except HTTPException:
            raise
        except Exception as e:
            await db.rollback()
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"Failed to generate document: {str(e)}"
            )

    async def get_generation_history(
        self,
        db: AsyncSession,
        current_user: TokenData,
        limit: int = 50,
    ) -> list[AIGenerationResponse]:
        """
        Get generation history for current tenant.

        Args:
            db: Database session
            current_user: Current authenticated user
            limit: Maximum number of records

        Returns:
            List of past generations
        """
        result = await db.execute(
            select(AIGeneration)
            .where(AIGeneration.tenant_id == UUID(current_user.tenant_id))
            .order_by(AIGeneration.created_at.desc())
            .limit(limit)
        )
        generations = result.scalars().all()

        return [
            AIGenerationResponse(
                id=gen.id,
                generation_type=gen.generation_type,
                content=gen.output_content or "",
                tokens_used=gen.tokens_used,
                model_used=gen.model_used,
                document_id=gen.document_id,
                created_at=gen.created_at,
            )
            for gen in generations
        ]

    async def get_usage_stats(
        self,
        db: AsyncSession,
        current_user: TokenData,
    ) -> AIUsageStats:
        """
        Get AI usage statistics for current tenant.

        Args:
            db: Database session
            current_user: Current authenticated user

        Returns:
            Usage statistics
        """
        tenant_id = UUID(current_user.tenant_id)

        # Total generations
        total_count = await db.execute(
            select(func.count(AIGeneration.id))
            .where(AIGeneration.tenant_id == tenant_id)
        )
        total_generations = total_count.scalar_one()

        # Total tokens
        total_tokens_result = await db.execute(
            select(func.sum(AIGeneration.tokens_used))
            .where(AIGeneration.tenant_id == tenant_id)
        )
        total_tokens = total_tokens_result.scalar_one() or 0

        # Generations by type
        by_type_result = await db.execute(
            select(
                AIGeneration.generation_type,
                func.count(AIGeneration.id)
            )
            .where(AIGeneration.tenant_id == tenant_id)
            .group_by(AIGeneration.generation_type)
        )
        generations_by_type = {row[0]: row[1] for row in by_type_result}

        # Current month stats
        from datetime import datetime
        current_month_start = datetime.utcnow().replace(day=1, hour=0, minute=0, second=0, microsecond=0)

        month_tokens_result = await db.execute(
            select(func.sum(AIGeneration.tokens_used))
            .where(
                AIGeneration.tenant_id == tenant_id,
                AIGeneration.created_at >= current_month_start
            )
        )
        current_month_tokens = month_tokens_result.scalar_one() or 0

        # Estimate costs (rough estimate using average model pricing)
        # Assuming 50/50 split between input and output tokens
        avg_cost_per_token = 0.00003  # Approximate average for Claude Sonnet
        estimated_cost = total_tokens * avg_cost_per_token
        current_month_cost = current_month_tokens * avg_cost_per_token

        return AIUsageStats(
            total_generations=total_generations,
            total_tokens_used=total_tokens,
            estimated_cost_usd=round(estimated_cost, 2),
            generations_by_type=generations_by_type,
            current_month_tokens=current_month_tokens,
            current_month_cost_usd=round(current_month_cost, 2),
        )


# Global service instance
ai_service = AIService()
