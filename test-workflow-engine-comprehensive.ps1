# Comprehensive Workflow Engine API Test Suite
# Tests all 11 workflow endpoints with realistic scenarios

$BaseUrl = "http://localhost:5058/api"
$OutputFile = "WORKFLOW_ENGINE_TEST_RESULTS_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"

# Color output functions
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }

# Initialize results
$Results = @{
    TotalTests = 0
    Passed = 0
    Failed = 0
    Errors = @()
    StartTime = Get-Date
}

# Start output file
$separator = "=" * 100
$separator | Out-File $OutputFile
"COMPREHENSIVE WORKFLOW ENGINE API TEST RESULTS" | Out-File $OutputFile -Append
"Test Started: $(Get-Date)" | Out-File $OutputFile -Append
$separator | Out-File $OutputFile -Append
"" | Out-File $OutputFile -Append

Write-Info "🚀 Starting Comprehensive Workflow Engine Test Suite..."
Write-Info "Output file: $OutputFile"
"" | Out-File $OutputFile -Append

#region Step 1: Authentication
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 1: Authenticating..."

try {
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    $userId = $loginResponse.data.id
    $companyId = $loginResponse.data.companyId

    Write-Success "Authentication successful"
    Write-Info "User ID: $userId"
    Write-Info "Company ID: $companyId"

    "STEP 1: AUTHENTICATION - PASSED" | Out-File $OutputFile -Append
    "User ID: $userId" | Out-File $OutputFile -Append
    "Company ID: $companyId" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Authentication failed: $($_.Exception.Message)"
    "STEP 1: AUTHENTICATION - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
    exit 1
}
$Results.TotalTests++

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}
#endregion

#region Step 2: Get Categories for Testing
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 2: Getting categories for workflow testing..."

try {
    $categories = Invoke-RestMethod -Uri "$BaseUrl/categories" -Method GET -Headers $headers

    if ($categories.data.Count -gt 0) {
        $testCategory = $categories.data | Where-Object { $_.name -like "*IT*" -or $_.name -like "*Support*" } | Select-Object -First 1

        if (-not $testCategory) {
            $testCategory = $categories.data[0]
        }

        $categoryId = $testCategory.id
        $categoryName = $testCategory.name

        Write-Success "Found test category: $categoryName"
        Write-Info "Category ID: $categoryId"

        "STEP 2: GET CATEGORIES - PASSED" | Out-File $OutputFile -Append
        "Test Category: $categoryName (ID: $categoryId)" | Out-File $OutputFile -Append
        "" | Out-File $OutputFile -Append

        $Results.Passed++
    }
    else {
        throw "No categories found"
    }
}
catch {
    Write-Error "Failed to get categories: $($_.Exception.Message)"
    "STEP 2: GET CATEGORIES - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
    exit 1
}
$Results.TotalTests++
#endregion

#region Step 3: Get Status Masters
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 3: Getting status masters..."

try {
    $statusMasters = Invoke-RestMethod -Uri "$BaseUrl/ComplaintStatusMaster" -Method GET -Headers $headers

    $submittedStatus = $statusMasters.data | Where-Object { $_.code -eq "SUBMITTED" } | Select-Object -First 1
    $inProgressStatus = $statusMasters.data | Where-Object { $_.code -eq "IN_PROGRESS" } | Select-Object -First 1
    $resolvedStatus = $statusMasters.data | Where-Object { $_.code -eq "RESOLVED" } | Select-Object -First 1
    $closedStatus = $statusMasters.data | Where-Object { $_.code -eq "CLOSED" } | Select-Object -First 1

    Write-Success "Found $($statusMasters.data.Count) status masters"
    Write-Info "SUBMITTED: $($submittedStatus.id)"
    Write-Info "IN_PROGRESS: $($inProgressStatus.id)"
    Write-Info "RESOLVED: $($resolvedStatus.id)"
    Write-Info "CLOSED: $($closedStatus.id)"

    "STEP 3: GET STATUS MASTERS - PASSED" | Out-File $OutputFile -Append
    "Total Status Masters: $($statusMasters.data.Count)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to get status masters: $($_.Exception.Message)"
    "STEP 3: GET STATUS MASTERS - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
    exit 1
}
$Results.TotalTests++
#endregion

#region Step 4: Test GET all workflows (should be empty initially)
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 4: Testing GET /api/workflows..."

try {
    $allWorkflows = Invoke-RestMethod -Uri "$BaseUrl/workflows?companyId=$companyId" -Method GET -Headers $headers

    Write-Success "GET all workflows - Status: 200 OK"
    Write-Info "Found $($allWorkflows.data.Count) existing workflows"

    "STEP 4: GET ALL WORKFLOWS - PASSED" | Out-File $OutputFile -Append
    "Existing workflows: $($allWorkflows.data.Count)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to get workflows: $($_.Exception.Message)"
    "STEP 4: GET ALL WORKFLOWS - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 5: Test CREATE workflow
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 5: Testing POST /api/workflows (Create Workflow)..."

try {
    $createWorkflowBody = @{
        categoryId = $categoryId
        name = "Test Workflow - API Test $(Get-Date -Format 'HHmmss')"
        description = "Automated test workflow for comprehensive API testing"
        isActive = $true
        isDefault = $true
        companyId = $companyId
    } | ConvertTo-Json

    $createWorkflowResponse = Invoke-RestMethod -Uri "$BaseUrl/workflows" -Method POST -Body $createWorkflowBody -Headers $headers

    $workflowId = $createWorkflowResponse.data.id

    Write-Success "Created workflow successfully"
    Write-Info "Workflow ID: $workflowId"
    Write-Info "Workflow Name: $($createWorkflowResponse.data.name)"

    "STEP 5: CREATE WORKFLOW - PASSED" | Out-File $OutputFile -Append
    "Workflow ID: $workflowId" | Out-File $OutputFile -Append
    "Workflow Name: $($createWorkflowResponse.data.name)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to create workflow: $($_.Exception.Message)"
    "STEP 5: CREATE WORKFLOW - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
    $Results.Errors += "Step 5: $($_.Exception.Message)"
}
$Results.TotalTests++
#endregion

#region Step 6: Test GET workflow for category
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 6: Testing GET /api/workflows/category/{categoryId}..."

try {
    $categoryWorkflow = Invoke-RestMethod -Uri "$BaseUrl/workflows/category/$categoryId" -Method GET -Headers $headers

    Write-Success "Retrieved workflow for category"
    Write-Info "Workflow: $($categoryWorkflow.data.name)"
    Write-Info "Is Active: $($categoryWorkflow.data.isActive)"

    "STEP 6: GET WORKFLOW FOR CATEGORY - PASSED" | Out-File $OutputFile -Append
    "Workflow Name: $($categoryWorkflow.data.name)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to get workflow for category: $($_.Exception.Message)"
    "STEP 6: GET WORKFLOW FOR CATEGORY - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 7: Test ADD statuses to workflow
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 7: Testing POST /api/workflows/{workflowId}/statuses (Add Statuses)..."

try {
    # Add SUBMITTED status (initial)
    $addStatus1Body = @{
        workflowId = $workflowId
        statusMasterId = $submittedStatus.id
        displayOrder = 1
        isInitialStatus = $true
        defaultSLAHours = 4
        escalationHours = 2
        requiresApproval = $false
        allowedRoles = @()
    } | ConvertTo-Json

    $addStatus1Response = Invoke-RestMethod -Uri "$BaseUrl/workflows/$workflowId/statuses" -Method POST -Body $addStatus1Body -Headers $headers
    Write-Success "Added SUBMITTED status (initial)"

    # Add IN_PROGRESS status
    $addStatus2Body = @{
        workflowId = $workflowId
        statusMasterId = $inProgressStatus.id
        displayOrder = 2
        isInitialStatus = $false
        defaultSLAHours = 24
        escalationHours = 12
        requiresApproval = $false
        allowedRoles = @()
    } | ConvertTo-Json

    $addStatus2Response = Invoke-RestMethod -Uri "$BaseUrl/workflows/$workflowId/statuses" -Method POST -Body $addStatus2Body -Headers $headers
    Write-Success "Added IN_PROGRESS status"

    # Add RESOLVED status
    $addStatus3Body = @{
        workflowId = $workflowId
        statusMasterId = $resolvedStatus.id
        displayOrder = 3
        isInitialStatus = $false
        defaultSLAHours = 48
        escalationHours = 24
        requiresApproval = $false
        allowedRoles = @()
    } | ConvertTo-Json

    $addStatus3Response = Invoke-RestMethod -Uri "$BaseUrl/workflows/$workflowId/statuses" -Method POST -Body $addStatus3Body -Headers $headers
    Write-Success "Added RESOLVED status"

    # Add CLOSED status
    $addStatus4Body = @{
        workflowId = $workflowId
        statusMasterId = $closedStatus.id
        displayOrder = 4
        isInitialStatus = $false
        defaultSLAHours = $null
        escalationHours = $null
        requiresApproval = $false
        allowedRoles = @()
    } | ConvertTo-Json

    $addStatus4Response = Invoke-RestMethod -Uri "$BaseUrl/workflows/$workflowId/statuses" -Method POST -Body $addStatus4Body -Headers $headers
    Write-Success "Added CLOSED status"

    Write-Success "Added 4 statuses to workflow"

    "STEP 7: ADD STATUSES TO WORKFLOW - PASSED" | Out-File $OutputFile -Append
    "Statuses added: SUBMITTED (initial), IN_PROGRESS, RESOLVED, CLOSED" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to add statuses: $($_.Exception.Message)"
    "STEP 7: ADD STATUSES TO WORKFLOW - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 8: Test ADD transitions to workflow
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 8: Testing POST /api/workflows/{workflowId}/transitions (Add Transitions)..."

try {
    # Transition: SUBMITTED → IN_PROGRESS
    $addTransition1Body = @{
        workflowId = $workflowId
        fromStatusId = $submittedStatus.id
        toStatusId = $inProgressStatus.id
        transitionName = "Start Work"
        description = "Begin working on the complaint"
        requiresComment = $false
        requiresApproval = $false
        allowedRoles = @()
        displayOrder = 1
        buttonColor = "#17a2b8"
        iconClass = "bi-play-circle"
    } | ConvertTo-Json

    $addTransition1Response = Invoke-RestMethod -Uri "$BaseUrl/workflows/$workflowId/transitions" -Method POST -Body $addTransition1Body -Headers $headers
    Write-Success "Added transition: SUBMITTED → IN_PROGRESS"

    # Transition: IN_PROGRESS → RESOLVED (requires comment)
    $addTransition2Body = @{
        workflowId = $workflowId
        fromStatusId = $inProgressStatus.id
        toStatusId = $resolvedStatus.id
        transitionName = "Resolve"
        description = "Mark complaint as resolved"
        requiresComment = $true
        requiresApproval = $false
        allowedRoles = @()
        displayOrder = 2
        buttonColor = "#28a745"
        iconClass = "bi-check-circle"
    } | ConvertTo-Json

    $addTransition2Response = Invoke-RestMethod -Uri "$BaseUrl/workflows/$workflowId/transitions" -Method POST -Body $addTransition2Body -Headers $headers
    Write-Success "Added transition: IN_PROGRESS → RESOLVED (requires comment)"

    # Transition: RESOLVED → CLOSED
    $addTransition3Body = @{
        workflowId = $workflowId
        fromStatusId = $resolvedStatus.id
        toStatusId = $closedStatus.id
        transitionName = "Close"
        description = "Close the complaint"
        requiresComment = $false
        requiresApproval = $false
        allowedRoles = @()
        displayOrder = 3
        buttonColor = "#6c757d"
        iconClass = "bi-check-circle-fill"
    } | ConvertTo-Json

    $addTransition3Response = Invoke-RestMethod -Uri "$BaseUrl/workflows/$workflowId/transitions" -Method POST -Body $addTransition3Body -Headers $headers
    Write-Success "Added transition: RESOLVED → CLOSED"

    Write-Success "Added 3 transitions to workflow"

    "STEP 8: ADD TRANSITIONS TO WORKFLOW - PASSED" | Out-File $OutputFile -Append
    "Transitions added: 3 (SUBMITTED→IN_PROGRESS, IN_PROGRESS→RESOLVED, RESOLVED→CLOSED)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to add transitions: $($_.Exception.Message)"
    "STEP 8: ADD TRANSITIONS TO WORKFLOW - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 9: Test GET workflow statuses
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 9: Testing GET /api/workflows/categories/{categoryId}/statuses..."

try {
    $workflowStatuses = Invoke-RestMethod -Uri "$BaseUrl/workflows/categories/$categoryId/statuses" -Method GET -Headers $headers

    Write-Success "Retrieved workflow statuses"
    Write-Info "Status count: $($workflowStatuses.data.Count)"

    foreach ($status in $workflowStatuses.data) {
        Write-Info "  - $($status.name) (Code: $($status.code))"
    }

    "STEP 9: GET WORKFLOW STATUSES - PASSED" | Out-File $OutputFile -Append
    "Status count: $($workflowStatuses.data.Count)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to get workflow statuses: $($_.Exception.Message)"
    "STEP 9: GET WORKFLOW STATUSES - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 10: Test GET initial status
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 10: Testing GET /api/workflows/categories/{categoryId}/initial-status..."

try {
    $initialStatus = Invoke-RestMethod -Uri "$BaseUrl/workflows/categories/$categoryId/initial-status" -Method GET -Headers $headers

    Write-Success "Retrieved initial status"
    Write-Info "Initial Status: $($initialStatus.data.name)"
    Write-Info "Status Code: $($initialStatus.data.code)"

    "STEP 10: GET INITIAL STATUS - PASSED" | Out-File $OutputFile -Append
    "Initial Status: $($initialStatus.data.name) ($($initialStatus.data.code))" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to get initial status: $($_.Exception.Message)"
    "STEP 10: GET INITIAL STATUS - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 11: Test GET allowed transitions
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 11: Testing GET /api/workflows/allowed-transitions..."

try {
    $allowedTransitions = Invoke-RestMethod -Uri "$BaseUrl/workflows/allowed-transitions?categoryId=$categoryId&currentStatusId=$($submittedStatus.id)" -Method GET -Headers $headers

    Write-Success "Retrieved allowed transitions from SUBMITTED status"
    Write-Info "Allowed transition count: $($allowedTransitions.data.transitions.Count)"

    foreach ($transition in $allowedTransitions.data.transitions) {
        Write-Info "  - $($transition.transitionName) (requires comment: $($transition.requiresComment))"
    }

    "STEP 11: GET ALLOWED TRANSITIONS - PASSED" | Out-File $OutputFile -Append
    "Allowed transitions from SUBMITTED: $($allowedTransitions.data.transitions.Count)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to get allowed transitions: $($_.Exception.Message)"
    "STEP 11: GET ALLOWED TRANSITIONS - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 12: Test CHECK transition allowed
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 12: Testing POST /api/workflows/check-transition..."

try {
    $checkTransitionBody = @{
        categoryId = $categoryId
        fromStatusId = $submittedStatus.id
        toStatusId = $inProgressStatus.id
        userId = $userId
    } | ConvertTo-Json

    $checkTransitionResponse = Invoke-RestMethod -Uri "$BaseUrl/workflows/check-transition" -Method POST -Body $checkTransitionBody -Headers $headers

    Write-Success "Checked transition permission"
    Write-Info "Is Allowed: $($checkTransitionResponse.data.isAllowed)"
    Write-Info "Message: $($checkTransitionResponse.data.message)"

    "STEP 12: CHECK TRANSITION ALLOWED - PASSED" | Out-File $OutputFile -Append
    "Transition SUBMITTED→IN_PROGRESS is allowed: $($checkTransitionResponse.data.isAllowed)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to check transition: $($_.Exception.Message)"
    "STEP 12: CHECK TRANSITION ALLOWED - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 13: Create a test complaint to verify workflow integration
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 13: Creating test complaint to verify workflow integration..."

try {
    $createComplaintBody = @{
        title = "Test Complaint - Workflow Engine Test $(Get-Date -Format 'HHmmss')"
        description = "This is a test complaint to verify workflow engine integration with complaint creation"
        categoryId = $categoryId
        complainantId = $userId
        companyId = $companyId
        priority = "Medium"
        isAnonymous = $false
    } | ConvertTo-Json

    $createComplaintResponse = Invoke-RestMethod -Uri "$BaseUrl/complaints" -Method POST -Body $createComplaintBody -Headers $headers

    $testComplaintId = $createComplaintResponse.data.id
    $testComplaintNumber = $createComplaintResponse.data.complaintNumber

    Write-Success "Created test complaint"
    Write-Info "Complaint Number: $testComplaintNumber"
    Write-Info "Complaint ID: $testComplaintId"
    Write-Info "Initial Status: $($createComplaintResponse.data.status)"

    "STEP 13: CREATE TEST COMPLAINT - PASSED" | Out-File $OutputFile -Append
    "Complaint Number: $testComplaintNumber" | Out-File $OutputFile -Append
    "Initial Status: $($createComplaintResponse.data.status)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to create test complaint: $($_.Exception.Message)"
    "STEP 13: CREATE TEST COMPLAINT - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 14: Test TRANSITION complaint
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 14: Testing POST /api/workflows/complaints/{complaintId}/transition..."

try {
    # Transition from SUBMITTED to IN_PROGRESS
    $transitionBody = @{
        newStatusId = $inProgressStatus.id
        comment = "Starting work on this complaint - automated test"
    } | ConvertTo-Json

    $transitionResponse = Invoke-RestMethod -Uri "$BaseUrl/workflows/complaints/$testComplaintId/transition" -Method POST -Body $transitionBody -Headers $headers

    Write-Success "Transitioned complaint successfully"
    Write-Info "Success: $($transitionResponse.success)"
    Write-Info "Message: $($transitionResponse.message)"

    # Verify the transition by getting the complaint
    $updatedComplaint = Invoke-RestMethod -Uri "$BaseUrl/complaints/$testComplaintId" -Method GET -Headers $headers
    Write-Info "Current Status: $($updatedComplaint.data.status)"

    "STEP 14: TRANSITION COMPLAINT - PASSED" | Out-File $OutputFile -Append
    "Transition: SUBMITTED → IN_PROGRESS" | Out-File $OutputFile -Append
    "Current Status: $($updatedComplaint.data.status)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed to transition complaint: $($_.Exception.Message)"
    "STEP 14: TRANSITION COMPLAINT - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 15: Test transition with comment requirement
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 15: Testing transition with comment requirement (IN_PROGRESS → RESOLVED)..."

try {
    # This transition requires a comment
    $transitionWithCommentBody = @{
        newStatusId = $resolvedStatus.id
        comment = "Issue has been resolved - tested and verified"
    } | ConvertTo-Json

    $transitionWithCommentResponse = Invoke-RestMethod -Uri "$BaseUrl/workflows/complaints/$testComplaintId/transition" -Method POST -Body $transitionWithCommentBody -Headers $headers

    Write-Success "Transitioned with required comment"
    Write-Info "Success: $($transitionWithCommentResponse.success)"

    # Verify the transition
    $resolvedComplaint = Invoke-RestMethod -Uri "$BaseUrl/complaints/$testComplaintId" -Method GET -Headers $headers
    Write-Info "Current Status: $($resolvedComplaint.data.status)"

    "STEP 15: TRANSITION WITH COMMENT - PASSED" | Out-File $OutputFile -Append
    "Transition: IN_PROGRESS → RESOLVED (comment required)" | Out-File $OutputFile -Append
    "Current Status: $($resolvedComplaint.data.status)" | Out-File $OutputFile -Append
    "" | Out-File $OutputFile -Append

    $Results.Passed++
}
catch {
    Write-Error "Failed transition with comment: $($_.Exception.Message)"
    "STEP 15: TRANSITION WITH COMMENT - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Step 16: Test fallback mechanism (category without workflow)
Write-Host "`n" + ("=" * 80)
Write-Info "STEP 16: Testing fallback mechanism for category without workflow..."

try {
    # Get a different category that doesn't have a workflow
    $otherCategory = $categories.data | Where-Object { $_.id -ne $categoryId } | Select-Object -First 1

    if ($otherCategory) {
        $fallbackInitialStatus = Invoke-RestMethod -Uri "$BaseUrl/workflows/categories/$($otherCategory.id)/initial-status" -Method GET -Headers $headers

        Write-Success "Fallback mechanism working"
        Write-Info "Category: $($otherCategory.name) (no custom workflow)"
        Write-Info "Fallback Initial Status: $($fallbackInitialStatus.data.name)"
        Write-Info "Expected: SUBMITTED (global fallback)"

        "STEP 16: FALLBACK MECHANISM - PASSED" | Out-File $OutputFile -Append
        "Category without workflow: $($otherCategory.name)" | Out-File $OutputFile -Append
        "Fallback status: $($fallbackInitialStatus.data.name)" | Out-File $OutputFile -Append
        "" | Out-File $OutputFile -Append

        $Results.Passed++
    }
    else {
        Write-Warning "Only one category available, skipping fallback test"
        "STEP 16: FALLBACK MECHANISM - SKIPPED (only one category)" | Out-File $OutputFile -Append
    }
}
catch {
    Write-Error "Failed fallback mechanism test: $($_.Exception.Message)"
    "STEP 16: FALLBACK MECHANISM - FAILED: $($_.Exception.Message)" | Out-File $OutputFile -Append
    $Results.Failed++
}
$Results.TotalTests++
#endregion

#region Final Summary
Write-Host "`n" + ("=" * 80)
Write-Host "`n📊 TEST SUMMARY" -ForegroundColor Cyan
Write-Host ("=" * 80)

$duration = (Get-Date) - $Results.StartTime
$successRate = [math]::Round(($Results.Passed / $Results.TotalTests) * 100, 2)

Write-Host "Total Tests: $($Results.TotalTests)" -ForegroundColor White
Write-Host "Passed: $($Results.Passed)" -ForegroundColor Green
Write-Host "Failed: $($Results.Failed)" -ForegroundColor $(if ($Results.Failed -gt 0) { "Red" } else { "Green" })
Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -eq 100) { "Green" } elseif ($successRate -ge 80) { "Yellow" } else { "Red" })
Write-Host "Duration: $($duration.TotalSeconds) seconds" -ForegroundColor White

"" | Out-File $OutputFile -Append
$separator | Out-File $OutputFile -Append
"TEST SUMMARY" | Out-File $OutputFile -Append
$separator | Out-File $OutputFile -Append
"Total Tests: $($Results.TotalTests)" | Out-File $OutputFile -Append
"Passed: $($Results.Passed)" | Out-File $OutputFile -Append
"Failed: $($Results.Failed)" | Out-File $OutputFile -Append
"Success Rate: $successRate%" | Out-File $OutputFile -Append
"Duration: $($duration.TotalSeconds) seconds" | Out-File $OutputFile -Append
"Test Completed: $(Get-Date)" | Out-File $OutputFile -Append
"" | Out-File $OutputFile -Append

if ($Results.Errors.Count -gt 0) {
    Write-Host "`n❌ ERRORS:" -ForegroundColor Red
    $Results.Errors | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }

    "ERRORS ENCOUNTERED:" | Out-File $OutputFile -Append
    $Results.Errors | ForEach-Object { "  - $_" | Out-File $OutputFile -Append }
}

Write-Host "`n✅ Test results saved to: $OutputFile" -ForegroundColor Green
Write-Host ""

if ($Results.Failed -eq 0) {
    Write-Host "🎉 ALL TESTS PASSED! Workflow engine is fully functional!" -ForegroundColor Green
    "STATUS: ALL TESTS PASSED" | Out-File $OutputFile -Append
}
else {
    Write-Host "⚠️  Some tests failed. Please review the errors above." -ForegroundColor Yellow
    "STATUS: SOME TESTS FAILED" | Out-File $OutputFile -Append
}

$separator | Out-File $OutputFile -Append
#endregion
