# Clean validation test for route fixes

$BaseUrl = "http://localhost:5058"
$passed = 0
$failed = 0

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "ROUTE FIX VALIDATION TEST" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Auth
Write-Host "Authenticating..." -ForegroundColor Yellow
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    $CompanyId = $loginResponse.data.user.companyId
    $authHeaders = @{ "Authorization" = "Bearer $token" }
    Write-Host "[OK] Authentication successful" -ForegroundColor Green
} catch {
    Write-Host "[FAIL] Authentication failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test Employee Types
Write-Host "[1/7] Testing Employee Types..." -NoNewline
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/employee-types?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host " [OK] Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host " [FAIL] Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

# Test Roles
Write-Host "[2/7] Testing Roles..." -NoNewline
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/roles" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host " [OK] Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host " [FAIL] Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

# Test Email Settings
Write-Host "[3/7] Testing Email Settings..." -NoNewline
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/email-settings?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host " [OK] Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host " [FAIL] Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

# Test SMS Gateway
Write-Host "[4/7] Testing SMS Gateway..." -NoNewline
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/sms-gateway?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host " [OK] Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host " [FAIL] Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

# Test WhatsApp Settings
Write-Host "[5/7] Testing WhatsApp Settings..." -NoNewline
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/whatsapp-settings?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host " [OK] Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host " [FAIL] Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

# Test Communication Templates
Write-Host "[6/7] Testing Communication Templates..." -NoNewline
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/communication-templates?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host " [OK] Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host " [FAIL] Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

# Test Event Communication Rules
Write-Host "[7/7] Testing Event Communication Rules..." -NoNewline
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/event-communication-rules?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host " [OK] Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host " [FAIL] Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "RESULTS" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Passed: $passed / 7" -ForegroundColor $(if ($passed -eq 7) { "Green" } else { "Yellow" })
Write-Host "Failed: $failed / 7" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
$successRate = [math]::Round(($passed / 7) * 100, 2)
Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -eq 100) { "Green" } elseif ($successRate -ge 70) { "Yellow" } else { "Red" })

if ($passed -eq 7) {
    Write-Host ""
    Write-Host "SUCCESS! All routes fixed and working!" -ForegroundColor Green
}
