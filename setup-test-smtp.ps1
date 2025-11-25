# Setup Test SMTP Configuration for Notification Testing
# This script inserts test SMTP settings into the EmailSettings table

$serverInstance = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
$database = "ComplaintManagementDB"

Write-Host "Setting up test SMTP configuration..." -ForegroundColor Cyan

# For testing, we'll use Ethereal Email (free test SMTP service)
# In production, replace with real SMTP credentials

$query = @"
-- Check if any active email settings exist
DECLARE @ExistingCount INT;
SELECT @ExistingCount = COUNT(*) FROM EmailSettings WHERE IsActive = 1;

IF @ExistingCount > 0
BEGIN
    PRINT 'Deactivating existing email settings...';
    UPDATE EmailSettings SET IsActive = 0 WHERE IsActive = 1;
END

-- Insert test SMTP configuration
INSERT INTO EmailSettings (
    Id,
    Name,
    SmtpHost,
    SmtpPort,
    UseSsl,
    RequiresAuthentication,
    Username,
    Password,
    FromEmail,
    FromName,
    ReplyToEmail,
    MaxRetries,
    RetryDelayMinutes,
    IsActive,
    IsDefault,
    CompanyId,
    CreatedAt,
    CreatedBy,
    UpdatedAt,
    UpdatedBy
)
VALUES (
    NEWID(),
    'Test SMTP Server - Mailtrap Style',
    'sandbox.smtp.mailtrap.io', -- Using Mailtrap-style test SMTP
    2525, -- Standard test SMTP port
    0, -- No SSL for test server
    1, -- Requires authentication
    'test_user', -- Placeholder username - will be updated
    'test_password', -- Placeholder password - will be updated
    'noreply@complaintmanagement.test',
    'Complaint Management System - Test',
    'support@complaintmanagement.test',
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

-- Verify insertion
SELECT TOP 1
    Id,
    Name,
    SmtpHost,
    SmtpPort,
    UseSsl,
    RequiresAuthentication,
    FromEmail,
    FromName,
    IsActive,
    CreatedAt
FROM EmailSettings
WHERE IsActive = 1
ORDER BY CreatedAt DESC;

PRINT 'Test SMTP configuration created successfully!';
PRINT '';
PRINT 'IMPORTANT: Update the Username and Password with real test SMTP credentials.';
PRINT 'Options:';
PRINT '  1. Mailtrap.io - Free test inbox (sandbox.smtp.mailtrap.io:2525)';
PRINT '  2. Ethereal Email - Free temporary inbox (smtp.ethereal.email:587)';
PRINT '  3. Gmail - Use app-specific password (smtp.gmail.com:587)';
PRINT '';
PRINT 'To update credentials, run:';
PRINT 'UPDATE EmailSettings SET Username = ''your_username'', Password = ''your_password'' WHERE IsActive = 1;';
"@

try {
    Write-Host "Executing SQL query..." -ForegroundColor Yellow

    $result = sqlcmd -S $serverInstance -d $database -E -Q $query -W

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✓ Test SMTP configuration created successfully!" -ForegroundColor Green
        Write-Host "`nCurrent configuration:" -ForegroundColor Cyan
        Write-Host $result

        Write-Host "`n=====================================" -ForegroundColor Yellow
        Write-Host "NEXT STEPS:" -ForegroundColor Yellow
        Write-Host "=====================================" -ForegroundColor Yellow
        Write-Host "1. For FREE test email service, visit: https://mailtrap.io" -ForegroundColor White
        Write-Host "   - Sign up for free account" -ForegroundColor White
        Write-Host "   - Get your SMTP credentials from inbox settings" -ForegroundColor White
        Write-Host "   - Update the EmailSettings table with real credentials" -ForegroundColor White
        Write-Host ""
        Write-Host "2. OR use Gmail for testing:" -ForegroundColor White
        Write-Host "   - Enable 2FA on your Gmail account" -ForegroundColor White
        Write-Host "   - Generate an App Password" -ForegroundColor White
        Write-Host "   - Update: smtp.gmail.com, Port 587, SSL=1" -ForegroundColor White
        Write-Host ""
        Write-Host "3. After updating credentials, run test-notifications.ps1" -ForegroundColor White
        Write-Host "=====================================" -ForegroundColor Yellow
    } else {
        Write-Host "✗ Error executing SQL query" -ForegroundColor Red
        Write-Host $result
    }
} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
}
