# Comprehensive Notification System Test Suite
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
    $token = $loginResponse.token
    $headers = @{ Authorization = "Bearer $token" }
    Write-Host "✓ Authentication successful" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "✗ Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
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
            Write-Host "  ✓ $TestName" -ForegroundColor Green
        } else {
            $result.Status = "FAIL"
            $result.Message = "Expected failure but got success"
            Write-Host "  ✗ $TestName - Expected failure but got success" -ForegroundColor Red
        }
        $script:TestResults += $result
        return $response
    } catch {
        if (!$ExpectSuccess) {
            $result.Status = "PASS"
            $result.Message = "Expected failure: $($_.Exception.Message)"
            Write-Host "  ✓ $TestName (Expected failure)" -ForegroundColor Green
        } else {
            $result.Status = "FAIL"
            $result.Message = $_.Exception.Message
            Write-Host "  ✗ $TestName - $($_.Exception.Message)" -ForegroundColor Red
        }
        $script:TestResults += $result
        return $null
    }
}

# =====================================================
# CATEGORY 1: EVENT TYPES TESTING (14 tests)
# =====================================================
Write-Host "=====================================================  " -ForegroundColor Cyan
Write-Host "CATEGORY 1: EVENT TYPES (14 tests)" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Get all event types
Write-Host "Test 1: Get All Event Types" -ForegroundColor Yellow
$eventTypes = Test-Endpoint "Event Types" "Get All Event Types" "GET" "/api/event-types"
Write-Host "  Found $($eventTypes.Count) event types" -ForegroundColor Gray
Write-Host ""

# Test 2: Get event types by entity type
Write-Host "Test 2: Filter by Entity Type (Complaint)" -ForegroundColor Yellow
$complaintEvents = Test-Endpoint "Event Types" "Filter by Entity Type" "GET" "/api/event-types?entityType=Complaint"
Write-Host "  Found $($complaintEvents.Count) complaint events" -ForegroundColor Gray
Write-Host ""

# Test 3: Get event types by category
Write-Host "Test 3: Filter by Category" -ForegroundColor Yellow
$statusEvents = Test-Endpoint "Event Types" "Filter by Category" "GET" "/api/event-types?category=StatusChange"
Write-Host ""

# Test 4: Get entity types list
Write-Host "Test 4: Get Entity Types List" -ForegroundColor Yellow
$entityTypesList = Test-Endpoint "Event Types" "Get Entity Types" "GET" "/api/event-types/entity-types"
Write-Host "  Entity types: $($entityTypesList.entityTypes -join ', ')" -ForegroundColor Gray
Write-Host ""

# Test 5: Get categories list
Write-Host "Test 5: Get Event Categories List" -ForegroundColor Yellow
$categoriesList = Test-Endpoint "Event Types" "Get Categories" "GET" "/api/event-types/categories"
Write-Host "  Categories: $($categoriesList.categories -join ', ')" -ForegroundColor Gray
Write-Host ""

# Test 6: Create new event type
Write-Host "Test 6: Create New Event Type" -ForegroundColor Yellow
$newEventType = @{
    name = "Test Auto Email Notification"
    code = "TEST_AUTO_EMAIL_$(Get-Random -Minimum 1000 -Maximum 9999)"
    description = "Test event type for automated email notifications"
    entityType = "Complaint"
    category = "Notification"
    isActive = $true
    availableFields = @("ComplaintNumber", "Title", "AssignedTo", "Status")
    iconClass = "fa fa-envelope"
}
$createdEvent = Test-Endpoint "Event Types" "Create Event Type" "POST" "/api/event-types" -Body $newEventType
$testEventId = $createdEvent.id
Write-Host "  Created event type ID: $testEventId" -ForegroundColor Gray
Write-Host ""

# Test 7: Get event type by ID
Write-Host "Test 7: Get Event Type by ID" -ForegroundColor Yellow
$eventById = Test-Endpoint "Event Types" "Get by ID" "GET" "/api/event-types/$testEventId"
Write-Host "  Retrieved: $($eventById.name)" -ForegroundColor Gray
Write-Host ""

# Test 8: Get event type by code
Write-Host "Test 8: Get Event Type by Code" -ForegroundColor Yellow
$eventByCode = Test-Endpoint "Event Types" "Get by Code" "GET" "/api/event-types/by-code/$($newEventType.code)"
Write-Host ""

# Test 9: Update event type
Write-Host "Test 9: Update Event Type" -ForegroundColor Yellow
$updateEvent = @{
    id = $testEventId
    name = "Test Auto Email Notification (Updated)"
    code = $newEventType.code
    description = "Updated description for test event"
    entityType = "Complaint"
    category = "Notification"
    isActive = $true
    availableFields = @("ComplaintNumber", "Title", "AssignedTo", "Status", "Priority")
    iconClass = "fa fa-envelope-open"
}
$updatedEvent = Test-Endpoint "Event Types" "Update Event Type" "PUT" "/api/event-types/$testEventId" -Body $updateEvent
Write-Host ""

# Test 10: Get rules for event type (should be empty initially)
Write-Host "Test 10: Get Rules for Event Type" -ForegroundColor Yellow
$eventRules = Test-Endpoint "Event Types" "Get Event Rules" "GET" "/api/event-types/$testEventId/rules"
Write-Host "  Rules count: $($eventRules.Count)" -ForegroundColor Gray
Write-Host ""

# Test 11: Validation - Create event type without name
Write-Host "Test 11: Validation - Missing Name" -ForegroundColor Yellow
$invalidEvent1 = @{
    code = "TEST_INVALID"
    entityType = "Complaint"
}
Test-Endpoint "Event Types" "Validation: Missing Name" "POST" "/api/event-types" -Body $invalidEvent1 -ExpectSuccess $false
Write-Host ""

# Test 12: Validation - Create event type without code
Write-Host "Test 12: Validation - Missing Code" -ForegroundColor Yellow
$invalidEvent2 = @{
    name = "Invalid Event"
    entityType = "Complaint"
}
Test-Endpoint "Event Types" "Validation: Missing Code" "POST" "/api/event-types" -Body $invalidEvent2 -ExpectSuccess $false
Write-Host ""

# Test 13: Validation - Create event type without entity type
Write-Host "Test 13: Validation - Missing Entity Type" -ForegroundColor Yellow
$invalidEvent3 = @{
    name = "Invalid Event"
    code = "TEST_INVALID"
}
Test-Endpoint "Event Types" "Validation: Missing Entity Type" "POST" "/api/event-types" -Body $invalidEvent3 -ExpectSuccess $false
Write-Host ""

# Test 14: Get inactive event types
Write-Host "Test 14: Get Inactive Event Types" -ForegroundColor Yellow
$inactiveEvents = Test-Endpoint "Event Types" "Get Inactive Events" "GET" "/api/event-types?includeInactive=true"
Write-Host ""

# =====================================================
# CATEGORY 2: COMMUNICATION TEMPLATES (14 tests)
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "CATEGORY 2: COMMUNICATION TEMPLATES (14 tests)" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Test 15: Get all templates
Write-Host "Test 15: Get All Templates" -ForegroundColor Yellow
$templates = Test-Endpoint "Templates" "Get All Templates" "GET" "/api/communication-templates"
Write-Host "  Found $($templates.Count) templates" -ForegroundColor Gray
Write-Host ""

# Test 16: Filter templates by channel (Email)
Write-Host "Test 16: Filter by Channel (Email)" -ForegroundColor Yellow
$emailTemplates = Test-Endpoint "Templates" "Filter by Email Channel" "GET" "/api/communication-templates?channel=Email"
Write-Host "  Found $($emailTemplates.Count) email templates" -ForegroundColor Gray
Write-Host ""

# Test 17: Filter templates by channel (SMS)
Write-Host "Test 17: Filter by Channel (SMS)" -ForegroundColor Yellow
$smsTemplates = Test-Endpoint "Templates" "Filter by SMS Channel" "GET" "/api/communication-templates?channel=SMS"
Write-Host ""

# Test 18: Filter templates by channel (WhatsApp)
Write-Host "Test 18: Filter by Channel (WhatsApp)" -ForegroundColor Yellow
$whatsappTemplates = Test-Endpoint "Templates" "Filter by WhatsApp Channel" "GET" "/api/communication-templates?channel=WhatsApp"
Write-Host ""

# Test 19: Create new template
Write-Host "Test 19: Create New Template" -ForegroundColor Yellow
$newTemplate = @{
    name = "Test Notification Template"
    code = "TEST_NOTIF_$(Get-Random -Minimum 1000 -Maximum 9999)"
    description = "Test template for notifications"
    channel = 0  # Email
    category = "Notification"
    language = "en"
    subject = "Complaint {{ComplaintNumber}} - {{Title}}"
    body = "Dear {{AssignedTo}}, Your complaint {{ComplaintNumber}} has been updated. Current status: {{Status}}. Please review and take action."
    htmlBody = "<p>Dear {{AssignedTo}},</p><p>Your complaint <strong>{{ComplaintNumber}}</strong> has been updated.</p><p>Current status: <strong>{{Status}}</strong></p><p>Please review and take action.</p>"
    availablePlaceholders = @("ComplaintNumber", "Title", "AssignedTo", "Status", "Priority")
    isActive = $true
    isSystem = $false
}
$createdTemplate = Test-Endpoint "Templates" "Create Template" "POST" "/api/communication-templates" -Body $newTemplate
$testTemplateId = $createdTemplate.id
Write-Host "  Created template ID: $testTemplateId" -ForegroundColor Gray
Write-Host ""

# Test 20: Get template by ID
Write-Host "Test 20: Get Template by ID" -ForegroundColor Yellow
$templateById = Test-Endpoint "Templates" "Get by ID" "GET" "/api/communication-templates/$testTemplateId"
Write-Host ""

# Test 21: Get template by code
Write-Host "Test 21: Get Template by Code" -ForegroundColor Yellow
$templateByCode = Test-Endpoint "Templates" "Get by Code" "GET" "/api/communication-templates/by-code/$($newTemplate.code)"
Write-Host ""

# Test 22: Update template
Write-Host "Test 22: Update Template" -ForegroundColor Yellow
$updateTemplate = @{
    id = $testTemplateId
    name = "Test Notification Template (Updated)"
    code = $newTemplate.code
    description = "Updated test template"
    channel = 0
    category = "Notification"
    language = "en"
    subject = "UPDATED: Complaint {{ComplaintNumber}} - {{Title}}"
    body = "UPDATED: Dear {{AssignedTo}}, Your complaint {{ComplaintNumber}} status: {{Status}}"
    htmlBody = "<p><strong>UPDATED:</strong> Dear {{AssignedTo}},</p><p>Complaint: {{ComplaintNumber}}</p>"
    availablePlaceholders = @("ComplaintNumber", "Title", "AssignedTo", "Status", "Priority", "CreatedDate")
    isActive = $true
    isSystem = $false
}
$updatedTemplate = Test-Endpoint "Templates" "Update Template" "PUT" "/api/communication-templates/$testTemplateId" -Body $updateTemplate
Write-Host ""

# Test 23: Validate template content
Write-Host "Test 23: Validate Template Content" -ForegroundColor Yellow
$validateRequest = @{
    templateContent = "Hello {{Name}}, your {{Item}} is {{Status}}. {{InvalidPlaceholder}}"
}
$validateResult = Test-Endpoint "Templates" "Validate Template" "POST" "/api/communication-templates/validate" -Body $validateRequest
Write-Host ""

# Test 24: Extract placeholders
Write-Host "Test 24: Extract Placeholders" -ForegroundColor Yellow
$extractRequest = @{
    templateContent = "Dear {{UserName}}, Complaint {{ComplaintNumber}} with priority {{Priority}} is now {{Status}}."
}
$extractResult = Test-Endpoint "Templates" "Extract Placeholders" "POST" "/api/communication-templates/extract-placeholders" -Body $extractRequest
Write-Host "  Extracted placeholders: $($extractResult.placeholders -join ', ')" -ForegroundColor Gray
Write-Host ""

# Test 25: Validation - Create template without name
Write-Host "Test 25: Validation - Missing Name" -ForegroundColor Yellow
$invalidTemplate = @{
    code = "TEST_INVALID"
    channel = 0
}
Test-Endpoint "Templates" "Validation: Missing Name" "POST" "/api/communication-templates" -Body $invalidTemplate -ExpectSuccess $false
Write-Host ""

# Test 26: Validation - Duplicate code
Write-Host "Test 26: Validation - Duplicate Code" -ForegroundColor Yellow
$duplicateTemplate = @{
    name = "Duplicate Test"
    code = $newTemplate.code  # Use same code
    channel = 0
    subject = "Test"
    body = "Test"
}
Test-Endpoint "Templates" "Validation: Duplicate Code" "POST" "/api/communication-templates" -Body $duplicateTemplate -ExpectSuccess $false
Write-Host ""

# Test 27: Get inactive templates
Write-Host "Test 27: Get Inactive Templates" -ForegroundColor Yellow
$inactiveTemplates = Test-Endpoint "Templates" "Get Inactive Templates" "GET" "/api/communication-templates?includeInactive=true"
Write-Host ""

# Test 28: Try to modify system template (should fail)
Write-Host "Test 28: Try to Modify System Template" -ForegroundColor Yellow
if ($templates.Count -gt 0) {
    $systemTemplate = $templates | Where-Object { $_.isSystem -eq $true } | Select-Object -First 1
    if ($systemTemplate) {
        $systemUpdate = @{
            id = $systemTemplate.id
            name = "Modified System Template"
            code = $systemTemplate.code
            channel = $systemTemplate.channel
            subject = "Modified"
            body = "Modified"
            isSystem = $true
        }
        Test-Endpoint "Templates" "Validation: Modify System Template" "PUT" "/api/communication-templates/$($systemTemplate.id)" -Body $systemUpdate -ExpectSuccess $false
    } else {
        Write-Host "  ⊘ Skipped - No system templates found" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⊘ Skipped - No templates available" -ForegroundColor Yellow
}
Write-Host ""

# =====================================================
# CATEGORY 3: NOTIFICATION RULES (18 tests)
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "CATEGORY 3: NOTIFICATION RULES (18 tests)" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Test 29: Get all rules
Write-Host "Test 29: Get All Notification Rules" -ForegroundColor Yellow
$rules = Test-Endpoint "Notification Rules" "Get All Rules" "GET" "/api/event-communication-rules"
Write-Host "  Found $($rules.Count) rules" -ForegroundColor Gray
Write-Host ""

# Test 30: Filter rules by event type
Write-Host "Test 30: Filter by Event Type" -ForegroundColor Yellow
$eventRules2 = Test-Endpoint "Notification Rules" "Filter by Event Type" "GET" "/api/event-communication-rules?eventTypeId=$testEventId"
Write-Host ""

# Test 31: Create notification rule
Write-Host "Test 31: Create Notification Rule" -ForegroundColor Yellow
$newRule = @{
    name = "Auto Email on Status Change"
    description = "Send email when complaint status changes"
    eventTypeId = $testEventId
    templateId = $testTemplateId
    channel = 0  # Email
    recipientType = "AssignedUser"
    specificEmails = @()
    specificUserIds = @()
    specificRoleIds = @()
    conditions = '{"statusChanged": true}'
    priority = 1
    delayMinutes = 0
    isActive = $true
}
$createdRule = Test-Endpoint "Notification Rules" "Create Rule" "POST" "/api/event-communication-rules" -Body $newRule
$testRuleId = $createdRule.id
Write-Host "  Created rule ID: $testRuleId" -ForegroundColor Gray
Write-Host ""

# Test 32: Get rule by ID
Write-Host "Test 32: Get Rule by ID" -ForegroundColor Yellow
$ruleById = Test-Endpoint "Notification Rules" "Get by ID" "GET" "/api/event-communication-rules/$testRuleId"
Write-Host ""

# Test 33: Create rule with specific emails
Write-Host "Test 33: Create Rule with Specific Emails" -ForegroundColor Yellow
$emailRule = @{
    name = "Email to Specific Users"
    description = "Send email to specific email addresses"
    eventTypeId = $testEventId
    templateId = $testTemplateId
    channel = 0
    recipientType = "SpecificEmails"
    specificEmails = @("admin@test.com", "manager@test.com")
    specificUserIds = @()
    specificRoleIds = @()
    conditions = '{}'
    priority = 2
    delayMinutes = 0
    isActive = $true
}
$emailRuleCreated = Test-Endpoint "Notification Rules" "Create Email Rule" "POST" "/api/event-communication-rules" -Body $emailRule
$emailRuleId = $emailRuleCreated.id
Write-Host ""

# Test 34: Create rule with specific roles
Write-Host "Test 34: Create Rule with Specific Roles" -ForegroundColor Yellow
# Get a role ID first
$rolesResponse = Invoke-RestMethod -Uri "$BaseUrl/api/roles" -Method GET -Headers $headers
$roleId = $rolesResponse[0].id

$roleRule = @{
    name = "Notify Admin Role"
    description = "Send notification to admin role"
    eventTypeId = $testEventId
    templateId = $testTemplateId
    channel = 0
    recipientType = "SpecificRoles"
    specificEmails = @()
    specificUserIds = @()
    specificRoleIds = @($roleId)
    conditions = '{}'
    priority = 3
    delayMinutes = 0
    isActive = $true
}
$roleRuleCreated = Test-Endpoint "Notification Rules" "Create Role Rule" "POST" "/api/event-communication-rules" -Body $roleRule
$roleRuleId = $roleRuleCreated.id
Write-Host ""

# Test 35: Create rule with delay
Write-Host "Test 35: Create Rule with Delay" -ForegroundColor Yellow
$delayRule = @{
    name = "Delayed Notification"
    description = "Send notification after 30 minutes"
    eventTypeId = $testEventId
    templateId = $testTemplateId
    channel = 0
    recipientType = "AssignedUser"
    specificEmails = @()
    specificUserIds = @()
    specificRoleIds = @()
    conditions = '{}'
    priority = 4
    delayMinutes = 30
    isActive = $true
}
$delayRuleCreated = Test-Endpoint "Notification Rules" "Create Delayed Rule" "POST" "/api/event-communication-rules" -Body $delayRule
$delayRuleId = $delayRuleCreated.id
Write-Host ""

# Test 36: Update rule
Write-Host "Test 36: Update Notification Rule" -ForegroundColor Yellow
$updateRule = @{
    id = $testRuleId
    name = "Auto Email on Status Change (Updated)"
    description = "Updated: Send email when complaint status changes"
    eventTypeId = $testEventId
    templateId = $testTemplateId
    channel = 0
    recipientType = "AssignedUser"
    specificEmails = @()
    specificUserIds = @()
    specificRoleIds = @()
    conditions = '{"statusChanged": true, "priorityHigh": true}'
    priority = 1
    delayMinutes = 5
    isActive = $true
}
$updatedRule = Test-Endpoint "Notification Rules" "Update Rule" "PUT" "/api/event-communication-rules/$testRuleId" -Body $updateRule
Write-Host ""

# Test 37: Reorder rules
Write-Host "Test 37: Reorder Rules" -ForegroundColor Yellow
$reorderRequest = @{
    rules = @(
        @{ id = $testRuleId; priority = 10 },
        @{ id = $emailRuleId; priority = 20 },
        @{ id = $roleRuleId; priority = 30 },
        @{ id = $delayRuleId; priority = 40 }
    )
}
$reorderResult = Test-Endpoint "Notification Rules" "Reorder Rules" "POST" "/api/event-communication-rules/reorder" -Body $reorderRequest
Write-Host ""

# Test 38: Verify rules after reordering
Write-Host "Test 38: Verify Rules After Reorder" -ForegroundColor Yellow
$reorderedRules = Test-Endpoint "Notification Rules" "Verify Reordered Rules" "GET" "/api/event-communication-rules?eventTypeId=$testEventId"
Write-Host "  Rules count: $($reorderedRules.Count)" -ForegroundColor Gray
Write-Host ""

# Test 39: Get inactive rules
Write-Host "Test 39: Get Inactive Rules" -ForegroundColor Yellow
$inactiveRules = Test-Endpoint "Notification Rules" "Get Inactive Rules" "GET" "/api/event-communication-rules?includeInactive=true"
Write-Host ""

# Test 40: Validation - Create rule with invalid event type
Write-Host "Test 40: Validation - Invalid Event Type" -ForegroundColor Yellow
$invalidRule1 = @{
    name = "Invalid Rule"
    eventTypeId = [Guid]::NewGuid()
    channel = 0
    recipientType = "AssignedUser"
}
Test-Endpoint "Notification Rules" "Validation: Invalid Event Type" "POST" "/api/event-communication-rules" -Body $invalidRule1 -ExpectSuccess $false
Write-Host ""

# Test 41: Validation - Create rule with invalid template
Write-Host "Test 41: Validation - Invalid Template" -ForegroundColor Yellow
$invalidRule2 = @{
    name = "Invalid Rule"
    eventTypeId = $testEventId
    templateId = [Guid]::NewGuid()
    channel = 0
    recipientType = "AssignedUser"
}
Test-Endpoint "Notification Rules" "Validation: Invalid Template" "POST" "/api/event-communication-rules" -Body $invalidRule2 -ExpectSuccess $false
Write-Host ""

# Test 42: Validation - Update with ID mismatch
Write-Host "Test 42: Validation - ID Mismatch" -ForegroundColor Yellow
$mismatchUpdate = @{
    id = [Guid]::NewGuid()  # Different ID
    name = "Mismatch Test"
    eventTypeId = $testEventId
    channel = 0
    recipientType = "AssignedUser"
}
Test-Endpoint "Notification Rules" "Validation: ID Mismatch" "PUT" "/api/event-communication-rules/$testRuleId" -Body $mismatchUpdate -ExpectSuccess $false
Write-Host ""

# Test 43: Get rules for specific event
Write-Host "Test 43: Get Rules for Specific Event" -ForegroundColor Yellow
$specificEventRules = Test-Endpoint "Notification Rules" "Get Rules for Event" "GET" "/api/event-types/$testEventId/rules"
Write-Host "  Rules for event: $($specificEventRules.Count)" -ForegroundColor Gray
Write-Host ""

# Test 44: SMS channel rule
Write-Host "Test 44: Create SMS Channel Rule" -ForegroundColor Yellow
$smsRule = @{
    name = "SMS Notification"
    description = "Send SMS on urgent complaints"
    eventTypeId = $testEventId
    templateId = $testTemplateId
    channel = 1  # SMS
    recipientType = "AssignedUser"
    specificEmails = @()
    specificUserIds = @()
    specificRoleIds = @()
    conditions = '{"priority": "Urgent"}'
    priority = 5
    delayMinutes = 0
    isActive = $true
}
$smsRuleCreated = Test-Endpoint "Notification Rules" "Create SMS Rule" "POST" "/api/event-communication-rules" -Body $smsRule
Write-Host ""

# Test 45: WhatsApp channel rule
Write-Host "Test 45: Create WhatsApp Channel Rule" -ForegroundColor Yellow
$whatsappRule = @{
    name = "WhatsApp Notification"
    description = "Send WhatsApp message on critical complaints"
    eventTypeId = $testEventId
    templateId = $testTemplateId
    channel = 2  # WhatsApp
    recipientType = "AssignedUser"
    specificEmails = @()
    specificUserIds = @()
    specificRoleIds = @()
    conditions = '{"priority": "Critical"}'
    priority = 6
    delayMinutes = 0
    isActive = $true
}
$whatsappRuleCreated = Test-Endpoint "Notification Rules" "Create WhatsApp Rule" "POST" "/api/event-communication-rules" -Body $whatsappRule
Write-Host ""

# Test 46: Delete notification rules
Write-Host "Test 46: Delete Notification Rules" -ForegroundColor Yellow
Test-Endpoint "Notification Rules" "Delete Rule 1" "DELETE" "/api/event-communication-rules/$testRuleId"
Test-Endpoint "Notification Rules" "Delete Rule 2" "DELETE" "/api/event-communication-rules/$emailRuleId"
Test-Endpoint "Notification Rules" "Delete Rule 3" "DELETE" "/api/event-communication-rules/$roleRuleId"
Test-Endpoint "Notification Rules" "Delete Rule 4" "DELETE" "/api/event-communication-rules/$delayRuleId"
Write-Host ""

# =====================================================
# CLEANUP
# =====================================================
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "CLEANUP" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Delete test template
Write-Host "Cleaning up test template..." -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/communication-templates/$testTemplateId" -Method DELETE -Headers $headers | Out-Null
    Write-Host "  ✓ Template deleted" -ForegroundColor Green
} catch {
    Write-Host "  ⊘ Template cleanup skipped or failed" -ForegroundColor Yellow
}

# Delete test event type
Write-Host "Cleaning up test event type..." -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/event-types/$testEventId" -Method DELETE -Headers $headers | Out-Null
    Write-Host "  ✓ Event type deleted" -ForegroundColor Green
} catch {
    Write-Host "  ⊘ Event type cleanup skipped or failed" -ForegroundColor Yellow
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
$passRate = [math]::Round(($passed / $total) * 100, 2)

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
    $catRate = [math]::Round(($catPassed / $catTotal) * 100, 2)
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
Write-Host "TEST SUITE COMPLETE" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

# Save results to file
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$TestResults | ConvertTo-Json -Depth 10 | Out-File "notification-system-test-results-$timestamp.json"
Write-Host ""
Write-Host "Results saved to: notification-system-test-results-$timestamp.json" -ForegroundColor Cyan
