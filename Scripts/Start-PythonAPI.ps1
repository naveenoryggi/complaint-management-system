# Start Python API for Tender Automation
# This script starts the FastAPI backend

param(
    [string]$PythonPath = "$env:ProgramFiles\ComplaintManagement\Python\python.exe",
    [string]$ApiPath = "$PSScriptRoot\..\backend\python-api",
    [int]$Port = 8000,
    [switch]$Production
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting Tender Automation Python API" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Check if Python exists
if (-not (Test-Path $PythonPath)) {
    Write-Host "Python not found at: $PythonPath" -ForegroundColor Red
    Write-Host "Please run Install-Python.ps1 first" -ForegroundColor Yellow
    exit 1
}

# Check if API directory exists
if (-not (Test-Path $ApiPath)) {
    Write-Host "API directory not found: $ApiPath" -ForegroundColor Red
    exit 1
}

# Navigate to API directory
Push-Location $ApiPath

try {
    Write-Host "`nChecking Python version..." -ForegroundColor Yellow
    & $PythonPath --version

    Write-Host "`nInstalling/Updating dependencies..." -ForegroundColor Yellow
    & $PythonPath -m pip install --upgrade pip --quiet
    & $PythonPath -m pip install -r requirements.txt --quiet

    Write-Host "`nCreating uploads directory..." -ForegroundColor Yellow
    $uploadsDir = Join-Path $ApiPath "uploads"
    if (-not (Test-Path $uploadsDir)) {
        New-Item -ItemType Directory -Path $uploadsDir -Force | Out-Null
    }

    Write-Host "`nChecking .env file..." -ForegroundColor Yellow
    if (-not (Test-Path ".env")) {
        Write-Host ".env file not found. Creating from .env.example..." -ForegroundColor Yellow
        Copy-Item ".env.example" ".env"
        Write-Host "Please edit .env file with your configuration" -ForegroundColor Yellow
    }

    Write-Host "`nStarting Python API..." -ForegroundColor Green
    Write-Host "API will be available at: http://localhost:$Port" -ForegroundColor Cyan
    Write-Host "API Docs: http://localhost:$Port/api/v1/docs" -ForegroundColor Cyan
    Write-Host "`nPress Ctrl+C to stop the server" -ForegroundColor Yellow
    Write-Host "========================================`n" -ForegroundColor Cyan

    if ($Production) {
        # Production mode with multiple workers
        & $PythonPath -m uvicorn app.main:app --host 0.0.0.0 --port $Port --workers 4
    } else {
        # Development mode with auto-reload
        & $PythonPath -m uvicorn app.main:app --reload --host 0.0.0.0 --port $Port
    }

} catch {
    Write-Host "`nError starting Python API: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}
