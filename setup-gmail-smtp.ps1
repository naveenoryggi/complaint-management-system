# Quick Gmail SMTP Setup for Testing
# This script helps you configure Gmail SMTP for testing notifications

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Gmail SMTP Setup for Testing" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "To use Gmail for testing, you need:" -ForegroundColor Yellow
Write-Host "1. A Gmail account" -ForegroundColor White
Write-Host "2. An App Password (not your regular password)" -ForegroundColor White
Write-Host ""

Write-Host "To create an App Password:" -ForegroundColor Yellow
Write-Host "1. Go to: https://myaccount.google.com/apppasswords" -ForegroundColor White
Write-Host "2. Sign in to your Google Account" -ForegroundColor White
Write-Host "3. Select 'App' → 'Mail' and 'Device' → 'Windows Computer'" -ForegroundColor White
Write-Host "4. Click 'Generate'" -ForegroundColor White
Write-Host "5. Copy the 16-character password (like: xxxx xxxx xxxx xxxx)" -ForegroundColor White
Write-Host ""

Write-Host "Do you have a Gmail account and App Password ready? (Y/N): " -ForegroundColor Green -NoNewline
$ready = Read-Host

if ($ready -ne 'Y' -and $ready -ne 'y') {
    Write-Host ""
    Write-Host "No problem! Please complete the steps above first." -ForegroundColor Yellow
    Write-Host "Then run this script again." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alternatively, I can set up a mock SMTP configuration for testing." -ForegroundColor Cyan
    Write-Host "Would you like me to set up mock SMTP instead? (Y/N): " -ForegroundColor Green -NoNewline
    $mock = Read-Host

    if ($mock -eq 'Y' -or $mock -eq 'y') {
        Write-Host ""
        Write-Host "Setting up mock SMTP configuration..." -ForegroundColor Cyan

        $serverInstance = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
        $database = "ComplaintManagementDB"

        $query = @"
UPDATE EmailSettings SET IsActive = 0 WHERE IsActive = 1;

INSERT INTO EmailSettings (
    Id, Name, SmtpHost, SmtpPort, UseSsl, RequiresAuthentication,
    Username, Password, FromEmail, FromName, ReplyToEmail,
    MaxRetries, RetryDelayMinutes, IsActive, IsDefault,
    CompanyId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
)
VALUES (
    NEWID(), 'Mock SMTP Server', 'localhost', 1025, 0, 0,
    '', '', 'noreply@complaintmanagement.test', 'Test System', 'support@complaintmanagement.test',
    3, 5, 1, 1,
    (SELECT TOP 1 Id FROM Companies WHERE IsActive = 1),
    GETUTCDATE(), 'System', GETUTCDATE(), 'System'
);

SELECT 'Mock SMTP configured (emails will fail but we can see the logs)' AS Status;
"@

        sqlcmd -S $serverInstance -d $database -E -Q $query -W
        Write-Host "✓ Mock SMTP configured!" -ForegroundColor Green
    }

    exit
}

Write-Host ""
Write-Host "Enter your Gmail address: " -ForegroundColor Green -NoNewline
$email = Read-Host

Write-Host "Enter your App Password (16 characters, no spaces): " -ForegroundColor Green -NoNewline
$appPassword = Read-Host -AsSecureString
$appPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($appPassword))

Write-Host ""
Write-Host "Configuring Gmail SMTP..." -ForegroundColor Cyan

$serverInstance = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
$database = "ComplaintManagementDB"

# Escape single quotes in password
$appPasswordEscaped = $appPasswordPlain -replace "'", "''"
$emailEscaped = $email -replace "'", "''"

$query = @"
-- Deactivate existing settings
UPDATE EmailSettings SET IsActive = 0 WHERE IsActive = 1;

-- Insert Gmail SMTP configuration
INSERT INTO EmailSettings (
    Id, Name, SmtpHost, SmtpPort, UseSsl, RequiresAuthentication,
    Username, Password, FromEmail, FromName, ReplyToEmail,
    MaxRetries, RetryDelayMinutes, IsActive, IsDefault,
    CompanyId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
)
VALUES (
    NEWID(),
    'Gmail SMTP - Test Configuration',
    'smtp.gmail.com',
    587,
    1, -- Use SSL
    1, -- Requires authentication
    '$emailEscaped',
    '$appPasswordEscaped',
    '$emailEscaped',
    'Complaint Management System',
    '$emailEscaped',
    3,
    5,
    1, -- Active
    1, -- Default
    (SELECT TOP 1 Id FROM Companies WHERE IsActive = 1),
    GETUTCDATE(),
    'System Setup',
    GETUTCDATE(),
    'System Setup'
);

SELECT 'Gmail SMTP configured successfully!' AS Status;
SELECT TOP 1 Name, SmtpHost, SmtpPort, FromEmail, IsActive FROM EmailSettings WHERE IsActive = 1;
"@

try {
    $result = sqlcmd -S $serverInstance -d $database -E -Q $query -W

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ Gmail SMTP configured successfully!" -ForegroundColor Green
        Write-Host $result
        Write-Host ""
        Write-Host "Ready to test notifications! ✓" -ForegroundColor Green
    } else {
        Write-Host "✗ Error configuring SMTP" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
}
