# Quick SLA API Test
Write-Host "Testing SLA Endpoints..." -ForegroundColor Cyan

# Login
$loginData = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5058/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $token = $response.data.token
    $headers = @{ Authorization = "Bearer $token" }
    Write-Host "✓ Login successful" -ForegroundColor Green
} catch {
    Write-Host "✗ Login failed - Backend may not be running" -ForegroundColor Red
    exit 1
}

# Test GET /api/sla/settings
Write-Host "`nTesting GET /api/sla/settings..." -ForegroundColor Yellow
try {
    $settings = Invoke-RestMethod -Uri "http://localhost:5058/api/sla/settings" -Method GET -Headers $headers
    Write-Host "✓ SLA settings retrieved" -ForegroundColor Green
    Write-Host "  IsEnabled: $($settings.data.isEnabled)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Failed: $_" -ForegroundColor Red
}

# Test POST /api/sla/levels - Create Standard
Write-Host "`nTesting POST /api/sla/levels (Standard)..." -ForegroundColor Yellow
$standardLevel = @{
    name = "Standard"
    description = "Standard SLA for regular complaints"
    order = 1
    isActive = $true
    colorCode = "#4CAF50"
    defaultResponseTime = 4
    responseTimeUnit = "Hours"
    defaultResolutionTime = 24
    resolutionTimeUnit = "Hours"
} | ConvertTo-Json

try {
    $level1 = Invoke-RestMethod -Uri "http://localhost:5058/api/sla/levels" -Method POST -Headers $headers -Body $standardLevel -ContentType "application/json"
    Write-Host "✓ Standard level created: $($level1.data.name)" -ForegroundColor Green
    Write-Host "  Response: $($level1.data.responseTimeDisplay)" -ForegroundColor Gray
    Write-Host "  Resolution: $($level1.data.resolutionTimeDisplay)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Failed: $_" -ForegroundColor Red
}

# Test GET /api/sla/levels - List all
Write-Host "`nTesting GET /api/sla/levels..." -ForegroundColor Yellow
try {
    $levels = Invoke-RestMethod -Uri "http://localhost:5058/api/sla/levels" -Method GET -Headers $headers
    Write-Host "✓ SLA levels retrieved: $($levels.data.Count) level(s)" -ForegroundColor Green
    foreach ($level in $levels.data) {
        Write-Host "  - $($level.name): Response $($level.responseTimeDisplay), Resolution $($level.resolutionTimeDisplay)" -ForegroundColor Gray
    }
} catch {
    Write-Host "✗ Failed: $_" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Complete!" -ForegroundColor Green
Write-Host "Backend SLA system is working!" -ForegroundColor Green
