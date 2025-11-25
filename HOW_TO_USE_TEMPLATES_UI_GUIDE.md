# How To Use Templates in Admin UI - Step-by-Step Guide

**Date:** November 13, 2025
**Purpose:** Complete guide to configure templates and auto-acknowledgement with ticket numbers
**Time Required:** 15-20 minutes

---

## 🎯 What You'll Achieve

By following this guide, you will:
1. ✅ Insert ready-to-use templates into the database
2. ✅ Configure auto-acknowledgement to use templates
3. ✅ Send emails with **{{TicketNumber}}** and other variables
4. ✅ Create notification rules for events
5. ✅ Test the complete flow

---

## 📋 Prerequisites

- ✅ Frontend running on http://localhost:4200
- ✅ Backend running on http://localhost:5000
- ✅ SQL Server Management Studio (SSMS) installed
- ✅ Admin login credentials

---

## 🚀 Step-by-Step Implementation

### STEP 1: Insert Default Templates (5 minutes)

**1.1 Open SQL Server Management Studio**

- Connect to: `PRANA-ASUS\SQLEXPRESS`
- Database: `ComplaintManagementDb`

**1.2 Open SQL Script**

- File → Open → File
- Navigate to: `C:\Users\Navin Chandra\Pictures\Complaint management system\`
- Select: `insert-default-templates.sql`

**1.3 Execute Script**

- Press **F5** or click **Execute**
- Wait for completion messages
- Should see 4 ✅ messages:
  ```
  ✅ Created template: Auto-Acknowledgement - New Ticket
  ✅ Created template: Status Update Notification
  ✅ Created template: Ticket Resolved Confirmation
  ✅ Created template: SLA Breach Warning
  ```

**1.4 Verify Templates Created**

Run this query to see your templates:
```sql
SELECT Code, Name, Channel, IsActive
FROM CommunicationTemplates
WHERE Code IN ('AUTO_ACK_NEW_TICKET', 'STATUS_UPDATED', 'TICKET_RESOLVED', 'SLA_BREACH_WARNING');
```

You should see 4 rows returned.

---

### STEP 2: Get Template ID for Auto-Acknowledgement (2 minutes)

**2.1 Copy Template ID**

Run this query to get the template ID:
```sql
SELECT
    Id AS [TemplateId],
    Name,
    Code
FROM CommunicationTemplates
WHERE Code = 'AUTO_ACK_NEW_TICKET' AND IsDeleted = 0;
```

**Example Output:**
```
TemplateId: 3FA85F64-5717-4562-B3FC-2C963F66AFA6
Name: Auto-Acknowledgement - New Ticket
Code: AUTO_ACK_NEW_TICKET
```

**2.2 Copy the TemplateId** (you'll need this in the next step)

---

### STEP 3: Configure Email Configuration (5 minutes)

**3.1 Login to Admin Panel**

1. Open browser: http://localhost:4200
2. Login with: `admin@complaintmanagement.com` / `Admin@123`

**3.2 Navigate to Email Ticketing Configuration**

1. Click **Admin Panel** (top right)
2. Click **Communication** menu
3. Click **Email Ticketing Configuration**

**3.3 Edit Email Configuration**

1. Find your email configuration (e.g., "Oryggi Tech Support")
2. Click the **Edit** button (pencil icon)

**3.4 Configure Auto-Acknowledgement Template**

The wizard will open. Navigate through the steps:

**Step 1: Authentication Type**
- Keep existing OAuth 2.0 settings
- Click **Next**

**Step 2: Provider Settings**
- Keep existing IMAP/SMTP settings
- Click **Next**

**Step 3: OAuth Credentials**
- Keep existing Client ID, Tenant ID, Secret
- Click **Next**

**Step 4: Additional Settings** ⭐ **IMPORTANT STEP**
- **Polling Interval:** Select "2 minutes (Fast - Recommended)"
- **IMAP Folder:** Keep as "INBOX"
- **Enable Email Ticketing:** ✅ Checked
- **Send Auto-Acknowledgement:** ✅ **CHECK THIS BOX**
- **Auto-Acknowledgement Template:** Paste the Template ID from Step 2
  - Example: `3FA85F64-5717-4562-B3FC-2C963F66AFA6`
- Click **Next**

**Step 5: Authorize Access**
- Click **Save Configuration**
- (If OAuth not authorized yet, click "Authorize Now")

**3.5 Verify Configuration Saved**

You should see:
- Success message: "Configuration saved successfully"
- Badge shows: "OAuth 2.0 - Authorized" (green)
- Poll interval: "Poll every 2 minutes"

---

### STEP 4: Apply Database Migration (1 minute)

Since we added the `pollingIntervalSeconds` field, we need to update the database:

**Option A - If backend is not running:**
```powershell
cd "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.API"
dotnet ef database update
```

**Option B - Restart backend:**
- The migration will be applied automatically on startup

---

### STEP 5: Test Auto-Acknowledgement with Ticket Number (5 minutes)

**5.1 Send Test Email**

Using any email client (Gmail, Outlook, etc.):

```
To: marketing@oryggitech.com
Subject: Test - Coffee Machine Broken
Body:
The coffee machine in Building A, 3rd floor is not working.
It needs urgent repair.

Please help!
```

**5.2 Wait for Polling**

- Backend polls every 2 minutes (120 seconds)
- Or click **"Poll Now"** button in the UI for immediate polling

**5.3 Check Backend Logs**

In the terminal where backend is running, you should see:
```
[INFO] Starting email poll for configuration...
[INFO] Connected to IMAP server outlook.office365.com
[INFO] Found 1 unread emails
[INFO] Created new complaint CMP-20251113-0042 from email
[INFO] Using template 3FA85F64-... for auto-acknowledgement
[INFO] Processed template for complaint, subject: Ticket Created: CMP-20251113-0042
[INFO] Sent auto-acknowledgement for complaint ... to sender@gmail.com
```

**5.4 Check Your Email Inbox**

Within 2-3 minutes, you should receive an auto-acknowledgement email:

**Subject:**
```
Ticket Created: CMP-20251113-0042
```

**Body:**
```
Dear Valued Customer,

Thank you for contacting [Your Company]. We have received your request
and created a support ticket.

Ticket Details:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Ticket Number: CMP-20251113-0042  ← YOUR TICKET NUMBER!
  Subject: Test - Coffee Machine Broken
  Priority: Medium
  Status: New
  Submitted: 2025-11-13 14:30
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

What Happens Next:
Our support team will review your request and respond as soon as possible.

Need to add more details? Simply reply to this email.

Best regards,
[Your Company] Support Team
```

✅ **SUCCESS!** The template processed `{{TicketNumber}}` and replaced it with the actual ticket number!

---

## 🎨 OPTIONAL: Create Custom Templates (10 minutes)

### Access Templates Management

1. **Login to Admin Panel:** http://localhost:4200
2. **Navigate to:** Admin Panel → Communication → Templates
3. **Click:** "Add New Template"

### Create Custom Template

**Template Information:**
- **Name:** My Custom Auto-Acknowledgement
- **Code:** `CUSTOM_AUTO_ACK` (unique identifier)
- **Description:** Custom template for our company
- **Channel:** Email
- **Active:** ✅ Yes

**Subject:**
```
Your Ticket {{TicketNumber}} - We're on it!
```

**Body (Plain Text):**
```
Hello {{CustomerName}},

Great news! We've received your request and assigned it ticket number: {{TicketNumber}}

Your Issue: {{Title}}
Priority: {{Priority}}
Status: {{Status}}

We'll get back to you within 24 hours.

Questions? Just reply to this email.

Cheers,
{{CompanyName}} Team
```

**HTML Body:**
```html
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                  color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: white; padding: 30px; border: 1px solid #ddd; }
        .ticket-box { background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;
                      border-left: 5px solid #667eea; }
        .ticket-number { font-size: 24px; color: #667eea; font-weight: bold; }
        .footer { text-align: center; padding: 20px; color: #999; font-size: 13px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1 style="margin: 0;">🎉 Ticket Created!</h1>
        </div>
        <div class="content">
            <p>Hello <strong>{{CustomerName}}</strong>,</p>
            <p>Great news! We've received your request and assigned it ticket number:</p>

            <div class="ticket-box">
                <div class="ticket-number">{{TicketNumber}}</div>
                <p style="margin: 10px 0 5px 0;"><strong>Your Issue:</strong> {{Title}}</p>
                <p style="margin: 5px 0;"><strong>Priority:</strong> {{Priority}}</p>
                <p style="margin: 5px 0;"><strong>Status:</strong> {{Status}}</p>
            </div>

            <p>We'll get back to you within 24 hours.</p>
            <p><em>Questions? Just reply to this email.</em></p>

            <p style="margin-top: 30px;">Cheers,<br/>
            <strong>{{CompanyName}} Team</strong></p>
        </div>
        <div class="footer">
            <p>This is an automated message from {{CompanyName}}</p>
        </div>
    </div>
</body>
</html>
```

**Save Template** and note the new Template ID, then update your Email Configuration to use it!

---

## 🔗 OPTIONAL: Create Notification Rules (10 minutes)

Instead of configuring auto-acknowledgement in Email Configuration, you can use **Notification Rules** for more flexibility.

### Create Notification Rule

1. **Navigate to:** Admin Panel → Communication → Notification Rules
2. **Click:** "Add New Rule"

**Rule Configuration:**

**Basic Information:**
- **Name:** Auto-Acknowledge New Email Tickets
- **Description:** Send acknowledgement when ticket created from email
- **Active:** ✅ Yes

**Event Configuration:**
- **Event Type:** Complaint Created
- **Channel:** Email
- **Template:** Select "Auto-Acknowledgement - New Ticket"

**Recipient Configuration:**
- **Recipient Type:** Complainant
- **Specific Users/Roles:** (leave empty)

**Advanced Settings:**
- **Priority:** 1 (highest priority)
- **Delay (minutes):** 0 (send immediately)
- **Send Only Once:** ✅ Yes
- **Conditions (optional):**
  ```json
  {
    "Source": "Email"
  }
  ```

**Save Rule**

Now whenever a complaint is created from email, this rule will automatically send the template!

---

## 📊 Testing Checklist

Use this checklist to verify everything works:

### Pre-Test Verification:
- [ ] Templates inserted in database (4 templates)
- [ ] Email configuration has template ID
- [ ] Auto-acknowledgement enabled in config
- [ ] Polling interval set to 2 minutes
- [ ] OAuth authorized (green badge)
- [ ] Backend and frontend running

### Test Execution:
- [ ] Send test email to configured address
- [ ] Wait 2 minutes or click "Poll Now"
- [ ] Check backend logs show "processed template"
- [ ] Receive auto-acknowledgement email
- [ ] Email contains actual ticket number (not {{TicketNumber}})
- [ ] Email contains actual ticket details
- [ ] HTML formatting looks correct

### Success Indicators:
- ✅ Auto-acknowledgement received within 2-3 minutes
- ✅ Ticket number is real (e.g., CMP-20251113-0042)
- ✅ All {{variables}} replaced with actual values
- ✅ HTML formatting displays correctly
- ✅ No errors in backend logs

---

## 🎯 Available Template Variables Reference

Quick reference of variables you can use in templates:

### Ticket Variables:
- `{{TicketNumber}}` - Unique ticket ID (CMP-20251113-0042)
- `{{Title}}` - Ticket title/subject
- `{{Description}}` - Full description
- `{{Status}}` - Current status (New, In Progress, Resolved)
- `{{Priority}}` - Priority level (Low, Medium, High, Critical)
- `{{SubmittedAt}}` - Submission date/time

### Customer Variables:
- `{{CustomerName}}` - Customer's name
- `{{CustomerEmail}}` - Customer's email

### Company Variables:
- `{{CompanyName}}` - Your company name
- `{{SupportEmail}}` - Support email address
- `{{FromEmail}}` - Configured from email
- `{{FromName}}` - Configured from name

### Date/Time Variables:
- `{{CurrentDate}}` - Today's date
- `{{CurrentTime}}` - Current time

For complete list, see: `TEMPLATE_VARIABLES_COMPLETE_GUIDE.md`

---

## 🚨 Troubleshooting

### Issue: No auto-acknowledgement received

**Check:**
1. Backend logs show "Sent auto-acknowledgement"?
2. Email configuration has `SendAutoAcknowledgement` = true?
3. Template ID is correct (valid GUID)?
4. SMTP settings configured correctly?
5. Check spam/junk folder?

**Solution:**
- Check backend terminal for errors
- Verify OAuth still authorized (green badge)
- Test SMTP by sending manual reply from UI

### Issue: {{TicketNumber}} not replaced

**Check:**
1. Backend logs show "Using template ... for auto-acknowledgement"?
2. Template exists and is active?
3. Spelling of variable is correct? (case-insensitive but must be {{TicketNumber}})

**Solution:**
- Check backend logs for template processing errors
- Verify template has {{TicketNumber}} in body
- Re-save email configuration

### Issue: HTML not formatting

**Check:**
1. Template has `HtmlBody` filled (not just `Body`)?
2. Email client supports HTML?

**Solution:**
- Ensure HTML body is populated in template
- Test with different email clients
- Check HTML syntax is valid

---

## 📖 Further Reading

- **Complete Template Guide:** `TEMPLATE_VARIABLES_COMPLETE_GUIDE.md`
- **Email Ticketing Explained:** `EMAIL_TICKETING_EXPLAINED.md`
- **Polling Interval Implementation:** `POLLING_INTERVAL_SECONDS_IMPLEMENTATION_COMPLETE.md`
- **Configuration Analysis:** `CONFIGURATION_CAPABILITIES_ANALYSIS.md`

---

## 🎉 Congratulations!

You now have a fully configured template system with:
✅ Auto-acknowledgement emails with ticket numbers
✅ Professional HTML templates
✅ Fast 2-minute email polling
✅ Multi-channel support (Email/SMS/WhatsApp)
✅ Event-driven notification rules

**Your customers will love it!** 🚀
