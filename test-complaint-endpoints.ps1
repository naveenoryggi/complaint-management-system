# Test Complaint Management failing endpoints
$baseUrl = "http://localhost:5058"

# Ensure admin user is active
Write-Host "Ensuring admin user is active..." -ForegroundColor Yellow
try {
    $null = sqlcmd -S 'LAPTOP-NF9BTG7Q\SQLEXPRESS' -d 'ComplaintManagementDB' -E -Q "UPDATE Users SET IsActive = 1, IsDeleted = 0, DeletedAt = NULL WHERE Email = 'admin@complaintmanagement.com'" -W 2>$null
    Write-Host "[INFO] Admin user activated" -ForegroundColor Cyan
} catch {
    Write-Host "[WARN] Could not ensure admin active: $_" -ForegroundColor Yellow
}

# Login first to get fresh token
Write-Host "Logging in..." -ForegroundColor Yellow
try {
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $TOKEN = $loginResponse.data.token
    Write-Host "[SUCCESS] Logged in successfully" -ForegroundColor Green
}
catch {
    Write-Host "[FAIL] Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $TOKEN"
    "Content-Type" = "application/json"
}
$testId = "00000000-0000-0000-0000-000000000001"

function Test-API {
    param([string]$name, [string]$method, [string]$endpoint, [object]$body = $null)

    Write-Host "`n=== Testing: $name ===" -ForegroundColor Cyan
    try {
        $params = @{
            Uri = "$baseUrl$endpoint"
            Method = $method
            Headers = $headers
            ErrorAction = "Stop"
        }

        if ($body) {
            $params.Body = ($body | ConvertTo-Json)
        }

        $response = Invoke-RestMethod @params
        Write-Host "[PASS] Status: 200, Response:" -ForegroundColor Green
        $response | ConvertTo-Json -Depth 3 | Write-Host
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $errorBody = $_.ErrorDetails.Message
        Write-Host "[FAIL] Status: $statusCode" -ForegroundColor Red
        if ($errorBody) {
            Write-Host "Error: $errorBody" -ForegroundColor Yellow
        }
        Write-Host "Exception: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Test the failing endpoints
Test-API "Get Complaint by ID" "GET" "/api/complaints/$testId"
Test-API "Delete Complaint" "DELETE" "/api/complaints/$testId"
Test-API "Get Comments" "GET" "/api/complaints/$testId/comments"
Test-API "Get Attachments" "GET" "/api/complaints/$testId/attachments"
Test-API "Get History" "GET" "/api/complaints/$testId/history"
Test-API "Create Complaint" "POST" "/api/complaints" @{
    title = "Test Complaint"
    description = "Test Description"
}
Test-API "Add Comment" "POST" "/api/complaints/$testId/comments" @{
    content = "Test Comment"
}
