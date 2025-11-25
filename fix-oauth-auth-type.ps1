# Fix OAuth authenticationType in database

$query = @"
UPDATE EmailConfigurations
SET AuthenticationType = 1
WHERE FromEmail = 'marketing@oryggitech.com';

SELECT
    Id,
    FromEmail,
    AuthenticationType,
    CASE WHEN OAuthAccessToken IS NULL THEN 'NULL' ELSE 'HAS_TOKEN' END as HasToken,
    OAuthTokenExpiresAt,
    OAuthClientId
FROM EmailConfigurations
WHERE FromEmail = 'marketing@oryggitech.com';
"@

Write-Host "Updating authenticationType to OAuth 2.0 (value = 1)..." -ForegroundColor Yellow

try {
    $result = Invoke-Sqlcmd -ServerInstance "PRANA-ASUS\SQLEXPRESS" -Database "ComplaintManagementDb" -Query $query

    Write-Host "`n✅ Update successful!" -ForegroundColor Green
    Write-Host "`nCurrent configuration:" -ForegroundColor Cyan
    $result | Format-Table -AutoSize

    Write-Host "`nVerification:" -ForegroundColor Yellow
    if ($result.AuthenticationType -eq 1) {
        Write-Host "  ✅ AuthenticationType is now set to 1 (OAuth 2.0)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ AuthenticationType is $($result.AuthenticationType) (expected 1)" -ForegroundColor Red
    }

    if ($result.HasToken -eq "HAS_TOKEN") {
        Write-Host "  ✅ OAuth access token exists" -ForegroundColor Green

        $expiryDate = [DateTime]::Parse($result.OAuthTokenExpiresAt)
        $now = Get-Date

        if ($expiryDate -gt $now) {
            $timeLeft = $expiryDate - $now
            Write-Host "  ✅ Token is valid for $($timeLeft.Days) days, $($timeLeft.Hours) hours" -ForegroundColor Green
            Write-Host "  → UI will show: 'OAuth 2.0 - Authorized' (green badge)" -ForegroundColor Cyan
        } else {
            $timeAgo = $now - $expiryDate
            Write-Host "  ⚠️  Token expired $($timeAgo.Days) days, $($timeAgo.Hours) hours ago" -ForegroundColor Yellow
            Write-Host "  → UI will show: 'OAuth 2.0 - Expired' (red badge)" -ForegroundColor Cyan
            Write-Host "  → Action button: 'Refresh OAuth'" -ForegroundColor Cyan
        }
    } else {
        Write-Host "  ℹ️  No OAuth access token found" -ForegroundColor Yellow

        if ($result.OAuthClientId) {
            Write-Host "  → OAuth credentials configured" -ForegroundColor Cyan
            Write-Host "  → UI will show: 'OAuth 2.0 - Pending' (orange pulsing badge)" -ForegroundColor Cyan
            Write-Host "  → Action button: 'Authorize Now'" -ForegroundColor Cyan
        } else {
            Write-Host "  → OAuth credentials NOT configured" -ForegroundColor Yellow
            Write-Host "  → UI will show: 'OAuth 2.0 - Not Configured' (gray badge)" -ForegroundColor Cyan
        }
    }

    Write-Host "`n📝 Next Steps:" -ForegroundColor Magenta
    Write-Host "  1. Refresh the Email Ticketing Configuration page in your browser" -ForegroundColor White
    Write-Host "  2. The badge should now display the correct OAuth status" -ForegroundColor White
    Write-Host "  3. Click Authorize Now or Refresh OAuth to complete authorization" -ForegroundColor White

} catch {
    Write-Host "`n❌ Error updating database:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
