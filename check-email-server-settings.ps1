# Check EmailServerSettings Configuration

$token = Get-Content ".working-token" -Raw
Write-Host "Checking EmailServerSettings..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/email-server-settings" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($response.isSuccess) {
        Write-Host "`nEmailServerSettings found: $($response.data.Count) records" -ForegroundColor Green
        $response.data | Select-Object Id, Name, Host, Port, IsActive, IsDefault, FromEmail | Format-Table -AutoSize
    } else {
        Write-Host "No EmailServerSettings configured: $($response.message)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "EmailServerSettings API not found or error: $_" -ForegroundColor Red
    Write-Host "`nThis might mean EmailServerSettings table doesn't exist or API endpoint is missing" -ForegroundColor Yellow
}
