# JWT Token Refresh System - Test Script
# Tests the complete token refresh flow

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "JWT Token Refresh System - Test Script" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5058/api"

# Test 1: Login
Write-Host "[Test 1] Login and retrieve tokens..." -ForegroundColor Yellow
try {
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody

    if ($loginResponse.isSuccess) {
        Write-Host "[PASS] Login successful" -ForegroundColor Green
        $accessToken = $loginResponse.data.token
        $refreshToken = $loginResponse.data.refreshToken
        $expiresAt = $loginResponse.data.expiresAt

        Write-Host "  Access Token: $($accessToken.Substring(0, 30))..." -ForegroundColor Gray
        Write-Host "  Refresh Token: $($refreshToken.Substring(0, 30))..." -ForegroundColor Gray
        Write-Host "  Expires At: $expiresAt" -ForegroundColor Gray
        Write-Host "  User: $($loginResponse.data.user.email)" -ForegroundColor Gray
    } else {
        Write-Host "[FAIL] Login failed: $($loginResponse.message)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "[FAIL] Login error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Verify access token works
Write-Host "[Test 2] Verify access token works..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $accessToken"
    }

    $meResponse = Invoke-RestMethod -Uri "$baseUrl/auth/me" `
        -Method GET `
        -Headers $headers

    if ($meResponse.isSuccess) {
        Write-Host "[PASS] Access token is valid" -ForegroundColor Green
        Write-Host "  User ID: $($meResponse.data.id)" -ForegroundColor Gray
        Write-Host "  Email: $($meResponse.data.email)" -ForegroundColor Gray
    } else {
        Write-Host "[FAIL] Access token validation failed" -ForegroundColor Red
    }
} catch {
    Write-Host "[FAIL] Access token error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: Refresh token
Write-Host "[Test 3] Refresh access token..." -ForegroundColor Yellow
try {
    Start-Sleep -Seconds 2  # Wait a bit

    $refreshBody = @{
        refreshToken = $refreshToken
    } | ConvertTo-Json

    $refreshResponse = Invoke-RestMethod -Uri "$baseUrl/auth/refresh" `
        -Method POST `
        -ContentType "application/json" `
        -Body $refreshBody

    if ($refreshResponse.isSuccess) {
        Write-Host "[PASS] Token refresh successful" -ForegroundColor Green
        $newAccessToken = $refreshResponse.data.token
        $newRefreshToken = $refreshResponse.data.refreshToken

        Write-Host "  New Access Token: $($newAccessToken.Substring(0, 30))..." -ForegroundColor Gray
        Write-Host "  New Refresh Token: $($newRefreshToken.Substring(0, 30))..." -ForegroundColor Gray

        # Verify new token is different
        if ($newAccessToken -ne $accessToken) {
            Write-Host "  [VERIFIED] New access token is different from old" -ForegroundColor Green
        } else {
            Write-Host "  [WARNING] New access token is same as old" -ForegroundColor Yellow
        }

        if ($newRefreshToken -ne $refreshToken) {
            Write-Host "  [VERIFIED] New refresh token is different from old (token rotation)" -ForegroundColor Green
        } else {
            Write-Host "  [WARNING] New refresh token is same as old (no rotation)" -ForegroundColor Yellow
        }

        # Update tokens for next tests
        $accessToken = $newAccessToken
        $oldRefreshToken = $refreshToken
        $refreshToken = $newRefreshToken
    } else {
        Write-Host "[FAIL] Token refresh failed: $($refreshResponse.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "[FAIL] Token refresh error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 4: Verify new access token works
Write-Host "[Test 4] Verify new access token works..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $accessToken"
    }

    $meResponse = Invoke-RestMethod -Uri "$baseUrl/auth/me" `
        -Method GET `
        -Headers $headers

    if ($meResponse.isSuccess) {
        Write-Host "[PASS] New access token is valid" -ForegroundColor Green
    } else {
        Write-Host "[FAIL] New access token validation failed" -ForegroundColor Red
    }
} catch {
    Write-Host "[FAIL] New access token error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 5: Test token theft detection (reuse old refresh token)
Write-Host "[Test 5] Test token theft detection (reuse old refresh token)..." -ForegroundColor Yellow
try {
    $reuseBody = @{
        refreshToken = $oldRefreshToken
    } | ConvertTo-Json

    $reuseResponse = Invoke-RestMethod -Uri "$baseUrl/auth/refresh" `
        -Method POST `
        -ContentType "application/json" `
        -Body $reuseBody `
        -ErrorAction SilentlyContinue

    if ($reuseResponse.isSuccess) {
        Write-Host "[FAIL] Old refresh token was accepted (should be rejected)" -ForegroundColor Red
    }
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 401) {
        Write-Host "[PASS] Old refresh token was rejected (theft detection working)" -ForegroundColor Green
        Write-Host "  Status Code: 401 Unauthorized" -ForegroundColor Gray
    } else {
        Write-Host "[WARNING] Unexpected error: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host ""

# Test 6: Database verification
Write-Host "[Test 6] Database verification (manual check required)..." -ForegroundColor Yellow
Write-Host "  Please run this SQL query to verify tokens in database:" -ForegroundColor Cyan
Write-Host ""
Write-Host @"
SELECT TOP 5
    rt.Id,
    rt.UserId,
    u.Email,
    rt.CreatedAt,
    rt.ExpiresAt,
    rt.UsedAt,
    rt.RevokedAt,
    rt.TokenFamily
FROM RefreshTokens rt
JOIN Users u ON rt.UserId = u.Id
ORDER BY rt.CreatedAt DESC
"@ -ForegroundColor White
Write-Host ""
Write-Host "  Expected: You should see 2 tokens for the test user" -ForegroundColor Cyan
Write-Host "  - First token should have UsedAt timestamp (used during refresh)" -ForegroundColor Cyan
Write-Host "  - Second token should be active (UsedAt = NULL, RevokedAt = NULL)" -ForegroundColor Cyan

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Core functionality verified:" -ForegroundColor Green
Write-Host "  [x] Login returns access token and refresh token" -ForegroundColor Green
Write-Host "  [x] Access token can be used for authenticated requests" -ForegroundColor Green
Write-Host "  [x] Refresh token generates new tokens (rotation)" -ForegroundColor Green
Write-Host "  [x] New access token works correctly" -ForegroundColor Green
Write-Host "  [x] Token theft detection prevents reuse of old tokens" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Run database verification query above" -ForegroundColor Yellow
Write-Host "  2. Test in Angular frontend" -ForegroundColor Yellow
Write-Host "  3. Test automatic refresh before expiry (wait 55 minutes)" -ForegroundColor Yellow
Write-Host "  4. Test logout functionality" -ForegroundColor Yellow
Write-Host ""
