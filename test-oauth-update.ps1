# Test OAuth configuration update to see actual validation errors

$token = Get-Content '.admin-token' -Raw
$configId = "4a1b41ef-cbc5-4858-a6a5-02b1c147a80a"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get current config first
Write-Host "Getting current configuration..." -ForegroundColor Cyan
try {
    $currentConfig = Invoke-RestMethod -Uri "http://localhost:5000/api/email-configuration/$configId" -Headers $headers -Method Get
    Write-Host "Current config retrieved" -ForegroundColor Green

    # Try to update with minimal changes
    $updateBody = @{
        id = $configId
        authenticationType = 2  # OAuth2
        imapHost = "outlook.office365.com"
        imapPort = 993
        imapUseSsl = $true
        imapUsername = "marketing@oryggitech.com"
        imapFolder = "INBOX"
        smtpHost = "smtp.office365.com"
        smtpPort = 587
        smtpUseSsl = $true
        smtpUsername = "marketing@oryggitech.com"
        fromEmail = "marketing@oryggitech.com"
        fromName = "Oryggi Tech Support"
        pollingIntervalMinutes = 5
        pollingIntervalSeconds = 120
        isEnabled = $true
        sendAutoAcknowledgement = $false
        enableThreading = $true
        threadTimeoutDays = 7
        maxAttachmentSizeBytes = 10485760
        allowedAttachmentExtensions = ".pdf,.jpg,.jpeg,.png,.doc,.docx"
    } | ConvertTo-Json -Depth 10

    Write-Host "`nSending update request..." -ForegroundColor Cyan
    Write-Host "Request body:" -ForegroundColor Yellow
    Write-Host $updateBody

    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/email-configuration/$configId" -Headers $headers -Method Put -Body $updateBody
        Write-Host "`n✅ Update successful!" -ForegroundColor Green
        Write-Host ($response | ConvertTo-Json -Depth 5)
    } catch {
        Write-Host "`n❌ Update failed!" -ForegroundColor Red
        Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red

        if ($_.ErrorDetails.Message) {
            Write-Host "`nValidation Errors:" -ForegroundColor Yellow
            $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
            Write-Host ($errorDetails | ConvertTo-Json -Depth 5)
        }
    }
} catch {
    Write-Host "❌ Failed to get config: $($_.Exception.Message)" -ForegroundColor Red
}
