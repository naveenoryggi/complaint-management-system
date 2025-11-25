# Configure Microsoft Domain Email Ticketing
# Email: marketing@oryggitech.com

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Email Ticketing Configuration - Microsoft Domain" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000"

# Step 1: Login to get auth token
Write-Host "[1/6] Logging in to get authentication token..." -ForegroundColor Yellow
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

    if ($loginResponse.isSuccess) {
        $token = $loginResponse.data.token
        Write-Host "✓ Login successful" -ForegroundColor Green
        Write-Host "  User: $($loginResponse.data.fullName)" -ForegroundColor Gray
        Write-Host ""
    } else {
        Write-Host "✗ Login failed: $($loginResponse.message)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Login error: $_" -ForegroundColor Red
    exit 1
}

# Step 2: Create email configuration for Microsoft domain
Write-Host "[2/6] Creating email configuration for marketing@oryggitech.com..." -ForegroundColor Yellow

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$emailConfig = @{
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
    fromEmail = "marketing@oryggitech.com"
    fromName = "Oryggi Tech Support"
    pollingIntervalMinutes = 5
    isEnabled = $true
    sendAutoAcknowledgement = $true
    enableThreading = $true
    threadTimeoutDays = 7
    maxAttachmentSizeBytes = 10485760
    allowedAttachmentExtensions = ".pdf,.doc,.docx,.txt,.jpg,.jpeg,.png,.gif,.zip"
} | ConvertTo-Json

try {
    $configResponse = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration" -Method POST -Headers $headers -Body $emailConfig

    if ($configResponse.isSuccess) {
        $configId = $configResponse.data.id
        Write-Host "✓ Email configuration created successfully" -ForegroundColor Green
        Write-Host "  Configuration ID: $configId" -ForegroundColor Gray
        Write-Host "  Email: $($configResponse.data.fromEmail)" -ForegroundColor Gray
        Write-Host "  Polling Interval: $($configResponse.data.pollingIntervalMinutes) minutes" -ForegroundColor Gray
        Write-Host ""
    } else {
        Write-Host "✗ Configuration creation failed: $($configResponse.message)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Configuration error: $_" -ForegroundColor Red
    Write-Host "  Response: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    exit 1
}

# Step 3: Test IMAP connection
Write-Host "[3/6] Testing IMAP connection to Outlook..." -ForegroundColor Yellow
try {
    $imapTestResponse = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration/$configId/test-imap" -Method POST -Headers $headers

    if ($imapTestResponse.isSuccess -and $imapTestResponse.data.success) {
        Write-Host "✓ IMAP connection successful!" -ForegroundColor Green
        Write-Host "  Message: $($imapTestResponse.data.message)" -ForegroundColor Gray
        Write-Host ""
    } else {
        Write-Host "✗ IMAP connection failed: $($imapTestResponse.data.message)" -ForegroundColor Red
        Write-Host "  This could be due to:" -ForegroundColor Yellow
        Write-Host "    - Incorrect password" -ForegroundColor Yellow
        Write-Host "    - 2FA enabled (need App Password)" -ForegroundColor Yellow
        Write-Host "    - IMAP not enabled in account settings" -ForegroundColor Yellow
        Write-Host ""
    }
} catch {
    Write-Host "✗ IMAP test error: $_" -ForegroundColor Red
}

# Step 4: Test SMTP connection
Write-Host "[4/6] Testing SMTP connection to Outlook..." -ForegroundColor Yellow
try {
    $smtpTestResponse = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration/$configId/test-smtp" -Method POST -Headers $headers

    if ($smtpTestResponse.isSuccess -and $smtpTestResponse.data.success) {
        Write-Host "✓ SMTP connection successful!" -ForegroundColor Green
        Write-Host "  Message: $($smtpTestResponse.data.message)" -ForegroundColor Gray
        Write-Host ""
    } else {
        Write-Host "✗ SMTP connection failed: $($smtpTestResponse.data.message)" -ForegroundColor Red
        Write-Host "  This could be due to:" -ForegroundColor Yellow
        Write-Host "    - Incorrect password" -ForegroundColor Yellow
        Write-Host "    - SMTP authentication required" -ForegroundColor Yellow
        Write-Host "    - Modern authentication required" -ForegroundColor Yellow
        Write-Host ""
    }
} catch {
    Write-Host "✗ SMTP test error: $_" -ForegroundColor Red
}

# Step 5: Get current configuration details
Write-Host "[5/6] Fetching configuration details..." -ForegroundColor Yellow
try {
    $configDetails = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration/$configId" -Method GET -Headers $headers

    if ($configDetails.isSuccess) {
        Write-Host "✓ Configuration retrieved" -ForegroundColor Green
        Write-Host ""
        Write-Host "Configuration Details:" -ForegroundColor Cyan
        Write-Host "  Email: $($configDetails.data.fromEmail)" -ForegroundColor White
        Write-Host "  Name: $($configDetails.data.fromName)" -ForegroundColor White
        Write-Host "  IMAP Server: $($configDetails.data.imapHost):$($configDetails.data.imapPort)" -ForegroundColor White
        Write-Host "  SMTP Server: $($configDetails.data.smtpHost):$($configDetails.data.smtpPort)" -ForegroundColor White
        Write-Host "  Status: $(if ($configDetails.data.isEnabled) { 'Enabled' } else { 'Disabled' })" -ForegroundColor White
        Write-Host "  Last Polled: $($configDetails.data.lastPolledAt)" -ForegroundColor White
        Write-Host "  Auto Acknowledgement: $(if ($configDetails.data.sendAutoAcknowledgement) { 'Yes' } else { 'No' })" -ForegroundColor White
        Write-Host "  Threading Enabled: $(if ($configDetails.data.enableThreading) { 'Yes' } else { 'No' })" -ForegroundColor White
        Write-Host ""
    }
} catch {
    Write-Host "✗ Error fetching configuration: $_" -ForegroundColor Red
}

# Step 6: Manual poll for emails
Write-Host "[6/6] Polling for emails now..." -ForegroundColor Yellow
try {
    $pollResponse = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration/$configId/poll-now" -Method POST -Headers $headers

    if ($pollResponse.isSuccess) {
        Write-Host "✓ Email polling completed!" -ForegroundColor Green
        Write-Host "  Emails Fetched: $($pollResponse.data.emailsFetched)" -ForegroundColor Gray
        Write-Host "  Complaints Created: $($pollResponse.data.complaintsCreated)" -ForegroundColor Gray
        Write-Host "  Failed: $($pollResponse.data.failed)" -ForegroundColor Gray

        if ($pollResponse.data.errors -and $pollResponse.data.errors.Count -gt 0) {
            Write-Host "  Errors:" -ForegroundColor Red
            $pollResponse.data.errors | ForEach-Object {
                Write-Host "    - $_" -ForegroundColor Red
            }
        }
        Write-Host ""
    } else {
        Write-Host "✗ Email polling failed: $($pollResponse.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Polling error: $_" -ForegroundColor Red
}

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Configuration Complete!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Send a test email to marketing@oryggitech.com" -ForegroundColor White
Write-Host "  2. Wait 5 minutes for automatic polling" -ForegroundColor White
Write-Host "  3. Or manually poll using the admin UI" -ForegroundColor White
Write-Host "  4. Check complaints list for new email-based complaints" -ForegroundColor White
Write-Host ""
Write-Host "Configuration ID: $configId" -ForegroundColor Yellow
Write-Host ""
