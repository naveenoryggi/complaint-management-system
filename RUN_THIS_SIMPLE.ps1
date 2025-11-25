# SIMPLE ONE-COMMAND AUTOMATION
# Run this to fix everything automatically

Write-Host "╔═══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  OAUTH AUTOMATION - SIMPLE VERSION                   ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Step 1: Fix Database via SQL
Write-Host "[1/2] Fixing database..." -ForegroundColor Yellow
Write-Host "Server: LAPTOP-NF9BTG7Q\SQLEXPRESS" -ForegroundColor Gray
Write-Host "Database: ComplaintManagementDB" -ForegroundColor Gray
Write-Host ""

$fixSQL = "UPDATE EmailConfigurations SET AuthenticationType = 1 WHERE Id = '4A1B41EF-CBC5-4858-A6A5-02B1C147A80A';"

try {
    sqlcmd -S "LAPTOP-NF9BTG7Q\SQLEXPRESS" -d "ComplaintManagementDB" -Q $fixSQL -b
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Database fixed successfully!" -ForegroundColor Green
    } else {
        Write-Host "✗ Database fix failed (exit code: $LASTEXITCODE)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error running SQL: $_" -ForegroundColor Red
}

Write-Host ""

# Step 2: Verify the fix
Write-Host "[2/2] Verifying..." -ForegroundColor Yellow

$verifySQL = @"
SELECT
    'Email: ' + FromEmail as Info,
    'AuthType: ' + CASE AuthenticationType WHEN 0 THEN 'Basic' WHEN 1 THEN 'OAuth' ELSE 'INVALID' END as Status
FROM EmailConfigurations
WHERE Id = '4A1B41EF-CBC5-4858-A6A5-02B1C147A80A';
"@

try {
    $result = sqlcmd -S "LAPTOP-NF9BTG7Q\SQLEXPRESS" -d "ComplaintManagementDB" -Q $verifySQL -h -1 -W
    Write-Host ""
    Write-Host "Current Status:" -ForegroundColor Cyan
    $result | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
    Write-Host ""

    if ($result -match "OAuth") {
        Write-Host "✓ SUCCESS! Database is now set to OAuth 2.0" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next Steps:" -ForegroundColor Cyan
        Write-Host "1. Open browser: http://localhost:4200" -ForegroundColor White
        Write-Host "2. Login: admin@complaintmanagement.com / Admin@123" -ForegroundColor White
        Write-Host "3. Go to: Admin Panel → Communication Settings → Email Ticketing" -ForegroundColor White
        Write-Host "4. Badge should show: OAuth 2.0 - Pending (orange)" -ForegroundColor White
        Write-Host ""
        Write-Host "Then follow: 10_MINUTE_OAUTH_SETUP.md" -ForegroundColor Yellow
    } else {
        Write-Host "! Still shows Basic Auth - may need manual fix" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Verification failed: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Done! Check status above." -ForegroundColor White
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
