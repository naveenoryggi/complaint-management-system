# Workflow Transition Fix Verification Test
# Critical: Verify that POST /workflows/complaints/{id}/transition returns updated complaint data

$baseUrl = "http://localhost:5058/api"
$testResults = @()
$startTime = Get-Date

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "WORKFLOW TRANSITION FIX VERIFICATION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Helper function to format JSON responses
function Format-JsonResponse {
    param($response)
    if ($response) {
        return ($response | ConvertTo-Json -Depth 10)
    }
    return "No response"
}

# Helper function to add test result
function Add-TestResult {
    param(
        [string]$testName,
        [string]$status,
        [string]$details
    )
    $script:testResults += [PSCustomObject]@{
        Test = $testName
        Status = $status
        Details = $details
    }
}

try {
    # Step 1: Authenticate
    Write-Host "[STEP 1] Authenticating..." -ForegroundColor Yellow
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"

    if (-not $loginResponse.data.token) {
        throw "Authentication failed - no token received"
    }

    $token = $loginResponse.data.token
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    Write-Host "  SUCCESS: Authentication successful" -ForegroundColor Green
    Add-TestResult "Authentication" "PASS" "Token obtained successfully"

    # Step 2: Get categories to find one with workflow
    Write-Host "`n[STEP 2] Getting categories with workflows..." -ForegroundColor Yellow
    $categoriesResponse = Invoke-RestMethod -Uri "$baseUrl/categories" -Method Get -Headers $headers

    # Find a category (any category will do for testing)
    $testCategory = $categoriesResponse.data[0]
    if (-not $testCategory) {
        throw "No categories found"
    }

    Write-Host "  SUCCESS: Using category: $($testCategory.name) (ID: $($testCategory.id))" -ForegroundColor Green
    Add-TestResult "Get Category" "PASS" "Category: $($testCategory.name)"

    # Step 3: Get status masters to use for complaint creation
    Write-Host "`n[STEP 3] Getting status masters..." -ForegroundColor Yellow
    $statusResponse = Invoke-RestMethod -Uri "$baseUrl/ComplaintStatusMaster" -Method Get -Headers $headers

    # Find initial status (typically "New" or "Submitted")
    $initialStatus = $statusResponse.data | Where-Object { $_.name -eq "Submitted" -or $_.name -eq "New" } | Select-Object -First 1
    if (-not $initialStatus) {
        $initialStatus = $statusResponse.data[0]
    }

    Write-Host "  SUCCESS: Initial status: $($initialStatus.name) (ID: $($initialStatus.id))" -ForegroundColor Green
    Add-TestResult "Get Status Masters" "PASS" "Initial status: $($initialStatus.name)"

    # Step 4: Create a test complaint
    Write-Host "`n[STEP 4] Creating test complaint..." -ForegroundColor Yellow
    $complaintBody = @{
        title = "Workflow Transition Test - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        description = "Testing that workflow transition returns updated complaint data"
        categoryId = $testCategory.id
        priority = 1
        isAnonymous = $false
    } | ConvertTo-Json

    $createResponse = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method Post -Body $complaintBody -Headers $headers

    if (-not $createResponse.data) {
        throw "Failed to create complaint"
    }

    $complaintId = $createResponse.data.id
    $categoryId = $createResponse.data.categoryId
    $currentStatusId = if ($createResponse.data.statusId) { $createResponse.data.statusId } else { $initialStatus.id }
    $currentStatusName = $createResponse.data.status

    Write-Host "  SUCCESS: Complaint created" -ForegroundColor Green
    Write-Host "    - ID: $complaintId" -ForegroundColor Gray
    Write-Host "    - Initial Status: $currentStatusName (ID: $currentStatusId)" -ForegroundColor Gray
    Add-TestResult "Create Complaint" "PASS" "Complaint ID: $complaintId, Initial Status: $currentStatusName"

    # Step 5: Get initial complaint state
    Write-Host "`n[STEP 5] Getting initial complaint state..." -ForegroundColor Yellow
    $initialComplaint = Invoke-RestMethod -Uri "$baseUrl/complaints/$complaintId" -Method Get -Headers $headers

    Write-Host "  SUCCESS: Initial complaint state retrieved" -ForegroundColor Green
    Write-Host "    - Status: $($initialComplaint.data.status)" -ForegroundColor Gray
    Write-Host "    - StatusId: $($initialComplaint.data.statusId)" -ForegroundColor Gray
    Add-TestResult "Get Initial State" "PASS" "Status: $($initialComplaint.data.status)"

    # Step 6: Get available transitions
    Write-Host "`n[STEP 6] Getting available transitions..." -ForegroundColor Yellow
    $transitionsUrl = ($baseUrl + "/workflows/allowed-transitions?categoryId=" + $categoryId + "&currentStatusId=" + $currentStatusId)

    $nextStatusId = $null
    $nextStatusName = $null

    try {
        $transitionsResponse = Invoke-RestMethod -Uri $transitionsUrl -Method Get -Headers $headers

        if (-not $transitionsResponse.data -or $transitionsResponse.data.Count -eq 0) {
            Write-Host "  WARNING: No workflow transitions configured for this category/status" -ForegroundColor Yellow
            Write-Host "    Will use fallback approach with manual status selection..." -ForegroundColor Yellow

            # Find "In Progress" status as fallback
            $inProgressStatus = $statusResponse.data | Where-Object { $_.name -eq "In Progress" -and $_.id -ne $currentStatusId } | Select-Object -First 1
            if (-not $inProgressStatus) {
                # Try "Acknowledged" status
                $inProgressStatus = $statusResponse.data | Where-Object { $_.name -eq "Acknowledged" -and $_.id -ne $currentStatusId } | Select-Object -First 1
            }
            if (-not $inProgressStatus) {
                # Use any different status
                $inProgressStatus = $statusResponse.data | Where-Object { $_.id -ne $currentStatusId } | Select-Object -First 1
            }

            if ($inProgressStatus) {
                Write-Host "    Will transition to: $($inProgressStatus.name) (ID: $($inProgressStatus.id))" -ForegroundColor Yellow
                $nextStatusId = $inProgressStatus.id
                $nextStatusName = $inProgressStatus.name
            } else {
                throw "No other status available for transition"
            }
        } else {
            $nextTransition = $transitionsResponse.data[0]
            $nextStatusId = $nextTransition.toStatusId
            $nextStatusName = $nextTransition.toStatusName

            Write-Host "  SUCCESS: Available transitions found" -ForegroundColor Green
            Write-Host "    - Next status: $nextStatusName (ID: $nextStatusId)" -ForegroundColor Gray
        }

        Add-TestResult "Get Transitions" "PASS" "Target status: $nextStatusName"
    } catch {
        Write-Host "  WARNING: Workflow API not available or no transitions configured" -ForegroundColor Yellow
        # Fallback: use any different status (make sure it's different!)
        $nextStatus = $statusResponse.data | Where-Object { $_.id -ne $currentStatusId } | Select-Object -First 1
        if ($nextStatus) {
            $nextStatusId = $nextStatus.id
            $nextStatusName = $nextStatus.name
            Write-Host "    Using fallback status: $nextStatusName (ID: $nextStatusId)" -ForegroundColor Yellow
            Add-TestResult "Get Transitions" "WARN" "Using fallback status: $nextStatusName"
        } else {
            throw "No alternative status available for transition testing"
        }
    }

    # Step 7: CRITICAL TEST - Execute workflow transition
    Write-Host "`n[STEP 7] *** CRITICAL TEST *** Executing workflow transition..." -ForegroundColor Magenta
    Write-Host "  Current Status: $currentStatusName (ID: $currentStatusId)" -ForegroundColor Cyan
    Write-Host "  Target Status: $nextStatusName (ID: $nextStatusId)" -ForegroundColor Cyan

    $transitionBody = @{
        newStatusId = $nextStatusId
        comment = "Testing workflow transition fix - verifying updated complaint is returned"
    } | ConvertTo-Json

    $transitionStartTime = Get-Date

    try {
        $transitionResponse = Invoke-RestMethod -Uri "$baseUrl/workflows/complaints/$complaintId/transition" -Method Post -Body $transitionBody -Headers $headers
        $transitionDuration = (Get-Date) - $transitionStartTime

        Write-Host "`n  === CRITICAL FIX VERIFICATION ===" -ForegroundColor Magenta

        # Check 1: Response structure
        $hasDataField = $null -ne $transitionResponse.data
        $hasIsSuccess = $null -ne $transitionResponse.isSuccess
        $isSuccess = $transitionResponse.isSuccess -eq $true

        Write-Host "`n  Response Structure:" -ForegroundColor Yellow
        Write-Host "    - Has 'isSuccess' field: $(if ($hasIsSuccess) { 'YES' } else { 'NO' })" -ForegroundColor $(if ($hasIsSuccess) { 'Green' } else { 'Red' })
        Write-Host "    - isSuccess value: $(if ($isSuccess) { 'TRUE' } else { 'FALSE' })" -ForegroundColor $(if ($isSuccess) { 'Green' } else { 'Red' })
        Write-Host "    - Has 'data' field: $(if ($hasDataField) { 'YES - FIX VERIFIED!' } else { 'NO - BUG PRESENT!' })" -ForegroundColor $(if ($hasDataField) { 'Green' } else { 'Red' })

        if ($hasDataField) {
            # Check 2: Updated complaint object
            $responseStatus = $transitionResponse.data.status
            $responseStatusId = $transitionResponse.data.statusId
            $responseComplaintId = $transitionResponse.data.id

            Write-Host "`n  Updated Complaint Data:" -ForegroundColor Yellow
            Write-Host "    - Complaint ID: $responseComplaintId" -ForegroundColor Gray
            Write-Host "    - Status Name: $responseStatus" -ForegroundColor Gray
            Write-Host "    - Status ID: $responseStatusId" -ForegroundColor Gray

            # Check 3: Status matches the new status
            $statusMatches = $responseStatusId -eq $nextStatusId
            $statusNameCorrect = $responseStatus -eq $nextStatusName

            Write-Host "`n  Status Update Verification:" -ForegroundColor Yellow
            Write-Host "    - Status ID matches target: $(if ($statusMatches) { 'YES' } else { 'NO' })" -ForegroundColor $(if ($statusMatches) { 'Green' } else { 'Red' })
            Write-Host "      Expected: $nextStatusId" -ForegroundColor Gray
            Write-Host "      Got: $responseStatusId" -ForegroundColor Gray
            Write-Host "    - Status Name matches target: $(if ($statusNameCorrect) { 'YES' } else { 'NO' })" -ForegroundColor $(if ($statusNameCorrect) { 'Green' } else { 'Red' })
            Write-Host "      Expected: $nextStatusName" -ForegroundColor Gray
            Write-Host "      Got: $responseStatus" -ForegroundColor Gray

            Write-Host "`n  Transition Duration: $($transitionDuration.TotalMilliseconds) ms" -ForegroundColor Gray

            if ($hasDataField -and $statusMatches -and $statusNameCorrect) {
                Write-Host "`n  SUCCESS: CRITICAL FIX VERIFIED - Transition returns updated complaint!" -ForegroundColor Green
                Add-TestResult "Workflow Transition (CRITICAL FIX)" "PASS" "Response includes updated complaint with correct status"
            } else {
                Write-Host "`n  FAIL: CRITICAL FIX INCOMPLETE - Transition response has issues!" -ForegroundColor Red
                Add-TestResult "Workflow Transition (CRITICAL FIX)" "FAIL" "Response has updated data but incorrect status"
            }
        } else {
            Write-Host "`n  FAIL: CRITICAL BUG - No 'data' field in response!" -ForegroundColor Red
            Write-Host "  This is the original bug - transition doesn't return updated complaint" -ForegroundColor Red
            Add-TestResult "Workflow Transition (CRITICAL FIX)" "FAIL" "Response missing 'data' field with updated complaint"
        }

        # Full response for debugging
        Write-Host "`n  Full Response:" -ForegroundColor Yellow
        Write-Host "  $(Format-JsonResponse $transitionResponse)" -ForegroundColor Gray

    } catch {
        $transitionDuration = (Get-Date) - $transitionStartTime
        Write-Host "`n  FAIL: Transition API call failed" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red

        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "  Response Body: $responseBody" -ForegroundColor Red
        }

        Add-TestResult "Workflow Transition (CRITICAL FIX)" "FAIL" "API call failed: $($_.Exception.Message)"
    }

    # Step 8: Verify status persisted in database
    Write-Host "`n[STEP 8] Verifying status persisted in database..." -ForegroundColor Yellow
    Start-Sleep -Seconds 1  # Small delay to ensure database update

    $verifyComplaint = Invoke-RestMethod -Uri "$baseUrl/complaints/$complaintId" -Method Get -Headers $headers

    $persistedStatus = $verifyComplaint.data.status
    $persistedStatusId = $verifyComplaint.data.statusId

    Write-Host "  Database State:" -ForegroundColor Yellow
    Write-Host "    - Status: $persistedStatus" -ForegroundColor Gray
    Write-Host "    - StatusId: $persistedStatusId" -ForegroundColor Gray

    $persistenceCorrect = $persistedStatusId -eq $nextStatusId

    if ($persistenceCorrect) {
        Write-Host "  SUCCESS: Status correctly persisted in database" -ForegroundColor Green
        Add-TestResult "Verify Persistence" "PASS" "Status updated in database: $persistedStatus"
    } else {
        Write-Host "  FAIL: Status not correctly persisted" -ForegroundColor Red
        Write-Host "    Expected: $nextStatusId" -ForegroundColor Red
        Write-Host "    Got: $persistedStatusId" -ForegroundColor Red
        Add-TestResult "Verify Persistence" "FAIL" "Status mismatch in database"
    }

    # Step 9: Test another transition if available
    Write-Host "`n[STEP 9] Testing second transition (if available)..." -ForegroundColor Yellow

    try {
        $transitionsUrl2 = ($baseUrl + "/workflows/allowed-transitions?categoryId=" + $categoryId + "&currentStatusId=" + $nextStatusId)
        $transitionsResponse2 = Invoke-RestMethod -Uri $transitionsUrl2 -Method Get -Headers $headers

        if ($transitionsResponse2.data -and $transitionsResponse2.data.Count -gt 0) {
            $secondTransition = $transitionsResponse2.data[0]
            $secondTargetStatusId = $secondTransition.toStatusId
            $secondTargetStatusName = $secondTransition.toStatusName

            Write-Host "  Available next transition: $secondTargetStatusName" -ForegroundColor Gray

            $transitionBody2 = @{
                newStatusId = $secondTargetStatusId
                comment = "Second transition test"
            } | ConvertTo-Json

            $transitionResponse2 = Invoke-RestMethod -Uri "$baseUrl/workflows/complaints/$complaintId/transition" -Method Post -Body $transitionBody2 -Headers $headers

            $hasData2 = $null -ne $transitionResponse2.data
            $statusCorrect2 = $transitionResponse2.data.statusId -eq $secondTargetStatusId

            if ($hasData2 -and $statusCorrect2) {
                Write-Host "  SUCCESS: Second transition also returns updated complaint correctly" -ForegroundColor Green
                Add-TestResult "Second Transition" "PASS" "Transitioned to: $secondTargetStatusName"
            } else {
                Write-Host "  FAIL: Second transition response incorrect" -ForegroundColor Red
                Add-TestResult "Second Transition" "FAIL" "Response incorrect"
            }
        } else {
            Write-Host "  INFO: No additional transitions available" -ForegroundColor Gray
            Add-TestResult "Second Transition" "SKIP" "No additional transitions available"
        }
    } catch {
        Write-Host "  INFO: Could not test second transition: $($_.Exception.Message)" -ForegroundColor Gray
        Add-TestResult "Second Transition" "SKIP" "Not tested"
    }

} catch {
    Write-Host "`nFAIL: Test execution failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    Add-TestResult "Test Execution" "FAIL" $_.Exception.Message
}

# Final Summary
$endTime = Get-Date
$totalDuration = $endTime - $startTime

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$passCount = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$skipCount = ($testResults | Where-Object { $_.Status -eq "SKIP" }).Count

Write-Host "`nResults:" -ForegroundColor Yellow
$testResults | ForEach-Object {
    $color = switch ($_.Status) {
        "PASS" { "Green" }
        "FAIL" { "Red" }
        "SKIP" { "Gray" }
        "WARN" { "Yellow" }
        default { "White" }
    }
    Write-Host "  [$($_.Status)] $($_.Test)" -ForegroundColor $color
    Write-Host "     $($_.Details)" -ForegroundColor Gray
}

Write-Host "`nStatistics:" -ForegroundColor Yellow
Write-Host "  Passed: $passCount" -ForegroundColor Green
Write-Host "  Failed: $failCount" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })
Write-Host "  Skipped: $skipCount" -ForegroundColor Gray
Write-Host "  Total Duration: $($totalDuration.TotalSeconds) seconds" -ForegroundColor Gray

Write-Host "`n========================================" -ForegroundColor Cyan
if ($failCount -eq 0 -and $passCount -gt 0) {
    Write-Host "WORKFLOW TRANSITION FIX: VERIFIED" -ForegroundColor Green
} else {
    Write-Host "WORKFLOW TRANSITION FIX: ISSUES FOUND" -ForegroundColor Red
}
Write-Host "========================================" -ForegroundColor Cyan

# Export results
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$reportPath = "C:\Users\Navin Chandra\Pictures\Complaint management system\WORKFLOW_TRANSITION_FIX_TEST_RESULTS_$timestamp.txt"
$testResults | Format-Table -AutoSize | Out-File -FilePath $reportPath
Write-Host "`nDetailed results saved to: $reportPath" -ForegroundColor Gray
