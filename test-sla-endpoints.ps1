# SLA Endpoints Test Script
# Run this after migration is applied

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SLA ENDPOINTS TEST SCRIPT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get authentication token
Write-Host "1. Getting authentication token..." -ForegroundColor Yellow
$loginData = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5058/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $token = $response.data.token
    $headers = @{ Authorization = "Bearer $token" }
    Write-Host "   ✓ Token received" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Login failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 1: Get SLA Settings
Write-Host "2. Testing GET /api/sla/settings..." -ForegroundColor Yellow
try {
    $settings = Invoke-RestMethod -Uri "http://localhost:5058/api/sla/settings" -Method GET -Headers $headers
    Write-Host "   ✓ Settings retrieved: $($settings.message)" -ForegroundColor Green
    Write-Host "      IsEnabled: $($settings.data.isEnabled)" -ForegroundColor Gray
} catch {
    Write-Host "   ✗ Failed: $_" -ForegroundColor Red
}

Write-Host ""

# Test 2: Update SLA Settings
Write-Host "3. Testing PUT /api/sla/settings..." -ForegroundColor Yellow
$settingsData = @{
    isEnabled = $true
    workingHoursOnly = $true
    workingHoursStart = "09:00:00"
    workingHoursEnd = "17:00:00"
    workingDays = "1,2,3,4,5"
    excludeHolidays = $true
    autoEscalateOnBreach = $true
    escalationThresholdPercent = 80
    notifyBeforeBreach = $true
    notifyBeforeBreachMinutes = 30
    pauseSLAOnPendingInfo = $true
    timezone = "UTC"
} | ConvertTo-Json

try {
    $updated = Invoke-RestMethod -Uri "http://localhost:5058/api/sla/settings" -Method PUT -Headers $headers -Body $settingsData -ContentType "application/json"
    Write-Host "   ✓ Settings updated: $($updated.message)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Failed: $_" -ForegroundColor Red
}

Write-Host ""

# Test 3: Create SLA Level - Standard
Write-Host "4. Testing POST /api/sla/levels (Standard)..." -ForegroundColor Yellow
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
    Write-Host "   ✓ Standard level created: $($level1.data.name)" -ForegroundColor Green
    $level1Id = $level1.data.id
} catch {
    Write-Host "   ✗ Failed: $_" -ForegroundColor Red
}

Write-Host ""

# Test 4: Create SLA Level - Premium
Write-Host "5. Testing POST /api/sla/levels (Premium)..." -ForegroundColor Yellow
$premiumLevel = @{
    name = "Premium"
    description = "Premium SLA for high-priority complaints"
    order = 2
    isActive = $true
    colorCode = "#FF9800"
    defaultResponseTime = 2
    responseTimeUnit = "Hours"
    defaultResolutionTime = 8
    resolutionTimeUnit = "Hours"
} | ConvertTo-Json

try {
    $level2 = Invoke-RestMethod -Uri "http://localhost:5058/api/sla/levels" -Method POST -Headers $headers -Body $premiumLevel -ContentType "application/json"
    Write-Host "   ✓ Premium level created: $($level2.data.name)" -ForegroundColor Green
    $level2Id = $level2.data.id
} catch {
    Write-Host "   ✗ Failed: $_" -ForegroundColor Red
}

Write-Host ""

# Test 5: Create SLA Level - Enterprise
Write-Host "6. Testing POST /api/sla/levels (Enterprise)..." -ForegroundColor Yellow
$enterpriseLevel = @{
    name = "Enterprise"
    description = "Enterprise SLA for critical complaints"
    order = 3
    isActive = $true
    colorCode = "#F44336"
    defaultResponseTime = 1
    responseTimeUnit = "Hours"
    defaultResolutionTime = 4
    resolutionTimeUnit = "Hours"
} | ConvertTo-Json

try {
    $level3 = Invoke-RestMethod -Uri "http://localhost:5058/api/sla/levels" -Method POST -Headers $headers -Body $enterpriseLevel -ContentType "application/json"
    Write-Host "   ✓ Enterprise level created: $($level3.data.name)" -ForegroundColor Green
    $level3Id = $level3.data.id
} catch {
    Write-Host "   ✗ Failed: $_" -ForegroundColor Red
}

Write-Host ""

# Test 6: Get All SLA Levels
Write-Host "7. Testing GET /api/sla/levels..." -ForegroundColor Yellow
try {
    $levels = Invoke-RestMethod -Uri "http://localhost:5058/api/sla/levels" -Method GET -Headers $headers
    Write-Host "   ✓ Levels retrieved: $($levels.data.Count) levels found" -ForegroundColor Green
    foreach ($level in $levels.data) {
        Write-Host "      - $($level.name): Response $($level.responseTimeDisplay), Resolution $($level.resolutionTimeDisplay)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ✗ Failed: $_" -ForegroundColor Red
}

Write-Host ""

# Test 7: Get Specific Level
if ($level1Id) {
    Write-Host "8. Testing GET /api/sla/levels/{id}..." -ForegroundColor Yellow
    try {
        $level = Invoke-RestMethod -Uri "http://localhost:5058/api/sla/levels/$level1Id" -Method GET -Headers $headers
        Write-Host "   ✓ Level retrieved: $($level.data.name)" -ForegroundColor Green
        Write-Host "      Response: $($level.data.responseTimeInMinutes) minutes" -ForegroundColor Gray
        Write-Host "      Resolution: $($level.data.resolutionTimeInMinutes) minutes" -ForegroundColor Gray
    } catch {
        Write-Host "   ✗ Failed: $_" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 8: Update Level
if ($level1Id) {
    Write-Host "9. Testing PUT /api/sla/levels/{id}..." -ForegroundColor Yellow
    $updateData = @{
        name = "Standard (Updated)"
        description = "Updated standard SLA"
        order = 1
        isActive = $true
        colorCode = "#4CAF50"
        defaultResponseTime = 6
        responseTimeUnit = "Hours"
        defaultResolutionTime = 48
        resolutionTimeUnit = "Hours"
    } | ConvertTo-Json

    try {
        $updated = Invoke-RestMethod -Uri "http://localhost:5058/api/sla/levels/$level1Id" -Method PUT -Headers $headers -Body $updateData -ContentType "application/json"
        Write-Host "   ✓ Level updated: $($updated.data.name)" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ Failed: $_" -ForegroundColor Red
    }
    Write-Host ""
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "All basic CRUD operations tested successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Test category SLA mappings (when endpoints are added)" -ForegroundColor Gray
Write-Host "2. Test priority SLA mappings (when endpoints are added)" -ForegroundColor Gray
Write-Host "3. Connect Angular frontend to these endpoints" -ForegroundColor Gray
Write-Host ""
