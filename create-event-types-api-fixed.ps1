# Create Event Types via API (Fixed Version)

$token = Get-Content ".working-token" -Raw

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Creating Event Types via API" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Define event types - AvailableFields as JSON string
$eventTypes = @(
    @{
        code = "COMPLAINT_CREATED"
        name = "Complaint Created"
        entityType = "Complaint"
        description = "Triggered when a new complaint is created"
        category = "Complaint Lifecycle"
        availableFields = '["complaintId","complaintNumber","title","description","categoryName","priorityName","statusName","complainantName","complainantEmail","complainantEmployeeCode","createdDate","dueDate","companyName"]'
        isActive = $true
        isSystem = $false
    },
    @{
        code = "COMPLAINT_ASSIGNED"
        name = "Complaint Assigned"
        entityType = "Complaint"
        description = "Triggered when a complaint is assigned to a handler"
        category = "Complaint Lifecycle"
        availableFields = '["complaintId","complaintNumber","title","assignedToName","assignedToEmail","assignedByName","assignedByEmail","assignedDate"]'
        isActive = $true
        isSystem = $false
    },
    @{
        code = "COMPLAINT_STATUS_CHANGED"
        name = "Complaint Status Changed"
        entityType = "Complaint"
        description = "Triggered when complaint status is changed"
        category = "Complaint Lifecycle"
        availableFields = '["complaintId","complaintNumber","title","oldStatus","newStatus","changedByName","changedByEmail","changedDate","reason"]'
        isActive = $true
        isSystem = $false
    },
    @{
        code = "COMPLAINT_ESCALATED"
        name = "Complaint Escalated"
        entityType = "Complaint"
        description = "Triggered when a complaint is escalated"
        category = "Complaint Lifecycle"
        availableFields = '["complaintId","complaintNumber","title","escalationLevel","escalatedToName","escalatedToEmail","escalatedByName","escalatedByEmail","escalatedDate","reason"]'
        isActive = $true
        isSystem = $false
    },
    @{
        code = "COMPLAINT_RESOLVED"
        name = "Complaint Resolved"
        entityType = "Complaint"
        description = "Triggered when a complaint is resolved"
        category = "Complaint Lifecycle"
        availableFields = '["complaintId","complaintNumber","title","resolution","resolvedByName","resolvedByEmail","resolvedDate","resolutionNotes"]'
        isActive = $true
        isSystem = $false
    },
    @{
        code = "COMPLAINT_CLOSED"
        name = "Complaint Closed"
        entityType = "Complaint"
        description = "Triggered when a complaint is closed"
        category = "Complaint Lifecycle"
        availableFields = '["complaintId","complaintNumber","title","closedByName","closedByEmail","closedDate","closureNotes"]'
        isActive = $true
        isSystem = $false
    },
    @{
        code = "COMPLAINT_REOPENED"
        name = "Complaint Reopened"
        entityType = "Complaint"
        description = "Triggered when a closed complaint is reopened"
        category = "Complaint Lifecycle"
        availableFields = '["complaintId","complaintNumber","title","reopenedByName","reopenedByEmail","reopenedDate","reopenReason"]'
        isActive = $true
        isSystem = $false
    },
    @{
        code = "COMMENT_ADDED"
        name = "Comment Added"
        entityType = "Comment"
        description = "Triggered when a comment is added to a complaint"
        category = "Communication"
        availableFields = '["complaintId","complaintNumber","commentText","commentByName","commentByEmail","commentDate","isInternal"]'
        isActive = $true
        isSystem = $false
    },
    @{
        code = "SLA_WARNING"
        name = "SLA Warning"
        entityType = "Complaint"
        description = "Triggered when complaint is approaching SLA deadline"
        category = "SLA Management"
        availableFields = '["complaintId","complaintNumber","title","slaLevel","timeRemaining","dueDate","assignedToName","assignedToEmail"]'
        isActive = $true
        isSystem = $false
    },
    @{
        code = "SLA_BREACHED"
        name = "SLA Breached"
        entityType = "Complaint"
        description = "Triggered when complaint has breached SLA deadline"
        category = "SLA Management"
        availableFields = '["complaintId","complaintNumber","title","slaLevel","breachTime","dueDate","assignedToName","assignedToEmail"]'
        isActive = $true
        isSystem = $false
    }
)

$successCount = 0
$failureCount = 0
$createdEventTypes = @()

foreach ($eventType in $eventTypes) {
    Write-Host "`nCreating: $($eventType.code)..." -ForegroundColor Yellow

    try {
        $body = $eventType | ConvertTo-Json -Depth 10

        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/event-types" `
            -Method POST `
            -Headers @{Authorization="Bearer $token"} `
            -Body $body `
            -ContentType "application/json"

        Write-Host "  SUCCESS!" -ForegroundColor Green
        Write-Host "    ID: $($response.id)" -ForegroundColor Gray
        $successCount++

        $createdEventTypes += @{
            code = $eventType.code
            id = $response.id
            name = $eventType.name
        }
    } catch {
        $errorDetail = $_.ErrorDetails.Message
        Write-Host "  FAILED: $_" -ForegroundColor Red
        if ($errorDetail) {
            Write-Host "    Details: $errorDetail" -ForegroundColor Gray
        }
        $failureCount++
    }

    Start-Sleep -Milliseconds 200
}

Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Successful: $successCount" -ForegroundColor Green
Write-Host "Failed: $failureCount" -ForegroundColor $(if ($failureCount -gt 0) { "Red" } else { "Gray" })

if ($createdEventTypes.Count -gt 0) {
    Write-Host "`nCreated Event Types:" -ForegroundColor Yellow
    $createdEventTypes | ForEach-Object {
        Write-Host "  - $($_.code)" -ForegroundColor Green
    }

    $createdEventTypes | ConvertTo-Json | Out-File "created-event-types.json"
    Write-Host "`nIDs saved to: created-event-types.json" -ForegroundColor Cyan
}

Write-Host "================================================" -ForegroundColor Cyan
