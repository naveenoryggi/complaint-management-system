"""Bid cost estimator service — aggregates EMD, fees, BoQ and AI margin analysis."""
from uuid import UUID
from typing import Optional

from sqlalchemy import select, func, and_
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.tender import Tender
from app.models.tracking import EMDRecord, TenderFee
from app.models.compliance_matrix import BoQLineItem
from app.services.ai_provider_service import send_message


async def estimate_bid_cost(
    db: AsyncSession,
    tender_id: str,
    tenant_id: str,
) -> dict:
    """Compute full bid cost estimate with AI-powered margin analysis."""

    tid = UUID(tender_id)

    # --- Fetch tender ---
    tender_result = await db.execute(
        select(Tender).where(and_(Tender.id == tid, Tender.tenant_id == UUID(tenant_id)))
    )
    tender = tender_result.scalar_one_or_none()
    if not tender:
        return {"error": "Tender not found"}

    # --- EMD ---
    emd_result = await db.execute(
        select(EMDRecord).where(EMDRecord.tender_id == tid)
    )
    emds = emd_result.scalars().all()
    total_emd = sum(e.amount for e in emds if e.amount)

    # --- Tender Fees ---
    fee_result = await db.execute(
        select(TenderFee).where(TenderFee.tender_id == tid)
    )
    fees = fee_result.scalars().all()
    total_fees = sum(f.amount for f in fees if f.amount)

    # --- BoQ Summary ---
    boq_result = await db.execute(
        select(BoQLineItem).where(BoQLineItem.tender_id == tid)
    )
    boq_items = boq_result.scalars().all()

    boq_subtotal = 0.0
    boq_gst = 0.0
    boq_total = 0.0
    for item in boq_items:
        line_total = float(item.quantity or 0) * float(item.unit_rate or 0)
        gst_pct = float(item.gst_percentage or 0)
        gst_amt = line_total * gst_pct / 100
        boq_subtotal += line_total
        boq_gst += gst_amt
        boq_total += line_total + gst_amt

    # Estimated overhead (2% of BoQ)
    document_cost = 5000  # Estimated printing/notarization
    travel_cost = 10000   # Estimated travel for submission
    overhead = document_cost + travel_cost

    direct_costs = {
        "emd": total_emd,
        "tender_fee": total_fees,
        "document_cost": document_cost,
        "travel_cost": travel_cost,
    }

    boq_summary = {
        "subtotal": round(boq_subtotal, 2),
        "gst": round(boq_gst, 2),
        "total": round(boq_total, 2),
        "item_count": len(boq_items),
    }

    total_bid_value = boq_total + total_emd + total_fees + overhead

    # --- AI Analysis (if BoQ items exist) ---
    ai_analysis = {
        "recommended_margin_pct": 15.0,
        "risk_factors": [],
        "competitive_score": 70,
    }

    if boq_items:
        try:
            prompt = f"""Analyze this bid cost estimate and provide recommendations:

Tender: {tender.title}
Estimated Value: {float(tender.estimated_value) if tender.estimated_value else 'Unknown'}
BoQ Total (excl GST): {boq_subtotal:,.2f}
BoQ Total (incl GST): {boq_total:,.2f}
EMD Amount: {total_emd:,.2f}
Tender Fees: {total_fees:,.2f}
Number of BoQ items: {len(boq_items)}

Respond in JSON format:
{{
  "recommended_margin_pct": <number 5-25>,
  "risk_factors": ["<risk1>", "<risk2>"],
  "competitive_score": <number 0-100>,
  "analysis_notes": "<brief analysis>"
}}"""
            response_text, tokens, model = await send_message(
                db=db,
                tenant_id=tenant_id,
                prompt=prompt,
                feature="alignment",
            )
            # Parse AI response
            import json
            import re
            json_match = re.search(r'\{[\s\S]*\}', response_text)
            if json_match:
                parsed = json.loads(json_match.group())
                ai_analysis = {
                    "recommended_margin_pct": parsed.get("recommended_margin_pct", 15.0),
                    "risk_factors": parsed.get("risk_factors", []),
                    "competitive_score": parsed.get("competitive_score", 70),
                    "analysis_notes": parsed.get("analysis_notes", ""),
                    "tokens_used": tokens,
                    "model_used": model,
                }
        except Exception:
            # Keep default analysis on AI failure
            ai_analysis["analysis_notes"] = "AI analysis unavailable — using defaults"

    profit_estimate = boq_subtotal * (ai_analysis["recommended_margin_pct"] / 100)

    return {
        "tender_id": tender_id,
        "tender_title": tender.title,
        "direct_costs": direct_costs,
        "boq_summary": boq_summary,
        "overhead": overhead,
        "ai_analysis": ai_analysis,
        "total_bid_value": round(total_bid_value, 2),
        "profit_estimate": round(profit_estimate, 2),
    }
