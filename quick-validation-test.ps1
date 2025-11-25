# Quick validation test for route fixes

$BaseUrl = "http://localhost:5058"
$passed = 0
$failed = 0

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "QUICK VALIDATION TEST - Route Fixes" -ForegroundColor Cyan
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
    Write-Host "✓ Authentication successful" -ForegroundColor Green
} catch {
    Write-Host "✗ Authentication failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test Employee Types (was 404, should now work)
Write-Host "[1] Testing Employee Types endpoints..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/employee-types?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✓ GET /api/employee-types - Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "  ✗ GET /api/employee-types - Failed: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
}

# Test Roles (was 404, should now work)
Write-Host "[2] Testing Roles endpoints..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/roles" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✓ GET /api/roles - Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "  ✗ GET /api/roles - Failed: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
}

# Test Email Settings (was 404, should now work)
Write-Host "[3] Testing Email Settings endpoints..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/email-settings?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✓ GET /api/email-settings - Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "  ✗ GET /api/email-settings - Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

# Test SMS Gateway (was 404, should now work)
Write-Host "[4] Testing SMS Gateway endpoints..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/sms-gateway?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✓ GET /api/sms-gateway - Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "  ✗ GET /api/sms-gateway - Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

# Test WhatsApp Settings (was 404, should now work)
Write-Host "[5] Testing WhatsApp Settings endpoints..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/whatsapp-settings?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✓ GET /api/whatsapp-settings - Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "  ✗ GET /api/whatsapp-settings - Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

# Test Communication Templates (was 404, should now work)
Write-Host "[6] Testing Communication Templates endpoints..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/communication-templates?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✓ GET /api/communication-templates - Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "  ✗ GET /api/communication-templates - Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $failed++
}

# Test Event Communication Rules (was 404, should now work)
Write-Host "[7] Testing Event Communication Rules endpoints..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/event-communication-rules?companyId=$CompanyId" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✓ GET /api/event-communication-rules - Status: $($response.StatusCode)" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "  ✗ GET /api/event-communication-rules - Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
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
    Write-Host "ALL ROUTES FIXED SUCCESSFULLY!" -ForegroundColor Green
}
