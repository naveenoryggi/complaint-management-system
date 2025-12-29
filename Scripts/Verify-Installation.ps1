# Verify-Installation.ps1
# Checks if the Complaint Management System is running correctly
# Shows status of API and Frontend to the user

param(
    [string]$Hostname = "localhost",
    [int]$WebPort = 80,
    [int]$ApiPort = 5000,
    [switch]$IsIIS
)

$ErrorActionPreference = "Continue"

Clear-Host
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Installation Verification                " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Hostname: $Hostname"
Write-Host "Web Port: $WebPort"
Write-Host "API Port: $ApiPort"
Write-Host "Mode: $(if ($IsIIS) { 'IIS' } else { 'Windows Service' })"
Write-Host ""

$allGood = $true

# ============================================================
# CHECK 1: API Health Check
# ============================================================
Write-Host "Step 1/4: Checking API Service..." -ForegroundColor Yellow

if ($IsIIS) {
    $apiUrl = "http://${Hostname}:${WebPort}/api/health"
} else {
    $apiUrl = "http://${Hostname}:${ApiPort}/api/health"
}

Write-Host "  Testing: $apiUrl" -ForegroundColor Gray

try {
    $response = Invoke-WebRequest -Uri $apiUrl -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
    if ($response.StatusCode -eq 200) {
        Write-Host "  [OK] API is running and responding!" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] API responded with status: $($response.StatusCode)" -ForegroundColor Yellow
        $allGood = $false
    }
} catch {
    Write-Host "  [FAIL] API is not responding" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Gray
    $allGood = $false

    # Check if it's a Windows Service
    if (-not $IsIIS) {
        Write-Host "  Checking Windows Service status..." -ForegroundColor Yellow
        $service = Get-Service -Name "ComplaintManagementAPI" -ErrorAction SilentlyContinue
        if ($service) {
            Write-Host "  Service Status: $($service.Status)" -ForegroundColor $(if ($service.Status -eq 'Running') { 'Green' } else { 'Red' })
        } else {
            Write-Host "  Service not found. May not be installed." -ForegroundColor Red
        }
    }
}

Write-Host ""

# ============================================================
# CHECK 2: Frontend Files Check
# ============================================================
Write-Host "Step 2/4: Checking Frontend Files..." -ForegroundColor Yellow

if ($IsIIS) {
    $frontendUrl = "http://${Hostname}:${WebPort}/"
} else {
    $frontendUrl = "http://${Hostname}:${ApiPort}/"
}

Write-Host "  Testing: $frontendUrl" -ForegroundColor Gray

try {
    $response = Invoke-WebRequest -Uri $frontendUrl -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
    if ($response.StatusCode -eq 200 -and $response.Content -like "*<app-root>*") {
        Write-Host "  [OK] Frontend is accessible and Angular app detected!" -ForegroundColor Green
    } elseif ($response.StatusCode -eq 200) {
        Write-Host "  [OK] Frontend is accessible!" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] Frontend responded with status: $($response.StatusCode)" -ForegroundColor Yellow
        $allGood = $false
    }
} catch {
    Write-Host "  [WARN] Frontend may not be accessible yet" -ForegroundColor Yellow
    Write-Host "  (This is normal if using Windows Service - frontend served by API)" -ForegroundColor Gray
}

Write-Host ""

# ============================================================
# CHECK 3: Database Connection Check
# ============================================================
Write-Host "Step 3/4: Checking API can reach database..." -ForegroundColor Yellow

if ($IsIIS) {
    $loginUrl = "http://${Hostname}:${WebPort}/api/auth/login"
} else {
    $loginUrl = "http://${Hostname}:${ApiPort}/api/auth/login"
}

Write-Host "  Testing: $loginUrl" -ForegroundColor Gray

try {
    # Try to hit the login endpoint (will fail auth but confirms API + DB working)
    $body = @{
        username = "test"
        password = "test"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri $loginUrl -Method POST -Body $body -ContentType "application/json" -UseBasicParsing -TimeoutSec 10 -ErrorAction SilentlyContinue
} catch {
    $statusCode = $_.Exception.Response.StatusCode.Value__
    if ($statusCode -eq 401 -or $statusCode -eq 400) {
        Write-Host "  [OK] API login endpoint working (auth correctly rejected test creds)" -ForegroundColor Green
    } elseif ($statusCode -eq 500) {
        Write-Host "  [FAIL] API returned 500 - database may not be connected" -ForegroundColor Red
        $allGood = $false
    } else {
        Write-Host "  [WARN] API returned status: $statusCode" -ForegroundColor Yellow
    }
}

Write-Host ""

# ============================================================
# CHECK 4: IIS Status (if applicable)
# ============================================================
if ($IsIIS) {
    Write-Host "Step 4/4: Checking IIS Website..." -ForegroundColor Yellow

    try {
        Import-Module WebAdministration -ErrorAction SilentlyContinue
        $site = Get-Website -Name "ComplaintManagement" -ErrorAction SilentlyContinue
        if ($site) {
            Write-Host "  Website: ComplaintManagement" -ForegroundColor Gray
            Write-Host "  Status: $($site.State)" -ForegroundColor $(if ($site.State -eq 'Started') { 'Green' } else { 'Red' })
            if ($site.State -ne 'Started') {
                $allGood = $false
            }
        } else {
            Write-Host "  [WARN] IIS Website 'ComplaintManagement' not found" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  [SKIP] Could not check IIS status" -ForegroundColor Gray
    }
} else {
    Write-Host "Step 4/4: Checking Windows Service..." -ForegroundColor Yellow

    $service = Get-Service -Name "ComplaintManagementAPI" -ErrorAction SilentlyContinue
    if ($service) {
        Write-Host "  Service: ComplaintManagementAPI" -ForegroundColor Gray
        Write-Host "  Status: $($service.Status)" -ForegroundColor $(if ($service.Status -eq 'Running') { 'Green' } else { 'Red' })
        if ($service.Status -ne 'Running') {
            $allGood = $false
        }
    } else {
        Write-Host "  [WARN] Windows Service 'ComplaintManagementAPI' not found" -ForegroundColor Yellow
        $allGood = $false
    }
}

Write-Host ""

# ============================================================
# SUMMARY
# ============================================================
Write-Host "============================================" -ForegroundColor $(if ($allGood) { 'Green' } else { 'Yellow' })
if ($allGood) {
    Write-Host "  INSTALLATION SUCCESSFUL!                 " -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Default Login Credentials:" -ForegroundColor Cyan
    Write-Host "  Admin: admin@complaintmanagement.com / Admin@123" -ForegroundColor White
} else {
    Write-Host "  INSTALLATION COMPLETED WITH WARNINGS     " -ForegroundColor Yellow
    Write-Host "============================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Some checks failed. The application may need:" -ForegroundColor Yellow
    Write-Host "  - A few more seconds to fully start" -ForegroundColor Gray
    Write-Host "  - Database connection to be configured" -ForegroundColor Gray
    Write-Host "  - Windows Service to be started manually" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Default Login Credentials:" -ForegroundColor Cyan
    Write-Host "  Admin: admin@complaintmanagement.com / Admin@123" -ForegroundColor White
}

Write-Host ""
Write-Host "Application URL:" -ForegroundColor Cyan
if ($IsIIS) {
    if ($WebPort -eq 80) {
        Write-Host "  http://$Hostname" -ForegroundColor White
    } else {
        Write-Host "  http://${Hostname}:${WebPort}" -ForegroundColor White
    }
} else {
    Write-Host "  http://${Hostname}:${ApiPort}" -ForegroundColor White
}

Write-Host ""
Write-Host "Press any key to close..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

exit 0
