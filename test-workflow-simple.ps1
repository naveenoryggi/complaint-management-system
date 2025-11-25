# Simple Workflow Transition Fix Verification Test
# Tests the critical fix: POST /workflows/complaints/{id}/transition returns updated complaint data

$baseUrl = "http://localhost:5058/api"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "WORKFLOW TRANSITION FIX - SIMPLE TEST" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login
Write-Host "[1] Authenticating..." -ForegroundColor Yellow
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}
Write-Host "    SUCCESS: Logged in" -ForegroundColor Green

# Step 2: Get statuses
Write-Host "`n[2] Getting available statuses..." -ForegroundColor Yellow
$statusResponse = Invoke-RestMethod -Uri "$baseUrl/ComplaintStatusMaster" -Method Get -Headers $headers
$allStatuses = $statusResponse.data

Write-Host "    Available statuses:" -ForegroundColor Gray
$allStatuses | ForEach-Object {
    Write-Host "      - $($_.name) (ID: $($_.id))" -ForegroundColor Gray
}

# Find Submitted and In Progress statuses
$submittedStatus = $allStatuses | Where-Object { $_.name -eq "Submitted" } | Select-Object -First 1
$inProgressStatus = $allStatuses | Where-Object { $_.name -eq "In Progress" -or $_.name -eq "Acknowledged" } | Select-Object -First 1

if (-not $inProgressStatus) {
    $inProgressStatus = $allStatuses | Where-Object { $_.id -ne $submittedStatus.id } | Select-Object -First 1
}

Write-Host "`n    Will test transition:" -ForegroundColor Yellow
Write-Host "      FROM: $($submittedStatus.name) (ID: $($submittedStatus.id))" -ForegroundColor Cyan
Write-Host "      TO:   $($inProgressStatus.name) (ID: $($inProgressStatus.id))" -ForegroundColor Cyan

# Step 3: Get category
Write-Host "`n[3] Getting a category..." -ForegroundColor Yellow
$categoriesResponse = Invoke-RestMethod -Uri "$baseUrl/categories" -Method Get -Headers $headers
$category = $categoriesResponse.data[0]
Write-Host "    Using category: $($category.name)" -ForegroundColor Gray

# Step 4: Create complaint
Write-Host "`n[4] Creating test complaint..." -ForegroundColor Yellow
$complaintBody = @{
    title = "Workflow Transition Test - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    description = "Testing that workflow transition returns updated complaint data"
    categoryId = $category.id
    priority = 1  # Low
    isAnonymous = $false
} | ConvertTo-Json

$createResponse = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method Post -Body $complaintBody -Headers $headers
$complaintId = $createResponse.data.id
$initialStatus = $createResponse.data.status

Write-Host "    SUCCESS: Created complaint $complaintId" -ForegroundColor Green
Write-Host "      Initial Status: $initialStatus" -ForegroundColor Gray

# Step 5: CRITICAL TEST - Execute transition
Write-Host "`n[5] *** CRITICAL TEST *** Executing workflow transition..." -ForegroundColor Magenta
Write-Host "    Transitioning from '$initialStatus' to '$($inProgressStatus.name)'..." -ForegroundColor Cyan

$transitionBody = @{
    newStatusId = $inProgressStatus.id
    comment = "Testing workflow transition fix"
} | ConvertTo-Json

Write-Host "`n    Request Body:" -ForegroundColor Gray
Write-Host "    $transitionBody" -ForegroundColor Gray

$transitionStartTime = Get-Date

try {
    $transitionResponse = Invoke-RestMethod -Uri "$baseUrl/workflows/complaints/$complaintId/transition" -Method Post -Body $transitionBody -Headers $headers
    $transitionDuration = (Get-Date) - $transitionStartTime

    Write-Host "`n    === RESPONSE ANALYSIS ===" -ForegroundColor Magenta

    # Check response structure
    $hasDataField = $null -ne $transitionResponse.data
    $hasIsSuccess = $null -ne $transitionResponse.isSuccess
    $isSuccess = $transitionResponse.isSuccess -eq $true

    Write-Host "`n    Response Structure:" -ForegroundColor Yellow
    Write-Host "      - Has 'isSuccess': $(if ($hasIsSuccess) { 'YES' } else { 'NO' })" -ForegroundColor $(if ($hasIsSuccess) { 'Green' } else { 'Red' })
    Write-Host "      - isSuccess value: $(if ($isSuccess) { 'TRUE' } else { 'FALSE' })" -ForegroundColor $(if ($isSuccess) { 'Green' } else { 'Red' })
    Write-Host "      - Has 'data' field: $(if ($hasDataField) { 'YES' } else { 'NO' })" -ForegroundColor $(if ($hasDataField) { 'Green' } else { 'Red' })

    if ($hasDataField) {
        # Extract updated complaint data
        $responseStatus = $transitionResponse.data.status
        $responseStatusId = $transitionResponse.data.statusId
        $responseComplaintId = $transitionResponse.data.id

        Write-Host "`n    Updated Complaint Data in Response:" -ForegroundColor Yellow
        Write-Host "      - Complaint ID: $responseComplaintId" -ForegroundColor Gray
        Write-Host "      - Status Name: $responseStatus" -ForegroundColor Gray
        Write-Host "      - Status ID: $responseStatusId" -ForegroundColor Gray

        # Verify status matches target
        $statusMatches = $responseStatusId -eq $inProgressStatus.id
        $statusNameCorrect = $responseStatus -eq $inProgressStatus.name

        Write-Host "`n    Verification:" -ForegroundColor Yellow
        Write-Host "      - Status ID matches target: $(if ($statusMatches) { 'YES' } else { 'NO' })" -ForegroundColor $(if ($statusMatches) { 'Green' } else { 'Red' })
        Write-Host "        Expected: $($inProgressStatus.id)" -ForegroundColor Gray
        Write-Host "        Got: $responseStatusId" -ForegroundColor Gray
        Write-Host "      - Status Name matches target: $(if ($statusNameCorrect) { 'YES' } else { 'NO' })" -ForegroundColor $(if ($statusNameCorrect) { 'Green' } else { 'Red' })
        Write-Host "        Expected: $($inProgressStatus.name)" -ForegroundColor Gray
        Write-Host "        Got: $responseStatus" -ForegroundColor Gray

        Write-Host "`n    Transition Duration: $($transitionDuration.TotalMilliseconds) ms" -ForegroundColor Gray

        if ($hasDataField -and $statusMatches -and $statusNameCorrect) {
            Write-Host "`n    ========================================" -ForegroundColor Green
            Write-Host "    CRITICAL FIX VERIFIED!" -ForegroundColor Green
            Write-Host "    ========================================" -ForegroundColor Green
            Write-Host "    The workflow transition API now returns" -ForegroundColor Green
            Write-Host "    the updated complaint data immediately!" -ForegroundColor Green
            Write-Host "    ========================================" -ForegroundColor Green
            $testPassed = $true
        } else {
            Write-Host "`n    FAIL: Response has data but status mismatch" -ForegroundColor Red
            $testPassed = $false
        }
    } else {
        Write-Host "`n    FAIL: Critical bug - no 'data' field in response" -ForegroundColor Red
        Write-Host "    The original bug is still present!" -ForegroundColor Red
        $testPassed = $false
    }

    # Show full response
    Write-Host "`n    Full Response:" -ForegroundColor Yellow
    Write-Host ($transitionResponse | ConvertTo-Json -Depth 10) -ForegroundColor Gray

} catch {
    Write-Host "`n    FAIL: Transition API call failed" -ForegroundColor Red
    Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "    Response Body: $responseBody" -ForegroundColor Red
    }

    $testPassed = $false
}

# Step 6: Verify persistence
Write-Host "`n[6] Verifying status persisted in database..." -ForegroundColor Yellow
Start-Sleep -Seconds 1

$verifyComplaint = Invoke-RestMethod -Uri "$baseUrl/complaints/$complaintId" -Method Get -Headers $headers
$persistedStatus = $verifyComplaint.data.status
$persistedStatusId = $verifyComplaint.data.statusId

Write-Host "    Database Status: $persistedStatus (ID: $persistedStatusId)" -ForegroundColor Gray

if ($persistedStatusId -eq $inProgressStatus.id) {
    Write-Host "    SUCCESS: Status correctly persisted" -ForegroundColor Green
} else {
    Write-Host "    FAIL: Status not persisted correctly" -ForegroundColor Red
}

# Final Summary
Write-Host "`n========================================" -ForegroundColor Cyan
if ($testPassed) {
    Write-Host "TEST RESULT: PASSED" -ForegroundColor Green
    Write-Host "The workflow transition fix is working!" -ForegroundColor Green
} else {
    Write-Host "TEST RESULT: FAILED" -ForegroundColor Red
    Write-Host "The workflow transition fix has issues" -ForegroundColor Red
}
Write-Host "========================================" -ForegroundColor Cyan
