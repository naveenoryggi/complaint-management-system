# Comprehensive test for all 22 route fixes

$BaseUrl = "http://localhost:5058"
$passed = 0
$failed = 0

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "COMPREHENSIVE ROUTE FIX VALIDATION" -ForegroundColor Cyan
Write-Host "Testing all 22 fixed routes" -ForegroundColor Cyan
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
Write-Host "Testing routes..." -ForegroundColor Cyan
Write-Host ""

# Test all routes
$routes = @(
    @{ Name = "Employee Types"; Endpoint = "/api/employee-types?companyId=$CompanyId" }
    @{ Name = "Roles"; Endpoint = "/api/roles" }
    @{ Name = "Email Settings"; Endpoint = "/api/email-settings?companyId=$CompanyId" }
    @{ Name = "SMS Gateway"; Endpoint = "/api/sms-gateway?companyId=$CompanyId" }
    @{ Name = "WhatsApp Settings"; Endpoint = "/api/whatsapp-settings?companyId=$CompanyId" }
    @{ Name = "Communication Templates"; Endpoint = "/api/communication-templates?companyId=$CompanyId" }
    @{ Name = "Event Communication Rules"; Endpoint = "/api/event-communication-rules?companyId=$CompanyId" }
    @{ Name = "Users"; Endpoint = "/api/users" }
    @{ Name = "Complaints"; Endpoint = "/api/complaints" }
    @{ Name = "Categories"; Endpoint = "/api/categories" }
    @{ Name = "Branches"; Endpoint = "/api/branches?companyId=$CompanyId" }
    @{ Name = "Departments"; Endpoint = "/api/departments" }
    @{ Name = "Sections"; Endpoint = "/api/sections" }
    @{ Name = "Dashboard"; Endpoint = "/api/dashboard/summary" }
    @{ Name = "Escalation"; Endpoint = "/api/escalation" }
    @{ Name = "Company"; Endpoint = "/api/company" }
    @{ Name = "Event Types"; Endpoint = "/api/event-types" }
    @{ Name = "Resource Pools"; Endpoint = "/api/resource-pools" }
    @{ Name = "Oryggi Connection"; Endpoint = "/api/oryggi-connection-settings?companyId=$CompanyId" }
    @{ Name = "Oryggi Sync"; Endpoint = "/api/oryggi-sync/status" }
    @{ Name = "Complaint Info Settings"; Endpoint = "/api/complaint-info-settings?companyId=$CompanyId" }
)

$counter = 1
foreach ($route in $routes) {
    Write-Host "[$counter/$($routes.Count)] Testing $($route.Name)..." -NoNewline
    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl$($route.Endpoint)" -Headers $authHeaders -UseBasicParsing -ErrorAction Stop
        Write-Host " [OK] Status: $($response.StatusCode)" -ForegroundColor Green
        $passed++
    } catch {
        $statusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.value__ } else { "N/A" }
        Write-Host " [FAIL] Status: $statusCode" -ForegroundColor Red
        $failed++
    }
    $counter++
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "RESULTS" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Passed: $passed / $($routes.Count)" -ForegroundColor $(if ($passed -eq $routes.Count) { "Green" } else { "Yellow" })
Write-Host "Failed: $failed / $($routes.Count)" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
$successRate = [math]::Round(($passed / $routes.Count) * 100, 2)
Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -eq 100) { "Green" } elseif ($successRate -ge 80) { "Yellow" } else { "Red" })

if ($passed -eq $routes.Count) {
    Write-Host ""
    Write-Host "SUCCESS! All $($routes.Count) routes are working!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Some routes still need attention. Check failed endpoints above." -ForegroundColor Yellow
}
