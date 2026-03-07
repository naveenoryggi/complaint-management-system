"""AI Alignment Verification service.

Re-analyzes the ENTIRE tender (compliance, declarations, BoQ, checklist,
pre-bid Q&A, OEM) to ensure 100% alignment before submission.
"""
import json
import logging
from uuid import UUID

from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.tender import Tender
from app.models.submission_checklist import SubmissionChecklistItem
from app.models.oem import OEMTenderRequirement, OEMMaster
from app.models.compliance_matrix import TechnicalComplianceItem
from app.models.tracking import EMDRecord, TenderFee
from app.models.reference_bundle import TenderCriteria

logger = logging.getLogger(__name__)

ALIGNMENT_PROMPT = """You are an expert Indian government tender submission auditor. Perform a COMPREHENSIVE alignment verification of the following tender bid preparation.

TENDER DETAILS:
- Title: {title}
- Reference Number: {reference_number}
- Issuing Authority: {issuing_authority}
- Estimated Value: {estimated_value}
- Deadline: {deadline}

COMPLIANCE MATRIX ({compliance_total} items):
- Complied: {compliance_complied}
- Partially Complied: {compliance_partial}
- Not Complied: {compliance_not_complied}
- Pending: {compliance_pending}
Problem Items:
{compliance_issues}

SUBMISSION CHECKLIST ({checklist_total} items):
- Online Ready: {checklist_online_ready} / {checklist_online_total}
- Offline Ready: {checklist_offline_ready} / {checklist_offline_total}
- Critical Pending: {checklist_critical_pending}
- Notarization Pending: {checklist_notarization_pending}
Pending Critical Items:
{checklist_critical_issues}

OEM REQUIREMENTS ({oem_total} OEMs):
{oem_summary}

EVALUATION CRITERIA ({criteria_total} criteria):
{criteria_summary}

EMD & FEES:
{emd_fees_summary}

PRE-BID QUERIES ({prebid_total} queries):
{prebid_summary}

TENDER REQUIREMENTS (extracted):
{requirements_json}

INSTRUCTIONS:
Analyze ALL the above data holistically and check for:
1. **Completeness**: Are all mandatory documents, declarations, and compliances addressed?
2. **Consistency**: Do all sections align with each other (e.g., BoQ matches OEM, criteria match compliance)?
3. **Contradictions**: Any conflicting information across sections?
4. **Critical Gaps**: Missing mandatory items that would lead to disqualification?
5. **Financial Alignment**: EMD paid? Fees paid? BoQ values consistent?
6. **OEM Alignment**: All OEM certificates (MAF, MS) obtained for required OEMs?
7. **Submission Readiness**: Is the bid package complete for all covers/envelopes?
8. **Pre-Bid Coverage**: Were critical issues from compliance matrix raised in pre-bid queries?

Return ONLY valid JSON (no markdown):
{{
  "alignment_score": 0-100,
  "alignment_label": "Fully Aligned|Mostly Aligned|Needs Attention|Critical Gaps",
  "summary": "2-3 sentence overall assessment",
  "issues": [
    {{
      "area": "compliance|checklist|oem|financial|declarations|prebid|criteria",
      "severity": "critical|warning|info",
      "title": "Brief issue title",
      "description": "What the issue is",
      "recommendation": "What to do about it"
    }}
  ],
  "strengths": ["List of areas that are well-prepared"],
  "risk_areas": ["List of areas with highest risk of disqualification"]
}}

Be thorough but fair. Flag genuine risks, not theoretical ones."""


async def run_alignment_check(
    db: AsyncSession,
    tender_id: UUID,
    tenant_id: str,
) -> dict:
    """Run a comprehensive AI alignment verification on the entire tender.

    Gathers all data, builds a mega-prompt, and returns structured results.

    Returns:
        dict with alignment_score, alignment_label, summary, issues, strengths, risk_areas
    """
    # 1. Fetch tender
    result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(tenant_id),
        )
    )
    tender = result.scalars().first()
    if not tender:
        raise ValueError(f"Tender {tender_id} not found")

    # 2. Compliance matrix
    comp_result = await db.execute(
        select(TechnicalComplianceItem)
        .where(TechnicalComplianceItem.tender_id == tender_id)
    )
    compliance_items = comp_result.scalars().all()

    compliance_complied = sum(1 for c in compliance_items if c.compliance_status == "complied")
    compliance_partial = sum(1 for c in compliance_items if c.compliance_status == "partially_complied")
    compliance_not_complied = sum(1 for c in compliance_items if c.compliance_status == "not_complied")
    compliance_pending = sum(1 for c in compliance_items if c.compliance_status == "pending")

    problem_items = [c for c in compliance_items if c.compliance_status in ("not_complied", "partially_complied", "pending")]
    compliance_issues = "\n".join(
        f"  - [{c.compliance_status}] {c.clause_number}: {c.clause_title} {'(MANDATORY)' if c.is_mandatory else ''} {'(CRITICAL)' if c.is_critical else ''}"
        + (f" — {c.deviation_remarks}" if c.deviation_remarks else "")
        for c in problem_items[:30]
    ) or "  None"

    # 3. Submission checklist
    cl_result = await db.execute(
        select(SubmissionChecklistItem)
        .where(SubmissionChecklistItem.tender_id == tender_id)
    )
    checklist_items = cl_result.scalars().all()

    online_items = [c for c in checklist_items if c.submission_mode in ("online", "both")]
    offline_items = [c for c in checklist_items if c.submission_mode in ("offline", "both")]
    online_ready = sum(1 for c in online_items if c.online_status == "uploaded")
    offline_ready = sum(1 for c in offline_items if c.offline_status == "ready")
    critical_pending = [c for c in checklist_items if c.is_critical and c.online_status != "uploaded"]
    notarization_pending = sum(1 for c in checklist_items if c.notarization_required and c.notarization_status != "completed")

    checklist_critical_issues = "\n".join(
        f"  - {c.document_name} ({c.document_category}) — {c.submission_mode}"
        for c in critical_pending[:20]
    ) or "  None"

    # 4. OEM requirements
    oem_result = await db.execute(
        select(OEMTenderRequirement, OEMMaster)
        .join(OEMMaster, OEMTenderRequirement.oem_id == OEMMaster.id)
        .where(OEMTenderRequirement.tender_id == tender_id)
    )
    oem_rows = oem_result.all()
    oem_summary = "\n".join(
        f"  - {master.name}: MAF={req.maf_status}, MS={req.ms_status}, PartnerCert={req.partner_cert_status}"
        for req, master in oem_rows[:20]
    ) or "  No OEM requirements"

    # 5. Evaluation criteria
    crit_result = await db.execute(
        select(TenderCriteria).where(TenderCriteria.tender_id == tender_id)
    )
    criteria_items = crit_result.scalars().all()
    criteria_summary = "\n".join(
        f"  - {c.criteria_code}: {c.description or ''} (Stage: {c.stage}, Max: {c.max_marks})"
        for c in criteria_items[:20]
    ) or "  No criteria"

    # 6. EMD & fees
    emd_result = await db.execute(
        select(EMDRecord).where(EMDRecord.tender_id == tender_id)
    )
    emd_items = emd_result.scalars().all()
    fee_result = await db.execute(
        select(TenderFee).where(TenderFee.tender_id == tender_id)
    )
    fee_items = fee_result.scalars().all()

    emd_lines = [f"  - EMD: {e.amount} ({e.mode}) Status: {e.status}" for e in emd_items]
    fee_lines = [f"  - Fee: {f.fee_type} = {f.amount} Status: {f.status}" for f in fee_items]
    emd_fees_summary = "\n".join(emd_lines + fee_lines) or "  No EMD/fees recorded"

    # 7. Requirements JSON (truncated)
    requirements_json = json.dumps(
        tender.requirements if tender.requirements else {},
        indent=2,
        default=str,
    )[:30000]

    # Build the prompt
    prompt = ALIGNMENT_PROMPT.format(
        title=tender.title or "N/A",
        reference_number=tender.reference_number or "N/A",
        issuing_authority=tender.issuing_authority or "N/A",
        estimated_value=tender.estimated_value or "N/A",
        deadline=str(tender.deadline) if tender.deadline else "N/A",
        compliance_total=len(compliance_items),
        compliance_complied=compliance_complied,
        compliance_partial=compliance_partial,
        compliance_not_complied=compliance_not_complied,
        compliance_pending=compliance_pending,
        compliance_issues=compliance_issues,
        checklist_total=len(checklist_items),
        checklist_online_ready=online_ready,
        checklist_online_total=len(online_items),
        checklist_offline_ready=offline_ready,
        checklist_offline_total=len(offline_items),
        checklist_critical_pending=len(critical_pending),
        checklist_notarization_pending=notarization_pending,
        checklist_critical_issues=checklist_critical_issues,
        oem_total=len(oem_rows),
        oem_summary=oem_summary,
        criteria_total=len(criteria_items),
        criteria_summary=criteria_summary,
        emd_fees_summary=emd_fees_summary,
        prebid_total=0,
        prebid_summary="  Pre-bid queries are generated client-side (not persisted).",
        requirements_json=requirements_json,
    )

    # Call AI
    from app.services.ai_provider_service import send_message

    response_text, tokens_used, model_used = await send_message(
        db=db,
        tenant_id=tenant_id,
        prompt=prompt,
        feature="alignment",
        max_tokens=4096,
        temperature=0,
    )

    # Parse response
    response_text = response_text.strip()
    if response_text.startswith("```"):
        lines = response_text.split("\n")
        json_lines = []
        in_block = False
        for line in lines:
            if line.strip().startswith("```"):
                in_block = not in_block
                continue
            if in_block:
                json_lines.append(line)
        response_text = "\n".join(json_lines)

    try:
        result_data = json.loads(response_text)
    except json.JSONDecodeError:
        result_data = {
            "alignment_score": 0,
            "alignment_label": "Error",
            "summary": "Failed to parse AI response.",
            "issues": [],
            "strengths": [],
            "risk_areas": [],
        }

    result_data["tender_id"] = str(tender_id)
    result_data["tokens_used"] = tokens_used
    result_data["model_used"] = model_used

    return result_data
