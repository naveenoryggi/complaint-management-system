"""Workflow endpoints — enforced status transitions with audit trail."""
from typing import Optional

from fastapi import APIRouter, Depends
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services import workflow_service

router = APIRouter()


class TransitionRequest(BaseModel):
    new_status: str
    reason: Optional[str] = None


@router.get("/{tender_id}/workflow/allowed-transitions")
async def get_allowed_transitions(
    tender_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get allowed status transitions for a tender."""
    from app.models.tender import Tender
    from sqlalchemy import select, and_
    from uuid import UUID

    result = await db.execute(
        select(Tender).where(
            and_(Tender.id == UUID(tender_id), Tender.tenant_id == UUID(current_user.tenant_id))
        )
    )
    tender = result.scalar_one_or_none()
    if not tender:
        from fastapi import HTTPException
        raise HTTPException(status_code=404, detail="Tender not found")

    allowed = workflow_service.get_allowed_transitions(tender.status)
    return {
        "tender_id": tender_id,
        "current_status": tender.status,
        "allowed_transitions": allowed,
    }


@router.post("/{tender_id}/workflow/transition")
async def transition_status(
    tender_id: str,
    body: TransitionRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Execute a status transition with audit trail."""
    return await workflow_service.transition_status(
        db, tender_id, current_user.tenant_id, current_user.user_id,
        body.new_status, body.reason
    )


@router.get("/{tender_id}/workflow/history")
async def get_status_history(
    tender_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get full status transition history for a tender."""
    return await workflow_service.get_status_history(
        db, tender_id, current_user.tenant_id
    )
