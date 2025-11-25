# Simple Workflow Engine API Test
# Tests core workflow functionality

$BaseUrl = "http://localhost:5058/api"
$OutputFile = "WORKFLOW_TEST_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"

$Results = @{
    Total = 0
    Passed = 0
    Failed = 0
}

Write-Host "`n=== WORKFLOW ENGINE API TEST ===" -ForegroundColor Cyan
Write-Host "Output: $OutputFile`n"

# Auth
Write-Host "1. Authenticating..." -ForegroundColor Yellow
try {
    $auth = @{ email = "admin@complaintmanagement.com"; password = "Admin@123" } | ConvertTo-Json
    $login = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method POST -Body $auth -ContentType "application/json"
    $token = $login.data.token
    $userId = $login.data.id
    $companyId = $login.data.companyId
    $headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }
    Write-Host "   PASSED - Token obtained" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
    exit 1
}
$Results.Total++

# Get categories
Write-Host "2. Getting categories..." -ForegroundColor Yellow
try {
    $categories = Invoke-RestMethod -Uri "$BaseUrl/categories" -Method GET -Headers $headers
    $testCategory = $categories.data[0]
    $categoryId = $testCategory.id
    Write-Host "   PASSED - Found category: $($testCategory.name)" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
    exit 1
}
$Results.Total++

# Get status masters
Write-Host "3. Getting status masters..." -ForegroundColor Yellow
try {
    $statuses = Invoke-RestMethod -Uri "$BaseUrl/ComplaintStatusMaster" -Method GET -Headers $headers
    $submittedStatus = $statuses.data | Where-Object { $_.code -eq "SUBMITTED" } | Select -First 1
    $inProgressStatus = $statuses.data | Where-Object { $_.code -eq "IN_PROGRESS" } | Select -First 1
    $resolvedStatus = $statuses.data | Where-Object { $_.code -eq "RESOLVED" } | Select -First 1
    $closedStatus = $statuses.data | Where-Object { $_.code -eq "CLOSED" } | Select -First 1
    Write-Host "   PASSED - Found $($statuses.data.Count) statuses" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
    exit 1
}
$Results.Total++

# Get all workflows
Write-Host "4. GET /api/workflows..." -ForegroundColor Yellow
try {
    $workflows = Invoke-RestMethod -Uri "$BaseUrl/workflows?companyId=$companyId" -Method GET -Headers $headers
    Write-Host "   PASSED - Found $($workflows.data.Count) workflows" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Create workflow
Write-Host "5. POST /api/workflows (Create)..." -ForegroundColor Yellow
try {
    $createBody = @{
        categoryId = $categoryId
        name = "Test Workflow $(Get-Date -Format 'HHmmss')"
        description = "Automated test workflow"
        isActive = $true
        isDefault = $true
        companyId = $companyId
    } | ConvertTo-Json

    $workflow = Invoke-RestMethod -Uri "$BaseUrl/workflows" -Method POST -Body $createBody -Headers $headers
    $workflowId = $workflow.data.id
    Write-Host "   PASSED - Created workflow: $workflowId" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Get workflow for category
Write-Host "6. GET /api/workflows/category/{id}..." -ForegroundColor Yellow
try {
    $categoryWorkflow = Invoke-RestMethod -Uri "$BaseUrl/workflows/category/$categoryId" -Method GET -Headers $headers
    Write-Host "   PASSED - Retrieved workflow for category" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Add statuses
Write-Host "7. POST /api/workflows/{id}/statuses..." -ForegroundColor Yellow
try {
    $status1 = @{
        workflowId = $workflowId
        statusMasterId = $submittedStatus.id
        displayOrder = 1
        isInitialStatus = $true
        defaultSLAHours = 4
    } | ConvertTo-Json
    Invoke-RestMethod -Uri "$BaseUrl/workflows/$workflowId/statuses" -Method POST -Body $status1 -Headers $headers | Out-Null

    $status2 = @{
        workflowId = $workflowId
        statusMasterId = $inProgressStatus.id
        displayOrder = 2
        isInitialStatus = $false
        defaultSLAHours = 24
    } | ConvertTo-Json
    Invoke-RestMethod -Uri "$BaseUrl/workflows/$workflowId/statuses" -Method POST -Body $status2 -Headers $headers | Out-Null

    Write-Host "   PASSED - Added 2 statuses" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Add transitions
Write-Host "8. POST /api/workflows/{id}/transitions..." -ForegroundColor Yellow
try {
    $transition = @{
        workflowId = $workflowId
        fromStatusId = $submittedStatus.id
        toStatusId = $inProgressStatus.id
        transitionName = "Start Work"
        requiresComment = $false
        requiresApproval = $false
    } | ConvertTo-Json
    Invoke-RestMethod -Uri "$BaseUrl/workflows/$workflowId/transitions" -Method POST -Body $transition -Headers $headers | Out-Null
    Write-Host "   PASSED - Added transition" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Get workflow statuses
Write-Host "9. GET /api/workflows/categories/{id}/statuses..." -ForegroundColor Yellow
try {
    $wfStatuses = Invoke-RestMethod -Uri "$BaseUrl/workflows/categories/$categoryId/statuses" -Method GET -Headers $headers
    Write-Host "   PASSED - Retrieved $($wfStatuses.data.Count) statuses" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Get initial status
Write-Host "10. GET /api/workflows/categories/{id}/initial-status..." -ForegroundColor Yellow
try {
    $initialStatus = Invoke-RestMethod -Uri "$BaseUrl/workflows/categories/$categoryId/initial-status" -Method GET -Headers $headers
    Write-Host "   PASSED - Initial status: $($initialStatus.data.name)" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Get allowed transitions
Write-Host "11. GET /api/workflows/allowed-transitions..." -ForegroundColor Yellow
try {
    $allowed = Invoke-RestMethod -Uri "$BaseUrl/workflows/allowed-transitions?categoryId=$categoryId&currentStatusId=$($submittedStatus.id)" -Method GET -Headers $headers
    Write-Host "   PASSED - Found $($allowed.data.transitions.Count) allowed transitions" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Check transition
Write-Host "12. POST /api/workflows/check-transition..." -ForegroundColor Yellow
try {
    $check = @{
        categoryId = $categoryId
        fromStatusId = $submittedStatus.id
        toStatusId = $inProgressStatus.id
        userId = $userId
    } | ConvertTo-Json
    $checkResult = Invoke-RestMethod -Uri "$BaseUrl/workflows/check-transition" -Method POST -Body $check -Headers $headers
    Write-Host "   PASSED - Transition allowed: $($checkResult.data.isAllowed)" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Create test complaint
Write-Host "13. Creating test complaint..." -ForegroundColor Yellow
try {
    $complaint = @{
        title = "Test Complaint - Workflow Test"
        description = "Testing workflow engine integration"
        categoryId = $categoryId
        complainantId = $userId
        companyId = $companyId
        priority = "Medium"
        isAnonymous = $false
    } | ConvertTo-Json
    $complaintResponse = Invoke-RestMethod -Uri "$BaseUrl/complaints" -Method POST -Body $complaint -Headers $headers
    $complaintId = $complaintResponse.data.id
    Write-Host "   PASSED - Created complaint: $($complaintResponse.data.complaintNumber)" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Transition complaint
Write-Host "14. POST /api/workflows/complaints/{id}/transition..." -ForegroundColor Yellow
try {
    $transitionBody = @{
        newStatusId = $inProgressStatus.id
        comment = "Starting work - test"
    } | ConvertTo-Json
    $transitionResult = Invoke-RestMethod -Uri "$BaseUrl/workflows/complaints/$complaintId/transition" -Method POST -Body $transitionBody -Headers $headers
    Write-Host "   PASSED - Transitioned complaint" -ForegroundColor Green
    $Results.Passed++
} catch {
    Write-Host "   FAILED - $($_.Exception.Message)" -ForegroundColor Red
    $Results.Failed++
}
$Results.Total++

# Summary
$successRate = [math]::Round(($Results.Passed / $Results.Total) * 100, 2)
Write-Host "`n=== TEST SUMMARY ===" -ForegroundColor Cyan
Write-Host "Total:  $($Results.Total)" -ForegroundColor White
Write-Host "Passed: $($Results.Passed)" -ForegroundColor Green
Write-Host "Failed: $($Results.Failed)" -ForegroundColor $(if ($Results.Failed -gt 0) { "Red" } else { "Green" })
Write-Host "Rate:   $successRate%" -ForegroundColor $(if ($successRate -eq 100) { "Green" } else { "Yellow" })

if ($Results.Failed -eq 0) {
    Write-Host "`nALL TESTS PASSED! Workflow engine is functional!" -ForegroundColor Green
} else {
    Write-Host "`nSome tests failed. Review errors above." -ForegroundColor Yellow
}

Write-Host ""
