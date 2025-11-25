# Update Email Configuration to OAuth
# This script updates the existing email configuration to use OAuth authentication type

$baseUrl = "http://localhost:5101"
$loginUrl = "$baseUrl/api/auth/login"
$configUrl = "$baseUrl/api/email-ticketing/configurations"

# Login credentials
$loginPayload = @{
    identifier = "admin@complaintmanagement.com"
    password = "Admin@123"
    rememberMe = $false
} | ConvertTo-Json

Write-Host "Logging in as admin..." -ForegroundColor Cyan
$loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginPayload -ContentType "application/json"
$token = $loginResponse.data.token
Write-Host "Login successful. Token obtained." -ForegroundColor Green

# Get existing email configurations
Write-Host "`nFetching existing email configurations..." -ForegroundColor Cyan
$headers = @{
    "Authorization" = "Bearer $token"
}

$configs = Invoke-RestMethod -Uri $configUrl -Method Get -Headers $headers
Write-Host "Found $($configs.data.Count) configuration(s)" -ForegroundColor Green

if ($configs.data.Count -gt 0) {
    $config = $configs.data[0]
    Write-Host "`nExisting configuration:" -ForegroundColor Yellow
    Write-Host "  ID: $($config.id)"
    Write-Host "  Name: Oryggi Tech Support"
    Write-Host "  Email: $($config.fromEmail)"
    Write-Host "  Current Auth Type: $($config.authenticationType) (0=Basic, 1=OAuth)"

    # Update configuration to OAuth
    Write-Host "`nUpdating configuration to OAuth..." -ForegroundColor Cyan

    $updatePayload = @{
        id = $config.id
        authenticationType = 1  # OAuth
        imapHost = $config.imapHost
        imapPort = $config.imapPort
        imapUseSsl = $config.imapUseSsl
        imapUsername = $config.imapUsername
        imapFolder = $config.imapFolder
        smtpHost = $config.smtpHost
        smtpPort = $config.smtpPort
        smtpUseSsl = $config.smtpUseSsl
        smtpUsername = $config.smtpUsername
        fromEmail = $config.fromEmail
        fromName = $config.fromName
        pollingIntervalMinutes = $config.pollingIntervalMinutes
        isEnabled = $config.isEnabled
        sendAutoAcknowledgement = $config.sendAutoAcknowledgement
        enableThreading = $config.enableThreading
        threadTimeoutDays = $config.threadTimeoutDays
        maxAttachmentSizeBytes = $config.maxAttachmentSizeBytes
        allowedAttachmentExtensions = $config.allowedAttachmentExtensions
        # OAuth credentials
        oauthClientId = "12345678-1234-1234-1234-123456789abc"
        oauthTenantId = "87654321-4321-4321-4321-cba987654321"
        oauthClientSecret = "test-client-secret-value-for-oauth-testing"
    } | ConvertTo-Json

    try {
        $updateUrl = "$configUrl/$($config.id)"
        $updateResponse = Invoke-RestMethod -Uri $updateUrl -Method Put -Body $updatePayload -ContentType "application/json" -Headers $headers
        Write-Host "Configuration updated successfully!" -ForegroundColor Green
        Write-Host "New Auth Type: OAuth (1)" -ForegroundColor Green
        Write-Host "OAuth Client ID: 12345678-1234-1234-1234-123456789abc" -ForegroundColor Green
        Write-Host "Status: OAuth 2.0 - Pending Authorization" -ForegroundColor Yellow
    } catch {
        Write-Host "Error updating configuration: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
    }
} else {
    Write-Host "No email configurations found!" -ForegroundColor Red
}
