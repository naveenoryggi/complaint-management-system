# Simple Email Send Test

$token = Get-Content ".working-token" -Raw
Write-Host "Using existing token" -ForegroundColor Cyan

# Get first complaint
Write-Host "`nGetting test complaint..." -ForegroundColor Yellow
$complaintsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints?pageNumber=1&pageSize=5" `
    -Method GET `
    -Headers @{Authorization="Bearer $token"} `
    -ContentType "application/json"

$testComplaint = $complaintsResponse.data.items[0]
Write-Host "Test complaint: $($testComplaint.complaintNumber)" -ForegroundColor Green
Write-Host "  Title: $($testComplaint.title)"

# Send test email
Write-Host "`nSending test email..." -ForegroundColor Yellow
$emailBody = @{
    complaintId = $testComplaint.id
    toEmail = if ($testComplaint.complainantEmail) { $testComplaint.complainantEmail } else { "test@example.com" }
    subject = "Re: $($testComplaint.title) - Test from Email Composer"
    body = "This is a test email from the Email Composer Modal. Complaint: $($testComplaint.complaintNumber)"
    isHtml = $false
    isInternal = $false
} | ConvertTo-Json

Write-Host "Sending to: $(($emailBody | ConvertFrom-Json).toEmail)"

$sendResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/email-ticketing/send-reply" `
    -Method POST `
    -Headers @{Authorization="Bearer $token"} `
    -Body $emailBody `
    -ContentType "application/json"

if ($sendResponse.isSuccess) {
    Write-Host "SUCCESS: Email sent!" -ForegroundColor Green
    Write-Host "Message: $($sendResponse.message)"
} else {
    Write-Host "FAILED: $($sendResponse.message)" -ForegroundColor Red
}

Write-Host "`nTest complete. Check http://localhost:5001 to verify the email composer modal works!" -ForegroundColor Cyan
