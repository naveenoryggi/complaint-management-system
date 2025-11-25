# Comprehensive Notification System Test Suite - FIXED VERSION
# Tests: Event Types, Communication Templates, and Notification Rules
# Date: November 10, 2025

$BaseUrl = "http://localhost:5000"
$TestResults = @()

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "NOTIFICATION SYSTEM COMPREHENSIVE TEST SUITE" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Get authentication token
Write-Host "Step 1: Authenticating..." -ForegroundColor Yellow
try {
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    # FIX: Token is in data.token, not token
    $token = $loginResponse.data.token
    $headers = @{ Authorization = "Bearer $token" }
    Write-Host "Authentication successful (Token length: $($token.Length))" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

function Test-Endpoint {
    param(
        [string]$Category,
        [string]$TestName,
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [hashtable]$Headers = $headers,
        [bool]$ExpectSuccess = $true
    )

    $result = @{
        Category = $Category
        Test = $TestName
        Status = "FAIL"
        Message = ""
    }

    try {
        $params = @{
            Uri = "$BaseUrl$Endpoint"
            Method = $Method
            Headers = $Headers
            ContentType = "application/json"
        }

        if ($Body) {
            $params.Body = $Body | ConvertTo-Json -Depth 10
        }

        $response = Invoke-RestMethod @params -ErrorAction Stop

        if ($ExpectSuccess) {
            $result.Status = "PASS"
            $result.Message = "Success"
            Write-Host "  PASS: $TestName" -ForegroundColor Green
        } else {
            $result.Status = "FAIL"
            $result.Message = "Expected failure but got success"
            Write-Host "  FAIL: $TestName - Expected failure but got success" -ForegroundColor Red
        }
        $script:TestResults += $result
        return $response
    } catch {
        if (!$ExpectSuccess) {
            $result.Status = "PASS"
            $result.Message = "Expected failure: $($_.Exception.Message)"
            Write-Host "  PASS: $TestName (Expected failure)" -ForegroundColor Green
        } else {
            $result.Status = "FAIL"
            $result.Message = $_.Exception.Message
            Write-Host "  FAIL: $TestName - $($_.Exception.Message)" -ForegroundColor Red
        }
        $script:TestResults += $result
        return $null
    }
}

# =====================================================
# CATEGORY 1: EVENT TYPES TESTING - 6 CORE TESTS
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "CATEGORY 1: EVENT TYPES - Core Functionality" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Get all event types
Write-Host "Test 1: Get All Event Types" -ForegroundColor Yellow
$eventTypes = Test-Endpoint "Event Types" "Get All Event Types" "GET" "/api/event-types"
if ($eventTypes) {
    Write-Host "  Found $($eventTypes.Count) event types" -ForegroundColor Gray
}
Write-Host ""

# Test 2: Get entity types list
Write-Host "Test 2: Get Entity Types List" -ForegroundColor Yellow
$entityTypesList = Test-Endpoint "Event Types" "Get Entity Types" "GET" "/api/event-types/entity-types"
if ($entityTypesList -and $entityTypesList.entityTypes) {
    Write-Host "  Entity types: $($entityTypesList.entityTypes -join ', ')" -ForegroundColor Gray
}
Write-Host ""

# Test 3: Get categories list
Write-Host "Test 3: Get Event Categories List" -ForegroundColor Yellow
$categoriesList = Test-Endpoint "Event Types" "Get Categories" "GET" "/api/event-types/categories"
if ($categoriesList -and $categoriesList.categories) {
    Write-Host "  Categories: $($categoriesList.categories -join ', ')" -ForegroundColor Gray
}
Write-Host ""

# Test 4: Create new event type
Write-Host "Test 4: Create New Event Type" -ForegroundColor Yellow
$newEventType = @{
    name = "Test Notification Event"
    code = "TEST_NOTIF_$(Get-Random -Minimum 1000 -Maximum 9999)"
    description = "Test event type for notification system"
    entityType = "Complaint"
    category = "Notification"
    isActive = $true
    availableFields = @("ComplaintNumber", "Title", "Status")
    iconClass = "fa fa-bell"
}
$createdEvent = Test-Endpoint "Event Types" "Create Event Type" "POST" "/api/event-types" -Body $newEventType
if ($createdEvent) {
    $testEventId = $createdEvent.id
    Write-Host "  Created event type ID: $testEventId" -ForegroundColor Gray
}
Write-Host ""

# Test 5: Get event type by ID
if ($testEventId) {
    Write-Host "Test 5: Get Event Type by ID" -ForegroundColor Yellow
    $eventById = Test-Endpoint "Event Types" "Get by ID" "GET" "/api/event-types/$testEventId"
    if ($eventById) {
        Write-Host "  Retrieved: $($eventById.name)" -ForegroundColor Gray
    }
    Write-Host ""
}

# Test 6: Update event type
if ($testEventId) {
    Write-Host "Test 6: Update Event Type" -ForegroundColor Yellow
    $updateEvent = @{
        id = $testEventId
        name = "Test Notification Event (Updated)"
        code = $newEventType.code
        description = "Updated description"
        entityType = "Complaint"
        category = "Notification"
        isActive = $true
        availableFields = @("ComplaintNumber", "Title", "Status", "Priority")
        iconClass = "fa fa-bell-o"
    }
    $updatedEvent = Test-Endpoint "Event Types" "Update Event Type" "PUT" "/api/event-types/$testEventId" -Body $updateEvent
    Write-Host ""
}

# =====================================================
# CATEGORY 2: COMMUNICATION TEMPLATES - 6 CORE TESTS
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "CATEGORY 2: COMMUNICATION TEMPLATES - Core Functionality" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Test 7: Get all templates
Write-Host "Test 7: Get All Templates" -ForegroundColor Yellow
$templates = Test-Endpoint "Templates" "Get All Templates" "GET" "/api/communication-templates"
if ($templates) {
    Write-Host "  Found $($templates.Count) templates" -ForegroundColor Gray
}
Write-Host ""

# Test 8: Filter by channel
Write-Host "Test 8: Filter Templates by Channel (Email)" -ForegroundColor Yellow
$emailTemplates = Test-Endpoint "Templates" "Filter by Email Channel" "GET" "/api/communication-templates?channel=Email"
if ($emailTemplates) {
    Write-Host "  Found $($emailTemplates.Count) email templates" -ForegroundColor Gray
}
Write-Host ""

# Test 9: Create new template
Write-Host "Test 9: Create New Template" -ForegroundColor Yellow
$newTemplate = @{
    name = "Test Email Template"
    code = "TEST_EMAIL_$(Get-Random -Minimum 1000 -Maximum 9999)"
    description = "Test template for emails"
    channel = 0
    category = "Notification"
    language = "en"
    subject = "Notification: {{Title}}"
    body = "Hello, this is a test notification for {{ComplaintNumber}}"
    htmlBody = "<p>Hello, this is a test notification for <strong>{{ComplaintNumber}}</strong></p>"
    availablePlaceholders = @("ComplaintNumber", "Title", "Status")
    isActive = $true
    isSystem = $false
}
$createdTemplate = Test-Endpoint "Templates" "Create Template" "POST" "/api/communication-templates" -Body $newTemplate
if ($createdTemplate) {
    $testTemplateId = $createdTemplate.id
    Write-Host "  Created template ID: $testTemplateId" -ForegroundColor Gray
}
Write-Host ""

# Test 10: Get template by ID
if ($testTemplateId) {
    Write-Host "Test 10: Get Template by ID" -ForegroundColor Yellow
    $templateById = Test-Endpoint "Templates" "Get by ID" "GET" "/api/communication-templates/$testTemplateId"
    Write-Host ""
}

# Test 11: Update template
if ($testTemplateId) {
    Write-Host "Test 11: Update Template" -ForegroundColor Yellow
    $updateTemplate = @{
        id = $testTemplateId
        name = "Test Email Template (Updated)"
        code = $newTemplate.code
        description = "Updated template"
        channel = 0
        category = "Notification"
        language = "en"
        subject = "Updated: {{Title}}"
        body = "Updated notification for {{ComplaintNumber}}"
        htmlBody = "<p>Updated notification for <strong>{{ComplaintNumber}}</strong></p>"
        availablePlaceholders = @("ComplaintNumber", "Title", "Status", "Priority")
        isActive = $true
        isSystem = $false
    }
    $updatedTemplate = Test-Endpoint "Templates" "Update Template" "PUT" "/api/communication-templates/$testTemplateId" -Body $updateTemplate
    Write-Host ""
}

# Test 12: Extract placeholders
Write-Host "Test 12: Extract Placeholders from Template" -ForegroundColor Yellow
$extractRequest = @{
    templateContent = "Dear {{UserName}}, Complaint {{ComplaintNumber}} is {{Status}}."
}
$extractResult = Test-Endpoint "Templates" "Extract Placeholders" "POST" "/api/communication-templates/extract-placeholders" -Body $extractRequest
if ($extractResult -and $extractResult.placeholders) {
    Write-Host "  Extracted: $($extractResult.placeholders -join ', ')" -ForegroundColor Gray
}
Write-Host ""

# =====================================================
# CATEGORY 3: NOTIFICATION RULES - 6 CORE TESTS
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "CATEGORY 3: NOTIFICATION RULES - Core Functionality" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Test 13: Get all rules
Write-Host "Test 13: Get All Notification Rules" -ForegroundColor Yellow
$rules = Test-Endpoint "Notification Rules" "Get All Rules" "GET" "/api/event-communication-rules"
if ($rules) {
    Write-Host "  Found $($rules.Count) rules" -ForegroundColor Gray
}
Write-Host ""

# Test 14: Create notification rule
if ($testEventId -and $testTemplateId) {
    Write-Host "Test 14: Create Notification Rule" -ForegroundColor Yellow
    $newRule = @{
        name = "Test Auto Notification"
        description = "Send notification on event"
        eventTypeId = $testEventId
        templateId = $testTemplateId
        channel = 0
        recipientType = "AssignedUser"
        specificEmails = @()
        specificUserIds = @()
        specificRoleIds = @()
        conditions = '{}'
        priority = 1
        delayMinutes = 0
        isActive = $true
    }
    $createdRule = Test-Endpoint "Notification Rules" "Create Rule" "POST" "/api/event-communication-rules" -Body $newRule
    if ($createdRule) {
        $testRuleId = $createdRule.id
        Write-Host "  Created rule ID: $testRuleId" -ForegroundColor Gray
    }
    Write-Host ""
}

# Test 15: Get rule by ID
if ($testRuleId) {
    Write-Host "Test 15: Get Rule by ID" -ForegroundColor Yellow
    $ruleById = Test-Endpoint "Notification Rules" "Get by ID" "GET" "/api/event-communication-rules/$testRuleId"
    Write-Host ""
}

# Test 16: Update rule
if ($testRuleId) {
    Write-Host "Test 16: Update Notification Rule" -ForegroundColor Yellow
    $updateRule = @{
        id = $testRuleId
        name = "Test Auto Notification (Updated)"
        description = "Updated notification rule"
        eventTypeId = $testEventId
        templateId = $testTemplateId
        channel = 0
        recipientType = "AssignedUser"
        specificEmails = @()
        specificUserIds = @()
        specificRoleIds = @()
        conditions = '{"updated": true}'
        priority = 1
        delayMinutes = 5
        isActive = $true
    }
    $updatedRule = Test-Endpoint "Notification Rules" "Update Rule" "PUT" "/api/event-communication-rules/$testRuleId" -Body $updateRule
    Write-Host ""
}

# Test 17: Get rules for event type
if ($testEventId) {
    Write-Host "Test 17: Get Rules for Specific Event Type" -ForegroundColor Yellow
    $eventRules = Test-Endpoint "Notification Rules" "Get Event Rules" "GET" "/api/event-types/$testEventId/rules"
    if ($eventRules) {
        Write-Host "  Rules for event: $($eventRules.Count)" -ForegroundColor Gray
    }
    Write-Host ""
}

# Test 18: Delete notification rule
if ($testRuleId) {
    Write-Host "Test 18: Delete Notification Rule" -ForegroundColor Yellow
    Test-Endpoint "Notification Rules" "Delete Rule" "DELETE" "/api/event-communication-rules/$testRuleId"
    Write-Host ""
}

# =====================================================
# CLEANUP
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "CLEANUP" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Delete test template
if ($testTemplateId) {
    Write-Host "Cleaning up test template..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/communication-templates/$testTemplateId" -Method DELETE -Headers $headers | Out-Null
        Write-Host "  Template deleted" -ForegroundColor Green
    } catch {
        Write-Host "  Template cleanup skipped or failed" -ForegroundColor Yellow
    }
}

# Delete test event type
if ($testEventId) {
    Write-Host "Cleaning up test event type..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/event-types/$testEventId" -Method DELETE -Headers $headers | Out-Null
        Write-Host "  Event type deleted" -ForegroundColor Green
    } catch {
        Write-Host "  Event type cleanup skipped or failed" -ForegroundColor Yellow
    }
}

Write-Host ""

# =====================================================
# FINAL RESULTS
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "FINAL TEST RESULTS" -ForegroundColor Cyan
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

# Show results by category
$categories = $TestResults | Group-Object Category
foreach ($cat in $categories) {
    $catPassed = ($cat.Group | Where-Object { $_.Status -eq "PASS" }).Count
    $catTotal = $cat.Group.Count
    $catRate = if ($catTotal -gt 0) { [math]::Round(($catPassed / $catTotal) * 100, 2) } else { 0 }
    Write-Host "$($cat.Name): $catPassed/$catTotal ($catRate%)" -ForegroundColor $(if ($catRate -eq 100) { "Green" } elseif ($catRate -ge 90) { "Yellow" } else { "Red" })
}

# Show failures if any
if ($failed -gt 0) {
    Write-Host ""
    Write-Host "FAILED TESTS:" -ForegroundColor Red
    $TestResults | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
        Write-Host "  - [$($_.Category)] $($_.Test): $($_.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
if ($passRate -eq 100) {
    Write-Host "ALL TESTS PASSED - NOTIFICATION SYSTEM WORKS!" -ForegroundColor Green
} elseif ($passRate -ge 90) {
    Write-Host "MOSTLY PASSING - MINOR ISSUES FOUND" -ForegroundColor Yellow
} else {
    Write-Host "TESTS COMPLETED - ISSUES FOUND" -ForegroundColor Red
}
Write-Host "=====================================================" -ForegroundColor Cyan

# Save results to file
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outputFile = "notification-system-test-results-$timestamp.json"
$TestResults | ConvertTo-Json -Depth 10 | Out-File $outputFile
Write-Host ""
Write-Host "Results saved to: $outputFile" -ForegroundColor Cyan
