# ═══════════════════════════════════════════════════════════════════════════
#  COMPLETE OAUTH AUTOMATION SCRIPT
#  Run this single command to fix everything automatically
# ═══════════════════════════════════════════════════════════════════════════

param(
    [switch]$SkipServerCheck,
    [switch]$SkipPlaywright
)

$ErrorActionPreference = "Continue"
$Global:StepsPassed = 0
$Global:StepsFailed = 0

function Write-Step {
    param([string]$Message, [string]$Color = "Cyan")
    Write-Host "`n$Message" -ForegroundColor $Color
    Write-Host ("=" * $Message.Length) -ForegroundColor $Color
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
    $Global:StepsPassed++
}

function Write-Failure {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
    $Global:StepsFailed++
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Yellow
}

# Clear screen for clean output
Clear-Host

Write-Host @"
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║           OAUTH COMPLETE AUTOMATION SCRIPT                        ║
║           Fix Everything with One Command                         ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

Write-Host ""
Write-Host "This script will:" -ForegroundColor White
Write-Host "  1. Check and start servers if needed" -ForegroundColor Gray
Write-Host "  2. Fix database via SQL command line" -ForegroundColor Gray
Write-Host "  3. Verify UI with Playwright automation" -ForegroundColor Gray
Write-Host "  4. Generate comprehensive report" -ForegroundColor Gray
Write-Host ""

Start-Sleep -Seconds 2

# ═══════════════════════════════════════════════════════════════════════════
# STEP 1: CHECK SERVERS
# ═══════════════════════════════════════════════════════════════════════════

if (-not $SkipServerCheck) {
    Write-Step "[1/4] Checking Servers"

    # Check backend
    Write-Host "Checking backend (http://localhost:5000)..." -ForegroundColor Yellow
    try {
        $backendResponse = Invoke-WebRequest -Uri "http://localhost:5000" -TimeoutSec 3 -UseBasicParsing -ErrorAction SilentlyContinue
        Write-Success "Backend is running"
    } catch {
        Write-Failure "Backend is not running"
        Write-Info "Starting backend server..."

        $backendPath = "complaint-system-dotnet\src\ComplaintManagement.API"
        if (Test-Path $backendPath) {
            Start-Process cmd -ArgumentList "/k", "cd `"$backendPath`" && dotnet run" -WindowStyle Normal
            Write-Info "Backend server starting in new window..."
            Write-Info "Waiting 30 seconds for startup..."
            Start-Sleep -Seconds 30
        } else {
            Write-Failure "Backend path not found: $backendPath"
        }
    }

    # Check frontend
    Write-Host "Checking frontend (http://localhost:4200)..." -ForegroundColor Yellow
    try {
        $frontendResponse = Invoke-WebRequest -Uri "http://localhost:4200" -TimeoutSec 3 -UseBasicParsing -ErrorAction SilentlyContinue
        Write-Success "Frontend is running"
    } catch {
        Write-Failure "Frontend is not running"
        Write-Info "Starting frontend server..."

        $frontendPath = "complaint-system-angular"
        if (Test-Path $frontendPath) {
            Start-Process cmd -ArgumentList "/k", "cd `"$frontendPath`" && npm start" -WindowStyle Normal
            Write-Info "Frontend server starting in new window..."
            Write-Info "Waiting 30 seconds for startup..."
            Start-Sleep -Seconds 30
        } else {
            Write-Failure "Frontend path not found: $frontendPath"
        }
    }

    Write-Host ""
} else {
    Write-Info "Skipping server check (--SkipServerCheck flag set)"
}

# ═══════════════════════════════════════════════════════════════════════════
# STEP 2: FIX DATABASE VIA SQL COMMAND LINE
# ═══════════════════════════════════════════════════════════════════════════

Write-Step "[2/4] Fixing Database via SQL Command Line"

if (Test-Path "fix-database-automated.ps1") {
    try {
        Write-Host "Running automated SQL fix..." -ForegroundColor Yellow
        & ".\fix-database-automated.ps1"

        if ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq $null) {
            Write-Success "Database fix completed successfully"
        } else {
            Write-Failure "Database fix encountered issues (exit code: $LASTEXITCODE)"
        }
    } catch {
        Write-Failure "Database fix script failed: $_"
    }
} else {
    Write-Failure "Database fix script not found: fix-database-automated.ps1"
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# STEP 3: VERIFY UI WITH PLAYWRIGHT
# ═══════════════════════════════════════════════════════════════════════════

if (-not $SkipPlaywright) {
    Write-Step "[3/4] Verifying UI with Playwright Automation"

    # Check if playwright is installed
    if (-not (Test-Path "node_modules\playwright")) {
        Write-Info "Playwright not installed. Installing..."
        try {
            npm install playwright
            Write-Success "Playwright installed"
        } catch {
            Write-Failure "Failed to install Playwright: $_"
            $SkipPlaywright = $true
        }
    }

    if (-not $SkipPlaywright -and (Test-Path "verify-oauth-ui-playwright.js")) {
        try {
            Write-Host "Running Playwright UI verification..." -ForegroundColor Yellow
            Write-Host "(Browser window will open - this is automated, don't interact)" -ForegroundColor Gray
            Write-Host ""

            node verify-oauth-ui-playwright.js

            if ($LASTEXITCODE -eq 0) {
                Write-Success "UI verification passed"
            } else {
                Write-Failure "UI verification failed (exit code: $LASTEXITCODE)"
            }
        } catch {
            Write-Failure "Playwright verification failed: $_"
        }
    } else {
        if (-not (Test-Path "verify-oauth-ui-playwright.js")) {
            Write-Failure "Playwright script not found: verify-oauth-ui-playwright.js"
        }
    }
} else {
    Write-Info "Skipping Playwright verification (--SkipPlaywright flag set)"
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# STEP 4: GENERATE FINAL REPORT
# ═══════════════════════════════════════════════════════════════════════════

Write-Step "[4/4] Generating Final Report"

$reportContent = @"
# Automated OAuth Setup - Execution Report

**Date:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
**Script:** AUTOMATE_EVERYTHING.ps1

## Execution Summary

- **Steps Passed:** $Global:StepsPassed
- **Steps Failed:** $Global:StepsFailed
- **Overall Status:** $(if ($Global:StepsFailed -eq 0) { "✅ SUCCESS" } else { "⚠️ PARTIAL SUCCESS" })

## Steps Executed

1. **Server Check:** $(if (-not $SkipServerCheck) { "Executed" } else { "Skipped" })
2. **Database Fix:** Executed via SQL command line
3. **UI Verification:** $(if (-not $SkipPlaywright) { "Executed with Playwright" } else { "Skipped" })
4. **Report Generation:** Completed

## Database Status

Run this query to verify:
``````sql
SELECT
    FromEmail,
    AuthenticationType,
    CASE AuthenticationType
        WHEN 0 THEN 'Basic Auth'
        WHEN 1 THEN 'OAuth 2.0'
        ELSE 'INVALID'
    END as AuthType,
    IsEnabled
FROM EmailConfigurations
WHERE Id = '4A1B41EF-CBC5-4858-A6A5-02B1C147A80A';
``````

Expected: AuthenticationType = 1 (OAuth 2.0)

## UI Verification

$(if (-not $SkipPlaywright) {
@"
Screenshots saved in: .playwright-oauth-verification/

Check these files:
- 01-login-page.png
- 02-dashboard.png
- 03-email-config-page.png
- 04-oauth-badge-status.png
- 05-oauth-buttons.png
- verification-results.json
"@
} else {
"Skipped (--SkipPlaywright flag set)"
})

## Next Steps

### If Database Fix Succeeded ✅

1. **Verify in Browser:**
   - Open: http://localhost:4200
   - Login: admin@complaintmanagement.com / Admin@123
   - Navigate to: Admin Panel → Communication Settings → Email Ticketing
   - Expected Badge: 🟠 "OAuth 2.0 - Pending" or 🔴 "OAuth 2.0 - Expired"

2. **Choose Your Path:**

   **Option A: Full OAuth (Recommended)**
   - Follow: `10_MINUTE_OAUTH_SETUP.md` (OAuth section)
   - Time: 40-45 minutes
   - Setup Azure AD, enter credentials, authorize

   **Option B: App Password (Quick)**
   - Follow: `10_MINUTE_OAUTH_SETUP.md` (App Password section)
   - Time: 10 minutes
   - Generate password, enter in UI, done

### If Database Fix Failed ❌

1. Check SQL Server is running
2. Verify connection string: PRANA-ASUS\SQLEXPRESS
3. Check database name: ComplaintManagementDb
4. Run fix manually: `fix-database-automated.ps1`

## Troubleshooting

### Backend Not Running
``````powershell
cd "complaint-system-dotnet\src\ComplaintManagement.API"
dotnet run
``````

### Frontend Not Running
``````powershell
cd complaint-system-angular
npm start
``````

### Database Connection Issues
- Make sure SQL Server is running
- Check Windows Services for SQL Server (SQLEXPRESS)
- Server instance: LAPTOP-NF9BTG7Q\SQLEXPRESS
- Database: ComplaintManagementDB

## Files Generated

- AUTOMATION_REPORT_$(Get-Date -Format "yyyyMMdd_HHmmss").md (this file)
$(if (-not $SkipPlaywright) { "- .playwright-oauth-verification/verification-results.json" } else { "" })
$(if (-not $SkipPlaywright) { "- .playwright-oauth-verification/*.png (screenshots)" } else { "" })

## Documentation Reference

- **`UNBLOCKED_STATUS_REPORT.md`** - Complete system status
- **`10_MINUTE_OAUTH_SETUP.md`** - Step-by-step setup guide
- **`OAUTH_QUICK_START.md`** - Quick reference
- **`AZURE_AD_OAUTH_SETUP_GUIDE.md`** - Azure AD details

---

**Generated by:** AUTOMATE_EVERYTHING.ps1
**Status:** $(if ($Global:StepsFailed -eq 0) { "All automated steps completed successfully ✅" } else { "$Global:StepsFailed step(s) need attention ⚠️" })
"@

$reportFilename = "AUTOMATION_REPORT_$(Get-Date -Format 'yyyyMMdd_HHmmss').md"
$reportContent | Out-File -FilePath $reportFilename -Encoding UTF8

Write-Success "Report generated: $reportFilename"

# ═══════════════════════════════════════════════════════════════════════════
# FINAL SUMMARY
# ═══════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host @"
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║                     AUTOMATION COMPLETE                           ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

Write-Host ""
Write-Host "Summary:" -ForegroundColor White
Write-Host "  Steps Passed: $Global:StepsPassed" -ForegroundColor Green
Write-Host "  Steps Failed: $Global:StepsFailed" -ForegroundColor $(if ($Global:StepsFailed -gt 0) { "Red" } else { "Gray" })
Write-Host ""

if ($Global:StepsFailed -eq 0) {
    Write-Host "✅ All automated steps completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "1. Review report: $reportFilename" -ForegroundColor White
    Write-Host "2. Check UI in browser: http://localhost:4200" -ForegroundColor White
    Write-Host "3. Follow setup guide: 10_MINUTE_OAUTH_SETUP.md" -ForegroundColor White
} else {
    Write-Host "⚠️ Some steps need attention" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Review the report for details: $reportFilename" -ForegroundColor White
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
