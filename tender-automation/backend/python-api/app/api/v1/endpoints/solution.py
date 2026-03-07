"""Solution document generation API — end-to-end proposal generation pipeline."""
from uuid import UUID
from typing import Optional

from fastapi import APIRouter, Depends, HTTPException, Query
from fastapi.responses import FileResponse
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services.solution_generator_service import (
    generate_solution_document,
    get_generation_status,
    get_proposal_sections,
    regenerate_section,
    export_proposal_docx,
)

router = APIRouter()


class GenerateRequest(BaseModel):
    skip_optimization: bool = False
    sections_to_generate: list[str] | None = None
    regenerate: bool = False


class RegenerateSectionRequest(BaseModel):
    custom_instructions: str | None = None


# ---------------------------------------------------------------------------
# Pipeline Endpoints
# ---------------------------------------------------------------------------

@router.post("/{tender_id}/solution/generate")
async def generate_proposal(
    tender_id: UUID,
    data: GenerateRequest | None = None,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Run the full AI solution document generation pipeline.

    This is a long-running operation (may take 2-5 minutes) that:
    1. Analyzes tender structure
    2. Classifies requirements
    3. Generates dynamic TOC
    4. Generates content per section
    5. Suggests diagrams
    6. Runs gap analysis
    7. Simulates evaluation scoring
    8. Optimizes weak sections

    Returns generation status with results summary.
    """
    options = {}
    if data:
        options = {
            "skip_optimization": data.skip_optimization,
            "sections_to_generate": data.sections_to_generate,
            "regenerate": data.regenerate,
        }

    try:
        result = await generate_solution_document(
            db=db,
            tender_id=tender_id,
            tenant_id=current_user.tenant_id,
            options=options,
        )
        return result
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Generation failed: {str(e)}")


@router.get("/{tender_id}/solution/status")
async def get_status(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get the latest solution generation status for a tender.

    Returns progress, results, gap analysis, and evaluation scores.
    """
    status = await get_generation_status(db, tender_id)
    if not status:
        return {"status": "not_started", "message": "No proposal generation found for this tender."}
    return status


@router.get("/{tender_id}/solution/sections")
async def list_sections(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get all proposal sections with their generated content.

    Returns the full TOC with content, version info, and quality scores.
    """
    sections = await get_proposal_sections(db, tender_id)
    return {"tender_id": str(tender_id), "sections": sections, "total": len(sections)}


@router.post("/{tender_id}/solution/sections/{section_id}/regenerate")
async def regenerate_single_section(
    tender_id: UUID,
    section_id: UUID,
    data: RegenerateSectionRequest | None = None,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Regenerate a single proposal section.

    Optionally provide custom instructions to guide the regeneration.
    Creates a new version — previous version is preserved.
    """
    try:
        result = await regenerate_section(
            db=db,
            tender_id=tender_id,
            tenant_id=current_user.tenant_id,
            section_id=section_id,
            custom_instructions=data.custom_instructions if data else None,
        )
        return result
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@router.get("/{tender_id}/solution/export")
async def export_docx(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Export the generated proposal as a DOCX document.

    Downloads a formatted Word document with cover page, TOC, and all sections.
    """
    try:
        filepath = await export_proposal_docx(db, tender_id, current_user.tenant_id)
        return FileResponse(
            path=filepath,
            media_type="application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            filename=filepath.split("\\")[-1] if "\\" in filepath else filepath.split("/")[-1],
        )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Export failed: {str(e)}")
