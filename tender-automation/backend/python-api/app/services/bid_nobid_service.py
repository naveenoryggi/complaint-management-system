"""Bid/No-Bid advisor service — AI-powered decision analysis."""
import json
import re
from uuid import UUID

from sqlalchemy import select, func, and_
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.tender import Tender
from app.models.tracking import EMDRecord, TenderFee
from app.models.compliance_matrix import BoQLineItem
from app.models.company import CompanyProfile
from app.models.tender_result import TenderResult
from app.services.ai_provider_service import send_message


async def analyze_bid_decision(
    db: AsyncSession,
    tender_id: str,
    tenant_id: str,
) -> dict:
    """Gather tender data + company capabilities, then use AI to recommend bid/no-bid."""
    tid = UUID(tender_id)
    tenant_uuid = UUID(tenant_id)

    # --- Gather data ---
    tender_q = await db.execute(
        select(Tender).where(and_(Tender.id == tid, Tender.tenant_id == tenant_uuid))
    )
    tender = tender_q.scalar_one_or_none()
    if not tender:
        return {"error": "Tender not found"}

    # Company profile
    company_q = await db.execute(
        select(CompanyProfile).where(CompanyProfile.tenant_id == tenant_uuid).limit(1)
    )
    company = company_q.scalar_one_or_none()

    # Past results
    results_q = await db.execute(
        select(TenderResult).where(TenderResult.tenant_id == tenant_uuid)
    )
    past_results = results_q.scalars().all()
    won_count = sum(1 for r in past_results if r.result == "won")
    lost_count = sum(1 for r in past_results if r.result == "lost")
    total = len(past_results)
    past_win_rate = (won_count / total * 100) if total > 0 else 0

    # EMD + fees
    emd_q = await db.execute(
        select(func.sum(EMDRecord.amount)).where(EMDRecord.tender_id == tid)
    )
    emd_total = float(emd_q.scalar() or 0)

    # Build AI prompt
    prompt = f"""Analyze whether we should bid on this tender. Provide a structured recommendation.

TENDER DETAILS:
- Title: {tender.title}
- Issuing Authority: {tender.issuing_authority or 'Unknown'}
- Estimated Value: {float(tender.estimated_value) if tender.estimated_value else 'Unknown'}
- Deadline: {str(tender.deadline) if tender.deadline else 'Unknown'}
- EMD Required: {emd_total:,.0f}
- Requirements: {json.dumps(tender.requirements) if tender.requirements else 'Not extracted'}

COMPANY CAPABILITIES:
- Company: {company.company_name if company else 'Unknown'}
- Past Win Rate: {past_win_rate:.0f}% ({won_count} won, {lost_count} lost out of {total})

Respond in JSON format:
{{
  "recommendation": "bid" or "no_bid" or "conditional",
  "confidence": <0-100>,
  "score_breakdown": {{
    "technical_fit": <0-100>,
    "financial_capacity": <0-100>,
    "timeline": <0-100>,
    "competition": <0-100>,
    "strategic": <0-100>
  }},
  "rationale": "<2-3 sentence analysis>",
  "conditions": ["<condition if conditional>"],
  "risks": ["<risk1>", "<risk2>"]
}}"""

    try:
        response_text, tokens, model = await send_message(
            db=db,
            tenant_id=tenant_id,
            prompt=prompt,
            feature="alignment",
        )

        json_match = re.search(r'\{[\s\S]*\}', response_text)
        if json_match:
            parsed = json.loads(json_match.group())
            return {
                "tender_id": tender_id,
                "recommendation": parsed.get("recommendation", "conditional"),
                "confidence": parsed.get("confidence", 50),
                "score_breakdown": parsed.get("score_breakdown", {}),
                "rationale": parsed.get("rationale", ""),
                "conditions": parsed.get("conditions", []),
                "risks": parsed.get("risks", []),
                "tokens_used": tokens,
                "model_used": model,
            }
    except Exception:
        pass

    # Fallback heuristic
    return {
        "tender_id": tender_id,
        "recommendation": "conditional",
        "confidence": 50,
        "score_breakdown": {
            "technical_fit": 60,
            "financial_capacity": 70,
            "timeline": 50,
            "competition": 50,
            "strategic": 60,
        },
        "rationale": "AI analysis unavailable. Manual review recommended based on company capabilities and tender requirements.",
        "conditions": ["Review technical requirements against company capabilities"],
        "risks": ["Insufficient data for automated analysis"],
        "tokens_used": 0,
        "model_used": "heuristic",
    }
