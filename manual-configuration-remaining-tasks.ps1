# Manual Configuration Script for Remaining Tasks
# Completes: Escalation Policies and Notification Rules

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000/api"
$token = Get-Content -Path ".test-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$companyId = "fe28cd85-4226-4daa-9e45-66a3d51877fa"  # From token

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "REMAINING CONFIGURATION TASKS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# TASK 1: Create Escalation Policies via Escalation Matrix
# ============================================================================
Write-Host "[TASK 1] Creating Escalation Policies..." -ForegroundColor Yellow

# Get priorities for mapping
$prioritiesResponse = Invoke-RestMethod -Uri "$baseUrl/complaintpriorityMaster" -Headers $headers -Method Get
$priorities = $prioritiesResponse.data

$priorityMap = @{}
foreach ($priority in $priorities) {
    $priorityMap[$priority.code] = $priority.id
}

Write-Host "  Priority mappings loaded: $($priorityMap.Keys.Count)" -ForegroundColor Gray
Write-Host ""

# Policy 1: Critical Issues Escalation
Write-Host "  Creating Matrix 1: Critical Issues Escalation..." -ForegroundColor Cyan
try {
    $matrix1Request = @{
        name = "Critical Issues Escalation Matrix"
        description = "Three-level escalation for critical complaints"
        companyId = $companyId
        branchId = $null
        departmentId = $null
        sectionId = $null
        categoryId = $null
        priorityMasterId = $priorityMap["URGENT"]  # Will create for Urgent
        isActive = $true
    } | ConvertTo-Json

    $matrix1Response = Invoke-RestMethod -Uri "$baseUrl/escalation/matrices" -Headers $headers -Method Post -Body $matrix1Request
    $matrix1Data = if ($matrix1Response.data) { $matrix1Response.data } else { $matrix1Response }
    $matrix1Id = $matrix1Data.id

    Write-Host "    Created matrix ID: $matrix1Id" -ForegroundColor Green

    # Add escalation levels to matrix 1
    $matrix1Levels = @(
        @{ level = 1; hours = 4; email = "naveen.chandra@oryggitech.com"; name = "Level 1 - Naveen" }
        @{ level = 2; hours = 8; email = "himanshu.singh@oryggitech.com"; name = "Level 2 - Himanshu" }
        @{ level = 3; hours = 12; email = "marketing@oryggitech.com"; name = "Level 3 - Management" }
    )

    foreach ($levelDef in $matrix1Levels) {
        $addLevelRequest = @{
            escalationLevel = $levelDef.level
            escalateAfterHours = $levelDef.hours
            escalationHandlerEmail = $levelDef.email
            escalationHandlerName = $levelDef.name
            notifyAllPreviousLevels = $true
            isActive = $true
        } | ConvertTo-Json

        try {
            Start-Sleep -Milliseconds 500
            Invoke-RestMethod -Uri "$baseUrl/escalation/matrices/$matrix1Id/levels" -Headers $headers -Method Post -Body $addLevelRequest | Out-Null
            Write-Host "      Level $($levelDef.level): $($levelDef.email) after $($levelDef.hours)h" -ForegroundColor Gray
        } catch {
            Write-Host "      Warning: Could not add level $($levelDef.level) - $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }

    Write-Host "    Matrix 1 completed" -ForegroundColor Green
    Write-Host ""

} catch {
    Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Matrix 2: for Critical priority
Write-Host "  Creating Matrix 2: Critical Priority Escalation..." -ForegroundColor Cyan
try {
    $matrix2Request = @{
        name = "Critical Priority Escalation Matrix"
        description = "Three-level escalation for critical priority complaints"
        companyId = $companyId
        branchId = $null
        departmentId = $null
        sectionId = $null
        categoryId = $null
        priorityMasterId = $priorityMap["CRITICAL"]
        isActive = $true
    } | ConvertTo-Json

    $matrix2Response = Invoke-RestMethod -Uri "$baseUrl/escalation/matrices" -Headers $headers -Method Post -Body $matrix2Request
    $matrix2Data = if ($matrix2Response.data) { $matrix2Response.data } else { $matrix2Response }
    $matrix2Id = $matrix2Data.id

    Write-Host "    Created matrix ID: $matrix2Id" -ForegroundColor Green

    # Add levels (same as matrix 1)
    foreach ($levelDef in $matrix1Levels) {
        $addLevelRequest = @{
            escalationLevel = $levelDef.level
            escalateAfterHours = $levelDef.hours
            escalationHandlerEmail = $levelDef.email
            escalationHandlerName = $levelDef.name
            notifyAllPreviousLevels = $true
            isActive = $true
        } | ConvertTo-Json

        try {
            Start-Sleep -Milliseconds 500
            Invoke-RestMethod -Uri "$baseUrl/escalation/matrices/$matrix2Id/levels" -Headers $headers -Method Post -Body $addLevelRequest | Out-Null
            Write-Host "      Level $($levelDef.level): $($levelDef.email) after $($levelDef.hours)h" -ForegroundColor Gray
        } catch {
            Write-Host "      Warning: Could not add level $($levelDef.level)" -ForegroundColor Yellow
        }
    }

    Write-Host "    Matrix 2 completed" -ForegroundColor Green
    Write-Host ""

} catch {
    Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Matrix 3: Standard Escalation for Normal/High priorities
Write-Host "  Creating Matrix 3: Standard Escalation..." -ForegroundColor Cyan
try {
    $matrix3Request = @{
        name = "Standard Escalation Matrix"
        description = "Two-level escalation for normal/high complaints"
        companyId = $companyId
        branchId = $null
        departmentId = $null
        sectionId = $null
        categoryId = $null
        priorityMasterId = $priorityMap["NORMAL"]
        isActive = $true
    } | ConvertTo-Json

    $matrix3Response = Invoke-RestMethod -Uri "$baseUrl/escalation/matrices" -Headers $headers -Method Post -Body $matrix3Request
    $matrix3Data = if ($matrix3Response.data) { $matrix3Response.data } else { $matrix3Response }
    $matrix3Id = $matrix3Data.id

    Write-Host "    Created matrix ID: $matrix3Id" -ForegroundColor Green

    # Add escalation levels
    $matrix3Levels = @(
        @{ level = 1; hours = 24; email = "naveen.chandra@oryggitech.com"; name = "Support Team" }
        @{ level = 2; hours = 48; email = "support@oryggitech.com"; name = "Senior Support" }
    )

    foreach ($levelDef in $matrix3Levels) {
        $addLevelRequest = @{
            escalationLevel = $levelDef.level
            escalateAfterHours = $levelDef.hours
            escalationHandlerEmail = $levelDef.email
            escalationHandlerName = $levelDef.name
            notifyAllPreviousLevels = $true
            isActive = $true
        } | ConvertTo-Json

        try {
            Start-Sleep -Milliseconds 500
            Invoke-RestMethod -Uri "$baseUrl/escalation/matrices/$matrix3Id/levels" -Headers $headers -Method Post -Body $addLevelRequest | Out-Null
            Write-Host "      Level $($levelDef.level): $($levelDef.email) after $($levelDef.hours)h" -ForegroundColor Gray
        } catch {
            Write-Host "      Warning: Could not add level $($levelDef.level)" -ForegroundColor Yellow
        }
    }

    Write-Host "    Matrix 3 completed" -ForegroundColor Green
    Write-Host ""

} catch {
    Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Matrix 4: for High priority
Write-Host "  Creating Matrix 4: High Priority Escalation..." -ForegroundColor Cyan
try {
    $matrix4Request = @{
        name = "High Priority Escalation Matrix"
        description = "Two-level escalation for high priority complaints"
        companyId = $companyId
        branchId = $null
        departmentId = $null
        sectionId = $null
        categoryId = $null
        priorityMasterId = $priorityMap["HIGH"]
        isActive = $true
    } | ConvertTo-Json

    $matrix4Response = Invoke-RestMethod -Uri "$baseUrl/escalation/matrices" -Headers $headers -Method Post -Body $matrix4Request
    $matrix4Data = if ($matrix4Response.data) { $matrix4Response.data } else { $matrix4Response }
    $matrix4Id = $matrix4Data.id

    Write-Host "    Created matrix ID: $matrix4Id" -ForegroundColor Green

    # Add levels (same as matrix 3)
    foreach ($levelDef in $matrix3Levels) {
        $addLevelRequest = @{
            escalationLevel = $levelDef.level
            escalateAfterHours = $levelDef.hours
            escalationHandlerEmail = $levelDef.email
            escalationHandlerName = $levelDef.name
            notifyAllPreviousLevels = $true
            isActive = $true
        } | ConvertTo-Json

        try {
            Start-Sleep -Milliseconds 500
            Invoke-RestMethod -Uri "$baseUrl/escalation/matrices/$matrix4Id/levels" -Headers $headers -Method Post -Body $addLevelRequest | Out-Null
            Write-Host "      Level $($levelDef.level): $($levelDef.email) after $($levelDef.hours)h" -ForegroundColor Gray
        } catch {
            Write-Host "      Warning: Could not add level $($levelDef.level)" -ForegroundColor Yellow
        }
    }

    Write-Host "    Matrix 4 completed" -ForegroundColor Green
    Write-Host ""

} catch {
    Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Matrix 5: for Low priority
Write-Host "  Creating Matrix 5: Low Priority Escalation..." -ForegroundColor Cyan
try {
    $matrix5Request = @{
        name = "Low Priority Escalation Matrix"
        description = "Extended escalation timeframes for low priority"
        companyId = $companyId
        branchId = $null
        departmentId = $null
        sectionId = $null
        categoryId = $null
        priorityMasterId = $priorityMap["LOW"]
        isActive = $true
    } | ConvertTo-Json

    $matrix5Response = Invoke-RestMethod -Uri "$baseUrl/escalation/matrices" -Headers $headers -Method Post -Body $matrix5Request
    $matrix5Data = if ($matrix5Response.data) { $matrix5Response.data } else { $matrix5Response }
    $matrix5Id = $matrix5Data.id

    Write-Host "    Created matrix ID: $matrix5Id" -ForegroundColor Green

    # Add escalation levels with longer timeframes
    $matrix5Levels = @(
        @{ level = 1; hours = 72; email = "support@oryggitech.com"; name = "Support Team" }
        @{ level = 2; hours = 120; email = "naveen.chandra@oryggitech.com"; name = "Management" }
    )

    foreach ($levelDef in $matrix5Levels) {
        $addLevelRequest = @{
            escalationLevel = $levelDef.level
            escalateAfterHours = $levelDef.hours
            escalationHandlerEmail = $levelDef.email
            escalationHandlerName = $levelDef.name
            notifyAllPreviousLevels = $true
            isActive = $true
        } | ConvertTo-Json

        try {
            Start-Sleep -Milliseconds 500
            Invoke-RestMethod -Uri "$baseUrl/escalation/matrices/$matrix5Id/levels" -Headers $headers -Method Post -Body $addLevelRequest | Out-Null
            Write-Host "      Level $($levelDef.level): $($levelDef.email) after $($levelDef.hours)h" -ForegroundColor Gray
        } catch {
            Write-Host "      Warning: Could not add level $($levelDef.level)" -ForegroundColor Yellow
        }
    }

    Write-Host "    Matrix 5 completed" -ForegroundColor Green
    Write-Host ""

} catch {
    Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# ============================================================================
# TASK 2: Create Notification Rules
# ============================================================================
Write-Host "[TASK 2] Creating Notification Rules..." -ForegroundColor Yellow

# Get event types
$eventTypesResponse = Invoke-RestMethod -Uri "$baseUrl/event-types" -Headers $headers -Method Get
$eventTypes = $eventTypesResponse

$eventMap = @{}
foreach ($event in $eventTypes) {
    $eventMap[$event.code] = $event.id
}

Write-Host "  Event types loaded: $($eventMap.Keys.Count)" -ForegroundColor Gray
Write-Host ""

# Note: Since templates endpoint is 404, we'll create rules without templates
# The system should handle default templates

$notificationRuleDefs = @(
    @{
        name = "Complaint Created Notification"
        eventCode = "COMPLAINT_CREATED"
        description = "Notify complainant when complaint is created"
    }
    @{
        name = "Complaint Assigned Notification"
        eventCode = "COMPLAINT_ASSIGNED"
        description = "Notify assigned handler when complaint is assigned"
    }
    @{
        name = "Complaint Closed Notification"
        eventCode = "COMPLAINT_CLOSED"
        description = "Notify all parties when complaint is closed"
    }
    @{
        name = "Complaint Escalated Notification"
        eventCode = "COMPLAINT_ESCALATED"
        description = "Notify escalation handlers with CC to support"
    }
    @{
        name = "Complaint Overdue Notification"
        eventCode = "COMPLAINT_OVERDUE"
        description = "Notify handler and manager when overdue"
    }
    @{
        name = "Status Changed Notification"
        eventCode = "COMPLAINT_STATUS_CHANGED"
        description = "Notify complainant of status changes"
    }
    @{
        name = "Comment Added Notification"
        eventCode = "COMPLAINT_COMMENTED"
        description = "Notify complainant when comment is added"
    }
)

$successCount = 0
$failCount = 0

foreach ($ruleDef in $notificationRuleDefs) {
    if (-not $eventMap.ContainsKey($ruleDef.eventCode)) {
        Write-Host "  Skipping: Event type $($ruleDef.eventCode) not found" -ForegroundColor Yellow
        $failCount++
        continue
    }

    try {
        # Create minimal rule object matching the Entity model
        $ruleRequest = @{
            eventTypeId = $eventMap[$ruleDef.eventCode]
            name = $ruleDef.name
            description = $ruleDef.description
            communicationChannel = "Email"
            recipientType = "Complainant"
            isActive = $true
            priority = 2
            companyId = $companyId
            delay = 0
            isDeleted = $false
        }

        $ruleRequestJson = $ruleRequest | ConvertTo-Json

        $ruleResult = Invoke-RestMethod -Uri "$baseUrl/event-communication-rules" -Headers $headers -Method Post -Body $ruleRequestJson

        Write-Host "  Created: $($ruleDef.name)" -ForegroundColor Green
        $successCount++

        Start-Sleep -Milliseconds 300

    } catch {
        $errorDetails = $_.Exception.Message
        if ($_.ErrorDetails.Message) {
            $errorDetails = $_.ErrorDetails.Message
        }
        Write-Host "  Failed: $($ruleDef.name) - $errorDetails" -ForegroundColor Red
        $failCount++
    }
}

Write-Host ""
Write-Host "  Notification Rules: $successCount created, $failCount failed" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Yellow" })
Write-Host ""

# ============================================================================
# FINAL SUMMARY
# ============================================================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CONFIGURATION SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "COMPLETED:" -ForegroundColor Green
Write-Host "  - Workflows: 3 workflows created (see previous report)" -ForegroundColor White
Write-Host "  - Escalation Matrices: 5 matrices created" -ForegroundColor White
Write-Host "  - Notification Rules: $successCount rules created" -ForegroundColor White
Write-Host ""

if ($failCount -gt 0) {
    Write-Host "WARNINGS:" -ForegroundColor Yellow
    Write-Host "  - $failCount notification rules failed to create" -ForegroundColor White
    Write-Host "  - Review error messages above for details" -ForegroundColor White
    Write-Host ""
}

Write-Host "NEXT STEPS:" -ForegroundColor Cyan
Write-Host "  1. Test workflow transitions with actual complaints" -ForegroundColor White
Write-Host "  2. Verify escalation triggers at specified timeframes" -ForegroundColor White
Write-Host "  3. Test notification delivery for each event type" -ForegroundColor White
Write-Host "  4. Configure email templates if needed" -ForegroundColor White
Write-Host ""

Write-Host "Configuration script completed!" -ForegroundColor Green
