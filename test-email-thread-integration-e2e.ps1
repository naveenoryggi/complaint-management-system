# Email Thread Integration - Week 1 E2E Test
# Tests all email thread viewing and reply functionality

Write-Host "=== Email Thread Integration E2E Test ===" -ForegroundColor Cyan
Write-Host ""

# Get fresh admin token
Write-Host "1. Getting admin authentication token..." -ForegroundColor Yellow
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

    if ($loginResponse.isSuccess) {
        $token = $loginResponse.data.token
        Write-Host "[PASS] Successfully authenticated as admin" -ForegroundColor Green
        Write-Host "  User: $($loginResponse.data.user.fullName)" -ForegroundColor Gray
        Write-Host "  Email: $($loginResponse.data.user.email)" -ForegroundColor Gray
    } else {
        Write-Host "[FAIL] Login failed: $($loginResponse.message)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "[FAIL] Login error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Create authorization header
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Initialize test results
$testResults = @()

# Test 1: Get Complaints List
Write-Host "2. Getting complaints list..." -ForegroundColor Yellow
try {
    $complaintsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" -Method GET -Headers $headers

    if ($complaintsResponse.isSuccess -and $complaintsResponse.data.Count -gt 0) {
        $complaintId = $complaintsResponse.data[0].id
        $complaintNumber = $complaintsResponse.data[0].complaintNumber
        Write-Host "[PASS] Retrieved complaints successfully" -ForegroundColor Green
        Write-Host "  Total complaints: $($complaintsResponse.data.Count)" -ForegroundColor Gray
        Write-Host "  Test complaint: $complaintNumber ($complaintId)" -ForegroundColor Gray
        $testResults += @{ Test = "Get Complaints"; Status = "PASS"; Details = "$($complaintsResponse.data.Count) complaints" }
    } else {
        Write-Host "[FAIL] No complaints found" -ForegroundColor Red
        $testResults += @{ Test = "Get Complaints"; Status = "FAIL"; Details = "No complaints" }
        exit 1
    }
} catch {
    Write-Host "[FAIL] Error: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Get Complaints"; Status = "FAIL"; Details = $_.Exception.Message }
    exit 1
}

Write-Host ""

# Test 2: Get Email Thread for Complaint
Write-Host "3. Getting email thread for complaint..." -ForegroundColor Yellow
try {
    $emailThreadResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/email-ticketing/complaint/$complaintId/emails" -Method GET -Headers $headers

    if ($emailThreadResponse.isSuccess) {
        $emailCount = if ($emailThreadResponse.data) { $emailThreadResponse.data.Count } else { 0 }
        Write-Host "[PASS] Email thread API endpoint working" -ForegroundColor Green
        Write-Host "  Emails in thread: $emailCount" -ForegroundColor Gray

        if ($emailCount -gt 0) {
            Write-Host "  Email details:" -ForegroundColor Gray
            $emailThreadResponse.data | ForEach-Object {
                Write-Host "    - From: $($_.fromEmail) | To: $($_.toEmail) | Subject: $($_.subject)" -ForegroundColor Gray
                Write-Host "      Direction: $($_.direction) | Status: $($_.status) | Date: $($_.receivedAt)" -ForegroundColor DarkGray
            }
        }

        $testResults += @{ Test = "Get Email Thread"; Status = "PASS"; Details = "$emailCount emails" }
    } else {
        Write-Host "[FAIL] Failed: $($emailThreadResponse.message)" -ForegroundColor Red
        $testResults += @{ Test = "Get Email Thread"; Status = "FAIL"; Details = $emailThreadResponse.message }
    }
} catch {
    Write-Host "[FAIL] Error: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Get Email Thread"; Status = "FAIL"; Details = $_.Exception.Message }
}

Write-Host ""

# Test 3: Get Email Statistics
Write-Host "4. Getting email statistics..." -ForegroundColor Yellow
try {
    $statsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/email-ticketing/statistics" -Method GET -Headers $headers

    if ($statsResponse.isSuccess -and $statsResponse.data) {
        Write-Host "[PASS] Email statistics API working" -ForegroundColor Green
        Write-Host "  Total emails: $($statsResponse.data.totalEmails)" -ForegroundColor Gray
        Write-Host "  Inbound: $($statsResponse.data.inboundEmails)" -ForegroundColor Gray
        Write-Host "  Outbound: $($statsResponse.data.outboundEmails)" -ForegroundColor Gray
        Write-Host "  Processed: $($statsResponse.data.processedEmails)" -ForegroundColor Gray
        Write-Host "  Failed: $($statsResponse.data.failedEmails)" -ForegroundColor Gray
        Write-Host "  Last 24h: $($statsResponse.data.emailsLast24Hours)" -ForegroundColor Gray
        Write-Host "  Last 7d: $($statsResponse.data.emailsLast7Days)" -ForegroundColor Gray
        Write-Host "  Last 30d: $($statsResponse.data.emailsLast30Days)" -ForegroundColor Gray
        $testResults += @{ Test = "Get Statistics"; Status = "PASS"; Details = "$($statsResponse.data.totalEmails) total emails" }
    } else {
        Write-Host "[FAIL] Failed: $($statsResponse.message)" -ForegroundColor Red
        $testResults += @{ Test = "Get Statistics"; Status = "FAIL"; Details = $statsResponse.message }
    }
} catch {
    Write-Host "[FAIL] Error: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Get Statistics"; Status = "FAIL"; Details = $_.Exception.Message }
}

Write-Host ""

# Test 4: Test Email Reply Validation (without actually sending)
Write-Host "5. Testing email reply validation..." -ForegroundColor Yellow
Write-Host "   (Will test validation without actually sending email)" -ForegroundColor Gray

$replyRequest = @{
    complaintId = $complaintId
    toEmail = ""  # Invalid - empty email
    subject = "Test Reply"
    body = "This is a test reply"
    isHtml = $false
} | ConvertTo-Json

try {
    $replyResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/email-ticketing/send-reply" -Method POST -Headers $headers -Body $replyRequest -ErrorAction SilentlyContinue

    # Should fail because toEmail is empty
    Write-Host "[FAIL] Validation not working - empty email was accepted" -ForegroundColor Red
    $testResults += @{ Test = "Email Reply Validation"; Status = "FAIL"; Details = "Empty email accepted" }
} catch {
    $errorMessage = $_.Exception.Message
    if ($errorMessage -match "400" -or $errorMessage -match "BadRequest" -or $errorMessage -match "required") {
        Write-Host "[PASS] Validation working correctly - empty email rejected" -ForegroundColor Green
        $testResults += @{ Test = "Email Reply Validation"; Status = "PASS"; Details = "Empty email rejected" }
    } else {
        Write-Host "[FAIL] Unexpected error: $errorMessage" -ForegroundColor Red
        $testResults += @{ Test = "Email Reply Validation"; Status = "FAIL"; Details = $errorMessage }
    }
}

Write-Host ""

# Test 5: Check if EmailResponseHistory table exists
Write-Host "6. Verifying database schema..." -ForegroundColor Yellow
try {
    $query = "SELECT CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'EmailResponseHistory') THEN 1 ELSE 0 END AS TableExists, (SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('EmailResponseHistory')) AS ColumnCount"

    $dbResult = Invoke-Sqlcmd -ServerInstance "PRANA-ASUS\SQLEXPRESS" -Database "ComplaintManagementDb" -Query $query

    if ($dbResult.TableExists -eq 1) {
        Write-Host "[PASS] EmailResponseHistory table exists" -ForegroundColor Green
        Write-Host "  Columns: $($dbResult.ColumnCount)" -ForegroundColor Gray
        $testResults += @{ Test = "Database Schema"; Status = "PASS"; Details = "EmailResponseHistory table with $($dbResult.ColumnCount) columns" }
    } else {
        Write-Host "[FAIL] EmailResponseHistory table not found" -ForegroundColor Red
        $testResults += @{ Test = "Database Schema"; Status = "FAIL"; Details = "Table not found" }
    }
} catch {
    Write-Host "[FAIL] Error: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Database Schema"; Status = "FAIL"; Details = $_.Exception.Message }
}

Write-Host ""

# Test Results Summary
Write-Host "=== Test Results Summary ===" -ForegroundColor Cyan
Write-Host ""

$passCount = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$totalTests = $testResults.Count

$testResults | ForEach-Object {
    $color = if ($_.Status -eq "PASS") { "Green" } else { "Red" }
    $icon = if ($_.Status -eq "PASS") { "[PASS]" } else { "[FAIL]" }
    Write-Host "$icon $($_.Test): $($_.Status)" -ForegroundColor $color
    Write-Host "  Details: $($_.Details)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Total Tests: $totalTests" -ForegroundColor Cyan
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red
$passRate = [math]::Round(($passCount / $totalTests) * 100, 2)
Write-Host "Pass Rate: $passRate%" -ForegroundColor $(if ($passRate -ge 80) { "Green" } else { "Yellow" })

Write-Host ""

# Overall Status
if ($failCount -eq 0) {
    Write-Host "=== [PASS] ALL TESTS PASSED ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "Email Thread Integration Week 1 is fully operational!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Features Verified:" -ForegroundColor Cyan
    Write-Host "  [PASS] Backend API endpoints working" -ForegroundColor Green
    Write-Host "  [PASS] Email thread retrieval functional" -ForegroundColor Green
    Write-Host "  [PASS] Email statistics available" -ForegroundColor Green
    Write-Host "  [PASS] Email reply validation working" -ForegroundColor Green
    Write-Host "  [PASS] Database schema correct" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Test frontend UI with Playwright" -ForegroundColor Gray
    Write-Host "  2. Configure email settings for actual sending" -ForegroundColor Gray
    Write-Host "  3. Deploy to production" -ForegroundColor Gray
    Write-Host "  4. Create user documentation" -ForegroundColor Gray
    Write-Host ""
    exit 0
} else {
    Write-Host "=== [WARN] SOME TESTS FAILED ===" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please review the failed tests above and fix issues." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}
