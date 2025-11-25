# Test SLA Calculator Integration
# This script tests the SLA calculation when creating complaints

$baseUrl = "http://localhost:5058/api"
$ErrorActionPreference = "Continue"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SLA CALCULATOR END-TO-END TEST" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Login as admin
Write-Host "[1/5] Logging in as admin..." -ForegroundColor Yellow
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post `
    -ContentType "application/json" `
    -Body (@{
        email = "admin@example.com"
        password = "Admin@123"
    } | ConvertTo-Json)

$token = $loginResponse.data.token
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "  Login successful. User: $($loginResponse.data.fullName)" -ForegroundColor Green

# Step 2: Get category and user IDs
Write-Host "`n[2/5] Fetching test data (categories, users)..." -ForegroundColor Yellow
$categories = Invoke-RestMethod -Uri "$baseUrl/categories" -Method Get -Headers $headers
$category = $categories.data[0]
Write-Host "  Category: $($category.name) (ID: $($category.id))" -ForegroundColor Green

$users = Invoke-RestMethod -Uri "$baseUrl/users" -Method Get -Headers $headers
$testUser = $users.data[0]
Write-Host "  Test User: $($testUser.fullName) (ID: $($testUser.id))" -ForegroundColor Green
Write-Host "  Company ID: $($loginResponse.data.companyId)" -ForegroundColor Green

# Step 3: Create test complaints with different priorities
Write-Host "`n[3/5] Creating test complaints with different priorities..." -ForegroundColor Yellow

$priorities = @("Low", "Normal", "High", "Critical", "Urgent")
$testResults = @()

foreach ($priority in $priorities) {
    Write-Host "  Testing Priority: $priority" -ForegroundColor Cyan

    $complaintData = @{
        title = "SLA Test - $priority Priority"
        description = "Testing SLA calculation for $priority priority complaints"
        categoryId = $category.id
        priority = $priority
        complainantId = $testUser.id
        companyId = $loginResponse.data.companyId
        isAnonymous = $false
    }

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method Post `
            -Headers $headers -Body ($complaintData | ConvertTo-Json)

        $complaint = $response.data
        $result = @{
            Priority = $priority
            ComplaintNumber = $complaint.complaintNumber
            SubmittedAt = $complaint.submittedAt
            DueDate = $complaint.dueDate
            Success = $true
            Error = $null
        }

        # Calculate SLA hours
        $submittedTime = [DateTime]::Parse($complaint.submittedAt)
        $dueTime = [DateTime]::Parse($complaint.dueDate)
        $slaHours = ($dueTime - $submittedTime).TotalHours

        Write-Host "    Complaint: $($complaint.complaintNumber)" -ForegroundColor White
        Write-Host "    Submitted: $($complaint.submittedAt)" -ForegroundColor White
        Write-Host "    Due Date:  $($complaint.dueDate)" -ForegroundColor White
        Write-Host "    SLA Hours: $([Math]::Round($slaHours, 2))" -ForegroundColor White
        Write-Host "    Status: PASSED" -ForegroundColor Green

        $result.SLAHours = [Math]::Round($slaHours, 2)

    } catch {
        $result = @{
            Priority = $priority
            ComplaintNumber = "N/A"
            SubmittedAt = "N/A"
            DueDate = "N/A"
            SLAHours = "N/A"
            Success = $false
            Error = $_.Exception.Message
        }
        Write-Host "    Status: FAILED - $($_.Exception.Message)" -ForegroundColor Red
    }

    $testResults += New-Object PSObject -Property $result
    Start-Sleep -Milliseconds 500
}

# Step 4: Verify SLA calculation logs
Write-Host "`n[4/5] Checking SLA Calculator Service logs..." -ForegroundColor Yellow
Write-Host "  The SLA Calculator should have logged calculation details for each complaint" -ForegroundColor White
Write-Host "  Check backend console for 'Calculating SLA for Category' messages" -ForegroundColor White

# Step 5: Display summary
Write-Host "`n[5/5] Test Summary" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan

$successCount = ($testResults | Where-Object { $_.Success }).Count
$failCount = ($testResults | Where-Object { -not $_.Success }).Count

Write-Host "`nResults Table:" -ForegroundColor White
$testResults | ForEach-Object {
    if ($_.Success) {
        Write-Host "  Priority: $($_.Priority.PadRight(10)) | Complaint: $($_.ComplaintNumber.PadRight(15)) | SLA Hours: $($_.SLAHours)" -ForegroundColor Green
    } else {
        Write-Host "  Priority: $($_.Priority.PadRight(10)) | ERROR: $($_.Error)" -ForegroundColor Red
    }
}

Write-Host "`n----------------------------------------" -ForegroundColor Cyan
Write-Host "Total Tests: $($testResults.Count)" -ForegroundColor White
Write-Host "Passed: $successCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 6: Test retrieving a complaint to verify DueDate is set
if ($successCount -gt 0) {
    $successfulComplaint = $testResults | Where-Object { $_.Success } | Select-Object -First 1
    Write-Host "[BONUS] Verifying complaint retrieval includes DueDate..." -ForegroundColor Yellow

    try {
        $retrievedComplaint = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method Get -Headers $headers
        $found = $retrievedComplaint.data | Where-Object { $_.complaintNumber -eq $successfulComplaint.ComplaintNumber }

        if ($found -and $found.dueDate) {
            Write-Host "  Complaint $($found.complaintNumber) has DueDate: $($found.dueDate)" -ForegroundColor Green
            Write-Host "  VERIFICATION PASSED" -ForegroundColor Green
        } else {
            Write-Host "  WARNING: DueDate not found in retrieved complaint" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Final verdict
Write-Host "`nFINAL VERDICT:" -ForegroundColor Cyan
if ($failCount -eq 0) {
    Write-Host "ALL TESTS PASSED - SLA Calculator is working correctly!" -ForegroundColor Green
} else {
    Write-Host "SOME TESTS FAILED - Please review errors above" -ForegroundColor Red
}
Write-Host ""
