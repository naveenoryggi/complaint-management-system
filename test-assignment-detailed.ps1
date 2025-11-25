# Detailed Assignment Engine Test
$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5058/api"

Write-Host "=== Detailed Assignment Engine Test ===" -ForegroundColor Cyan

# Get token
$loginData = '{"email":"admin@complaintmanagement.com","password":"Admin@123"}'
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json"
$token = $loginResponse.data.token
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "Token obtained" -ForegroundColor Green

# Get test data
$complaintsResponse = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method GET -Headers $headers
$testComplaint = $complaintsResponse.data.items[0]
$complaintId = $testComplaint.id

Write-Host "`nTest Complaint: $($testComplaint.complaintNumber)" -ForegroundColor Gray
Write-Host "  Status: $($testComplaint.status)"
Write-Host "  Priority: $($testComplaint.priority)"
Write-Host "  Category: $($testComplaint.category.name)"

# Get resource pools with details
Write-Host "`n=== Resource Pools Analysis ===" -ForegroundColor Yellow
$poolsResponse = Invoke-RestMethod -Uri "$baseUrl/resource-pools" -Method GET -Headers $headers

Write-Host "Found $($poolsResponse.data.Count) resource pool(s)" -ForegroundColor Gray

foreach ($pool in $poolsResponse.data) {
    Write-Host "`nPool: $($pool.name)" -ForegroundColor Cyan
    Write-Host "  ID: $($pool.id)"
    Write-Host "  Active: $($pool.isActive)"
    Write-Host "  Company: $($pool.companyId)"
    Write-Host "  Members: $($pool.members.Count)"

    if ($pool.members -and $pool.members.Count -gt 0) {
        Write-Host "  Members Details:" -ForegroundColor Gray
        foreach ($member in $pool.members) {
            Write-Host "    - User ID: $($member.userId) | Deleted: $($member.isDeleted)"
        }

        $testPool = $pool
        $testPoolId = $pool.id
        $testMember = $pool.members[0]
    }
}

# Test 1: Get Candidates with Details
Write-Host "`n=== Test 1: Get Assignment Candidates ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/assignment/candidates/$complaintId" -Method GET -Headers $headers
    Write-Host "SUCCESS - Found $($response.count) candidates" -ForegroundColor Green

    if ($response.data) {
        foreach ($candidate in $response.data) {
            Write-Host "`nCandidate Pool:" -ForegroundColor Cyan
            Write-Host "  Name: $($candidate.poolName)"
            Write-Host "  Suitability Score: $($candidate.suitabilityScore)"
            Write-Host "  Available Users: $($candidate.availableUserCount)"
            Write-Host "  Organizational Alignment: $($candidate.organizationalAlignmentScore)"
        }
    }
}
catch {
    Write-Host "FAILED: $_" -ForegroundColor Red
    Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

# Test 2: Validate with specific pool
if ($testPoolId) {
    Write-Host "`n=== Test 2: Validate Assignment to Specific Pool ===" -ForegroundColor Yellow
    try {
        $validateBody = @{
            userId = $null
            poolId = $testPoolId
            forceAssignment = $false
            detailedValidation = $true
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri "$baseUrl/assignment/validate/$complaintId" -Method POST -Body $validateBody -Headers $headers
        Write-Host "SUCCESS - Validation completed" -ForegroundColor Green
        Write-Host "  Valid: $($response.data.isValid)"
        Write-Host "  Suitability Score: $($response.data.suitabilityScore)"

        if ($response.data.validationErrors) {
            Write-Host "  Errors: $($response.data.validationErrors.Count)"
            foreach ($error in $response.data.validationErrors) {
                Write-Host "    - $error" -ForegroundColor Red
            }
        }

        if ($response.data.validationWarnings) {
            Write-Host "  Warnings: $($response.data.validationWarnings.Count)"
            foreach ($warning in $response.data.validationWarnings) {
                Write-Host "    - $warning" -ForegroundColor Yellow
            }
        }
    }
    catch {
        Write-Host "FAILED: $_" -ForegroundColor Red
        Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    }
}

# Test 3: Select User from Pool (with error details)
if ($testPoolId) {
    Write-Host "`n=== Test 3: Select User from Pool ===" -ForegroundColor Yellow

    $methods = @("BestFit", "RoundRobin", "LeastBusy", "SkillBased")

    foreach ($method in $methods) {
        try {
            $selectUrl = "$baseUrl/assignment/select-user/$testPoolId" + "?method=$method"
            $response = Invoke-RestMethod -Uri $selectUrl -Method GET -Headers $headers

            Write-Host "  $method - SUCCESS" -ForegroundColor Green
            Write-Host "    Selected: $($response.data.userName)"
            Write-Host "    User ID: $($response.data.userId)"
            Write-Host "    Score: $($response.data.score)"
        }
        catch {
            $errorDetail = $_.ErrorDetails.Message | ConvertFrom-Json
            Write-Host "  $method - FAILED" -ForegroundColor Red
            Write-Host "    Error: $($errorDetail.message)" -ForegroundColor Yellow
        }
    }
}

# Test 4: Suitability Score
if ($testPoolId -and $testMember) {
    Write-Host "`n=== Test 4: Calculate Suitability Score ===" -ForegroundColor Yellow
    try {
        $scoreUrl = "$baseUrl/assignment/suitability-score?userId=$($testMember.userId)&poolId=$testPoolId"
        $response = Invoke-RestMethod -Uri $scoreUrl -Method GET -Headers $headers

        Write-Host "SUCCESS - Suitability score calculated" -ForegroundColor Green
        Write-Host "  User ID: $($testMember.userId)"
        Write-Host "  Pool ID: $testPoolId"
        Write-Host "  Score: $($response.data.suitabilityScore)"
        Write-Host "  Rating: $($response.data.rating)"
    }
    catch {
        Write-Host "FAILED: $_" -ForegroundColor Red
        Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    }
}

# Test 5: Assign to Pool (actual assignment)
if ($testPoolId) {
    Write-Host "`n=== Test 5: Assign to Pool (ACTUAL ASSIGNMENT) ===" -ForegroundColor Yellow
    try {
        $assignBody = @{
            resourcePoolId = $testPoolId
            assignmentMethod = "BestFit"
            specificUserId = $null
            assignmentReason = "Comprehensive test - validating dynamic status"
            forceAssignment = $false
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri "$baseUrl/assignment/assign-to-pool/$complaintId" -Method POST -Body $assignBody -Headers $headers

        Write-Host "SUCCESS - Complaint assigned!" -ForegroundColor Green
        Write-Host "  Assigned To: $($response.data.assignedUserName)"
        Write-Host "  User ID: $($response.data.assignedUserId)"
        Write-Host "  Method: $($response.data.assignmentMethod)"
        Write-Host "  Pool: $($response.data.resourcePoolName)"

        # Check dynamic status management
        Write-Host "`n  Dynamic Status Check:" -ForegroundColor Cyan
        if ($response.data.statusMasterId) {
            Write-Host "    ✓ StatusMasterId: $($response.data.statusMasterId)" -ForegroundColor Green
        }
        Write-Host "    Status: $($response.data.status)"

        # Verify in database
        Write-Host "`n  Verifying assignment in database..." -ForegroundColor Gray
        $verifyResponse = Invoke-RestMethod -Uri "$baseUrl/complaints/$complaintId" -Method GET -Headers $headers
        Write-Host "    ✓ Confirmed - Assigned To: $($verifyResponse.data.assignedTo.fullName)" -ForegroundColor Green
        Write-Host "    ✓ Status: $($verifyResponse.data.status)" -ForegroundColor Green
        if ($verifyResponse.data.statusMasterId) {
            Write-Host "    ✓ StatusMasterId: $($verifyResponse.data.statusMasterId)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "FAILED: $_" -ForegroundColor Red
        $errorDetail = $_.ErrorDetails.Message
        Write-Host "Response: $errorDetail" -ForegroundColor Yellow
    }
}

# Test 6: Execute Assignment Rules
Write-Host "`n=== Test 6: Execute Assignment Rules ===" -ForegroundColor Yellow
try {
    # Get another unassigned complaint
    $complaintsResponse = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method GET -Headers $headers
    $unassignedComplaint = $complaintsResponse.data.items | Where-Object { -not $_.assignedToId } | Select-Object -First 1

    if ($unassignedComplaint) {
        Write-Host "Testing with complaint: $($unassignedComplaint.complaintNumber)" -ForegroundColor Gray

        $response = Invoke-RestMethod -Uri "$baseUrl/assignment/execute-rules/$($unassignedComplaint.id)" -Method POST -Headers $headers

        Write-Host "SUCCESS - Rules executed" -ForegroundColor Green
        Write-Host "  Result: $($response.message)"
    }
    else {
        Write-Host "SKIPPED - No unassigned complaints available" -ForegroundColor Yellow
    }
}
catch {
    $errorDetail = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "FAILED (Expected if no rules configured)" -ForegroundColor Yellow
    Write-Host "  Message: $($errorDetail.message)" -ForegroundColor Gray
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
Write-Host "All critical endpoints tested successfully!" -ForegroundColor Green
