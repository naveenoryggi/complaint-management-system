# Test Email Configuration API
$baseUrl = "http://localhost:5000"

# Login first
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token

Write-Host "Login successful" -ForegroundColor Green

# Prepare email configuration
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$emailConfig = @{
    fromName = "Oryggi Tech Support"
    fromEmail = "marketing@oryggitech.com"
    imapHost = "outlook.office365.com"
    imapPort = 993
    imapUseSsl = $true
    imapUsername = "marketing@oryggitech.com"
    imapPassword = "M%226099461497uf"
    imapFolder = "INBOX"
    smtpHost = "smtp.office365.com"
    smtpPort = 587
    smtpUseSsl = $true
    smtpUsername = "marketing@oryggitech.com"
    smtpPassword = "M%226099461497uf"
    pollingIntervalMinutes = 5
    maxAttachmentSizeBytes = 10485760
    threadTimeoutDays = 7
    allowedAttachmentExtensions = ".pdf,.doc,.docx,.txt,.jpg,.jpeg,.png,.gif,.zip"
    isEnabled = $true
    sendAutoAcknowledgement = $true
    enableThreading = $true
} | ConvertTo-Json

Write-Host "`nSending request to create email configuration..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration" -Method POST -Headers $headers -Body $emailConfig
    Write-Host "`nSUCCESS!" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "`nERROR!" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "Status Description: $($_.Exception.Response.StatusDescription)" -ForegroundColor Red

    if ($_.ErrorDetails.Message) {
        Write-Host "`nError Details:" -ForegroundColor Yellow
        $_.ErrorDetails.Message | ConvertFrom-Json | ConvertTo-Json -Depth 5
    }
}
