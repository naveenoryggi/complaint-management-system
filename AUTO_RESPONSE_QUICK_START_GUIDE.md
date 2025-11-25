# Auto-Response Email System - Quick Start Guide

## What is it?

The **Auto-Response Email System** automatically sends emails when specific events occur in the complaint management system:

- Customer submits a complaint → **Acknowledgment email sent**
- Complaint assigned to handler → **Assignment notification sent**
- Status changes → **Status update email sent**
- Complaint resolved → **Resolution email sent**
- Complaint reassigned → **Both handlers notified**

---

## Quick Configuration

### Enable/Disable Auto-Responses

Edit `appsettings.json`:

```json
{
  "AutoResponse": {
    "Enabled": true,                          // Master switch
    "SendAcknowledgmentOnWebCreation": true,  // Web-created complaints
    "StatusChangeNotifications": true,        // Status changes
    "AssignmentNotifications": true,          // Assignments/reassignments
    "ResolutionNotifications": true,          // Resolutions
    "CommentNotifications": false,            // Comments (usually disabled)
    "EscalationNotifications": true,          // Escalations
    "SLANotifications": true                  // SLA warnings/breaches
  }
}
```

### Disable All Auto-Responses
```json
{ "AutoResponse": { "Enabled": false } }
```

---

## Testing

### Run the Test Script
```powershell
.\test-auto-response-system.ps1
```

This will:
1. Create a test complaint
2. Assign it to a handler
3. Change its status
4. Resolve it
5. Reassign it
6. Show results and logs

### Manual Testing Checklist
- [ ] Create complaint → Check complainant email for acknowledgment
- [ ] Assign complaint → Check handler email for notification
- [ ] Change status → Check complainant email for update
- [ ] Resolve complaint → Check complainant email for resolution
- [ ] Reassign complaint → Check both handlers' emails

---

## Template Variables

Use these in your email templates:

### Essential Variables
```
{{ComplaintNumber}}  - Ticket number (e.g., CMP-20251115-0001)
{{Title}}            - Complaint title
{{CustomerName}}     - Complainant name
{{Status}}           - Current status
{{Priority}}         - Priority level
{{CompanyName}}      - Your company name
{{ComplaintUrl}}     - Link to view complaint
```

### Date Variables
```
{{SubmittedDate}}    - When submitted (formatted)
{{DueDate}}          - SLA due date
{{CurrentDate}}      - Today's date
```

### Status Change Variables
```
{{OldStatus}}        - Previous status
{{NewStatus}}        - New status
{{StatusTransition}} - "Old → New"
```

### Assignment Variables
```
{{AssignedToName}}   - Handler name
{{AssignedToEmail}}  - Handler email
```

### Full List
See `AUTO_RESPONSE_EMAIL_SYSTEM_IMPLEMENTATION_REPORT.md` for all 40+ variables.

---

## Common Use Cases

### 1. Acknowledgment Email Template
```html
<h2>Ticket Created: {{ComplaintNumber}}</h2>
<p>Dear {{CustomerName}},</p>
<p>Your complaint has been received.</p>
<p><strong>Title:</strong> {{Title}}</p>
<p><strong>Status:</strong> {{Status}}</p>
<p><strong>Priority:</strong> {{Priority}}</p>
<p>Track your complaint: <a href="{{ComplaintUrl}}">Click here</a></p>
```

### 2. Status Change Template
```html
<h2>Status Update: {{ComplaintNumber}}</h2>
<p>Dear {{CustomerName}},</p>
<p>Your complaint status has changed:</p>
<p><strong>{{OldStatus}}</strong> → <strong>{{NewStatus}}</strong></p>
<p>View details: <a href="{{ComplaintUrl}}">Click here</a></p>
```

### 3. Resolution Template
```html
<h2>Complaint Resolved: {{ComplaintNumber}}</h2>
<p>Dear {{CustomerName}},</p>
<p>Your complaint has been resolved.</p>
<p><strong>Resolution:</strong> {{ResolutionNotes}}</p>
<p>If you need further assistance, please contact us.</p>
```

---

## Troubleshooting

### Problem: Auto-Responses Not Sending

**Solution 1: Check Configuration**
```json
// Make sure enabled in appsettings.json
{ "AutoResponse": { "Enabled": true } }
```

**Solution 2: Check Email Configuration**
- Go to Admin Panel → Email Configuration
- Verify email settings are configured and enabled
- Test SMTP connection

**Solution 3: Check Logs**
```sql
-- Check recent communication logs
SELECT TOP 20 * FROM CommunicationLogs
ORDER BY CreatedAt DESC

-- Check for errors
SELECT * FROM CommunicationLogs
WHERE Status = 'Failed'
ORDER BY CreatedAt DESC
```

### Problem: Emails Go to Spam

**Solution:**
- Configure SPF, DKIM, DMARC records for your domain
- Use a reputable SMTP service (Office 365, SendGrid, etc.)
- Verify email content doesn't trigger spam filters

### Problem: Wrong Template Variables

**Solution:**
- Check template syntax (use `{{VariableName}}` not `{VariableName}`)
- Verify variable name spelling (case-sensitive)
- See full variable list in documentation

---

## Architecture

```
Complaint Event (Create, Assign, Update, Resolve)
    ↓
Command Handler (CreateComplaintCommandHandler, etc.)
    ↓
AutoResponseService.SendXxxAutoResponseAsync()
    ↓
NotificationDispatcher (Evaluates EventCommunicationRules)
    ↓
TemplateService (Substitutes variables)
    ↓
EmailTicketingService (Sends via SMTP)
    ↓
EmailMessage + CommunicationLog (Saved to database)
```

---

## Key Files

### Backend
- **Interface:** `ComplaintManagement.Application/Interfaces/Services/IAutoResponseService.cs`
- **Implementation:** `ComplaintManagement.Infrastructure/Services/AutoResponseService.cs`
- **Config:** `ComplaintManagement.API/appsettings.json`
- **DI Registration:** `ComplaintManagement.Infrastructure/DependencyInjection.cs`

### Handlers (Integration Points)
- `CreateComplaintCommandHandler.cs` - Acknowledgment emails
- `UpdateComplaintCommandHandler.cs` - Status change & resolution emails
- `AssignComplaintCommandHandler.cs` - Assignment/reassignment emails

### Testing
- **Test Script:** `test-auto-response-system.ps1`
- **Full Documentation:** `AUTO_RESPONSE_EMAIL_SYSTEM_IMPLEMENTATION_REPORT.md`

---

## FAQ

**Q: Do auto-responses work with email-created complaints?**
A: Yes! When a complaint is created via email (EmailTicketingService), the acknowledgment is sent automatically.

**Q: Can I customize which events trigger emails?**
A: Yes! Use the configuration settings in `appsettings.json` to enable/disable each event type.

**Q: Can I use different templates for different companies?**
A: Yes! Templates and EventCommunicationRules support company-specific configuration.

**Q: What happens if email sending fails?**
A: The error is logged in CommunicationLog and application logs, but the main operation (create complaint, etc.) continues successfully.

**Q: Can I test without actually sending emails?**
A: Set `AutoResponse.Enabled = false` in appsettings.json. The system will log what would have been sent without actually sending.

**Q: How do I add more template variables?**
A: Edit `AutoResponseService.BuildComplaintTemplateData()` to add new variables.

**Q: Can I send to multiple recipients?**
A: Yes! Use EventCommunicationRules to configure multiple recipient types (Complainant, Handler, Manager, etc.).

**Q: Does this support SMS or WhatsApp?**
A: Not yet. Currently email only. SMS/WhatsApp support planned for future releases.

---

## Next Steps

1. **Configure Email Settings**
   - Admin Panel → Email Configuration
   - Set up SMTP credentials
   - Test connection

2. **Create Templates**
   - Admin Panel → Communication Templates
   - Use template variables
   - Preview before saving

3. **Configure Event Rules**
   - Admin Panel → Event Communication Rules
   - Map events to templates
   - Set recipient types

4. **Test the System**
   - Run `test-auto-response-system.ps1`
   - Verify emails are received
   - Check template variable substitution

5. **Monitor and Adjust**
   - Review CommunicationLogs regularly
   - Adjust templates based on user feedback
   - Fine-tune event rules

---

**For detailed implementation information, see:**
`AUTO_RESPONSE_EMAIL_SYSTEM_IMPLEMENTATION_REPORT.md`

**For troubleshooting and support:**
Check application logs and CommunicationLogs table
