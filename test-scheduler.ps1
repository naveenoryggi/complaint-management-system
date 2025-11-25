# =====================================================
# AUTONOMOUS TEST SCHEDULER
# Runs tests on a schedule without user interaction
# =====================================================

param(
    [ValidateSet("Hourly", "Every6Hours", "Daily", "Weekly")]
    [string]$Schedule = "Every6Hours"
)

$ErrorActionPreference = "Continue"

Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "AUTONOMOUS TEST SCHEDULER" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "Schedule: $Schedule" -ForegroundColor Yellow
Write-Host "Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Yellow
Write-Host "======================================================" -ForegroundColor Cyan

# Calculate interval based on schedule
$intervalSeconds = switch ($Schedule) {
    "Hourly" { 3600 }
    "Every6Hours" { 21600 }
    "Daily" { 86400 }
    "Weekly" { 604800 }
}

$runCount = 0

while ($true) {
    $runCount++
    Write-Host "`n[RUN #$runCount] Starting test execution at $(Get-Date -Format 'HH:mm:ss')..." -ForegroundColor Green

    # Run the master test suite
    try {
        & ".\automated-test-master.ps1"
        Write-Host "[RUN #$runCount] Tests completed successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "[RUN #$runCount] Test execution failed: $_" -ForegroundColor Red
    }

    # Calculate next run time
    $nextRun = (Get-Date).AddSeconds($intervalSeconds)
    Write-Host "`nNext test run scheduled for: $($nextRun.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Yellow
    Write-Host "Sleeping for $($intervalSeconds / 3600) hours...`n" -ForegroundColor Gray

    # Sleep until next run
    Start-Sleep -Seconds $intervalSeconds
}
