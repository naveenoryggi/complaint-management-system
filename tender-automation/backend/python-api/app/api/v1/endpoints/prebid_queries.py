"""Pre-bid query generator API — AI-powered generation, export, and editing of pre-bid queries."""
from uuid import UUID
from typing import Optional

from fastapi import APIRouter, Depends, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services import prebid_query_service

router = APIRouter()


# ---------------------------------------------------------------------------
# Pydantic Schemas
# ---------------------------------------------------------------------------

class PrebidQuery(BaseModel):
    sno: int
    tender_reference: str
    existing_clause: str
    clarification_sought: str
    response: str = ""
    category: str = ""
    priority: str = "medium"


class ExportRequest(BaseModel):
    queries: list[PrebidQuery]
    tender_title: str = ""
    tender_reference: str = ""


class PrebidQueryUpdate(BaseModel):
    tender_reference: Optional[str] = None
    existing_clause: Optional[str] = None
    clarification_sought: Optional[str] = None
    response: Optional[str] = None
    category: Optional[str] = None
    priority: Optional[str] = None


class SuggestResponseRequest(BaseModel):
    query_index: int
    query: PrebidQuery
    all_queries: list[PrebidQuery] = []


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------

@router.post("/{tender_id}/prebid-queries/generate")
async def generate_prebid_queries(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """AI-generate pre-bid queries by analysing tender clauses and compliance matrix.

    Analyses the tender's requirements, compliance matrix (especially not_complied
    and partially_complied items), and generates structured pre-bid queries suitable
    for submission to the issuing authority.
    """
    try:
        result = await prebid_query_service.generate_prebid_queries(
            db, tender_id, current_user.tenant_id,
        )
        return result
    except ValueError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to generate pre-bid queries: {str(e)}")


@router.post("/{tender_id}/prebid-queries/export")
async def export_prebid_queries(
    tender_id: UUID,
    data: ExportRequest,
    current_user: TokenData = Depends(get_current_user),
):
    """Export pre-bid queries as a DOCX document.

    Accepts the queries list (possibly edited by the user) and returns a
    formatted Word document with a standard pre-bid query table.
    """
    if not data.queries:
        raise HTTPException(status_code=400, detail="queries list cannot be empty")

    queries_dicts = [q.model_dump() for q in data.queries]
    buffer = prebid_query_service.export_prebid_queries_docx(
        queries=queries_dicts,
        tender_title=data.tender_title,
        tender_reference=data.tender_reference,
    )

    # Build a safe filename
    safe_ref = (data.tender_reference or "tender").replace("/", "-").replace(" ", "_")[:50]
    filename = f"prebid_queries_{safe_ref}.docx"

    return StreamingResponse(
        buffer,
        media_type="application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        headers={"Content-Disposition": f"attachment; filename={filename}"},
    )


@router.put("/{tender_id}/prebid-queries/{index}")
async def update_prebid_query(
    tender_id: UUID,
    index: int,
    data: PrebidQueryUpdate,
    current_user: TokenData = Depends(get_current_user),
):
    """Edit a single pre-bid query by its index (0-based).

    This is a stateless endpoint — the client holds the queries list in memory
    and sends the updated fields for a specific query. The server validates and
    returns the merged query object so the client can replace it in its local list.

    Note: tender_id is accepted for route consistency but the actual query data
    comes from the request body. No database persistence is involved since
    pre-bid queries are generated on-the-fly and edited client-side.
    """
    if index < 0:
        raise HTTPException(status_code=400, detail="index must be >= 0")

    # Build the updated query from provided fields
    update_fields = data.model_dump(exclude_unset=True)
    if not update_fields:
        raise HTTPException(status_code=400, detail="No fields provided for update")

    # Validate category if provided
    valid_categories = [
        "ambiguous_specs",
        "contradictory_clauses",
        "restrictive_conditions",
        "missing_timelines",
        "unclear_evaluation",
        "onerous_commercial",
    ]
    if "category" in update_fields and update_fields["category"] not in valid_categories:
        raise HTTPException(
            status_code=400,
            detail=f"Invalid category. Must be one of: {', '.join(valid_categories)}",
        )

    # Validate priority if provided
    valid_priorities = ["high", "medium", "low"]
    if "priority" in update_fields and update_fields["priority"] not in valid_priorities:
        raise HTTPException(
            status_code=400,
            detail=f"Invalid priority. Must be one of: {', '.join(valid_priorities)}",
        )

    # Return the updated query with sno preserved
    updated_query = {
        "sno": index + 1,
        "tender_reference": update_fields.get("tender_reference", ""),
        "existing_clause": update_fields.get("existing_clause", ""),
        "clarification_sought": update_fields.get("clarification_sought", ""),
        "response": update_fields.get("response", ""),
        "category": update_fields.get("category", ""),
        "priority": update_fields.get("priority", "medium"),
        "index": index,
        "updated": True,
    }

    return updated_query


@router.post("/{tender_id}/prebid-queries/suggest-response")
async def suggest_prebid_response(
    tender_id: UUID,
    data: SuggestResponseRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """AI-generate a suggested response for a single pre-bid query.

    The AI drafts a response as if it were the tendering authority,
    providing a professional clarification or amendment.
    """
    try:
        result = await prebid_query_service.suggest_response(
            db=db,
            tender_id=tender_id,
            tenant_id=current_user.tenant_id,
            query=data.query.model_dump(),
            all_queries=[q.model_dump() for q in data.all_queries],
        )
        return result
    except ValueError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to suggest response: {str(e)}")
