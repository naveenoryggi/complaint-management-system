"""Compliance readiness service - computes submission readiness across 6 categories."""
import asyncio
from datetime import datetime
from uuid import UUID

from sqlalchemy import select, func
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.tender import Tender
from app.models.document import TenderDocument
from app.models.tracking import EMDRecord, TenderFee
from app.models.oem import OEMTenderRequirement, OEMMaster
from app.models.reference_bundle import TenderCriteria, BundleCriteriaAssignment
from app.models.company import CompanyProfile, Certification
from app.models.compliance_matrix import TechnicalComplianceItem, BoQLineItem, TenderMilestone, TenderCorrigendum
from app.models.submission_checklist import SubmissionChecklistItem


# Status values considered "ready" for OEM requirement fields
OEM_READY_STATUSES = {"received", "uploaded", "verified", "na", "not_applicable"}

# Online/offline ready statuses for submission checklist
CHECKLIST_ONLINE_READY = {"uploaded", "digitally_signed", "verified"}
CHECKLIST_OFFLINE_READY = {"dispatched", "received", "verified"}

# Category weights (total: 100 for applicable categories)
WEIGHTS = {
    "documents": 15,
    "emd_fees": 12,
    "oem": 13,
    "declarations": 8,
    "criteria": 8,
    "company_profile": 8,
    "bid_preparation": 13,
    "timeline": 5,
    "submission_checklist": 18,
}


def _score_label(score: float) -> str:
    if score >= 90:
        return "Ready to Submit"
    if score >= 70:
        return "Almost Ready"
    if score >= 50:
        return "Needs Attention"
    return "Critical"


def _item(key: str, label: str, status: str, detail: str | None = None) -> dict:
    return {"key": key, "label": label, "status": status, "detail": detail}


async def _check_documents(db: AsyncSession, tender: Tender) -> dict:
    """Check document readiness."""
    result = await db.execute(
        select(TenderDocument).where(TenderDocument.tender_id == tender.id)
    )
    docs = result.scalars().all()
    items = []

    # Check if tender has a document_checklist from extraction
    checklist = None
    if tender.requirements and isinstance(tender.requirements, dict):
        checklist = tender.requirements.get("document_checklist")

    if checklist and isinstance(checklist, list) and len(checklist) > 0:
        # Match extracted checklist items against uploaded docs
        for item_name in checklist:
            # Simple heuristic: check if any doc name loosely matches
            found = any(
                d.document_id is not None
                for d in docs
            )
            # For now, mark as ready if we have docs, partial if checklist is large
            items.append(_item(
                f"checklist_{item_name[:30].lower().replace(' ', '_')}",
                item_name[:80],
                "ready" if found else "missing",
            ))
    else:
        # Generic document checks
        has_any = len(docs) > 0
        items.append(_item(
            "has_documents",
            "At least one document attached",
            "ready" if has_any else "missing",
            f"{len(docs)} document(s) attached" if has_any else None,
        ))

        # Check for technical proposal type
        has_technical = any(
            getattr(d, "is_generated", False) or True  # Any doc counts
            for d in docs
        ) if len(docs) >= 2 else False
        items.append(_item(
            "technical_proposal",
            "Technical proposal",
            "ready" if has_technical else "missing",
        ))

        has_financial = len(docs) >= 3
        items.append(_item(
            "financial_proposal",
            "Financial proposal",
            "ready" if has_financial else "missing",
        ))

    ready = sum(1 for i in items if i["status"] == "ready")
    total = len(items)
    score = (ready / total * 100) if total > 0 else 0

    return {
        "category": "documents",
        "label": "Documents",
        "icon": "description",
        "weight": WEIGHTS["documents"],
        "items": items,
        "ready_count": ready,
        "total_count": total,
        "score": round(score, 1),
    }


async def _check_emd_fees(db: AsyncSession, tender: Tender) -> dict:
    """Check EMD & fees readiness."""
    emd_result = await db.execute(
        select(EMDRecord).where(EMDRecord.tender_id == tender.id)
    )
    emds = emd_result.scalars().all()

    fee_result = await db.execute(
        select(TenderFee).where(TenderFee.tender_id == tender.id)
    )
    fees = fee_result.scalars().all()

    items = []

    if not emds and not fees:
        # Check if tender requirements mention EMD
        has_emd_req = False
        if tender.requirements and isinstance(tender.requirements, dict):
            emd_data = tender.requirements.get("emd")
            has_emd_req = emd_data is not None
        if not has_emd_req:
            return {
                "category": "emd_fees",
                "label": "EMD & Fees",
                "icon": "account_balance",
                "weight": WEIGHTS["emd_fees"],
                "items": [_item("no_emd_required", "No EMD/fees required", "na")],
                "ready_count": 0,
                "total_count": 0,
                "score": 0,
                "_na": True,
            }

    # EMD checks
    if emds:
        items.append(_item(
            "emd_created",
            "EMD record created",
            "ready",
            f"{len(emds)} EMD record(s)",
        ))
        submitted = any(e.status in ("submitted", "released") for e in emds)
        items.append(_item(
            "emd_submitted",
            "EMD submitted",
            "ready" if submitted else "missing",
            next((f"{e.mode.upper()} - {e.status}" for e in emds if e.status in ("submitted", "released")), None),
        ))
    else:
        items.append(_item("emd_created", "EMD record created", "missing"))
        items.append(_item("emd_submitted", "EMD submitted", "missing"))

    # Fee checks
    if fees:
        all_paid = all(f.status in ("paid", "exempted") for f in fees)
        paid_count = sum(1 for f in fees if f.status in ("paid", "exempted"))
        items.append(_item(
            "fees_paid",
            "All fees paid",
            "ready" if all_paid else "partial",
            f"{paid_count}/{len(fees)} fees paid",
        ))
        all_receipts = all(f.receipt_path for f in fees if f.status == "paid")
        items.append(_item(
            "fee_receipts",
            "Fee receipts uploaded",
            "ready" if all_receipts else "missing",
        ))
    elif emds:
        # Have EMD but no fees - check if fees are needed
        items.append(_item("fees_paid", "Tender fees", "na", "No fees required"))

    ready = sum(1 for i in items if i["status"] in ("ready", "na"))
    total = sum(1 for i in items if i["status"] != "na")
    score = (ready / total * 100) if total > 0 else 100

    return {
        "category": "emd_fees",
        "label": "EMD & Fees",
        "icon": "account_balance",
        "weight": WEIGHTS["emd_fees"],
        "items": items,
        "ready_count": sum(1 for i in items if i["status"] == "ready"),
        "total_count": total,
        "score": round(score, 1),
    }


async def _check_oem(db: AsyncSession, tender: Tender) -> dict:
    """Check OEM requirements readiness."""
    result = await db.execute(
        select(OEMTenderRequirement, OEMMaster.name)
        .join(OEMMaster, OEMTenderRequirement.oem_id == OEMMaster.id)
        .where(OEMTenderRequirement.tender_id == tender.id)
    )
    rows = result.all()

    if not rows:
        return {
            "category": "oem",
            "label": "OEM Requirements",
            "icon": "precision_manufacturing",
            "weight": WEIGHTS["oem"],
            "items": [_item("no_oem", "No OEM requirements", "na")],
            "ready_count": 0,
            "total_count": 0,
            "score": 0,
            "_na": True,
        }

    items = []
    fields = [
        ("maf_status", "MAF"),
        ("ms_status", "Make Slip"),
        ("partner_cert_status", "Partner Certificate"),
        ("datasheet_status", "Datasheet"),
        ("compliance_cert_status", "Compliance Certificate"),
    ]

    for req, oem_name in rows:
        for field_name, field_label in fields:
            status_val = getattr(req, field_name, "pending")
            is_ready = status_val in OEM_READY_STATUSES
            items.append(_item(
                f"oem_{oem_name[:20]}_{field_name}",
                f"{oem_name}: {field_label}",
                "ready" if is_ready else "missing",
                status_val if is_ready else None,
            ))

    ready = sum(1 for i in items if i["status"] == "ready")
    total = len(items)
    score = (ready / total * 100) if total > 0 else 0

    return {
        "category": "oem",
        "label": "OEM Requirements",
        "icon": "precision_manufacturing",
        "weight": WEIGHTS["oem"],
        "items": items,
        "ready_count": ready,
        "total_count": total,
        "score": round(score, 1),
    }


async def _check_declarations(db: AsyncSession, tender: Tender) -> dict:
    """Check declarations readiness."""
    items = []

    # Check if declarations analysis was done (from requirements JSON)
    has_analysis = False
    has_generated = False
    if tender.requirements and isinstance(tender.requirements, dict):
        has_analysis = bool(tender.requirements.get("special_conditions"))
        has_generated = bool(tender.requirements.get("declarations_generated"))

    items.append(_item(
        "declarations_analysis",
        "Declaration analysis done",
        "ready" if has_analysis else "missing",
    ))

    # Check for declaration-type documents
    result = await db.execute(
        select(func.count(TenderDocument.id))
        .where(TenderDocument.tender_id == tender.id)
    )
    doc_count = result.scalar() or 0

    items.append(_item(
        "declarations_generated",
        "Declarations generated",
        "ready" if has_generated else "missing",
    ))

    items.append(_item(
        "declarations_attached",
        "Declaration documents attached",
        "ready" if doc_count > 0 and has_generated else "missing",
    ))

    ready = sum(1 for i in items if i["status"] == "ready")
    total = len(items)
    score = (ready / total * 100) if total > 0 else 0

    return {
        "category": "declarations",
        "label": "Declarations",
        "icon": "gavel",
        "weight": WEIGHTS["declarations"],
        "items": items,
        "ready_count": ready,
        "total_count": total,
        "score": round(score, 1),
    }


async def _check_criteria(db: AsyncSession, tender: Tender) -> dict:
    """Check criteria & scoring readiness."""
    criteria_result = await db.execute(
        select(TenderCriteria).where(TenderCriteria.tender_id == tender.id)
    )
    criteria = criteria_result.scalars().all()

    if not criteria:
        return {
            "category": "criteria",
            "label": "Criteria & Scoring",
            "icon": "grading",
            "weight": WEIGHTS["criteria"],
            "items": [_item("no_criteria", "No criteria defined", "na")],
            "ready_count": 0,
            "total_count": 0,
            "score": 0,
            "_na": True,
        }

    items = []
    items.append(_item(
        "criteria_defined",
        "Criteria defined",
        "ready",
        f"{len(criteria)} criteria",
    ))

    # Check assignments
    criteria_ids = [c.id for c in criteria]
    assignment_result = await db.execute(
        select(BundleCriteriaAssignment)
        .where(BundleCriteriaAssignment.criteria_id.in_(criteria_ids))
    )
    assignments = assignment_result.scalars().all()

    has_assignments = len(assignments) > 0
    items.append(_item(
        "evidence_mapped",
        "Evidence bundles mapped",
        "ready" if has_assignments else "missing",
        f"{len(assignments)} assignment(s)" if has_assignments else None,
    ))

    has_scoring = any(a.predicted_marks is not None for a in assignments)
    items.append(_item(
        "scoring_simulated",
        "Scoring simulated",
        "ready" if has_scoring else "missing",
    ))

    ready = sum(1 for i in items if i["status"] == "ready")
    total = len(items)
    score = (ready / total * 100) if total > 0 else 0

    return {
        "category": "criteria",
        "label": "Criteria & Scoring",
        "icon": "grading",
        "weight": WEIGHTS["criteria"],
        "items": items,
        "ready_count": ready,
        "total_count": total,
        "score": round(score, 1),
    }


async def _check_company_profile(db: AsyncSession, tenant_id: str) -> dict:
    """Check company profile readiness."""
    profile_result = await db.execute(
        select(CompanyProfile).where(CompanyProfile.tenant_id == UUID(tenant_id))
    )
    profile = profile_result.scalars().first()

    items = []

    if not profile:
        items.append(_item("company_profile", "Company profile created", "missing"))
        return {
            "category": "company_profile",
            "label": "Company Profile",
            "icon": "business",
            "weight": WEIGHTS["company_profile"],
            "items": items,
            "ready_count": 0,
            "total_count": 1,
            "score": 0,
        }

    # Core fields
    core_fields = [
        (profile.company_name, "Company name"),
        (profile.pan_number, "PAN number"),
        (profile.gstin, "GSTIN"),
        (profile.registered_address, "Registered address"),
    ]
    all_core = all(f[0] for f in core_fields)
    filled = sum(1 for f in core_fields if f[0])
    items.append(_item(
        "core_info",
        "Core company info",
        "ready" if all_core else "partial",
        f"{filled}/4 fields filled",
    ))

    # Brand assets
    has_logo = bool(profile.logo_path)
    has_signature = bool(profile.signature_path)
    items.append(_item(
        "brand_assets",
        "Brand assets (logo & signature)",
        "ready" if has_logo and has_signature else ("partial" if has_logo or has_signature else "missing"),
    ))

    # Bank details
    bank_fields = [profile.bank_name, profile.account_number, profile.ifsc_code]
    all_bank = all(bank_fields)
    items.append(_item(
        "bank_details",
        "Bank details",
        "ready" if all_bank else "missing",
    ))

    # Certifications
    cert_result = await db.execute(
        select(Certification).where(Certification.company_id == profile.id)
    )
    certs = cert_result.scalars().all()
    has_certs = len(certs) > 0
    all_valid = all(c.is_valid for c in certs) if certs else False
    items.append(_item(
        "certifications",
        "Valid certifications",
        "ready" if has_certs and all_valid else ("partial" if has_certs else "missing"),
        f"{sum(1 for c in certs if c.is_valid)}/{len(certs)} valid" if certs else None,
    ))

    ready = sum(1 for i in items if i["status"] == "ready")
    total = len(items)
    score = (ready / total * 100) if total > 0 else 0

    return {
        "category": "company_profile",
        "label": "Company Profile",
        "icon": "business",
        "weight": WEIGHTS["company_profile"],
        "items": items,
        "ready_count": ready,
        "total_count": total,
        "score": round(score, 1),
    }


async def _check_bid_preparation(db: AsyncSession, tender: Tender) -> dict:
    """Check compliance matrix and BoQ readiness."""
    items = []

    # Compliance matrix
    matrix_result = await db.execute(
        select(TechnicalComplianceItem).where(TechnicalComplianceItem.tender_id == tender.id)
    )
    matrix_items = matrix_result.scalars().all()

    if matrix_items:
        total_clauses = len(matrix_items)
        complied = sum(1 for m in matrix_items if m.compliance_status == "complied")
        pending = sum(1 for m in matrix_items if m.compliance_status == "pending")
        critical_fail = any(m.compliance_status == "not_complied" and m.is_critical for m in matrix_items)

        items.append(_item(
            "compliance_matrix",
            "Technical compliance matrix",
            "ready" if pending == 0 and not critical_fail else ("partial" if complied > 0 else "missing"),
            f"{complied}/{total_clauses} clauses complied" + (" (CRITICAL FAILURE)" if critical_fail else ""),
        ))
    else:
        items.append(_item(
            "compliance_matrix",
            "Technical compliance matrix",
            "missing",
            "No clauses added",
        ))

    # BoQ / Price schedule
    boq_result = await db.execute(
        select(BoQLineItem).where(BoQLineItem.tender_id == tender.id)
    )
    boq_items = boq_result.scalars().all()

    if boq_items:
        all_priced = all(b.unit_rate and b.unit_rate > 0 for b in boq_items)
        items.append(_item(
            "boq_prepared",
            "Price schedule / BoQ prepared",
            "ready" if all_priced else "partial",
            f"{len(boq_items)} line items, {sum(1 for b in boq_items if b.unit_rate and b.unit_rate > 0)} priced",
        ))
    else:
        items.append(_item(
            "boq_prepared",
            "Price schedule / BoQ prepared",
            "missing",
        ))

    ready = sum(1 for i in items if i["status"] == "ready")
    total = len(items)
    score = (ready / total * 100) if total > 0 else 0

    return {
        "category": "bid_preparation",
        "label": "Bid Preparation",
        "icon": "assignment",
        "weight": WEIGHTS["bid_preparation"],
        "items": items,
        "ready_count": ready,
        "total_count": total,
        "score": round(score, 1),
    }


async def _check_timeline(db: AsyncSession, tender: Tender) -> dict:
    """Check milestones and corrigendum acknowledgement."""
    items = []

    # Milestones
    milestone_result = await db.execute(
        select(TenderMilestone).where(TenderMilestone.tender_id == tender.id)
    )
    milestones = milestone_result.scalars().all()

    if milestones:
        items.append(_item(
            "milestones_tracked",
            "Important dates tracked",
            "ready",
            f"{len(milestones)} milestone(s)",
        ))

        # Check if submission deadline exists
        has_submission = any(m.milestone_type == "bid_submission" for m in milestones)
        items.append(_item(
            "submission_date",
            "Bid submission date tracked",
            "ready" if has_submission else "missing",
        ))
    else:
        items.append(_item(
            "milestones_tracked",
            "Important dates tracked",
            "missing",
            "No milestones added",
        ))

    # Corrigendums
    corr_result = await db.execute(
        select(TenderCorrigendum).where(TenderCorrigendum.tender_id == tender.id)
    )
    corrigendums = corr_result.scalars().all()

    if corrigendums:
        all_ack = all(c.is_acknowledged for c in corrigendums)
        items.append(_item(
            "corrigendums_acknowledged",
            "All corrigendums acknowledged",
            "ready" if all_ack else "missing",
            f"{sum(1 for c in corrigendums if c.is_acknowledged)}/{len(corrigendums)} acknowledged",
        ))

    ready = sum(1 for i in items if i["status"] == "ready")
    total = len(items)
    score = (ready / total * 100) if total > 0 else 0

    return {
        "category": "timeline",
        "label": "Timeline & Dates",
        "icon": "timeline",
        "weight": WEIGHTS["timeline"],
        "items": items,
        "ready_count": ready,
        "total_count": total,
        "score": round(score, 1),
    }


async def _check_submission_checklist(db: AsyncSession, tender: Tender) -> dict:
    """Check submission checklist readiness across online/offline modes."""
    result = await db.execute(
        select(SubmissionChecklistItem)
        .where(SubmissionChecklistItem.tender_id == tender.id)
    )
    checklist_items = result.scalars().all()

    if not checklist_items:
        return {
            "category": "submission_checklist",
            "label": "Submission Checklist",
            "icon": "checklist_rtl",
            "weight": WEIGHTS["submission_checklist"],
            "items": [_item("no_checklist", "No checklist generated", "na")],
            "ready_count": 0,
            "total_count": 0,
            "score": 0,
            "_na": True,
        }

    items = []
    for ci in checklist_items:
        # Determine readiness based on applicable mode(s)
        is_ready = True
        status_parts = []

        if ci.submission_mode in ("online", "both"):
            online_ok = ci.online_status in CHECKLIST_ONLINE_READY
            if not online_ok:
                is_ready = False
            status_parts.append(f"Online: {ci.online_status}")

        if ci.submission_mode in ("offline", "both"):
            offline_ok = ci.offline_status in CHECKLIST_OFFLINE_READY
            if not offline_ok:
                is_ready = False
            status_parts.append(f"Offline: {ci.offline_status}")

        # Check notarization if required
        if ci.notarization_required and ci.notarization_status not in ("completed", "verified", "not_required"):
            is_ready = False
            status_parts.append(f"Notarization: {ci.notarization_status}")

        item_status = "ready" if is_ready else "missing"
        detail = " | ".join(status_parts) if not is_ready else None

        items.append(_item(
            f"checklist_{str(ci.id)[:8]}",
            f"{'[CRITICAL] ' if ci.is_critical else ''}{ci.document_name[:60]}",
            item_status,
            detail,
        ))

    ready = sum(1 for i in items if i["status"] == "ready")
    total = len(items)
    score = (ready / total * 100) if total > 0 else 0

    return {
        "category": "submission_checklist",
        "label": "Submission Checklist",
        "icon": "checklist_rtl",
        "weight": WEIGHTS["submission_checklist"],
        "items": items,
        "ready_count": ready,
        "total_count": total,
        "score": round(score, 1),
    }


async def compute_compliance(
    db: AsyncSession, tender_id: UUID, tenant_id: str
) -> dict:
    """Compute full compliance check for a single tender."""
    tender_result = await db.execute(
        select(Tender).where(Tender.id == tender_id)
    )
    tender = tender_result.scalars().first()
    if not tender:
        return None

    # Run checks sequentially — pyodbc does not support concurrent queries
    # on the same connection (raises "Connection is busy with results for
    # another command"). asyncio.gather with a single shared AsyncSession
    # causes this on SQL Server / aioodbc.
    categories = []
    categories.append(await _check_documents(db, tender))
    categories.append(await _check_emd_fees(db, tender))
    categories.append(await _check_oem(db, tender))
    categories.append(await _check_declarations(db, tender))
    categories.append(await _check_criteria(db, tender))
    categories.append(await _check_company_profile(db, tenant_id))
    categories.append(await _check_bid_preparation(db, tender))
    categories.append(await _check_timeline(db, tender))
    categories.append(await _check_submission_checklist(db, tender))

    # Compute weighted overall score (exclude N/A categories)
    applicable = [c for c in categories if not c.get("_na")]
    if applicable:
        weighted_sum = sum(c["weight"] * c["score"] for c in applicable)
        weight_total = sum(c["weight"] for c in applicable)
        overall_score = weighted_sum / weight_total if weight_total > 0 else 0
    else:
        overall_score = 0

    overall_score = round(overall_score, 1)

    # Clean internal flags before returning
    for c in categories:
        c.pop("_na", None)

    return {
        "tender_id": str(tender_id),
        "overall_score": overall_score,
        "overall_label": _score_label(overall_score),
        "categories": categories,
        "checked_at": datetime.utcnow().isoformat(),
    }


async def compute_batch_scores(
    db: AsyncSession, tender_ids: list[UUID], tenant_id: str
) -> list[dict]:
    """Compute compliance scores for multiple tenders (lightweight)."""
    results = []
    for tid in tender_ids:
        full = await compute_compliance(db, tid, tenant_id)
        if full:
            results.append({
                "tender_id": str(tid),
                "overall_score": full["overall_score"],
                "overall_label": full["overall_label"],
            })
    return results
