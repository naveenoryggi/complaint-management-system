# Configure Python API as Windows Service
# This script sets up the Python API to run as a Windows service

param(
    [string]$InstallPath = "$env:ProgramFiles\ComplaintManagement",
    [int]$Port = 8000
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Configuring Python API Windows Service" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$pythonExe = "$InstallPath\Python\python.exe"
$apiPath = "$InstallPath\backend\python-api"
$serviceName = "ComplaintManagementTenderAPI"
$serviceDisplayName = "Complaint Management - Tender API"

# Check if Python is installed
if (-not (Test-Path $pythonExe)) {
    Write-Host "Python not found at: $pythonExe" -ForegroundColor Red
    Write-Host "Please run installer first" -ForegroundColor Yellow
    exit 1
}

try {
    # Install dependencies
    Write-Host "`nInstalling Python dependencies..." -ForegroundColor Yellow
    Push-Location $apiPath
    & $pythonExe -m pip install --upgrade pip 2>&1 | Out-Null
    & $pythonExe -m pip install -r requirements.txt 2>&1 | Out-Null
    Pop-Location
    Write-Host "✓ Dependencies installed" -ForegroundColor Green

    # Create .env file if doesn't exist
    $envFile = Join-Path $apiPath ".env"
    if (-not (Test-Path $envFile)) {
        Write-Host "`nCreating .env configuration..." -ForegroundColor Yellow
        Copy-Item "$apiPath\.env.example" $envFile
        Write-Host "✓ Created .env file" -ForegroundColor Green
    }

    # Create uploads directory
    $uploadsDir = Join-Path $apiPath "uploads"
    if (-not (Test-Path $uploadsDir)) {
        New-Item -ItemType Directory -Path $uploadsDir -Force | Out-Null
        Write-Host "✓ Created uploads directory" -ForegroundColor Green
    }

    # Create startup script for service
    $startupScript = @"
@echo off
cd /d "$apiPath"
"$pythonExe" -m uvicorn app.main:app --host 0.0.0.0 --port $Port --workers 4
"@

    $startupScriptPath = Join-Path $apiPath "start-service.bat"
    $startupScript | Set-Content $startupScriptPath
    Write-Host "✓ Created startup script" -ForegroundColor Green

    # Check if service exists
    $existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

    if ($existingService) {
        Write-Host "`nService already exists. Stopping and removing..." -ForegroundColor Yellow
        Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
        sc.exe delete $serviceName | Out-Null
        Write-Host "✓ Removed existing service" -ForegroundColor Green
    }

    # Note: Windows services require third-party tools like NSSM for Python apps
    # For now, we'll create a scheduled task that runs at startup

    Write-Host "`nCreating startup task..." -ForegroundColor Yellow

    $taskName = "ComplaintManagement-TenderAPI"
    $taskDescription = "Tender Automation Python API for Complaint Management"

    # Remove existing task if exists
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue

    # Create task action
    $action = New-ScheduledTaskAction -Execute $pythonExe `
        -Argument "-m uvicorn app.main:app --host 0.0.0.0 --port $Port --workers 4" `
        -WorkingDirectory $apiPath

    # Create task trigger (at startup)
    $trigger = New-ScheduledTaskTrigger -AtStartup

    # Create task settings
    $settings = New-ScheduledTaskSettingsSet `
        -AllowStartIfOnBatteries `
        -DontStopIfGoingOnBatteries `
        -StartWhenAvailable `
        -RunOnlyIfNetworkAvailable `
        -RestartCount 3 `
        -RestartInterval (New-TimeSpan -Minutes 1)

    # Create task principal (run as SYSTEM)
    $principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest

    # Register the task
    Register-ScheduledTask `
        -TaskName $taskName `
        -Description $taskDescription `
        -Action $action `
        -Trigger $trigger `
        -Settings $settings `
        -Principal $principal `
        -Force | Out-Null

    Write-Host "✓ Created startup task: $taskName" -ForegroundColor Green

    # Start the task now
    Write-Host "`nStarting Python API..." -ForegroundColor Yellow
    Start-ScheduledTask -TaskName $taskName

    Start-Sleep -Seconds 5

    # Check if API is responding
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$Port/health" -TimeoutSec 5 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ Python API is running and healthy" -ForegroundColor Green
            Write-Host "`nAPI is available at:" -ForegroundColor Cyan
            Write-Host "  - API: http://localhost:$Port" -ForegroundColor White
            Write-Host "  - Docs: http://localhost:$Port/api/v1/docs" -ForegroundColor White
        }
    } catch {
        Write-Host "⚠ API may still be starting up. Check logs in a moment." -ForegroundColor Yellow
    }

    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Python API Service Configured!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "`nThe API will start automatically on system boot." -ForegroundColor Cyan
    Write-Host "`nTo manage the service:" -ForegroundColor Yellow
    Write-Host "  - Start: Start-ScheduledTask -TaskName '$taskName'" -ForegroundColor White
    Write-Host "  - Stop: Stop-ScheduledTask -TaskName '$taskName'" -ForegroundColor White
    Write-Host "  - Status: Get-ScheduledTask -TaskName '$taskName'" -ForegroundColor White

    return $true

} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "Python API Service Configuration Failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    return $false
}
