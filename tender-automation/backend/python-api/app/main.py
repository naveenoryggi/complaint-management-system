"""
Tender Automation API - Main FastAPI Application

This API handles tender document management, AI generation, and document assembly.
Authentication is handled by the existing .NET API.
"""
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from app.core.config import settings

# Create FastAPI app
app = FastAPI(
    title=settings.app_name,
    version=settings.version,
    description="AI-Powered Tender Filling Automation Platform",
    docs_url="/api/v1/docs",
    redoc_url="/api/v1/redoc",
    openapi_url="/api/v1/openapi.json",
)

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.cors_origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/")
async def root():
    """Root endpoint - API health check."""
    return {
        "message": "Tender Automation API",
        "version": settings.version,
        "status": "running",
        "environment": settings.environment,
    }


@app.get("/health")
async def health_check():
    """Health check endpoint for monitoring."""
    return {
        "status": "healthy",
        "service": "tender-api",
        "version": settings.version,
    }


# Import and include routers
from app.api.v1.endpoints import documents, ai, assembly, tenders, company, bundles, tracking, extraction, declarations

app.include_router(documents.router, prefix="/api/v1/documents", tags=["Documents"])
app.include_router(ai.router, prefix="/api/v1/ai", tags=["AI Generation"])
app.include_router(assembly.router, prefix="/api/v1/assembly", tags=["Assembly"])
app.include_router(tenders.router, prefix="/api/v1/tenders", tags=["Tenders"])
app.include_router(company.router, prefix="/api/v1/company", tags=["Company Master"])
app.include_router(bundles.router, prefix="/api/v1/bundles", tags=["Reference Bundles"])
app.include_router(tracking.router, prefix="/api/v1/tracking", tags=["Tracking"])
app.include_router(extraction.router, prefix="/api/v1/tenders", tags=["AI Extraction"])
app.include_router(declarations.router, prefix="/api/v1/declarations", tags=["Declarations"])


@app.exception_handler(Exception)
async def global_exception_handler(request, exc):
    """Global exception handler for unhandled errors."""
    if settings.debug:
        # In debug mode, show full error details
        import traceback
        return JSONResponse(
            status_code=500,
            content={
                "detail": str(exc),
                "type": type(exc).__name__,
                "traceback": traceback.format_exc(),
            },
        )
    else:
        # In production, hide error details
        return JSONResponse(
            status_code=500,
            content={"detail": "Internal server error"},
        )


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(
        "app.main:app",
        host=settings.host,
        port=settings.port,
        reload=settings.debug,
        log_level="debug" if settings.debug else "info",
    )
