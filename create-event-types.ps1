# Create Event Types for Week 2 Auto-Response System

$token = Get-Content ".working-token" -Raw

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Creating Event Types for Auto-Response System" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Define event types to create
$eventTypes = @(
    @{
        code = "COMPLAINT_CREATED"
        name = "Complaint Created"
        entityType = "Complaint"
        description = "Triggered when a new complaint is created"
        category = "Complaint Lifecycle"
        availableFields = @(
            "complaintId", "complaintNumber", "title", "description",
            "categoryName", "priorityName", "statusName",
            "complainantName", "complainantEmail", "complainantEmployeeCode",
            "createdDate", "dueDate", "companyName"
        )
    },
    @{
        code = "COMPLAINT_ASSIGNED"
        name = "Complaint Assigned"
        entityType = "Complaint"
        description = "Triggered when a complaint is assigned to a handler"
        category = "Complaint Lifecycle"
        availableFields = @(
            "complaintId", "complaintNumber", "title",
            "assignedToName", "assignedToEmail",
            "assignedByName", "assignedByEmail",
            "assignedDate"
        )
    },
    @{
        code = "COMPLAINT_STATUS_CHANGED"
        name = "Complaint Status Changed"
        entityType = "Complaint"
        description = "Triggered when complaint status is changed"
        category = "Complaint Lifecycle"
        availableFields = @(
            "complaintId", "complaintNumber", "title",
            "oldStatus", "newStatus",
            "changedByName", "changedByEmail",
            "changedDate", "reason"
        )
    },
    @{
        code = "COMPLAINT_ESCALATED"
        name = "Complaint Escalated"
        entityType = "Complaint"
        description = "Triggered when a complaint is escalated"
        category = "Complaint Lifecycle"
        availableFields = @(
            "complaintId", "complaintNumber", "title",
            "escalationLevel", "escalatedToName", "escalatedToEmail",
            "escalatedByName", "escalatedByEmail",
            "escalatedDate", "reason"
        )
    },
    @{
        code = "COMPLAINT_RESOLVED"
        name = "Complaint Resolved"
        entityType = "Complaint"
        description = "Triggered when a complaint is resolved"
        category = "Complaint Lifecycle"
        availableFields = @(
            "complaintId", "complaintNumber", "title",
            "resolution", "resolvedByName", "resolvedByEmail",
            "resolvedDate", "resolutionNotes"
        )
    },
    @{
        code = "COMPLAINT_CLOSED"
        name = "Complaint Closed"
        entityType = "Complaint"
        description = "Triggered when a complaint is closed"
        category = "Complaint Lifecycle"
        availableFields = @(
            "complaintId", "complaintNumber", "title",
            "closedByName", "closedByEmail",
            "closedDate", "closureNotes"
        )
    },
    @{
        code = "COMPLAINT_REOPENED"
        name = "Complaint Reopened"
        entityType = "Complaint"
        description = "Triggered when a closed complaint is reopened"
        category = "Complaint Lifecycle"
        availableFields = @(
            "complaintId", "complaintNumber", "title",
            "reopenedByName", "reopenedByEmail",
            "reopenedDate", "reopenReason"
        )
    },
    @{
        code = "COMMENT_ADDED"
        name = "Comment Added"
        entityType = "Comment"
        description = "Triggered when a comment is added to a complaint"
        category = "Communication"
        availableFields = @(
            "complaintId", "complaintNumber",
            "commentText", "commentByName", "commentByEmail",
            "commentDate", "isInternal"
        )
    },
    @{
        code = "SLA_WARNING"
        name = "SLA Warning"
        entityType = "Complaint"
        description = "Triggered when complaint is approaching SLA deadline"
        category = "SLA Management"
        availableFields = @(
            "complaintId", "complaintNumber", "title",
            "slaLevel", "timeRemaining", "dueDate",
            "assignedToName", "assignedToEmail"
        )
    },
    @{
        code = "SLA_BREACHED"
        name = "SLA Breached"
        entityType = "Complaint"
        description = "Triggered when complaint has breached SLA deadline"
        category = "SLA Management"
        availableFields = @(
            "complaintId", "complaintNumber", "title",
            "slaLevel", "breachTime", "dueDate",
            "assignedToName", "assignedToEmail"
        )
    }
)

$successCount = 0
$failureCount = 0
$createdEventTypes = @()

foreach ($eventType in $eventTypes) {
    Write-Host "`nCreating event type: $($eventType.code)..." -ForegroundColor Yellow

    try {
        $body = @{
            code = $eventType.code
            name = $eventType.name
            entityType = $eventType.entityType
            description = $eventType.description
            category = $eventType.category
            availableFields = $eventType.availableFields
            isActive = $true
            isSystem = $false
        } | ConvertTo-Json -Depth 10

        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/event-types" `
            -Method POST `
            -Headers @{Authorization="Bearer $token"} `
            -Body $body `
            -ContentType "application/json"

        if ($response.isSuccess) {
            Write-Host "  SUCCESS: Created $($eventType.code)" -ForegroundColor Green
            Write-Host "    ID: $($response.data.id)" -ForegroundColor Gray
            $successCount++

            $createdEventTypes += @{
                code = $eventType.code
                id = $response.data.id
                name = $eventType.name
            }
        } else {
            Write-Host "  FAILED: $($response.message)" -ForegroundColor Red
            $failureCount++
        }
    } catch {
        Write-Host "  ERROR: $_" -ForegroundColor Red
        $failureCount++
    }

    Start-Sleep -Milliseconds 200
}

Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "EVENT TYPES CREATION SUMMARY" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Total Attempted: $($eventTypes.Count)" -ForegroundColor White
Write-Host "Successful: $successCount" -ForegroundColor Green
Write-Host "Failed: $failureCount" -ForegroundColor $(if ($failureCount -gt 0) { "Red" } else { "Gray" })

if ($createdEventTypes.Count -gt 0) {
    Write-Host "`nCreated Event Types:" -ForegroundColor Yellow
    $createdEventTypes | ForEach-Object {
        Write-Host "  - $($_.code) ($($_.name))" -ForegroundColor White
        Write-Host "    ID: $($_.id)" -ForegroundColor Gray
    }

    # Save to file for next step
    $createdEventTypes | ConvertTo-Json | Out-File "created-event-types.json"
    Write-Host "`nEvent type IDs saved to: created-event-types.json" -ForegroundColor Cyan
}

Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "Next Step: Link notification rules to event types" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
