$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "Testing 404 Endpoints..." -ForegroundColor Cyan
Write-Host ""

# Test Employee Types
Write-Host "1. Testing Employee Types endpoint:" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/employeetypes" -Headers $headers
    Write-Host "   SUCCESS - EmployeeTypes working" -ForegroundColor Green
} catch {
    Write-Host "   FAILED - Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
}

# Test SMS Gateway Settings
Write-Host "2. Testing SMS Gateway Settings endpoint:" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/communication/sms-settings" -Headers $headers
    Write-Host "   SUCCESS - SMS Settings working" -ForegroundColor Green
} catch {
    Write-Host "   FAILED - Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
}

# Test WhatsApp Settings
Write-Host "3. Testing WhatsApp Settings endpoint:" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/communication/whatsapp-settings" -Headers $headers
    Write-Host "   SUCCESS - WhatsApp Settings working" -ForegroundColor Green
} catch {
    Write-Host "   FAILED - Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
}

# Test Complaint Info Settings
Write-Host "4. Testing Complaint Info Settings endpoint:" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/ComplaintInfoSettings" -Headers $headers
    Write-Host "   SUCCESS - Complaint Info Settings working" -ForegroundColor Green
} catch {
    Write-Host "   FAILED - Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Test Complete" -ForegroundColor Cyan
