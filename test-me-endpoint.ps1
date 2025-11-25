# Test /me endpoint
$baseUrl = "http://localhost:5058"

# Ensure admin is active
try {
    $null = sqlcmd -S 'LAPTOP-NF9BTG7Q\SQLEXPRESS' -d 'ComplaintManagementDB' -E -Q "UPDATE Users SET IsActive = 1, IsDeleted = 0, DeletedAt = NULL WHERE Email = 'admin@complaintmanagement.com'" -W 2>$null
    Write-Host "[INFO] Admin user activated" -ForegroundColor Cyan
} catch {
    Write-Host "[WARN] Could not ensure admin active" -ForegroundColor Yellow
}

# Login
Write-Host "`nLogging in..." -ForegroundColor Yellow
try {
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "[PASS] Login successful" -ForegroundColor Green
}
catch {
    Write-Host "[FAIL] Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test /me endpoint
Write-Host "`n=== Testing /me endpoint ===" -ForegroundColor Cyan
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }

    $meResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/me" -Method GET -Headers $headers
    Write-Host "[PASS] /me endpoint returned 200" -ForegroundColor Green
    Write-Host "`nResponse:" -ForegroundColor Cyan
    $meResponse | ConvertTo-Json -Depth 3
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message
    Write-Host "[FAIL] Status: $statusCode" -ForegroundColor Red
    if ($errorBody) {
        Write-Host "Error: $errorBody" -ForegroundColor Yellow
    }
}
