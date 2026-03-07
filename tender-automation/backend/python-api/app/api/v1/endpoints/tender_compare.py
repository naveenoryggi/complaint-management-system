"""Tender comparison endpoint — side-by-side metrics."""
from typing import List
from uuid import UUID

from fastapi import APIRouter, Depends
from pydantic import BaseModel
from sqlalchemy import select, func, and_
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.models.tender import Tender
from app.models.tracking import EMDRecord, TenderFee
from app.models.compliance_matrix import BoQLineItem
from app.models.oem import OEMTenderRequirement
from app.models.document import TenderDocument

router = APIRouter()


class CompareRequest(BaseModel):
    tender_ids: List[str]


@router.post("/compare")
async def compare_tenders(
    body: CompareRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Compare multiple tenders side-by-side."""
    comparisons = []

    for tid_str in body.tender_ids[:4]:  # Max 4
        tid = UUID(tid_str)

        # Tender basics
        t_result = await db.execute(
            select(Tender).where(
                and_(Tender.id == tid, Tender.tenant_id == UUID(current_user.tenant_id))
            )
        )
        tender = t_result.scalar_one_or_none()
        if not tender:
            continue

        # Document count
        doc_count_result = await db.execute(
            select(func.count()).where(TenderDocument.tender_id == tid)
        )
        doc_count = doc_count_result.scalar() or 0

        # EMD total
        emd_result = await db.execute(
            select(func.sum(EMDRecord.amount)).where(EMDRecord.tender_id == tid)
        )
        emd_total = emd_result.scalar() or 0

        # Fee total
        fee_result = await db.execute(
            select(func.sum(TenderFee.amount)).where(TenderFee.tender_id == tid)
        )
        fee_total = fee_result.scalar() or 0

        # OEM count
        oem_result = await db.execute(
            select(func.count()).where(OEMTenderRequirement.tender_id == tid)
        )
        oem_count = oem_result.scalar() or 0

        # BoQ total
        boq_result = await db.execute(
            select(func.count()).where(BoQLineItem.tender_id == tid)
        )
        boq_count = boq_result.scalar() or 0

        comparisons.append({
            "tender_id": tid_str,
            "title": tender.title,
            "reference_number": tender.reference_number,
            "issuing_authority": tender.issuing_authority,
            "status": tender.status,
            "deadline": str(tender.deadline) if tender.deadline else None,
            "estimated_value": float(tender.estimated_value) if tender.estimated_value else None,
            "document_count": doc_count,
            "emd_total": float(emd_total),
            "fee_total": float(fee_total),
            "oem_count": oem_count,
            "boq_items": boq_count,
        })

    return comparisons
