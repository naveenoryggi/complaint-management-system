"""Workflow service — enforced status transitions with audit trail."""
from uuid import UUID
from datetime import datetime

from sqlalchemy import select, and_, desc, update
from sqlalchemy.ext.asyncio import AsyncSession
from fastapi import HTTPException, status

from app.models.tender import Tender
from app.models.tender_status_transition import TenderStatusTransition

# --- Status transition graph ---
TRANSITIONS = {
    "draft": ["in_progress", "cancelled"],
    "in_progress": ["submitted", "cancelled", "draft"],
    "submitted": ["won", "lost", "cancelled"],
    "won": [],
    "lost": [],
    "cancelled": [],
}


def get_allowed_transitions(current_status: str) -> list[str]:
    """Return list of valid next statuses."""
    return TRANSITIONS.get(current_status, [])


async def transition_status(
    db: AsyncSession,
    tender_id: str,
    tenant_id: str,
    user_id: str,
    new_status: str,
    reason: str | None = None,
) -> dict:
    """Validate and execute a status transition, creating an audit record."""
    tid = UUID(tender_id)

    # Fetch tender
    result = await db.execute(
        select(Tender).where(
            and_(Tender.id == tid, Tender.tenant_id == UUID(tenant_id))
        )
    )
    tender = result.scalar_one_or_none()
    if not tender:
        raise HTTPException(status_code=404, detail="Tender not found")

    current = tender.status
    allowed = get_allowed_transitions(current)

    if new_status not in allowed:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Cannot transition from '{current}' to '{new_status}'. Allowed: {allowed}",
        )

    # Create transition record
    transition = TenderStatusTransition(
        tender_id=tid,
        tenant_id=UUID(tenant_id),
        from_status=current,
        to_status=new_status,
        changed_by=UUID(user_id),
        change_reason=reason,
    )
    db.add(transition)

    # Update tender status
    await db.execute(
        update(Tender)
        .where(Tender.id == tid)
        .values(status=new_status, updated_at=datetime.utcnow())
    )

    await db.flush()

    return {
        "tender_id": tender_id,
        "from_status": current,
        "to_status": new_status,
        "reason": reason,
        "changed_at": str(transition.changed_at),
    }


async def get_status_history(
    db: AsyncSession,
    tender_id: str,
    tenant_id: str,
) -> list[dict]:
    """Get the full status transition history for a tender."""
    result = await db.execute(
        select(TenderStatusTransition)
        .where(
            and_(
                TenderStatusTransition.tender_id == UUID(tender_id),
                TenderStatusTransition.tenant_id == UUID(tenant_id),
            )
        )
        .order_by(desc(TenderStatusTransition.changed_at))
    )
    transitions = result.scalars().all()

    return [
        {
            "id": str(t.id),
            "from_status": t.from_status,
            "to_status": t.to_status,
            "changed_by": str(t.changed_by),
            "change_reason": t.change_reason,
            "changed_at": str(t.changed_at),
        }
        for t in transitions
    ]
