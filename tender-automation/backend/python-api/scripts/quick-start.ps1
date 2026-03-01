# Quick Start Script for Tender Automation API (Windows PowerShell)
# This script sets up and runs the development environment

Write-Host "🚀 Tender Automation API - Quick Start" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if .env exists
if (-not (Test-Path .env)) {
    Write-Host "⚠️  .env file not found. Creating from .env.example..." -ForegroundColor Yellow
    Copy-Item .env.example .env
    Write-Host "✅ Created .env file. Please edit it with your configuration." -ForegroundColor Green
    Write-Host ""
    Write-Host "Required settings:" -ForegroundColor Yellow
    Write-Host "  - JWT_SECRET_KEY (must match .NET API)"
    Write-Host "  - ANTHROPIC_API_KEY (get from anthropic.com)"
    Write-Host ""
    Read-Host "Press Enter after updating .env"
}

# Check if virtual environment exists
if (-not (Test-Path venv)) {
    Write-Host "📦 Creating virtual environment..." -ForegroundColor Cyan
    python -m venv venv
}

# Activate virtual environment
Write-Host "🔄 Activating virtual environment..." -ForegroundColor Cyan
& .\venv\Scripts\Activate.ps1

# Install dependencies
Write-Host "📚 Installing dependencies..." -ForegroundColor Cyan
pip install -r requirements.txt

# Run database migrations
Write-Host "🗄️  Running database migrations..." -ForegroundColor Cyan
alembic upgrade head

# Start the server
Write-Host ""
Write-Host "🚀 Starting FastAPI server..." -ForegroundColor Green
Write-Host ""
Write-Host "API will be available at:" -ForegroundColor Yellow
Write-Host "  - http://localhost:8000"
Write-Host "  - Swagger Docs: http://localhost:8000/api/v1/docs"
Write-Host ""

uvicorn app.main:app --reload --port 8000
