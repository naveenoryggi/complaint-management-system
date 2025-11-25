# Quick Fix: Re-Authorize OAuth Token for Email Ticketing
# This script opens the OAuth authorization URL to refresh the expired token

Write-Host "=== Email Ticketing OAuth Token Refresh ===" -ForegroundColor Cyan
Write-Host ""

# Configuration ID from database
$configId = "4A1B41EF-CBC5-4858-A6A5-02B1C147A80A"

Write-Host "Checking current token status..." -ForegroundColor Yellow
Write-Host ""

# Check current status
try {
    $result = sqlcmd -S '(local)\SQLEXPRESS' -d ComplaintManagementDB -Q "
        SELECT
            FromEmail,
            CASE
                WHEN OAuthTokenExpiresAt > GETDATE() THEN 'VALID'
                ELSE 'EXPIRED'
            END as Status,
            OAuthTokenExpiresAt
        FROM EmailConfigurations
        WHERE Id = '$configId' AND IsDeleted = 0
    " -h -1 -W

    Write-Host "Current Status:" -ForegroundColor Cyan
    Write-Host $result
    Write-Host ""
} catch {
    Write-Host "Warning: Could not query database status" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "Opening OAuth authorization page..." -ForegroundColor Yellow
Write-Host ""

# Open OAuth authorization URL
$authUrl = "http://localhost:5000/api/oauth/authorize/$configId"
Write-Host "Authorization URL: $authUrl" -ForegroundColor White
Write-Host ""

Start-Process $authUrl

Write-Host "INSTRUCTIONS:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Browser will open with Microsoft login" -ForegroundColor White
Write-Host "2. Sign in with: marketing@oryggitech.com" -ForegroundColor Cyan
Write-Host "3. Grant the requested permissions" -ForegroundColor White
Write-Host "4. You'll be redirected back with 'Authorization successful'" -ForegroundColor White
Write-Host "5. Token will be saved automatically" -ForegroundColor White
Write-Host ""
Write-Host "Press Enter after completing authorization..." -ForegroundColor Yellow
Read-Host

Write-Host ""
Write-Host "Verifying new token..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $result = sqlcmd -S '(local)\SQLEXPRESS' -d ComplaintManagementDB -Q "
        SELECT
            FromEmail,
            CASE
                WHEN OAuthTokenExpiresAt > GETDATE() THEN 'VALID ✓'
                ELSE 'EXPIRED ✗'
            END as Status,
            OAuthTokenExpiresAt as [Expires At],
            DATEDIFF(DAY, GETDATE(), OAuthTokenExpiresAt) as [Days Until Expiry]
        FROM EmailConfigurations
        WHERE Id = '$configId' AND IsDeleted = 0
    " -W

    Write-Host ""
    Write-Host "Updated Status:" -ForegroundColor Green
    Write-Host $result
    Write-Host ""

    # Check if token is now valid
    $statusCheck = sqlcmd -S '(local)\SQLEXPRESS' -d ComplaintManagementDB -Q "
        SELECT CASE WHEN OAuthTokenExpiresAt > GETDATE() THEN 1 ELSE 0 END
        FROM EmailConfigurations
        WHERE Id = '$configId'
    " -h -1

    if ($statusCheck -match "1") {
        Write-Host "✓ SUCCESS: OAuth token refreshed successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Email ticketing is now active and will poll every 5 minutes" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Test it by:" -ForegroundColor Yellow
        Write-Host "  1. Send an email to: marketing@oryggitech.com" -ForegroundColor White
        Write-Host "  2. Wait 5 minutes for automatic polling" -ForegroundColor White
        Write-Host "  3. Or trigger manual poll in Admin UI" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host "⚠ Token still expired. Please try again or check:" -ForegroundColor Yellow
        Write-Host "  - Azure AD app registration is active" -ForegroundColor White
        Write-Host "  - Correct Microsoft account used" -ForegroundColor White
        Write-Host "  - All permissions granted" -ForegroundColor White
        Write-Host ""
    }

} catch {
    Write-Host "Could not verify token status: $_" -ForegroundColor Red
    Write-Host ""
}

Write-Host "=== Done ===" -ForegroundColor Green
Write-Host ""
Write-Host "Access Email Ticketing Config:" -ForegroundColor Cyan
Write-Host "http://localhost:4200" -ForegroundColor White
Write-Host "→ Admin Panel → Communication Settings → Email Ticketing Config" -ForegroundColor White
Write-Host ""
