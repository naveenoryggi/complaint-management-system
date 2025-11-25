# Simple Test for Assignment Endpoints
$ErrorActionPreference = "Continue"

$baseUrl = "http://localhost:5058/api"

# Get token
Write-Host "Getting token..." -ForegroundColor Cyan
$loginData = '{"email":"admin@complaintmanagement.com","password":"Admin@123"}'
try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "Token obtained" -ForegroundColor Green
}
catch {
    Write-Host "Failed to get token: $_" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get a complaint
Write-Host "`nGetting test complaint..." -ForegroundColor Cyan
try {
    $url = $baseUrl + '/complaints'
    $complaintsResponse = Invoke-RestMethod -Uri $url -Method GET -Headers $headers

    if ($complaintsResponse.data.items.Count -gt 0) {
        $complaintId = $complaintsResponse.data.items[0].id
        $complaintNumber = $complaintsResponse.data.items[0].complaintNumber
        Write-Host "Using complaint: $complaintNumber" -ForegroundColor Green
    }
    else {
        Write-Host "No complaints found" -ForegroundColor Yellow
        exit 0
    }
}
catch {
    Write-Host "Failed to get complaints: $_" -ForegroundColor Red
}

# Test 1: Get Candidates
Write-Host "`nTest 1: Get Assignment Candidates" -ForegroundColor Cyan
try {
    $url = $baseUrl + '/assignment/candidates/' + $complaintId
    $response = Invoke-RestMethod -Uri $url -Method GET -Headers $headers
    Write-Host "SUCCESS - Found $($response.count) candidates" -ForegroundColor Green
}
catch {
    $errorMsg = $_.ErrorDetails.Message
    Write-Host "FAILED - $errorMsg" -ForegroundColor Red
}

# Test 2: Validate Assignment
Write-Host "`nTest 2: Validate Assignment" -ForegroundColor Cyan
try {
    $url = $baseUrl + '/assignment/validate/' + $complaintId
    $body = '{"userId":null,"poolId":null,"forceAssignment":false,"detailedValidation":true}'
    $response = Invoke-RestMethod -Uri $url -Method POST -Body $body -Headers $headers
    Write-Host "SUCCESS - Valid: $($response.data.isValid)" -ForegroundColor Green
}
catch {
    $errorMsg = $_.ErrorDetails.Message
    Write-Host "FAILED - $errorMsg" -ForegroundColor Red
}

# Test 3: Get Resource Pools
Write-Host "`nTest 3: Get Resource Pools" -ForegroundColor Cyan
try {
    $url = $baseUrl + '/resource-pools'
    $poolsResponse = Invoke-RestMethod -Uri $url -Method GET -Headers $headers

    if ($poolsResponse.data.Count -gt 0) {
        $poolId = $poolsResponse.data[0].id
        $poolName = $poolsResponse.data[0].name
        Write-Host "SUCCESS - Found pool: $poolName" -ForegroundColor Green

        # Test 4: Select User from Pool
        Write-Host "`nTest 4: Select User from Pool" -ForegroundColor Cyan
        try {
            $url = $baseUrl + '/assignment/select-user/' + $poolId + '?method=BestFit'
            $response = Invoke-RestMethod -Uri $url -Method GET -Headers $headers
            Write-Host "SUCCESS - Selected: $($response.data.userName)" -ForegroundColor Green
        }
        catch {
            $errorMsg = $_.ErrorDetails.Message
            Write-Host "FAILED - $errorMsg" -ForegroundColor Red
        }
    }
    else {
        Write-Host "No resource pools found" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "Failed to get resource pools: $_" -ForegroundColor Red
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
