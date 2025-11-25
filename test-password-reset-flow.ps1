# Test Password Reset Flow
# Comprehensive testing script for self-service password reset functionality

Write-Host "`n=== Password Reset Flow - Comprehensive Test Suite ===" -ForegroundColor Cyan
Write-Host "Testing Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray

$apiUrl = "http://localhost:5000/api"
$testResults = @()

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [object]$Body,
        [int]$ExpectedStatus
    )

    Write-Host "`nTesting: $Name" -ForegroundColor Yellow

    try {
        $params = @{
            Uri = $Url
            Method = $Method
            ContentType = "application/json"
            ErrorAction = "Stop"
        }

        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }

        $response = Invoke-RestMethod @params
        $statusCode = 200

        Write-Host "  Status: SUCCESS ($statusCode)" -ForegroundColor Green
        Write-Host "  Response: $($response | ConvertTo-Json -Compress)" -ForegroundColor Gray

        $script:testResults += [PSCustomObject]@{
            Test = $Name
            Status = "PASS"
            StatusCode = $statusCode
            Response = $response
        }

        return $response
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $errorMessage = $_.ErrorDetails.Message

        if ($statusCode -eq $ExpectedStatus) {
            Write-Host "  Status: EXPECTED ERROR ($statusCode)" -ForegroundColor Yellow
            $script:testResults += [PSCustomObject]@{
                Test = $Name
                Status = "PASS"
                StatusCode = $statusCode
                Response = $errorMessage
            }
        }
        else {
            Write-Host "  Status: FAIL ($statusCode)" -ForegroundColor Red
            Write-Host "  Error: $errorMessage" -ForegroundColor Red
            $script:testResults += [PSCustomObject]@{
                Test = $Name
                Status = "FAIL"
                StatusCode = $statusCode
                Response = $errorMessage
            }
        }

        return $null
    }
}

# Test 1: Request Password Reset - Valid Email
Write-Host "`n--- Test 1: Request Password Reset (Valid Email) ---" -ForegroundColor Magenta
$response1 = Test-Endpoint `
    -Name "Request password reset for admin@company.com" `
    -Method "POST" `
    -Url "$apiUrl/password-reset/request" `
    -Body @{ email = "admin@company.com" } `
    -ExpectedStatus 200

# Test 2: Request Password Reset - Invalid Email
Write-Host "`n--- Test 2: Request Password Reset (Invalid Email) ---" -ForegroundColor Magenta
$response2 = Test-Endpoint `
    -Name "Request password reset for nonexistent@test.com" `
    -Method "POST" `
    -Url "$apiUrl/password-reset/request" `
    -Body @{ email = "nonexistent@test.com" } `
    -ExpectedStatus 200  # Should still return success for security

# Test 3: Request Password Reset - Empty Email
Write-Host "`n--- Test 3: Request Password Reset (Empty Email) ---" -ForegroundColor Magenta
$response3 = Test-Endpoint `
    -Name "Request password reset with empty email" `
    -Method "POST" `
    -Url "$apiUrl/password-reset/request" `
    -Body @{ email = "" } `
    -ExpectedStatus 200

# Test 4: Rate Limiting - Multiple Requests
Write-Host "`n--- Test 4: Rate Limiting Test ---" -ForegroundColor Magenta
for ($i = 1; $i -le 5; $i++) {
    Write-Host "  Request $i of 5..." -ForegroundColor Gray
    $rateLimitTest = Test-Endpoint `
        -Name "Rate limit test - Request $i" `
        -Method "POST" `
        -Url "$apiUrl/password-reset/request" `
        -Body @{ email = "ratelimit@test.com" } `
        -ExpectedStatus $(if ($i -le 3) { 200 } else { 429 })

    if ($i -lt 5) { Start-Sleep -Seconds 1 }
}

# Test 5: Validate Token - Invalid Token
Write-Host "`n--- Test 5: Validate Invalid Token ---" -ForegroundColor Magenta
$response5 = Test-Endpoint `
    -Name "Validate invalid token" `
    -Method "POST" `
    -Url "$apiUrl/password-reset/validate" `
    -Body @{ token = "invalid-token-12345" } `
    -ExpectedStatus 200

# Test 6: Validate Token - Empty Token
Write-Host "`n--- Test 6: Validate Empty Token ---" -ForegroundColor Magenta
$response6 = Test-Endpoint `
    -Name "Validate empty token" `
    -Method "POST" `
    -Url "$apiUrl/password-reset/validate" `
    -Body @{ token = "" } `
    -ExpectedStatus 200

# Test 7: Reset Password - Invalid Token
Write-Host "`n--- Test 7: Reset Password with Invalid Token ---" -ForegroundColor Magenta
$response7 = Test-Endpoint `
    -Name "Reset password with invalid token" `
    -Method "POST" `
    -Url "$apiUrl/password-reset/reset" `
    -Body @{
        token = "invalid-token-12345"
        newPassword = "NewSecureP@ssw0rd!"
    } `
    -ExpectedStatus 400

# Test 8: Reset Password - Weak Password
Write-Host "`n--- Test 8: Reset Password with Weak Password ---" -ForegroundColor Magenta
$response8 = Test-Endpoint `
    -Name "Reset password with weak password" `
    -Method "POST" `
    -Url "$apiUrl/password-reset/reset" `
    -Body @{
        token = "valid-token-placeholder"
        newPassword = "123456"
    } `
    -ExpectedStatus 400

# Generate Summary Report
Write-Host "`n`n=== Test Summary ===" -ForegroundColor Cyan

$passCount = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$totalCount = $testResults.Count

Write-Host "Total Tests: $totalCount" -ForegroundColor White
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })

if ($failCount -gt 0) {
    Write-Host "`nFailed Tests:" -ForegroundColor Red
    $testResults | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
        Write-Host "  - $($_.Test) (Status Code: $($_.StatusCode))" -ForegroundColor Red
    }
}

# Save results to file
$resultsFile = "password-reset-test-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$testResults | ConvertTo-Json -Depth 10 | Out-File $resultsFile
Write-Host "`nResults saved to: $resultsFile" -ForegroundColor Green

Write-Host "`n=== Manual Testing Steps ===" -ForegroundColor Cyan
Write-Host @"

1. Navigate to http://localhost:4200/login
2. Click 'Forgot Password' link
3. Enter your email address (e.g., admin@company.com)
4. Click 'Send Reset Link'
5. Check your email for the reset link
6. Click the reset link (or copy the token from the email)
7. Enter a new password (must meet complexity requirements)
8. Confirm the password
9. Click 'Reset Password'
10. Login with your new password

Expected Results:
- Forgot password page loads successfully
- Email is sent with reset link
- Reset link navigates to reset password page
- Token is validated successfully
- Password strength meter shows password strength
- Password update succeeds
- Login with new password works

"@ -ForegroundColor Gray

Write-Host "=== Test Complete ===" -ForegroundColor Cyan
