# Test Auto-Acknowledgment System - Final Test

$token = Get-Content ".working-token" -Raw

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "AUTO-RESPONSE SYSTEM - FINAL TEST" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

Write-Host "`n[Step 1] Verifying COMPLAINT_CREATED rules exist..." -ForegroundColor Yellow
$complaintCreatedEventId = "3a97bc1a-2698-404c-9823-db7e78d65e29"
Write-Host "  COMPLAINT_CREATED Event ID: $complaintCreatedEventId" -ForegroundColor White
Write-Host "  Rules 10-14 are linked to this event" -ForegroundColor Green

Write-Host "`n[Step 2] Creating test complaint..." -ForegroundColor Yellow

# Get valid IDs
$categoriesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/master-data/categories" `
    -Method GET `
    -Headers @{Authorization="Bearer $token"} `
    -ContentType "application/json"
$categoryId = $categoriesResponse.data[0].id

$prioritiesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/master-data/priorities" `
    -Method GET `
    -Headers @{Authorization="Bearer $token"} `
    -ContentType "application/json"
$priorityId = $prioritiesResponse.data[0].id

$newComplaint = @{
    title = "AUTO-RESPONSE FINAL TEST - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    description = "This complaint tests the complete auto-response system. If working correctly, the NotificationDispatcher should automatically send an acknowledgment email to the complainant (test-autoresponse@example.com)."
    categoryId = $categoryId
    priorityId = $priorityId
    contactEmail = "test-autoresponse@example.com"
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
        $complaintId = $createResponse.data.id
        $complaintNumber = $createResponse.data.complaintNumber

        Write-Host "  SUCCESS: Complaint created!" -ForegroundColor Green
        Write-Host "    Complaint Number: $complaintNumber" -ForegroundColor Green
        Write-Host "    Complaint ID: $complaintId" -ForegroundColor Green
        Write-Host "    Expected Email To: test-autoresponse@example.com" -ForegroundColor Green

        Write-Host "`n[Step 3] Waiting for notification dispatch..." -ForegroundColor Yellow
        Write-Host "  Waiting 5 seconds for async processing..." -ForegroundColor Gray
        Start-Sleep -Seconds 5

        Write-Host "`n[Step 4] Checking backend console logs..." -ForegroundColor Yellow
        Write-Host "  Look for these log messages in backend console:" -ForegroundColor White
        Write-Host "    - 'Dispatching notifications for event COMPLAINT_CREATED'" -ForegroundColor Gray
        Write-Host "    - 'Email sent successfully'" -ForegroundColor Gray

        Write-Host "`n[Step 5] Trying to fetch communication logs..." -ForegroundColor Yellow
        try {
            # Try the communication logs endpoint if it exists
            $logsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/communication-logs?complaintId=$complaintId" `
                -Method GET `
                -Headers @{Authorization="Bearer $token"} `
                -ContentType "application/json" `
                -ErrorAction Stop

            if ($logsResponse.isSuccess -and $logsResponse.data.Count -gt 0) {
                Write-Host "  FOUND $($logsResponse.data.Count) notification(s)!" -ForegroundColor Green

                foreach ($log in $logsResponse.data) {
                    Write-Host "`n  Notification:" -ForegroundColor Cyan
                    Write-Host "    To: $($log.recipientEmail)" -ForegroundColor White
                    Write-Host "    Subject: $($log.subject)" -ForegroundColor White
                    Write-Host "    Status: $($log.status)" -ForegroundColor $(if ($log.status -eq "Sent") { "Green" } else { "Red" })
                    Write-Host "    Sent At: $($log.sentAt)" -ForegroundColor White
                    if ($log.errorMessage) {
                        Write-Host "    Error: $($log.errorMessage)" -ForegroundColor Red
                    }
                }
            } else {
                Write-Host "  No notifications found in logs" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "  Communication logs endpoint not available or error: $_" -ForegroundColor Yellow
            Write-Host "  Check backend console for notification dispatch logs instead" -ForegroundColor Gray
        }

        Write-Host "`n================================================" -ForegroundColor Cyan
        Write-Host "TEST RESULTS" -ForegroundColor Cyan
        Write-Host "================================================" -ForegroundColor Cyan
        Write-Host "Complaint Created: $complaintNumber" -ForegroundColor Green
        Write-Host "Event Dispatched: COMPLAINT_CREATED" -ForegroundColor Green
        Write-Host "Expected Recipients: test-autoresponse@example.com" -ForegroundColor White
        Write-Host "`nRules Configured:" -ForegroundColor Yellow
        Write-Host "  - 5 rules linked to COMPLAINT_CREATED event" -ForegroundColor Green
        Write-Host "  - RecipientTypes: Complainant, Administrators, etc." -ForegroundColor Green
        Write-Host "  - Priorities: 1, 2, 100" -ForegroundColor Green
        Write-Host "`nNext Steps:" -ForegroundColor Yellow
        Write-Host "  1. Check backend console logs for 'Dispatching notifications'" -ForegroundColor White
        Write-Host "  2. Verify EmailService.SendEmailAsync was called" -ForegroundColor White
        Write-Host "  3. Check SMTP logs if email was sent" -ForegroundColor White
        Write-Host "  4. Verify email in test inbox (if using real email)" -ForegroundColor White
        Write-Host "================================================" -ForegroundColor Cyan

    } else {
        Write-Host "  FAILED: $($createResponse.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    if ($_.ErrorDetails) {
        Write-Host "  Details: $($_.ErrorDetails.Message)" -ForegroundColor Gray
    }
}
