"""Corrigendum analysis service — AI-powered amendment change detection."""
import json
import logging
from uuid import UUID

from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.compliance_matrix import TenderCorrigendum
from app.models.tender import Tender

logger = logging.getLogger(__name__)

CORRIGENDUM_ANALYSIS_PROMPT = """You are an expert Indian government tender analyst.

Analyze this corrigendum/amendment and extract all changes made to the original tender.

TENDER TITLE: {title}
REFERENCE: {reference_number}

CORRIGENDUM #{corrigendum_number}
TITLE: {corrigendum_title}
SUMMARY: {corrigendum_summary}

ORIGINAL TENDER REQUIREMENTS (for context):
{requirements_json}

Identify every change in the corrigendum. For each change, determine:
1. What field/clause was changed
2. The old value (if determinable)
3. The new value
4. The impact level: minor, moderate, major, critical
5. Which areas are affected: eligibility, technical, financial, timeline, documentation, other

Return ONLY valid JSON:
{{
  "changes": [
    {{
      "field": "deadline" | "emd_amount" | "eligibility" | "technical_spec" | "clause_number" | "other",
      "clause_reference": "e.g., Clause 3.2 or Annexure-A",
      "description": "What changed in plain English",
      "old_value": "Previous value if known, or null",
      "new_value": "New value",
      "impact": "minor | moderate | major | critical",
      "affected_areas": ["eligibility", "technical"],
      "action_required": "What the bidder needs to do in response"
    }}
  ],
  "summary": "Brief summary of all changes",
  "deadline_affected": true | false,
  "eligibility_affected": true | false,
  "requires_rebid": true | false,
  "total_changes": 5
}}"""


async def analyze_corrigendum(
    db: AsyncSession,
    tender_id: UUID,
    corrigendum_id: UUID,
    tenant_id: str,
) -> dict:
    """AI-analyze a corrigendum to extract structured changes.

    Returns extracted changes and updates the corrigendum record.
    """
    # Load tender
    tender_result = await db.execute(select(Tender).where(Tender.id == tender_id))
    tender = tender_result.scalars().first()
    if not tender:
        raise ValueError("Tender not found")

    # Load corrigendum
    corr_result = await db.execute(
        select(TenderCorrigendum).where(
            TenderCorrigendum.id == corrigendum_id,
            TenderCorrigendum.tender_id == tender_id,
        )
    )
    corrigendum = corr_result.scalars().first()
    if not corrigendum:
        raise ValueError("Corrigendum not found")

    requirements_json = json.dumps(
        tender.requirements if tender.requirements else {},
        indent=2, default=str
    )[:20000]

    prompt = CORRIGENDUM_ANALYSIS_PROMPT.format(
        title=tender.title or "N/A",
        reference_number=tender.reference_number or "N/A",
        corrigendum_number=corrigendum.corrigendum_number,
        corrigendum_title=corrigendum.title or "N/A",
        corrigendum_summary=corrigendum.summary or "No summary provided",
        requirements_json=requirements_json,
    )

    from app.services.ai_provider_service import send_message
    response_text, tokens_used, model_used = await send_message(
        db=db,
        tenant_id=tenant_id,
        prompt=prompt,
        feature="corrigendum_analysis",
        max_tokens=4096,
        temperature=0.1,
    )

    parsed = _parse_json_response(response_text)
    changes = parsed.get("changes", [])

    # Update corrigendum with structured changes
    corrigendum.changes = changes
    await db.commit()

    return {
        "corrigendum_id": str(corrigendum_id),
        "corrigendum_number": corrigendum.corrigendum_number,
        "changes": changes,
        "summary": parsed.get("summary", ""),
        "deadline_affected": parsed.get("deadline_affected", False),
        "eligibility_affected": parsed.get("eligibility_affected", False),
        "requires_rebid": parsed.get("requires_rebid", False),
        "total_changes": parsed.get("total_changes", len(changes)),
        "tokens_used": tokens_used,
        "model_used": model_used,
    }


def _parse_json_response(text: str) -> dict:
    text = text.strip()
    if text.startswith("```"):
        lines = text.split("\n")
        lines = [l for l in lines if not l.strip().startswith("```")]
        text = "\n".join(lines)
    return json.loads(text)
