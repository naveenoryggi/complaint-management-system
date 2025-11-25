# ============================================
# USER AUTHENTICATION TEST SCRIPT
# Tests login for both users via API
# ============================================

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "USER AUTHENTICATION TEST" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000/api"
$loginUrl = "$baseUrl/auth/login"

# Test Results
$results = @{
    complainant = $null
    handler = $null
}

# ============================================
# TEST 1: Complainant Login
# ============================================

Write-Host "TEST 1: Complainant Login" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow
Write-Host "Email: nav_nainital@yahoo.com"
Write-Host "Password: Nav@12345"
Write-Host ""

$complainantBody = @{
    email = "nav_nainital@yahoo.com"
    password = "Nav@12345"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $complainantBody -ContentType "application/json"

    Write-Host "SUCCESS: Login successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Response Details:" -ForegroundColor Green
    Write-Host "  Token Length: $($response.token.Length)"
    Write-Host "  User ID: $($response.userId)"
    Write-Host "  Email: $($response.email)"
    Write-Host "  Name: $($response.firstName) $($response.lastName)"
    Write-Host "  Roles: $($response.roles -join ', ')"
    Write-Host ""

    # Decode JWT token to show claims
    $tokenParts = $response.token.Split('.')
    if ($tokenParts.Length -ge 2) {
        $payload = $tokenParts[1]
        # Add padding if needed
        while ($payload.Length % 4 -ne 0) {
            $payload += "="
        }
        $payloadJson = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($payload))
        $claims = $payloadJson | ConvertFrom-Json

        Write-Host "JWT Token Claims:" -ForegroundColor Green
        $claims.PSObject.Properties | ForEach-Object {
            Write-Host "  $($_.Name): $($_.Value)"
        }
    }

    Write-Host ""

    $results.complainant = @{
        success = $true
        token = $response.token
        userId = $response.userId
        email = $response.email
        roles = $response.roles
    }

} catch {
    Write-Host "FAILED: Login failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errorBody = $reader.ReadToEnd()
        Write-Host "Response: $errorBody" -ForegroundColor Red
    }
    Write-Host ""

    $results.complainant = @{
        success = $false
        error = $_.Exception.Message
    }
}

Write-Host ""

# ============================================
# TEST 2: Handler Login
# ============================================

Write-Host "TEST 2: Handler Login" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow
Write-Host "Email: naveen.chandra@oryggitech.com"
Write-Host "Password: Naveen@12345"
Write-Host ""

$handlerBody = @{
    email = "naveen.chandra@oryggitech.com"
    password = "Naveen@12345"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $handlerBody -ContentType "application/json"

    Write-Host "SUCCESS: Login successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Response Details:" -ForegroundColor Green
    Write-Host "  Token Length: $($response.token.Length)"
    Write-Host "  User ID: $($response.userId)"
    Write-Host "  Email: $($response.email)"
    Write-Host "  Name: $($response.firstName) $($response.lastName)"
    Write-Host "  Roles: $($response.roles -join ', ')"
    Write-Host ""

    # Decode JWT token to show claims
    $tokenParts = $response.token.Split('.')
    if ($tokenParts.Length -ge 2) {
        $payload = $tokenParts[1]
        # Add padding if needed
        while ($payload.Length % 4 -ne 0) {
            $payload += "="
        }
        $payloadJson = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($payload))
        $claims = $payloadJson | ConvertFrom-Json

        Write-Host "JWT Token Claims:" -ForegroundColor Green
        $claims.PSObject.Properties | ForEach-Object {
            Write-Host "  $($_.Name): $($_.Value)"
        }
    }

    Write-Host ""

    $results.handler = @{
        success = $true
        token = $response.token
        userId = $response.userId
        email = $response.email
        roles = $response.roles
    }

} catch {
    Write-Host "FAILED: Login failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errorBody = $reader.ReadToEnd()
        Write-Host "Response: $errorBody" -ForegroundColor Red
    }
    Write-Host ""

    $results.handler = @{
        success = $false
        error = $_.Exception.Message
    }
}

Write-Host ""

# ============================================
# SUMMARY
# ============================================

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "AUTHENTICATION TEST SUMMARY" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$complainantStatus = if ($results.complainant.success) { "PASSED" } else { "FAILED" }
$complainantColor = if ($results.complainant.success) { "Green" } else { "Red" }
Write-Host "Complainant (nav_nainital@yahoo.com): $complainantStatus" -ForegroundColor $complainantColor

$handlerStatus = if ($results.handler.success) { "PASSED" } else { "FAILED" }
$handlerColor = if ($results.handler.success) { "Green" } else { "Red" }
Write-Host "Handler (naveen.chandra@oryggitech.com): $handlerStatus" -ForegroundColor $handlerColor

Write-Host ""

if ($results.complainant.success -and $results.handler.success) {
    Write-Host "ALL TESTS PASSED!" -ForegroundColor Green
    Write-Host "Both users can successfully authenticate." -ForegroundColor Green
} else {
    Write-Host "SOME TESTS FAILED!" -ForegroundColor Red
    Write-Host "Please review the errors above." -ForegroundColor Red
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan

# Save results to file
$results | ConvertTo-Json -Depth 10 | Out-File "authentication-test-results.json"
Write-Host ""
Write-Host "Results saved to: authentication-test-results.json" -ForegroundColor Cyan
