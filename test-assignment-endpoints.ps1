# Test Advanced Assignment Engine Endpoints
# This script tests the newly implemented assignment endpoints

$baseUrl = "http://localhost:5058/api"

# Get token
Write-Host "=== Getting Authentication Token ===" -ForegroundColor Cyan
$loginData = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "✓ Token obtained successfully" -ForegroundColor Green

    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
} catch {
    Write-Host "✗ Failed to get token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Get a complaint to test with
Write-Host "`n=== Getting a Test Complaint ===" -ForegroundColor Cyan
try {
    $complaintsResponse = Invoke-RestMethod -Uri ($baseUrl + '/complaints?pageNumber=1&pageSize=1') -Method GET -Headers $headers

    if ($complaintsResponse.data.items.Count -gt 0) {
        $testComplaint = $complaintsResponse.data.items[0]
        $complaintId = $testComplaint.id
        Write-Host "✓ Using complaint: $($testComplaint.complaintNumber) (ID: $complaintId)" -ForegroundColor Green
        Write-Host "  Status: $($testComplaint.status)" -ForegroundColor Gray
        Write-Host "  Category: $($testComplaint.category.name)" -ForegroundColor Gray
    } else {
        Write-Host "✗ No complaints found to test with" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Failed to get complaints: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 1: Get Assignment Candidates
Write-Host "`n=== Test 1: Get Assignment Candidates ===" -ForegroundColor Cyan
try {
    $candidatesResponse = Invoke-RestMethod -Uri "$baseUrl/assignment/candidates/$complaintId" -Method GET -Headers $headers
    Write-Host "✓ Candidates retrieved successfully" -ForegroundColor Green
    Write-Host "  Found $($candidatesResponse.count) candidate pool(s)" -ForegroundColor Gray

    if ($candidatesResponse.data -and $candidatesResponse.data.Count -gt 0) {
        foreach ($candidate in $candidatesResponse.data) {
            Write-Host "  - Pool: $($candidate.poolName) (Score: $($candidate.suitabilityScore))" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "✗ Failed to get candidates: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Response: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

# Test 2: Calculate Suitability Score (if we have a pool)
Write-Host "`n=== Test 2: Calculate Suitability Score ===" -ForegroundColor Cyan
try {
    # Get a resource pool first
    $poolsResponse = Invoke-RestMethod -Uri "$baseUrl/resource-pools" -Method GET -Headers $headers

    if ($poolsResponse.data.Count -gt 0) {
        $testPool = $poolsResponse.data[0]
        $poolId = $testPool.id

        # Get a user from the pool
        if ($testPool.members -and $testPool.members.Count -gt 0) {
            $testUser = $testPool.members[0].userId

            $scoreResponse = Invoke-RestMethod -Uri ($baseUrl + "/assignment/suitability-score?userId=$testUser&poolId=$poolId") -Method GET -Headers $headers
            Write-Host "✓ Suitability score calculated" -ForegroundColor Green
            Write-Host "  User: $testUser" -ForegroundColor Gray
            Write-Host "  Pool: $($testPool.name)" -ForegroundColor Gray
            Write-Host "  Score: $($scoreResponse.data.suitabilityScore) ($($scoreResponse.data.rating))" -ForegroundColor Gray
        } else {
            Write-Host "⚠ Pool has no members to test with" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠ No resource pools found to test with" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Failed to calculate suitability score: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Response: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

# Test 3: Validate Assignment
Write-Host "`n=== Test 3: Validate Assignment ===" -ForegroundColor Cyan
try {
    $validateData = @{
        userId = $null
        poolId = $null
        forceAssignment = $false
        detailedValidation = $true
    } | ConvertTo-Json

    $validateResponse = Invoke-RestMethod -Uri "$baseUrl/assignment/validate/$complaintId" -Method POST -Body $validateData -Headers $headers
    Write-Host "✓ Assignment validation completed" -ForegroundColor Green
    Write-Host "  Valid: $($validateResponse.data.isValid)" -ForegroundColor Gray

    if ($validateResponse.data.validationErrors -and $validateResponse.data.validationErrors.Count -gt 0) {
        Write-Host "  Errors:" -ForegroundColor Yellow
        foreach ($error in $validateResponse.data.validationErrors) {
            Write-Host "    - $error" -ForegroundColor Yellow
        }
    }

    if ($validateResponse.data.validationWarnings -and $validateResponse.data.validationWarnings.Count -gt 0) {
        Write-Host "  Warnings:" -ForegroundColor Yellow
        foreach ($warning in $validateResponse.data.validationWarnings) {
            Write-Host "    - $warning" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "✗ Failed to validate assignment: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Response: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

# Test 4: Select User from Pool (Preview)
Write-Host "`n=== Test 4: Select User from Pool (Preview) ===" -ForegroundColor Cyan
try {
    $poolsResponse = Invoke-RestMethod -Uri "$baseUrl/resource-pools" -Method GET -Headers $headers

    if ($poolsResponse.data.Count -gt 0) {
        $testPool = $poolsResponse.data[0]
        $poolId = $testPool.id

        $selectResponse = Invoke-RestMethod -Uri ($baseUrl + "/assignment/select-user/$poolId" + '?method=BestFit') -Method GET -Headers $headers
        Write-Host "✓ User selected from pool" -ForegroundColor Green
        Write-Host "  Pool: $($testPool.name)" -ForegroundColor Gray
        Write-Host "  Selected User: $($selectResponse.data.userName)" -ForegroundColor Gray
        Write-Host "  Score: $($selectResponse.data.score)" -ForegroundColor Gray
    } else {
        Write-Host "⚠ No resource pools found to test with" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Failed to select user: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Response: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

Write-Host "`n=== Test Summary ===" -ForegroundColor Cyan
Write-Host "Advanced Assignment Engine endpoints tested." -ForegroundColor Green
Write-Host "Note: Auto-assignment and assign-to-pool endpoints require proper setup (resource pools, members, etc.)" -ForegroundColor Yellow
