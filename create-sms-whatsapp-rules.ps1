$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRW1wbG95ZWVDb2RlIjoiQURNSU4wMDEiLCJDb21wYW55SWQiOiJmZTI4Y2Q4NS00MjI2LTRkYWEtOWU0NS02NmEzZDUxODc3ZmEiLCJQZXJtaXNzaW9uIjpbIlZpZXdDb21wbGFpbnRzIiwiQWRkQ29tbWVudCIsIkVzY2FsYXRlQ29tcGxhaW50IiwiTWFuYWdlVXNlcnMiLCJWaWV3QXVkaXRMb2dzIiwiRWRpdENvbXBsYWludCIsIkNyZWF0ZUNvbXBsYWludCIsIlZpZXdDb21tZW50cyIsIk1hbmFnZVJvbGVzIiwiTWFuYWdlRXNjYWxhdGlvbiIsIlZpZXdBdHRhY2htZW50cyIsIlZpZXdFc2NhbGF0aW9uIiwiQXNzaWduQ29tcGxhaW50IiwiVmlld1JlcG9ydHMiLCJEZWxldGVDb21wbGFpbnQiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MTIxNjQxMCwiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.J-rRMZtpLFyvzC-J4KrIsKvtiwaaf9aQw2uhy0HH82Q"
$baseUrl = "http://localhost:5058/api/communication"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "Fetching event types..." -ForegroundColor Cyan
$eventTypesResponse = Invoke-RestMethod -Uri "$baseUrl/event-types" -Method GET -Headers $headers
$eventTypes = $eventTypesResponse | Where-Object { $_.code -in @("COMPLAINT_CREATED", "COMPLAINT_ASSIGNED", "COMPLAINT_CLOSED", "COMPLAINT_ESCALATED", "COMPLAINT_OVERDUE") }

Write-Host "Fetching templates..." -ForegroundColor Cyan
$templatesResponse = Invoke-RestMethod -Uri "$baseUrl/templates" -Method GET -Headers $headers
$templates = $templatesResponse | Where-Object { $_.channel -in @(1, 2) }  # SMS=1, WhatsApp=2

Write-Host "`nFound $($eventTypes.Count) event types and $($templates.Count) templates`n" -ForegroundColor Green

# Mapping of events to templates and recipients
$ruleMappings = @(
    @{
        EventCode = "COMPLAINT_CREATED"
        SmsTemplate = "COMPLAINT_CREATED_SMS"
        WhatsAppTemplate = "COMPLAINT_CREATED_WHATSAPP"
        RecipientType = 0  # Complainant
        Description = "Notify complainant when complaint is created"
    },
    @{
        EventCode = "COMPLAINT_ASSIGNED"
        SmsTemplate = "COMPLAINT_ASSIGNED_SMS"
        WhatsAppTemplate = "COMPLAINT_ASSIGNED_WHATSAPP"
        RecipientType = 1  # AssignedHandler
        Description = "Notify handler when complaint is assigned"
    },
    @{
        EventCode = "COMPLAINT_CLOSED"
        SmsTemplate = "COMPLAINT_CLOSED_SMS"
        WhatsAppTemplate = "COMPLAINT_CLOSED_WHATSAPP"
        RecipientType = 0  # Complainant
        Description = "Notify complainant when complaint is closed"
    },
    @{
        EventCode = "COMPLAINT_ESCALATED"
        SmsTemplate = "COMPLAINT_ESCALATED_SMS"
        WhatsAppTemplate = "COMPLAINT_ESCALATED_WHATSAPP"
        RecipientType = 4  # EscalationHandler
        Description = "Notify escalation handler"
    },
    @{
        EventCode = "COMPLAINT_OVERDUE"
        SmsTemplate = "COMPLAINT_OVERDUE_SMS"
        WhatsAppTemplate = "COMPLAINT_OVERDUE_WHATSAPP"
        RecipientType = 1  # AssignedHandler
        Description = "Notify handler of overdue complaint"
    }
)

$successCount = 0
$errorCount = 0

foreach ($mapping in $ruleMappings) {
    $event = $eventTypes | Where-Object { $_.code -eq $mapping.EventCode } | Select-Object -First 1

    if (-not $event) {
        Write-Host "Event $($mapping.EventCode) not found, skipping..." -ForegroundColor Yellow
        continue
    }

    # Create SMS rule
    $smsTemplate = $templates | Where-Object { $_.code -eq $mapping.SmsTemplate } | Select-Object -First 1
    if ($smsTemplate) {
        Write-Host "Creating SMS rule: $($mapping.EventCode) -> $($mapping.SmsTemplate)..." -ForegroundColor White

        $smsRule = @{
            name = "$($event.name) - SMS Notification"
            eventTypeId = $event.id
            templateId = $smsTemplate.id
            recipientType = $mapping.RecipientType
            channel = 1  # SMS
            priority = 2  # Normal priority
            isActive = $true
            description = "$($mapping.Description) via SMS"
        } | ConvertTo-Json

        try {
            $result = Invoke-RestMethod -Uri "$baseUrl/notification-rules" -Method POST -Headers $headers -Body $smsRule
            Write-Host "  Success! Rule ID: $($result.id)" -ForegroundColor Green
            $successCount++
        } catch {
            Write-Host "  Error: $_" -ForegroundColor Red
            $errorCount++
        }
    }

    # Create WhatsApp rule
    $whatsappTemplate = $templates | Where-Object { $_.code -eq $mapping.WhatsAppTemplate } | Select-Object -First 1
    if ($whatsappTemplate) {
        Write-Host "Creating WhatsApp rule: $($mapping.EventCode) -> $($mapping.WhatsAppTemplate)..." -ForegroundColor White

        $whatsappRule = @{
            name = "$($event.name) - WhatsApp Notification"
            eventTypeId = $event.id
            templateId = $whatsappTemplate.id
            recipientType = $mapping.RecipientType
            channel = 2  # WhatsApp
            priority = 2  # Normal priority
            isActive = $true
            description = "$($mapping.Description) via WhatsApp"
        } | ConvertTo-Json

        try {
            $result = Invoke-RestMethod -Uri "$baseUrl/notification-rules" -Method POST -Headers $headers -Body $whatsappRule
            Write-Host "  Success! Rule ID: $($result.id)" -ForegroundColor Green
            $successCount++
        } catch {
            Write-Host "  Error: $_" -ForegroundColor Red
            $errorCount++
        }
    }

    Write-Host ""
}

Write-Host "`n========== Summary ==========" -ForegroundColor Cyan
Write-Host "Successfully created: $successCount rules" -ForegroundColor Green
Write-Host "Errors: $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
Write-Host "============================`n" -ForegroundColor Cyan
