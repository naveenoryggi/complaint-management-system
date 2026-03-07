"""Bid/No-Bid advisor endpoint."""
from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services import bid_nobid_service

router = APIRouter()


@router.post("/{tender_id}/bid-nobid-analysis")
async def analyze_bid_decision(
    tender_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """AI-powered bid/no-bid decision analysis."""
    return await bid_nobid_service.analyze_bid_decision(
        db, tender_id, current_user.tenant_id
    )
