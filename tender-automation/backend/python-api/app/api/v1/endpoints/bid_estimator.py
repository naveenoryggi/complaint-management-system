"""Bid cost estimator endpoint."""
from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services import bid_estimator_service

router = APIRouter()


@router.post("/{tender_id}/estimate-bid-cost")
async def estimate_bid_cost(
    tender_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Compute full bid cost estimate with AI-powered margin analysis."""
    return await bid_estimator_service.estimate_bid_cost(
        db, tender_id, current_user.tenant_id
    )
