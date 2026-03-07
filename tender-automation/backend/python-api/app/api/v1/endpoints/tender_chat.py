"""Tender AI Chat API — per-tender conversational AI assistant."""
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, Query, status
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services.tender_chat_service import send_chat_message, get_chat_history, clear_chat

router = APIRouter()


class ChatMessageRequest(BaseModel):
    message: str


@router.get("/{tender_id}/chat")
async def get_history(
    tender_id: UUID,
    page: int = Query(1, ge=1),
    page_size: int = Query(50, ge=1, le=100),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get paginated chat history for a tender."""
    return await get_chat_history(
        db, tender_id, UUID(current_user.tenant_id), page=page, page_size=page_size
    )


@router.post("/{tender_id}/chat")
async def post_chat_message(
    tender_id: UUID,
    body: ChatMessageRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Send a chat message and get AI response."""
    if not body.message or not body.message.strip():
        raise HTTPException(status_code=400, detail="Message cannot be empty")

    result = await send_chat_message(db, tender_id, body.message.strip(), current_user)
    await db.commit()
    return result


@router.delete("/{tender_id}/chat")
async def delete_chat(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Clear all chat messages for a tender."""
    result = await clear_chat(db, tender_id, UUID(current_user.tenant_id))
    await db.commit()
    return result
