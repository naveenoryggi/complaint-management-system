# Comprehensive API Test Script for Email Modules
$baseUrl = "http://localhost:5000"
$results = @()
$testCount = 0
$passCount = 0
$failCount = 0
$warnCount = 0

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
    $color = if ($status -eq "PASS") { "Green" } elseif ($status -eq "FAIL") { "Red" } else { "Yellow" }
    Write-Host "[$status] $Method $Endpoint - $StatusCode - $Notes" -ForegroundColor $color
}

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "EMAIL MODULES API VALIDATION TEST SUITE" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

# Step 1: Authenticate
Write-Host "`n[STEP 1] Authentication" -ForegroundColor Yellow

$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    $companyId = $loginResponse.data.user.companyId

    Write-Host "Authentication successful" -ForegroundColor Green
    Write-Host "Company ID: $companyId" -ForegroundColor Gray

    Log-Test -Endpoint "/api/auth/login" -Method "POST" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Authentication successful"
} catch {
    Write-Host "Authentication failed" -ForegroundColor Red
    Log-Test -Endpoint "/api/auth/login" -Method "POST" -StatusCode 401 -Expected "200" -Result "FAIL" -Notes "Authentication failed"
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Test EmailServerSettingsController
Write-Host "`n[STEP 2] EmailServerSettingsController" -ForegroundColor Yellow

# GET all
Write-Host "`nTest 2.1: GET /api/email-settings" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings" -Method GET -Headers $headers
    $count = $response.data.Count
    Log-Test -Endpoint "/api/email-settings" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved $count settings"
} catch {
    Log-Test -Endpoint "/api/email-settings" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
}

# POST create
Write-Host "`nTest 2.2: POST /api/email-settings" -ForegroundColor White
$newSetting = @{
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
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings" -Method POST -Headers $headers -Body $newSetting
    $createdId = $response.data.id
    Log-Test -Endpoint "/api/email-settings" -Method "POST" -StatusCode 201 -Expected "201" -Result "PASS" -Notes "Created ID: $createdId"
} catch {
    Log-Test -Endpoint "/api/email-settings" -Method "POST" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "201" -Result "FAIL" -Notes $_.Exception.Message
}

# Test EmailConfigurationController
Write-Host "`n[STEP 3] EmailConfigurationController" -ForegroundColor Yellow

Write-Host "`nTest 3.1: GET /api/email-configuration" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-configuration" -Method GET -Headers $headers
    $count = if ($response.data) { $response.data.Count } else { 0 }
    Log-Test -Endpoint "/api/email-configuration" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved $count configurations"
} catch {
    Log-Test -Endpoint "/api/email-configuration" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
}

# Test CommunicationTemplatesController
Write-Host "`n[STEP 4] CommunicationTemplatesController" -ForegroundColor Yellow

Write-Host "`nTest 4.1: GET /api/communication-templates" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/communication-templates" -Method GET -Headers $headers
    $count = $response.Count
    Log-Test -Endpoint "/api/communication-templates" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved $count templates"
} catch {
    Log-Test -Endpoint "/api/communication-templates" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
}

# Test SystemConfigurationController
Write-Host "`n[STEP 5] SystemConfigurationController" -ForegroundColor Yellow

Write-Host "`nTest 5.1: GET /api/SystemConfiguration" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/SystemConfiguration" -Method GET -Headers $headers
    Log-Test -Endpoint "/api/SystemConfiguration" -Method "GET" -StatusCode 200 -Expected "200" -Result "PASS" -Notes "Retrieved system configuration"
} catch {
    Log-Test -Endpoint "/api/SystemConfiguration" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "200" -Result "FAIL" -Notes $_.Exception.Message
}

# Test without token
Write-Host "`n[STEP 6] Security Tests" -ForegroundColor Yellow

Write-Host "`nTest 6.1: GET /api/email-settings (No Token)" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/email-settings" -Method GET
    Log-Test -Endpoint "/api/email-settings (No Token)" -Method "GET" -StatusCode 200 -Expected "401" -Result "FAIL" -Notes "Accessible without auth!"
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 401) {
        Log-Test -Endpoint "/api/email-settings (No Token)" -Method "GET" -StatusCode 401 -Expected "401" -Result "PASS" -Notes "Correctly protected"
    } else {
        Log-Test -Endpoint "/api/email-settings (No Token)" -Method "GET" -StatusCode $_.Exception.Response.StatusCode.value__ -Expected "401" -Result "WARN" -Notes "Unexpected status"
    }
}

# Summary
Write-Host "`n===============================================" -ForegroundColor Cyan
Write-Host "TEST EXECUTION SUMMARY" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Total Tests: $testCount" -ForegroundColor White
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red
Write-Host "Warnings: $warnCount" -ForegroundColor Yellow

$csvPath = "C:\Users\Navin Chandra\Pictures\Complaint management system\api_test_results.csv"
$results | Export-Csv -Path $csvPath -NoTypeInformation
Write-Host "`nResults exported to: $csvPath" -ForegroundColor Cyan

if ($failCount -gt 0) {
    Write-Host "`nFAILED TESTS:" -ForegroundColor Red
    $results | Where-Object { $_.Result -eq "FAIL" } | Format-Table -AutoSize
}

Write-Host "`nTest completed!" -ForegroundColor Cyan
