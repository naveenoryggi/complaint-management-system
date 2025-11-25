$BaseUrl = "http://localhost:5000"

Write-Host "Testing Notification System API Authentication" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login and get fresh token
Write-Host "Step 1: Getting fresh authentication token..." -ForegroundColor Yellow
try {
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "SUCCESS: Got token (length: $($token.Length))" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Test Event Types API
Write-Host "Step 2: Testing Event Types API..." -ForegroundColor Yellow
$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $eventTypes = Invoke-RestMethod -Uri "$BaseUrl/api/event-types" -Method GET -Headers $headers
    Write-Host "SUCCESS: Event Types API - Found $($eventTypes.Count) event types" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "FAILED: Event Types API" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    Write-Host ""
}

# Step 3: Test Communication Templates API
Write-Host "Step 3: Testing Communication Templates API..." -ForegroundColor Yellow
try {
    $templates = Invoke-RestMethod -Uri "$BaseUrl/api/communication-templates" -Method GET -Headers $headers
    Write-Host "SUCCESS: Templates API - Found $($templates.Count) templates" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "FAILED: Templates API" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    Write-Host ""
}

# Step 4: Test Event Communication Rules API
Write-Host "Step 4: Testing Event Communication Rules API..." -ForegroundColor Yellow
try {
    $rules = Invoke-RestMethod -Uri "$BaseUrl/api/event-communication-rules" -Method GET -Headers $headers
    Write-Host "SUCCESS: Rules API - Found $($rules.Count) rules" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "FAILED: Rules API" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    Write-Host ""
}

# Step 5: Check if endpoints exist by calling them
Write-Host "Step 5: Checking endpoint availability..." -ForegroundColor Yellow

$endpoints = @(
    "GET /api/event-types",
    "GET /api/event-types/entity-types",
    "GET /api/event-types/categories",
    "GET /api/communication-templates",
    "GET /api/event-communication-rules"
)

foreach ($endpoint in $endpoints) {
    $parts = $endpoint -split ' '
    $method = $parts[0]
    $path = $parts[1]

    try {
        $result = Invoke-RestMethod -Uri "$BaseUrl$path" -Method $method -Headers $headers -ErrorAction Stop
        Write-Host "  OK: $endpoint" -ForegroundColor Green
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 404) {
            Write-Host "  NOT FOUND (404): $endpoint" -ForegroundColor Red
        } elseif ($statusCode -eq 401) {
            Write-Host "  UNAUTHORIZED (401): $endpoint" -ForegroundColor Yellow
        } else {
            Write-Host "  ERROR ($statusCode): $endpoint" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "Test complete!" -ForegroundColor Cyan
