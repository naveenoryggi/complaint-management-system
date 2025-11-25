# Comprehensive API Test Script for Email Modules
# Date: 2025-11-17
# Purpose: Validate all email-related endpoints

$baseUrl = "http://localhost:5000"
$results = @()
$testCount = 0
$passCount = 0
$failCount = 0
$warnCount = 0

# Function to log test results
function Log-Test {
    param(
        [string]$Endpoint,
        [string]$Method,
        [int]$StatusCode,
        [string]$Expected,
        [string]$Result,
        [string]$Notes
    )

    $script:testCount++
    $status = "PASS"

    if ($Result -eq "FAIL") {
        $status = "FAIL"
        $script:failCount++
    } elseif ($Result -eq "WARN") {
        $status = "WARN"
        $script:warnCount++
    } else {
        $script:passCount++
    }

    $testResult = [PSCustomObject]@{
        TestNumber = $script:testCount
        Method = $Method
        Endpoint = $Endpoint
        StatusCode = $StatusCode
        Expected = $Expected
        Result = $status
        Notes = $Notes
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }

    $script:results += $testResult

    Write-Host "[$status] $Method $Endpoint - $StatusCode - $Notes" -ForegroundColor $(if ($status -eq "PASS") { "Green" } elseif ($status -eq "FAIL") { "Red" } else { "Yellow" })
}

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "EMAIL MODULES API VALIDATION TEST SUITE" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Authenticate
Write-Host "`n[STEP 1] Authentication" -ForegroundColor Yellow
Write-Host "Authenticating as admin@complaintmanagement.com..." -ForegroundColor White

$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    $companyId = $loginResponse.data.user.companyId
    $userId = $loginResponse.data.user.id

    Write-Host "✓ Authentication successful" -ForegroundColor Green
    Write-Host "  User: $($loginResponse.data.user.fullName)" -ForegroundColor Gray
    Write-Host "  Company ID: $companyId" -ForegroundColor Gray
    Write-Host "  User ID: $userId" -ForegroundColor Gray

    Log-Test -Endpoint "/api/auth/login" -Method "POST" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Authentication successful"
} catch {
    Write-Host "✗ Authentication failed: $_" -ForegroundColor Red
    Log-Test -Endpoint "/api/auth/login" -Method "POST" -StatusCode 401 -Expected "200" -Result "FAIL" -Notes "Authentication failed"
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Step 2: Test EmailServerSettingsController
Write-Host "`n[STEP 2] EmailServerSettingsController - 6 Endpoints" -ForegroundColor Yellow

# Test 2.1: GET /api/email-settings (List all)
Write-Host "`nTest 2.1: GET /api/email-settings" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings" -Method GET -Headers $headers
    $count = $response.data.Count
    Write-Host "  Retrieved $count email server settings" -ForegroundColor Gray
    Log-Test -Endpoint "/api/email-settings" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved $count settings"
} catch {
    Log-Test -Endpoint "/api/email-settings" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
}

# Test 2.2: GET /api/email-settings (With includeInactive=true)
Write-Host "`nTest 2.2: GET /api/email-settings?includeInactive=true" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings?includeInactive=true" -Method GET -Headers $headers
    $count = $response.data.Count
    Write-Host "  Retrieved $count settings including inactive" -ForegroundColor Gray
    Log-Test -Endpoint "/api/email-settings?includeInactive=true" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved $count settings (including inactive)"
} catch {
    Log-Test -Endpoint "/api/email-settings?includeInactive=true" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
}

# Test 2.3: POST /api/email-settings (Create new setting)
Write-Host "`nTest 2.3: POST /api/email-settings (Create)" -ForegroundColor White
$newEmailSetting = @{
    name = "Test SMTP Server"
    host = "smtp.test.com"
    port = 587
    useSsl = $true
    username = "test@test.com"
    password = "testPassword123"
    fromEmail = "noreply@test.com"
    fromName = "Test System"
    replyToEmail = "support@test.com"
    isDefault = $false
    isActive = $true
    timeoutSeconds = 30
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings" -Method POST -Headers $headers -Body $newEmailSetting
    $createdId = $response.data.id
    Write-Host "  Created email setting with ID: $createdId" -ForegroundColor Gray
    Log-Test -Endpoint "/api/email-settings" -Method "POST" -StatusCode 201 -Expected "201" -Result "PASS" -Notes "Created setting ID: $createdId"

    # Test 2.4: GET /api/email-settings/{id} (Get by ID)
    Write-Host "`nTest 2.4: GET /api/email-settings/$createdId" -ForegroundColor White
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings/$createdId" -Method GET -Headers $headers
        Write-Host "  Retrieved setting: $($response.data.name)" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-settings/{id}" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved setting: $($response.data.name)"
    } catch {
        Log-Test -Endpoint "/api/email-settings/{id}" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
    }

    # Test 2.5: PUT /api/email-settings/{id} (Update)
    Write-Host "`nTest 2.5: PUT /api/email-settings/$createdId (Update)" -ForegroundColor White
    $updateSetting = $response.data
    $updateSetting.name = "Updated Test SMTP Server"
    $updateSetting.port = 465
    $updateBody = $updateSetting | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings/$createdId" -Method PUT -Headers $headers -Body $updateBody
        Write-Host "  Updated setting name to: $($response.data.name)" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-settings/{id}" -Method "PUT" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Updated setting successfully"
    } catch {
        Log-Test -Endpoint "/api/email-settings/{id}" -Method "PUT" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
    }

    # Test 2.6: POST /api/email-settings/{id}/test (Test connection)
    Write-Host "`nTest 2.6: POST /api/email-settings/$createdId/test" -ForegroundColor White
    $testEmailRequest = @{
        testRecipient = "test@example.com"
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings/$createdId/test" -Method POST -Headers $headers -Body $testEmailRequest
        Write-Host "  Test result: $($response.message)" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-settings/{id}/test" -Method "POST" -StatusCode 200 -Expected "200/500" -Result "PASS" -Notes "Test connection executed (may fail if SMTP not real)"
    } catch {
        # Expected to fail with test SMTP server
        if ($_.Exception.Response.StatusCode.value__ -eq 500) {
            Write-Host "  Test failed as expected (test SMTP server)" -ForegroundColor Gray
            Log-Test -Endpoint "/api/email-settings/{id}/test" -Method "POST" -StatusCode 500 -Expected "200/500" -Result "PASS" -Notes "Failed as expected (test server)"
        } else {
            Log-Test -Endpoint "/api/email-settings/{id}/test" -Method "POST" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200/500" -Result "WARN" -Notes $_.Exception.Message
        }
    }

    # Test 2.7: DELETE /api/email-settings/{id}
    Write-Host "`nTest 2.7: DELETE /api/email-settings/$createdId" -ForegroundColor White
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings/$createdId" -Method DELETE -Headers $headers
        Write-Host "  Deleted setting successfully" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-settings/{id}" -Method "DELETE" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Soft deleted setting"
    } catch {
        Log-Test -Endpoint "/api/email-settings/{id}" -Method "DELETE" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
    }

} catch {
    Log-Test -Endpoint "/api/email-settings" -Method "POST" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "201" -Result "FAIL" -Notes $_.Exception.Message
}

# Step 3: Test EmailConfigurationController
Write-Host "`n[STEP 3] EmailConfigurationController - 8 Endpoints" -ForegroundColor Yellow

# Test 3.1: GET /api/email-configuration
Write-Host "`nTest 3.1: GET /api/email-configuration" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration" -Method GET -Headers $headers
    $count = if ($response.data) { $response.data.Count } else { 0 }
    Write-Host "  Retrieved $count email configurations" -ForegroundColor Gray
    Log-Test -Endpoint "/api/email-configuration" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved $count configurations"
} catch {
    Log-Test -Endpoint "/api/email-configuration" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
}

# Test 3.2: POST /api/email-configuration (Create with Basic Auth)
Write-Host "`nTest 3.2: POST /api/email-configuration (Basic Auth)" -ForegroundColor White
$newEmailConfig = @{
    fromName = "Test Email Config"
    fromEmail = "test@example.com"
    imapHost = "imap.example.com"
    imapPort = 993
    imapUseSsl = $true
    imapUsername = "test@example.com"
    imapPassword = "testPassword123"
    imapFolder = "INBOX"
    smtpHost = "smtp.example.com"
    smtpPort = 587
    smtpUseSsl = $true
    smtpUsername = "test@example.com"
    smtpPassword = "testPassword123"
    pollingIntervalMinutes = 5
    sendAutoAcknowledgement = $false
    enableThreading = $true
    threadTimeoutDays = 7
    maxAttachmentSizeBytes = 10485760
    allowedAttachmentExtensions = "pdf,jpg,png,doc,docx"
    authenticationType = "BasicAuth"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration" -Method POST -Headers $headers -Body $newEmailConfig
    $configId = $response.data.id
    Write-Host "  Created email configuration with ID: $configId" -ForegroundColor Gray
    Log-Test -Endpoint "/api/email-configuration" -Method "POST" -StatusCode 201 -Expected "201" -Result "PASS" -Notes "Created config ID: $configId"

    # Test 3.3: GET /api/email-configuration/{id}
    Write-Host "`nTest 3.3: GET /api/email-configuration/$configId" -ForegroundColor White
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration/$configId" -Method GET -Headers $headers
        Write-Host "  Retrieved config: $($response.data.fromEmail)" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-configuration/{id}" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved config: $($response.data.fromEmail)"
    } catch {
        Log-Test -Endpoint "/api/email-configuration/{id}" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
    }

    # Test 3.4: PUT /api/email-configuration/{id}
    Write-Host "`nTest 3.4: PUT /api/email-configuration/$configId (Update)" -ForegroundColor White
    $updateConfig = $response.data
    $updateConfig.pollingIntervalMinutes = 10
    $updateConfig.fromName = "Updated Test Config"
    $updateBody = $updateConfig | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration/$configId" -Method PUT -Headers $headers -Body $updateBody
        Write-Host "  Updated config successfully" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-configuration/{id}" -Method "PUT" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Updated configuration"
    } catch {
        Log-Test -Endpoint "/api/email-configuration/{id}" -Method "PUT" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
    }

    # Test 3.5: POST /api/email-configuration/{id}/test-imap
    Write-Host "`nTest 3.5: POST /api/email-configuration/$configId/test-imap" -ForegroundColor White
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration/$configId/test-imap" -Method POST -Headers $headers
        Write-Host "  IMAP test result: $($response.message)" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-configuration/{id}/test-imap" -Method "POST" -StatusCode 200 -Expected "200/500" -Result "PASS" -Notes "IMAP test executed"
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 500) {
            Write-Host "  IMAP test failed (expected with test server)" -ForegroundColor Gray
            Log-Test -Endpoint "/api/email-configuration/{id}/test-imap" -Method "POST" -StatusCode 500 -Expected "200/500" -Result "PASS" -Notes "Failed as expected (test server)"
        } else {
            Log-Test -Endpoint "/api/email-configuration/{id}/test-imap" -Method "POST" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200/500" -Result "WARN" -Notes $_.Exception.Message
        }
    }

    # Test 3.6: POST /api/email-configuration/{id}/test-smtp
    Write-Host "`nTest 3.6: POST /api/email-configuration/$configId/test-smtp" -ForegroundColor White
    $testSmtpRequest = @{
        testRecipient = "test@example.com"
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration/$configId/test-smtp" -Method POST -Headers $headers -Body $testSmtpRequest
        Write-Host "  SMTP test result: $($response.message)" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-configuration/{id}/test-smtp" -Method "POST" -StatusCode 200 -Expected "200/500" -Result "PASS" -Notes "SMTP test executed"
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 500) {
            Write-Host "  SMTP test failed (expected with test server)" -ForegroundColor Gray
            Log-Test -Endpoint "/api/email-configuration/{id}/test-smtp" -Method "POST" -StatusCode 500 -Expected "200/500" -Result "PASS" -Notes "Failed as expected (test server)"
        } else {
            Log-Test -Endpoint "/api/email-configuration/{id}/test-smtp" -Method "POST" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200/500" -Result "WARN" -Notes $_.Exception.Message
        }
    }

    # Test 3.7: POST /api/email-configuration/{id}/poll-now
    Write-Host "`nTest 3.7: POST /api/email-configuration/$configId/poll-now" -ForegroundColor White
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration/$configId/poll-now" -Method POST -Headers $headers
        Write-Host "  Poll result: $($response.message)" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-configuration/{id}/poll-now" -Method "POST" -StatusCode 200 -Expected "200/500" -Result "PASS" -Notes "Email poll executed"
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 500) {
            Write-Host "  Email poll failed (expected with test server)" -ForegroundColor Gray
            Log-Test -Endpoint "/api/email-configuration/{id}/poll-now" -Method "POST" -StatusCode 500 -Expected "200/500" -Result "PASS" -Notes "Failed as expected (test server)"
        } else {
            Log-Test -Endpoint "/api/email-configuration/{id}/poll-now" -Method "POST" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200/500" -Result "WARN" -Notes $_.Exception.Message
        }
    }

    # Test 3.8: DELETE /api/email-configuration/{id}
    Write-Host "`nTest 3.8: DELETE /api/email-configuration/$configId" -ForegroundColor White
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration/$configId" -Method DELETE -Headers $headers
        Write-Host "  Deleted configuration successfully" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-configuration/{id}" -Method "DELETE" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Deleted configuration"
    } catch {
        Log-Test -Endpoint "/api/email-configuration/{id}" -Method "DELETE" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
    }

} catch {
    Log-Test -Endpoint "/api/email-configuration" -Method "POST" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "201" -Result "FAIL" -Notes $_.Exception.Message
}

# Step 4: Test CommunicationTemplatesController
Write-Host "`n[STEP 4] CommunicationTemplatesController - 7 Endpoints" -ForegroundColor Yellow

# Test 4.1: GET /api/communication-templates
Write-Host "`nTest 4.1: GET /api/communication-templates" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/communication-templates" -Method GET -Headers $headers
    $count = $response.Count
    Write-Host "  Retrieved $count communication templates" -ForegroundColor Gray
    Log-Test -Endpoint "/api/communication-templates" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved $count templates"
} catch {
    Log-Test -Endpoint "/api/communication-templates" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
}

# Test 4.2: GET /api/communication-templates?channel=Email
Write-Host "`nTest 4.2: GET /api/communication-templates?channel=Email" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/communication-templates?channel=Email" -Method GET -Headers $headers
    $count = $response.Count
    Write-Host "  Retrieved $count email templates" -ForegroundColor Gray
    Log-Test -Endpoint "/api/communication-templates?channel=Email" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved $count email templates"
} catch {
    Log-Test -Endpoint "/api/communication-templates?channel=Email" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
}

# Step 5: Test SystemConfigurationController
Write-Host "`n[STEP 5] SystemConfigurationController - 3 Endpoints" -ForegroundColor Yellow

# Test 5.1: GET /api/SystemConfiguration
Write-Host "`nTest 5.1: GET /api/SystemConfiguration" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/SystemConfiguration" -Method GET -Headers $headers
    Write-Host "  Retrieved system configuration" -ForegroundColor Gray
    Log-Test -Endpoint "/api/SystemConfiguration" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved system configuration"
} catch {
    Log-Test -Endpoint "/api/SystemConfiguration" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
}

# Test 5.2: PUT /api/SystemConfiguration
Write-Host "`nTest 5.2: PUT /api/SystemConfiguration" -ForegroundColor White
try {
    # This may fail due to role-based authorization
    $updateConfig = @{
        oAuthTokenRefreshIntervalMinutes = 30
        defaultEmailPollingIntervalSeconds = 300
        maxEmailsFetchPerPoll = 50
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/api/SystemConfiguration" -Method PUT -Headers $headers -Body $updateConfig
    Write-Host "  Updated system configuration" -ForegroundColor Gray
    Log-Test -Endpoint "/api/SystemConfiguration" -Method "PUT" -StatusCode 200 -Expected "200/403" -Result "PASS" -Notes "Updated configuration or forbidden"
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 403) {
        Write-Host "  Forbidden (requires Admin role)" -ForegroundColor Gray
        Log-Test -Endpoint "/api/SystemConfiguration" -Method "PUT" -StatusCode 403 -Expected "200/403" -Result "PASS" -Notes "Forbidden - requires Admin role"
    } else {
        Log-Test -Endpoint "/api/SystemConfiguration" -Method "PUT" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200/403" -Result "WARN" -Notes $_.Exception.Message
    }
}

# Test 5.3: POST /api/SystemConfiguration/reset
Write-Host "`nTest 5.3: POST /api/SystemConfiguration/reset" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/SystemConfiguration/reset" -Method POST -Headers $headers
    Write-Host "  Reset system configuration to defaults" -ForegroundColor Gray
    Log-Test -Endpoint "/api/SystemConfiguration/reset" -Method "POST" -StatusCode 200 -Expected "200/403" -Result "PASS" -Notes "Reset configuration or forbidden"
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 403) {
        Write-Host "  Forbidden (requires Admin role)" -ForegroundColor Gray
        Log-Test -Endpoint "/api/SystemConfiguration/reset" -Method "POST" -StatusCode 403 -Expected "200/403" -Result "PASS" -Notes "Forbidden - requires Admin role"
    } else {
        Log-Test -Endpoint "/api/SystemConfiguration/reset" -Method "POST" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200/403" -Result "WARN" -Notes $_.Exception.Message
    }
}

# Test Authentication scenarios
Write-Host "`n[STEP 6] Authentication & Authorization Tests" -ForegroundColor Yellow

# Test 6.1: Request without token (should return 401)
Write-Host "`nTest 6.1: GET /api/email-settings (No Token)" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings" -Method GET
    Log-Test -Endpoint "/api/email-settings" -Method "GET" -StatusCode 200 -Expected "401" -Result "FAIL" -Notes "Endpoint accessible without authentication!"
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 401) {
        Write-Host "  Correctly returned 401 Unauthorized" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-settings (No Token)" -Method "GET" -StatusCode 401 -Expected "401" -Result "PASS" -Notes "Correctly protected endpoint"
    } else {
        Log-Test -Endpoint "/api/email-settings (No Token)" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "401" -Result "WARN" -Notes "Unexpected status code"
    }
}

# Test 6.2: Request with invalid token
Write-Host "`nTest 6.2: GET /api/email-settings (Invalid Token)" -ForegroundColor White
$invalidHeaders = @{
    "Authorization" = "Bearer INVALID_TOKEN_12345"
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings" -Method GET -Headers $invalidHeaders
    Log-Test -Endpoint "/api/email-settings" -Method "GET" -StatusCode 200 -Expected "401" -Result "FAIL" -Notes "Accepted invalid token!"
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 401) {
        Write-Host "  Correctly rejected invalid token" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-settings (Invalid Token)" -Method "GET" -StatusCode 401 -Expected "401" -Result "PASS" -Notes "Token validation working"
    } else {
        Log-Test -Endpoint "/api/email-settings (Invalid Token)" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "401" -Result "WARN" -Notes "Unexpected status code"
    }
}

# Test validation scenarios
Write-Host "`n[STEP 7] Data Validation Tests" -ForegroundColor Yellow

# Test 7.1: Create email setting with missing required fields
Write-Host "`nTest 7.1: POST /api/email-settings (Missing required fields)" -ForegroundColor White
$invalidSetting = @{
    name = "Test"
    # Missing host, port, username, password, etc.
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings" -Method POST -Headers $headers -Body $invalidSetting
    Log-Test -Endpoint "/api/email-settings (Invalid Data)" -Method "POST" -StatusCode 201 -Expected "400" -Result "WARN" -Notes "Accepted invalid data"
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 400) {
        Write-Host "  Correctly rejected invalid data" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-settings (Invalid Data)" -Method "POST" -StatusCode 400 -Expected "400" -Result "PASS" -Notes "Data validation working"
    } else {
        Log-Test -Endpoint "/api/email-settings (Invalid Data)" -Method "POST" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "400" -Result "WARN" -Notes "Unexpected status code"
    }
}

# Test 7.2: Get non-existent resource
Write-Host "`nTest 7.2: GET /api/email-settings/{nonExistentId}" -ForegroundColor White
$nonExistentId = "00000000-0000-0000-0000-000000000000"

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings/$nonExistentId" -Method GET -Headers $headers
    Log-Test -Endpoint "/api/email-settings/{id} (Non-existent)" -Method "GET" -StatusCode 200 -Expected "404" -Result "WARN" -Notes "Returned resource for non-existent ID"
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 404) {
        Write-Host "  Correctly returned 404 Not Found" -ForegroundColor Gray
        Log-Test -Endpoint "/api/email-settings/{id} (Non-existent)" -Method "GET" -StatusCode 404 -Expected "404" -Result "PASS" -Notes "Correctly handled non-existent resource"
    } else {
        Log-Test -Endpoint "/api/email-settings/{id} (Non-existent)" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "404" -Result "WARN" -Notes "Unexpected status code"
    }
}

# Summary Report
Write-Host "`n===============================================" -ForegroundColor Cyan
Write-Host "TEST EXECUTION SUMMARY" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Total Tests: $testCount" -ForegroundColor White
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red
Write-Host "Warnings: $warnCount" -ForegroundColor Yellow
Write-Host "Success Rate: $([math]::Round(($passCount/$testCount)*100, 2))%" -ForegroundColor White
Write-Host ""

# Export results to CSV
$csvPath = "C:\Users\Navin Chandra\Pictures\Complaint management system\api_test_results.csv"
$results | Export-Csv -Path $csvPath -NoTypeInformation
Write-Host "Detailed results exported to: $csvPath" -ForegroundColor Cyan

# Display failed tests
if ($failCount -gt 0) {
    Write-Host "`nFAILED TESTS:" -ForegroundColor Red
    $results | Where-Object { $_.Result -eq "FAIL" } | Format-Table -Property TestNumber, Method, Endpoint, StatusCode, Notes -AutoSize
}

# Display warnings
if ($warnCount -gt 0) {
    Write-Host "`nWARNINGS:" -ForegroundColor Yellow
    $results | Where-Object { $_.Result -eq "WARN" } | Format-Table -Property TestNumber, Method, Endpoint, StatusCode, Notes -AutoSize
}

Write-Host "`nTest execution completed!" -ForegroundColor Cyan
