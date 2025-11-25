# Quick test for Communication validation fixes
$baseUrl = "http://localhost:5058/api"
$testResults = @()
$passCount = 0
$failCount = 0

Write-Host "`n=== Testing Communication Validation Fixes ===" -ForegroundColor Cyan
Write-Host "Testing 3 nullable field validations...`n"

# Login first
try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body (@{
        email = "admin@tenant1.com"
        password = "Admin@123"
    } | ConvertTo-Json) -ContentType "application/json"
    $token = $loginResponse.data.token
    $headers = @{ Authorization = "Bearer $token" }
    Write-Host "[OK] Logged in successfully" -ForegroundColor Green
} catch {
    Write-Host "[FAIL] Login failed: $_" -ForegroundColor Red
    exit 1
}

# Test 1: SMS - Empty API Key (AccountSid)
Write-Host "`n1. Testing SMS: Empty API Key (AccountSid)..."
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/sms-gateway" -Method Post `
        -Headers $headers `
        -Body (@{
            name = "Test SMS"
            provider = "Twilio"
            accountSid = ""  # Empty - should fail
            authToken = "test123"
            fromNumber = "+1234567890"
        } | ConvertTo-Json) `
        -ContentType "application/json" `
        -SkipHttpErrorCheck

    if ($response.StatusCode -eq 400) {
        Write-Host "[PASS] Returned 400 Bad Request as expected" -ForegroundColor Green
        $passCount++
    } else {
        Write-Host "[FAIL] Expected 400, got $($response.StatusCode)" -ForegroundColor Red
        $failCount++
    }
} catch {
    Write-Host "[FAIL] Test error: $_" -ForegroundColor Red
    $failCount++
}

# Test 2: WhatsApp - Empty Phone Number ID
Write-Host "`n2. Testing WhatsApp: Empty Phone Number ID..."
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/whatsapp-settings" -Method Post `
        -Headers $headers `
        -Body (@{
            name = "Test WhatsApp"
            provider = "WhatsApp Business API"
            businessAccountId = "test123"
            phoneNumberId = ""  # Empty - should fail
            accessToken = "test_token"
            fromNumber = "+1234567890"
        } | ConvertTo-Json) `
        -ContentType "application/json" `
        -SkipHttpErrorCheck

    if ($response.StatusCode -eq 400) {
        Write-Host "[PASS] Returned 400 Bad Request as expected" -ForegroundColor Green
        $passCount++
    } else {
        Write-Host "[FAIL] Expected 400, got $($response.StatusCode)" -ForegroundColor Red
        $failCount++
    }
} catch {
    Write-Host "[FAIL] Test error: $_" -ForegroundColor Red
    $failCount++
}

# Test 3: WhatsApp - Empty Business Account ID
Write-Host "`n3. Testing WhatsApp: Empty Business Account ID..."
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/whatsapp-settings" -Method Post `
        -Headers $headers `
        -Body (@{
            name = "Test WhatsApp 2"
            provider = "WhatsApp Business API"
            businessAccountId = ""  # Empty - should fail
            phoneNumberId = "test_phone_id"
            accessToken = "test_token"
            fromNumber = "+1234567890"
        } | ConvertTo-Json) `
        -ContentType "application/json" `
        -SkipHttpErrorCheck

    if ($response.StatusCode -eq 400) {
        Write-Host "[PASS] Returned 400 Bad Request as expected" -ForegroundColor Green
        $passCount++
    } else {
        Write-Host "[FAIL] Expected 400, got $($response.StatusCode)" -ForegroundColor Red
        $failCount++
    }
} catch {
    Write-Host "[FAIL] Test error: $_" -ForegroundColor Red
    $failCount++
}

# Summary
Write-Host "`n=== Test Summary ===" -ForegroundColor Cyan
Write-Host "Passed: $passCount/3" -ForegroundColor $(if ($passCount -eq 3) { "Green" } else { "Yellow" })
Write-Host "Failed: $failCount/3" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })

if ($passCount -eq 3) {
    Write-Host "`nAll Communication validation tests PASSED!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nSome tests failed. Review above for details." -ForegroundColor Yellow
    exit 1
}
