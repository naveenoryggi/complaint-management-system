# Check Email Settings (correct endpoint)

$token = Get-Content ".working-token" -Raw
Write-Host "Checking EmailServerSettings via /api/email-settings..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/email-settings" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($response.isSuccess) {
        Write-Host "`nEmailServerSettings found: $($response.data.Count) records" -ForegroundColor Green

        if ($response.data.Count -gt 0) {
            Write-Host "`nEmail Settings:" -ForegroundColor Yellow
            $response.data | Select-Object Id, Name, Host, Port, UseSsl, FromEmail, IsActive, IsDefault | Format-Table -AutoSize
        } else {
            Write-Host "`nNo EmailServerSettings configured yet!" -ForegroundColor Yellow
            Write-Host "This is required for auto-response notifications to work." -ForegroundColor Yellow
        }
    } else {
        Write-Host "Error: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
