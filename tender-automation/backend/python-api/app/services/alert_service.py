"""Alert service — computes deadline alerts from tenders, EMDs, and fees."""
import hashlib
from datetime import datetime, date, timedelta
from uuid import UUID
from typing import Optional

from sqlalchemy import select, and_
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.tender import Tender
from app.models.tracking import EMDRecord, TenderFee
from app.models.dismissed_alert import DismissedAlert


def _alert_key(alert_type: str, entity_id: str, threshold: int) -> str:
    """Generate a stable key for a specific alert."""
    raw = f"{alert_type}:{entity_id}:{threshold}"
    return hashlib.md5(raw.encode()).hexdigest()[:16]


def _days_remaining(target_date) -> int:
    """Calculate days remaining from today to target date."""
    if target_date is None:
        return 999
    if isinstance(target_date, datetime):
        target = target_date.date()
    elif isinstance(target_date, date):
        target = target_date
    else:
        return 999
    return (target - date.today()).days


def _severity(days: int) -> str:
    if days < 0:
        return "critical"
    if days <= 3:
        return "critical"
    if days <= 7:
        return "warning"
    return "info"


async def get_tender_alerts(
    db: AsyncSession,
    tenant_id: str,
    user_id: str,
) -> list[dict]:
    """Compute all deadline alerts for active tenders, excluding dismissed ones."""

    # Get dismissed alert keys for this user
    dismissed_result = await db.execute(
        select(DismissedAlert.alert_key).where(
            and_(
                DismissedAlert.tenant_id == UUID(tenant_id),
                DismissedAlert.user_id == UUID(user_id),
            )
        )
    )
    dismissed_keys = set(row[0] for row in dismissed_result.fetchall())

    alerts = []

    # --- Tender deadline alerts ---
    tender_result = await db.execute(
        select(Tender).where(
            and_(
                Tender.tenant_id == UUID(tenant_id),
                Tender.status.in_(["draft", "in_progress"]),
                Tender.deadline.isnot(None),
            )
        )
    )
    tenders = tender_result.scalars().all()

    for t in tenders:
        days = _days_remaining(t.deadline)
        if days <= 7:
            for threshold in [7, 3, 1, 0]:
                if days <= threshold:
                    key = _alert_key("tender_deadline", str(t.id), threshold)
                    if key not in dismissed_keys:
                        if days < 0:
                            msg = f"Overdue by {abs(days)} day(s)"
                        elif days == 0:
                            msg = "Due today"
                        elif days == 1:
                            msg = "Due tomorrow"
                        else:
                            msg = f"{days} day(s) remaining"
                        alerts.append({
                            "id": key,
                            "tender_id": str(t.id),
                            "tender_title": t.title,
                            "alert_type": "tender_deadline",
                            "severity": _severity(days),
                            "message": f"Tender deadline: {msg}",
                            "due_date": str(t.deadline) if t.deadline else None,
                            "days_remaining": days,
                        })
                    break

    # --- EMD expiry alerts ---
    emd_result = await db.execute(
        select(EMDRecord, Tender.title).join(
            Tender, EMDRecord.tender_id == Tender.id
        ).where(
            and_(
                Tender.tenant_id == UUID(tenant_id),
                EMDRecord.status != "released",
                EMDRecord.validity_end_date.isnot(None),
            )
        )
    )
    emd_rows = emd_result.fetchall()

    for emd, tender_title in emd_rows:
        days = _days_remaining(emd.validity_end_date)
        if days <= 7:
            key = _alert_key("emd_expiry", str(emd.id), 7)
            if key not in dismissed_keys:
                if days < 0:
                    msg = f"Expired {abs(days)} day(s) ago"
                elif days == 0:
                    msg = "Expires today"
                else:
                    msg = f"Expires in {days} day(s)"
                alerts.append({
                    "id": key,
                    "tender_id": str(emd.tender_id),
                    "tender_title": tender_title,
                    "alert_type": "emd_expiry",
                    "severity": _severity(days),
                    "message": f"EMD ({emd.mode.upper()} {emd.instrument_number or ''}): {msg}",
                    "due_date": str(emd.validity_end_date) if emd.validity_end_date else None,
                    "days_remaining": days,
                })

    # --- Fee payment due alerts ---
    fee_result = await db.execute(
        select(TenderFee, Tender.title, Tender.deadline).join(
            Tender, TenderFee.tender_id == Tender.id
        ).where(
            and_(
                Tender.tenant_id == UUID(tenant_id),
                TenderFee.status == "pending",
            )
        )
    )
    fee_rows = fee_result.fetchall()

    for fee, tender_title, tender_deadline in fee_rows:
        # Use tender deadline as proxy for fee due date
        days = _days_remaining(tender_deadline)
        if days <= 7:
            key = _alert_key("fee_due", str(fee.id), 7)
            if key not in dismissed_keys:
                alerts.append({
                    "id": key,
                    "tender_id": str(fee.tender_id),
                    "tender_title": tender_title,
                    "alert_type": "fee_due",
                    "severity": "warning",
                    "message": f"{fee.fee_type.replace('_', ' ').title()} pending: {fee.amount:,.0f}",
                    "due_date": str(tender_deadline) if tender_deadline else None,
                    "days_remaining": days,
                })

    # Sort: critical first, then warning, then info
    severity_order = {"critical": 0, "warning": 1, "info": 2}
    alerts.sort(key=lambda a: (severity_order.get(a["severity"], 3), a["days_remaining"]))

    return alerts


async def dismiss_alert(
    db: AsyncSession,
    tenant_id: str,
    user_id: str,
    alert_id: str,
) -> dict:
    """Dismiss an alert for a specific user."""
    dismissed = DismissedAlert(
        tenant_id=UUID(tenant_id),
        user_id=UUID(user_id),
        alert_key=alert_id,
    )
    db.add(dismissed)
    await db.flush()
    return {"dismissed": True, "alert_id": alert_id}


async def get_alert_summary(
    db: AsyncSession,
    tenant_id: str,
    user_id: str,
) -> dict:
    """Get alert counts by severity."""
    alerts = await get_tender_alerts(db, tenant_id, user_id)
    summary = {"critical": 0, "warning": 0, "info": 0, "total": len(alerts)}
    for a in alerts:
        sev = a["severity"]
        if sev in summary:
            summary[sev] += 1
    return summary
