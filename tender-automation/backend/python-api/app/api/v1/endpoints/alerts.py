"""Alert endpoints — deadline alerts, dismissals, and summaries."""
from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services import alert_service

router = APIRouter()


@router.get("")
async def get_alerts(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get all active alerts for the current user (excludes dismissed)."""
    return await alert_service.get_tender_alerts(
        db, current_user.tenant_id, current_user.user_id
    )


@router.post("/{alert_id}/dismiss")
async def dismiss_alert(
    alert_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Dismiss an alert for the current user."""
    return await alert_service.dismiss_alert(
        db, current_user.tenant_id, current_user.user_id, alert_id
    )


@router.get("/summary")
async def get_alert_summary(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get alert counts by severity."""
    return await alert_service.get_alert_summary(
        db, current_user.tenant_id, current_user.user_id
    )
