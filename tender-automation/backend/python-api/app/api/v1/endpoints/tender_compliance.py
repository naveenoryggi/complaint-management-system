"""Technical Compliance Matrix, BoQ, Milestones, Corrigendum, Eligibility, QCBS endpoints."""
from typing import List, Optional
from uuid import UUID
from datetime import datetime
from decimal import Decimal
from fastapi import APIRouter, Depends, HTTPException, Query, status
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, func, delete

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.models.compliance_matrix import (
    TechnicalComplianceItem, BoQLineItem, TenderMilestone, TenderCorrigendum
)
from app.models.tender import Tender
from app.models.company import CompanyProfile, Certification
from app.models.reference_bundle import ReferenceBundle

router = APIRouter()


# ---------------------------------------------------------------------------
# Pydantic Schemas
# ---------------------------------------------------------------------------

class ComplianceItemCreate(BaseModel):
    clause_number: str
    clause_title: str
    clause_description: str | None = None
    section: str | None = None
    compliance_status: str = "pending"
    deviation_remarks: str | None = None
    page_reference: str | None = None
    supporting_document_id: str | None = None
    is_mandatory: bool = True
    is_critical: bool = False
    sort_order: int = 0

class ComplianceItemUpdate(BaseModel):
    clause_number: str | None = None
    clause_title: str | None = None
    clause_description: str | None = None
    section: str | None = None
    compliance_status: str | None = None
    deviation_remarks: str | None = None
    page_reference: str | None = None
    supporting_document_id: str | None = None
    is_mandatory: bool | None = None
    is_critical: bool | None = None
    sort_order: int | None = None

class BoQItemCreate(BaseModel):
    item_number: str
    description: str
    unit: str | None = None
    quantity: float = 1
    unit_rate: float | None = None
    gst_percentage: float | None = 18.0
    category: str | None = None
    is_optional: bool = False
    make_model: str | None = None
    oem_id: str | None = None
    sort_order: int = 0
    notes: str | None = None

class BoQItemUpdate(BaseModel):
    item_number: str | None = None
    description: str | None = None
    unit: str | None = None
    quantity: float | None = None
    unit_rate: float | None = None
    gst_percentage: float | None = None
    category: str | None = None
    is_optional: bool | None = None
    make_model: str | None = None
    oem_id: str | None = None
    sort_order: int | None = None
    notes: str | None = None

class MilestoneCreate(BaseModel):
    milestone_type: str
    label: str
    event_date: str | None = None
    event_time: str | None = None
    location: str | None = None
    notes: str | None = None
    alert_days_before: int | None = 2
    sort_order: int = 0

class MilestoneUpdate(BaseModel):
    milestone_type: str | None = None
    label: str | None = None
    event_date: str | None = None
    event_time: str | None = None
    location: str | None = None
    is_completed: bool | None = None
    notes: str | None = None
    alert_days_before: int | None = None
    sort_order: int | None = None

class CorrigendumCreate(BaseModel):
    corrigendum_number: int
    issue_date: str | None = None
    title: str
    summary: str | None = None
    changes: list | None = None
    document_path: str | None = None

class CorrigendumUpdate(BaseModel):
    issue_date: str | None = None
    title: str | None = None
    summary: str | None = None
    changes: list | None = None
    document_path: str | None = None
    is_acknowledged: bool | None = None


# ---------------------------------------------------------------------------
# Technical Compliance Matrix CRUD
# ---------------------------------------------------------------------------

@router.get("/{tender_id}/compliance-matrix")
async def list_compliance_items(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all technical compliance items for a tender."""
    result = await db.execute(
        select(TechnicalComplianceItem)
        .where(TechnicalComplianceItem.tender_id == tender_id)
        .order_by(TechnicalComplianceItem.sort_order, TechnicalComplianceItem.clause_number)
    )
    items = result.scalars().all()
    return [_compliance_item_to_dict(i) for i in items]


@router.post("/{tender_id}/compliance-matrix", status_code=status.HTTP_201_CREATED)
async def create_compliance_item(
    tender_id: UUID,
    data: ComplianceItemCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a technical compliance matrix item."""
    item = TechnicalComplianceItem(
        tender_id=tender_id,
        clause_number=data.clause_number,
        clause_title=data.clause_title,
        clause_description=data.clause_description,
        section=data.section,
        compliance_status=data.compliance_status,
        deviation_remarks=data.deviation_remarks,
        page_reference=data.page_reference,
        supporting_document_id=UUID(data.supporting_document_id) if data.supporting_document_id else None,
        is_mandatory=data.is_mandatory,
        is_critical=data.is_critical,
        sort_order=data.sort_order,
    )
    db.add(item)
    await db.flush()
    return _compliance_item_to_dict(item)


@router.post("/{tender_id}/compliance-matrix/bulk", status_code=status.HTTP_201_CREATED)
async def bulk_create_compliance_items(
    tender_id: UUID,
    items: List[ComplianceItemCreate],
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Bulk create compliance matrix items (e.g., from AI extraction)."""
    created = []
    for i, data in enumerate(items):
        item = TechnicalComplianceItem(
            tender_id=tender_id,
            clause_number=data.clause_number,
            clause_title=data.clause_title,
            clause_description=data.clause_description,
            section=data.section,
            compliance_status=data.compliance_status,
            is_mandatory=data.is_mandatory,
            is_critical=data.is_critical,
            sort_order=data.sort_order or i,
        )
        db.add(item)
        created.append(item)
    await db.flush()
    return {"created": len(created)}


@router.put("/compliance-matrix/{item_id}")
async def update_compliance_item(
    item_id: UUID,
    data: ComplianceItemUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a compliance matrix item."""
    result = await db.execute(
        select(TechnicalComplianceItem).where(TechnicalComplianceItem.id == item_id)
    )
    item = result.scalars().first()
    if not item:
        raise HTTPException(status_code=404, detail="Compliance item not found")

    update_data = data.model_dump(exclude_unset=True)
    if "supporting_document_id" in update_data and update_data["supporting_document_id"]:
        update_data["supporting_document_id"] = UUID(update_data["supporting_document_id"])
    for key, value in update_data.items():
        setattr(item, key, value)
    await db.flush()
    return _compliance_item_to_dict(item)


@router.delete("/compliance-matrix/{item_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_compliance_item(
    item_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a compliance matrix item."""
    result = await db.execute(
        select(TechnicalComplianceItem).where(TechnicalComplianceItem.id == item_id)
    )
    item = result.scalars().first()
    if not item:
        raise HTTPException(status_code=404, detail="Compliance item not found")
    await db.delete(item)


@router.get("/{tender_id}/compliance-matrix/summary")
async def get_compliance_matrix_summary(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get compliance matrix summary stats."""
    result = await db.execute(
        select(TechnicalComplianceItem)
        .where(TechnicalComplianceItem.tender_id == tender_id)
    )
    items = result.scalars().all()
    total = len(items)
    if total == 0:
        return {"total": 0, "complied": 0, "not_complied": 0, "partially_complied": 0, "pending": 0, "compliance_percentage": 0}

    counts = {}
    for i in items:
        counts[i.compliance_status] = counts.get(i.compliance_status, 0) + 1

    complied = counts.get("complied", 0)
    non_pending = total - counts.get("pending", 0) - counts.get("na", 0)
    pct = round((complied / non_pending * 100), 1) if non_pending > 0 else 0

    critical_fail = any(i.compliance_status == "not_complied" and i.is_critical for i in items)

    return {
        "total": total,
        "complied": complied,
        "not_complied": counts.get("not_complied", 0),
        "partially_complied": counts.get("partially_complied", 0),
        "pending": counts.get("pending", 0),
        "na": counts.get("na", 0),
        "compliance_percentage": pct,
        "has_critical_failure": critical_fail,
    }


@router.post("/{tender_id}/compliance-matrix/interpret")
async def interpret_clauses(
    tender_id: UUID,
    clause_ids: list[str] | None = None,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """AI-interpret tender clauses with risk scoring and cross-reference detection.

    Analyzes each clause to provide:
    - Plain-English interpretation
    - Risk score (0-10) and category (low/medium/high/critical)
    - Cross-references to related clauses
    - Red flags that need immediate attention

    Pass clause_ids to interpret specific clauses, or omit to interpret all.
    """
    from app.services.clause_interpretation_service import interpret_clauses as _interpret

    try:
        result = await _interpret(
            db=db,
            tender_id=tender_id,
            tenant_id=current_user.tenant_id,
            clause_ids=clause_ids,
        )
        return result
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Clause interpretation failed: {str(e)}")


@router.get("/{tender_id}/compliance-matrix/risk-summary")
async def get_risk_summary(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get risk summary for all interpreted clauses."""
    result = await db.execute(
        select(TechnicalComplianceItem)
        .where(TechnicalComplianceItem.tender_id == tender_id)
        .where(TechnicalComplianceItem.risk_score.isnot(None))
        .order_by(TechnicalComplianceItem.risk_score.desc())
    )
    items = result.scalars().all()

    if not items:
        return {"message": "No clauses interpreted yet. Run interpretation first.", "items": []}

    dist = {"low": 0, "medium": 0, "high": 0, "critical": 0}
    scores = []
    red_flags = []

    for item in items:
        cat = item.risk_category or "low"
        if cat in dist:
            dist[cat] += 1
        if item.risk_score:
            scores.append(item.risk_score)
        if item.risk_score and item.risk_score >= 8:
            red_flags.append({
                "clause_number": item.clause_number,
                "clause_title": item.clause_title,
                "risk_score": item.risk_score,
                "risk_reason": item.risk_reason,
            })

    return {
        "total_interpreted": len(items),
        "risk_distribution": dist,
        "average_risk": round(sum(scores) / len(scores), 2) if scores else 0,
        "max_risk": max(scores) if scores else 0,
        "red_flags": red_flags,
        "top_risks": [
            {
                "id": str(i.id),
                "clause_number": i.clause_number,
                "clause_title": i.clause_title,
                "risk_score": i.risk_score,
                "risk_category": i.risk_category,
                "risk_reason": i.risk_reason,
                "interpretation": i.interpretation,
                "cross_references": i.cross_references,
            }
            for i in items[:10]  # Top 10 by risk (already sorted desc)
        ],
    }


def _compliance_item_to_dict(item: TechnicalComplianceItem) -> dict:
    return {
        "id": str(item.id),
        "tender_id": str(item.tender_id),
        "clause_number": item.clause_number,
        "clause_title": item.clause_title,
        "clause_description": item.clause_description,
        "section": item.section,
        "compliance_status": item.compliance_status,
        "deviation_remarks": item.deviation_remarks,
        "page_reference": item.page_reference,
        "supporting_document_id": str(item.supporting_document_id) if item.supporting_document_id else None,
        "is_mandatory": item.is_mandatory,
        "is_critical": item.is_critical,
        "ai_extracted": item.ai_extracted,
        "sort_order": item.sort_order,
        "risk_score": item.risk_score,
        "risk_category": item.risk_category,
        "risk_reason": item.risk_reason,
        "interpretation": item.interpretation,
        "cross_references": item.cross_references,
        "created_at": item.created_at.isoformat() if item.created_at else None,
        "updated_at": item.updated_at.isoformat() if item.updated_at else None,
    }


# ---------------------------------------------------------------------------
# Bill of Quantities (BoQ) / Price Schedule
# ---------------------------------------------------------------------------

@router.get("/{tender_id}/boq")
async def list_boq_items(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all BoQ items for a tender."""
    result = await db.execute(
        select(BoQLineItem)
        .where(BoQLineItem.tender_id == tender_id)
        .order_by(BoQLineItem.sort_order, BoQLineItem.item_number)
    )
    items = result.scalars().all()
    return [_boq_to_dict(i) for i in items]


@router.post("/{tender_id}/boq", status_code=status.HTTP_201_CREATED)
async def create_boq_item(
    tender_id: UUID,
    data: BoQItemCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a BoQ line item with auto-computed totals."""
    total_before = Decimal(str(data.quantity)) * Decimal(str(data.unit_rate or 0))
    gst_pct = Decimal(str(data.gst_percentage or 0))
    gst_amt = total_before * gst_pct / 100
    total_with = total_before + gst_amt

    item = BoQLineItem(
        tender_id=tender_id,
        item_number=data.item_number,
        description=data.description,
        unit=data.unit,
        quantity=data.quantity,
        unit_rate=data.unit_rate,
        total_before_tax=total_before,
        gst_percentage=data.gst_percentage,
        gst_amount=gst_amt,
        total_with_tax=total_with,
        category=data.category,
        is_optional=data.is_optional,
        make_model=data.make_model,
        oem_id=UUID(data.oem_id) if data.oem_id else None,
        sort_order=data.sort_order,
        notes=data.notes,
    )
    db.add(item)
    await db.flush()
    return _boq_to_dict(item)


@router.put("/boq/{item_id}")
async def update_boq_item(
    item_id: UUID,
    data: BoQItemUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a BoQ item with auto-recomputation of totals."""
    result = await db.execute(select(BoQLineItem).where(BoQLineItem.id == item_id))
    item = result.scalars().first()
    if not item:
        raise HTTPException(status_code=404, detail="BoQ item not found")

    update_data = data.model_dump(exclude_unset=True)
    if "oem_id" in update_data and update_data["oem_id"]:
        update_data["oem_id"] = UUID(update_data["oem_id"])
    for key, value in update_data.items():
        setattr(item, key, value)

    # Recompute totals
    qty = Decimal(str(item.quantity or 1))
    rate = Decimal(str(item.unit_rate or 0))
    gst_pct = Decimal(str(item.gst_percentage or 0))
    item.total_before_tax = qty * rate
    item.gst_amount = item.total_before_tax * gst_pct / 100
    item.total_with_tax = item.total_before_tax + item.gst_amount

    await db.flush()
    return _boq_to_dict(item)


@router.delete("/boq/{item_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_boq_item(
    item_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a BoQ item."""
    result = await db.execute(select(BoQLineItem).where(BoQLineItem.id == item_id))
    item = result.scalars().first()
    if not item:
        raise HTTPException(status_code=404, detail="BoQ item not found")
    await db.delete(item)


@router.get("/{tender_id}/boq/summary")
async def get_boq_summary(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get BoQ totals and category breakdown."""
    result = await db.execute(
        select(BoQLineItem).where(BoQLineItem.tender_id == tender_id)
    )
    items = result.scalars().all()

    mandatory = [i for i in items if not i.is_optional]
    optional = [i for i in items if i.is_optional]

    def _sum(lst, field):
        return float(sum(getattr(i, field) or 0 for i in lst))

    # Category breakdown
    categories = {}
    for i in items:
        cat = i.category or "Uncategorized"
        if cat not in categories:
            categories[cat] = {"count": 0, "total_before_tax": 0, "total_with_tax": 0}
        categories[cat]["count"] += 1
        categories[cat]["total_before_tax"] += float(i.total_before_tax or 0)
        categories[cat]["total_with_tax"] += float(i.total_with_tax or 0)

    return {
        "total_items": len(items),
        "mandatory_items": len(mandatory),
        "optional_items": len(optional),
        "total_before_tax": _sum(items, "total_before_tax"),
        "total_gst": _sum(items, "gst_amount"),
        "total_with_tax": _sum(items, "total_with_tax"),
        "mandatory_total": _sum(mandatory, "total_with_tax"),
        "optional_total": _sum(optional, "total_with_tax"),
        "category_breakdown": categories,
    }


def _boq_to_dict(item: BoQLineItem) -> dict:
    return {
        "id": str(item.id),
        "tender_id": str(item.tender_id),
        "item_number": item.item_number,
        "description": item.description,
        "unit": item.unit,
        "quantity": float(item.quantity) if item.quantity else None,
        "unit_rate": float(item.unit_rate) if item.unit_rate else None,
        "total_before_tax": float(item.total_before_tax) if item.total_before_tax else None,
        "gst_percentage": item.gst_percentage,
        "gst_amount": float(item.gst_amount) if item.gst_amount else None,
        "total_with_tax": float(item.total_with_tax) if item.total_with_tax else None,
        "category": item.category,
        "is_optional": item.is_optional,
        "make_model": item.make_model,
        "oem_id": str(item.oem_id) if item.oem_id else None,
        "sort_order": item.sort_order,
        "notes": item.notes,
        "created_at": item.created_at.isoformat() if item.created_at else None,
        "updated_at": item.updated_at.isoformat() if item.updated_at else None,
    }


# ---------------------------------------------------------------------------
# Tender Milestones / Important Dates
# ---------------------------------------------------------------------------

@router.get("/{tender_id}/milestones")
async def list_milestones(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all milestones/important dates for a tender."""
    result = await db.execute(
        select(TenderMilestone)
        .where(TenderMilestone.tender_id == tender_id)
        .order_by(TenderMilestone.sort_order, TenderMilestone.event_date)
    )
    return [_milestone_to_dict(m) for m in result.scalars().all()]


@router.post("/{tender_id}/milestones", status_code=status.HTTP_201_CREATED)
async def create_milestone(
    tender_id: UUID,
    data: MilestoneCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a tender milestone."""
    milestone = TenderMilestone(
        tender_id=tender_id,
        milestone_type=data.milestone_type,
        label=data.label,
        event_date=datetime.fromisoformat(data.event_date) if data.event_date else None,
        event_time=data.event_time,
        location=data.location,
        notes=data.notes,
        alert_days_before=data.alert_days_before,
        sort_order=data.sort_order,
    )
    db.add(milestone)
    await db.flush()
    return _milestone_to_dict(milestone)


@router.put("/milestones/{milestone_id}")
async def update_milestone(
    milestone_id: UUID,
    data: MilestoneUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a milestone."""
    result = await db.execute(select(TenderMilestone).where(TenderMilestone.id == milestone_id))
    milestone = result.scalars().first()
    if not milestone:
        raise HTTPException(status_code=404, detail="Milestone not found")

    update_data = data.model_dump(exclude_unset=True)
    if "event_date" in update_data and update_data["event_date"]:
        update_data["event_date"] = datetime.fromisoformat(update_data["event_date"])
    for key, value in update_data.items():
        setattr(milestone, key, value)
    await db.flush()
    return _milestone_to_dict(milestone)


@router.delete("/milestones/{milestone_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_milestone(
    milestone_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a milestone."""
    result = await db.execute(select(TenderMilestone).where(TenderMilestone.id == milestone_id))
    milestone = result.scalars().first()
    if not milestone:
        raise HTTPException(status_code=404, detail="Milestone not found")
    await db.delete(milestone)


def _milestone_to_dict(m: TenderMilestone) -> dict:
    return {
        "id": str(m.id),
        "tender_id": str(m.tender_id),
        "milestone_type": m.milestone_type,
        "label": m.label,
        "event_date": m.event_date.isoformat() if m.event_date else None,
        "event_time": m.event_time,
        "location": m.location,
        "is_completed": m.is_completed,
        "notes": m.notes,
        "alert_days_before": m.alert_days_before,
        "sort_order": m.sort_order,
    }


# ---------------------------------------------------------------------------
# Corrigendum Management
# ---------------------------------------------------------------------------

@router.get("/{tender_id}/corrigendums")
async def list_corrigendums(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all corrigendums for a tender."""
    result = await db.execute(
        select(TenderCorrigendum)
        .where(TenderCorrigendum.tender_id == tender_id)
        .order_by(TenderCorrigendum.corrigendum_number)
    )
    return [_corrigendum_to_dict(c) for c in result.scalars().all()]


@router.post("/{tender_id}/corrigendums", status_code=status.HTTP_201_CREATED)
async def create_corrigendum(
    tender_id: UUID,
    data: CorrigendumCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a corrigendum record."""
    corrigendum = TenderCorrigendum(
        tender_id=tender_id,
        corrigendum_number=data.corrigendum_number,
        issue_date=datetime.strptime(data.issue_date, "%Y-%m-%d").date() if data.issue_date else None,
        title=data.title,
        summary=data.summary,
        changes=data.changes,
        document_path=data.document_path,
    )
    db.add(corrigendum)
    await db.flush()
    return _corrigendum_to_dict(corrigendum)


@router.put("/corrigendums/{corrigendum_id}")
async def update_corrigendum(
    corrigendum_id: UUID,
    data: CorrigendumUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a corrigendum."""
    result = await db.execute(select(TenderCorrigendum).where(TenderCorrigendum.id == corrigendum_id))
    corrigendum = result.scalars().first()
    if not corrigendum:
        raise HTTPException(status_code=404, detail="Corrigendum not found")

    update_data = data.model_dump(exclude_unset=True)
    if "issue_date" in update_data and update_data["issue_date"]:
        update_data["issue_date"] = datetime.strptime(update_data["issue_date"], "%Y-%m-%d").date()
    for key, value in update_data.items():
        setattr(corrigendum, key, value)
    await db.flush()
    return _corrigendum_to_dict(corrigendum)


@router.delete("/corrigendums/{corrigendum_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_corrigendum(
    corrigendum_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a corrigendum."""
    result = await db.execute(select(TenderCorrigendum).where(TenderCorrigendum.id == corrigendum_id))
    corrigendum = result.scalars().first()
    if not corrigendum:
        raise HTTPException(status_code=404, detail="Corrigendum not found")
    await db.delete(corrigendum)


@router.post("/{tender_id}/corrigendums/{corrigendum_id}/analyze")
async def analyze_corrigendum_changes(
    tender_id: UUID,
    corrigendum_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """AI-analyze a corrigendum to extract structured changes.

    Identifies what changed, impact level, and required bidder actions.
    Updates the corrigendum's `changes` field with structured data.
    """
    from app.services.corrigendum_service import analyze_corrigendum

    try:
        result = await analyze_corrigendum(
            db=db,
            tender_id=tender_id,
            corrigendum_id=corrigendum_id,
            tenant_id=current_user.tenant_id,
        )
        return result
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Corrigendum analysis failed: {str(e)}")


def _corrigendum_to_dict(c: TenderCorrigendum) -> dict:
    return {
        "id": str(c.id),
        "tender_id": str(c.tender_id),
        "corrigendum_number": c.corrigendum_number,
        "issue_date": c.issue_date.isoformat() if c.issue_date else None,
        "title": c.title,
        "summary": c.summary,
        "changes": c.changes,
        "document_path": c.document_path,
        "is_acknowledged": c.is_acknowledged,
        "created_at": c.created_at.isoformat() if c.created_at else None,
    }


# ---------------------------------------------------------------------------
# Eligibility Auto-Check (Turnover + Experience)
# ---------------------------------------------------------------------------

@router.get("/{tender_id}/eligibility-check")
async def check_eligibility(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Auto-check PQ eligibility: turnover, experience, certifications, EMD exemption.
    Compares company profile and reference bundles against tender requirements.
    """
    # Load tender
    tender_result = await db.execute(select(Tender).where(Tender.id == tender_id))
    tender = tender_result.scalars().first()
    if not tender:
        raise HTTPException(status_code=404, detail="Tender not found")

    # Load company profile
    profile_result = await db.execute(
        select(CompanyProfile).where(CompanyProfile.tenant_id == UUID(current_user.tenant_id))
    )
    profile = profile_result.scalars().first()

    # Load certifications
    certs = []
    if profile:
        cert_result = await db.execute(
            select(Certification).where(Certification.company_id == profile.id)
        )
        certs = cert_result.scalars().all()

    # Load reference bundles
    bundle_result = await db.execute(
        select(ReferenceBundle)
        .where(ReferenceBundle.tenant_id == UUID(current_user.tenant_id))
        .where(ReferenceBundle.status.in_(["completed", "ongoing"]))
    )
    bundles = bundle_result.scalars().all()

    # Extract requirements from tender
    reqs = tender.requirements or {}
    estimated_value = float(tender.estimated_value or 0)

    checks = []

    # --- 1. Turnover Check ---
    turnover_req = reqs.get("min_annual_turnover") or reqs.get("turnover_requirement")
    annual_turnover = profile.annual_turnover if profile and profile.annual_turnover else {}

    if turnover_req:
        try:
            req_amount = float(turnover_req)
        except (ValueError, TypeError):
            req_amount = 0

        turnover_values = [float(v) for v in annual_turnover.values() if v]
        avg_turnover = sum(turnover_values) / len(turnover_values) if turnover_values else 0

        checks.append({
            "check": "annual_turnover",
            "label": "Average Annual Turnover",
            "requirement": f"Min {_format_inr(req_amount)}",
            "actual": f"{_format_inr(avg_turnover)} (avg of {len(turnover_values)} years)",
            "status": "pass" if avg_turnover >= req_amount else "fail",
            "is_critical": True,
        })
    else:
        # Auto-estimate: typically 30% of estimated value per year
        if estimated_value > 0:
            threshold = estimated_value * 0.3
            turnover_values = [float(v) for v in annual_turnover.values() if v]
            avg_turnover = sum(turnover_values) / len(turnover_values) if turnover_values else 0
            checks.append({
                "check": "annual_turnover",
                "label": "Average Annual Turnover (estimated threshold)",
                "requirement": f"~{_format_inr(threshold)} (30% of estimated value)",
                "actual": f"{_format_inr(avg_turnover)}" if turnover_values else "Not entered",
                "status": "pass" if avg_turnover >= threshold else ("warn" if not turnover_values else "fail"),
                "is_critical": True,
            })

    # --- 2. Similar Work Experience ---
    min_similar_works = reqs.get("min_similar_works", 3)
    min_work_value_pct = reqs.get("min_work_value_percentage", 40)
    experience_years = reqs.get("experience_years", 7)

    if estimated_value > 0:
        min_work_value = estimated_value * min_work_value_pct / 100
    else:
        min_work_value = 0

    from datetime import datetime as dt
    cutoff_year = dt.now().year - experience_years
    qualifying_bundles = []
    for b in bundles:
        if b.total_value and float(b.total_value) >= min_work_value:
            if b.actual_completion_date and b.actual_completion_date.year >= cutoff_year:
                qualifying_bundles.append(b)
            elif b.status == "ongoing":
                qualifying_bundles.append(b)

    checks.append({
        "check": "similar_works",
        "label": f"Similar Work Experience (last {experience_years} years)",
        "requirement": f"{min_similar_works} works of value >= {_format_inr(min_work_value)} ({min_work_value_pct}% of EV)",
        "actual": f"{len(qualifying_bundles)} qualifying work(s)",
        "qualifying_works": [
            {"name": b.bundle_name, "client": b.client_name, "value": float(b.total_value or 0)}
            for b in qualifying_bundles[:5]
        ],
        "status": "pass" if len(qualifying_bundles) >= min_similar_works else "fail",
        "is_critical": True,
    })

    # --- 3. Certification Check ---
    required_certs = reqs.get("required_certifications", [])
    if required_certs:
        valid_cert_names = {c.name.lower() for c in certs if c.is_valid}
        for req_cert in required_certs:
            found = any(req_cert.lower() in cn for cn in valid_cert_names)
            checks.append({
                "check": f"certification_{req_cert}",
                "label": f"Certification: {req_cert}",
                "requirement": "Required and valid",
                "actual": "Found" if found else "Not found",
                "status": "pass" if found else "fail",
                "is_critical": False,
            })
    else:
        has_any_valid = any(c.is_valid for c in certs)
        checks.append({
            "check": "certifications_general",
            "label": "Valid Certifications",
            "requirement": "At least one valid certification",
            "actual": f"{sum(1 for c in certs if c.is_valid)} valid certification(s)",
            "status": "pass" if has_any_valid else "warn",
            "is_critical": False,
        })

    # --- 4. EMD Exemption Check (MSME / Startup India) ---
    is_msme = bool(profile and profile.msme_registration)
    emd_amount = reqs.get("emd", {}).get("amount", 0) if isinstance(reqs.get("emd"), dict) else 0

    checks.append({
        "check": "emd_exemption",
        "label": "EMD Exemption Eligibility (MSME/Startup)",
        "requirement": "MSME registration under MSMED Act / Startup India DPIIT",
        "actual": f"MSME: {profile.msme_registration}" if is_msme else "Not registered as MSME",
        "status": "pass" if is_msme else "info",
        "is_critical": False,
        "savings": float(emd_amount) if is_msme and emd_amount else 0,
        "note": "Eligible for EMD exemption under GFR 170(i)" if is_msme else "Submit Bid Security Declaration (BSD) if MSME exemption claimed",
    })

    # --- 5. Company Profile Completeness ---
    if profile:
        required_fields = ["company_name", "pan_number", "gstin", "registered_address", "bank_name", "account_number", "ifsc_code"]
        filled = sum(1 for f in required_fields if getattr(profile, f, None))
        checks.append({
            "check": "profile_completeness",
            "label": "Company Profile Completeness",
            "requirement": f"All {len(required_fields)} mandatory fields",
            "actual": f"{filled}/{len(required_fields)} fields filled",
            "status": "pass" if filled == len(required_fields) else "fail",
            "is_critical": False,
        })
    else:
        checks.append({
            "check": "profile_completeness",
            "label": "Company Profile",
            "requirement": "Company profile must exist",
            "actual": "Not created",
            "status": "fail",
            "is_critical": True,
        })

    # Overall
    critical_checks = [c for c in checks if c.get("is_critical")]
    all_pass = all(c["status"] == "pass" for c in critical_checks) if critical_checks else True
    total_pass = sum(1 for c in checks if c["status"] == "pass")

    return {
        "tender_id": str(tender_id),
        "eligible": all_pass,
        "overall_status": "Eligible" if all_pass else "Not Eligible (Critical Failures)",
        "checks": checks,
        "summary": {
            "total_checks": len(checks),
            "passed": total_pass,
            "failed": sum(1 for c in checks if c["status"] == "fail"),
            "warnings": sum(1 for c in checks if c["status"] == "warn"),
        },
    }


# ---------------------------------------------------------------------------
# QCBS Score Calculator
# ---------------------------------------------------------------------------

@router.post("/{tender_id}/qcbs-calculate")
async def calculate_qcbs_score(
    tender_id: UUID,
    technical_weight: float = Query(default=70, description="Technical weight (e.g., 70)"),
    financial_weight: float = Query(default=30, description="Financial weight (e.g., 30)"),
    our_technical_score: float = Query(default=0, description="Our technical score (0-100)"),
    our_financial_bid: float = Query(default=0, description="Our financial bid amount"),
    lowest_financial_bid: float = Query(default=0, description="Lowest financial bid (L1)"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Calculate QCBS combined score.
    Formula: Combined = (Ts × Wt/100) + (Fs × Wf/100)
    Where Fs = (Fm / F) × 100 (Fm = lowest bid, F = our bid)
    """
    if our_financial_bid <= 0:
        raise HTTPException(status_code=400, detail="Financial bid must be > 0")

    # Financial score: Fs = (Lowest / Ours) * 100
    financial_score = (lowest_financial_bid / our_financial_bid * 100) if lowest_financial_bid > 0 else 0
    financial_score = min(financial_score, 100)  # Cap at 100

    # Combined score
    combined = (our_technical_score * technical_weight / 100) + (financial_score * financial_weight / 100)

    return {
        "tender_id": str(tender_id),
        "technical_weight": technical_weight,
        "financial_weight": financial_weight,
        "our_technical_score": round(our_technical_score, 2),
        "our_financial_bid": our_financial_bid,
        "lowest_financial_bid": lowest_financial_bid,
        "financial_score": round(financial_score, 2),
        "combined_score": round(combined, 2),
        "breakdown": {
            "technical_contribution": round(our_technical_score * technical_weight / 100, 2),
            "financial_contribution": round(financial_score * financial_weight / 100, 2),
        },
        "notes": [
            f"Technical: {our_technical_score:.1f} × {technical_weight:.0f}% = {our_technical_score * technical_weight / 100:.2f}",
            f"Financial: ({lowest_financial_bid:,.0f} / {our_financial_bid:,.0f}) × 100 = {financial_score:.2f} × {financial_weight:.0f}% = {financial_score * financial_weight / 100:.2f}",
            f"Combined Score = {combined:.2f} / 100",
        ],
    }


def _format_inr(amount: float) -> str:
    """Format amount in Indian numbering system."""
    if amount >= 10000000:
        return f"Rs. {amount / 10000000:.2f} Cr"
    if amount >= 100000:
        return f"Rs. {amount / 100000:.2f} L"
    return f"Rs. {amount:,.0f}"
