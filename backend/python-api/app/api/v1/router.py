"""
API v1 Router
Includes all endpoint routers
"""
from fastapi import APIRouter

from app.api.v1.endpoints import tenders, documents, ai, assembly

api_router = APIRouter()

# Include endpoint routers
api_router.include_router(tenders.router, prefix="/tenders", tags=["tenders"])
api_router.include_router(documents.router, prefix="/documents", tags=["documents"])
api_router.include_router(ai.router, prefix="/ai", tags=["ai"])
api_router.include_router(assembly.router, prefix="/assembly", tags=["assembly"])
