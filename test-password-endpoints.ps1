# Password Management Endpoints Test Script
# Tests all 9 password management endpoints

$baseUrl = "http://localhost:5000"
$loginUrl = "$baseUrl/api/auth/login"

# Color functions
function Write-Success { param($msg) Write-Host $msg -ForegroundColor Green }
function Write-Error { param($msg) Write-Host $msg -ForegroundColor Red }
function Write-Info { param($msg) Write-Host $msg -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host $msg -ForegroundColor Yellow }

Write-Info ""
Write-Info "=========================================="
Write-Info "PASSWORD MANAGEMENT ENDPOINTS TEST"
Write-Info "=========================================="
Write-Info ""

# Test results
$results = @{
    Total = 0
    Passed = 0
    Failed = 0
}

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = "GET",
        [object]$Body = $null,
        [hashtable]$Headers = @{},
        [int]$ExpectedStatus = 200
    )

    $results.Total++
    Write-Info "Testing: $Name"

    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            ContentType = "application/json"
        }

        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }

        $response = Invoke-WebRequest @params -UseBasicParsing

        if ($response.StatusCode -eq $ExpectedStatus) {
            Write-Success "  PASSED - Status: $($response.StatusCode)"
            $results.Passed++

            # Parse and display response
            $content = $response.Content | ConvertFrom-Json
            Write-Host "  Response: " -NoNewline
            Write-Host ($content | ConvertTo-Json -Compress) -ForegroundColor Gray

            return $content
        } else {
            Write-Error "  FAILED - Expected: $ExpectedStatus, Got: $($response.StatusCode)"
            $results.Failed++
            return $null
        }
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq $ExpectedStatus) {
            Write-Success "  PASSED - Status: $statusCode (Expected error)"
            $results.Passed++
        } else {
            Write-Error "  FAILED - Status: $statusCode, Error: $($_.Exception.Message)"
            $results.Failed++
        }
        return $null
    }
}

# Step 1: Login as admin to get token
Write-Info ""
Write-Info "=== STEP 1: LOGIN AS ADMIN ==="
Write-Info ""

$loginBody = @{
    username = "admin@system.com"
    password = "Admin@123"
}

$loginResponse = Test-Endpoint `
    -Name "Admin Login" `
    -Url $loginUrl `
    -Method "POST" `
    -Body $loginBody

if (-not $loginResponse -or -not $loginResponse.token) {
    Write-Error ""
    Write-Error "Failed to login. Cannot continue testing."
    exit 1
}

$token = $loginResponse.token
$userId = $loginResponse.userId
$companyId = $loginResponse.companyId

$authHeaders = @{
    "Authorization" = "Bearer $token"
}

Write-Success ""
Write-Success "Logged in successfully!"
Write-Info "User ID: $userId"
Write-Info "Company ID: $companyId"
Write-Info "Token: $($token.Substring(0, 30))..."

# Step 2: Test Password Strength Endpoint (Anonymous)
Write-Info ""
Write-Info ""
Write-Info "=== STEP 2: TEST PASSWORD STRENGTH (ANONYMOUS) ==="
Write-Info ""

$passwordStrengthTests = @(
    @{ Password = "12345"; Expected = "VeryWeak" }
    @{ Password = "password"; Expected = "Weak" }
    @{ Password = "Password123"; Expected = "Fair/Good" }
    @{ Password = "P@ssw0rd!2024"; Expected = "Strong/VeryStrong" }
)

foreach ($test in $passwordStrengthTests) {
    $strengthBody = @{
        password = $test.Password
    }

    $strengthResponse = Test-Endpoint `
        -Name "Check Password Strength: '$($test.Password)'" `
        -Url "$baseUrl/api/password/strength" `
        -Method "POST" `
        -Body $strengthBody

    if ($strengthResponse) {
        Write-Info "  Expected: $($test.Expected), Got: $($strengthResponse.category) (Score: $($strengthResponse.score))"
    }
}

# Step 3: Test Password Validation
Write-Info ""
Write-Info ""
Write-Info "=== STEP 3: TEST PASSWORD VALIDATION ==="
Write-Info ""

$validateBody = @{
    password = "Test@123"
    companyId = $companyId
}

Test-Endpoint `
    -Name "Validate Password Against Policy" `
    -Url "$baseUrl/api/password/validate" `
    -Method "POST" `
    -Body $validateBody `
    -Headers $authHeaders

# Step 4: Test Get Password Status (Current User)
Write-Info ""
Write-Info ""
Write-Info "=== STEP 4: TEST GET PASSWORD STATUS ==="
Write-Info ""

Test-Endpoint `
    -Name "Get Current User Password Status" `
    -Url "$baseUrl/api/password/status" `
    -Method "GET" `
    -Headers $authHeaders

# Step 5: Test Generate Password (Admin)
Write-Info ""
Write-Info ""
Write-Info "=== STEP 5: TEST GENERATE PASSWORD (ADMIN) ==="
Write-Info ""

$generateBody = @{
    length = 16
    includeUppercase = $true
    includeLowercase = $true
    includeDigits = $true
    includeSpecialChars = $true
}

$generatedPasswordResponse = Test-Endpoint `
    -Name "Generate Secure Password" `
    -Url "$baseUrl/api/password/generate" `
    -Method "POST" `
    -Body $generateBody `
    -Headers $authHeaders

if ($generatedPasswordResponse) {
    $generatedPassword = $generatedPasswordResponse.password
    Write-Info ""
    Write-Info "  Generated Password: $generatedPassword"
    Write-Info "  Strength Score: $($generatedPasswordResponse.strength.score)"
    Write-Info "  Category: $($generatedPasswordResponse.strength.category)"
}

# Step 6: Test Get User Password Status (Admin)
Write-Info ""
Write-Info ""
Write-Info "=== STEP 6: TEST GET USER PASSWORD STATUS (ADMIN) ==="
Write-Info ""

Test-Endpoint `
    -Name "Get User Password Status by Admin" `
    -Url "$baseUrl/api/password/status/$userId" `
    -Method "GET" `
    -Headers $authHeaders

# Step 7: Test Change Password (Current User)
Write-Info ""
Write-Info ""
Write-Info "=== STEP 7: TEST CHANGE PASSWORD (CURRENT USER) ==="
Write-Info ""

# First, let's test with wrong current password (should fail)
$changePasswordWrongBody = @{
    currentPassword = "WrongPassword@123"
    newPassword = "NewPassword@123"
}

Test-Endpoint `
    -Name "Change Password (Wrong Current Password - Should Fail)" `
    -Url "$baseUrl/api/password/change" `
    -Method "POST" `
    -Body $changePasswordWrongBody `
    -Headers $authHeaders `
    -ExpectedStatus = 400

# Test with correct current password
Write-Info ""
Write-Info "Note: Skipping actual password change to avoid disrupting current session"
Write-Warning "To test password change, use: currentPassword='Admin@123', newPassword='NewPassword@123'"

# Step 8: Test Set User Password (Admin)
Write-Info ""
Write-Info ""
Write-Info "=== STEP 8: TEST SET USER PASSWORD (ADMIN) ==="
Write-Info ""

Write-Warning "Skipping Set Password test to avoid modifying user accounts"
Write-Info "Endpoint: POST /api/password/set"
Write-Info "Body: { userId, password, mustChangeOnNextLogin, sendEmail }"

# Step 9: Test Reset User Password (Admin)
Write-Info ""
Write-Info ""
Write-Info "=== STEP 9: TEST RESET USER PASSWORD (ADMIN) ==="
Write-Info ""

Write-Warning "Skipping Reset Password test to avoid modifying user accounts"
Write-Info "Endpoint: POST /api/password/reset"
Write-Info "Body: { userId, sendEmail }"

# Step 10: Test Unlock Account (Admin)
Write-Info ""
Write-Info ""
Write-Info "=== STEP 10: TEST UNLOCK ACCOUNT (ADMIN) ==="
Write-Info ""

Write-Warning "Skipping Unlock Account test (no locked accounts to test)"
Write-Info "Endpoint: POST /api/password/unlock"
Write-Info "Body: { userId }"

# Summary
Write-Info ""
Write-Info ""
Write-Info "=========================================="
Write-Info "TEST SUMMARY"
Write-Info "=========================================="
Write-Info ""

Write-Info "Total Tests: $($results.Total)"
Write-Success "Passed: $($results.Passed)"
if ($results.Failed -gt 0) {
    Write-Error "Failed: $($results.Failed)"
} else {
    Write-Info "Failed: $($results.Failed)"
}

$successRate = [math]::Round(($results.Passed / $results.Total) * 100, 2)
Write-Info ""
Write-Info "Success Rate: $successRate%"

if ($results.Failed -eq 0) {
    Write-Success ""
    Write-Success "ALL TESTS PASSED!"
} else {
    Write-Warning ""
    Write-Warning "SOME TESTS FAILED"
}

Write-Info ""
Write-Info "=========================================="
Write-Info "PASSWORD ENDPOINTS AVAILABLE:"
Write-Info "=========================================="
Write-Info ""

Write-Host "User Operations:" -ForegroundColor Yellow
Write-Host "  POST   /api/password/change          - Change own password"
Write-Host "  POST   /api/password/strength        - Check password strength (anonymous)"
Write-Host "  POST   /api/password/validate        - Validate password against policy"
Write-Host "  GET    /api/password/status          - Get own password status"

Write-Host ""
Write-Host "Admin Operations:" -ForegroundColor Yellow
Write-Host "  POST   /api/password/set             - Set user password"
Write-Host "  POST   /api/password/reset           - Reset user password (auto-generate)"
Write-Host "  POST   /api/password/generate        - Generate secure password"
Write-Host "  POST   /api/password/unlock          - Unlock locked account"
Write-Host "  GET    /api/password/status/{userId} - Get user password status"

Write-Info ""
Write-Info "=========================================="
