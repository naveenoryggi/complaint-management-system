"""Result service — post-submission outcome tracking and analytics."""
from uuid import UUID
from datetime import datetime

from sqlalchemy import select, and_, func
from sqlalchemy.ext.asyncio import AsyncSession
from fastapi import HTTPException

from app.models.tender_result import TenderResult
from app.models.tender import Tender


async def get_result(
    db: AsyncSession, tender_id: str, tenant_id: str
) -> dict | None:
    result = await db.execute(
        select(TenderResult).where(
            and_(
                TenderResult.tender_id == UUID(tender_id),
                TenderResult.tenant_id == UUID(tenant_id),
            )
        )
    )
    r = result.scalar_one_or_none()
    if not r:
        return None
    return _to_dict(r)


async def create_result(
    db: AsyncSession, tender_id: str, tenant_id: str, data: dict
) -> dict:
    # Check if result already exists
    existing = await get_result(db, tender_id, tenant_id)
    if existing:
        raise HTTPException(status_code=400, detail="Result already exists for this tender. Use PUT to update.")

    r = TenderResult(
        tender_id=UUID(tender_id),
        tenant_id=UUID(tenant_id),
        **_parse_fields(data),
    )
    db.add(r)
    await db.flush()
    return _to_dict(r)


async def update_result(
    db: AsyncSession, tender_id: str, tenant_id: str, data: dict
) -> dict:
    result = await db.execute(
        select(TenderResult).where(
            and_(
                TenderResult.tender_id == UUID(tender_id),
                TenderResult.tenant_id == UUID(tenant_id),
            )
        )
    )
    r = result.scalar_one_or_none()
    if not r:
        raise HTTPException(status_code=404, detail="Result not found")

    fields = _parse_fields(data)
    for key, val in fields.items():
        setattr(r, key, val)
    r.updated_at = datetime.utcnow()
    await db.flush()
    return _to_dict(r)


async def get_win_loss_analytics(
    db: AsyncSession, tenant_id: str
) -> dict:
    """Compute win/loss analytics across all tenders."""
    results_q = await db.execute(
        select(TenderResult).where(TenderResult.tenant_id == UUID(tenant_id))
    )
    results = results_q.scalars().all()

    total = len(results)
    won = [r for r in results if r.result == "won"]
    lost = [r for r in results if r.result == "lost"]
    cancelled = [r for r in results if r.result == "cancelled"]

    win_rate = (len(won) / total * 100) if total > 0 else 0

    # Loss reasons distribution
    loss_reasons: dict[str, int] = {}
    for r in lost:
        reason = r.loss_reason or "Not specified"
        # Truncate long reasons
        short = reason[:80] if len(reason) > 80 else reason
        loss_reasons[short] = loss_reasons.get(short, 0) + 1

    # Average awarded value
    won_values = [float(r.awarded_value) for r in won if r.awarded_value]
    avg_value = sum(won_values) / len(won_values) if won_values else 0

    return {
        "total_results": total,
        "won_count": len(won),
        "lost_count": len(lost),
        "cancelled_count": len(cancelled),
        "win_rate": round(win_rate, 1),
        "loss_reasons": loss_reasons,
        "average_awarded_value": round(avg_value, 2),
        "total_awarded_value": round(sum(won_values), 2),
    }


def _parse_fields(data: dict) -> dict:
    """Parse and validate result fields."""
    fields = {}
    if "result" in data:
        fields["result"] = data["result"]
    for key in ["awarded_value", "award_date", "winning_bidder", "loss_reason",
                 "lessons_learned", "contract_number", "contract_start", "contract_end"]:
        if key in data and data[key] is not None:
            fields[key] = data[key]
    return fields


def _to_dict(r: TenderResult) -> dict:
    return {
        "id": str(r.id),
        "tender_id": str(r.tender_id),
        "result": r.result,
        "awarded_value": float(r.awarded_value) if r.awarded_value else None,
        "award_date": str(r.award_date) if r.award_date else None,
        "winning_bidder": r.winning_bidder,
        "loss_reason": r.loss_reason,
        "lessons_learned": r.lessons_learned,
        "contract_number": r.contract_number,
        "contract_start": str(r.contract_start) if r.contract_start else None,
        "contract_end": str(r.contract_end) if r.contract_end else None,
        "created_at": str(r.created_at),
        "updated_at": str(r.updated_at),
    }
