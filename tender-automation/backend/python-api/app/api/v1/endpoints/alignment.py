"""AI Alignment Verification API endpoint."""
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services import alignment_service

router = APIRouter()


@router.post("/{tender_id}/alignment-check")
async def run_alignment_check(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Run a comprehensive AI alignment verification on the entire tender.

    Analyzes compliance matrix, submission checklist, OEM requirements,
    evaluation criteria, EMD/fees, and pre-bid queries to identify gaps,
    contradictions, and risks before submission.
    """
    try:
        result = await alignment_service.run_alignment_check(
            db=db,
            tender_id=tender_id,
            tenant_id=current_user.tenant_id,
        )
        return result
    except ValueError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Alignment check failed: {str(e)}")
