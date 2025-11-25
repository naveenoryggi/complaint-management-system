# Quick Password Endpoint Test
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()
$userId = "f56d8d03-e382-454b-bf7d-fa8236c125c3"
$companyId = "fe28cd85-4226-4daa-9e45-66a3d51877fa"

Write-Host ""
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "TESTING PASSWORD MANAGEMENT ENDPOINTS" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Password Strength (Anonymous - No Auth Required)
Write-Host "Test 1: Password Strength Endpoint" -ForegroundColor Yellow
try {
    $body = @{ password = "Test@123456" } | ConvertTo-Json
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/password/strength" -Method POST -Body $body -ContentType "application/json"
    Write-Host "  SUCCESS" -ForegroundColor Green
    Write-Host "  Score: $($response.score), Category: $($response.category), Color: $($response.colorCode)" -ForegroundColor Gray
} catch {
    Write-Host "  FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Password Validation
Write-Host ""
Write-Host "Test 2: Password Validation Endpoint" -ForegroundColor Yellow
try {
    $headers = @{ "Authorization" = "Bearer $token" }
    $body = @{ password = "NewPass@123"; companyId = $companyId } | ConvertTo-Json
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/password/validate" -Method POST -Body $body -ContentType "application/json" -Headers $headers
    Write-Host "  SUCCESS" -ForegroundColor Green
    Write-Host "  Valid: $($response.isValid), Errors: $($response.errors.Count)" -ForegroundColor Gray
} catch {
    Write-Host "  FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get Password Status
Write-Host ""
Write-Host "Test 3: Get Password Status Endpoint" -ForegroundColor Yellow
try {
    $headers = @{ "Authorization" = "Bearer $token" }
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/password/status" -Method GET -Headers $headers
    Write-Host "  SUCCESS" -ForegroundColor Green
    Write-Host "  Days Until Expiration: $($response.daysUntilExpiration), Expired: $($response.isExpired), Locked: $($response.isLocked)" -ForegroundColor Gray
} catch {
    Write-Host "  FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Generate Password (Admin)
Write-Host ""
Write-Host "Test 4: Generate Password Endpoint (Admin)" -ForegroundColor Yellow
try {
    $headers = @{ "Authorization" = "Bearer $token" }
    $body = @{ length = 16; includeUppercase = $true; includeLowercase = $true; includeDigits = $true; includeSpecialChars = $true } | ConvertTo-Json
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/password/generate" -Method POST -Body $body -ContentType "application/json" -Headers $headers
    Write-Host "  SUCCESS" -ForegroundColor Green
    Write-Host "  Generated: $($response.password)" -ForegroundColor Gray
    Write-Host "  Strength: $($response.strength.category) (Score: $($response.strength.score))" -ForegroundColor Gray
} catch {
    Write-Host "  FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Get User Password Status (Admin)
Write-Host ""
Write-Host "Test 5: Get User Password Status by ID (Admin)" -ForegroundColor Yellow
try {
    $headers = @{ "Authorization" = "Bearer $token" }
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/password/status/$userId" -Method GET -Headers $headers
    Write-Host "  SUCCESS" -ForegroundColor Green
    Write-Host "  User ID: $($response.userId), Days Until Exp: $($response.daysUntilExpiration), Locked: $($response.isLocked)" -ForegroundColor Gray
} catch {
    Write-Host "  FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "TESTING COMPLETE" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""
