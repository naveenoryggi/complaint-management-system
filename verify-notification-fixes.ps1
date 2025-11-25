# Verify Notification System API Fixes
# Tests all 3 notification API endpoints

$token = Get-Content .test-token -Raw
$headers = @{
    'Authorization' = "Bearer $($token.Trim())"
    'Content-Type' = 'application/json'
}

Write-Host ""
Write-Host "=== NOTIFICATION SYSTEM API VERIFICATION ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Event Types
Write-Host "1. Testing Event Types API..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/event-types' -Method GET -Headers $headers
    Write-Host "   ✓ GET /api/event-types - SUCCESS" -ForegroundColor Green
    Write-Host "     Found $($response.data.Count) event types" -ForegroundColor Gray
} catch {
    Write-Host "   ✗ GET /api/event-types - FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 2: Communication Templates
Write-Host "2. Testing Communication Templates API..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/communication-templates' -Method GET -Headers $headers
    Write-Host "   ✓ GET /api/communication-templates - SUCCESS" -ForegroundColor Green
    Write-Host "     Found $($response.data.Count) templates" -ForegroundColor Gray
} catch {
    Write-Host "   ✗ GET /api/communication-templates - FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: Event Communication Rules
Write-Host "3. Testing Event Communication Rules API..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/event-communication-rules' -Method GET -Headers $headers
    Write-Host "   ✓ GET /api/event-communication-rules - SUCCESS" -ForegroundColor Green
    Write-Host "     Found $($response.data.Count) rules" -ForegroundColor Gray
} catch {
    Write-Host "   ✗ GET /api/event-communication-rules - FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== VERIFICATION COMPLETE ===" -ForegroundColor Cyan
Write-Host ""
