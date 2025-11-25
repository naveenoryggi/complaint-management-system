# Test Email Composer Modal - End-to-End Test

$tokenFile = ".working-token"
if (!(Test-Path $tokenFile)) {
    Write-Host "No token found - getting fresh token" -ForegroundColor Yellow
    # Login to get token
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
        -Method POST `
        -Body $loginBody `
        -ContentType "application/json"

    if ($loginResponse.isSuccess) {
        $token = $loginResponse.data.token
        Set-Content -Path $tokenFile -Value $token
        Write-Host "Login successful, token saved" -ForegroundColor Green
    }
    else {
        Write-Host "Login failed: $($loginResponse.message)" -ForegroundColor Red
        exit 1
    }
}
else {
    $token = Get-Content $tokenFile -Raw
    Write-Host "Using existing token" -ForegroundColor Cyan
}

# Test 1: Get a complaint ID to test with
Write-Host "`n=== Test 1: Get Complaint List ===" -ForegroundColor Yellow
try {
    $complaintsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints?pageNumber=1&pageSize=5" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($complaintsResponse.isSuccess -and $complaintsResponse.data.items.Count -gt 0) {
        $testComplaint = $complaintsResponse.data.items[0]
        Write-Host "Found test complaint: $($testComplaint.complaintNumber)" -ForegroundColor Green
        Write-Host "  ID: $($testComplaint.id)"
        Write-Host "  Title: $($testComplaint.title)"
        Write-Host "  Complainant: $($testComplaint.complainantEmail)"
    }
    else {
        Write-Host "No complaints found to test with" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "Error getting complaints: $_" -ForegroundColor Red
    exit 1
}

# Test 2: Check email thread for this complaint
Write-Host "`n=== Test 2: Check Email Thread ===" -ForegroundColor Yellow
try {
    $emailThreadResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/email-ticketing/complaint/$($testComplaint.id)/emails" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($emailThreadResponse.isSuccess) {
        $emailCount = $emailThreadResponse.data.Count
        Write-Host "Email thread has $emailCount messages" -ForegroundColor Green
    }
    else {
        Write-Host "No emails in thread yet (this is OK)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "Error getting email thread: $_" -ForegroundColor Red
}

# Test 3: Send test email reply
Write-Host "`n=== Test 3: Send Test Email Reply ===" -ForegroundColor Yellow
$emailBody = @{
    complaintId = $testComplaint.id
    toEmail = if ($testComplaint.complainantEmail) { $testComplaint.complainantEmail } else { "test@example.com" }
    subject = "Re: $($testComplaint.title) - Test from Email Composer"
    body = "This is a test email sent from the new Email Composer Modal.`n`nComplaint Number: $($testComplaint.complaintNumber)`n`nSent at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    isHtml = $false
    isInternal = $false
} | ConvertTo-Json

Write-Host "Sending email to: $($emailBody | ConvertFrom-Json | Select-Object -ExpandProperty toEmail)" -ForegroundColor Cyan

try {
    $sendResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/email-ticketing/send-reply" `
        -Method POST `
        -Headers @{Authorization="Bearer $token"} `
        -Body $emailBody `
        -ContentType "application/json"

    if ($sendResponse.isSuccess) {
        Write-Host "✓ Email sent successfully!" -ForegroundColor Green
        Write-Host "  Message: $($sendResponse.message)"
    }
    else {
        Write-Host "✗ Email send failed" -ForegroundColor Red
        Write-Host "  Message: $($sendResponse.message)"
        if ($sendResponse.errors) {
            Write-Host "  Errors: $($sendResponse.errors -join ', ')"
        }
    }
}
catch {
    Write-Host "✗ Error sending email: $_" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "  Details: $($_.ErrorDetails.Message)"
    }
}

# Test 4: Verify email was added to thread
Write-Host "`n=== Test 4: Verify Email in Thread ===" -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $verifyResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/email-ticketing/complaint/$($testComplaint.id)/emails" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($verifyResponse.isSuccess) {
        $newEmailCount = $verifyResponse.data.Count
        Write-Host "Email thread now has $newEmailCount messages" -ForegroundColor Green

        if ($newEmailCount -gt $emailCount) {
            Write-Host "✓ New email successfully added to thread!" -ForegroundColor Green
        }
        else {
            Write-Host "⚠ Email count unchanged - email may be queued" -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Host "Error verifying email: $_" -ForegroundColor Red
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
Write-Host "Summary:"
Write-Host "  - Complaint ID: $($testComplaint.id)"
Write-Host "  - Complaint Number: $($testComplaint.complaintNumber)"
Write-Host "  - Email sent to: $($emailBody | ConvertFrom-Json | Select-Object -ExpandProperty toEmail)"
Write-Host "`nNext step: Check the UI at http://localhost:5001 and verify the email composer modal works!" -ForegroundColor Green
