# Quick Email Configuration Script
$baseUrl = "http://localhost:5000"

# Login
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token

Write-Host "Logged in successfully" -ForegroundColor Green

# Create email configuration
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$emailConfigJson = Get-Content -Path "email-config-payload.json" -Raw

try {
    $result = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration" -Method POST -Headers $headers -Body $emailConfigJson

    if ($result.isSuccess) {
        Write-Host "`nEmail configuration created successfully!" -ForegroundColor Green
        Write-Host "Configuration ID: $($result.data.id)" -ForegroundColor Cyan
        Write-Host "Email: $($result.data.fromEmail)" -ForegroundColor Cyan
        Write-Host "IMAP: $($result.data.imapHost):$($result.data.imapPort)" -ForegroundColor Cyan
        Write-Host "SMTP: $($result.data.smtpHost):$($result.data.smtpPort)" -ForegroundColor Cyan
        Write-Host "Status: $(if ($result.data.isEnabled) { 'Enabled' } else { 'Disabled' })" -ForegroundColor Cyan

        # Save config ID for later use
        $result.data.id | Out-File -FilePath ".email-config-id.txt"

        Write-Host "`n✓ Configuration saved!" -ForegroundColor Green
        Write-Host "Next: Testing connections..." -ForegroundColor Yellow
    } else {
        Write-Host "`nFailed: $($result.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "Error occurred: $_" -ForegroundColor Red
}
