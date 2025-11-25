# Test Week 2 Auto-Response System
# This will create a new complaint and check if auto-acknowledgment email is triggered

$token = Get-Content ".working-token" -Raw
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Week 2 Auto-Response System E2E Test" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Step 1: Check notification rules for COMPLAINT_CREATED
Write-Host "`n[Step 1] Checking COMPLAINT_CREATED notification rules..." -ForegroundColor Yellow
try {
    $rulesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/event-communication-rules" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    $complaintCreatedRules = $rulesResponse.data | Where-Object {
        $_.eventType.code -eq "COMPLAINT_CREATED" -and $_.isActive
    }

    if ($complaintCreatedRules) {
        Write-Host "  Found $($complaintCreatedRules.Count) active COMPLAINT_CREATED rules:" -ForegroundColor Green
        $complaintCreatedRules | ForEach-Object {
            Write-Host "    - $($_.ruleName) (Priority: $($_.priority), Template: $($_.template.name))" -ForegroundColor White
        }
    } else {
        Write-Host "  No active COMPLAINT_CREATED rules found" -ForegroundColor Red
        Write-Host "  Auto-response will not work without notification rules!" -ForegroundColor Red
    }
} catch {
    Write-Host "  Error checking rules: $_" -ForegroundColor Red
}

# Step 2: Get current complaint count
Write-Host "`n[Step 2] Getting current complaint count..." -ForegroundColor Yellow
try {
    $beforeResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints?pageNumber=1&pageSize=1" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    $beforeCount = $beforeResponse.data.totalCount
    Write-Host "  Current total complaints: $beforeCount" -ForegroundColor White
} catch {
    Write-Host "  Error getting complaint count: $_" -ForegroundColor Red
    exit
}

# Step 3: Create test complaint
Write-Host "`n[Step 3] Creating test complaint to trigger auto-response..." -ForegroundColor Yellow

$newComplaint = @{
    title = "AUTO-RESPONSE TEST - Week 2 Verification $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    description = "This is a test complaint created to verify the Week 2 Auto-Response System. If configured correctly, an acknowledgment email should be sent automatically to the complainant."
    categoryId = "d4e4c02b-5f75-4f58-8f0c-3e1d6a6c0c8b"  # Default category
    priorityId = "98a8b5c3-1e2f-4d8c-a5e9-2c3d4e5f6a7b"  # Normal priority
    contactEmail = "test-auto-response@example.com"
    contactPhone = "+1234567890"
    expectedResolutionDate = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" `
        -Method POST `
        -Headers @{Authorization="Bearer $token"} `
        -Body $newComplaint `
        -ContentType "application/json"

    if ($createResponse.isSuccess) {
        $newComplaintId = $createResponse.data.id
        $newComplaintNumber = $createResponse.data.complaintNumber

        Write-Host "  SUCCESS: Complaint created!" -ForegroundColor Green
        Write-Host "    Complaint Number: $newComplaintNumber" -ForegroundColor Green
        Write-Host "    Complaint ID: $newComplaintId" -ForegroundColor Green
        Write-Host "    Expected Email To: test-auto-response@example.com" -ForegroundColor Green
    } else {
        Write-Host "  FAILED: $($createResponse.message)" -ForegroundColor Red
        exit
    }
} catch {
    Write-Host "  Error creating complaint: $_" -ForegroundColor Red
    exit
}

# Step 4: Wait a moment for async notification processing
Write-Host "`n[Step 4] Waiting for async notification processing..." -ForegroundColor Yellow
Write-Host "  Waiting 3 seconds..." -ForegroundColor Gray
Start-Sleep -Seconds 3

# Step 5: Check communication logs
Write-Host "`n[Step 5] Checking communication logs for sent notifications..." -ForegroundColor Yellow
try {
    $logsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/communication-logs?entityId=$newComplaintId" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json" `
        -ErrorAction SilentlyContinue

    if ($logsResponse.isSuccess -and $logsResponse.data.Count -gt 0) {
        Write-Host "  FOUND $($logsResponse.data.Count) notification(s) sent!" -ForegroundColor Green

        foreach ($log in $logsResponse.data) {
            Write-Host "`n  Notification Details:" -ForegroundColor Cyan
            Write-Host "    To: $($log.recipientEmail)" -ForegroundColor White
            Write-Host "    Subject: $($log.subject)" -ForegroundColor White
            Write-Host "    Status: $($log.status)" -ForegroundColor $(if ($log.status -eq "Sent") { "Green" } else { "Red" })
            Write-Host "    Sent At: $($log.sentAt)" -ForegroundColor White
            if ($log.errorMessage) {
                Write-Host "    Error: $($log.errorMessage)" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "  No notifications found in communication logs" -ForegroundColor Yellow
        Write-Host "  This could mean:" -ForegroundColor Yellow
        Write-Host "    1. Communication logs API doesn't exist" -ForegroundColor Gray
        Write-Host "    2. Notifications weren't triggered" -ForegroundColor Gray
        Write-Host "    3. Async processing not complete yet" -ForegroundColor Gray
    }
} catch {
    Write-Host "  Communication logs API not available or error: $_" -ForegroundColor Yellow
}

# Step 6: Check backend logs (if running)
Write-Host "`n[Step 6] Checking if backend logged notification dispatch..." -ForegroundColor Yellow
Write-Host "  (Check backend console for log messages about:" -ForegroundColor Gray
Write-Host "   'Dispatching notifications for event COMPLAINT_CREATED')" -ForegroundColor Gray

# Summary
Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Complaint Created: $newComplaintNumber" -ForegroundColor Green
Write-Host "Expected Email: test-auto-response@example.com" -ForegroundColor White
Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Check backend logs for notification dispatch messages" -ForegroundColor White
Write-Host "2. Check email inbox (if real email configured)" -ForegroundColor White
Write-Host "3. Verify CommunicationLogs table in database" -ForegroundColor White
Write-Host "`nNote: Gmail SMTP may require App Password configuration" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
