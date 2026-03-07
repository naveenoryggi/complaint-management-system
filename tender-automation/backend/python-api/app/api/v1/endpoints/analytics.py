"""Tender analytics API endpoint."""
from uuid import UUID
from datetime import datetime, timedelta
from typing import List, Optional

from fastapi import APIRouter, Depends, Query
from pydantic import BaseModel
from sqlalchemy import select, func, desc, case, extract
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.models.tender import Tender

router = APIRouter()


# ---------- Response Schemas ----------

class StatusDistributionItem(BaseModel):
    status: str
    count: int


class MonthlyTrendItem(BaseModel):
    year: int
    month: int
    count: int


class AuthorityBreakdownItem(BaseModel):
    authority: str
    count: int


class ValueAnalysisItem(BaseModel):
    status: str
    total_value: float
    count: int


class AnalyticsResponse(BaseModel):
    total_tenders: int
    status_distribution: List[StatusDistributionItem]
    monthly_trend: List[MonthlyTrendItem]
    authority_breakdown: List[AuthorityBreakdownItem]
    win_rate: float
    value_analysis: List[ValueAnalysisItem]


# ---------- Endpoint ----------

@router.get("/", response_model=AnalyticsResponse)
async def get_tender_analytics(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get tender analytics for the current tenant.

    Returns aggregated statistics including:
    - **total_tenders**: Total number of tenders
    - **status_distribution**: Count of tenders grouped by status
    - **monthly_trend**: Tender creation count per month (last 12 months)
    - **authority_breakdown**: Top 10 issuing authorities by tender count
    - **win_rate**: Percentage of won tenders out of decided tenders (submitted + won + lost)
    - **value_analysis**: Estimated value totals grouped by status

    Requires authentication.
    """
    tenant_filter = Tender.tenant_id == UUID(current_user.tenant_id)

    # 1. Total tenders
    total_query = select(func.count()).select_from(Tender).where(tenant_filter)
    total_result = await db.execute(total_query)
    total_tenders = total_result.scalar() or 0

    # 2. Status distribution — GROUP BY status
    status_query = (
        select(
            Tender.status,
            func.count().label("count"),
        )
        .where(tenant_filter)
        .group_by(Tender.status)
    )
    status_result = await db.execute(status_query)
    status_distribution = [
        StatusDistributionItem(status=row.status, count=row.count)
        for row in status_result.all()
    ]

    # 3. Monthly trend — last 12 months, GROUP BY year + month of created_at
    twelve_months_ago = datetime.utcnow() - timedelta(days=365)
    year_col = extract("year", Tender.created_at).label("year")
    month_col = extract("month", Tender.created_at).label("month")

    monthly_query = (
        select(
            year_col,
            month_col,
            func.count().label("count"),
        )
        .where(tenant_filter, Tender.created_at >= twelve_months_ago)
        .group_by(year_col, month_col)
        .order_by(desc(year_col), desc(month_col))
        .limit(12)
    )
    monthly_result = await db.execute(monthly_query)
    monthly_trend = [
        MonthlyTrendItem(year=int(row.year), month=int(row.month), count=row.count)
        for row in monthly_result.all()
    ]

    # 4. Authority breakdown — top 10 issuing authorities
    authority_query = (
        select(
            Tender.issuing_authority,
            func.count().label("count"),
        )
        .where(tenant_filter, Tender.issuing_authority.isnot(None))
        .group_by(Tender.issuing_authority)
        .order_by(desc("count"))
        .limit(10)
    )
    authority_result = await db.execute(authority_query)
    authority_breakdown = [
        AuthorityBreakdownItem(authority=row.issuing_authority, count=row.count)
        for row in authority_result.all()
    ]

    # 5. Win rate — won / (submitted + won + lost) * 100
    decided_statuses = ["submitted", "won", "lost"]
    win_rate_query = select(
        func.sum(case((Tender.status == "won", 1), else_=0)).label("won_count"),
        func.count().label("decided_count"),
    ).where(tenant_filter, Tender.status.in_(decided_statuses))
    win_rate_result = await db.execute(win_rate_query)
    win_rate_row = win_rate_result.one()
    won_count = win_rate_row.won_count or 0
    decided_count = win_rate_row.decided_count or 0
    win_rate = round((won_count / decided_count) * 100, 2) if decided_count > 0 else 0.0

    # 6. Value analysis — SUM(estimated_value) grouped by status
    value_query = (
        select(
            Tender.status,
            func.sum(Tender.estimated_value).label("total_value"),
            func.count().label("count"),
        )
        .where(tenant_filter, Tender.estimated_value.isnot(None))
        .group_by(Tender.status)
    )
    value_result = await db.execute(value_query)
    value_analysis = [
        ValueAnalysisItem(
            status=row.status,
            total_value=float(row.total_value or 0),
            count=row.count,
        )
        for row in value_result.all()
    ]

    return AnalyticsResponse(
        total_tenders=total_tenders,
        status_distribution=status_distribution,
        monthly_trend=monthly_trend,
        authority_breakdown=authority_breakdown,
        win_rate=win_rate,
        value_analysis=value_analysis,
    )
