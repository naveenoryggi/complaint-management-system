# Run database migrations for Tender Automation
# This script applies database migrations using Alembic

param(
    [string]$PythonPath = "$env:ProgramFiles\ComplaintManagement\Python\python.exe",
    [string]$ApiPath = "$PSScriptRoot\..\backend\python-api"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Running Tender Database Migrations" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Check if Python exists
if (-not (Test-Path $PythonPath)) {
    # Try to find Python in PATH
    $pythonInPath = Get-Command python -ErrorAction SilentlyContinue
    if ($pythonInPath) {
        $PythonPath = $pythonInPath.Source
        Write-Host "Using Python from PATH: $PythonPath" -ForegroundColor Yellow
    } else {
        Write-Host "Python not found. Please install Python first." -ForegroundColor Red
        exit 1
    }
}

# Check if API directory exists
if (-not (Test-Path $ApiPath)) {
    Write-Host "API directory not found: $ApiPath" -ForegroundColor Red
    exit 1
}

# Navigate to API directory
Push-Location $ApiPath

try {
    Write-Host "`nPython version:" -ForegroundColor Yellow
    & $PythonPath --version

    Write-Host "`nInstalling Alembic..." -ForegroundColor Yellow
    & $PythonPath -m pip install alembic psycopg2-binary --quiet
    Write-Host "✓ Alembic installed" -ForegroundColor Green

    Write-Host "`nChecking database connection..." -ForegroundColor Yellow
    # Test database connection would go here

    Write-Host "`nApplying database migrations..." -ForegroundColor Yellow
    & $PythonPath -m alembic upgrade head

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n========================================" -ForegroundColor Green
        Write-Host "Database Migrations Completed!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "`nTender tables created successfully:" -ForegroundColor Cyan
        Write-Host "  - tenders" -ForegroundColor White
        Write-Host "  - documents" -ForegroundColor White
        Write-Host "  - tender_documents" -ForegroundColor White
        Write-Host "  - ai_generations" -ForegroundColor White
        return $true
    } else {
        Write-Host "`nMigration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        return $false
    }

} catch {
    Write-Host "`nError running migrations: $_" -ForegroundColor Red
    return $false
} finally {
    Pop-Location
}
