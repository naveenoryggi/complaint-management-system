# Simple Auto-Acknowledgment Test - Using existing complaint data

$token = Get-Content ".working-token" -Raw

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "AUTO-RESPONSE SYSTEM - SIMPLE TEST" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

Write-Host "`n[Step 1] Getting existing complaint for reference..." -ForegroundColor Yellow
$existingResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints?pageNumber=1&pageSize=1" `
    -Method GET `
    -Headers @{Authorization="Bearer $token"} `
    -ContentType "application/json"

$referenceComplaint = $existingResponse.data.items[0]
$categoryId = $referenceComplaint.categoryId
$priorityId = $referenceComplaint.priorityId
Write-Host "  Using CategoryId: $categoryId" -ForegroundColor White
Write-Host "  Using PriorityId: $priorityId" -ForegroundColor White

Write-Host "`n[Step 2] Creating test complaint..." -ForegroundColor Yellow

$newComplaint = @{
    title = "AUTO-RESPONSE TEST - $(Get-Date -Format 'HH:mm:ss')"
    description = "Testing auto-response system. Expected: Automatic acknowledgment email to test-autoresponse@example.com"
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

        Write-Host "  ✓ SUCCESS: Complaint created!" -ForegroundColor Green
        Write-Host "    Number: $complaintNumber" -ForegroundColor Cyan
        Write-Host "    ID: $complaintId" -ForegroundColor Gray

        Write-Host "`n[Step 3] Waiting for notification dispatch (5 seconds)..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5

        Write-Host "`n================================================" -ForegroundColor Cyan
        Write-Host "TEST COMPLETE" -ForegroundColor Cyan
        Write-Host "================================================" -ForegroundColor Cyan
        Write-Host "Complaint: $complaintNumber" -ForegroundColor White
        Write-Host "Expected: Auto-acknowledgment email sent" -ForegroundColor White
        Write-Host "Recipient: test-autoresponse@example.com" -ForegroundColor White
        Write-Host "`nCheck Backend Console For:" -ForegroundColor Yellow
        Write-Host "  - 'Dispatching notifications for event COMPLAINT_CREATED'" -ForegroundColor Gray
        Write-Host "  - 'Email sent successfully'" -ForegroundColor Gray
        Write-Host "  - Any errors in notification dispatch" -ForegroundColor Gray
        Write-Host "================================================" -ForegroundColor Cyan

    } else {
        Write-Host "  ✗ FAILED: $($createResponse.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ✗ ERROR: $_" -ForegroundColor Red
}
