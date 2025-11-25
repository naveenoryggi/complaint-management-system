# Email Ticketing OAuth Setup Script
# This script configures email ticketing with OAuth authentication

Write-Host "=== Email Ticketing OAuth Configuration ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Get Admin Token
Write-Host "Step 1: Logging in as Admin..." -ForegroundColor Yellow
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
        -Method POST `
        -Body $loginBody `
        -ContentType "application/json"

    $token = $loginResponse.data.token
    Write-Host "✓ Login successful" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "✗ Login failed: $_" -ForegroundColor Red
    exit 1
}

# Step 2: Check existing email configurations
Write-Host "Step 2: Checking existing email configurations..." -ForegroundColor Yellow
try {
    $existingConfigs = Invoke-RestMethod -Uri "http://localhost:5000/api/emailconfiguration" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"}

    Write-Host "✓ Found $($existingConfigs.data.Count) existing configuration(s)" -ForegroundColor Green

    if ($existingConfigs.data.Count -gt 0) {
        Write-Host "`nExisting Configurations:" -ForegroundColor Cyan
        foreach ($config in $existingConfigs.data) {
            Write-Host "  - $($config.fromEmail) ($($config.authenticationType))" -ForegroundColor Gray
        }
    }
    Write-Host ""
} catch {
    Write-Host "✗ Failed to fetch configurations: $_" -ForegroundColor Red
}

# Step 3: Prompt for email configuration details
Write-Host "Step 3: Email Configuration Setup" -ForegroundColor Yellow
Write-Host ""

$emailAddress = Read-Host "Enter support email address (e.g., support@yourdomain.com)"
$displayName = Read-Host "Enter display name (e.g., Support Team)"
$description = Read-Host "Enter description (e.g., Main support inbox)"

Write-Host ""
Write-Host "Select Email Provider:" -ForegroundColor Cyan
Write-Host "1. Office365 (Microsoft 365 / Outlook)"
Write-Host "2. Gmail (Google Workspace)"
Write-Host "3. Outlook.com"
$providerChoice = Read-Host "Enter choice (1-3)"

switch ($providerChoice) {
    "1" {
        $provider = "Office365"
        $imapServer = "outlook.office365.com"
        $oauthProviderType = "Microsoft"
    }
    "2" {
        $provider = "Gmail"
        $imapServer = "imap.gmail.com"
        $oauthProviderType = "Google"
    }
    "3" {
        $provider = "Outlook"
        $imapServer = "outlook.office365.com"
        $oauthProviderType = "Microsoft"
    }
    default {
        Write-Host "Invalid choice. Defaulting to Office365" -ForegroundColor Yellow
        $provider = "Office365"
        $imapServer = "outlook.office365.com"
        $oauthProviderType = "Microsoft"
    }
}

Write-Host ""
Write-Host "Using provider: $provider" -ForegroundColor Green
Write-Host "IMAP Server: $imapServer" -ForegroundColor Green
Write-Host ""

# Step 4: OAuth credentials (using existing Azure AD app)
Write-Host "Step 4: OAuth Credentials (Using existing Azure AD app)" -ForegroundColor Yellow
$clientId = "e623af77-783b-4da7-82eb-289606731d41"
$clientSecret = "tR78Q~3WbJ6q.oyVxOOrKWIXJ6Nq_s46.ulwpcYU"
$tenantId = "d6c5af8d-1821-4696-bcdf-47d30e50551a"

Write-Host "✓ Client ID: $clientId" -ForegroundColor Green
Write-Host "✓ Tenant ID: $tenantId" -ForegroundColor Green
Write-Host ""

# Step 5: Polling settings
Write-Host "Step 5: Polling Configuration" -ForegroundColor Yellow
$pollingInterval = Read-Host "Enter polling interval in seconds (default: 300)"
if ([string]::IsNullOrWhiteSpace($pollingInterval)) {
    $pollingInterval = 300
}

Write-Host ""

# Step 6: Create email configuration
Write-Host "Step 6: Creating email ticketing configuration..." -ForegroundColor Yellow

$configBody = @{
    fromEmail = $emailAddress
    displayName = $displayName
    description = $description
    authenticationType = "OAuth"
    provider = $provider
    imapServer = $imapServer
    imapPort = 993
    useSsl = $true
    oauthProviderType = $oauthProviderType
    oauthClientId = $clientId
    oauthClientSecret = $clientSecret
    oauthTenantId = $tenantId
    oauthRedirectUri = "http://localhost:5000/api/oauth/callback"
    pollingIntervalSeconds = [int]$pollingInterval
    isActive = $true
    enableAutoAcknowledgement = $false
} | ConvertTo-Json

Write-Host ""
Write-Host "Configuration Payload:" -ForegroundColor Cyan
Write-Host $configBody -ForegroundColor Gray
Write-Host ""

try {
    $createResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/emailconfiguration" `
        -Method POST `
        -Headers @{Authorization="Bearer $token"} `
        -Body $configBody `
        -ContentType "application/json"

    Write-Host "✓ Email configuration created successfully!" -ForegroundColor Green
    $configId = $createResponse.data.id
    Write-Host "  Configuration ID: $configId" -ForegroundColor Cyan
    Write-Host ""

    # Step 7: Generate OAuth authorization URL
    Write-Host "Step 7: OAuth Authorization" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To complete OAuth setup, you need to authorize access:" -ForegroundColor Cyan
    Write-Host ""

    if ($oauthProviderType -eq "Microsoft") {
        $authUrl = "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/authorize?" +
                   "client_id=$clientId" +
                   "&response_type=code" +
                   "&redirect_uri=http://localhost:5000/api/oauth/callback" +
                   "&response_mode=query" +
                   "&scope=https://outlook.office365.com/IMAP.AccessAsUser.All https://outlook.office365.com/SMTP.Send offline_access" +
                   "&state=$configId"

        Write-Host "Authorization URL:" -ForegroundColor Yellow
        Write-Host $authUrl -ForegroundColor White
        Write-Host ""
        Write-Host "Opening browser for authorization..." -ForegroundColor Cyan
        Start-Process $authUrl
    }

    Write-Host ""
    Write-Host "NEXT STEPS:" -ForegroundColor Yellow
    Write-Host "1. ✓ Email configuration created" -ForegroundColor Green
    Write-Host "2. → Complete OAuth authorization in browser" -ForegroundColor Cyan
    Write-Host "3. → Sign in with $emailAddress" -ForegroundColor Cyan
    Write-Host "4. → Grant permissions when prompted" -ForegroundColor Cyan
    Write-Host "5. → After authorization, test the configuration in Admin UI" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Configuration will be available at:" -ForegroundColor Yellow
    Write-Host "http://localhost:4200 → Admin Panel → Communication Settings → Email Ticketing Config" -ForegroundColor White
    Write-Host ""

} catch {
    Write-Host "✗ Failed to create configuration: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error Details:" -ForegroundColor Yellow
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host $responseBody -ForegroundColor Red
    }
    exit 1
}

Write-Host ""
Write-Host "=== Setup Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Email: $emailAddress" -ForegroundColor White
Write-Host "  Provider: $provider" -ForegroundColor White
Write-Host "  Authentication: OAuth 2.0" -ForegroundColor White
Write-Host "  Polling Interval: $pollingInterval seconds" -ForegroundColor White
Write-Host "  IMAP Server: $imapServer:993" -ForegroundColor White
Write-Host ""
Write-Host "Monitor email polling status:" -ForegroundColor Yellow
Write-Host "  Check LastPolledAt timestamp in Admin UI" -ForegroundColor White
Write-Host "  View processed emails in EmailMessages table" -ForegroundColor White
Write-Host ""
