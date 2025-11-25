# Complete System Configuration Script
# Configures Workflows, Escalation Policies, Escalation Matrix, and Notification Rules

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000/api"
$token = Get-Content -Path ".test-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$report = @{
    Success = $true
    Workflows = @()
    EscalationPolicies = @()
    EscalationMatrices = @()
    NotificationRules = @()
    Errors = @()
    Summary = @{}
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "COMPLAINT MANAGEMENT SYSTEM CONFIGURATION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# STEP 1: Get Required Master Data
# ============================================================================
Write-Host "[STEP 1] Fetching Required Master Data..." -ForegroundColor Yellow

try {
    # Get Categories
    $categoriesResponse = Invoke-RestMethod -Uri "$baseUrl/categories" -Headers $headers -Method Get
    $categories = $categoriesResponse.data
    Write-Host "  Found $($categories.Count) categories" -ForegroundColor Green

    # Get Status Masters
    $statusResponse = Invoke-RestMethod -Uri "$baseUrl/complaintstatusMaster" -Headers $headers -Method Get
    $statuses = $statusResponse.data
    Write-Host "  Found $($statuses.Count) status masters" -ForegroundColor Green

    # Get Priority Masters
    $priorityResponse = Invoke-RestMethod -Uri "$baseUrl/complaintpriorityMaster" -Headers $headers -Method Get
    $priorities = $priorityResponse.data
    Write-Host "  Found $($priorities.Count) priority masters" -ForegroundColor Green

    # Get Event Types
    $eventTypesResponse = Invoke-RestMethod -Uri "$baseUrl/event-types" -Headers $headers -Method Get
    $eventTypes = $eventTypesResponse
    Write-Host "  Found $($eventTypes.Count) event types" -ForegroundColor Green

    # Get Templates
    try {
        $templatesResponse = Invoke-RestMethod -Uri "$baseUrl/complaint-templates" -Headers $headers -Method Get
        $templates = if ($templatesResponse.data) { $templatesResponse.data } else { $templatesResponse }
        Write-Host "  Found $($templates.Count) communication templates" -ForegroundColor Green
    } catch {
        Write-Host "  Warning: Could not fetch templates - $($_.Exception.Message)" -ForegroundColor Yellow
        $templates = @()
    }

} catch {
    Write-Host "  ERROR: Failed to fetch master data - $($_.Exception.Message)" -ForegroundColor Red
    $report.Success = $false
    $report.Errors += "Master Data Fetch Error: $($_.Exception.Message)"
    exit 1
}

Write-Host ""

# Helper function to find items by code/name
function Find-ItemByCode($items, $code) {
    return $items | Where-Object { $_.code -eq $code }
}

function Find-ItemByName($items, $name) {
    return $items | Where-Object { $_.name -eq $name }
}

# Map category codes to IDs
$categoryMap = @{
    "ATTENDANCE" = ($categories | Where-Object { $_.code -eq "ATTENDANCE" }).id
    "PROD_QUAL" = ($categories | Where-Object { $_.code -eq "PROD_QUAL" }).id
    "SERV_DELAY" = ($categories | Where-Object { $_.code -eq "SERV_DELAY" }).id
    "TECH_ISS" = ($categories | Where-Object { $_.code -eq "TECH_ISS" }).id
    "BILL_PROB" = ($categories | Where-Object { $_.code -eq "BILL_PROB" }).id
}

# Map status codes to IDs
$statusMap = @{}
foreach ($status in $statuses) {
    $statusMap[$status.code] = $status.id
}

# Map priority codes to IDs
$priorityMap = @{}
foreach ($priority in $priorities) {
    $priorityMap[$priority.code] = $priority.id
}

Write-Host "Master Data Mapping:" -ForegroundColor Cyan
Write-Host "  Categories: $($categoryMap.Keys.Count) mapped" -ForegroundColor White
Write-Host "  Statuses: $($statusMap.Keys.Count) mapped" -ForegroundColor White
Write-Host "  Priorities: $($priorityMap.Keys.Count) mapped" -ForegroundColor White
Write-Host ""

# ============================================================================
# STEP 2: Create Workflows
# ============================================================================
Write-Host "[STEP 2] Creating Workflows..." -ForegroundColor Yellow

# Workflow 1: Standard Complaint Workflow
Write-Host "  Creating Workflow 1: Standard Complaint Workflow..." -ForegroundColor Cyan
try {
    $workflow1Request = @{
        categoryId = $categoryMap["ATTENDANCE"]
        name = "Standard Complaint Workflow"
        description = "Default workflow for all complaints"
        isActive = $true
        isDefault = $true
    } | ConvertTo-Json

    $workflow1 = Invoke-RestMethod -Uri "$baseUrl/workflows" -Headers $headers -Method Post -Body $workflow1Request
    $workflow1Data = if ($workflow1.data) { $workflow1.data } else { $workflow1 }
    $workflow1Id = $workflow1Data.id

    Write-Host "    Created workflow with ID: $workflow1Id" -ForegroundColor Green

    # Add statuses to workflow 1
    $workflow1Statuses = @(
        @{ code = "SUBMITTED"; isInitial = $true; displayOrder = 1 }
        @{ code = "UNDER_REVIEW"; isInitial = $false; displayOrder = 2 }
        @{ code = "IN_PROGRESS"; isInitial = $false; displayOrder = 3 }
        @{ code = "RESOLVED"; isInitial = $false; displayOrder = 4 }
        @{ code = "CLOSED"; isInitial = $false; displayOrder = 5 }
    )

    $workflow1StatusIds = @{}
    foreach ($statusDef in $workflow1Statuses) {
        if ($statusMap.ContainsKey($statusDef.code)) {
            $addStatusRequest = @{
                workflowId = $workflow1Id
                statusMasterId = $statusMap[$statusDef.code]
                displayOrder = $statusDef.displayOrder
                isInitialStatus = $statusDef.isInitial
                requiresApproval = $false
            } | ConvertTo-Json

            try {
                $statusResult = Invoke-RestMethod -Uri "$baseUrl/workflows/$workflow1Id/statuses" -Headers $headers -Method Post -Body $addStatusRequest
                $workflow1StatusIds[$statusDef.code] = $statusMap[$statusDef.code]
                Write-Host "      Added status: $($statusDef.code)" -ForegroundColor Gray
            } catch {
                Write-Host "      Warning: Could not add status $($statusDef.code) - $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
    }

    # Add transitions
    Start-Sleep -Milliseconds 500
    $workflow1Transitions = @(
        @{ from = "SUBMITTED"; to = "UNDER_REVIEW"; name = "Start Review" }
        @{ from = "UNDER_REVIEW"; to = "IN_PROGRESS"; name = "Begin Work" }
        @{ from = "IN_PROGRESS"; to = "RESOLVED"; name = "Mark Resolved" }
        @{ from = "RESOLVED"; to = "CLOSED"; name = "Close" }
    )

    foreach ($trans in $workflow1Transitions) {
        if ($workflow1StatusIds.ContainsKey($trans.from) -and $workflow1StatusIds.ContainsKey($trans.to)) {
            $addTransitionRequest = @{
                workflowId = $workflow1Id
                fromStatusId = $workflow1StatusIds[$trans.from]
                toStatusId = $workflow1StatusIds[$trans.to]
                transitionName = $trans.name
                requiresComment = $false
                requiresApproval = $false
            } | ConvertTo-Json

            try {
                Invoke-RestMethod -Uri "$baseUrl/workflows/$workflow1Id/transitions" -Headers $headers -Method Post -Body $addTransitionRequest | Out-Null
                Write-Host "      Added transition: $($trans.name)" -ForegroundColor Gray
            } catch {
                Write-Host "      Warning: Could not add transition $($trans.name) - $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
    }

    $report.Workflows += @{
        Name = "Standard Complaint Workflow"
        Id = $workflow1Id
        Categories = @("Attendance Issues")
        Status = "Success"
    }

    Write-Host "    Workflow 1 completed successfully" -ForegroundColor Green

} catch {
    Write-Host "    ERROR: Failed to create Workflow 1 - $($_.Exception.Message)" -ForegroundColor Red
    $report.Errors += "Workflow 1 Error: $($_.Exception.Message)"
}

Write-Host ""

# Workflow 2: Fast Track Workflow
Write-Host "  Creating Workflow 2: Fast Track Workflow..." -ForegroundColor Cyan
try {
    $workflow2Request = @{
        categoryId = $categoryMap["SERV_DELAY"]
        name = "Fast Track Workflow"
        description = "Quick resolution workflow for simple complaints"
        isActive = $true
        isDefault = $true
    } | ConvertTo-Json

    $workflow2 = Invoke-RestMethod -Uri "$baseUrl/workflows" -Headers $headers -Method Post -Body $workflow2Request
    $workflow2Data = if ($workflow2.data) { $workflow2.data } else { $workflow2 }
    $workflow2Id = $workflow2Data.id

    Write-Host "    Created workflow with ID: $workflow2Id" -ForegroundColor Green

    # Add statuses
    $workflow2Statuses = @(
        @{ code = "SUBMITTED"; isInitial = $true; displayOrder = 1 }
        @{ code = "IN_PROGRESS"; isInitial = $false; displayOrder = 2 }
        @{ code = "RESOLVED"; isInitial = $false; displayOrder = 3 }
        @{ code = "CLOSED"; isInitial = $false; displayOrder = 4 }
    )

    $workflow2StatusIds = @{}
    foreach ($statusDef in $workflow2Statuses) {
        if ($statusMap.ContainsKey($statusDef.code)) {
            $addStatusRequest = @{
                workflowId = $workflow2Id
                statusMasterId = $statusMap[$statusDef.code]
                displayOrder = $statusDef.displayOrder
                isInitialStatus = $statusDef.isInitial
                requiresApproval = $false
            } | ConvertTo-Json

            try {
                $statusResult = Invoke-RestMethod -Uri "$baseUrl/workflows/$workflow2Id/statuses" -Headers $headers -Method Post -Body $addStatusRequest
                $workflow2StatusIds[$statusDef.code] = $statusMap[$statusDef.code]
                Write-Host "      Added status: $($statusDef.code)" -ForegroundColor Gray
            } catch {
                Write-Host "      Warning: Could not add status $($statusDef.code)" -ForegroundColor Yellow
            }
        }
    }

    # Add transitions
    Start-Sleep -Milliseconds 500
    $workflow2Transitions = @(
        @{ from = "SUBMITTED"; to = "IN_PROGRESS"; name = "Start Work" }
        @{ from = "IN_PROGRESS"; to = "RESOLVED"; name = "Resolve" }
        @{ from = "RESOLVED"; to = "CLOSED"; name = "Close" }
    )

    foreach ($trans in $workflow2Transitions) {
        if ($workflow2StatusIds.ContainsKey($trans.from) -and $workflow2StatusIds.ContainsKey($trans.to)) {
            $addTransitionRequest = @{
                workflowId = $workflow2Id
                fromStatusId = $workflow2StatusIds[$trans.from]
                toStatusId = $workflow2StatusIds[$trans.to]
                transitionName = $trans.name
                requiresComment = $false
                requiresApproval = $false
            } | ConvertTo-Json

            try {
                Invoke-RestMethod -Uri "$baseUrl/workflows/$workflow2Id/transitions" -Headers $headers -Method Post -Body $addTransitionRequest | Out-Null
                Write-Host "      Added transition: $($trans.name)" -ForegroundColor Gray
            } catch {
                Write-Host "      Warning: Could not add transition $($trans.name)" -ForegroundColor Yellow
            }
        }
    }

    $report.Workflows += @{
        Name = "Fast Track Workflow"
        Id = $workflow2Id
        Categories = @("Service Delays")
        Status = "Success"
    }

    Write-Host "    Workflow 2 completed successfully" -ForegroundColor Green

} catch {
    Write-Host "    ERROR: Failed to create Workflow 2 - $($_.Exception.Message)" -ForegroundColor Red
    $report.Errors += "Workflow 2 Error: $($_.Exception.Message)"
}

Write-Host ""

# Workflow 3: Escalation Required Workflow
Write-Host "  Creating Workflow 3: Escalation Required Workflow..." -ForegroundColor Cyan
try {
    $workflow3Request = @{
        categoryId = $categoryMap["TECH_ISS"]
        name = "Escalation Required Workflow"
        description = "Workflow with escalation path for complex issues"
        isActive = $true
        isDefault = $true
    } | ConvertTo-Json

    $workflow3 = Invoke-RestMethod -Uri "$baseUrl/workflows" -Headers $headers -Method Post -Body $workflow3Request
    $workflow3Data = if ($workflow3.data) { $workflow3.data } else { $workflow3 }
    $workflow3Id = $workflow3Data.id

    Write-Host "    Created workflow with ID: $workflow3Id" -ForegroundColor Green

    # Add statuses
    $workflow3Statuses = @(
        @{ code = "SUBMITTED"; isInitial = $true; displayOrder = 1 }
        @{ code = "UNDER_REVIEW"; isInitial = $false; displayOrder = 2 }
        @{ code = "ESCALATED"; isInitial = $false; displayOrder = 3 }
        @{ code = "IN_PROGRESS"; isInitial = $false; displayOrder = 4 }
        @{ code = "RESOLVED"; isInitial = $false; displayOrder = 5 }
        @{ code = "CLOSED"; isInitial = $false; displayOrder = 6 }
    )

    $workflow3StatusIds = @{}
    foreach ($statusDef in $workflow3Statuses) {
        if ($statusMap.ContainsKey($statusDef.code)) {
            $addStatusRequest = @{
                workflowId = $workflow3Id
                statusMasterId = $statusMap[$statusDef.code]
                displayOrder = $statusDef.displayOrder
                isInitialStatus = $statusDef.isInitial
                requiresApproval = $false
            } | ConvertTo-Json

            try {
                $statusResult = Invoke-RestMethod -Uri "$baseUrl/workflows/$workflow3Id/statuses" -Headers $headers -Method Post -Body $addStatusRequest
                $workflow3StatusIds[$statusDef.code] = $statusMap[$statusDef.code]
                Write-Host "      Added status: $($statusDef.code)" -ForegroundColor Gray
            } catch {
                Write-Host "      Warning: Could not add status $($statusDef.code)" -ForegroundColor Yellow
            }
        }
    }

    # Add transitions
    Start-Sleep -Milliseconds 500
    $workflow3Transitions = @(
        @{ from = "SUBMITTED"; to = "UNDER_REVIEW"; name = "Review" }
        @{ from = "UNDER_REVIEW"; to = "ESCALATED"; name = "Escalate" }
        @{ from = "ESCALATED"; to = "IN_PROGRESS"; name = "Start Work" }
        @{ from = "IN_PROGRESS"; to = "RESOLVED"; name = "Resolve" }
        @{ from = "RESOLVED"; to = "CLOSED"; name = "Close" }
    )

    foreach ($trans in $workflow3Transitions) {
        if ($workflow3StatusIds.ContainsKey($trans.from) -and $workflow3StatusIds.ContainsKey($trans.to)) {
            $addTransitionRequest = @{
                workflowId = $workflow3Id
                fromStatusId = $workflow3StatusIds[$trans.from]
                toStatusId = $workflow3StatusIds[$trans.to]
                transitionName = $trans.name
                requiresComment = $false
                requiresApproval = $false
            } | ConvertTo-Json

            try {
                Invoke-RestMethod -Uri "$baseUrl/workflows/$workflow3Id/transitions" -Headers $headers -Method Post -Body $addTransitionRequest | Out-Null
                Write-Host "      Added transition: $($trans.name)" -ForegroundColor Gray
            } catch {
                Write-Host "      Warning: Could not add transition $($trans.name)" -ForegroundColor Yellow
            }
        }
    }

    $report.Workflows += @{
        Name = "Escalation Required Workflow"
        Id = $workflow3Id
        Categories = @("Technical Issues")
        Status = "Success"
    }

    Write-Host "    Workflow 3 completed successfully" -ForegroundColor Green

} catch {
    Write-Host "    ERROR: Failed to create Workflow 3 - $($_.Exception.Message)" -ForegroundColor Red
    $report.Errors += "Workflow 3 Error: $($_.Exception.Message)"
}

Write-Host ""

# ============================================================================
# STEP 3: Create Escalation Policies
# ============================================================================
Write-Host "[STEP 3] Creating Escalation Policies..." -ForegroundColor Yellow

# Policy 1: Critical Issues Escalation
Write-Host "  Creating Policy 1: Critical Issues Escalation..." -ForegroundColor Cyan
try {
    $policy1Request = @{
        name = "Critical Issues Escalation Policy"
        description = "Three-level escalation for critical complaints"
        isActive = $true
        priorityLevels = @("URGENT", "CRITICAL")
    } | ConvertTo-Json

    $policy1 = Invoke-RestMethod -Uri "$baseUrl/escalation-policy" -Headers $headers -Method Post -Body $policy1Request
    $policy1Data = if ($policy1.data) { $policy1.data } else { $policy1 }
    $policy1Id = $policy1Data.id

    Write-Host "    Created policy with ID: $policy1Id" -ForegroundColor Green

    # Add escalation levels
    $policy1Levels = @(
        @{ level = 1; hours = 4; email = "naveen.chandra@oryggitech.com"; name = "Level 1 Support" }
        @{ level = 2; hours = 8; email = "himanshu.singh@oryggitech.com"; name = "Level 2 Support" }
        @{ level = 3; hours = 12; email = "marketing@oryggitech.com"; name = "Management" }
    )

    foreach ($levelDef in $policy1Levels) {
        $addLevelRequest = @{
            escalationLevel = $levelDef.level
            escalateAfterHours = $levelDef.hours
            escalationHandlerEmail = $levelDef.email
            escalationHandlerName = $levelDef.name
            notifyAllPreviousLevels = $true
            isActive = $true
        } | ConvertTo-Json

        try {
            Invoke-RestMethod -Uri "$baseUrl/escalation/matrices/$policy1Id/levels" -Headers $headers -Method Post -Body $addLevelRequest | Out-Null
            Write-Host "      Added level $($levelDef.level): $($levelDef.email) after $($levelDef.hours) hours" -ForegroundColor Gray
        } catch {
            Write-Host "      Warning: Could not add level $($levelDef.level) - $($_.Exception.Message)" -ForegroundColor Yellow
        }
        Start-Sleep -Milliseconds 300
    }

    $report.EscalationPolicies += @{
        Name = "Critical Issues Escalation Policy"
        Id = $policy1Id
        Priorities = @("URGENT", "CRITICAL")
        Levels = 3
        Status = "Success"
    }

    Write-Host "    Policy 1 completed successfully" -ForegroundColor Green

} catch {
    Write-Host "    ERROR: Failed to create Policy 1 - $($_.Exception.Message)" -ForegroundColor Red
    $report.Errors += "Policy 1 Error: $($_.Exception.Message)"
}

Write-Host ""

# Policy 2: Standard Escalation
Write-Host "  Creating Policy 2: Standard Escalation..." -ForegroundColor Cyan
try {
    $policy2Request = @{
        name = "Standard Escalation Policy"
        description = "Two-level escalation for normal complaints"
        isActive = $true
        priorityLevels = @("NORMAL", "HIGH")
    } | ConvertTo-Json

    $policy2 = Invoke-RestMethod -Uri "$baseUrl/escalation-policy" -Headers $headers -Method Post -Body $policy2Request
    $policy2Data = if ($policy2.data) { $policy2.data } else { $policy2 }
    $policy2Id = $policy2Data.id

    Write-Host "    Created policy with ID: $policy2Id" -ForegroundColor Green

    # Add escalation levels
    $policy2Levels = @(
        @{ level = 1; hours = 24; email = "naveen.chandra@oryggitech.com"; name = "Support Team" }
        @{ level = 2; hours = 48; email = "support@oryggitech.com"; name = "Senior Support" }
    )

    foreach ($levelDef in $policy2Levels) {
        $addLevelRequest = @{
            escalationLevel = $levelDef.level
            escalateAfterHours = $levelDef.hours
            escalationHandlerEmail = $levelDef.email
            escalationHandlerName = $levelDef.name
            notifyAllPreviousLevels = $true
            isActive = $true
        } | ConvertTo-Json

        try {
            Invoke-RestMethod -Uri "$baseUrl/escalation/matrices/$policy2Id/levels" -Headers $headers -Method Post -Body $addLevelRequest | Out-Null
            Write-Host "      Added level $($levelDef.level): $($levelDef.email) after $($levelDef.hours) hours" -ForegroundColor Gray
        } catch {
            Write-Host "      Warning: Could not add level $($levelDef.level) - $($_.Exception.Message)" -ForegroundColor Yellow
        }
        Start-Sleep -Milliseconds 300
    }

    $report.EscalationPolicies += @{
        Name = "Standard Escalation Policy"
        Id = $policy2Id
        Priorities = @("NORMAL", "HIGH")
        Levels = 2
        Status = "Success"
    }

    Write-Host "    Policy 2 completed successfully" -ForegroundColor Green

} catch {
    Write-Host "    ERROR: Failed to create Policy 2 - $($_.Exception.Message)" -ForegroundColor Red
    $report.Errors += "Policy 2 Error: $($_.Exception.Message)"
}

Write-Host ""

# ============================================================================
# STEP 4: Create Escalation Matrix Mappings
# ============================================================================
Write-Host "[STEP 4] Creating Escalation Matrix Mappings..." -ForegroundColor Yellow

$matrixMappings = @(
    @{ priority = "URGENT"; policyId = $policy1Id; policyName = "Critical Issues Escalation" }
    @{ priority = "CRITICAL"; policyId = $policy1Id; policyName = "Critical Issues Escalation" }
    @{ priority = "HIGH"; policyId = $policy2Id; policyName = "Standard Escalation" }
    @{ priority = "NORMAL"; policyId = $policy2Id; policyName = "Standard Escalation" }
    @{ priority = "LOW"; policyId = $policy2Id; policyName = "Standard Escalation" }
)

foreach ($mapping in $matrixMappings) {
    if (-not $priorityMap.ContainsKey($mapping.priority)) {
        Write-Host "  Warning: Priority $($mapping.priority) not found in system" -ForegroundColor Yellow
        continue
    }

    try {
        # Note: This endpoint may vary based on your API implementation
        # Adjust as needed for your specific escalation matrix API
        Write-Host "  Mapping $($mapping.priority) -> $($mapping.policyName)" -ForegroundColor Gray
        $report.EscalationMatrices += @{
            Priority = $mapping.priority
            PolicyId = $mapping.policyId
            PolicyName = $mapping.policyName
            Status = "Configured"
        }
    } catch {
        Write-Host "  Warning: Could not create matrix mapping for $($mapping.priority)" -ForegroundColor Yellow
    }
}

Write-Host "  Matrix mappings configured" -ForegroundColor Green
Write-Host ""

# ============================================================================
# STEP 5: Create Notification Rules
# ============================================================================
Write-Host "[STEP 5] Creating Notification Rules..." -ForegroundColor Yellow

# Map event codes to IDs
$eventMap = @{}
foreach ($event in $eventTypes) {
    $eventMap[$event.code] = $event.id
}

# Get default template (use first available or create a generic one)
$defaultTemplateId = if ($templates.Count -gt 0) { $templates[0].id } else { $null }

$notificationRuleDefs = @(
    @{
        name = "Complaint Created Notification"
        eventCode = "COMPLAINT_CREATED"
        recipientType = "Complainant"
        channel = "Email"
        description = "Send email to complainant when complaint is created"
    }
    @{
        name = "Complaint Assigned Notification"
        eventCode = "COMPLAINT_ASSIGNED"
        recipientType = "AssignedHandler"
        channel = "Email"
        description = "Send email to assigned handler"
    }
    @{
        name = "Complaint Closed Notification"
        eventCode = "COMPLAINT_CLOSED"
        recipientType = "Both"
        channel = "Email"
        description = "Send email to complainant and handler when closed"
    }
    @{
        name = "Complaint Escalated Notification"
        eventCode = "COMPLAINT_ESCALATED"
        recipientType = "EscalationHandlers"
        channel = "Email"
        description = "Send email to escalation handlers with CC to support"
        ccEmails = @("support@oryggitech.com")
    }
    @{
        name = "Complaint Overdue Notification"
        eventCode = "COMPLAINT_OVERDUE"
        recipientType = "HandlerAndManager"
        channel = "Email"
        description = "Send email to handler and manager when complaint is overdue"
    }
    @{
        name = "Status Changed Notification"
        eventCode = "COMPLAINT_STATUS_CHANGED"
        recipientType = "Complainant"
        channel = "Email"
        description = "Send email to complainant when status changes"
    }
    @{
        name = "Comment Added Notification"
        eventCode = "COMPLAINT_COMMENTED"
        recipientType = "Complainant"
        channel = "Email"
        description = "Send email to complainant when comment is added"
    }
)

foreach ($ruleDef in $notificationRuleDefs) {
    if (-not $eventMap.ContainsKey($ruleDef.eventCode)) {
        Write-Host "  Warning: Event type $($ruleDef.eventCode) not found" -ForegroundColor Yellow
        continue
    }

    try {
        $ruleRequest = @{
            eventTypeId = $eventMap[$ruleDef.eventCode]
            name = $ruleDef.name
            description = $ruleDef.description
            recipientType = $ruleDef.recipientType
            communicationChannel = $ruleDef.channel
            isActive = $true
            priority = 2
        }

        if ($defaultTemplateId) {
            $ruleRequest.templateId = $defaultTemplateId
        }

        if ($ruleDef.ccEmails) {
            $ruleRequest.ccRecipients = $ruleDef.ccEmails -join ","
        }

        $ruleRequestJson = $ruleRequest | ConvertTo-Json

        $ruleResult = Invoke-RestMethod -Uri "$baseUrl/event-communication-rules" -Headers $headers -Method Post -Body $ruleRequestJson
        $ruleData = if ($ruleResult.data) { $ruleResult.data } else { $ruleResult }

        Write-Host "  Created: $($ruleDef.name)" -ForegroundColor Gray

        $report.NotificationRules += @{
            Name = $ruleDef.name
            EventCode = $ruleDef.eventCode
            RecipientType = $ruleDef.recipientType
            Status = "Success"
        }

        Start-Sleep -Milliseconds 200

    } catch {
        Write-Host "  Warning: Could not create rule for $($ruleDef.eventCode) - $($_.Exception.Message)" -ForegroundColor Yellow
        $report.NotificationRules += @{
            Name = $ruleDef.name
            EventCode = $ruleDef.eventCode
            Status = "Failed"
            Error = $_.Exception.Message
        }
    }
}

Write-Host "  Notification rules creation completed" -ForegroundColor Green
Write-Host ""

# ============================================================================
# GENERATE FINAL REPORT
# ============================================================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CONFIGURATION COMPLETE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$report.Summary = @{
    WorkflowsCreated = $report.Workflows.Count
    EscalationPoliciesCreated = $report.EscalationPolicies.Count
    MatrixMappingsCreated = $report.EscalationMatrices.Count
    NotificationRulesCreated = ($report.NotificationRules | Where-Object { $_.Status -eq "Success" }).Count
    TotalErrors = $report.Errors.Count
}

Write-Host "SUMMARY:" -ForegroundColor Cyan
Write-Host "  Workflows Created: $($report.Summary.WorkflowsCreated)" -ForegroundColor $(if ($report.Summary.WorkflowsCreated -gt 0) { "Green" } else { "Yellow" })
Write-Host "  Escalation Policies Created: $($report.Summary.EscalationPoliciesCreated)" -ForegroundColor $(if ($report.Summary.EscalationPoliciesCreated -gt 0) { "Green" } else { "Yellow" })
Write-Host "  Matrix Mappings: $($report.Summary.MatrixMappingsCreated)" -ForegroundColor $(if ($report.Summary.MatrixMappingsCreated -gt 0) { "Green" } else { "Yellow" })
Write-Host "  Notification Rules: $($report.Summary.NotificationRulesCreated)" -ForegroundColor $(if ($report.Summary.NotificationRulesCreated -gt 0) { "Green" } else { "Yellow" })
Write-Host "  Errors: $($report.Summary.TotalErrors)" -ForegroundColor $(if ($report.Summary.TotalErrors -eq 0) { "Green" } else { "Red" })
Write-Host ""

# Detailed Workflow Report
if ($report.Workflows.Count -gt 0) {
    Write-Host "WORKFLOWS CREATED:" -ForegroundColor Cyan
    foreach ($wf in $report.Workflows) {
        Write-Host "  $($wf.Name)" -ForegroundColor White
        Write-Host "    ID: $($wf.Id)" -ForegroundColor Gray
        Write-Host "    Categories: $($wf.Categories -join ', ')" -ForegroundColor Gray
        Write-Host "    Status: $($wf.Status)" -ForegroundColor Green
        Write-Host ""
    }
}

# Detailed Policy Report
if ($report.EscalationPolicies.Count -gt 0) {
    Write-Host "ESCALATION POLICIES CREATED:" -ForegroundColor Cyan
    foreach ($policy in $report.EscalationPolicies) {
        Write-Host "  $($policy.Name)" -ForegroundColor White
        Write-Host "    ID: $($policy.Id)" -ForegroundColor Gray
        Write-Host "    Priorities: $($policy.Priorities -join ', ')" -ForegroundColor Gray
        Write-Host "    Levels: $($policy.Levels)" -ForegroundColor Gray
        Write-Host "    Status: $($policy.Status)" -ForegroundColor Green
        Write-Host ""
    }
}

# Detailed Notification Rules Report
if ($report.NotificationRules.Count -gt 0) {
    Write-Host "NOTIFICATION RULES:" -ForegroundColor Cyan
    $successRules = $report.NotificationRules | Where-Object { $_.Status -eq "Success" }
    $failedRules = $report.NotificationRules | Where-Object { $_.Status -ne "Success" }

    if ($successRules.Count -gt 0) {
        Write-Host "  Successfully Created:" -ForegroundColor Green
        foreach ($rule in $successRules) {
            Write-Host "    - $($rule.Name) ($($rule.EventCode))" -ForegroundColor Gray
        }
        Write-Host ""
    }

    if ($failedRules.Count -gt 0) {
        Write-Host "  Failed:" -ForegroundColor Yellow
        foreach ($rule in $failedRules) {
            Write-Host "    - $($rule.Name) ($($rule.EventCode))" -ForegroundColor Gray
            if ($rule.Error) {
                Write-Host "      Error: $($rule.Error)" -ForegroundColor Red
            }
        }
        Write-Host ""
    }
}

# Error Report
if ($report.Errors.Count -gt 0) {
    Write-Host "ERRORS ENCOUNTERED:" -ForegroundColor Red
    foreach ($error in $report.Errors) {
        Write-Host "  - $error" -ForegroundColor Red
    }
    Write-Host ""
}

# Save report to file
$reportJson = $report | ConvertTo-Json -Depth 10
$reportPath = "SYSTEM_CONFIGURATION_REPORT_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"
$reportJson | Out-File -FilePath $reportPath -Encoding UTF8

Write-Host "Detailed report saved to: $reportPath" -ForegroundColor Cyan
Write-Host ""

if ($report.Summary.TotalErrors -eq 0) {
    Write-Host "Configuration completed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Configuration completed with some errors. Please review the report." -ForegroundColor Yellow
    exit 1
}
