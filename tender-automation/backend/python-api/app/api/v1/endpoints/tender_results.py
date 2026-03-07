"""Tender result endpoints — post-submission outcome tracking."""
from fastapi import APIRouter, Depends, HTTPException
from pydantic import BaseModel
from typing import Optional
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services import result_service

router = APIRouter()


class TenderResultCreate(BaseModel):
    result: str  # won/lost/cancelled
    awarded_value: Optional[float] = None
    award_date: Optional[str] = None
    winning_bidder: Optional[str] = None
    loss_reason: Optional[str] = None
    lessons_learned: Optional[str] = None
    contract_number: Optional[str] = None
    contract_start: Optional[str] = None
    contract_end: Optional[str] = None


@router.get("/{tender_id}/result")
async def get_result(
    tender_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    result = await result_service.get_result(db, tender_id, current_user.tenant_id)
    if not result:
        raise HTTPException(status_code=404, detail="No result recorded for this tender")
    return result


@router.post("/{tender_id}/result")
async def create_result(
    tender_id: str,
    body: TenderResultCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await result_service.create_result(
        db, tender_id, current_user.tenant_id, body.model_dump()
    )


@router.put("/{tender_id}/result")
async def update_result(
    tender_id: str,
    body: TenderResultCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await result_service.update_result(
        db, tender_id, current_user.tenant_id, body.model_dump()
    )


@router.get("/results/analytics")
async def get_win_loss_analytics(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await result_service.get_win_loss_analytics(db, current_user.tenant_id)
