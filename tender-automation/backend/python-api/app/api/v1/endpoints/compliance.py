"""Compliance readiness API endpoints."""
from typing import List, Optional
from uuid import UUID
from fastapi import APIRouter, Depends, HTTPException, Query, status
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services.compliance_service import compute_compliance, compute_batch_scores

router = APIRouter()


@router.get("/{tender_id}/compliance")
async def get_compliance_check(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get full compliance checklist for a single tender."""
    result = await compute_compliance(db, tender_id, current_user.tenant_id)
    if result is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found",
        )
    return result


@router.get("/compliance/scores")
async def get_compliance_scores(
    tender_ids: Optional[str] = Query(None, description="Comma-separated tender IDs"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get batch compliance scores for tender list/dashboard."""
    if not tender_ids:
        return []

    ids = []
    for tid in tender_ids.split(","):
        tid = tid.strip()
        if tid:
            try:
                ids.append(UUID(tid))
            except ValueError:
                continue

    if not ids:
        return []

    scores = await compute_batch_scores(db, ids, current_user.tenant_id)
    return scores
