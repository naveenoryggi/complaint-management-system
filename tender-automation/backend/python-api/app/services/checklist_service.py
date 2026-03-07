"""Submission checklist service — auto-generation from templates, AI analysis, and summary computation."""
import json
import logging
from uuid import UUID

from sqlalchemy import select, func
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.submission_checklist import SubmissionChecklistItem
from app.services.checklist_templates import get_all_templates

logger = logging.getLogger(__name__)

# Status values considered "ready" for each mode
ONLINE_READY = {"uploaded", "digitally_signed", "verified"}
OFFLINE_READY = {"dispatched", "received", "verified"}
NOTARIZATION_READY = {"completed", "verified", "not_required"}


async def auto_generate_checklist(
    db: AsyncSession,
    tender_id: UUID,
    include_offline: bool = True,
) -> int:
    """Idempotently populate checklist from standard templates.

    Only creates items that don't already exist (matched by document_name + tender_id).
    Returns number of items created.
    """
    # Get existing item names for this tender
    result = await db.execute(
        select(SubmissionChecklistItem.document_name)
        .where(SubmissionChecklistItem.tender_id == tender_id)
    )
    existing_names = {row[0] for row in result.all()}
    existing_names_lower = {n.lower() for n in existing_names}

    templates = get_all_templates()
    created = 0

    for tpl in templates:
        # Skip offline-only items if not requested
        if not include_offline and tpl["submission_mode"] == "offline":
            continue

        # Skip if already exists (exact match)
        if tpl["document_name"] in existing_names:
            continue

        # Skip if a similar item already exists (fuzzy match against AI-extracted items)
        tpl_lower = tpl["document_name"].lower()
        tpl_core = tpl_lower.split("(")[0].strip()
        tpl_words = set(w for w in tpl_lower.split() if len(w) > 3)
        is_covered = False
        for existing_lower in existing_names_lower:
            existing_core = existing_lower.split("(")[0].strip()
            # Substring containment
            if len(tpl_core) > 5 and len(existing_core) > 5:
                if tpl_core in existing_core or existing_core in tpl_core:
                    is_covered = True
                    break
            # Fuzzy word overlap
            existing_words = set(w for w in existing_lower.split() if len(w) > 3)
            if tpl_words and existing_words:
                overlap = len(tpl_words & existing_words)
                min_len = min(len(tpl_words), len(existing_words))
                if min_len > 0 and overlap / min_len >= 0.6:
                    is_covered = True
                    break
        if is_covered:
            continue

        notarization_status = "pending" if tpl.get("notarization_required") else "not_required"

        item = SubmissionChecklistItem(
            tender_id=tender_id,
            document_name=tpl["document_name"],
            document_category=tpl["document_category"],
            submission_mode=tpl["submission_mode"],
            is_critical=tpl["is_critical"],
            envelope=tpl.get("envelope"),
            cover_name=tpl.get("cover_name"),
            notarization_required=tpl.get("notarization_required", False),
            notarization_type=tpl.get("notarization_type"),
            notarization_status=notarization_status,
            document_origin=tpl.get("document_origin", "self_generated"),
            format_reference=tpl.get("format_reference"),
            obtaining_source=tpl.get("obtaining_source"),
            can_auto_generate=tpl.get("can_auto_generate", False),
            sort_order=tpl["sort_order"],
            source="template",
        )
        db.add(item)
        created += 1

    if created > 0:
        await db.flush()

    # Auto-link company documents to newly created checklist items
    try:
        from app.services.company_document_service import auto_link_to_checklist
        # We need tenant_id — derive from the first checklist item or query the tender
        from app.models.tender import Tender
        tender_result = await db.execute(
            select(Tender.tenant_id).where(Tender.id == tender_id)
        )
        tenant_row = tender_result.first()
        if tenant_row:
            await auto_link_to_checklist(db, tender_id, tenant_row[0])
            await db.flush()
    except Exception:
        pass  # Non-critical — don't block checklist generation

    return created


DEFAULT_DEDUP_THRESHOLD = 0.6


def _find_duplicate_pairs(
    items: list[SubmissionChecklistItem],
    threshold: float = DEFAULT_DEDUP_THRESHOLD,
) -> tuple[list[SubmissionChecklistItem], list[SubmissionChecklistItem], list[dict]]:
    """Identify duplicate items at a given similarity threshold.

    Returns (kept_items, duplicate_items, duplicate_details).
    duplicate_details contains {item_id, item_name, matched_with, match_type, similarity}.
    """
    kept_items: list[SubmissionChecklistItem] = []
    to_delete: list[SubmissionChecklistItem] = []
    details: list[dict] = []

    for item in items:
        item_lower = item.document_name.lower().strip()
        item_core = item_lower.split("(")[0].strip()
        item_words = set(w for w in item_lower.split() if len(w) > 3)

        match_info: dict | None = None
        for kept in kept_items:
            kept_lower = kept.document_name.lower().strip()
            kept_core = kept_lower.split("(")[0].strip()

            # Exact match (always a dup regardless of threshold)
            if item_lower == kept_lower:
                match_info = {
                    "matched_with": kept.document_name,
                    "matched_with_id": str(kept.id),
                    "match_type": "exact",
                    "similarity": 1.0,
                }
                break

            # Core name match (ignoring parenthetical)
            if item_core and kept_core and item_core == kept_core:
                match_info = {
                    "matched_with": kept.document_name,
                    "matched_with_id": str(kept.id),
                    "match_type": "core_name",
                    "similarity": 1.0,
                }
                break

            # Substring containment
            if len(item_core) > 5 and len(kept_core) > 5:
                if item_core in kept_core or kept_core in item_core:
                    longer = max(len(item_core), len(kept_core))
                    shorter = min(len(item_core), len(kept_core))
                    sim = shorter / longer if longer > 0 else 0
                    if sim >= threshold:
                        match_info = {
                            "matched_with": kept.document_name,
                            "matched_with_id": str(kept.id),
                            "match_type": "substring",
                            "similarity": round(sim, 2),
                        }
                        break

            # Fuzzy word overlap
            kept_words = set(w for w in kept_lower.split() if len(w) > 3)
            if item_words and kept_words:
                overlap = len(item_words & kept_words)
                min_len = min(len(item_words), len(kept_words))
                sim = overlap / min_len if min_len > 0 else 0
                if sim >= threshold:
                    match_info = {
                        "matched_with": kept.document_name,
                        "matched_with_id": str(kept.id),
                        "match_type": "word_overlap",
                        "similarity": round(sim, 2),
                    }
                    break

        if match_info:
            to_delete.append(item)
            details.append({
                "item_id": str(item.id),
                "item_name": item.document_name,
                **match_info,
            })
        else:
            kept_items.append(item)

    return kept_items, to_delete, details


async def find_duplicates(
    db: AsyncSession,
    tender_id: UUID,
    threshold: float = DEFAULT_DEDUP_THRESHOLD,
) -> list[dict]:
    """Preview duplicates at a given threshold without deleting.

    Returns list of duplicate details with match info.
    """
    result = await db.execute(
        select(SubmissionChecklistItem)
        .where(SubmissionChecklistItem.tender_id == tender_id)
        .order_by(SubmissionChecklistItem.sort_order)
    )
    items = result.scalars().all()

    if len(items) <= 1:
        return []

    _, _, details = _find_duplicate_pairs(items, threshold)
    return details


async def deduplicate_checklist(
    db: AsyncSession,
    tender_id: UUID,
    threshold: float = DEFAULT_DEDUP_THRESHOLD,
) -> int:
    """Remove duplicate/near-duplicate checklist items for a tender.

    Keeps the first occurrence (by sort_order) and removes later duplicates.
    Returns number of items removed.
    """
    result = await db.execute(
        select(SubmissionChecklistItem)
        .where(SubmissionChecklistItem.tender_id == tender_id)
        .order_by(SubmissionChecklistItem.sort_order)
    )
    items = result.scalars().all()

    if len(items) <= 1:
        return 0

    _, to_delete, _ = _find_duplicate_pairs(items, threshold)

    for item in to_delete:
        await db.delete(item)

    if to_delete:
        await db.flush()

    return len(to_delete)


async def compute_checklist_summary(db: AsyncSession, tender_id: UUID) -> dict:
    """Compute dual-mode readiness summary for a tender's submission checklist."""
    result = await db.execute(
        select(SubmissionChecklistItem)
        .where(SubmissionChecklistItem.tender_id == tender_id)
        .order_by(SubmissionChecklistItem.sort_order)
    )
    items = result.scalars().all()

    if not items:
        return {
            "total": 0,
            "online": {"total": 0, "ready": 0, "percentage": 0},
            "offline": {"total": 0, "ready": 0, "percentage": 0},
            "critical_pending": 0,
            "notarization_pending": 0,
            "by_category": {},
            "by_envelope": {},
            "by_cover": {},
        }

    # Online items = items with mode "online" or "both"
    online_items = [i for i in items if i.submission_mode in ("online", "both")]
    offline_items = [i for i in items if i.submission_mode in ("offline", "both")]

    online_ready = sum(1 for i in online_items if i.online_status in ONLINE_READY)
    offline_ready = sum(1 for i in offline_items if i.offline_status in OFFLINE_READY)

    online_total = len(online_items)
    offline_total = len(offline_items)

    # Critical pending: critical items that are NOT ready in their required mode(s)
    critical_pending = 0
    for i in items:
        if not i.is_critical:
            continue
        if i.submission_mode in ("online", "both") and i.online_status not in ONLINE_READY:
            critical_pending += 1
            continue
        if i.submission_mode in ("offline", "both") and i.offline_status not in OFFLINE_READY:
            critical_pending += 1

    # Notarization pending
    notarization_pending = sum(
        1 for i in items
        if i.notarization_required and i.notarization_status not in NOTARIZATION_READY
    )

    # By category breakdown
    by_category = {}
    for i in items:
        cat = i.document_category or "other"
        if cat not in by_category:
            by_category[cat] = {"total": 0, "online_ready": 0, "offline_ready": 0, "notarization_pending": 0}
        by_category[cat]["total"] += 1
        if i.submission_mode in ("online", "both") and i.online_status in ONLINE_READY:
            by_category[cat]["online_ready"] += 1
        if i.submission_mode in ("offline", "both") and i.offline_status in OFFLINE_READY:
            by_category[cat]["offline_ready"] += 1
        if i.notarization_required and i.notarization_status not in NOTARIZATION_READY:
            by_category[cat]["notarization_pending"] += 1

    # By envelope breakdown
    by_envelope = {}
    for i in items:
        env = i.envelope or "unassigned"
        if env not in by_envelope:
            by_envelope[env] = {"total": 0, "ready": 0}
        by_envelope[env]["total"] += 1
        # Ready = all applicable modes ready
        is_ready = True
        if i.submission_mode in ("online", "both") and i.online_status not in ONLINE_READY:
            is_ready = False
        if i.submission_mode in ("offline", "both") and i.offline_status not in OFFLINE_READY:
            is_ready = False
        if is_ready:
            by_envelope[env]["ready"] += 1

    # By cover breakdown (for document packaging view)
    by_cover = {}
    for i in items:
        cover = i.cover_name or i.envelope or "Unassigned"
        if cover not in by_cover:
            by_cover[cover] = {"cover_name": cover, "envelope": i.envelope, "total": 0, "ready": 0, "critical": 0, "documents": []}
        by_cover[cover]["total"] += 1
        if i.is_critical:
            by_cover[cover]["critical"] += 1
        is_ready = True
        if i.submission_mode in ("online", "both") and i.online_status not in ONLINE_READY:
            is_ready = False
        if i.submission_mode in ("offline", "both") and i.offline_status not in OFFLINE_READY:
            is_ready = False
        if is_ready:
            by_cover[cover]["ready"] += 1
        by_cover[cover]["documents"].append({
            "id": str(i.id),
            "document_name": i.document_name,
            "is_critical": i.is_critical,
            "online_status": i.online_status if i.submission_mode in ("online", "both") else None,
            "offline_status": i.offline_status if i.submission_mode in ("offline", "both") else None,
            "notarization_required": i.notarization_required,
            "notarization_type": i.notarization_type,
            "notarization_status": i.notarization_status,
            "document_origin": i.document_origin,
            "can_auto_generate": i.can_auto_generate,
        })

    return {
        "total": len(items),
        "online": {
            "total": online_total,
            "ready": online_ready,
            "percentage": round(online_ready / online_total * 100, 1) if online_total > 0 else 0,
        },
        "offline": {
            "total": offline_total,
            "ready": offline_ready,
            "percentage": round(offline_ready / offline_total * 100, 1) if offline_total > 0 else 0,
        },
        "critical_pending": critical_pending,
        "notarization_pending": notarization_pending,
        "by_category": by_category,
        "by_envelope": by_envelope,
        "by_cover": list(by_cover.values()),
    }


# ---------------------------------------------------------------------------
# AI Checklist Analysis
# ---------------------------------------------------------------------------

CHECKLIST_ANALYSIS_PROMPT = """You are an expert Indian government tender analyst. Analyze the tender requirements below and determine which submission checklist documents are ACTUALLY REQUIRED for this specific tender.

TENDER DETAILS:
Title: {tender_title}
Reference: {tender_reference}
Issuing Authority: {issuing_authority}

EXTRACTED REQUIREMENTS:
{requirements_text}

ELIGIBILITY CRITERIA:
{eligibility_text}

EVALUATION / MARKING CRITERIA:
{evaluation_text}

EMD / TENDER FEE DETAILS:
{emd_fee_text}

CURRENT CHECKLIST ITEMS:
{checklist_items_text}

INSTRUCTIONS:
For each checklist item, determine:
1. "relevant" — Is this document actually needed for THIS specific tender? (true/false)
2. "is_required" — If relevant, is it mandatory or optional for this tender? (true/false)
3. "reason" — Brief reason (e.g., "Tender clause 5.2 requires EMD of Rs. 5 lakhs", "Not applicable — tender is for services, not goods", "MSME exemption not mentioned in this tender")

EMD/Fee Rules:
- If EMD amount is specified and > 0, EMD-related documents (BG, DD, FDR) are REQUIRED
- If EMD is exempted or amount is 0/nil, EMD documents are IRRELEVANT
- If MSME exemption is mentioned for EMD, MSME-related exemption documents become REQUIRED for MSME bidders
- If tender fee / document cost is specified, tender fee receipt is REQUIRED
- If tender fee is exempted or not mentioned, tender fee receipt is IRRELEVANT

Eligibility/Evaluation Criteria Rules:
- If eligibility criteria lists specific documents (e.g., "3 years balance sheet", "ISO certificate", "turnover certificate"), those documents are REQUIRED
- If evaluation/marking criteria assigns marks to specific documents (e.g., "Experience certificates: 20 marks"), those are REQUIRED
- If a document carries marks in evaluation, it is REQUIRED even if not listed in eligibility
- If minimum turnover/experience is specified, corresponding financial/experience certificates are REQUIRED
- Documents mentioned in eligibility as "mandatory" or "must submit" are REQUIRED
- Documents mentioned as "if applicable" or "where relevant" are OPTIONAL

General Rules:
- If the tender is on GeM portal, GeM-specific documents are relevant
- If the tender does NOT mention EMD or exempts it, EMD documents are irrelevant
- If there's no mention of MSME benefits, MSME exemption declarations are irrelevant
- If it's a services tender, product datasheets/OEM authorization may be irrelevant
- If it's online-only (no physical submission), offline documents are irrelevant
- If there's no mention of notarization/affidavit, notarized documents may be optional
- If the tender doesn't mention integrity pact, that item is irrelevant
- Check if specific certificates (ISO, EPF, ESI) are actually asked for
- Cover Letter, Technical Compliance Matrix, Financial Bid/BoQ are almost always REQUIRED
- Company Registration, PAN Card, GST Certificate are REQUIRED for all government tenders

Return ONLY valid JSON (no markdown, no code blocks):
{{
  "analysis": [
    {{
      "item_id": "uuid-of-item",
      "document_name": "name for reference",
      "relevant": true,
      "is_required": true,
      "reason": "Brief reason"
    }}
  ],
  "summary": "Brief summary of what this tender actually requires including EMD and fee status"
}}

Analyze ALL {num_items} checklist items."""


async def analyze_checklist_requirements(
    db: AsyncSession,
    tenant_id: str,
    tender: dict,
    tender_requirements: dict | None,
    checklist_items: list[dict],
) -> dict:
    """
    Use AI to analyze which checklist items are relevant for this specific tender.

    Returns dict with 'analysis' list and 'summary'.
    Also persists is_critical updates to the database.
    """
    # Build requirements text (general requirements excluding eligibility/evaluation which get their own sections)
    req_parts = []
    eligibility_parts = []
    evaluation_parts = []

    if tender_requirements:
        for key in ["technical_requirements", "special_conditions", "document_checklist"]:
            data = tender_requirements.get(key)
            if not data:
                continue
            req_parts.append(f"\n{key.upper().replace('_', ' ')}:")
            if isinstance(data, list):
                for item in data:
                    if isinstance(item, str):
                        req_parts.append(f"  - {item}")
                    elif isinstance(item, dict):
                        req_parts.append(f"  - {item.get('description', str(item))}")
            elif isinstance(data, str):
                req_parts.append(f"  {data}")

        # Eligibility criteria — dedicated section
        elig_data = tender_requirements.get("eligibility_criteria")
        if elig_data:
            if isinstance(elig_data, list):
                for item in elig_data:
                    if isinstance(item, str):
                        eligibility_parts.append(f"  - {item}")
                    elif isinstance(item, dict):
                        desc = item.get("description", item.get("criteria", str(item)))
                        mandatory = item.get("mandatory", item.get("is_mandatory", ""))
                        if mandatory:
                            eligibility_parts.append(f"  - [MANDATORY] {desc}")
                        else:
                            eligibility_parts.append(f"  - {desc}")
            elif isinstance(elig_data, str):
                eligibility_parts.append(f"  {elig_data}")

        # Evaluation/marking criteria — dedicated section
        eval_data = tender_requirements.get("evaluation_criteria") or tender_requirements.get("marking_criteria")
        if eval_data:
            if isinstance(eval_data, list):
                for item in eval_data:
                    if isinstance(item, dict):
                        desc = item.get("description", item.get("criteria", ""))
                        marks = item.get("max_marks", item.get("marks", item.get("weightage", "")))
                        if marks:
                            evaluation_parts.append(f"  - {desc} (Marks/Weight: {marks})")
                        else:
                            evaluation_parts.append(f"  - {desc}")
                    elif isinstance(item, str):
                        evaluation_parts.append(f"  - {item}")
            elif isinstance(eval_data, str):
                evaluation_parts.append(f"  {eval_data}")

        # Also check for scoring_criteria, qualification_criteria
        for alt_key in ["scoring_criteria", "qualification_criteria", "pre_qualification"]:
            alt_data = tender_requirements.get(alt_key)
            if alt_data and isinstance(alt_data, list):
                evaluation_parts.append(f"\n  {alt_key.upper().replace('_', ' ')}:")
                for item in alt_data:
                    if isinstance(item, str):
                        evaluation_parts.append(f"  - {item}")
                    elif isinstance(item, dict):
                        evaluation_parts.append(f"  - {item.get('description', str(item))}")

    requirements_text = "\n".join(req_parts) if req_parts else "No specific requirements extracted from tender document."
    eligibility_text = "\n".join(eligibility_parts) if eligibility_parts else "No eligibility criteria extracted."
    evaluation_text = "\n".join(evaluation_parts) if evaluation_parts else "No evaluation/marking criteria extracted."

    # Build EMD / tender fee text separately for prominence
    emd_fee_parts = []
    if tender_requirements:
        emd = tender_requirements.get("emd")
        if emd and isinstance(emd, dict):
            emd_fee_parts.append(f"EMD Amount: {emd.get('amount', 'Not specified')}")
            emd_fee_parts.append(f"EMD Mode: {emd.get('mode', 'Not specified')}")
            emd_fee_parts.append(f"MSME Exempted: {emd.get('msme_exempted', 'Not specified')}")
            if emd.get('bank_name'):
                emd_fee_parts.append(f"Bank: {emd.get('bank_name')}")
            if emd.get('account_number'):
                emd_fee_parts.append(f"Account: {emd.get('account_number')}")
        else:
            emd_fee_parts.append("EMD: Not mentioned in tender (likely exempted or NIL)")

        tender_fees = tender_requirements.get("tender_fees") or tender_requirements.get("tender_fee")
        if tender_fees and isinstance(tender_fees, dict):
            emd_fee_parts.append(f"Tender Fee: {tender_fees.get('amount', 'Not specified')}")
            emd_fee_parts.append(f"Fee Mode: {tender_fees.get('mode', 'Not specified')}")
            if tender_fees.get('exempted'):
                emd_fee_parts.append("Tender Fee Exempted: Yes")
        elif tender_fees and isinstance(tender_fees, str):
            emd_fee_parts.append(f"Tender Fee: {tender_fees}")
        else:
            emd_fee_parts.append("Tender Fee: Not mentioned in tender")

        # Check for document cost
        doc_cost = tender_requirements.get("document_cost") or tender_requirements.get("tender_document_cost")
        if doc_cost:
            emd_fee_parts.append(f"Document Cost: {doc_cost}")
    else:
        emd_fee_parts.append("No EMD or tender fee information extracted from tender document.")

    emd_fee_text = "\n".join(emd_fee_parts)

    # Build checklist items text
    items_text = "\n".join(
        f"  - [{item['id']}] {item['document_name']} (category: {item.get('document_category', 'other')}, "
        f"mode: {item.get('submission_mode', 'online')}, critical: {item.get('is_critical', False)}, "
        f"source: {item.get('source', 'unknown')})"
        for item in checklist_items
    )

    prompt = CHECKLIST_ANALYSIS_PROMPT.format(
        tender_title=tender.get("title", "N/A"),
        tender_reference=tender.get("reference_number", "N/A"),
        issuing_authority=tender.get("issuing_authority", "N/A"),
        requirements_text=requirements_text,
        eligibility_text=eligibility_text,
        evaluation_text=evaluation_text,
        emd_fee_text=emd_fee_text,
        checklist_items_text=items_text,
        num_items=len(checklist_items),
    )

    try:
        from app.services.ai_provider_service import send_message

        response_text, tokens_used, model_used = await send_message(
            db=db,
            tenant_id=tenant_id,
            prompt=prompt,
            feature="checklist_analysis",
            max_tokens=4096,
            temperature=0,
        )

        response_text = response_text.strip()
        if response_text.startswith("```"):
            lines = response_text.split("\n")
            lines = [l for l in lines if not l.startswith("```")]
            response_text = "\n".join(lines)

        result = json.loads(response_text)
        analysis = result.get("analysis", [])

        # Persist is_critical updates to DB
        updated_count = await _persist_analysis_to_db(db, analysis)
        logger.info(f"Checklist analysis: updated is_critical on {updated_count} items")

        return {
            "analysis": analysis,
            "summary": result.get("summary", ""),
            "updated_count": updated_count,
        }

    except Exception as e:
        logger.error(f"Checklist analysis failed: {e}")
        # Fallback: mark all as relevant
        return {
            "analysis": [
                {
                    "item_id": item["id"],
                    "document_name": item["document_name"],
                    "relevant": True,
                    "is_required": item.get("is_critical", False),
                    "reason": "Analysis unavailable — defaulting to relevant",
                }
                for item in checklist_items
            ],
            "summary": "AI analysis failed. All items shown as relevant.",
        }


async def _persist_analysis_to_db(
    db: AsyncSession,
    analysis: list[dict],
) -> int:
    """Update is_critical on checklist items based on AI analysis results.

    Sets is_critical=True for items marked is_required=True by AI,
    and is_critical=False for items marked is_required=False.
    Returns number of items updated.
    """
    updated = 0
    for item_analysis in analysis:
        item_id = item_analysis.get("item_id")
        is_required = item_analysis.get("is_required", False)
        if not item_id:
            continue
        try:
            result = await db.execute(
                select(SubmissionChecklistItem).where(
                    SubmissionChecklistItem.id == UUID(item_id)
                )
            )
            item = result.scalars().first()
            if item and item.is_critical != is_required:
                item.is_critical = is_required
                updated += 1
        except Exception:
            continue

    if updated > 0:
        await db.flush()
    return updated


async def remove_irrelevant_items(
    db: AsyncSession,
    tender_id: UUID,
    item_ids: list[str],
) -> int:
    """Remove specific checklist items by their IDs. Returns count removed."""
    removed = 0
    for item_id in item_ids:
        result = await db.execute(
            select(SubmissionChecklistItem).where(
                SubmissionChecklistItem.id == UUID(item_id),
                SubmissionChecklistItem.tender_id == tender_id,
            )
        )
        item = result.scalars().first()
        if item:
            await db.delete(item)
            removed += 1

    if removed > 0:
        await db.flush()
    return removed
