$token = Get-Content ".oauth-fix-token" -Raw | ForEach-Object { $_.Trim() }
$headers = @{
    "Authorization" = "Bearer $token"
}

Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "OAuth Email Ticketing Status Check" -ForegroundColor Cyan
Write-Host "================================================`n" -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/email-configuration" -Headers $headers -Method Get

    if ($response.isSuccess -and $response.data.Count -gt 0) {
        $config = $response.data[0]

        Write-Host "Configuration Details:" -ForegroundColor Green
        Write-Host "  Email: $($config.fromEmail)" -ForegroundColor White
        Write-Host "  Display Name: $($config.fromName)" -ForegroundColor White
        Write-Host "  Enabled: $($config.isEnabled)" -ForegroundColor $(if ($config.isEnabled) { 'Green' } else { 'Red' })

        Write-Host "`nAuthentication:" -ForegroundColor Yellow
        Write-Host "  Auth Type: $($config.authenticationType) $(if ($config.authenticationType -eq 2) { '(OAuth2 ✓)' } else { '(Basic ✗)' })" -ForegroundColor $(if ($config.authenticationType -eq 2) { 'Green' } else { 'Red' })

        if ($config.oAuthTokenExpiresAt) {
            $expiryDate = [DateTime]::Parse($config.oAuthTokenExpiresAt)
            $isExpired = $expiryDate -lt (Get-Date)

            Write-Host "  Token Expires: $($config.oAuthTokenExpiresAt)" -ForegroundColor $(if ($isExpired) { 'Red' } else { 'Green' })
            Write-Host "  Token Status: $(if ($isExpired) { 'EXPIRED ✗' } else { 'Valid ✓' })" -ForegroundColor $(if ($isExpired) { 'Red' } else { 'Green' })

            if ($isExpired) {
                $hoursSinceExpiry = ((Get-Date) - $expiryDate).TotalHours
                Write-Host "  Expired: $([math]::Round($hoursSinceExpiry, 1)) hours ago" -ForegroundColor Red
            }
        }

        Write-Host "  Has Refresh Token: $(if ($config.oAuthRefreshToken) { 'Yes ✓' } else { 'No ✗' })" -ForegroundColor $(if ($config.oAuthRefreshToken) { 'Green' } else { 'Red' })

        Write-Host "`nServer Settings:" -ForegroundColor Yellow
        Write-Host "  IMAP: $($config.imapHost):$($config.imapPort)" -ForegroundColor White
        Write-Host "  SMTP: $($config.smtpHost):$($config.smtpPort)" -ForegroundColor White
        Write-Host "  Folder: $($config.imapFolder)" -ForegroundColor White

        Write-Host "`nPolling:" -ForegroundColor Yellow
        if ($config.pollingIntervalSeconds) {
            Write-Host "  Interval: $($config.pollingIntervalSeconds) seconds" -ForegroundColor White
        } else {
            Write-Host "  Interval: $($config.pollingIntervalMinutes) minutes" -ForegroundColor White
        }
        Write-Host "  Last Polled: $($config.lastPolledAt)" -ForegroundColor White

        Write-Host "`n================================================" -ForegroundColor Cyan

        if ($config.authenticationType -eq 2 -and $config.oAuthTokenExpiresAt) {
            $expiryDate = [DateTime]::Parse($config.oAuthTokenExpiresAt)
            if ($expiryDate -lt (Get-Date)) {
                Write-Host "ACTION REQUIRED: OAuth Token Expired!" -ForegroundColor Red
                Write-Host "User must complete OAuth authorization flow" -ForegroundColor Yellow
                Write-Host "Navigate to: http://localhost:4200/admin/email-ticketing-config" -ForegroundColor Cyan
                Write-Host "Click 'Re-authorize' button and sign in with:" -ForegroundColor Cyan
                Write-Host "  $($config.fromEmail)" -ForegroundColor White
            } else {
                Write-Host "STATUS: OAuth Configuration Valid" -ForegroundColor Green
            }
        }

        Write-Host "================================================`n" -ForegroundColor Cyan

    } else {
        Write-Host "No email configurations found" -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
