"""AI Provider configuration API endpoints — CRUD, test, default, feature mapping."""
from uuid import UUID
from typing import Optional

from fastapi import APIRouter, Depends, HTTPException, status
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.services import ai_provider_service

router = APIRouter()


# ---------------------------------------------------------------------------
# Schemas
# ---------------------------------------------------------------------------

class AIProviderCreate(BaseModel):
    provider_name: str  # anthropic | openai | google | azure_openai
    display_name: str
    api_key: str
    endpoint_url: Optional[str] = None
    deployment_name: Optional[str] = None
    api_version: Optional[str] = None
    default_model: str
    feature_mapping: Optional[dict] = None
    is_active: bool = True
    is_default: bool = False


class AIProviderUpdate(BaseModel):
    display_name: Optional[str] = None
    api_key: Optional[str] = None  # Only set if changed
    endpoint_url: Optional[str] = None
    deployment_name: Optional[str] = None
    api_version: Optional[str] = None
    default_model: Optional[str] = None
    feature_mapping: Optional[dict] = None
    is_active: Optional[bool] = None
    is_default: Optional[bool] = None


class FeatureMappingUpdate(BaseModel):
    feature_mapping: dict


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------

@router.get("")
async def list_providers(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all AI providers for the current tenant (API keys masked)."""
    providers = await ai_provider_service.list_providers(db, current_user.tenant_id)
    return {"providers": providers, "total": len(providers)}


@router.post("", status_code=status.HTTP_201_CREATED)
async def create_provider(
    data: AIProviderCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new AI provider configuration."""
    if data.provider_name not in ai_provider_service.VALID_PROVIDERS:
        raise HTTPException(
            status_code=400,
            detail=f"Invalid provider_name. Must be one of: {', '.join(ai_provider_service.VALID_PROVIDERS)}",
        )

    create_data = data.model_dump()
    create_data["created_by"] = current_user.user_id

    provider = await ai_provider_service.create_provider(db, current_user.tenant_id, create_data)
    await db.commit()
    return provider


@router.get("/{provider_id}")
async def get_provider(
    provider_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get a single AI provider (masked key)."""
    provider = await ai_provider_service.get_provider(db, provider_id, current_user.tenant_id)
    if not provider:
        raise HTTPException(status_code=404, detail="Provider not found")
    return provider


@router.put("/{provider_id}")
async def update_provider(
    provider_id: UUID,
    data: AIProviderUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update an AI provider. Re-encrypts key if changed."""
    update_data = data.model_dump(exclude_unset=True)
    provider = await ai_provider_service.update_provider(db, provider_id, current_user.tenant_id, update_data)
    if not provider:
        raise HTTPException(status_code=404, detail="Provider not found")
    await db.commit()
    return provider


@router.delete("/{provider_id}")
async def delete_provider(
    provider_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete an AI provider configuration."""
    deleted = await ai_provider_service.delete_provider(db, provider_id, current_user.tenant_id)
    if not deleted:
        raise HTTPException(status_code=404, detail="Provider not found")
    await db.commit()
    return {"message": "Provider deleted"}


@router.post("/{provider_id}/test")
async def test_connection(
    provider_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Test connection to an AI provider with a minimal request."""
    result = await ai_provider_service.test_connection(db, provider_id, current_user.tenant_id)
    await db.commit()
    return result


@router.put("/{provider_id}/set-default")
async def set_default_provider(
    provider_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Set a provider as the tenant default."""
    provider = await ai_provider_service.set_default(db, provider_id, current_user.tenant_id)
    if not provider:
        raise HTTPException(status_code=404, detail="Provider not found")
    await db.commit()
    return provider


@router.put("/{provider_id}/features")
async def update_feature_mapping(
    provider_id: UUID,
    data: FeatureMappingUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update the feature mapping for a provider."""
    provider = await ai_provider_service.update_provider(
        db, provider_id, current_user.tenant_id,
        {"feature_mapping": data.feature_mapping},
    )
    if not provider:
        raise HTTPException(status_code=404, detail="Provider not found")
    await db.commit()
    return provider
