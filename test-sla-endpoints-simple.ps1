# Test SLA Display Endpoints
$baseUrl = "http://localhost:5000"
$email = "admin@complaintmanagement.com"
$password = "Admin@123"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  SLA Display Endpoints Test" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Login
Write-Host "[1/7] Logging in..." -ForegroundColor Yellow
$loginPayload = @{
    email = $email
    password = $password
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" `
        -Method Post `
        -ContentType "application/json" `
        -Body $loginPayload

    $token = $loginResponse.data.token
    Write-Host "  SUCCESS: Token obtained" -ForegroundColor Green
} catch {
    Write-Host "  FAILED: Login error - $_" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get test complaint
Write-Host "`n[2/7] Getting test complaint..." -ForegroundColor Yellow
try {
    $complaintsResponse = Invoke-RestMethod -Uri "$baseUrl/api/complaints?pageSize=5" `
        -Method Get `
        -Headers $headers

    $testComplaintId = $complaintsResponse.data.items[0].id
    $testComplaintNumber = $complaintsResponse.data.items[0].complaintNumber
    $complaintIds = $complaintsResponse.data.items | Select-Object -First 5 | ForEach-Object { $_.id }

    Write-Host "  SUCCESS: Using $testComplaintNumber (ID: $testComplaintId)" -ForegroundColor Green
} catch {
    Write-Host "  FAILED: $_" -ForegroundColor Red
    exit 1
}

# Test Results
$passed = 0
$failed = 0

# Test 1: GET /api/sla/status/{complaintId}
Write-Host "`n[3/7] Testing GET /api/sla/status/{complaintId}..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/sla/status/$testComplaintId" `
        -Method Get `
        -Headers $headers

    if ($response.isSuccess) {
        Write-Host "  SUCCESS: SLA status retrieved" -ForegroundColor Green
        Write-Host "    - Status: $($response.data.status)" -ForegroundColor Gray
        Write-Host "    - SLA Level: $($response.data.slaLevelName)" -ForegroundColor Gray
        Write-Host "    - Progress: $([math]::Round($response.data.resolutionProgress, 1))%" -ForegroundColor Gray
        $passed++
    } else {
        Write-Host "  FAILED: $($response.message)" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "  FAILED: $_" -ForegroundColor Red
    $failed++
}

# Test 2: POST /api/sla/status/bulk
Write-Host "`n[4/7] Testing POST /api/sla/status/bulk..." -ForegroundColor Yellow
$bulkPayload = @{
    complaintIds = $complaintIds
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/sla/status/bulk" `
        -Method Post `
        -Headers $headers `
        -Body $bulkPayload

    if ($response.isSuccess) {
        Write-Host "  SUCCESS: Bulk SLA status retrieved" -ForegroundColor Green
        Write-Host "    - Total: $($response.data.totalCount)" -ForegroundColor Gray
        Write-Host "    - Success: $($response.data.successCount)" -ForegroundColor Gray
        Write-Host "    - Failed: $($response.data.failedCount)" -ForegroundColor Gray
        $passed++
    } else {
        Write-Host "  FAILED: $($response.message)" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "  FAILED: $_" -ForegroundColor Red
    $failed++
}

# Test 3: POST /api/sla/applicable
Write-Host "`n[5/7] Testing POST /api/sla/applicable..." -ForegroundColor Yellow
try {
    $categoriesResponse = Invoke-RestMethod -Uri "$baseUrl/api/categories?pageSize=1" `
        -Method Get `
        -Headers $headers
    $categoryId = $categoriesResponse.data.items[0].id

    $applicablePayload = @{
        categoryId = $categoryId
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/api/sla/applicable" `
        -Method Post `
        -Headers $headers `
        -Body $applicablePayload

    if ($response.isSuccess) {
        Write-Host "  SUCCESS: Applicable SLA retrieved" -ForegroundColor Green
        Write-Host "    - SLA Level: $($response.data.slaLevelName)" -ForegroundColor Gray
        Write-Host "    - Source: $($response.data.source)" -ForegroundColor Gray
        $passed++
    } else {
        Write-Host "  FAILED: $($response.message)" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "  FAILED: $_" -ForegroundColor Red
    $failed++
}

# Test 4: GET /api/sla/timeline/{complaintId}
Write-Host "`n[6/7] Testing GET /api/sla/timeline/{complaintId}..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/sla/timeline/$testComplaintId" `
        -Method Get `
        -Headers $headers

    if ($response.isSuccess) {
        Write-Host "  SUCCESS: Timeline retrieved" -ForegroundColor Green
        Write-Host "    - Events: $($response.data.events.Count)" -ForegroundColor Gray
        $passed++
    } else {
        Write-Host "  FAILED: $($response.message)" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "  FAILED: $_" -ForegroundColor Red
    $failed++
}

# Test 5: GET /api/sla/coverage-matrix
Write-Host "`n[7/7] Testing GET /api/sla/coverage-matrix..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/sla/coverage-matrix" `
        -Method Get `
        -Headers $headers

    if ($response.isSuccess) {
        Write-Host "  SUCCESS: Coverage matrix retrieved" -ForegroundColor Green
        Write-Host "    - Total Categories: $($response.data.totalCategories)" -ForegroundColor Gray
        Write-Host "    - Coverage: $([math]::Round($response.data.coveragePercentage, 1))%" -ForegroundColor Gray
        $passed++
    } else {
        Write-Host "  FAILED: $($response.message)" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "  FAILED: $_" -ForegroundColor Red
    $failed++
}

# Test 6: GET /api/sla/warnings
Write-Host "`n[BONUS] Testing GET /api/sla/warnings..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/sla/warnings" `
        -Method Get `
        -Headers $headers

    if ($response.isSuccess) {
        Write-Host "  SUCCESS: Warnings retrieved" -ForegroundColor Green
        Write-Host "    - Total: $($response.data.totalWarnings)" -ForegroundColor Gray
        Write-Host "    - Breached: $($response.data.breachedCount)" -ForegroundColor Gray
        Write-Host "    - Critical: $($response.data.criticalCount)" -ForegroundColor Gray
        $passed++
    } else {
        Write-Host "  FAILED: $($response.message)" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "  FAILED: $_" -ForegroundColor Red
    $failed++
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Passed: $passed / 6" -ForegroundColor $(if ($passed -eq 6) { "Green" } else { "Yellow" })
Write-Host "Failed: $failed / 6" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
$successRate = [math]::Round(($passed / 6) * 100, 1)
Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($passed -eq 6) { "Green" } else { "Yellow" })
Write-Host "========================================`n" -ForegroundColor Cyan

if ($failed -eq 0) {
    Write-Host "ALL TESTS PASSED!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Some tests failed. Review details above." -ForegroundColor Red
    exit 1
}
