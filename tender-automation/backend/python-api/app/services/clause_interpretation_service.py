"""Clause interpretation service — AI-powered risk scoring and cross-reference detection."""
import json
import logging
from uuid import UUID

from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.compliance_matrix import TechnicalComplianceItem
from app.models.tender import Tender

logger = logging.getLogger(__name__)

INTERPRETATION_PROMPT = """You are an expert Indian government tender analyst specializing in clause-by-clause risk assessment.

Analyze these tender clauses and for each one provide:
1. A plain-English interpretation (what it actually means for the bidder)
2. A risk score (0-10, where 0=no risk, 10=critical risk to bid)
3. A risk category: low (0-3), medium (4-6), high (7-8), critical (9-10)
4. The reason for the risk assessment
5. Any cross-references to other clauses (dependencies, contradictions, related clauses)

TENDER: {title}
REFERENCE: {reference_number}

CLAUSES TO ANALYZE:
{clauses_json}

Return ONLY valid JSON:
{{
  "interpretations": [
    {{
      "clause_id": "the clause id from input",
      "interpretation": "Plain-English explanation of what this clause means for the bidder",
      "risk_score": 7.5,
      "risk_category": "high",
      "risk_reason": "Why this is risky — e.g., tight timeline, onerous penalty, restrictive eligibility",
      "cross_references": [
        {{
          "clause": "Referenced clause number",
          "relationship": "depends_on | contradicts | related_to | superseded_by | modifies"
        }}
      ]
    }}
  ],
  "overall_risk_summary": "Brief summary of highest-risk areas",
  "red_flags": ["List of clauses that are dealbreakers or need immediate attention"]
}}

Risk scoring guidelines:
- 0-2: Standard clause, minimal risk (e.g., standard definitions, routine submission requirements)
- 3-4: Minor risk (e.g., specific format requirements, standard penalties)
- 5-6: Moderate risk (e.g., tight timelines, performance guarantees, liquidated damages)
- 7-8: High risk (e.g., unlimited liability, onerous SLA penalties, restrictive eligibility)
- 9-10: Critical risk (e.g., blacklisting clause, loss of EMD conditions, sole-source lock-in)"""


async def interpret_clauses(
    db: AsyncSession,
    tender_id: UUID,
    tenant_id: str,
    clause_ids: list[str] | None = None,
) -> dict:
    """AI-interpret tender clauses with risk scoring and cross-reference detection.

    Args:
        db: Database session
        tender_id: Tender UUID
        tenant_id: Tenant UUID string
        clause_ids: Optional list of specific clause IDs to interpret (all if None)

    Returns:
        Dict with interpretations, overall_risk_summary, red_flags, and update counts.
    """
    # Load tender
    tender_result = await db.execute(
        select(Tender).where(Tender.id == tender_id)
    )
    tender = tender_result.scalars().first()
    if not tender:
        raise ValueError("Tender not found")

    # Load compliance items
    query = select(TechnicalComplianceItem).where(
        TechnicalComplianceItem.tender_id == tender_id
    ).order_by(TechnicalComplianceItem.sort_order)

    if clause_ids:
        query = query.where(TechnicalComplianceItem.id.in_([UUID(c) for c in clause_ids]))

    result = await db.execute(query)
    items = result.scalars().all()

    if not items:
        return {
            "interpreted": 0,
            "message": "No compliance items found to interpret",
            "interpretations": [],
        }

    # Build clauses JSON for prompt
    clauses_data = []
    for item in items:
        clauses_data.append({
            "clause_id": str(item.id),
            "clause_number": item.clause_number,
            "clause_title": item.clause_title,
            "clause_description": item.clause_description or "",
            "section": item.section or "",
            "is_mandatory": item.is_mandatory,
            "is_critical": item.is_critical,
        })

    prompt = INTERPRETATION_PROMPT.format(
        title=tender.title or "N/A",
        reference_number=tender.reference_number or "N/A",
        clauses_json=json.dumps(clauses_data, indent=2)[:30000],
    )

    # Call AI
    from app.services.ai_provider_service import send_message
    response_text, tokens_used, model_used = await send_message(
        db=db,
        tenant_id=tenant_id,
        prompt=prompt,
        feature="clause_interpretation",
        max_tokens=4096,
        temperature=0.15,
    )

    # Parse response
    parsed = _parse_json_response(response_text)
    interpretations = parsed.get("interpretations", [])

    # Persist to DB
    item_map = {str(item.id): item for item in items}
    updated = 0

    for interp in interpretations:
        cid = interp.get("clause_id")
        if not cid or cid not in item_map:
            continue

        item = item_map[cid]
        item.interpretation = interp.get("interpretation")
        item.risk_score = interp.get("risk_score")
        item.risk_category = interp.get("risk_category")
        item.risk_reason = interp.get("risk_reason")
        item.cross_references = interp.get("cross_references")
        updated += 1

    if updated > 0:
        await db.commit()

    return {
        "interpreted": updated,
        "total_clauses": len(items),
        "tokens_used": tokens_used,
        "model_used": model_used,
        "overall_risk_summary": parsed.get("overall_risk_summary", ""),
        "red_flags": parsed.get("red_flags", []),
        "interpretations": interpretations,
        "risk_distribution": _compute_risk_distribution(interpretations),
    }


def _compute_risk_distribution(interpretations: list[dict]) -> dict:
    """Compute risk distribution stats."""
    dist = {"low": 0, "medium": 0, "high": 0, "critical": 0}
    scores = []
    for i in interpretations:
        cat = i.get("risk_category", "low")
        if cat in dist:
            dist[cat] += 1
        score = i.get("risk_score", 0)
        if score:
            scores.append(score)

    return {
        "distribution": dist,
        "average_risk_score": round(sum(scores) / len(scores), 2) if scores else 0,
        "max_risk_score": max(scores) if scores else 0,
        "high_risk_count": dist["high"] + dist["critical"],
    }


def _parse_json_response(text: str) -> dict:
    """Parse JSON from AI response, stripping markdown code blocks if present."""
    text = text.strip()
    if text.startswith("```"):
        lines = text.split("\n")
        lines = [l for l in lines if not l.strip().startswith("```")]
        text = "\n".join(lines)
    return json.loads(text)
