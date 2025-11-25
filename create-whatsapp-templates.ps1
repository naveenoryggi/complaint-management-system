$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRW1wbG95ZWVDb2RlIjoiQURNSU4wMDEiLCJDb21wYW55SWQiOiJmZTI4Y2Q4NS00MjI2LTRkYWEtOWU0NS02NmEzZDUxODc3ZmEiLCJQZXJtaXNzaW9uIjpbIlZpZXdDb21wbGFpbnRzIiwiQWRkQ29tbWVudCIsIkVzY2FsYXRlQ29tcGxhaW50IiwiTWFuYWdlVXNlcnMiLCJWaWV3QXVkaXRMb2dzIiwiRWRpdENvbXBsYWludCIsIkNyZWF0ZUNvbXBsYWludCIsIlZpZXdDb21tZW50cyIsIk1hbmFnZVJvbGVzIiwiTWFuYWdlRXNjYWxhdGlvbiIsIlZpZXdBdHRhY2htZW50cyIsIlZpZXdFc2NhbGF0aW9uIiwiQXNzaWduQ29tcGxhaW50IiwiVmlld1JlcG9ydHMiLCJEZWxldGVDb21wbGFpbnQiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MTIxNjQxMCwiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.J-rRMZtpLFyvzC-J4KrIsKvtiwaaf9aQw2uhy0HH82Q"
$baseUrl = "http://localhost:5058/api/communication/templates"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Template 1: Complaint Created - WhatsApp
$body1 = @"
*Complaint Submitted*

Dear {{complainantName}},

Your complaint has been successfully submitted.

*Details:*
• Number: #{{complaintNumber}}
• Title: {{title}}
• Category: {{categoryName}}
• Priority: {{priorityName}}

We will review shortly.

Thank you,
{{companyName}}
"@

$template1 = @{
    name = "Complaint Created - WhatsApp"
    code = "COMPLAINT_CREATED_WHATSAPP"
    description = "WhatsApp template for new complaint creation"
    channel = 2
    subject = $null
    body = $body1
    htmlBody = $null
    isActive = $true
    isSystem = $true
} | ConvertTo-Json

Write-Host "Creating Complaint Created - WhatsApp template..."
try {
    Invoke-RestMethod -Uri $baseUrl -Method POST -Headers $headers -Body $template1
    Write-Host "Success!" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Template 2: Complaint Assigned - WhatsApp
$body2 = @"
*Complaint Assigned*

Dear {{assignedToName}},

A complaint has been assigned to you.

*Details:*
• Number: #{{complaintNumber}}
• Title: {{title}}
• Category: {{categoryName}}
• Priority: {{priorityName}}
• Complainant: {{complainantName}}
• Due: {{dueDate}}

Please review and take action.

{{companyName}}
"@

$template2 = @{
    name = "Complaint Assigned - WhatsApp"
    code = "COMPLAINT_ASSIGNED_WHATSAPP"
    description = "WhatsApp template for complaint assignment"
    channel = 2
    subject = $null
    body = $body2
    htmlBody = $null
    isActive = $true
    isSystem = $true
} | ConvertTo-Json

Write-Host "Creating Complaint Assigned - WhatsApp template..."
try {
    Invoke-RestMethod -Uri $baseUrl -Method POST -Headers $headers -Body $template2
    Write-Host "Success!" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Template 3: Complaint Closed - WhatsApp
$body3 = @"
*Complaint Closed*

Dear {{complainantName}},

Your complaint has been resolved.

*Details:*
• Number: #{{complaintNumber}}
• Title: {{title}}
• Closed By: {{closedBy}}
• Resolution: {{resolution}}

If you need further assistance, please let us know.

Thank you,
{{companyName}}
"@

$template3 = @{
    name = "Complaint Closed - WhatsApp"
    code = "COMPLAINT_CLOSED_WHATSAPP"
    description = "WhatsApp template for complaint closure"
    channel = 2
    subject = $null
    body = $body3
    htmlBody = $null
    isActive = $true
    isSystem = $true
} | ConvertTo-Json

Write-Host "Creating Complaint Closed - WhatsApp template..."
try {
    Invoke-RestMethod -Uri $baseUrl -Method POST -Headers $headers -Body $template3
    Write-Host "Success!" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Template 4: Complaint Escalated - SMS
$body4_sms = "URGENT: Complaint #{{complaintNumber}} escalated to Level {{escalationLevel}}. Reason: {{escalationReason}}. Please take immediate action. - {{companyName}}"

$template4_sms = @{
    name = "Complaint Escalated - SMS"
    code = "COMPLAINT_ESCALATED_SMS"
    description = "SMS template for complaint escalation"
    channel = 1
    subject = $null
    body = $body4_sms
    htmlBody = $null
    isActive = $true
    isSystem = $true
} | ConvertTo-Json

Write-Host "Creating Complaint Escalated - SMS template..."
try {
    Invoke-RestMethod -Uri $baseUrl -Method POST -Headers $headers -Body $template4_sms
    Write-Host "Success!" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Template 5: Complaint Escalated - WhatsApp
$body5 = @"
*⚠️ URGENT: Complaint Escalated*

Dear {{escalatedTo}},

A complaint has been escalated to your level.

*Details:*
• Number: #{{complaintNumber}}
• Title: {{title}}
• Priority: {{priorityName}}
• Escalation Level: {{escalationLevel}}
• Reason: {{escalationReason}}
• Complainant: {{complainantName}}

This requires URGENT attention.

{{companyName}}
"@

$template5 = @{
    name = "Complaint Escalated - WhatsApp"
    code = "COMPLAINT_ESCALATED_WHATSAPP"
    description = "WhatsApp template for complaint escalation"
    channel = 2
    subject = $null
    body = $body5
    htmlBody = $null
    isActive = $true
    isSystem = $true
} | ConvertTo-Json

Write-Host "Creating Complaint Escalated - WhatsApp template..."
try {
    Invoke-RestMethod -Uri $baseUrl -Method POST -Headers $headers -Body $template5
    Write-Host "Success!" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Template 6: Complaint Overdue - SMS
$body6_sms = "URGENT: Complaint #{{complaintNumber}} is {{daysOverdue}} days overdue. Due: {{dueDate}}. Please resolve immediately. - {{companyName}}"

$template6_sms = @{
    name = "Complaint Overdue - SMS"
    code = "COMPLAINT_OVERDUE_SMS"
    description = "SMS template for overdue complaint alert"
    channel = 1
    subject = $null
    body = $body6_sms
    htmlBody = $null
    isActive = $true
    isSystem = $true
} | ConvertTo-Json

Write-Host "Creating Complaint Overdue - SMS template..."
try {
    Invoke-RestMethod -Uri $baseUrl -Method POST -Headers $headers -Body $template6_sms
    Write-Host "Success!" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Template 7: Complaint Overdue - WhatsApp
$body7 = @"
*⚠️ URGENT: Complaint Overdue*

Dear {{assignedToName}},

Complaint #{{complaintNumber}} is now OVERDUE.

*Details:*
• Number: #{{complaintNumber}}
• Title: {{title}}
• Priority: {{priorityName}}
• Due Date: {{dueDate}}
• Days Overdue: {{daysOverdue}}

Please take IMMEDIATE action to resolve this complaint.

{{companyName}}
"@

$template7 = @{
    name = "Complaint Overdue - WhatsApp"
    code = "COMPLAINT_OVERDUE_WHATSAPP"
    description = "WhatsApp template for overdue complaint alert"
    channel = 2
    subject = $null
    body = $body7
    htmlBody = $null
    isActive = $true
    isSystem = $true
} | ConvertTo-Json

Write-Host "Creating Complaint Overdue - WhatsApp template..."
try {
    Invoke-RestMethod -Uri $baseUrl -Method POST -Headers $headers -Body $template7
    Write-Host "Success!" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host "`nAll templates created successfully!" -ForegroundColor Cyan
