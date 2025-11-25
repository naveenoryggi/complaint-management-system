# Get Email Settings Details

$token = Get-Content ".working-token" -Raw
Write-Host "Getting EmailServerSettings details..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/email-settings" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($response.isSuccess) {
        Write-Host "`nEmailServerSettings: $($response.data.Count) records" -ForegroundColor Green

        foreach ($setting in $response.data) {
            Write-Host "`n----------------------------------------" -ForegroundColor Cyan
            Write-Host "Name: $($setting.name)" -ForegroundColor Yellow
            Write-Host "  ID: $($setting.id)"
            Write-Host "  Host: $($setting.host):$($setting.port)"
            Write-Host "  Use SSL: $($setting.useSsl)"
            Write-Host "  From: $($setting.fromName) <$($setting.fromEmail)>"
            Write-Host "  Is Active: $($setting.isActive)" -ForegroundColor $(if ($setting.isActive) { "Green" } else { "Red" })
            Write-Host "  Is Default: $($setting.isDefault)" -ForegroundColor $(if ($setting.isDefault) { "Green" } else { "Gray" })
            Write-Host "  Username: $($setting.username)"
        }

        $defaultSetting = $response.data | Where-Object { $_.isDefault -eq $true }
        if ($defaultSetting) {
            Write-Host "`n========================================" -ForegroundColor Green
            Write-Host "DEFAULT EMAIL SETTING:" -ForegroundColor Green
            Write-Host "  $($defaultSetting.name)" -ForegroundColor Green
            Write-Host "  From: $($defaultSetting.fromEmail)" -ForegroundColor Green
            Write-Host "========================================" -ForegroundColor Green
        } else {
            Write-Host "`nWARNING: No default email setting configured!" -ForegroundColor Yellow
            Write-Host "Auto-response system needs a default email setting." -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
