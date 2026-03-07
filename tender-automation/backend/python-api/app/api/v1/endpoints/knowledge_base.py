"""Knowledge base API — CRUD, suggestions, harvesting, usage tracking, dashboard stats, and doc generation."""
from typing import Optional, List
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, Query, status
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services import knowledge_base_service as kb_svc

router = APIRouter()


# ---------------------------------------------------------------------------
# Pydantic Schemas
# ---------------------------------------------------------------------------

class KBEntryCreate(BaseModel):
    title: str
    content: str
    content_format: str = "text"
    category: str
    subcategory: str | None = None
    tags: list | None = None
    keywords: list | None = None
    source_type: str = "manual"
    source_tender_id: str | None = None
    source_description: str | None = None
    valid_from: str | None = None
    valid_until: str | None = None
    fiscal_year: str | None = None
    is_current: bool = True
    confidence_score: float | None = None
    is_verified: bool = False


class KBEntryUpdate(BaseModel):
    title: str | None = None
    content: str | None = None
    content_format: str | None = None
    category: str | None = None
    subcategory: str | None = None
    tags: list | None = None
    keywords: list | None = None
    source_type: str | None = None
    source_tender_id: str | None = None
    source_description: str | None = None
    valid_from: str | None = None
    valid_until: str | None = None
    fiscal_year: str | None = None
    is_current: bool | None = None
    confidence_score: float | None = None
    is_verified: bool | None = None
    is_archived: bool | None = None
    change_reason: str | None = None


class RecordUsageRequest(BaseModel):
    entry_id: str
    tender_id: str
    usage_context: str | None = None


class GenerateSummaryRequest(BaseModel):
    tender_id: str
    entry_ids: list[str]


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------

@router.get("")
async def list_entries(
    category: str | None = Query(None),
    subcategory: str | None = Query(None),
    search: str | None = Query(None),
    is_current: bool | None = Query(None),
    page: int = Query(1, ge=1),
    page_size: int = Query(50, ge=1, le=200),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List knowledge base entries with optional filters and pagination."""
    return await kb_svc.list_entries(
        db, current_user.tenant_id,
        category=category, subcategory=subcategory,
        search=search, is_current=is_current,
        page=page, page_size=page_size,
    )


@router.get("/categories")
async def get_category_stats(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get per-category stats (counts, usage totals, stale counts)."""
    return await kb_svc.get_category_stats(db, current_user.tenant_id)


@router.get("/suggest")
async def get_suggestions(
    category: str | None = Query(None),
    subcategory: str | None = Query(None),
    keywords: str | None = Query(None, description="Comma-separated keywords"),
    tender_id: str | None = Query(None),
    limit: int = Query(20, ge=1, le=100),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get ranked KB suggestions based on category, keywords, and context."""
    kw_list = [k.strip() for k in keywords.split(",") if k.strip()] if keywords else None
    return await kb_svc.get_suggestions(
        db, current_user.tenant_id,
        category=category, subcategory=subcategory,
        context_keywords=kw_list, tender_id=tender_id,
        limit=limit,
    )


@router.get("/dashboard-stats")
async def get_dashboard_stats(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get aggregate KB stats for the tenant dashboard."""
    return await kb_svc.dashboard_stats(db, current_user.tenant_id)


@router.post("/generate-summary")
async def generate_summary(
    data: GenerateSummaryRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Generate a DOCX summary from selected KB entries."""
    if not data.entry_ids:
        raise HTTPException(status_code=400, detail="entry_ids cannot be empty")
    buffer = await kb_svc.generate_summary(
        db, UUID(data.tender_id), data.entry_ids, current_user.tenant_id,
    )
    return StreamingResponse(
        buffer,
        media_type="application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        headers={"Content-Disposition": "attachment; filename=kb_summary.docx"},
    )


@router.get("/{entry_id}")
async def get_entry(
    entry_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get a single KB entry with its versions and tender links."""
    result = await kb_svc.get_entry(db, entry_id, current_user.tenant_id)
    if not result:
        raise HTTPException(status_code=404, detail="Knowledge base entry not found")
    return result


@router.post("", status_code=status.HTTP_201_CREATED)
async def create_entry(
    data: KBEntryCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new knowledge base entry (with initial version v1)."""
    return await kb_svc.create_entry(
        db, current_user.tenant_id, current_user.user_id,
        data.model_dump(),
    )


@router.put("/{entry_id}")
async def update_entry(
    entry_id: UUID,
    data: KBEntryUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a KB entry. Creates a new version if content is changed."""
    result = await kb_svc.update_entry(
        db, entry_id, current_user.tenant_id, current_user.user_id,
        data.model_dump(exclude_unset=True),
    )
    if not result:
        raise HTTPException(status_code=404, detail="Knowledge base entry not found")
    return result


@router.delete("/{entry_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_entry(
    entry_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a knowledge base entry."""
    deleted = await kb_svc.delete_entry(db, entry_id, current_user.tenant_id)
    if not deleted:
        raise HTTPException(status_code=404, detail="Knowledge base entry not found")


@router.post("/record-usage")
async def record_usage(
    data: RecordUsageRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Record that a KB entry was used in a tender (increments usage_count, creates tender link)."""
    result = await kb_svc.record_usage(
        db, UUID(data.entry_id), data.tender_id,
        data.usage_context, current_user.user_id,
    )
    if "error" in result:
        raise HTTPException(status_code=404, detail=result["error"])
    return result


@router.post("/harvest/tender/{tender_id}")
async def harvest_from_tender(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Harvest reusable content from a specific tender's compliance matrix and BoQ."""
    result = await kb_svc.harvest_from_tender(
        db, tender_id, current_user.tenant_id, current_user.user_id,
    )
    return result


@router.post("/harvest/company")
async def harvest_company_data(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Sync company master data (turnover, personnel, experience) into KB entries."""
    result = await kb_svc.harvest_company_data(
        db, current_user.tenant_id, current_user.user_id,
    )
    return result


@router.get("/auto-fill/{tender_id}")
async def get_auto_fill_suggestions(
    tender_id: str,
    target_area: str = Query(..., description="Target area: declarations, compliance, or bid_preparation"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get auto-fill suggestions from KB entries matched to tender requirements."""
    return await kb_svc.auto_fill_suggestions(
        db, current_user.tenant_id, tender_id, target_area,
    )
