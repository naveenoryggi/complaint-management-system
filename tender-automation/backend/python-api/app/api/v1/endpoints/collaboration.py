"""Collaboration endpoints — team assignments and comments."""
from typing import Optional

from fastapi import APIRouter, Depends, Query
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services import collaboration_service

router = APIRouter()


class AssignUserRequest(BaseModel):
    user_id: str
    user_name: str
    role: str = "member"


class CreateCommentRequest(BaseModel):
    comment_text: str
    parent_id: Optional[str] = None


class UpdateCommentRequest(BaseModel):
    comment_text: str


# --- Assignments ---

@router.get("/{tender_id}/assignments")
async def list_assignments(
    tender_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await collaboration_service.list_assignments(
        db, tender_id, current_user.tenant_id
    )


@router.post("/{tender_id}/assignments")
async def assign_user(
    tender_id: str,
    body: AssignUserRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await collaboration_service.assign_user(
        db, tender_id, current_user.tenant_id,
        body.user_id, body.user_name, body.role,
        current_user.user_id
    )


@router.delete("/{tender_id}/assignments/{user_id}")
async def unassign_user(
    tender_id: str,
    user_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await collaboration_service.unassign_user(
        db, tender_id, current_user.tenant_id, user_id
    )


# --- Comments ---

@router.get("/{tender_id}/comments")
async def list_comments(
    tender_id: str,
    page: int = Query(1, ge=1),
    page_size: int = Query(50, ge=1, le=200),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await collaboration_service.list_comments(
        db, tender_id, current_user.tenant_id, page, page_size
    )


@router.post("/{tender_id}/comments")
async def create_comment(
    tender_id: str,
    body: CreateCommentRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await collaboration_service.create_comment(
        db, tender_id, current_user.tenant_id,
        current_user.user_id, current_user.full_name or current_user.email,
        body.comment_text, body.parent_id
    )


@router.put("/{tender_id}/comments/{comment_id}")
async def update_comment(
    tender_id: str,
    comment_id: str,
    body: UpdateCommentRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await collaboration_service.update_comment(
        db, comment_id, current_user.tenant_id,
        current_user.user_id, body.comment_text
    )


@router.delete("/{tender_id}/comments/{comment_id}")
async def delete_comment(
    tender_id: str,
    comment_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await collaboration_service.delete_comment(
        db, comment_id, current_user.tenant_id, current_user.user_id
    )


@router.get("/{tender_id}/collaboration/summary")
async def get_collaboration_summary(
    tender_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    return await collaboration_service.get_collaboration_summary(
        db, tender_id, current_user.tenant_id
    )
