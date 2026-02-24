# Setup Tender Automation Complete Installation
# This script is called by the Inno Setup installer
# It performs all post-installation configuration

param(
    [string]$InstallPath = "$env:ProgramFiles\ComplaintManagement"
)

$ErrorActionPreference = "Continue"  # Continue on errors to complete as much as possible

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Tender Automation Post-Install Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Track overall success
$overallSuccess = $true
$pythonInstalled = $false
$migrationsRun = $false
$serviceConfigured = $false

# Step 1: Install Python
Write-Host "`n[1/3] Installing Python 3.12..." -ForegroundColor Cyan
try {
    $installPythonScript = Join-Path $PSScriptRoot "Install-Python.ps1"
    if (Test-Path $installPythonScript) {
        $result = & $installPythonScript -InstallPath "$InstallPath\Python"
        if ($result) {
            Write-Host "✓ Python installed successfully" -ForegroundColor Green
            $pythonInstalled = $true
        } else {
            Write-Host "✗ Python installation failed" -ForegroundColor Red
            $overallSuccess = $false
        }
    } else {
        Write-Host "✗ Install-Python.ps1 not found" -ForegroundColor Red
        $overallSuccess = $false
    }
} catch {
    Write-Host "✗ Error installing Python: $_" -ForegroundColor Red
    $overallSuccess = $false
}

# Step 2: Run Database Migrations
if ($pythonInstalled) {
    Write-Host "`n[2/3] Running database migrations..." -ForegroundColor Cyan
    try {
        $migrationsScript = Join-Path $PSScriptRoot "Run-DatabaseMigrations.ps1"
        if (Test-Path $migrationsScript) {
            $pythonExe = "$InstallPath\Python\python.exe"
            $apiPath = "$InstallPath\backend\python-api"

            $result = & $migrationsScript -PythonPath $pythonExe -ApiPath $apiPath
            if ($result) {
                Write-Host "✓ Database migrations completed" -ForegroundColor Green
                $migrationsRun = $true
            } else {
                Write-Host "⚠ Database migrations failed (can be run manually later)" -ForegroundColor Yellow
            }
        } else {
            Write-Host "✗ Run-DatabaseMigrations.ps1 not found" -ForegroundColor Red
        }
    } catch {
        Write-Host "⚠ Error running migrations: $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "`n[2/3] Skipping migrations (Python not installed)" -ForegroundColor Yellow
}

# Step 3: Configure Python API Service
if ($pythonInstalled) {
    Write-Host "`n[3/3] Configuring Python API service..." -ForegroundColor Cyan
    try {
        $serviceScript = Join-Path $PSScriptRoot "Configure-PythonAPI-Service.ps1"
        if (Test-Path $serviceScript) {
            $result = & $serviceScript -InstallPath $InstallPath -Port 8000
            if ($result) {
                Write-Host "✓ Python API service configured" -ForegroundColor Green
                $serviceConfigured = $true
            } else {
                Write-Host "⚠ Service configuration failed (can be configured manually)" -ForegroundColor Yellow
            }
        } else {
            Write-Host "✗ Configure-PythonAPI-Service.ps1 not found" -ForegroundColor Red
        }
    } catch {
        Write-Host "⚠ Error configuring service: $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "`n[3/3] Skipping service configuration (Python not installed)" -ForegroundColor Yellow
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Installation Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`nComponents:" -ForegroundColor Yellow
if ($pythonInstalled) {
    Write-Host "  ✓ Python 3.12" -ForegroundColor Green
} else {
    Write-Host "  ✗ Python 3.12" -ForegroundColor Red
}

if ($migrationsRun) {
    Write-Host "  ✓ Database Migrations" -ForegroundColor Green
} else {
    Write-Host "  ⚠ Database Migrations (run manually)" -ForegroundColor Yellow
}

if ($serviceConfigured) {
    Write-Host "  ✓ Python API Service" -ForegroundColor Green
} else {
    Write-Host "  ⚠ Python API Service (configure manually)" -ForegroundColor Yellow
}

Write-Host "`nInstallation Directory:" -ForegroundColor Yellow
Write-Host "  $InstallPath" -ForegroundColor White

Write-Host "`nAPI Endpoints:" -ForegroundColor Yellow
Write-Host "  .NET Auth API: http://localhost:5000" -ForegroundColor White
Write-Host "  Python Tender API: http://localhost:8000" -ForegroundColor White
Write-Host "  API Docs: http://localhost:8000/api/v1/docs" -ForegroundColor White

if (-not $migrationsRun) {
    Write-Host "`n⚠ To run migrations manually:" -ForegroundColor Yellow
    Write-Host "  1. Open PowerShell as Administrator" -ForegroundColor White
    Write-Host "  2. Run: $InstallPath\Scripts\Run-DatabaseMigrations.ps1" -ForegroundColor White
}

if (-not $serviceConfigured) {
    Write-Host "`n⚠ To configure service manually:" -ForegroundColor Yellow
    Write-Host "  1. Open PowerShell as Administrator" -ForegroundColor White
    Write-Host "  2. Run: $InstallPath\Scripts\Configure-PythonAPI-Service.ps1" -ForegroundColor White
}

Write-Host "`n========================================" -ForegroundColor Green
if ($overallSuccess) {
    Write-Host "Installation Completed Successfully!" -ForegroundColor Green
} else {
    Write-Host "Installation Completed with Warnings" -ForegroundColor Yellow
}
Write-Host "========================================" -ForegroundColor Green

return $overallSuccess
