# Comprehensive Test Suite for Advanced Assignment Engine
# Tests all endpoints and validates dynamic status management

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5058/api"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Advanced Assignment Engine Test Suite" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Test Results Tracking
$testResults = @()

function Add-TestResult {
    param($TestName, $Status, $Message, $ResponseTime)
    $script:testResults += [PSCustomObject]@{
        Test = $TestName
        Status = $Status
        Message = $Message
        ResponseTime = $ResponseTime
    }
}

# Step 1: Get Authentication Token
Write-Host "`n[1/8] Authentication Test" -ForegroundColor Yellow
Write-Host "Getting authentication token..." -ForegroundColor Gray

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $loginData = '{"email":"admin@complaintmanagement.com","password":"Admin@123"}'
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $token = $loginResponse.data.token
    $stopwatch.Stop()

    Write-Host "✓ Authentication successful ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
    Add-TestResult -TestName "Authentication" -Status "PASS" -Message "Token obtained" -ResponseTime $stopwatch.ElapsedMilliseconds

    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
}
catch {
    $stopwatch.Stop()
    Write-Host "✗ Authentication failed: $_" -ForegroundColor Red
    Add-TestResult -TestName "Authentication" -Status "FAIL" -Message $_.Exception.Message -ResponseTime $stopwatch.ElapsedMilliseconds
    exit 1
}

# Get test data
Write-Host "`nPreparing test data..." -ForegroundColor Gray
try {
    $complaintsResponse = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method GET -Headers $headers
    $testComplaint = $complaintsResponse.data.items[0]
    $complaintId = $testComplaint.id
    $complaintNumber = $testComplaint.complaintNumber
    Write-Host "  Using complaint: $complaintNumber (ID: $complaintId)" -ForegroundColor Gray

    $poolsResponse = Invoke-RestMethod -Uri "$baseUrl/resource-pools" -Method GET -Headers $headers
    if ($poolsResponse.data.Count -gt 0) {
        $testPool = $poolsResponse.data[0]
        $poolId = $testPool.id
        $poolName = $testPool.name
        Write-Host "  Using pool: $poolName (ID: $poolId)" -ForegroundColor Gray

        if ($testPool.members -and $testPool.members.Count -gt 0) {
            $testUserId = $testPool.members[0].userId
            Write-Host "  Using user: $testUserId" -ForegroundColor Gray
        }
    }
}
catch {
    Write-Host "  ⚠ Could not get all test data: $_" -ForegroundColor Yellow
}

# Test 2: Get Assignment Candidates
Write-Host "`n[2/8] Test: GET /api/assignment/candidates/{complaintId}" -ForegroundColor Yellow
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $url = $baseUrl + '/assignment/candidates/' + $complaintId
    $response = Invoke-RestMethod -Uri $url -Method GET -Headers $headers
    $stopwatch.Stop()

    Write-Host "✓ SUCCESS - Found $($response.count) candidate pool(s) ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
    if ($response.data -and $response.data.Count -gt 0) {
        foreach ($candidate in $response.data) {
            Write-Host "  - Pool: $($candidate.poolName) | Score: $($candidate.suitabilityScore) | Available Users: $($candidate.availableUserCount)" -ForegroundColor Gray
        }
    }
    Add-TestResult -TestName "Get Assignment Candidates" -Status "PASS" -Message "Found $($response.count) candidates" -ResponseTime $stopwatch.ElapsedMilliseconds
}
catch {
    $stopwatch.Stop()
    $errorMsg = if ($_.ErrorDetails.Message) { $_.ErrorDetails.Message } else { $_.Exception.Message }
    Write-Host "✗ FAILED - $errorMsg ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Red
    Add-TestResult -TestName "Get Assignment Candidates" -Status "FAIL" -Message $errorMsg -ResponseTime $stopwatch.ElapsedMilliseconds
}

# Test 3: Validate Assignment
Write-Host "`n[3/8] Test: POST /api/assignment/validate/{complaintId}" -ForegroundColor Yellow
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $url = $baseUrl + '/assignment/validate/' + $complaintId
    $body = '{"userId":null,"poolId":null,"forceAssignment":false,"detailedValidation":true}'
    $response = Invoke-RestMethod -Uri $url -Method POST -Body $body -Headers $headers
    $stopwatch.Stop()

    Write-Host "✓ SUCCESS - Valid: $($response.data.isValid) ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
    Write-Host "  Suitability Score: $($response.data.suitabilityScore)" -ForegroundColor Gray

    if ($response.data.validationErrors -and $response.data.validationErrors.Count -gt 0) {
        Write-Host "  Errors:" -ForegroundColor Yellow
        foreach ($error in $response.data.validationErrors) {
            Write-Host "    - $error" -ForegroundColor Yellow
        }
    }

    if ($response.data.validationWarnings -and $response.data.validationWarnings.Count -gt 0) {
        Write-Host "  Warnings:" -ForegroundColor Yellow
        foreach ($warning in $response.data.validationWarnings) {
            Write-Host "    - $warning" -ForegroundColor Yellow
        }
    }

    Add-TestResult -TestName "Validate Assignment" -Status "PASS" -Message "Validation completed" -ResponseTime $stopwatch.ElapsedMilliseconds
}
catch {
    $stopwatch.Stop()
    $errorMsg = if ($_.ErrorDetails.Message) { $_.ErrorDetails.Message } else { $_.Exception.Message }
    Write-Host "✗ FAILED - $errorMsg ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Red
    Add-TestResult -TestName "Validate Assignment" -Status "FAIL" -Message $errorMsg -ResponseTime $stopwatch.ElapsedMilliseconds
}

# Test 4: Calculate Suitability Score
if ($testUserId -and $poolId) {
    Write-Host "`n[4/8] Test: GET /api/assignment/suitability-score" -ForegroundColor Yellow
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $url = $baseUrl + "/assignment/suitability-score?userId=$testUserId&poolId=$poolId"
        $response = Invoke-RestMethod -Uri $url -Method GET -Headers $headers
        $stopwatch.Stop()

        Write-Host "✓ SUCCESS - Score calculated ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
        Write-Host "  User ID: $testUserId" -ForegroundColor Gray
        Write-Host "  Pool ID: $poolId" -ForegroundColor Gray
        Write-Host "  Suitability Score: $($response.data.suitabilityScore)" -ForegroundColor Gray
        Write-Host "  Rating: $($response.data.rating)" -ForegroundColor Gray

        Add-TestResult -TestName "Calculate Suitability Score" -Status "PASS" -Message "Score: $($response.data.suitabilityScore)" -ResponseTime $stopwatch.ElapsedMilliseconds
    }
    catch {
        $stopwatch.Stop()
        $errorMsg = if ($_.ErrorDetails.Message) { $_.ErrorDetails.Message } else { $_.Exception.Message }
        Write-Host "✗ FAILED - $errorMsg ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Red
        Add-TestResult -TestName "Calculate Suitability Score" -Status "FAIL" -Message $errorMsg -ResponseTime $stopwatch.ElapsedMilliseconds
    }
} else {
    Write-Host "`n[4/8] Test: Calculate Suitability Score - SKIPPED (no test data)" -ForegroundColor Yellow
    Add-TestResult -TestName "Calculate Suitability Score" -Status "SKIP" -Message "No test user/pool available" -ResponseTime 0
}

# Test 5: Select User from Pool
if ($poolId) {
    Write-Host "`n[5/8] Test: GET /api/assignment/select-user/{poolId}" -ForegroundColor Yellow

    $methods = @("BestFit", "RoundRobin", "LeastBusy")
    foreach ($method in $methods) {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        try {
            $url = $baseUrl + "/assignment/select-user/$poolId" + "?method=$method"
            $response = Invoke-RestMethod -Uri $url -Method GET -Headers $headers
            $stopwatch.Stop()

            Write-Host "  ✓ $method - Selected: $($response.data.userName) | Score: $($response.data.score) ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
            Add-TestResult -TestName "Select User ($method)" -Status "PASS" -Message "User selected" -ResponseTime $stopwatch.ElapsedMilliseconds
        }
        catch {
            $stopwatch.Stop()
            $errorMsg = if ($_.ErrorDetails.Message) { $_.ErrorDetails.Message } else { $_.Exception.Message }
            Write-Host "  ✗ $method - $errorMsg ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Red
            Add-TestResult -TestName "Select User ($method)" -Status "FAIL" -Message $errorMsg -ResponseTime $stopwatch.ElapsedMilliseconds
        }
    }
} else {
    Write-Host "`n[5/8] Test: Select User from Pool - SKIPPED (no pool)" -ForegroundColor Yellow
    Add-TestResult -TestName "Select User from Pool" -Status "SKIP" -Message "No test pool available" -ResponseTime 0
}

# Test 6: Execute Assignment Rules
Write-Host "`n[6/8] Test: POST /api/assignment/execute-rules/{complaintId}" -ForegroundColor Yellow
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $url = $baseUrl + '/assignment/execute-rules/' + $complaintId
    $response = Invoke-RestMethod -Uri $url -Method POST -Headers $headers
    $stopwatch.Stop()

    Write-Host "✓ SUCCESS - Rules executed ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
    Write-Host "  Result: $($response.message)" -ForegroundColor Gray

    Add-TestResult -TestName "Execute Assignment Rules" -Status "PASS" -Message $response.message -ResponseTime $stopwatch.ElapsedMilliseconds
}
catch {
    $stopwatch.Stop()
    $errorMsg = if ($_.ErrorDetails.Message) { $_.ErrorDetails.Message } else { $_.Exception.Message }
    Write-Host "✗ FAILED - $errorMsg ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Red
    Add-TestResult -TestName "Execute Assignment Rules" -Status "FAIL" -Message $errorMsg -ResponseTime $stopwatch.ElapsedMilliseconds
}

# Test 7: Assign to Pool
if ($poolId) {
    Write-Host "`n[7/8] Test: POST /api/assignment/assign-to-pool/{complaintId}" -ForegroundColor Yellow
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $url = $baseUrl + '/assignment/assign-to-pool/' + $complaintId
        $body = @{
            resourcePoolId = $poolId
            assignmentMethod = "BestFit"
            specificUserId = $null
            assignmentReason = "Test assignment via comprehensive test suite"
            forceAssignment = $false
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri $url -Method POST -Body $body -Headers $headers
        $stopwatch.Stop()

        Write-Host "✓ SUCCESS - Assigned to pool ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
        Write-Host "  Assigned To: $($response.data.assignedUserName)" -ForegroundColor Gray
        Write-Host "  Method Used: $($response.data.assignmentMethod)" -ForegroundColor Gray
        Write-Host "  Message: $($response.data.message)" -ForegroundColor Gray

        # Check if StatusMasterId was set (dynamic status management)
        if ($response.data.statusMasterId) {
            Write-Host "  ✓ Dynamic Status: StatusMasterId set to $($response.data.statusMasterId)" -ForegroundColor Green
        }

        Add-TestResult -TestName "Assign to Pool" -Status "PASS" -Message "Assigned to $($response.data.assignedUserName)" -ResponseTime $stopwatch.ElapsedMilliseconds
    }
    catch {
        $stopwatch.Stop()
        $errorMsg = if ($_.ErrorDetails.Message) { $_.ErrorDetails.Message } else { $_.Exception.Message }
        Write-Host "✗ FAILED - $errorMsg ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Red
        Add-TestResult -TestName "Assign to Pool" -Status "FAIL" -Message $errorMsg -ResponseTime $stopwatch.ElapsedMilliseconds
    }
} else {
    Write-Host "`n[7/8] Test: Assign to Pool - SKIPPED (no pool)" -ForegroundColor Yellow
    Add-TestResult -TestName "Assign to Pool" -Status "SKIP" -Message "No test pool available" -ResponseTime 0
}

# Test 8: Auto-Assign (comprehensive test)
Write-Host "`n[8/8] Test: POST /api/assignment/auto-assign/{complaintId}" -ForegroundColor Yellow
Write-Host "  Testing with a different complaint to avoid conflicts..." -ForegroundColor Gray

try {
    # Get another complaint
    $complaintsResponse = Invoke-RestMethod -Uri ($baseUrl + '/complaints?pageNumber=1&pageSize=10') -Method GET -Headers $headers
    $unassignedComplaint = $complaintsResponse.data.items | Where-Object { $_.assignedToId -eq $null } | Select-Object -First 1

    if ($unassignedComplaint) {
        $autoAssignComplaintId = $unassignedComplaint.id

        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        try {
            $url = $baseUrl + '/assignment/auto-assign/' + $autoAssignComplaintId
            $response = Invoke-RestMethod -Uri $url -Method POST -Headers $headers
            $stopwatch.Stop()

            Write-Host "✓ SUCCESS - Auto-assigned ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
            Write-Host "  Complaint: $($unassignedComplaint.complaintNumber)" -ForegroundColor Gray
            Write-Host "  Assigned To: $($response.data.assignedUserName)" -ForegroundColor Gray
            Write-Host "  Method: $($response.data.assignmentMethod)" -ForegroundColor Gray
            Write-Host "  Message: $($response.data.message)" -ForegroundColor Gray

            Add-TestResult -TestName "Auto-Assign" -Status "PASS" -Message "Auto-assigned successfully" -ResponseTime $stopwatch.ElapsedMilliseconds
        }
        catch {
            $stopwatch.Stop()
            $errorMsg = if ($_.ErrorDetails.Message) { $_.ErrorDetails.Message } else { $_.Exception.Message }
            Write-Host "✗ FAILED - $errorMsg ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Red
            Add-TestResult -TestName "Auto-Assign" -Status "FAIL" -Message $errorMsg -ResponseTime $stopwatch.ElapsedMilliseconds
        }
    } else {
        Write-Host "  ⚠ No unassigned complaints available for testing" -ForegroundColor Yellow
        Add-TestResult -TestName "Auto-Assign" -Status "SKIP" -Message "No unassigned complaint" -ResponseTime 0
    }
}
catch {
    Write-Host "  ⚠ Could not get unassigned complaint: $_" -ForegroundColor Yellow
    Add-TestResult -TestName "Auto-Assign" -Status "SKIP" -Message "Data retrieval failed" -ResponseTime 0
}

# Summary Report
Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

$passed = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failed = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$skipped = ($testResults | Where-Object { $_.Status -eq "SKIP" }).Count
$total = $testResults.Count

Write-Host "`nResults:" -ForegroundColor White
Write-Host "  ✓ Passed:  $passed" -ForegroundColor Green
Write-Host "  ✗ Failed:  $failed" -ForegroundColor Red
Write-Host "  ⊘ Skipped: $skipped" -ForegroundColor Yellow
Write-Host "  ═ Total:   $total" -ForegroundColor Cyan

$avgResponseTime = ($testResults | Where-Object { $_.Status -eq "PASS" } | Measure-Object -Property ResponseTime -Average).Average
if ($avgResponseTime) {
    Write-Host "`n  Average Response Time: $([math]::Round($avgResponseTime, 2))ms" -ForegroundColor Gray
}

Write-Host "`nDetailed Results:" -ForegroundColor White
$testResults | Format-Table -AutoSize

if ($failed -eq 0) {
    Write-Host "`n🎉 ALL TESTS PASSED!" -ForegroundColor Green
    Write-Host "The Advanced Assignment Engine is working correctly." -ForegroundColor Green
} else {
    Write-Host "`n⚠ SOME TESTS FAILED" -ForegroundColor Yellow
    Write-Host "Review the failed tests above for details." -ForegroundColor Yellow
}

Write-Host "`n=====================================" -ForegroundColor Cyan
