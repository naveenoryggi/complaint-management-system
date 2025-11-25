Write-Host "Fixing OAuth Authentication Type via API..." -ForegroundColor Cyan

# Get fresh token
$token = Get-Content ".oauth-fix-token" -Raw | ForEach-Object { $_.Trim() }
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$configId = "4a1b41ef-cbc5-4858-a6a5-02b1c147a80a"

try {
    # Get current configuration
    Write-Host "Fetching current configuration..." -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/email-configuration/$configId" -Headers $headers -Method Get

    if ($response.isSuccess) {
        $config = $response.data

        Write-Host "Current Authentication Type: $($config.authenticationType)" -ForegroundColor Cyan

        # Update authentication type to OAuth2 (2)
        $config.authenticationType = 2

        Write-Host "Updating to OAuth2 (value = 2)..." -ForegroundColor Yellow

        # Send update request
        $updateResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/email-configuration/$configId" `
            -Headers $headers `
            -Method PUT `
            -Body ($config | ConvertTo-Json -Depth 10)

        if ($updateResponse.isSuccess) {
            Write-Host "`nSUCCESS: Configuration updated!" -ForegroundColor Green
            Write-Host "Authentication Type: $($updateResponse.data.authenticationType) (OAuth2)" -ForegroundColor Green
            Write-Host "`n================================================" -ForegroundColor Yellow
            Write-Host "OAuth authentication type has been fixed!" -ForegroundColor Green
            Write-Host "You can now use the Re-authorize button" -ForegroundColor Green
            Write-Host "================================================" -ForegroundColor Yellow
        } else {
            Write-Host "ERROR: $($updateResponse.message)" -ForegroundColor Red
        }
    } else {
        Write-Host "ERROR: Failed to fetch configuration - $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    }
}
