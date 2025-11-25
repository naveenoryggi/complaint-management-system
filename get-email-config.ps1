$token = Get-Content ".oauth-fix-token" -Raw
$token = $token.Trim()
$headers = @{
    "Authorization" = "Bearer $token"
}

Write-Host "Fetching email configurations..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/email-configuration" -Headers $headers -Method Get
    Write-Host "SUCCESS: Retrieved configurations" -ForegroundColor Green

    # Extract the data from the Result wrapper
    if ($response.data) {
        $configs = $response.data
        Write-Host "`nFound $($configs.Count) email configurations" -ForegroundColor Cyan

        foreach ($config in $configs) {
            Write-Host "`n================================================" -ForegroundColor Yellow
            Write-Host "Configuration ID: $($config.id)" -ForegroundColor White
            Write-Host "From Email: $($config.fromEmail)" -ForegroundColor Cyan
            Write-Host "Authentication Type: $($config.authenticationType)" -ForegroundColor Cyan
            Write-Host "Is Enabled: $($config.isEnabled)" -ForegroundColor Cyan
            Write-Host "IMAP Host: $($config.imapHost):$($config.imapPort)" -ForegroundColor Cyan
            Write-Host "OAuth Client ID: $($config.oAuthClientId)" -ForegroundColor Cyan
            Write-Host "OAuth Tenant ID: $($config.oAuthTenantId)" -ForegroundColor Cyan
            Write-Host "OAuth Token Expires: $($config.oAuthTokenExpiresAt)" -ForegroundColor Cyan
            Write-Host "Last Polled: $($config.lastPolledAt)" -ForegroundColor Cyan
            Write-Host "================================================" -ForegroundColor Yellow
        }

        # Save full response for debugging
        $response | ConvertTo-Json -Depth 10 | Out-File "email-config-details.json"
        Write-Host "`nFull details saved to email-config-details.json" -ForegroundColor Green
    } else {
        Write-Host "No data in response" -ForegroundColor Yellow
        $response | ConvertTo-Json -Depth 5
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
    }
}
