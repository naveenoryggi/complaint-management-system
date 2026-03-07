"""Company Document Library service — CRUD, auto-linking to checklists, expiry tracking."""
from datetime import date, timedelta
from typing import Optional
from uuid import UUID

from sqlalchemy import select, func, and_, or_
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.document import Document
from app.models.submission_checklist import SubmissionChecklistItem

# Maps company_doc_category → keywords that match checklist document_name
CATEGORY_TO_CHECKLIST_KEYWORDS: dict[str, list[str]] = {
    "pan_card": ["pan card", "pan copy", "permanent account number"],
    "gst_certificate": ["gst certificate", "gst registration", "gstin"],
    "company_registration": [
        "company registration", "certificate of incorporation",
        "incorporation certificate", "mca certificate",
    ],
    "msme_certificate": ["msme certificate", "msme registration", "udyam", "udyog aadhar"],
    "solvency_certificate": ["solvency certificate", "solvency"],
    "turnover_certificate": [
        "turnover certificate", "annual turnover", "turnover statement",
        "ca certificate", "chartered accountant certificate",
    ],
    "iso_certificate": ["iso certificate", "iso certification", "iso 9001", "iso 14001", "iso 27001"],
    "audited_balance_sheet": [
        "audited balance sheet", "balance sheet", "audited financial",
        "financial statement", "profit and loss",
    ],
    "power_of_attorney": ["power of attorney", "poa", "authorisation letter", "authorization letter"],
    "board_resolution": ["board resolution", "resolution"],
    "epf_certificate": ["epf certificate", "epf registration", "provident fund"],
    "esi_certificate": ["esi certificate", "esi registration", "employee state insurance"],
}

CATEGORY_LABELS: dict[str, str] = {
    "pan_card": "PAN Card",
    "gst_certificate": "GST Certificate",
    "company_registration": "Company Registration",
    "msme_certificate": "MSME Certificate",
    "solvency_certificate": "Solvency Certificate",
    "turnover_certificate": "Turnover Certificate",
    "iso_certificate": "ISO Certificate",
    "audited_balance_sheet": "Audited Balance Sheet",
    "power_of_attorney": "Power of Attorney",
    "board_resolution": "Board Resolution",
    "epf_certificate": "EPF Certificate",
    "esi_certificate": "ESI Certificate",
    "other": "Other",
}


async def list_company_documents(
    db: AsyncSession,
    tenant_id: UUID,
    category: Optional[str] = None,
) -> list[dict]:
    """List all company documents for a tenant, optionally filtered by category."""
    query = (
        select(Document)
        .where(
            Document.tenant_id == tenant_id,
            Document.is_company_document == True,
        )
        .order_by(Document.company_doc_category, Document.name)
    )

    if category:
        query = query.where(Document.company_doc_category == category)

    result = await db.execute(query)
    docs = result.scalars().all()

    return [_doc_to_dict(d) for d in docs]


async def get_category_summary(db: AsyncSession, tenant_id: UUID) -> list[dict]:
    """Return each category with counts and expiry status."""
    result = await db.execute(
        select(Document)
        .where(
            Document.tenant_id == tenant_id,
            Document.is_company_document == True,
        )
    )
    docs = result.scalars().all()

    today = date.today()
    threshold = today + timedelta(days=30)

    # Build per-category summary
    cat_map: dict[str, dict] = {}
    for cat_key in CATEGORY_LABELS:
        cat_map[cat_key] = {
            "category": cat_key,
            "label": CATEGORY_LABELS[cat_key],
            "count": 0,
            "has_expiring": False,
            "has_expired": False,
            "earliest_expiry": None,
        }

    for doc in docs:
        cat = doc.company_doc_category or "other"
        if cat not in cat_map:
            cat_map[cat] = {
                "category": cat,
                "label": CATEGORY_LABELS.get(cat, cat),
                "count": 0,
                "has_expiring": False,
                "has_expired": False,
                "earliest_expiry": None,
            }
        cat_map[cat]["count"] += 1
        if doc.expiry_date:
            if doc.expiry_date < today:
                cat_map[cat]["has_expired"] = True
            elif doc.expiry_date <= threshold:
                cat_map[cat]["has_expiring"] = True
            exp_str = doc.expiry_date.isoformat()
            if cat_map[cat]["earliest_expiry"] is None or exp_str < cat_map[cat]["earliest_expiry"]:
                cat_map[cat]["earliest_expiry"] = exp_str

    return list(cat_map.values())


async def get_expiring_documents(
    db: AsyncSession, tenant_id: UUID, days: int = 30
) -> list[dict]:
    """Return company docs expiring within N days."""
    threshold = date.today() + timedelta(days=days)
    result = await db.execute(
        select(Document)
        .where(
            Document.tenant_id == tenant_id,
            Document.is_company_document == True,
            Document.expiry_date != None,
            Document.expiry_date <= threshold,
        )
        .order_by(Document.expiry_date)
    )
    return [_doc_to_dict(d) for d in result.scalars().all()]


async def auto_link_to_checklist(
    db: AsyncSession,
    tender_id: UUID,
    tenant_id: UUID,
) -> dict:
    """Auto-link company docs to unlinked checklist items via keyword matching.

    Returns {"linked": int, "details": [...]}.
    """
    # Get all company docs for this tenant
    doc_result = await db.execute(
        select(Document).where(
            Document.tenant_id == tenant_id,
            Document.is_company_document == True,
        )
    )
    company_docs = doc_result.scalars().all()

    if not company_docs:
        return {"linked": 0, "details": []}

    # Build keyword→doc mapping
    keyword_map: list[tuple[list[str], Document]] = []
    for doc in company_docs:
        cat = doc.company_doc_category
        if cat and cat in CATEGORY_TO_CHECKLIST_KEYWORDS:
            keyword_map.append((CATEGORY_TO_CHECKLIST_KEYWORDS[cat], doc))

    # Get unlinked checklist items for this tender
    item_result = await db.execute(
        select(SubmissionChecklistItem).where(
            SubmissionChecklistItem.tender_id == tender_id,
            or_(
                SubmissionChecklistItem.linked_document_id == None,
                SubmissionChecklistItem.linked_document_id == "",
            ),
        )
    )
    items = item_result.scalars().all()

    linked_count = 0
    details = []

    for item in items:
        item_name_lower = (item.document_name or "").lower()
        for keywords, doc in keyword_map:
            if any(kw in item_name_lower for kw in keywords):
                item.linked_document_id = str(doc.id)
                linked_count += 1
                details.append({
                    "checklist_item": item.document_name,
                    "linked_document": doc.name,
                    "category": doc.company_doc_category,
                })
                break  # Link first match only

    if linked_count > 0:
        await db.flush()

    return {"linked": linked_count, "details": details}


def _doc_to_dict(doc: Document) -> dict:
    """Convert Document model to dict for API response."""
    return {
        "id": str(doc.id),
        "name": doc.name,
        "description": doc.description,
        "file_path": doc.file_path,
        "file_size": doc.file_size,
        "mime_type": doc.mime_type,
        "document_type": doc.document_type,
        "company_doc_category": doc.company_doc_category,
        "expiry_date": doc.expiry_date.isoformat() if doc.expiry_date else None,
        "is_company_document": doc.is_company_document,
        "tags": doc.tags,
        "version": doc.version,
        "created_at": doc.created_at.isoformat() if doc.created_at else None,
        "updated_at": doc.updated_at.isoformat() if doc.updated_at else None,
    }
