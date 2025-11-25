# Login to API
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "http://localhost:5058/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token
Write-Host "Logged in successfully" -ForegroundColor Green

# Setup headers
$headers = @{ "Authorization" = "Bearer $token" }

# Get the email server
$emailServers = Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings?includeInactive=false" -Method Get -Headers $headers
$server = $emailServers.data | Where-Object { $_.name -eq "Production Gmail SMTP" }

Write-Host "Found server: $($server.name) with ID: $($server.id)" -ForegroundColor Cyan
Write-Host ""

# Check what the backend expects
Write-Host "=== Testing Email Configuration ===" -ForegroundColor Yellow
Write-Host ""

# Test 1: with testRecipient parameter
Write-Host "Test 1: Using testRecipient parameter" -ForegroundColor Cyan
$testBody1 = @{
    testRecipient = "nav_nainital@yahoo.com"
} | ConvertTo-Json

Write-Host "Request body: $testBody1" -ForegroundColor Gray

try {
    $testResponse1 = Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings/$($server.id)/test" -Method Post -Body $testBody1 -ContentType "application/json" -Headers $headers
    Write-Host "SUCCESS with testRecipient!" -ForegroundColor Green
    Write-Host "Response: $($testResponse1 | ConvertTo-Json -Depth 3)" -ForegroundColor Gray
} catch {
    Write-Host "FAILED with testRecipient" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "---" -ForegroundColor Gray
Write-Host ""

# Test 2: with testEmail parameter
Write-Host "Test 2: Using testEmail parameter" -ForegroundColor Cyan
$testBody2 = @{
    testEmail = "nav_nainital@yahoo.com"
} | ConvertTo-Json

Write-Host "Request body: $testBody2" -ForegroundColor Gray

try {
    $testResponse2 = Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings/$($server.id)/test" -Method Post -Body $testBody2 -ContentType "application/json" -Headers $headers
    Write-Host "SUCCESS with testEmail!" -ForegroundColor Green
    Write-Host "Response: $($testResponse2 | ConvertTo-Json -Depth 3)" -ForegroundColor Gray
} catch {
    Write-Host "FAILED with testEmail" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Yellow
