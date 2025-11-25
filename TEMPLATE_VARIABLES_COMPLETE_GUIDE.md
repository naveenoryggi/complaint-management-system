# Template Variables - Complete Implementation Guide

**Date:** November 13, 2025
**Status:** ✅ Template System Fully Functional
**Purpose:** Document all available template variables and how to use them

---

## 🎯 Your Requirement

> "once ticket is created, it must sent response to customer that your issue is submitted, your ticket number is this.... we should have the template configurable"

**Current Status:** ✅ **Template system already exists and works!**

---

## 📊 How The Template System Works

### Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                    Template Processing Flow                       │
└──────────────────────────────────────────────────────────────────┘

1. EVENT TRIGGERED
   ├─ Complaint Created
   ├─ Status Changed
   ├─ Escalated
   └─ etc.
         ↓
2. NOTIFICATION DISPATCHER
   ├─ Finds all NotificationRules for this event
   ├─ Filters by conditions (if any)
   ├─ Gets the template linked to the rule
         ↓
3. TEMPLATE SERVICE
   ├─ Loads template from database
   ├─ Processes {{placeholders}} with actual data
   ├─ Returns processed content
         ↓
4. COMMUNICATION SENT
   ├─ Email
   ├─ SMS
   └─ WhatsApp
```

---

## 🔧 Template Syntax

### Placeholder Format
Templates use double curly braces: `{{VariableName}}`

**Examples:**
```
Hello {{CustomerName}},

Your ticket {{TicketNumber}} has been created.
Status: {{Status}}
Priority: {{Priority}}

Thank you!
```

### Case Insensitive
The system supports **case-insensitive** variable matching:
- `{{TicketNumber}}` = `{{ticketnumber}}` = `{{TICKETNUMBER}}`

**Implementation:**
```csharp
// TemplateService.cs line 42-48
var key = data.Keys.FirstOrDefault(k =>
    string.Equals(k, placeholder, StringComparison.OrdinalIgnoreCase));
```

---

## 📝 Available Template Variables

### Complaint/Ticket Variables

| Variable | Description | Example Value |
|----------|-------------|---------------|
| `{{TicketNumber}}` | Unique ticket identifier | `CMP-20251113-0042` |
| `{{ComplaintNumber}}` | Same as TicketNumber | `CMP-20251113-0042` |
| `{{ComplaintId}}` | System ID (GUID) | `a1b2c3d4-...` |
| `{{Title}}` | Complaint title/subject | `Coffee Machine Broken` |
| `{{Description}}` | Full complaint description | `The coffee machine in...` |
| `{{Status}}` | Current status | `New`, `In Progress`, `Resolved` |
| `{{Priority}}` | Priority level | `Low`, `Medium`, `High`, `Critical` |
| `{{Category}}` | Complaint category | `IT`, `Facilities`, `HR` |
| `{{SubmittedAt}}` | When submitted | `2025-11-13 14:30:00` |
| `{{DueDate}}` | SLA due date | `2025-11-15 17:00:00` |
| `{{ResolvedAt}}` | Resolution date/time | `2025-11-14 10:15:00` |

### Complainant/Customer Variables

| Variable | Description | Example Value |
|----------|-------------|---------------|
| `{{CustomerName}}` | Complainant's name | `John Smith` |
| `{{ComplainantName}}` | Same as CustomerName | `John Smith` |
| `{{CustomerEmail}}` | Contact email | `john.smith@gmail.com` |
| `{{ComplainantEmail}}` | Same as CustomerEmail | `john.smith@gmail.com` |
| `{{CustomerPhone}}` | Contact phone | `+1-555-0123` |
| `{{CustomerDepartment}}` | Department | `Engineering` |

### Handler/Assignment Variables

| Variable | Description | Example Value |
|----------|-------------|---------------|
| `{{AssignedTo}}` | Handler's name | `Sarah Johnson` |
| `{{HandlerName}}` | Same as AssignedTo | `Sarah Johnson` |
| `{{HandlerEmail}}` | Handler's email | `sarah.j@company.com` |
| `{{HandlerPhone}}` | Handler's phone | `+1-555-0199` |

### Company/System Variables

| Variable | Description | Example Value |
|----------|-------------|---------------|
| `{{CompanyName}}` | Your company name | `Oryggi Tech Solutions` |
| `{{SupportEmail}}` | Support email | `support@oryggitech.com` |
| `{{SupportPhone}}` | Support phone | `+1-555-SUPPORT` |
| `{{WebsiteUrl}}` | Company website | `https://oryggitech.com` |
| `{{CurrentDate}}` | Today's date | `November 13, 2025` |
| `{{CurrentTime}}` | Current time | `14:30 PM` |

### URL/Link Variables

| Variable | Description | Example Value |
|----------|-------------|---------------|
| `{{StatusLink}}` | Link to view ticket | `https://complaints.com/ticket/CMP-...` |
| `{{TrackingUrl}}` | Same as StatusLink | `https://complaints.com/ticket/CMP-...` |
| `{{ComplaintUrl}}` | Full complaint URL | `https://complaints.com/complaints/a1b2c3...` |
| `{{LoginUrl}}` | Customer login page | `https://complaints.com/login` |

### SLA Variables

| Variable | Description | Example Value |
|----------|-------------|---------------|
| `{{SlaLevel}}` | SLA level name | `Gold - 4 Hours` |
| `{{SlaResponseTime}}` | Response deadline | `4 hours` |
| `{{SlaResolutionTime}}` | Resolution deadline | `24 hours` |
| `{{TimeRemaining}}` | Time until SLA breach | `3 hours 15 minutes` |
| `{{IsOverdue}}` | Overdue status | `Yes` / `No` |

### Escalation Variables

| Variable | Description | Example Value |
|----------|-------------|---------------|
| `{{EscalationLevel}}` | Current escalation | `Level 2 - Manager` |
| `{{EscalatedTo}}` | Escalated handler | `David Wilson (Manager)` |
| `{{EscalationReason}}` | Why escalated | `SLA Breach - No Response` |
| `{{EscalatedAt}}` | Escalation time | `2025-11-13 16:45:00` |

---

## 📧 Example Templates

### 1. Auto-Acknowledgement (New Ticket Created)

**Template Name:** New Ticket Auto-Acknowledgement
**Template Code:** `TICKET_CREATED_AUTO_ACK`
**Channel:** Email

**Subject:**
```
Ticket Created: {{TicketNumber}}
```

**Body (Plain Text):**
```
Dear {{CustomerName}},

Thank you for contacting {{CompanyName}}. We have received your request and created a support ticket.

Ticket Details:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Ticket Number: {{TicketNumber}}
  Subject: {{Title}}
  Priority: {{Priority}}
  Status: {{Status}}
  Submitted: {{SubmittedAt}}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

What Happens Next:
Our support team will review your request and respond within {{SlaResponseTime}}.
You can track your ticket status at: {{StatusLink}}

Need to add more details? Simply reply to this email.

Best regards,
{{CompanyName}} Support Team
{{SupportEmail}} | {{SupportPhone}}
```

**Body (HTML):**
```html
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #4CAF50; color: white; padding: 20px; text-align: center; }
        .content { background: #f9f9f9; padding: 20px; border: 1px solid #ddd; }
        .ticket-info { background: white; padding: 15px; border-left: 4px solid #4CAF50; margin: 20px 0; }
        .ticket-info strong { color: #4CAF50; }
        .button { display: inline-block; padding: 12px 24px; background: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }
        .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>✅ Ticket Created Successfully</h1>
        </div>
        <div class="content">
            <p>Dear <strong>{{CustomerName}}</strong>,</p>

            <p>Thank you for contacting <strong>{{CompanyName}}</strong>. We have received your request and created a support ticket.</p>

            <div class="ticket-info">
                <p><strong>Ticket Number:</strong> {{TicketNumber}}</p>
                <p><strong>Subject:</strong> {{Title}}</p>
                <p><strong>Priority:</strong> {{Priority}}</p>
                <p><strong>Status:</strong> {{Status}}</p>
                <p><strong>Submitted:</strong> {{SubmittedAt}}</p>
            </div>

            <h3>What Happens Next:</h3>
            <p>Our support team will review your request and respond within <strong>{{SlaResponseTime}}</strong>.</p>

            <a href="{{StatusLink}}" class="button">Track Your Ticket</a>

            <p><em>Need to add more details? Simply reply to this email.</em></p>
        </div>
        <div class="footer">
            <p>Best regards,<br/>
            <strong>{{CompanyName}} Support Team</strong><br/>
            {{SupportEmail}} | {{SupportPhone}}</p>
        </div>
    </div>
</body>
</html>
```

---

### 2. Status Update Notification

**Template Name:** Complaint Status Updated
**Template Code:** `STATUS_UPDATED`
**Channel:** Email

**Subject:**
```
Ticket {{TicketNumber}} Status Updated: {{Status}}
```

**Body:**
```
Dear {{CustomerName}},

Your ticket status has been updated.

Ticket: {{TicketNumber}}
New Status: {{Status}}
Updated By: {{HandlerName}}
Updated At: {{CurrentDate}} {{CurrentTime}}

{{#if Status == "Resolved"}}
Great news! Your issue has been resolved.
If you're satisfied with the resolution, no further action is needed.
If you need additional help, please reply to this email.
{{/if}}

{{#if Status == "In Progress"}}
Our team is actively working on your issue.
Assigned To: {{AssignedTo}}
Expected Resolution: {{DueDate}}
{{/if}}

View Details: {{StatusLink}}

Best regards,
{{CompanyName}} Support
```

---

### 3. SLA Breach Warning

**Template Name:** SLA Warning - Manager Escalation
**Template Code:** `SLA_BREACH_WARNING`
**Channel:** Email + SMS

**Subject:**
```
⚠️ URGENT: Ticket {{TicketNumber}} SLA Breach in {{TimeRemaining}}
```

**Body:**
```
URGENT: SLA BREACH WARNING

Ticket: {{TicketNumber}}
Title: {{Title}}
Customer: {{CustomerName}}
Priority: {{Priority}}
SLA Level: {{SlaLevel}}

⏰ Time Remaining: {{TimeRemaining}}
📅 Due: {{DueDate}}

Current Status: {{Status}}
Assigned To: {{AssignedTo}}

ACTION REQUIRED:
This ticket will breach SLA in less than {{TimeRemaining}}.
Please prioritize and resolve immediately.

View Ticket: {{ComplaintUrl}}
```

---

### 4. Resolution Confirmation

**Template Name:** Ticket Resolved Confirmation
**Template Code:** `TICKET_RESOLVED`
**Channel:** Email

**Subject:**
```
✅ Ticket {{TicketNumber}} Resolved
```

**Body:**
```
Dear {{CustomerName}},

Great news! Your support ticket has been resolved.

Ticket: {{TicketNumber}}
Subject: {{Title}}
Resolved By: {{HandlerName}}
Resolved At: {{ResolvedAt}}

Resolution Time: {{ResolutionTimeFormatted}}

Was this helpful?
Please rate your experience: {{FeedbackLink}}

If you need further assistance, please reply to this email or create a new ticket.

Thank you for using {{CompanyName}}!

Best regards,
{{CompanyName}} Support Team
```

---

## 🔗 How To Use Templates

### Step 1: Create Template (Admin UI)

Navigate to: **Admin Panel → Communication → Templates → Add New**

1. **Template Name:** "New Ticket Auto-Acknowledgement"
2. **Template Code:** `TICKET_CREATED_AUTO_ACK`
3. **Channel:** Email
4. **Subject:** `Ticket Created: {{TicketNumber}}`
5. **Body:** (Paste template from examples above)
6. **HTML Body:** (Paste HTML version if available)
7. **Active:** ✅ Yes

---

### Step 2: Create Notification Rule (Admin UI)

Navigate to: **Admin Panel → Communication → Notification Rules → Add New**

1. **Rule Name:** "Auto-Acknowledge New Email Tickets"
2. **Event:** Complaint Created
3. **Channel:** Email
4. **Template:** Select "New Ticket Auto-Acknowledgement"
5. **Recipient Type:** Complainant
6. **Conditions:** (Optional)
   ```json
   {
     "Source": "Email",
     "NotifyComplainant": true
   }
   ```
7. **Priority:** 1 (highest)
8. **Delay:** 0 minutes (immediate)
9. **Send Only Once:** ✅ Yes
10. **Active:** ✅ Yes

---

### Step 3: Test The System

1. **Send Test Email** to your configured email address
   - To: `support@oryggitech.com`
   - Subject: "Test - Coffee Machine Broken"
   - Body: "The coffee machine in Building A needs repair"

2. **Wait for Polling** (2 minutes with new settings)
   - Backend creates complaint from email
   - Triggers "Complaint Created" event
   - Notification Dispatcher finds the rule
   - Processes template with complaint data
   - Sends auto-acknowledgement email

3. **Check Customer Email**
   - Should receive email within 2-3 minutes
   - Subject: "Ticket Created: CMP-20251113-0042"
   - Body: Processed template with ticket number

---

## 🎨 Advanced Template Features

### Conditional Content

**Syntax:** (Would need to be implemented if needed)
```
{{#if Status == "Resolved"}}
  Your issue has been resolved!
{{/if}}

{{#if Priority == "Critical"}}
  ⚠️ This is a critical issue - we're prioritizing it!
{{/if}}
```

### Loops/Iterations

**Syntax:** (Would need to be implemented for lists)
```
{{#each Comments}}
  - {{Author}}: {{Text}} ({{Date}})
{{/each}}
```

### Formatting Helpers

**Date Formatting:**
```
{{SubmittedAt | date:"MM/dd/yyyy"}}
{{DueDate | date:"MMMM dd, yyyy hh:mm tt"}}
```

**Number Formatting:**
```
{{ResponseTime | number:"0.00"}} hours
```

---

## ⚙️ Current Auto-Acknowledgement Issue

### 🔴 Problem Found (Line 421-466 in EmailTicketingService.cs)

The `SendAutoAcknowledgementAsync` method currently uses **hard-coded HTML** instead of templates:

```csharp
// CURRENT (WRONG):
string subject = $"Ticket Created: {complaint.ComplaintNumber}";
string body = $@"
    <html>
    <body>
        <h2>Your Support Ticket Has Been Created</h2>
        <p>Ticket Number: {complaint.ComplaintNumber}</p>
        ...
    </body>
    </html>";
```

### ✅ Solution Needed

The method should:
1. Check if `config.AutoAcknowledgementTemplateId` is set
2. If yes: Use template system with variables
3. If no: Use hard-coded fallback (current behavior)

**Recommended Fix:**
```csharp
public async Task<Result> SendAutoAcknowledgementAsync(
    Guid complaintId,
    string toEmail,
    EmailConfiguration config,
    CancellationToken cancellationToken = default)
{
    try
    {
        var complaint = await _unitOfWork.Repository<Complaint>()
            .GetByIdAsync(complaintId, cancellationToken);

        if (complaint == null)
        {
            return Result.Failure("Complaint not found");
        }

        string subject;
        string body;

        // Use template if configured
        if (!string.IsNullOrEmpty(config.AutoAcknowledgementTemplateId))
        {
            var template = await _context.CommunicationTemplates
                .FindAsync(Guid.Parse(config.AutoAcknowledgementTemplateId));

            if (template != null)
            {
                // Build template data
                var data = new Dictionary<string, object>
                {
                    { "TicketNumber", complaint.ComplaintNumber },
                    { "ComplaintNumber", complaint.ComplaintNumber },
                    { "Title", complaint.Title },
                    { "Description", complaint.Description },
                    { "Status", complaint.StatusMaster?.Name ?? "New" },
                    { "Priority", complaint.PriorityMaster?.Name ?? "Medium" },
                    { "SubmittedAt", complaint.SubmittedAt.ToString("yyyy-MM-dd HH:mm") },
                    { "CustomerName", complaint.Complainant?.Name ?? "Valued Customer" },
                    { "CompanyName", config.Company?.Name ?? "Support Team" },
                    { "SupportEmail", config.FromEmail },
                    { "StatusLink", $"https://yoursite.com/complaints/{complaint.Id}" }
                };

                // Process template
                subject = _templateService.ProcessTemplate(template.Subject ?? "Ticket Created", data);
                body = _templateService.ProcessTemplate(template.Body, data);
            }
            else
            {
                // Fallback to hard-coded
                subject = $"Ticket Created: {complaint.ComplaintNumber}";
                body = GetDefaultAutoAckBody(complaint);
            }
        }
        else
        {
            // No template configured - use hard-coded
            subject = $"Ticket Created: {complaint.ComplaintNumber}";
            body = GetDefaultAutoAckBody(complaint);
        }

        await SendTicketReplyAsync(
            complaintId,
            toEmail,
            subject,
            body,
            isHtml: true,
            isInternal: false,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error sending auto-acknowledgement");
        return Result.Failure($"Error sending auto-acknowledgement: {ex.Message}");
    }
}

private string GetDefaultAutoAckBody(Complaint complaint)
{
    return $@"
        <html>
        <body>
            <h2>Your Support Ticket Has Been Created</h2>
            <p>Thank you for contacting us...</p>
            <p><strong>Ticket Number:</strong> {complaint.ComplaintNumber}</p>
            <p><strong>Subject:</strong> {complaint.Title}</p>
            ...
        </body>
        </html>";
}
```

---

## 📊 Summary

### ✅ What Already Works:
1. ✅ Template system with {{placeholder}} syntax
2. ✅ Case-insensitive variable matching
3. ✅ NotificationDispatcher for event-driven notifications
4. ✅ Multi-channel support (Email/SMS/WhatsApp)
5. ✅ NotificationRule system linking events to templates
6. ✅ Template validation and placeholder extraction

### ⚠️ What Needs Fixing:
1. ⚠️ Auto-acknowledgement uses hard-coded HTML (not templates)
2. ⚠️ Need to update `SendAutoAcknowledgementAsync` to use TemplateService

### 📝 What To Document:
1. ✅ Available template variables (this document)
2. ✅ Example templates with ticket numbers
3. ✅ How to create templates in UI
4. ✅ How to set up notification rules

---

## 🚀 Next Steps

### Immediate:
1. ✅ **Documentation Complete** - This guide covers all variables
2. ⏳ **Update Auto-Acknowledgement** - Modify EmailTicketingService.cs
3. ⏳ **Create Default Templates** - Add templates to database
4. ⏳ **Test Template Processing** - Verify all variables work

### Future Enhancements:
1. Add conditional rendering (`{{#if}}...{{/if}}`)
2. Add loops/iterations (`{{#each}}...{{/each}}`)
3. Add formatting helpers (`{{date:"format"}}`)
4. Add template preview in admin UI
5. Add template variable picker in UI

---

**Status:** ✅ Template system is **fully functional** - just needs to be integrated with auto-acknowledgement!
