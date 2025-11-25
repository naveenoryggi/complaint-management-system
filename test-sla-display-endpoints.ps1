# Test SLA Display Endpoints
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()
$baseUrl = "http://localhost:5000"

Write-Host "`n=== SLA DISPLAY ENDPOINT TESTS ===" -ForegroundColor Cyan

# First, get some complaints to test with
Write-Host "`n1. Getting complaints..." -ForegroundColor Yellow
$complaintsResponse = Invoke-RestMethod -Uri "$baseUrl/api/complaints?page=1&pageSize=5" `
    -Method GET `
    -Headers @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

Write-Host "Got $($complaintsResponse.data.items.Count) complaints" -ForegroundColor Green
$complaintIds = $complaintsResponse.data.items | Select-Object -First 3 | ForEach-Object { $_.id }
Write-Host "Sample Complaint IDs:"
$complaintIds | ForEach-Object { Write-Host "  - $_" }

# Test 1: GET /api/sla/status/{complaintId}
Write-Host "`n2. Testing GET /api/sla/status/{complaintId}..." -ForegroundColor Yellow
if ($complaintIds.Count -gt 0) {
    $testId = $complaintIds[0]
    Write-Host "Testing with complaint ID: $testId"

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/sla/status/$testId" `
            -Method GET `
            -Headers @{
                "Authorization" = "Bearer $token"
                "Content-Type" = "application/json"
            }

        Write-Host "Success - Status: SUCCESS" -ForegroundColor Green
        Write-Host "Response Data:"
        $response.data | ConvertTo-Json -Depth 3
    }
    catch {
        Write-Host "Failed - Status: FAILED" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 2: POST /api/sla/status/bulk
Write-Host "`n3. Testing POST /api/sla/status/bulk..." -ForegroundColor Yellow
Write-Host "Sending bulk request for $($complaintIds.Count) complaints"

try {
    $requestBody = @{
        ComplaintIds = @($complaintIds)
    }
    $body = $requestBody | ConvertTo-Json
    Write-Host "Request body: $body"

    $response = Invoke-RestMethod -Uri "$baseUrl/api/sla/status/bulk" `
        -Method POST `
        -Headers @{
            "Authorization" = "Bearer $token"
            "Content-Type" = "application/json"
        } `
        -Body $body

    Write-Host "Success - Status: SUCCESS" -ForegroundColor Green
    Write-Host "Response Data:"
    $response.data | ConvertTo-Json -Depth 3
}
catch {
    Write-Host "Failed - Status: FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== SLA ENDPOINT TESTS COMPLETE ===" -ForegroundColor Cyan
