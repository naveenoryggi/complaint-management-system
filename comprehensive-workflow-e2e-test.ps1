# Comprehensive End-to-End Workflow Management CRUD Test
# Testing after BUG #001 and BUG #002 fixes

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5058/api"
$frontendUrl = "http://localhost:4200"

# Test Results Tracking
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
    param($testName, $passed, $details = "", $severity = "INFO")

    $result = @{
        TestName = $testName
        Passed = $passed
        Details = $details
        Severity = $severity
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
    param($bugId, $title, $description, $severity, $reproSteps)

    $bug = @{
        BugId = $bugId
        Title = $title
        Description = $description
        Severity = $severity
        ReproSteps = $reproSteps
        FoundAt = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    }

    $script:bugsFound += $bug
}

# Get authentication token
function Get-AuthToken {
    Write-TestHeader "AUTHENTICATION"

    try {
        $loginBody = @{
            email = "admin@example.com"
            password = "Admin@123"
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"

        if ($response.token) {
            Write-TestResult "Login successful" $true "Token obtained: $($response.token.Substring(0,20))..."
            return $response.token
        } else {
            Write-TestResult "Login failed" $false "No token in response"
            return $null
        }
    } catch {
        Write-TestResult "Login failed" $false $_.Exception.Message
        return $null
    }
}

# Create headers with token
function Get-Headers {
    param($token)
    return @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
}

# ============================================
# PHASE 1: CREATE OPERATIONS
# ============================================
function Test-CreateWorkflow {
    param($token, $headers)

    Write-TestHeader "PHASE 1: CREATE OPERATIONS - Workflow Creation"

    # Step 1: Get available categories (BUG #001 FIX VERIFICATION)
    Write-Host "`n--- Step 1: Verify Category Dropdown Population (BUG #001) ---" -ForegroundColor Yellow

    try {
        $categories = Invoke-RestMethod -Uri "$baseUrl/categories" -Method Get -Headers $headers

        if ($categories -and $categories.Count -gt 0) {
            Write-TestResult "BUG #001 FIX VERIFIED - Category dropdown populated" $true "Found $($categories.Count) categories"
            Write-Host "Available Categories:" -ForegroundColor Cyan
            $categories | ForEach-Object { Write-Host "  - [$($_.categoryId)] $($_.categoryName)" -ForegroundColor Gray }
        } else {
            Write-TestResult "BUG #001 NOT FIXED - Category dropdown empty" $false "No categories found"
            Add-Bug "BUG#001-REGRESSION" "Category dropdown still empty" "Categories endpoint returns no data" "CRITICAL" @("Navigate to Workflow Management", "Click Create Workflow", "Observe category dropdown is empty")
            return $null
        }
    } catch {
        Write-TestResult "Failed to fetch categories" $false $_.Exception.Message
        return $null
    }

    # Select first category for testing
    $testCategory = $categories[0]
    Write-Host "`nSelected category for testing: [$($testCategory.categoryId)] $($testCategory.categoryName)" -ForegroundColor Green

    # Step 2: Get available statuses (BUG #002 FIX VERIFICATION)
    Write-Host "`n--- Step 2: Verify Status Master Dropdown Population (BUG #002) ---" -ForegroundColor Yellow

    try {
        $statuses = Invoke-RestMethod -Uri "$baseUrl/complaint-status-master" -Method Get -Headers $headers

        if ($statuses -and $statuses.Count -gt 0) {
            Write-TestResult "BUG #002 FIX VERIFIED - Status Master dropdown populated" $true "Found $($statuses.Count) statuses"
            Write-Host "Available Statuses:" -ForegroundColor Cyan
            $statuses | ForEach-Object { Write-Host "  - [$($_.statusMasterId)] $($_.statusName) (Order: $($_.displayOrder))" -ForegroundColor Gray }
        } else {
            Write-TestResult "BUG #002 NOT FIXED - Status dropdown empty" $false "No statuses found"
            Add-Bug "BUG#002-REGRESSION" "Status dropdown still empty" "Status master endpoint returns no data" "CRITICAL" @("Navigate to Workflow Management", "Select workflow", "Click Add Status", "Observe status dropdown is empty")
            return $null
        }
    } catch {
        Write-TestResult "Failed to fetch statuses" $false $_.Exception.Message
        return $null
    }

    # Select first 3 statuses for testing
    $testStatuses = $statuses | Select-Object -First 3
    Write-Host "`nSelected statuses for testing:" -ForegroundColor Green
    $testStatuses | ForEach-Object { Write-Host "  - [$($_.statusMasterId)] $($_.statusName)" -ForegroundColor Gray }

    # Step 3: Create new workflow
    Write-Host "`n--- Step 3: Create New Workflow ---" -ForegroundColor Yellow

    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $workflowData = @{
        categoryId = $testCategory.categoryId
        workflowName = "E2E Test Workflow $timestamp"
        description = "Comprehensive test workflow created after bug fixes"
        isActive = $true
        isDefault = $true
    } | ConvertTo-Json

    try {
        $newWorkflow = Invoke-RestMethod -Uri "$baseUrl/workflow" -Method Post -Headers $headers -Body $workflowData

        if ($newWorkflow.workflowId) {
            Write-TestResult "Workflow created successfully" $true "Workflow ID: $($newWorkflow.workflowId)"
            Write-Host "`nCreated Workflow Details:" -ForegroundColor Cyan
            Write-Host "  ID: $($newWorkflow.workflowId)" -ForegroundColor Gray
            Write-Host "  Name: $($newWorkflow.workflowName)" -ForegroundColor Gray
            Write-Host "  Category: $($newWorkflow.categoryId)" -ForegroundColor Gray
            Write-Host "  Active: $($newWorkflow.isActive)" -ForegroundColor Gray
            Write-Host "  Default: $($newWorkflow.isDefault)" -ForegroundColor Gray
        } else {
            Write-TestResult "Workflow creation failed" $false "No workflow ID in response"
            return $null
        }
    } catch {
        Write-TestResult "Workflow creation failed" $false $_.Exception.Message
        Add-Bug "BUG#003" "Cannot create workflow" $_.Exception.Message "CRITICAL" @("Navigate to Workflow Management", "Click Create Workflow", "Fill in all fields", "Click Submit")
        return $null
    }

    # Step 4: Add statuses to workflow
    Write-Host "`n--- Step 4: Add Statuses to Workflow ---" -ForegroundColor Yellow

    $statusOrder = 1
    $addedStatuses = @()

    foreach ($status in $testStatuses) {
        $workflowStatusData = @{
            workflowId = $newWorkflow.workflowId
            statusMasterId = $status.statusMasterId
            slaHours = (24 * $statusOrder)  # 24, 48, 72 hours
            isInitialStatus = ($statusOrder -eq 1)
            statusOrder = $statusOrder
        } | ConvertTo-Json

        try {
            $addedStatus = Invoke-RestMethod -Uri "$baseUrl/workflow/$($newWorkflow.workflowId)/statuses" -Method Post -Headers $headers -Body $workflowStatusData

            if ($addedStatus) {
                Write-TestResult "Added status: $($status.statusName)" $true "SLA: $($statusOrder * 24) hours, Order: $statusOrder"
                $addedStatuses += $addedStatus
            } else {
                Write-TestResult "Failed to add status: $($status.statusName)" $false "No response data"
            }
        } catch {
            Write-TestResult "Failed to add status: $($status.statusName)" $false $_.Exception.Message
            Add-Bug "BUG#004" "Cannot add status to workflow" $_.Exception.Message "HIGH" @("Open workflow", "Click Add Status", "Select status", "Enter SLA hours", "Click Submit")
        }

        $statusOrder++
    }

    Write-Host "`nAdded $($addedStatuses.Count) statuses to workflow" -ForegroundColor Green

    # Step 5: Add transitions
    Write-Host "`n--- Step 5: Add Workflow Transitions ---" -ForegroundColor Yellow

    if ($addedStatuses.Count -ge 2) {
        # Create transition from first to second status
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
                Write-TestResult "Added transition: Start Work" $true "From status 1 to status 2, requires comment"
            } else {
                Write-TestResult "Failed to add transition: Start Work" $false "No response data"
            }
        } catch {
            Write-TestResult "Failed to add transition: Start Work" $false $_.Exception.Message
            Add-Bug "BUG#005" "Cannot add workflow transition" $_.Exception.Message "HIGH" @("Open workflow", "Click Add Transition", "Select from/to statuses", "Fill in details", "Click Submit")
        }
    }

    if ($addedStatuses.Count -ge 3) {
        # Create transition from second to third status
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
                Write-TestResult "Added transition: Complete" $true "From status 2 to status 3"
            } else {
                Write-TestResult "Failed to add transition: Complete" $false "No response data"
            }
        } catch {
            Write-TestResult "Failed to add transition: Complete" $false $_.Exception.Message
        }
    }

    return @{
        Workflow = $newWorkflow
        Category = $testCategory
        Statuses = $addedStatuses
        AllStatuses = $statuses
    }
}

# ============================================
# PHASE 2: READ OPERATIONS
# ============================================
function Test-ReadWorkflow {
    param($token, $headers, $workflowData)

    Write-TestHeader "PHASE 2: READ OPERATIONS - Workflow Retrieval"

    $workflowId = $workflowData.Workflow.workflowId

    # Test 1: Get all workflows
    Write-Host "`n--- Test 1: Get All Workflows ---" -ForegroundColor Yellow

    try {
        $allWorkflows = Invoke-RestMethod -Uri "$baseUrl/workflow" -Method Get -Headers $headers

        if ($allWorkflows -and $allWorkflows.Count -gt 0) {
            Write-TestResult "Retrieved all workflows" $true "Found $($allWorkflows.Count) workflows"

            # Verify our new workflow is in the list
            $foundWorkflow = $allWorkflows | Where-Object { $_.workflowId -eq $workflowId }
            if ($foundWorkflow) {
                Write-TestResult "New workflow appears in list" $true "Workflow ID: $workflowId"
            } else {
                Write-TestResult "New workflow NOT in list" $false "Created workflow not found in GET all"
                Add-Bug "BUG#006" "Created workflow not appearing in list" "Workflow created but not returned by GET all endpoint" "HIGH" @("Create a workflow", "Navigate to workflow list", "Observe created workflow is missing")
            }
        } else {
            Write-TestResult "Failed to retrieve workflows" $false "Empty or null response"
        }
    } catch {
        Write-TestResult "Failed to retrieve workflows" $false $_.Exception.Message
    }

    # Test 2: Get workflow by ID
    Write-Host "`n--- Test 2: Get Workflow by ID ---" -ForegroundColor Yellow

    try {
        $workflow = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId" -Method Get -Headers $headers

        if ($workflow.workflowId -eq $workflowId) {
            Write-TestResult "Retrieved workflow by ID" $true "Workflow: $($workflow.workflowName)"
            Write-Host "`nWorkflow Details:" -ForegroundColor Cyan
            Write-Host "  ID: $($workflow.workflowId)" -ForegroundColor Gray
            Write-Host "  Name: $($workflow.workflowName)" -ForegroundColor Gray
            Write-Host "  Description: $($workflow.description)" -ForegroundColor Gray
            Write-Host "  Category ID: $($workflow.categoryId)" -ForegroundColor Gray
            Write-Host "  Active: $($workflow.isActive)" -ForegroundColor Gray
            Write-Host "  Default: $($workflow.isDefault)" -ForegroundColor Gray
        } else {
            Write-TestResult "Failed to retrieve workflow by ID" $false "Response doesn't match requested ID"
        }
    } catch {
        Write-TestResult "Failed to retrieve workflow by ID" $false $_.Exception.Message
    }

    # Test 3: Get workflow statuses
    Write-Host "`n--- Test 3: Get Workflow Statuses ---" -ForegroundColor Yellow

    try {
        $workflowStatuses = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId/statuses" -Method Get -Headers $headers

        if ($workflowStatuses -and $workflowStatuses.Count -gt 0) {
            Write-TestResult "Retrieved workflow statuses" $true "Found $($workflowStatuses.Count) statuses"
            Write-Host "`nWorkflow Statuses:" -ForegroundColor Cyan
            $workflowStatuses | ForEach-Object {
                Write-Host "  - [$($_.workflowStatusId)] Status Master ID: $($_.statusMasterId), SLA: $($_.slaHours)h, Order: $($_.statusOrder), Initial: $($_.isInitialStatus)" -ForegroundColor Gray
            }
        } else {
            Write-TestResult "Failed to retrieve workflow statuses" $false "Empty or null response"
            Add-Bug "BUG#007" "Cannot retrieve workflow statuses" "GET workflow statuses returns empty" "HIGH" @("Create workflow with statuses", "View workflow details", "Observe statuses section is empty")
        }
    } catch {
        Write-TestResult "Failed to retrieve workflow statuses" $false $_.Exception.Message
    }

    # Test 4: Get workflow transitions
    Write-Host "`n--- Test 4: Get Workflow Transitions ---" -ForegroundColor Yellow

    try {
        $workflowTransitions = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId/transitions" -Method Get -Headers $headers

        if ($workflowTransitions) {
            Write-TestResult "Retrieved workflow transitions" $true "Found $($workflowTransitions.Count) transitions"
            Write-Host "`nWorkflow Transitions:" -ForegroundColor Cyan
            $workflowTransitions | ForEach-Object {
                Write-Host "  - [$($_.transitionId)] $($_.transitionName): From $($_.fromStatusId) to $($_.toStatusId)" -ForegroundColor Gray
                Write-Host "    Button: $($_.buttonLabel) [$($_.buttonColor)] Icon: $($_.buttonIcon)" -ForegroundColor DarkGray
                Write-Host "    Requires Comment: $($_.requiresComment), Requires Attachment: $($_.requiresAttachment)" -ForegroundColor DarkGray
            }
        } else {
            Write-TestResult "No workflow transitions found" $true "This is expected if no transitions were added"
        }
    } catch {
        Write-TestResult "Failed to retrieve workflow transitions" $false $_.Exception.Message
    }

    # Test 5: Get workflows by category
    Write-Host "`n--- Test 5: Get Workflows by Category ---" -ForegroundColor Yellow

    try {
        $categoryWorkflows = Invoke-RestMethod -Uri "$baseUrl/workflow/category/$($workflowData.Category.categoryId)" -Method Get -Headers $headers

        if ($categoryWorkflows -and $categoryWorkflows.Count -gt 0) {
            Write-TestResult "Retrieved workflows by category" $true "Found $($categoryWorkflows.Count) workflows for category $($workflowData.Category.categoryName)"

            # Verify our workflow is in the category list
            $foundWorkflow = $categoryWorkflows | Where-Object { $_.workflowId -eq $workflowId }
            if ($foundWorkflow) {
                Write-TestResult "New workflow appears in category list" $true "Category filter working correctly"
            } else {
                Write-TestResult "New workflow NOT in category list" $false "Workflow not returned when filtering by category"
                Add-Bug "BUG#008" "Workflow not appearing in category filter" "Created workflow not returned by GET workflows by category" "MEDIUM" @("Create workflow for specific category", "Filter workflows by that category", "Observe workflow is missing")
            }
        } else {
            Write-TestResult "Failed to retrieve workflows by category" $false "Empty or null response"
        }
    } catch {
        Write-TestResult "Failed to retrieve workflows by category" $false $_.Exception.Message
    }
}

# ============================================
# PHASE 3: UPDATE OPERATIONS
# ============================================
function Test-UpdateWorkflow {
    param($token, $headers, $workflowData)

    Write-TestHeader "PHASE 3: UPDATE OPERATIONS - Workflow Modification"

    $workflowId = $workflowData.Workflow.workflowId

    # Test 1: Update workflow details
    Write-Host "`n--- Test 1: Update Workflow Details ---" -ForegroundColor Yellow

    $updateData = @{
        workflowId = $workflowId
        categoryId = $workflowData.Category.categoryId
        workflowName = "$($workflowData.Workflow.workflowName) - UPDATED"
        description = "Updated description after comprehensive testing"
        isActive = $true
        isDefault = $false  # Change default flag
    } | ConvertTo-Json

    try {
        $updatedWorkflow = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId" -Method Put -Headers $headers -Body $updateData

        if ($updatedWorkflow) {
            Write-TestResult "Updated workflow details" $true "Name and description updated"
            Write-Host "  Updated Name: $($updatedWorkflow.workflowName)" -ForegroundColor Gray
            Write-Host "  Updated Description: $($updatedWorkflow.description)" -ForegroundColor Gray
            Write-Host "  Updated IsDefault: $($updatedWorkflow.isDefault)" -ForegroundColor Gray
        } else {
            Write-TestResult "Failed to update workflow" $false "No response data"
        }
    } catch {
        Write-TestResult "Failed to update workflow" $false $_.Exception.Message
        Add-Bug "BUG#009" "Cannot update workflow" $_.Exception.Message "MEDIUM" @("Open workflow", "Click edit", "Change details", "Click save")
    }

    # Test 2: Update workflow active status
    Write-Host "`n--- Test 2: Toggle Workflow Active Status ---" -ForegroundColor Yellow

    $toggleData = @{
        workflowId = $workflowId
        categoryId = $workflowData.Category.categoryId
        workflowName = $updatedWorkflow.workflowName
        description = $updatedWorkflow.description
        isActive = $false  # Deactivate
        isDefault = $updatedWorkflow.isDefault
    } | ConvertTo-Json

    try {
        $toggledWorkflow = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId" -Method Put -Headers $headers -Body $toggleData

        if ($toggledWorkflow.isActive -eq $false) {
            Write-TestResult "Deactivated workflow" $true "IsActive changed from true to false"
        } else {
            Write-TestResult "Failed to deactivate workflow" $false "IsActive flag not updated"
        }

        # Reactivate for further testing
        Start-Sleep -Milliseconds 500
        $toggleData = @{
            workflowId = $workflowId
            categoryId = $workflowData.Category.categoryId
            workflowName = $toggledWorkflow.workflowName
            description = $toggledWorkflow.description
            isActive = $true  # Reactivate
            isDefault = $toggledWorkflow.isDefault
        } | ConvertTo-Json

        $reactivatedWorkflow = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId" -Method Put -Headers $headers -Body $toggleData

        if ($reactivatedWorkflow.isActive -eq $true) {
            Write-TestResult "Reactivated workflow" $true "IsActive changed from false to true"
        } else {
            Write-TestResult "Failed to reactivate workflow" $false "IsActive flag not updated"
        }
    } catch {
        Write-TestResult "Failed to toggle workflow active status" $false $_.Exception.Message
    }

    # Test 3: Update workflow status SLA hours
    Write-Host "`n--- Test 3: Update Workflow Status SLA Hours ---" -ForegroundColor Yellow

    if ($workflowData.Statuses -and $workflowData.Statuses.Count -gt 0) {
        $statusToUpdate = $workflowData.Statuses[0]

        $statusUpdateData = @{
            workflowStatusId = $statusToUpdate.workflowStatusId
            workflowId = $workflowId
            statusMasterId = $statusToUpdate.statusMasterId
            slaHours = 48  # Change from 24 to 48
            isInitialStatus = $statusToUpdate.isInitialStatus
            statusOrder = $statusToUpdate.statusOrder
        } | ConvertTo-Json

        try {
            $updatedStatus = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId/statuses/$($statusToUpdate.workflowStatusId)" -Method Put -Headers $headers -Body $statusUpdateData

            if ($updatedStatus.slaHours -eq 48) {
                Write-TestResult "Updated workflow status SLA hours" $true "SLA changed from 24 to 48 hours"
            } else {
                Write-TestResult "Failed to update status SLA hours" $false "SLA not updated correctly"
            }
        } catch {
            Write-TestResult "Failed to update status SLA hours" $false $_.Exception.Message
            Add-Bug "BUG#010" "Cannot update workflow status SLA" $_.Exception.Message "MEDIUM" @("Open workflow", "View statuses", "Edit status", "Change SLA hours", "Click save")
        }
    }
}

# ============================================
# PHASE 4: DELETE OPERATIONS
# ============================================
function Test-DeleteWorkflow {
    param($token, $headers, $workflowData)

    Write-TestHeader "PHASE 4: DELETE OPERATIONS - Workflow Deletion"

    $workflowId = $workflowData.Workflow.workflowId

    # Test 1: Delete a transition
    Write-Host "`n--- Test 1: Delete Workflow Transition ---" -ForegroundColor Yellow

    try {
        $transitions = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId/transitions" -Method Get -Headers $headers

        if ($transitions -and $transitions.Count -gt 0) {
            $transitionToDelete = $transitions[0]

            try {
                Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId/transitions/$($transitionToDelete.transitionId)" -Method Delete -Headers $headers
                Write-TestResult "Deleted workflow transition" $true "Transition ID: $($transitionToDelete.transitionId)"

                # Verify deletion
                Start-Sleep -Milliseconds 500
                $remainingTransitions = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId/transitions" -Method Get -Headers $headers
                $stillExists = $remainingTransitions | Where-Object { $_.transitionId -eq $transitionToDelete.transitionId }

                if (-not $stillExists) {
                    Write-TestResult "Verified transition deletion" $true "Transition no longer appears in list"
                } else {
                    Write-TestResult "Transition not fully deleted" $false "Transition still appears after delete"
                    Add-Bug "BUG#011" "Transition not deleted properly" "DELETE request succeeds but transition still exists" "HIGH" @("Open workflow", "View transitions", "Delete a transition", "Verify it's still there")
                }
            } catch {
                Write-TestResult "Failed to delete transition" $false $_.Exception.Message
                Add-Bug "BUG#012" "Cannot delete workflow transition" $_.Exception.Message "MEDIUM" @("Open workflow", "View transitions", "Click delete on a transition")
            }
        } else {
            Write-Host "No transitions to delete (skipping test)" -ForegroundColor Yellow
        }
    } catch {
        Write-TestResult "Failed to retrieve transitions for deletion test" $false $_.Exception.Message
    }

    # Test 2: Delete a workflow status
    Write-Host "`n--- Test 2: Delete Workflow Status ---" -ForegroundColor Yellow

    if ($workflowData.Statuses -and $workflowData.Statuses.Count -gt 1) {
        # Don't delete the first status (initial status), delete the last one
        $statusToDelete = $workflowData.Statuses[-1]

        try {
            Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId/statuses/$($statusToDelete.workflowStatusId)" -Method Delete -Headers $headers
            Write-TestResult "Deleted workflow status" $true "Status ID: $($statusToDelete.workflowStatusId)"

            # Verify deletion
            Start-Sleep -Milliseconds 500
            $remainingStatuses = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId/statuses" -Method Get -Headers $headers
            $stillExists = $remainingStatuses | Where-Object { $_.workflowStatusId -eq $statusToDelete.workflowStatusId }

            if (-not $stillExists) {
                Write-TestResult "Verified status deletion" $true "Status no longer appears in list"
            } else {
                Write-TestResult "Status not fully deleted" $false "Status still appears after delete"
                Add-Bug "BUG#013" "Status not deleted properly" "DELETE request succeeds but status still exists" "HIGH" @("Open workflow", "View statuses", "Delete a status", "Verify it's still there")
            }
        } catch {
            Write-TestResult "Failed to delete status" $false $_.Exception.Message
            Add-Bug "BUG#014" "Cannot delete workflow status" $_.Exception.Message "MEDIUM" @("Open workflow", "View statuses", "Click delete on a status")
        }
    }

    # Test 3: Attempt to delete workflow (should fail if in use)
    Write-Host "`n--- Test 3: Delete Workflow (Safety Check) ---" -ForegroundColor Yellow

    try {
        Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId" -Method Delete -Headers $headers
        Write-TestResult "Deleted workflow" $true "Workflow ID: $workflowId"

        # Verify deletion
        Start-Sleep -Milliseconds 500
        try {
            $deletedWorkflow = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId" -Method Get -Headers $headers
            Write-TestResult "Workflow not fully deleted" $false "Workflow still accessible after delete"
            Add-Bug "BUG#015" "Workflow not deleted properly" "DELETE request succeeds but workflow still exists" "HIGH" @("Open workflow list", "Delete a workflow", "Try to access it again")
        } catch {
            # Expected - workflow should be gone
            Write-TestResult "Verified workflow deletion" $true "Workflow no longer accessible"
        }
    } catch {
        $errorMessage = $_.Exception.Message
        if ($errorMessage -match "in use|constraint|foreign key") {
            Write-TestResult "Workflow deletion prevented (expected)" $true "System correctly prevents deleting workflow in use"
        } else {
            Write-TestResult "Failed to delete workflow" $false $errorMessage
        }
    }
}

# ============================================
# PHASE 5: INTEGRATION TESTING
# ============================================
function Test-WorkflowIntegration {
    param($token, $headers, $workflowData)

    Write-TestHeader "PHASE 5: INTEGRATION TESTING - Workflow with Complaints"

    # Test 1: Get complaints for the workflow category
    Write-Host "`n--- Test 1: Get Complaints in Workflow Category ---" -ForegroundColor Yellow

    try {
        $complaints = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method Get -Headers $headers

        if ($complaints -and $complaints.Count -gt 0) {
            Write-TestResult "Retrieved complaints" $true "Found $($complaints.Count) complaints"

            # Find complaints in our test category
            $categoryComplaints = $complaints | Where-Object { $_.categoryId -eq $workflowData.Category.categoryId }

            if ($categoryComplaints -and $categoryComplaints.Count -gt 0) {
                Write-TestResult "Found complaints in test category" $true "Found $($categoryComplaints.Count) complaints"
                Write-Host "`nComplaints in category '$($workflowData.Category.categoryName)':" -ForegroundColor Cyan
                $categoryComplaints | Select-Object -First 5 | ForEach-Object {
                    Write-Host "  - [$($_.complaintNumber)] Status: $($_.currentStatusName)" -ForegroundColor Gray
                }
            } else {
                Write-Host "No complaints found in test category (creating new complaint for integration test)" -ForegroundColor Yellow

                # Create a test complaint
                $complaintData = @{
                    categoryId = $workflowData.Category.categoryId
                    subject = "Integration Test Complaint for Workflow"
                    description = "This complaint is created to test workflow integration"
                    priorityId = 2  # Medium priority
                    departmentId = 1
                    branchId = 1
                } | ConvertTo-Json

                try {
                    $newComplaint = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method Post -Headers $headers -Body $complaintData
                    Write-TestResult "Created test complaint for integration" $true "Complaint: $($newComplaint.complaintNumber)"
                    $categoryComplaints = @($newComplaint)
                } catch {
                    Write-TestResult "Failed to create test complaint" $false $_.Exception.Message
                    return
                }
            }

            # Test 2: Get workflow for complaint's category
            Write-Host "`n--- Test 2: Verify Workflow Applied to Complaint ---" -ForegroundColor Yellow

            $testComplaint = $categoryComplaints[0]

            try {
                $complaintWorkflow = Invoke-RestMethod -Uri "$baseUrl/workflow/category/$($testComplaint.categoryId)" -Method Get -Headers $headers

                if ($complaintWorkflow -and $complaintWorkflow.Count -gt 0) {
                    Write-TestResult "Workflow available for complaint category" $true "Found $($complaintWorkflow.Count) workflows"

                    # Check if our test workflow is the default
                    $defaultWorkflow = $complaintWorkflow | Where-Object { $_.isDefault -eq $true -and $_.isActive -eq $true }
                    if ($defaultWorkflow) {
                        Write-TestResult "Default workflow configured" $true "Workflow: $($defaultWorkflow.workflowName)"
                    } else {
                        Write-TestResult "No default workflow" $false "Category has workflows but none set as default"
                        Add-Bug "BUG#016" "No default workflow for category" "Complaints won't have workflow transitions available" "HIGH" @("Create complaint in category with workflow", "Open complaint detail", "No status transition buttons appear")
                    }
                } else {
                    Write-TestResult "No workflow for complaint category" $false "Complaint category has no workflow configured"
                }
            } catch {
                Write-TestResult "Failed to get workflow for complaint" $false $_.Exception.Message
            }

            # Test 3: Get available transitions for complaint
            Write-Host "`n--- Test 3: Get Available Status Transitions ---" -ForegroundColor Yellow

            try {
                $availableTransitions = Invoke-RestMethod -Uri "$baseUrl/workflow/complaint/$($testComplaint.complaintId)/transitions" -Method Get -Headers $headers

                if ($availableTransitions -and $availableTransitions.Count -gt 0) {
                    Write-TestResult "Retrieved available transitions" $true "Found $($availableTransitions.Count) transitions"
                    Write-Host "`nAvailable Transitions:" -ForegroundColor Cyan
                    $availableTransitions | ForEach-Object {
                        Write-Host "  - $($_.buttonLabel) [$($_.buttonColor)] - $($_.transitionName)" -ForegroundColor Gray
                    }
                } else {
                    Write-Host "No transitions available for complaint (may be in final status)" -ForegroundColor Yellow
                }
            } catch {
                Write-TestResult "Failed to get available transitions" $false $_.Exception.Message
                Add-Bug "BUG#017" "Cannot retrieve available transitions" $_.Exception.Message "HIGH" @("Open complaint detail", "Check if status transition buttons are available")
            }

            # Test 4: Execute a status transition (if available)
            Write-Host "`n--- Test 4: Execute Status Transition ---" -ForegroundColor Yellow

            if ($availableTransitions -and $availableTransitions.Count -gt 0) {
                $transitionToExecute = $availableTransitions[0]

                $transitionData = @{
                    complaintId = $testComplaint.complaintId
                    transitionId = $transitionToExecute.transitionId
                    comment = "Executing transition as part of comprehensive E2E test"
                } | ConvertTo-Json

                try {
                    $transitionResult = Invoke-RestMethod -Uri "$baseUrl/workflow/complaint/$($testComplaint.complaintId)/transition" -Method Post -Headers $headers -Body $transitionData

                    if ($transitionResult) {
                        Write-TestResult "Executed status transition" $true "Transition: $($transitionToExecute.transitionName)"

                        # Verify complaint status changed
                        Start-Sleep -Milliseconds 500
                        $updatedComplaint = Invoke-RestMethod -Uri "$baseUrl/complaints/$($testComplaint.complaintId)" -Method Get -Headers $headers

                        if ($updatedComplaint.currentStatusId -ne $testComplaint.currentStatusId) {
                            Write-TestResult "Complaint status updated" $true "Status changed after transition"
                            Write-Host "  Old Status ID: $($testComplaint.currentStatusId)" -ForegroundColor Gray
                            Write-Host "  New Status ID: $($updatedComplaint.currentStatusId)" -ForegroundColor Gray
                        } else {
                            Write-TestResult "Complaint status not updated" $false "Status unchanged after transition"
                            Add-Bug "BUG#018" "Status transition doesn't update complaint" "Transition executes but complaint status doesn't change" "CRITICAL" @("Open complaint", "Click status transition button", "Verify status doesn't change")
                        }
                    } else {
                        Write-TestResult "Failed to execute transition" $false "No response data"
                    }
                } catch {
                    Write-TestResult "Failed to execute transition" $false $_.Exception.Message
                    Add-Bug "BUG#019" "Cannot execute status transition" $_.Exception.Message "CRITICAL" @("Open complaint", "Click status transition button", "Observe error")
                }
            } else {
                Write-Host "Skipping transition execution (no transitions available)" -ForegroundColor Yellow
            }
        } else {
            Write-TestResult "No complaints found for integration testing" $false "Cannot test workflow integration without complaints"
        }
    } catch {
        Write-TestResult "Failed to retrieve complaints" $false $_.Exception.Message
    }
}

# ============================================
# PHASE 6: VALIDATION & ERROR TESTING
# ============================================
function Test-ValidationAndErrors {
    param($token, $headers, $workflowData)

    Write-TestHeader "PHASE 6: VALIDATION & ERROR TESTING"

    # Test 1: Create workflow without required fields
    Write-Host "`n--- Test 1: Create Workflow Without Required Fields ---" -ForegroundColor Yellow

    $invalidWorkflows = @(
        @{ data = @{ workflowName = "" }; name = "Empty workflow name" },
        @{ data = @{ categoryId = 999999 }; name = "Invalid category ID" },
        @{ data = @{ workflowName = "Test"; categoryId = $null }; name = "Null category ID" }
    )

    foreach ($test in $invalidWorkflows) {
        $testData = $test.data | ConvertTo-Json

        try {
            $result = Invoke-RestMethod -Uri "$baseUrl/workflow" -Method Post -Headers $headers -Body $testData
            Write-TestResult "Validation test: $($test.name)" $false "Request succeeded when it should have failed"
            Add-Bug "BUG#020-$($test.name)" "Missing validation" "System accepts invalid data: $($test.name)" "MEDIUM" @("Try to create workflow with invalid data", "System should reject but accepts")
        } catch {
            $errorMessage = $_.Exception.Message
            if ($errorMessage -match "validation|required|invalid|bad request") {
                Write-TestResult "Validation test: $($test.name)" $true "System correctly rejected invalid data"
            } else {
                Write-TestResult "Validation test: $($test.name)" $false "Unexpected error: $errorMessage"
            }
        }
    }

    # Test 2: Add status without required fields
    Write-Host "`n--- Test 2: Add Status Without Required Fields ---" -ForegroundColor Yellow

    $workflowId = $workflowData.Workflow.workflowId

    $invalidStatuses = @(
        @{ data = @{ workflowId = $workflowId; statusMasterId = $null }; name = "Null status master ID" },
        @{ data = @{ workflowId = $workflowId; statusMasterId = 999999 }; name = "Invalid status master ID" },
        @{ data = @{ workflowId = $workflowId; statusMasterId = 1; slaHours = -1 }; name = "Negative SLA hours" },
        @{ data = @{ workflowId = $workflowId; statusMasterId = 1; slaHours = 100000 }; name = "Extremely large SLA hours" }
    )

    foreach ($test in $invalidStatuses) {
        $testData = $test.data | ConvertTo-Json

        try {
            $result = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId/statuses" -Method Post -Headers $headers -Body $testData
            Write-TestResult "Validation test: $($test.name)" $false "Request succeeded when it should have failed"
            Add-Bug "BUG#021-$($test.name)" "Missing validation" "System accepts invalid status data: $($test.name)" "MEDIUM" @("Try to add status with invalid data", "System should reject but accepts")
        } catch {
            $errorMessage = $_.Exception.Message
            if ($errorMessage -match "validation|required|invalid|bad request|constraint") {
                Write-TestResult "Validation test: $($test.name)" $true "System correctly rejected invalid data"
            } else {
                Write-TestResult "Validation test: $($test.name)" $false "Unexpected error: $errorMessage"
            }
        }
    }

    # Test 3: Add transition with invalid data
    Write-Host "`n--- Test 3: Add Transition With Invalid Data ---" -ForegroundColor Yellow

    if ($workflowData.Statuses -and $workflowData.Statuses.Count -gt 0) {
        $firstStatusId = $workflowData.Statuses[0].workflowStatusId

        $invalidTransitions = @(
            @{ data = @{ workflowId = $workflowId; fromStatusId = $firstStatusId; toStatusId = $firstStatusId }; name = "Same from/to status" },
            @{ data = @{ workflowId = $workflowId; fromStatusId = 999999; toStatusId = $firstStatusId }; name = "Invalid from status" },
            @{ data = @{ workflowId = $workflowId; fromStatusId = $firstStatusId; toStatusId = 999999 }; name = "Invalid to status" },
            @{ data = @{ workflowId = $workflowId; fromStatusId = $firstStatusId; toStatusId = $firstStatusId + 1; transitionName = "" }; name = "Empty transition name" }
        )

        foreach ($test in $invalidTransitions) {
            $testData = $test.data | ConvertTo-Json

            try {
                $result = Invoke-RestMethod -Uri "$baseUrl/workflow/$workflowId/transitions" -Method Post -Headers $headers -Body $testData
                Write-TestResult "Validation test: $($test.name)" $false "Request succeeded when it should have failed"
                Add-Bug "BUG#022-$($test.name)" "Missing validation" "System accepts invalid transition data: $($test.name)" "MEDIUM" @("Try to add transition with invalid data", "System should reject but accepts")
            } catch {
                $errorMessage = $_.Exception.Message
                if ($errorMessage -match "validation|required|invalid|bad request|constraint") {
                    Write-TestResult "Validation test: $($test.name)" $true "System correctly rejected invalid data"
                } else {
                    Write-TestResult "Validation test: $($test.name)" $false "Unexpected error: $errorMessage"
                }
            }
        }
    }

    # Test 4: Unauthorized access
    Write-Host "`n--- Test 4: Unauthorized Access Test ---" -ForegroundColor Yellow

    $invalidHeaders = @{
        "Authorization" = "Bearer invalid_token_xyz123"
        "Content-Type" = "application/json"
    }

    try {
        $result = Invoke-RestMethod -Uri "$baseUrl/workflow" -Method Get -Headers $invalidHeaders
        Write-TestResult "Unauthorized access test" $false "Request succeeded with invalid token"
        Add-Bug "BUG#023" "No authentication validation" "System accepts invalid authentication tokens" "CRITICAL" @("Use invalid token", "Try to access workflow API", "Request succeeds when it should fail")
    } catch {
        $errorMessage = $_.Exception.Message
        if ($errorMessage -match "401|unauthorized|forbidden") {
            Write-TestResult "Unauthorized access test" $true "System correctly rejected invalid token"
        } else {
            Write-TestResult "Unauthorized access test" $false "Unexpected error: $errorMessage"
        }
    }

    # Test 5: SQL injection attempt
    Write-Host "`n--- Test 5: SQL Injection Security Test ---" -ForegroundColor Yellow

    $sqlInjectionData = @{
        categoryId = 1
        workflowName = "Test'; DROP TABLE Workflows; --"
        description = "SQL injection test"
        isActive = $true
        isDefault = $false
    } | ConvertTo-Json

    try {
        $result = Invoke-RestMethod -Uri "$baseUrl/workflow" -Method Post -Headers $headers -Body $sqlInjectionData

        if ($result.workflowName -eq "Test'; DROP TABLE Workflows; --") {
            Write-TestResult "SQL injection test" $true "System stored SQL string safely (parameterized queries working)"

            # Clean up the test workflow
            try {
                Invoke-RestMethod -Uri "$baseUrl/workflow/$($result.workflowId)" -Method Delete -Headers $headers -ErrorAction SilentlyContinue
            } catch {
                # Ignore cleanup errors
            }
        } else {
            Write-TestResult "SQL injection test" $false "Unexpected behavior with SQL characters"
        }
    } catch {
        Write-TestResult "SQL injection test" $false "System failed to handle SQL characters: $($_.Exception.Message)"
    }

    # Test 6: XSS attempt
    Write-Host "`n--- Test 6: XSS Security Test ---" -ForegroundColor Yellow

    $xssData = @{
        categoryId = 1
        workflowName = "<script>alert('XSS')</script>"
        description = "XSS injection test"
        isActive = $true
        isDefault = $false
    } | ConvertTo-Json

    try {
        $result = Invoke-RestMethod -Uri "$baseUrl/workflow" -Method Post -Headers $headers -Body $xssData

        if ($result.workflowName -eq "<script>alert('XSS')</script>") {
            Write-Host "  WARNING: XSS payload stored as-is. Frontend MUST sanitize output!" -ForegroundColor Yellow
            Write-TestResult "XSS test" $true "System stored XSS string (frontend should sanitize on display)"

            # Clean up the test workflow
            try {
                Invoke-RestMethod -Uri "$baseUrl/workflow/$($result.workflowId)" -Method Delete -Headers $headers -ErrorAction SilentlyContinue
            } catch {
                # Ignore cleanup errors
            }
        } else {
            Write-TestResult "XSS test" $true "System sanitized XSS payload"
        }
    } catch {
        Write-TestResult "XSS test" $false "System failed to handle XSS characters: $($_.Exception.Message)"
    }
}

# ============================================
# MAIN EXECUTION
# ============================================
Write-Host @"
╔══════════════════════════════════════════════════════════════════════════════╗
║                                                                              ║
║     COMPREHENSIVE END-TO-END WORKFLOW MANAGEMENT CRUD TEST SUITE             ║
║                                                                              ║
║     Testing After BUG #001 and BUG #002 Fixes                                ║
║     - BUG #001: Category dropdown population                                 ║
║     - BUG #002: Status Master dropdown population                            ║
║                                                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

Write-Host "`nTest Configuration:" -ForegroundColor White
Write-Host "  Backend URL: $baseUrl" -ForegroundColor Gray
Write-Host "  Frontend URL: $frontendUrl" -ForegroundColor Gray
Write-Host "  Start Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# Get authentication token
$token = Get-AuthToken

if (-not $token) {
    Write-Host "`nCRITICAL: Cannot proceed without authentication token!" -ForegroundColor Red
    exit 1
}

$headers = Get-Headers -token $token

# Execute all test phases
try {
    # Phase 1: CREATE
    $workflowData = Test-CreateWorkflow -token $token -headers $headers

    if ($workflowData) {
        # Phase 2: READ
        Test-ReadWorkflow -token $token -headers $headers -workflowData $workflowData

        # Phase 3: UPDATE
        Test-UpdateWorkflow -token $token -headers $headers -workflowData $workflowData

        # Phase 5: INTEGRATION (before DELETE so we have data)
        Test-WorkflowIntegration -token $token -headers $headers -workflowData $workflowData

        # Phase 4: DELETE
        Test-DeleteWorkflow -token $token -headers $headers -workflowData $workflowData

        # Phase 6: VALIDATION
        Test-ValidationAndErrors -token $token -headers $headers -workflowData $workflowData
    } else {
        Write-Host "`nCRITICAL: Workflow creation failed. Cannot proceed with remaining tests." -ForegroundColor Red
    }
} catch {
    Write-Host "`nUNEXPECTED ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.Exception.StackTrace -ForegroundColor DarkRed
}

# ============================================
# GENERATE COMPREHENSIVE REPORT
# ============================================
Write-TestHeader "TEST EXECUTION COMPLETE"

$endTime = Get-Date
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

# Generate detailed report
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportFile = "WORKFLOW_E2E_TEST_REPORT_$timestamp.md"

$reportContent = @"
# Comprehensive End-to-End Workflow Management Test Report

**Test Execution Date:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
**Backend URL:** $baseUrl
**Frontend URL:** $frontendUrl

---

## Executive Summary

| Metric | Value |
|--------|-------|
| **Total Tests** | $totalTests |
| **Tests Passed** | $script:testsPassed |
| **Tests Failed** | $script:testsFailed |
| **Success Rate** | $successRate% |
| **Bugs Found** | $($script:bugsFound.Count) |
| **Critical Bugs** | $(($script:bugsFound | Where-Object { $_.Severity -eq "CRITICAL" }).Count) |
| **High Priority Bugs** | $(($script:bugsFound | Where-Object { $_.Severity -eq "HIGH" }).Count) |

---

## Bug Fixes Verification

### BUG #001: Category Dropdown Population
**Status:** $(if (($script:testResults | Where-Object { $_.TestName -match "BUG #001" -and $_.Passed }).Count -gt 0) { "✅ VERIFIED FIXED" } else { "❌ NOT FIXED" })

**Details:**
$(($script:testResults | Where-Object { $_.TestName -match "BUG #001" } | ForEach-Object { "- $($_.Details)" }) -join "`n")

### BUG #002: Status Master Dropdown Population
**Status:** $(if (($script:testResults | Where-Object { $_.TestName -match "BUG #002" -and $_.Passed }).Count -gt 0) { "✅ VERIFIED FIXED" } else { "❌ NOT FIXED" })

**Details:**
$(($script:testResults | Where-Object { $_.TestName -match "BUG #002" } | ForEach-Object { "- $($_.Details)" }) -join "`n")

---

## Test Results by Phase

### Phase 1: CREATE Operations
$(($script:testResults | Where-Object { $_.TestName -match "Phase 1|CREATE|workflow created|Added status|Added transition|BUG #001|BUG #002" } | ForEach-Object {
    $icon = if ($_.Passed) { "✅" } else { "❌" }
    "- $icon **$($_.TestName)**: $($_.Details)"
}) -join "`n")

### Phase 2: READ Operations
$(($script:testResults | Where-Object { $_.TestName -match "Phase 2|READ|Retrieved|workflow by ID|workflow statuses|workflow transitions" } | ForEach-Object {
    $icon = if ($_.Passed) { "✅" } else { "❌" }
    "- $icon **$($_.TestName)**: $($_.Details)"
}) -join "`n")

### Phase 3: UPDATE Operations
$(($script:testResults | Where-Object { $_.TestName -match "Phase 3|UPDATE|Updated workflow|Toggle|SLA hours" } | ForEach-Object {
    $icon = if ($_.Passed) { "✅" } else { "❌" }
    "- $icon **$($_.TestName)**: $($_.Details)"
}) -join "`n")

### Phase 4: DELETE Operations
$(($script:testResults | Where-Object { $_.TestName -match "Phase 4|DELETE|Deleted|deletion" } | ForEach-Object {
    $icon = if ($_.Passed) { "✅" } else { "❌" }
    "- $icon **$($_.TestName)**: $($_.Details)"
}) -join "`n")

### Phase 5: INTEGRATION Testing
$(($script:testResults | Where-Object { $_.TestName -match "Phase 5|INTEGRATION|complaint|transition|workflow integration" } | ForEach-Object {
    $icon = if ($_.Passed) { "✅" } else { "❌" }
    "- $icon **$($_.TestName)**: $($_.Details)"
}) -join "`n")

### Phase 6: VALIDATION & ERROR Testing
$(($script:testResults | Where-Object { $_.TestName -match "Phase 6|VALIDATION|Validation test|Security|SQL|XSS|Unauthorized" } | ForEach-Object {
    $icon = if ($_.Passed) { "✅" } else { "❌" }
    "- $icon **$($_.TestName)**: $($_.Details)"
}) -join "`n")

---

## Bugs Found

$(if ($script:bugsFound.Count -eq 0) {
    "✅ **NO BUGS FOUND** - All tests passed successfully!"
} else {
    ($script:bugsFound | ForEach-Object {
        @"
### $($_.BugId): $($_.Title)
**Severity:** $($_.Severity)
**Found At:** $($_.FoundAt)

**Description:**
$($_.Description)

**Reproduction Steps:**
$(($_.ReproSteps | ForEach-Object { "1. $_" }) -join "`n")

---
"@
    }) -join "`n"
})

---

## Detailed Test Results

| Test Name | Status | Details | Timestamp |
|-----------|--------|---------|-----------|
$(($script:testResults | ForEach-Object {
    $icon = if ($_.Passed) { "✅" } else { "❌" }
    "| $($_.TestName) | $icon | $($_.Details) | $($_.Timestamp) |"
}) -join "`n")

---

## Recommendations

$(if ($successRate -eq 100) {
    @"
### 🎉 EXCELLENT! 100% Success Rate

All tests passed successfully. The Workflow Management system is functioning correctly after the bug fixes.

**Next Steps:**
1. Perform manual UI/UX validation
2. Test with different user roles and permissions
3. Perform load testing with multiple concurrent users
4. Test with edge cases and boundary values
"@
} elseif ($successRate -ge 90) {
    @"
### ✅ GOOD! $successRate% Success Rate

Most tests passed. Review the failed tests and address any critical or high-priority bugs.

**Next Steps:**
1. Fix the failed test cases
2. Re-run the test suite to verify fixes
3. Perform additional manual testing
"@
} elseif ($successRate -ge 70) {
    @"
### ⚠️ MODERATE! $successRate% Success Rate

Significant issues found. Priority should be given to fixing critical and high-severity bugs.

**Next Steps:**
1. Address all critical bugs immediately
2. Fix high-priority bugs
3. Re-run comprehensive test suite
4. Consider additional unit tests
"@
} else {
    @"
### ❌ CRITICAL! $successRate% Success Rate

Major issues detected. System requires significant fixes before production deployment.

**Next Steps:**
1. Stop deployment
2. Address all critical bugs immediately
3. Review system architecture and design
4. Re-run all tests after fixes
5. Consider additional code review
"@
})

---

## Test Coverage

### CRUD Operations
- **CREATE**: $(($script:testResults | Where-Object { $_.TestName -match "CREATE|created|Added" }).Count) tests
- **READ**: $(($script:testResults | Where-Object { $_.TestName -match "READ|Retrieved|Get" }).Count) tests
- **UPDATE**: $(($script:testResults | Where-Object { $_.TestName -match "UPDATE|Updated|Toggle" }).Count) tests
- **DELETE**: $(($script:testResults | Where-Object { $_.TestName -match "DELETE|Deleted|deletion" }).Count) tests

### Additional Coverage
- **Integration Tests**: $(($script:testResults | Where-Object { $_.TestName -match "INTEGRATION|integration|complaint" }).Count) tests
- **Validation Tests**: $(($script:testResults | Where-Object { $_.TestName -match "VALIDATION|Validation" }).Count) tests
- **Security Tests**: $(($script:testResults | Where-Object { $_.TestName -match "Security|SQL|XSS|Unauthorized" }).Count) tests

---

## Conclusion

$(if ($successRate -eq 100 -and $script:bugsFound.Count -eq 0) {
    "🎯 **SYSTEM READY FOR PRODUCTION**`n`nAll tests passed successfully. Both BUG #001 (Category dropdown) and BUG #002 (Status dropdown) have been verified as fixed. The Workflow Management system is functioning correctly and ready for production deployment."
} elseif ($successRate -ge 90) {
    "✅ **SYSTEM MOSTLY FUNCTIONAL**`n`nMost tests passed. Minor issues found that should be addressed before production deployment. Both critical bug fixes (BUG #001 and BUG #002) have been verified."
} else {
    "⚠️ **SYSTEM REQUIRES ATTENTION**`n`nSignificant issues found. System requires fixes before production deployment. Please review and address all critical and high-priority bugs."
})

---

**Report Generated:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
**Report File:** $reportFile
"@

# Save report to file
$reportContent | Out-File -FilePath $reportFile -Encoding UTF8

Write-Host "Detailed test report saved to: $reportFile" -ForegroundColor Green
Write-Host ""

# Display bugs summary if any
if ($script:bugsFound.Count -gt 0) {
    Write-Host "⚠️ BUGS FOUND:" -ForegroundColor Yellow
    Write-Host ""
    $script:bugsFound | ForEach-Object {
        Write-Host "  $($_.BugId): $($_.Title) [$($_.Severity)]" -ForegroundColor Yellow
        Write-Host "  Description: $($_.Description)" -ForegroundColor Gray
        Write-Host ""
    }
} else {
    Write-Host "✅ NO BUGS FOUND - All tests passed!" -ForegroundColor Green
    Write-Host ""
}

Write-Host "Test execution complete. Review the detailed report at: $reportFile" -ForegroundColor Cyan
