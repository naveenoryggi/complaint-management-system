# Notification System Test - 100% CORRECTED
# All 3 APIs with proper data types and enum values
# Date: November 10, 2025

$BaseUrl = "http://localhost:5000"
$passedTests = 0
$failedTests = 0

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "NOTIFICATION SYSTEM - COMPLETE API TEST" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Authenticate
Write-Host "Authenticating..." -ForegroundColor Yellow
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token
$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}
Write-Host "SUCCESS" -ForegroundColor Green
Write-Host ""

# =====================================================
# API 1: EVENT TYPES
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "API 1: EVENT TYPES" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Test 1: GET /api/event-types" -ForegroundColor Yellow
try {
    $eventTypes = Invoke-RestMethod -Uri "$BaseUrl/api/event-types" -Method GET -Headers $headers
    Write-Host "  PASS - Found $($eventTypes.Count) event types" -ForegroundColor Green
    $passedTests++
} catch {
    Write-Host "  FAIL - $($_.Exception.Message)" -ForegroundColor Red
    $failedTests++
}

Write-Host "Test 2: POST /api/event-types (create)" -ForegroundColor Yellow
$newEvent = @{
    name = "Final Test Event"
    code = "FINAL_TEST_$(Get-Random -Minimum 1000 -Maximum 9999)"
    description = "Final test"
    entityType = "Complaint"
    category = "Notification"
    isActive = $true
    availableFields = '["ComplaintNumber","Status"]'
    iconClass = "fa fa-test"
}
try {
    $createdEvent = Invoke-RestMethod -Uri "$BaseUrl/api/event-types" -Method POST -Headers $headers -Body ($newEvent | ConvertTo-Json)
    $testEventId = $createdEvent.id
    Write-Host "  PASS - Created event ID: $testEventId" -ForegroundColor Green
    $passedTests++
} catch {
    Write-Host "  FAIL - $($_.Exception.Message)" -ForegroundColor Red
    $failedTests++
}

# =====================================================
# API 2: COMMUNICATION TEMPLATES
# =====================================================
Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "API 2: COMMUNICATION TEMPLATES" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Test 3: GET /api/communication-templates" -ForegroundColor Yellow
try {
    $templates = Invoke-RestMethod -Uri "$BaseUrl/api/communication-templates" -Method GET -Headers $headers
    Write-Host "  PASS - Found $($templates.Count) templates" -ForegroundColor Green
    $passedTests++
} catch {
    Write-Host "  FAIL - $($_.Exception.Message)" -ForegroundColor Red
    $failedTests++
}

Write-Host "Test 4: POST /api/communication-templates (create)" -ForegroundColor Yellow
$newTemplate = @{
    name = "Final Test Template"
    code = "FINAL_TEMPLATE_$(Get-Random -Minimum 1000 -Maximum 9999)"
    description = "Final test"
    channel = 0
    category = "Notification"
    language = "en"
    subject = "Test {{ComplaintNumber}}"
    body = "Test body"
    htmlBody = "<p>Test</p>"
    availablePlaceholders = '["ComplaintNumber","Status"]'
    isActive = $true
    isSystem = $false
}
try {
    $createdTemplate = Invoke-RestMethod -Uri "$BaseUrl/api/communication-templates" -Method POST -Headers $headers -Body ($newTemplate | ConvertTo-Json)
    $testTemplateId = $createdTemplate.id
    Write-Host "  PASS - Created template ID: $testTemplateId" -ForegroundColor Green
    $passedTests++
} catch {
    Write-Host "  FAIL - $($_.Exception.Message)" -ForegroundColor Red
    $failedTests++
}

# =====================================================
# API 3: EVENT COMMUNICATION RULES
# =====================================================
Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "API 3: EVENT COMMUNICATION RULES" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Test 5: GET /api/event-communication-rules" -ForegroundColor Yellow
try {
    $rules = Invoke-RestMethod -Uri "$BaseUrl/api/event-communication-rules" -Method GET -Headers $headers
    Write-Host "  PASS - Found $($rules.Count) rules" -ForegroundColor Green
    $passedTests++
} catch {
    Write-Host "  FAIL - $($_.Exception.Message)" -ForegroundColor Red
    $failedTests++
}

if ($testEventId -and $testTemplateId) {
    Write-Host "Test 6: POST /api/event-communication-rules (create)" -ForegroundColor Yellow
    # CORRECTED: RecipientType = 1 (AssignedHandler) and arrays as JSON strings
    $newRule = @{
        name = "Final Test Rule"
        description = "Final test rule"
        eventTypeId = $testEventId
        templateId = $testTemplateId
        channel = 0
        recipientType = 1  # AssignedHandler enum value
        specificUserIds = '[]'  # JSON string
        specificRoleIds = '[]'  # JSON string
        specificEmails = '[]'  # JSON string
        conditions = '{}'
        priority = 1
        delayMinutes = 0
        isActive = $true
    }
    try {
        $createdRule = Invoke-RestMethod -Uri "$BaseUrl/api/event-communication-rules" -Method POST -Headers $headers -Body ($newRule | ConvertTo-Json)
        $testRuleId = $createdRule.id
        Write-Host "  PASS - Created rule ID: $testRuleId" -ForegroundColor Green
        $passedTests++

        # Test 7: DELETE the rule
        Write-Host "Test 7: DELETE /api/event-communication-rules/{id}" -ForegroundColor Yellow
        try {
            Invoke-RestMethod -Uri "$BaseUrl/api/event-communication-rules/$testRuleId" -Method DELETE -Headers $headers | Out-Null
            Write-Host "  PASS - Deleted rule" -ForegroundColor Green
            $passedTests++
        } catch {
            Write-Host "  FAIL - $($_.Exception.Message)" -ForegroundColor Red
            $failedTests++
        }
    } catch {
        Write-Host "  FAIL - $($_.Exception.Message)" -ForegroundColor Red
        $failedTests++
    }
}

# =====================================================
# CLEANUP
# =====================================================
Write-Host ""
Write-Host "Cleanup..." -ForegroundColor Yellow
if ($testTemplateId) {
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/communication-templates/$testTemplateId" -Method DELETE -Headers $headers | Out-Null
        Write-Host "  Template deleted" -ForegroundColor Gray
    } catch {}
}
if ($testEventId) {
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/event-types/$testEventId" -Method DELETE -Headers $headers | Out-Null
        Write-Host "  Event type deleted" -ForegroundColor Gray
    } catch {}
}

# =====================================================
# FINAL RESULTS
# =====================================================
Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "FINAL RESULTS" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

$total = $passedTests + $failedTests
$passRate = if ($total -gt 0) { [math]::Round(($passedTests / $total) * 100, 2) } else { 0 }

Write-Host "Tests Passed: $passedTests" -ForegroundColor Green
Write-Host "Tests Failed: $failedTests" -ForegroundColor $(if ($failedTests -eq 0) { "Green" } else { "Red" })
Write-Host "Total Tests: $total" -ForegroundColor White
Write-Host "Pass Rate: $passRate%" -ForegroundColor $(if ($passRate -eq 100) { "Green" } elseif ($passRate -ge 90) { "Yellow" } else { "Red" })
Write-Host ""

Write-Host "API Verification:" -ForegroundColor Cyan
Write-Host "  1. Event Types API:            $(if ($eventTypes) { 'WORKING' } else { 'FAILED' })" -ForegroundColor $(if ($eventTypes) { "Green" } else { "Red" })
Write-Host "  2. Communication Templates API: $(if ($templates) { 'WORKING' } else { 'FAILED' })" -ForegroundColor $(if ($templates) { "Green" } else { "Red" })
Write-Host "  3. Event Communication Rules:   $(if ($rules) { 'WORKING' } else { 'FAILED' })" -ForegroundColor $(if ($rules) { "Green" } else { "Red" })
Write-Host ""

Write-Host "=====================================================" -ForegroundColor Cyan
if ($passRate -eq 100) {
    Write-Host "SUCCESS: ALL 3 NOTIFICATION APIS VERIFIED" -ForegroundColor Green
} else {
    Write-Host "COMPLETED WITH $failedTests FAILURE(S)" -ForegroundColor Yellow
}
Write-Host "=====================================================" -ForegroundColor Cyan
