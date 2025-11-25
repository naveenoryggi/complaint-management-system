# Update SMTP Credentials for Real Email Sending
# This script updates the test SMTP configuration with real Gmail credentials

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Update SMTP Credentials" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "To send real emails via Gmail, you need:" -ForegroundColor Yellow
Write-Host "1. A Gmail account" -ForegroundColor White
Write-Host "2. An App Password (NOT your regular password)" -ForegroundColor White
Write-Host ""

Write-Host "To create a Gmail App Password:" -ForegroundColor Yellow
Write-Host "1. Go to: https://myaccount.google.com/apppasswords" -ForegroundColor White
Write-Host "2. Sign in to your Google Account" -ForegroundColor White
Write-Host "3. Select 'App' -> 'Mail' and 'Device' -> 'Windows Computer'" -ForegroundColor White
Write-Host "4. Click 'Generate'" -ForegroundColor White
Write-Host "5. Copy the 16-character password (e.g., xxxx xxxx xxxx xxxx)" -ForegroundColor White
Write-Host ""

Write-Host "Do you have your Gmail credentials ready? (Y/N): " -ForegroundColor Green -NoNewline
$ready = Read-Host

if ($ready -ne 'Y' -and $ready -ne 'y') {
    Write-Host ""
    Write-Host "No problem! Run this script again when you're ready." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "NOTE: The notification system is working correctly!" -ForegroundColor Green
    Write-Host "It just needs real credentials to actually send emails." -ForegroundColor Green
    exit
}

Write-Host ""
Write-Host "Enter your Gmail address: " -ForegroundColor Green -NoNewline
$email = Read-Host

Write-Host "Enter your Gmail App Password (16 chars, remove spaces): " -ForegroundColor Green -NoNewline
$appPassword = Read-Host -AsSecureString
$appPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($appPassword))

# Remove spaces from app password
$appPasswordPlain = $appPasswordPlain -replace '\s', ''

Write-Host ""
Write-Host "Updating SMTP configuration..." -ForegroundColor Cyan

$emailEscaped = $email -replace "'", "''"
$passwordEscaped = $appPasswordPlain -replace "'", "''"

$query = "UPDATE EmailServerSettings SET Username = '$emailEscaped', Password = '$passwordEscaped', FromEmail = '$emailEscaped' WHERE IsActive = 1; SELECT 'SMTP credentials updated successfully' AS Status, Name, Host, Port, Username, FromEmail FROM EmailServerSettings WHERE IsActive = 1;"

try {
    $result = sqlcmd -S 'LAPTOP-NF9BTG7Q\SQLEXPRESS' -d 'ComplaintManagementDB' -E -Q $query -W

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ SMTP credentials updated successfully!" -ForegroundColor Green
        Write-Host $result
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "Ready to Send Real Emails!" -ForegroundColor Green
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next Steps:" -ForegroundColor Yellow
        Write-Host "1. Run: test-notification-simple.ps1 to create another test complaint" -ForegroundColor White
        Write-Host "2. Check your email inbox for the notification" -ForegroundColor White
        Write-Host "3. Check CommunicationLogs - Status should be 2 (Sent) instead of 5 (Failed)" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host "✗ Error updating credentials" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
}
