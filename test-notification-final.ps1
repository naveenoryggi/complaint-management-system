# Notification System Comprehensive Test - FINAL VERSION
# Tests all 3 APIs with correct data formats
# Date: November 10, 2025

$BaseUrl = "http://localhost:5000"
$TestResults = @()

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "NOTIFICATION SYSTEM - COMPREHENSIVE TEST SUITE" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Authenticate
Write-Host "Authenticating..." -ForegroundColor Yellow
try {
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
    Write-Host "SUCCESS: Authenticated (Token length: $($token.Length))" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "FAILED: Authentication failed" -ForegroundColor Red
    exit 1
}

function Test-API {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [bool]$ExpectSuccess = $true
    )

    try {
        $params = @{
            Uri = "$BaseUrl$Endpoint"
            Method = $Method
            Headers = $headers
        }

        if ($Body) {
            $params.Body = $Body | ConvertTo-Json -Depth 10
            $params.ContentType = "application/json"
        }

        $response = Invoke-RestMethod @params -ErrorAction Stop

        if ($ExpectSuccess) {
            Write-Host "  PASS: $Name" -ForegroundColor Green
            $script:TestResults += @{ Test = $Name; Status = "PASS" }
            return $response
        } else {
            Write-Host "  FAIL: $Name - Expected failure but succeeded" -ForegroundColor Red
            $script:TestResults += @{ Test = $Name; Status = "FAIL"; Message = "Expected failure" }
            return $null
        }
    } catch {
        if (!$ExpectSuccess) {
            Write-Host "  PASS: $Name (Expected failure)" -ForegroundColor Green
            $script:TestResults += @{ Test = $Name; Status = "PASS" }
            return $null
        } else {
            Write-Host "  FAIL: $Name - $($_.Exception.Message)" -ForegroundColor Red
            $script:TestResults += @{ Test = $Name; Status = "FAIL"; Message = $_.Exception.Message }
            return $null
        }
    }
}

# =====================================================
# EVENT TYPES API TESTING
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "API 1: EVENT TYPES (/api/event-types)" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: GET all event types
$eventTypes = Test-API "GET all event types" "GET" "/api/event-types"
if ($eventTypes) {
    Write-Host "    Found: $($eventTypes.Count) event types" -ForegroundColor Gray
}

# Test 2: GET entity types
$entityTypes = Test-API "GET entity types list" "GET" "/api/event-types/entity-types"
if ($entityTypes -and $entityTypes.entityTypes) {
    Write-Host "    Entity types: $($entityTypes.entityTypes -join ', ')" -ForegroundColor Gray
}

# Test 3: GET categories
$categories = Test-API "GET categories list" "GET" "/api/event-types/categories"
if ($categories -and $categories.categories) {
    Write-Host "    Categories: $($categories.categories -join ', ')" -ForegroundColor Gray
}

# Test 4: POST create new event type (FIXED - AvailableFields as JSON string)
$newEvent = @{
    name = "Test Notification Event"
    code = "TEST_EVENT_$(Get-Random -Minimum 1000 -Maximum 9999)"
    description = "Test event for notifications"
    entityType = "Complaint"
    category = "Notification"
    isActive = $true
    availableFields = '["ComplaintNumber","Title","Status","Priority"]'  # JSON string
    iconClass = "fa fa-bell"
}
$createdEvent = Test-API "POST create event type" "POST" "/api/event-types" -Body $newEvent
if ($createdEvent) {
    $testEventId = $createdEvent.id
    Write-Host "    Created ID: $testEventId" -ForegroundColor Gray
}

# Test 5: GET by ID
if ($testEventId) {
    $eventById = Test-API "GET event type by ID" "GET" "/api/event-types/$testEventId"
}

# Test 6: PUT update event type
if ($testEventId) {
    $updateEvent = @{
        id = $testEventId
        name = "Test Notification Event (Updated)"
        code = $newEvent.code
        description = "Updated test event"
        entityType = "Complaint"
        category = "Notification"
        isActive = $true
        availableFields = '["ComplaintNumber","Title","Status","Priority","AssignedTo"]'
        iconClass = "fa fa-bell-o"
    }
    $updated = Test-API "PUT update event type" "PUT" "/api/event-types/$testEventId" -Body $updateEvent
}

Write-Host ""

# =====================================================
# COMMUNICATION TEMPLATES API TESTING
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "API 2: COMMUNICATION TEMPLATES (/api/communication-templates)" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Test 7: GET all templates
$templates = Test-API "GET all templates" "GET" "/api/communication-templates"
if ($templates) {
    Write-Host "    Found: $($templates.Count) templates" -ForegroundColor Gray
}

# Test 8: GET by channel
$emailTemplates = Test-API "GET templates by channel (Email)" "GET" "/api/communication-templates?channel=Email"
if ($emailTemplates) {
    Write-Host "    Email templates: $($emailTemplates.Count)" -ForegroundColor Gray
}

# Test 9: POST create new template (FIXED - AvailablePlaceholders as JSON string)
$newTemplate = @{
    name = "Test Email Notification"
    code = "TEST_TEMPLATE_$(Get-Random -Minimum 1000 -Maximum 9999)"
    description = "Test email template"
    channel = 0  # Email
    category = "Notification"
    language = "en"
    subject = "Complaint {{ComplaintNumber}} Update"
    body = "Hello, your complaint {{ComplaintNumber}} status: {{Status}}"
    htmlBody = "<p>Hello, your complaint <strong>{{ComplaintNumber}}</strong> status: <strong>{{Status}}</strong></p>"
    availablePlaceholders = '["ComplaintNumber","Title","Status","Priority"]'  # JSON string
    isActive = $true
    isSystem = $false
}
$createdTemplate = Test-API "POST create template" "POST" "/api/communication-templates" -Body $newTemplate
if ($createdTemplate) {
    $testTemplateId = $createdTemplate.id
    Write-Host "    Created ID: $testTemplateId" -ForegroundColor Gray
}

# Test 10: GET by ID
if ($testTemplateId) {
    $templateById = Test-API "GET template by ID" "GET" "/api/communication-templates/$testTemplateId"
}

# Test 11: PUT update template
if ($testTemplateId) {
    $updateTemplate = @{
        id = $testTemplateId
        name = "Test Email Notification (Updated)"
        code = $newTemplate.code
        description = "Updated template"
        channel = 0
        category = "Notification"
        language = "en"
        subject = "UPDATED: {{ComplaintNumber}}"
        body = "Updated notification"
        htmlBody = "<p>Updated notification</p>"
        availablePlaceholders = '["ComplaintNumber","Title","Status","Priority","AssignedTo"]'
        isActive = $true
        isSystem = $false
    }
    $updatedTemplate = Test-API "PUT update template" "PUT" "/api/communication-templates/$testTemplateId" -Body $updateTemplate
}

# Test 12: POST extract placeholders
$extractRequest = @{
    templateContent = "Hello {{UserName}}, complaint {{ComplaintNumber}} is {{Status}}"
}
$extractResult = Test-API "POST extract placeholders" "POST" "/api/communication-templates/extract-placeholders" -Body $extractRequest
if ($extractResult -and $extractResult.placeholders) {
    Write-Host "    Extracted: $($extractResult.placeholders -join ', ')" -ForegroundColor Gray
}

Write-Host ""

# =====================================================
# EVENT COMMUNICATION RULES API TESTING
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "API 3: EVENT COMMUNICATION RULES (/api/event-communication-rules)" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Test 13: GET all rules
$rules = Test-API "GET all notification rules" "GET" "/api/event-communication-rules"
if ($rules) {
    Write-Host "    Found: $($rules.Count) rules" -ForegroundColor Gray
}

# Test 14: POST create rule
if ($testEventId -and $testTemplateId) {
    $newRule = @{
        name = "Test Auto Notification Rule"
        description = "Automatic email notification"
        eventTypeId = $testEventId
        templateId = $testTemplateId
        channel = 0  # Email
        recipientType = "AssignedUser"
        specificEmails = @()
        specificUserIds = @()
        specificRoleIds = @()
        conditions = '{}'
        priority = 1
        delayMinutes = 0
        isActive = $true
    }
    $createdRule = Test-API "POST create notification rule" "POST" "/api/event-communication-rules" -Body $newRule
    if ($createdRule) {
        $testRuleId = $createdRule.id
        Write-Host "    Created ID: $testRuleId" -ForegroundColor Gray
    }
}

# Test 15: GET by ID
if ($testRuleId) {
    $ruleById = Test-API "GET rule by ID" "GET" "/api/event-communication-rules/$testRuleId"
}

# Test 16: PUT update rule
if ($testRuleId) {
    $updateRule = @{
        id = $testRuleId
        name = "Test Auto Notification Rule (Updated)"
        description = "Updated automatic email"
        eventTypeId = $testEventId
        templateId = $testTemplateId
        channel = 0
        recipientType = "AssignedUser"
        specificEmails = @()
        specificUserIds = @()
        specificRoleIds = @()
        conditions = '{"updated":true}'
        priority = 1
        delayMinutes = 5
        isActive = $true
    }
    $updatedRule = Test-API "PUT update notification rule" "PUT" "/api/event-communication-rules/$testRuleId" -Body $updateRule
}

# Test 17: GET rules for event type
if ($testEventId) {
    $eventRules = Test-API "GET rules for event type" "GET" "/api/event-types/$testEventId/rules"
    if ($eventRules) {
        Write-Host "    Event rules: $($eventRules.Count)" -ForegroundColor Gray
    }
}

# Test 18: DELETE rule
if ($testRuleId) {
    Test-API "DELETE notification rule" "DELETE" "/api/event-communication-rules/$testRuleId"
}

Write-Host ""

# =====================================================
# CLEANUP
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "CLEANUP" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

if ($testTemplateId) {
    Write-Host "Deleting test template..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/communication-templates/$testTemplateId" -Method DELETE -Headers $headers | Out-Null
        Write-Host "  Template deleted" -ForegroundColor Green
    } catch {
        Write-Host "  Template cleanup failed" -ForegroundColor Yellow
    }
}

if ($testEventId) {
    Write-Host "Deleting test event type..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/event-types/$testEventId" -Method DELETE -Headers $headers | Out-Null
        Write-Host "  Event type deleted" -ForegroundColor Green
    } catch {
        Write-Host "  Event type cleanup failed" -ForegroundColor Yellow
    }
}

Write-Host ""

# =====================================================
# RESULTS SUMMARY
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "TEST RESULTS SUMMARY" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

$passed = ($TestResults | Where-Object { $_.Status -eq "PASS" }).Count
$failed = ($TestResults | Where-Object { $_.Status -eq "FAIL" }).Count
$total = $TestResults.Count
$passRate = if ($total -gt 0) { [math]::Round(($passed / $total) * 100, 2) } else { 0 }

Write-Host "Total Tests: $total" -ForegroundColor White
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
Write-Host "Pass Rate: $passRate%" -ForegroundColor $(if ($passRate -eq 100) { "Green" } elseif ($passRate -ge 90) { "Yellow" } else { "Red" })
Write-Host ""

Write-Host "API Endpoint Status:" -ForegroundColor Cyan
Write-Host "  Event Types API:              $(if ($eventTypes) { 'WORKING' } else { 'FAILED' })" -ForegroundColor $(if ($eventTypes) { "Green" } else { "Red" })
Write-Host "  Communication Templates API:  $(if ($templates) { 'WORKING' } else { 'FAILED' })" -ForegroundColor $(if ($templates) { "Green" } else { "Red" })
Write-Host "  Event Communication Rules API: $(if ($rules) { 'WORKING' } else { 'FAILED' })" -ForegroundColor $(if ($rules) { "Green" } else { "Red" })
Write-Host ""

if ($failed -gt 0) {
    Write-Host "Failed Tests:" -ForegroundColor Red
    $TestResults | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
        Write-Host "  - $($_.Test): $($_.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "=====================================================" -ForegroundColor Cyan
if ($passRate -eq 100) {
    Write-Host "SUCCESS: ALL NOTIFICATION SYSTEM APIS WORKING!" -ForegroundColor Green
} elseif ($passRate -ge 80) {
    Write-Host "GOOD: NOTIFICATION SYSTEM MOSTLY FUNCTIONAL" -ForegroundColor Yellow
} else {
    Write-Host "ISSUES FOUND IN NOTIFICATION SYSTEM" -ForegroundColor Red
}
Write-Host "=====================================================" -ForegroundColor Cyan

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$TestResults | ConvertTo-Json | Out-File "notification-test-results-$timestamp.json"
Write-Host ""
Write-Host "Results saved to: notification-test-results-$timestamp.json" -ForegroundColor Cyan
