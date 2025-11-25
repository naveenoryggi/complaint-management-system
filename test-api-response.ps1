$token = Get-Content '.admin-token' -Raw
$configId = "4a1b41ef-cbc5-4858-a6a5-02b1c147a80a"

$headers = @{
    "Authorization" = "Bearer $token"
}

Write-Host "Fetching email configuration from API..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/email-configuration/$configId" -Headers $headers -Method Get

    if ($response.isSuccess -and $response.data) {
        $config = $response.data
        Write-Host "`n=== API Response Data ===" -ForegroundColor Yellow
        Write-Host "Authentication Type: $($config.authenticationType)"
        Write-Host "OAuth Token Expires At: $($config.oAuthTokenExpiresAt)"
        Write-Host "From Email: $($config.fromEmail)"
        Write-Host "Last Polled: $($config.lastPolledAt)"

        # Calculate if expired
        $expiryDate = [DateTime]::Parse($config.oAuthTokenExpiresAt)
        $now = [DateTime]::UtcNow
        $minutesRemaining = ($expiryDate - $now).TotalMinutes

        Write-Host "`n=== Token Analysis ===" -ForegroundColor Yellow
        Write-Host "Current UTC Time: $now"
        Write-Host "Token Expires (UTC): $expiryDate"
        Write-Host "Minutes Remaining: $([Math]::Round($minutesRemaining, 1))"
        Write-Host "Status: $(if ($minutesRemaining -gt 0) { 'VALID ✓' } else { 'EXPIRED ✗' })" -ForegroundColor $(if ($minutesRemaining -gt 0) { 'Green' } else { 'Red' })
    } else {
        Write-Host "API returned unsuccessful response" -ForegroundColor Red
        Write-Host ($response | ConvertTo-Json -Depth 5)
    }
} catch {
    Write-Host "Error calling API: $($_.Exception.Message)" -ForegroundColor Red
}
