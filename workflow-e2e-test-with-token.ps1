# Comprehensive Workflow E2E Test with Existing Token
$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5058/api"

# Load existing token
$token = Get-Content ".fresh-token" -ErrorAction Stop

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Test results tracking
$script:testResults = @()
$script:testsPassed = 0
$script:testsFailed = 0
$script:bugsFound = @()

function Write-TestHeader {
    param($message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Write-TestResult {
    param($testName, $passed, $details = "")

    $result = @{
        TestName = $testName
        Passed = $passed
        Details = $details
        Timestamp = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    }

    $script:testResults += $result

    if ($passed) {
        $script:testsPassed++
        Write-Host "[PASS] $testName" -ForegroundColor Green
        if ($details) { Write-Host "       $details" -ForegroundColor Gray }
    } else {
        $script:testsFailed++
        Write-Host "[FAIL] $testName" -ForegroundColor Red
        if ($details) { Write-Host "       $details" -ForegroundColor Yellow }
    }
}

function Add-Bug {
    param($bugId, $title, $description, $severity)

    $bug = @{
        BugId = $bugId
        Title = $title
        Description = $description
        Severity = $severity
        FoundAt = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    }

    $script:bugsFound += $bug
}

Write-Host @"
╔══════════════════════════════════════════════════════════════════════╗
║                                                                      ║
║     COMPREHENSIVE WORKFLOW MANAGEMENT E2E TEST SUITE                 ║
║     Testing BUG #001 and BUG #002 Fixes                              ║
║                                                                      ║
╚══════════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

Write-Host "`nStart Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# ============================================
# PHASE 1: CREATE OPERATIONS
# ============================================
Write-TestHeader "PHASE 1: CREATE OPERATIONS"

# Step 1: Verify Category Dropdown (BUG #001)
Write-Host "--- Verifying BUG #001 Fix: Category Dropdown Population ---" -ForegroundColor Yellow

try {
    $categories = Invoke-RestMethod -Uri "$baseUrl/categories" -Method Get -Headers $headers

    if ($categories -and $categories.Count -gt 0) {
        Write-TestResult "BUG #001 FIX VERIFIED - Category dropdown populated" $true "Found $($categories.Count) categories"
        Write-Host "Available Categories:" -ForegroundColor Cyan
        $categories | Select-Object -First 5 | ForEach-Object {
            Write-Host "  - [$($_.categoryId)] $($_.categoryName)" -ForegroundColor Gray
        }
    } else {
        Write-TestResult "BUG #001 NOT FIXED - Category dropdown empty" $false "No categories returned"
        Add-Bug "BUG#001-REGRESSION" "Category dropdown still empty" "Categories API returns empty" "CRITICAL"
    }
} catch {
    Write-TestResult "Failed to fetch categories" $false $_.Exception.Message
    Add-Bug "BUG#001-ERROR" "Cannot fetch categories" $_.Exception.Message "CRITICAL"
}

if (-not $categories -or $categories.Count -eq 0) {
    Write-Host "`nCRITICAL: Cannot proceed without categories!" -ForegroundColor Red
    exit 1
}

$testCategory = $categories[0]

# Step 2: Verify Status Master Dropdown (BUG #002)
Write-Host "`n--- Verifying BUG #002 Fix: Status Master Dropdown Population ---" -ForegroundColor Yellow

try {
    $statuses = Invoke-RestMethod -Uri "$baseUrl/complaint-status-master" -Method Get -Headers $headers

    if ($statuses -and $statuses.Count -gt 0) {
        Write-TestResult "BUG #002 FIX VERIFIED - Status Master dropdown populated" $true "Found $($statuses.Count) statuses"
        Write-Host "Available Statuses:" -ForegroundColor Cyan
        $statuses | Select-Object -First 5 | ForEach-Object {
            Write-Host "  - [$($_.statusMasterId)] $($_.statusName) (Order: $($_.displayOrder))" -ForegroundColor Gray
        }
    } else {
        Write-TestResult "BUG #002 NOT FIXED - Status dropdown empty" $false "No statuses returned"
        Add-Bug "BUG#002-REGRESSION" "Status dropdown still empty" "Status Master API returns empty" "CRITICAL"
    }
} catch {
    Write-TestResult "Failed to fetch statuses" $false $_.Exception.Message
    Add-Bug "BUG#002-ERROR" "Cannot fetch statuses" $_.Exception.Message "CRITICAL"
}

if (-not $statuses -or $statuses.Count -eq 0) {
    Write-Host "`nCRITICAL: Cannot proceed without statuses!" -ForegroundColor Red
    exit 1
}

$testStatuses = $statuses | Select-Object -First 3

# Step 3: Create New Workflow
Write-Host "`n--- Creating New Workflow ---" -ForegroundColor Yellow

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$workflowData = @{
    categoryId = $testCategory.categoryId
    workflowName = "E2E Test Workflow $timestamp"
    description = "Comprehensive test workflow after bug fixes"
    isActive = $true
    isDefault = $true
} | ConvertTo-Json

try {
    $newWorkflow = Invoke-RestMethod -Uri "$baseUrl/workflow" -Method Post -Headers $headers -Body $workflowData

    if ($newWorkflow.workflowId) {
        Write-TestResult "Created new workflow" $true "Workflow ID: $($newWorkflow.workflowId), Name: $($newWorkflow.workflowName)"
        Write-Host "`nWorkflow Details:" -ForegroundColor Cyan
        Write-Host "  ID: $($newWorkflow.workflowId)" -ForegroundColor Gray
        Write-Host "  Name: $($newWorkflow.workflowName)" -ForegroundColor Gray
        Write-Host "  Category: [$($testCategory.categoryId)] $($testCategory.categoryName)" -ForegroundColor Gray
    } else {
        Write-TestResult "Failed to create workflow" $false "No workflow ID in response"
        exit 1
    }
} catch {
    Write-TestResult "Failed to create workflow" $false $_.Exception.Message
    Add-Bug "BUG#003" "Cannot create workflow" $_.Exception.Message "CRITICAL"
    exit 1
}

# Step 4: Add Statuses to Workflow
Write-Host "`n--- Adding Statuses to Workflow ---" -ForegroundColor Yellow

$addedStatuses = @()
$statusOrder = 1

foreach ($status in $testStatuses) {
    $workflowStatusData = @{
        workflowId = $newWorkflow.workflowId
        statusMasterId = $status.statusMasterId
        slaHours = (24 * $statusOrder)
        isInitialStatus = ($statusOrder -eq 1)
        statusOrder = $statusOrder
    } | ConvertTo-Json

    try {
        $addedStatus = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/statuses" -Method Post -Headers $headers -Body $workflowStatusData

        if ($addedStatus) {
            Write-TestResult "Added status: $($status.statusName)" $true "SLA: $($statusOrder * 24) hours, Order: $statusOrder, Initial: $($statusOrder -eq 1)"
            $addedStatuses += $addedStatus
        }
    } catch {
        Write-TestResult "Failed to add status: $($status.statusName)" $false $_.Exception.Message
        Add-Bug "BUG#004" "Cannot add status to workflow" $_.Exception.Message "HIGH"
    }

    $statusOrder++
}

Write-Host "`nTotal Statuses Added: $($addedStatuses.Count)" -ForegroundColor Green

# Step 5: Add Transitions
Write-Host "`n--- Adding Workflow Transitions ---" -ForegroundColor Yellow

$addedTransitions = @()

if ($addedStatuses.Count -ge 2) {
    # Transition 1: From first to second status
    $transitionData = @{
        workflowId = $newWorkflow.workflowId
        fromStatusId = $addedStatuses[0].workflowStatusId
        toStatusId = $addedStatuses[1].workflowStatusId
        transitionName = "Start Work"
        buttonLabel = "Start Work"
        buttonColor = "#4CAF50"
        buttonIcon = "play_arrow"
        requiresComment = $true
        requiresAttachment = $false
        isSystemTransition = $false
    } | ConvertTo-Json

    try {
        $transition1 = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/transitions" -Method Post -Headers $headers -Body $transitionData

        if ($transition1) {
            Write-TestResult "Added transition: Start Work" $true "From status 1 to 2, requires comment"
            $addedTransitions += $transition1
        }
    } catch {
        Write-TestResult "Failed to add transition: Start Work" $false $_.Exception.Message
        Add-Bug "BUG#005" "Cannot add workflow transition" $_.Exception.Message "HIGH"
    }
}

if ($addedStatuses.Count -ge 3) {
    # Transition 2: From second to third status
    $transitionData = @{
        workflowId = $newWorkflow.workflowId
        fromStatusId = $addedStatuses[1].workflowStatusId
        toStatusId = $addedStatuses[2].workflowStatusId
        transitionName = "Complete"
        buttonLabel = "Complete"
        buttonColor = "#2196F3"
        buttonIcon = "check_circle"
        requiresComment = $false
        requiresAttachment = $false
        isSystemTransition = $false
    } | ConvertTo-Json

    try {
        $transition2 = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/transitions" -Method Post -Headers $headers -Body $transitionData

        if ($transition2) {
            Write-TestResult "Added transition: Complete" $true "From status 2 to 3"
            $addedTransitions += $transition2
        }
    } catch {
        Write-TestResult "Failed to add transition: Complete" $false $_.Exception.Message
    }
}

Write-Host "`nTotal Transitions Added: $($addedTransitions.Count)" -ForegroundColor Green

# ============================================
# PHASE 2: READ OPERATIONS
# ============================================
Write-TestHeader "PHASE 2: READ OPERATIONS"

# Test 1: Get All Workflows
Write-Host "--- Getting All Workflows ---" -ForegroundColor Yellow

try {
    $allWorkflows = Invoke-RestMethod -Uri "$baseUrl/workflow" -Method Get -Headers $headers

    if ($allWorkflows -and $allWorkflows.Count -gt 0) {
        Write-TestResult "Retrieved all workflows" $true "Found $($allWorkflows.Count) workflows"

        $foundWorkflow = $allWorkflows | Where-Object { $_.workflowId -eq $newWorkflow.workflowId }
        if ($foundWorkflow) {
            Write-TestResult "New workflow appears in list" $true "Workflow is visible in GET all"
        } else {
            Write-TestResult "New workflow NOT in list" $false "Created workflow missing from list"
            Add-Bug "BUG#006" "Workflow not in list" "Created workflow doesn't appear in GET all" "HIGH"
        }
    } else {
        Write-TestResult "Failed to retrieve workflows" $false "Empty response"
    }
} catch {
    Write-TestResult "Failed to retrieve workflows" $false $_.Exception.Message
}

# Test 2: Get Workflow by ID
Write-Host "`n--- Getting Workflow by ID ---" -ForegroundColor Yellow

try {
    $workflow = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)" -Method Get -Headers $headers

    if ($workflow.workflowId -eq $newWorkflow.workflowId) {
        Write-TestResult "Retrieved workflow by ID" $true "Workflow: $($workflow.workflowName)"
    } else {
        Write-TestResult "Failed to retrieve workflow by ID" $false "ID mismatch"
    }
} catch {
    Write-TestResult "Failed to retrieve workflow by ID" $false $_.Exception.Message
}

# Test 3: Get Workflow Statuses
Write-Host "`n--- Getting Workflow Statuses ---" -ForegroundColor Yellow

try {
    $workflowStatuses = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/statuses" -Method Get -Headers $headers

    if ($workflowStatuses -and $workflowStatuses.Count -gt 0) {
        Write-TestResult "Retrieved workflow statuses" $true "Found $($workflowStatuses.Count) statuses"
        Write-Host "Statuses:" -ForegroundColor Cyan
        $workflowStatuses | ForEach-Object {
            Write-Host "  - Status Master ID: $($_.statusMasterId), SLA: $($_.slaHours)h, Order: $($_.statusOrder)" -ForegroundColor Gray
        }
    } else {
        Write-TestResult "Failed to retrieve workflow statuses" $false "Empty response"
        Add-Bug "BUG#007" "Cannot retrieve statuses" "GET workflow statuses returns empty" "HIGH"
    }
} catch {
    Write-TestResult "Failed to retrieve workflow statuses" $false $_.Exception.Message
}

# Test 4: Get Workflow Transitions
Write-Host "`n--- Getting Workflow Transitions ---" -ForegroundColor Yellow

try {
    $workflowTransitions = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/transitions" -Method Get -Headers $headers

    if ($workflowTransitions -and $workflowTransitions.Count -gt 0) {
        Write-TestResult "Retrieved workflow transitions" $true "Found $($workflowTransitions.Count) transitions"
        Write-Host "Transitions:" -ForegroundColor Cyan
        $workflowTransitions | ForEach-Object {
            Write-Host "  - $($_.transitionName): From $($_.fromStatusId) to $($_.toStatusId)" -ForegroundColor Gray
        }
    } else {
        Write-Host "No transitions found (may be expected)" -ForegroundColor Yellow
    }
} catch {
    Write-TestResult "Failed to retrieve workflow transitions" $false $_.Exception.Message
}

# Test 5: Get Workflows by Category
Write-Host "`n--- Getting Workflows by Category ---" -ForegroundColor Yellow

try {
    $categoryWorkflows = Invoke-RestMethod -Uri "$baseUrl/workflow/category/$($testCategory.categoryId)" -Method Get -Headers $headers

    if ($categoryWorkflows -and $categoryWorkflows.Count -gt 0) {
        Write-TestResult "Retrieved workflows by category" $true "Found $($categoryWorkflows.Count) workflows"

        $foundWorkflow = $categoryWorkflows | Where-Object { $_.workflowId -eq $newWorkflow.workflowId }
        if ($foundWorkflow) {
            Write-TestResult "New workflow appears in category list" $true "Category filter working"
        } else {
            Write-TestResult "New workflow NOT in category list" $false "Category filter issue"
            Add-Bug "BUG#008" "Workflow not in category filter" "Workflow missing from category results" "MEDIUM"
        }
    } else {
        Write-TestResult "Failed to retrieve workflows by category" $false "Empty response"
    }
} catch {
    Write-TestResult "Failed to retrieve workflows by category" $false $_.Exception.Message
}

# ============================================
# PHASE 3: UPDATE OPERATIONS
# ============================================
Write-TestHeader "PHASE 3: UPDATE OPERATIONS"

# Test 1: Update Workflow Details
Write-Host "--- Updating Workflow Details ---" -ForegroundColor Yellow

$updateData = @{
    workflowId = $newWorkflow.workflowId
    categoryId = $testCategory.categoryId
    workflowName = "$($newWorkflow.workflowName) - UPDATED"
    description = "Updated description after E2E testing"
    isActive = $true
    isDefault = $false
} | ConvertTo-Json

try {
    $updatedWorkflow = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)" -Method Put -Headers $headers -Body $updateData

    if ($updatedWorkflow) {
        Write-TestResult "Updated workflow details" $true "Name and description updated"
        Write-Host "  New Name: $($updatedWorkflow.workflowName)" -ForegroundColor Gray
    } else {
        Write-TestResult "Failed to update workflow" $false "No response"
    }
} catch {
    Write-TestResult "Failed to update workflow" $false $_.Exception.Message
    Add-Bug "BUG#009" "Cannot update workflow" $_.Exception.Message "MEDIUM"
}

# Test 2: Update Workflow Status SLA
Write-Host "`n--- Updating Workflow Status SLA Hours ---" -ForegroundColor Yellow

if ($addedStatuses -and $addedStatuses.Count -gt 0) {
    $statusToUpdate = $addedStatuses[0]

    $statusUpdateData = @{
        workflowStatusId = $statusToUpdate.workflowStatusId
        workflowId = $newWorkflow.workflowId
        statusMasterId = $statusToUpdate.statusMasterId
        slaHours = 48
        isInitialStatus = $statusToUpdate.isInitialStatus
        statusOrder = $statusToUpdate.statusOrder
    } | ConvertTo-Json

    try {
        $updatedStatus = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/statuses/$($statusToUpdate.workflowStatusId)" -Method Put -Headers $headers -Body $statusUpdateData

        if ($updatedStatus.slaHours -eq 48) {
            Write-TestResult "Updated status SLA hours" $true "SLA changed to 48 hours"
        } else {
            Write-TestResult "Failed to update status SLA" $false "SLA not updated"
        }
    } catch {
        Write-TestResult "Failed to update status SLA" $false $_.Exception.Message
        Add-Bug "BUG#010" "Cannot update status SLA" $_.Exception.Message "MEDIUM"
    }
}

# ============================================
# PHASE 4: DELETE OPERATIONS
# ============================================
Write-TestHeader "PHASE 4: DELETE OPERATIONS"

# Test 1: Delete Transition
Write-Host "--- Deleting Workflow Transition ---" -ForegroundColor Yellow

if ($addedTransitions -and $addedTransitions.Count -gt 0) {
    $transitionToDelete = $addedTransitions[0]

    try {
        Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/transitions/$($transitionToDelete.transitionId)" -Method Delete -Headers $headers
        Write-TestResult "Deleted transition" $true "Transition ID: $($transitionToDelete.transitionId)"

        Start-Sleep -Milliseconds 500
        $remainingTransitions = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/transitions" -Method Get -Headers $headers
        $stillExists = $remainingTransitions | Where-Object { $_.transitionId -eq $transitionToDelete.transitionId }

        if (-not $stillExists) {
            Write-TestResult "Verified transition deletion" $true "Transition no longer exists"
        } else {
            Write-TestResult "Transition not deleted properly" $false "Still exists after delete"
            Add-Bug "BUG#011" "Transition not fully deleted" "DELETE succeeds but transition remains" "HIGH"
        }
    } catch {
        Write-TestResult "Failed to delete transition" $false $_.Exception.Message
        Add-Bug "BUG#012" "Cannot delete transition" $_.Exception.Message "MEDIUM"
    }
}

# Test 2: Delete Status
Write-Host "`n--- Deleting Workflow Status ---" -ForegroundColor Yellow

if ($addedStatuses -and $addedStatuses.Count -gt 1) {
    $statusToDelete = $addedStatuses[-1]

    try {
        Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/statuses/$($statusToDelete.workflowStatusId)" -Method Delete -Headers $headers
        Write-TestResult "Deleted status" $true "Status ID: $($statusToDelete.workflowStatusId)"

        Start-Sleep -Milliseconds 500
        $remainingStatuses = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/statuses" -Method Get -Headers $headers
        $stillExists = $remainingStatuses | Where-Object { $_.workflowStatusId -eq $statusToDelete.workflowStatusId }

        if (-not $stillExists) {
            Write-TestResult "Verified status deletion" $true "Status no longer exists"
        } else {
            Write-TestResult "Status not deleted properly" $false "Still exists after delete"
            Add-Bug "BUG#013" "Status not fully deleted" "DELETE succeeds but status remains" "HIGH"
        }
    } catch {
        Write-TestResult "Failed to delete status" $false $_.Exception.Message
        Add-Bug "BUG#014" "Cannot delete status" $_.Exception.Message "MEDIUM"
    }
}

# ============================================
# PHASE 5: INTEGRATION TESTING
# ============================================
Write-TestHeader "PHASE 5: INTEGRATION TESTING"

# Test: Get complaints and check workflow integration
Write-Host "--- Testing Workflow Integration with Complaints ---" -ForegroundColor Yellow

try {
    $complaints = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method Get -Headers $headers

    if ($complaints -and $complaints.Count -gt 0) {
        Write-TestResult "Retrieved complaints" $true "Found $($complaints.Count) complaints"

        $categoryComplaints = $complaints | Where-Object { $_.categoryId -eq $testCategory.categoryId }

        if ($categoryComplaints -and $categoryComplaints.Count -gt 0) {
            Write-TestResult "Found complaints in test category" $true "Count: $($categoryComplaints.Count)"

            $testComplaint = $categoryComplaints[0]

            # Try to get available transitions
            try {
                $availableTransitions = Invoke-RestMethod -Uri "$baseUrl/workflow/complaint/$($testComplaint.complaintId)/transitions" -Method Get -Headers $headers

                if ($availableTransitions -and $availableTransitions.Count -gt 0) {
                    Write-TestResult "Retrieved available transitions for complaint" $true "Found $($availableTransitions.Count) transitions"
                } else {
                    Write-Host "No transitions available (complaint may be in final status)" -ForegroundColor Yellow
                }
            } catch {
                Write-TestResult "Failed to get available transitions" $false $_.Exception.Message
                Add-Bug "BUG#015" "Cannot get complaint transitions" $_.Exception.Message "HIGH"
            }
        } else {
            Write-Host "No complaints in test category" -ForegroundColor Yellow
        }
    } else {
        Write-Host "No complaints found for integration test" -ForegroundColor Yellow
    }
} catch {
    Write-TestResult "Failed to retrieve complaints" $false $_.Exception.Message
}

# ============================================
# PHASE 6: VALIDATION TESTING
# ============================================
Write-TestHeader "PHASE 6: VALIDATION & ERROR TESTING"

# Test 1: Try to create workflow without required fields
Write-Host "--- Testing Validation: Empty Workflow Name ---" -ForegroundColor Yellow

$invalidData = @{
    categoryId = $testCategory.categoryId
    workflowName = ""
    description = "Test"
    isActive = $true
    isDefault = $false
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$baseUrl/workflow" -Method Post -Headers $headers -Body $invalidData
    Write-TestResult "Validation: Empty workflow name" $false "System accepted invalid data"
    Add-Bug "BUG#016" "Missing validation for empty name" "System accepts empty workflow name" "MEDIUM"
} catch {
    if ($_.Exception.Message -match "validation|required|bad request") {
        Write-TestResult "Validation: Empty workflow name" $true "System correctly rejected empty name"
    } else {
        Write-TestResult "Validation: Empty workflow name" $false "Unexpected error: $($_.Exception.Message)"
    }
}

# Test 2: Try to create workflow with invalid category
Write-Host "`n--- Testing Validation: Invalid Category ID ---" -ForegroundColor Yellow

$invalidData = @{
    categoryId = 999999
    workflowName = "Test"
    description = "Test"
    isActive = $true
    isDefault = $false
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$baseUrl/workflow" -Method Post -Headers $headers -Body $invalidData
    Write-TestResult "Validation: Invalid category ID" $false "System accepted invalid category"
    Add-Bug "BUG#017" "Missing validation for category" "System accepts invalid category ID" "MEDIUM"
} catch {
    if ($_.Exception.Message -match "validation|invalid|not found|constraint") {
        Write-TestResult "Validation: Invalid category ID" $true "System correctly rejected invalid category"
    } else {
        Write-TestResult "Validation: Invalid category ID" $false "Unexpected error: $($_.Exception.Message)"
    }
}

# ============================================
# FINAL SUMMARY
# ============================================
Write-TestHeader "TEST EXECUTION COMPLETE"

$totalTests = $script:testsPassed + $script:testsFailed
$successRate = if ($totalTests -gt 0) { [math]::Round(($script:testsPassed / $totalTests) * 100, 2) } else { 0 }

Write-Host "`nTest Summary:" -ForegroundColor White
Write-Host "  Total Tests: $totalTests" -ForegroundColor Gray
Write-Host "  Passed: $script:testsPassed" -ForegroundColor Green
Write-Host "  Failed: $script:testsFailed" -ForegroundColor $(if ($script:testsFailed -eq 0) { "Green" } else { "Red" })
Write-Host "  Success Rate: $successRate%" -ForegroundColor $(if ($successRate -ge 90) { "Green" } elseif ($successRate -ge 70) { "Yellow" } else { "Red" })
Write-Host "  Bugs Found: $($script:bugsFound.Count)" -ForegroundColor $(if ($script:bugsFound.Count -eq 0) { "Green" } else { "Yellow" })
Write-Host "  End Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# Generate report
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportFile = "WORKFLOW_E2E_TEST_RESULTS_$timestamp.txt"

$reportContent = @"
╔══════════════════════════════════════════════════════════════════════╗
║                                                                      ║
║     COMPREHENSIVE WORKFLOW MANAGEMENT E2E TEST RESULTS               ║
║                                                                      ║
╚══════════════════════════════════════════════════════════════════════╝

Test Execution Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Backend URL: $baseUrl

═══════════════════════════════════════════════════════════════════════

EXECUTIVE SUMMARY

Total Tests: $totalTests
Tests Passed: $script:testsPassed
Tests Failed: $script:testsFailed
Success Rate: $successRate%
Bugs Found: $($script:bugsFound.Count)

═══════════════════════════════════════════════════════════════════════

BUG FIXES VERIFICATION

BUG #001: Category Dropdown Population
Status: $(if (($script:testResults | Where-Object { $_.TestName -match "BUG #001.*VERIFIED" -and $_.Passed }).Count -gt 0) { "✅ VERIFIED FIXED" } else { "❌ NOT FIXED" })

BUG #002: Status Master Dropdown Population
Status: $(if (($script:testResults | Where-Object { $_.TestName -match "BUG #002.*VERIFIED" -and $_.Passed }).Count -gt 0) { "✅ VERIFIED FIXED" } else { "❌ NOT FIXED" })

═══════════════════════════════════════════════════════════════════════

DETAILED TEST RESULTS

$(($script:testResults | ForEach-Object {
    $icon = if ($_.Passed) { "[PASS]" } else { "[FAIL]" }
    "$icon $($_.TestName)`n      $($_.Details)`n      Time: $($_.Timestamp)`n"
}) -join "`n")

═══════════════════════════════════════════════════════════════════════

BUGS FOUND

$(if ($script:bugsFound.Count -eq 0) {
    "✅ NO BUGS FOUND - All tests passed successfully!"
} else {
    ($script:bugsFound | ForEach-Object {
        @"
$($_.BugId): $($_.Title)
Severity: $($_.Severity)
Description: $($_.Description)
Found At: $($_.FoundAt)

"@
    }) -join "`n"
})

═══════════════════════════════════════════════════════════════════════

CONCLUSION

$(if ($successRate -eq 100 -and $script:bugsFound.Count -eq 0) {
    "🎯 SYSTEM READY FOR PRODUCTION

All tests passed successfully. Both BUG #001 (Category dropdown) and BUG #002
(Status dropdown) have been verified as fixed. The Workflow Management system
is functioning correctly and ready for production use."
} elseif ($successRate -ge 90) {
    "✅ SYSTEM MOSTLY FUNCTIONAL

Most tests passed. Minor issues found that should be addressed. Both critical
bug fixes (BUG #001 and BUG #002) have been verified as working correctly."
} else {
    "⚠️ SYSTEM REQUIRES ATTENTION

Significant issues found. System requires fixes before production deployment.
Please review and address all critical and high-priority bugs."
})

═══════════════════════════════════════════════════════════════════════

Report Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Report File: $reportFile
"@

$reportContent | Out-File -FilePath $reportFile -Encoding UTF8

Write-Host "Detailed test report saved to: $reportFile" -ForegroundColor Green
Write-Host ""

if ($script:bugsFound.Count -gt 0) {
    Write-Host "⚠️ BUGS FOUND:" -ForegroundColor Yellow
    $script:bugsFound | ForEach-Object {
        Write-Host "  $($_.BugId): $($_.Title) [$($_.Severity)]" -ForegroundColor Yellow
    }
    Write-Host ""
}

if ($successRate -eq 100) {
    Write-Host "✅ ALL TESTS PASSED! System is ready." -ForegroundColor Green
} elseif ($successRate -ge 90) {
    Write-Host "✅ Most tests passed. Review failed tests." -ForegroundColor Yellow
} else {
    Write-Host "❌ Multiple tests failed. System needs attention." -ForegroundColor Red
}

Write-Host ""
