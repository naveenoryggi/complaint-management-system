$token = Get-Content ".fresh-token"
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}
$baseUrl = "http://localhost:5058/api"

$passed = 0
$failed = 0

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "WORKFLOW MANAGEMENT E2E TEST SUITE" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# TEST 1: Categories (BUG #001)
Write-Host "[TEST 1] BUG #001: Categories Dropdown" -ForegroundColor Yellow
try {
    $catResponse = Invoke-RestMethod -Uri "$baseUrl/categories" -Method Get -Headers $headers
    if ($catResponse.isSuccess -and $catResponse.data.Count -gt 0) {
        Write-Host "  PASS: Found $($catResponse.data.Count) categories" -ForegroundColor Green
        $testCategory = $catResponse.data[0]
        $passed++
    } else {
        Write-Host "  FAIL: No categories" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
}

# TEST 2: Status Masters (BUG #002)
Write-Host "`n[TEST 2] BUG #002: Status Masters Dropdown" -ForegroundColor Yellow
try {
    $statusResponse = Invoke-RestMethod -Uri "$baseUrl/ComplaintStatusMaster" -Method Get -Headers $headers
    if ($statusResponse.isSuccess -and $statusResponse.data.Count -gt 0) {
        Write-Host "  PASS: Found $($statusResponse.data.Count) status masters" -ForegroundColor Green
        $testStatuses = $statusResponse.data | Select-Object -First 3
        $passed++
    } else {
        Write-Host "  FAIL: No status masters" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
}

# TEST 3: Get Workflows
Write-Host "`n[TEST 3] Get All Workflows" -ForegroundColor Yellow
try {
    $workflows = Invoke-RestMethod -Uri "$baseUrl/workflows" -Method Get -Headers $headers
    if ($workflows.isSuccess) {
        Write-Host "  PASS: Found $($workflows.data.Count) workflows" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "  FAIL: API returned error" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
}

# TEST 4: Create Workflow
Write-Host "`n[TEST 4] Create New Workflow" -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$createRequest = @{
    categoryId = $testCategory.id
    name = "E2E Test Workflow $timestamp"
    description = "Test workflow for E2E testing"
    isActive = $true
    isDefault = $true
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/workflows" -Method Post -Headers $headers -Body $createRequest
    if ($createResponse.isSuccess -and $createResponse.data.id) {
        Write-Host "  PASS: Workflow created with ID $($createResponse.data.id)" -ForegroundColor Green
        $newWorkflow = $createResponse.data
        $passed++
    } else {
        Write-Host "  FAIL: Could not create workflow" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
}

# TEST 5: Add Status to Workflow
if ($newWorkflow) {
    Write-Host "`n[TEST 5] Add Status to Workflow" -ForegroundColor Yellow
    $addStatusRequest = @{
        statusMasterId = $testStatuses[0].id
        displayOrder = 1
        isInitialStatus = $true
        defaultSLAHours = 24
        escalationHours = 48
        requiresApproval = $false
    } | ConvertTo-Json

    try {
        $statusResponse = Invoke-RestMethod -Uri "$baseUrl/workflows/$($newWorkflow.id)/statuses" -Method Post -Headers $headers -Body $addStatusRequest
        if ($statusResponse.isSuccess) {
            Write-Host "  PASS: Status added to workflow" -ForegroundColor Green
            $passed++
        } else {
            Write-Host "  FAIL: Could not add status" -ForegroundColor Red
            $failed++
        }
    } catch {
        Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
        $failed++
    }
}

# TEST 6: Get Workflow Details
if ($newWorkflow) {
    Write-Host "`n[TEST 6] Get Workflow Details" -ForegroundColor Yellow
    try {
        $detailResponse = Invoke-RestMethod -Uri "$baseUrl/workflows/$($newWorkflow.id)" -Method Get -Headers $headers
        if ($detailResponse.isSuccess) {
            Write-Host "  PASS: Retrieved workflow details" -ForegroundColor Green
            $passed++
        } else {
            Write-Host "  FAIL: Could not get details" -ForegroundColor Red
            $failed++
        }
    } catch {
        Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
        $failed++
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TEST RESULTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor Red
$total = $passed + $failed
$rate = [math]::Round(($passed / $total) * 100, 2)
Write-Host "Success Rate: $rate%" -ForegroundColor $(if ($rate -ge 90) { "Green" } else { "Yellow" })
Write-Host ""

if ($passed -eq $total) {
    Write-Host "ALL TESTS PASSED!" -ForegroundColor Green
} else {
    Write-Host "SOME TESTS FAILED" -ForegroundColor Yellow
}
