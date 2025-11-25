# Quick Start: Email Notification Testing

## Current Status: ✅ 90% Complete!

Your notification system is **fully functional** and ready to send real emails. You just need to add your Gmail credentials.

---

## What's Already Working

✅ Notification system detects complaint creation
✅ SMTP configuration loaded from database
✅ System connects to Gmail SMTP server
✅ Email templates processed correctly
✅ 2 notifications created per complaint (complainant + manager)
✅ All errors logged to CommunicationLogs

**Only missing**: Real Gmail App Password

---

## Quick Test (5 Minutes)

### Step 1: Get Gmail App Password (2 minutes)

1. Open browser: https://myaccount.google.com/apppasswords
2. Sign in to your Gmail account
3. If not already enabled, enable 2-Factor Authentication first
4. Create an App Password:
   - App: Mail
   - Device: Windows Computer
5. Copy the 16-character password (e.g., `xxxx xxxx xxxx xxxx`)

### Step 2: Update SMTP Credentials (1 minute)

**Option A - Using the script** (Recommended):
```powershell
.\update-smtp-credentials.ps1
```
- Enter your Gmail address
- Paste your app password

**Option B - Manually**:
```sql
USE ComplaintManagementDB;

UPDATE EmailServerSettings
SET Username = 'your-email@gmail.com',
    Password = 'your-16-char-app-password',
    FromEmail = 'your-email@gmail.com'
WHERE IsActive = 1;
```

### Step 3: Test Email Sending (1 minute)

```powershell
.\test-notification-simple.ps1
```

This will:
1. Create a test complaint
2. Trigger notification dispatch
3. Send 2 emails
4. Show results in terminal

### Step 4: Verify Success (1 minute)

**Check 1: Your Email Inbox**
- Open your Gmail
- Look for 2 new emails with subject starting with "Complaint #CMP-2025-"

**Check 2: Database Logs**
```sql
SELECT TOP 2
    RecipientEmail,
    Subject,
    Status,
    ErrorMessage,
    CreatedAt
FROM CommunicationLogs
ORDER BY CreatedAt DESC;
```

Expected: `Status = 2` (Sent) with no ErrorMessage

---

## Troubleshooting

### "Username and Password not accepted"
- Make sure you're using an **App Password**, not your regular Gmail password
- Remove spaces from the app password
- Try creating a new app password

### "Must enable 2-Factor Authentication"
1. Go to https://myaccount.google.com/security
2. Enable 2-Step Verification
3. Wait 5 minutes
4. Try creating app password again

### "Less secure app access"
- Gmail no longer supports "less secure apps"
- **You MUST use an App Password** (not your regular password)

---

## Alternative: Use Free Test SMTP Service

If you don't want to use your personal Gmail:

### Option 1: Mailtrap (Free Forever Tier)
1. Sign up: https://mailtrap.io
2. Get your inbox credentials
3. Update database:
```sql
UPDATE EmailServerSettings
SET Host = 'sandbox.smtp.mailtrap.io',
    Port = 2525,
    UseSsl = 0,
    Username = 'your-mailtrap-username',
    Password = 'your-mailtrap-password',
    FromEmail = 'test@example.com'
WHERE IsActive = 1;
```

### Option 2: Ethereal Email (Instant Temp Account)
1. Visit: https://ethereal.email
2. Click "Create Ethereal Account"
3. Copy the credentials shown
4. Update EmailServerSettings with those credentials

**Note**: Ethereal emails don't actually deliver, but you can view them in the Ethereal web interface.

---

## Testing Other Notification Events

Once basic email sending works, test other events:

### Test COMPLAINT_ASSIGNED
```powershell
# After creating a complaint, assign it to a user via the frontend
# Or use API:
curl -X PUT "http://localhost:5058/api/complaints/{id}/assign" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"assignedToUserId\": \"some-user-id\"}"
```

### Test COMPLAINT_STATUS_CHANGED
```powershell
# Change complaint status via frontend or API:
curl -X PUT "http://localhost:5058/api/complaints/{id}/status" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"status\": 2, \"notes\": \"Moving to In Progress\"}"
```

### Test COMPLAINT_COMMENTED
```powershell
# Add a comment via frontend or API:
curl -X POST "http://localhost:5058/api/complaints/{id}/comments" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"commentText\": \"Test comment for notification\"}"
```

---

## Expected Email Content

When emails start working, you should receive:

**Subject**:
```
Complaint #CMP-2025-XXXX Created - [Complaint Title]
```

**Body** (based on template):
```
A new complaint has been submitted:

Complaint Number: CMP-2025-XXXX
Title: [Complaint Title]
Category: [Category Name]
Priority: [Priority Level]
Submitted By: [User Name]
Submitted On: [Date Time]

Description:
[Complaint Description]

Please log in to the complaint management system to review and take action.

Best regards,
Complaint Management System
```

---

## Success Criteria

✅ Email appears in Gmail inbox
✅ Subject line contains complaint number
✅ Body contains all complaint details
✅ CommunicationLogs shows `Status = 2` (Sent)
✅ No error message in database
✅ Both recipients receive email (complainant + manager)

---

## Next Features to Configure

Once email sending works:

### 1. SMS Notifications (Optional)
- Choose provider: Twilio, AWS SNS, etc.
- Implement ISmsService interface
- Update notification dispatcher

### 2. WhatsApp Notifications (Optional)
- Set up WhatsApp Business API
- Implement IWhatsAppService interface
- Configure WhatsApp templates

### 3. Email Templates Customization
- Go to: http://localhost:4200/admin/templates
- Customize email templates with your branding
- Add company logo
- Adjust formatting

### 4. Notification Rules
- Go to: http://localhost:4200/admin/notification-rules
- Enable/disable specific notifications
- Adjust recipient types
- Configure priority

---

## Support Files

- **SMTP_TEST_RESULTS.md** - Detailed test analysis
- **NOTIFICATION_SYSTEM_TEST_RESULTS.md** - Original test report
- **NOTIFICATION_SYSTEM_IMPLEMENTATION.md** - Full implementation guide
- **update-smtp-credentials.ps1** - Credential update script
- **test-notification-simple.ps1** - Simple test script

---

## Questions?

Check the comprehensive documentation:
- Architecture: COMPLAINT_MANAGEMENT_ARCHITECTURE.md
- API Endpoints: API_ENDPOINT_TEST_RESULTS.md
- Configuration: CONFIGURATION_MANAGEMENT_GUIDE.md

---

**Estimated Time to Complete**: 5-10 minutes
**Difficulty**: Easy
**Prerequisites**: Gmail account with 2FA enabled

Good luck! 🚀
