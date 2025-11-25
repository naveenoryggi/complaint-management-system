# Email Server Test Plan - Standard Email Accounts

## Test Environment Setup
- **Backend API**: http://localhost:5000
- **Frontend**: http://localhost:4200
- **Database**: SQL Server LocalDB

## Test Accounts Required

### 1. Gmail Account (App Password Required)
- **Email**: your-test-email@gmail.com
- **SMTP Server**: smtp.gmail.com
- **SMTP Port**: 587
- **IMAP Server**: imap.gmail.com
- **IMAP Port**: 993
- **Security**: TLS/SSL
- **Authentication**: App Password (not regular password)

### 2. Outlook/Office365 Account
- **Email**: your-test-email@outlook.com
- **SMTP Server**: smtp-mail.outlook.com
- **SMTP Port**: 587
- **IMAP Server**: outlook.office365.com
- **IMAP Port**: 993
- **Security**: STARTTLS
- **Authentication**: Basic Auth (App Password recommended)

### 3. Yahoo Mail Account
- **Email**: your-test-email@yahoo.com
- **SMTP Server**: smtp.mail.yahoo.com
- **SMTP Port**: 587
- **IMAP Server**: imap.mail.yahoo.com
- **IMAP Port**: 993
- **Security**: TLS/SSL
- **Authentication**: App Password required

---

## Test Cases

### TC-001: Email Server Configuration - Gmail
**Objective**: Verify Gmail SMTP/IMAP configuration works with app password

**Prerequisites**:
- Gmail account with "Less secure app access" OFF
- App Password generated from Google Account settings

**Test Steps**:
1. Navigate to Admin Panel → Email Ticketing Configuration
2. Click "Add Email Server"
3. Enter Gmail SMTP settings:
   - Email: your-email@gmail.com
   - Display Name: Test Gmail Account
   - SMTP Server: smtp.gmail.com
   - SMTP Port: 587
   - Username: your-email@gmail.com
   - Password: [16-character app password]
   - Enable TLS: Yes
4. Enter Gmail IMAP settings:
   - IMAP Server: imap.gmail.com
   - IMAP Port: 993
   - Enable SSL: Yes
5. Click "Test Connection"
6. Click "Save"

**Expected Result**:
- ✅ Connection test shows "SMTP Connected Successfully"
- ✅ Connection test shows "IMAP Connected Successfully"
- ✅ Configuration saved without errors
- ✅ Server status shows "Active"

**Pass/Fail**: _______

---

### TC-002: Email Server Configuration - Outlook
**Objective**: Verify Outlook/Office365 SMTP/IMAP configuration

**Test Steps**:
1. Navigate to Admin Panel → Email Ticketing Configuration
2. Click "Add Email Server"
3. Enter Outlook SMTP settings:
   - Email: your-email@outlook.com
   - Display Name: Test Outlook Account
   - SMTP Server: smtp-mail.outlook.com
   - SMTP Port: 587
   - Username: your-email@outlook.com
   - Password: [app password or account password]
   - Enable TLS: Yes
4. Enter Outlook IMAP settings:
   - IMAP Server: outlook.office365.com
   - IMAP Port: 993
   - Enable SSL: Yes
5. Click "Test Connection"
6. Click "Save"

**Expected Result**:
- ✅ Both SMTP and IMAP connections successful
- ✅ Configuration saved
- ✅ Server status "Active"

**Pass/Fail**: _______

---

### TC-003: Send Test Email - Gmail to External
**Objective**: Verify outbound email sending via Gmail SMTP

**Prerequisites**: TC-001 passed

**Test Steps**:
1. Create a new complaint (any method)
2. Open the complaint detail page
3. Click "Compose Email"
4. Fill in:
   - To: external-test-email@example.com
   - Subject: Test Email - Gmail SMTP
   - Body: This is a test email sent via Gmail SMTP
5. Click "Send"

**Expected Result**:
- ✅ Success message: "Email sent successfully"
- ✅ Email appears in "Email Thread" section as "Sent"
- ✅ Email received at recipient's inbox within 1-2 minutes
- ✅ Email headers show correct "From" address (Gmail account)

**Pass/Fail**: _______

---

### TC-004: Receive Inbound Email - Gmail IMAP
**Objective**: Verify inbound email polling and ticket creation via Gmail IMAP

**Prerequisites**: TC-001 passed

**Test Steps**:
1. From external email account, send email to configured Gmail address
   - Subject: Test Inbound Email
   - Body: Creating a complaint via email
2. Wait for polling interval (default 5 minutes) OR trigger manual poll
3. Check Dashboard for new complaint
4. Open the new complaint

**Expected Result**:
- ✅ New complaint created automatically
- ✅ Complaint number format: CMP-YYYYMMDD-XXXX
- ✅ Complainant name extracted from email sender
- ✅ Email content appears in complaint description
- ✅ Email appears in Email Thread section as "Received"
- ✅ Auto-response sent to sender (if configured)

**Pass/Fail**: _______

---

### TC-005: Email Reply - Gmail
**Objective**: Verify reply functionality maintains email thread

**Prerequisites**: TC-004 passed (complaint exists with inbound email)

**Test Steps**:
1. Open complaint created from inbound email
2. In Email Thread section, click "Reply" on the original email
3. Fill in reply:
   - Subject: Re: Test Inbound Email (auto-populated)
   - Body: Thank you for your email. We are processing your request.
4. Click "Send"

**Expected Result**:
- ✅ Reply sent successfully
- ✅ Reply appears in Email Thread as "Sent"
- ✅ Email received by original sender
- ✅ Email client shows as part of conversation thread
- ✅ "In-Reply-To" and "References" headers correctly set

**Pass/Fail**: _______

---

### TC-006: Email Forward - Gmail
**Objective**: Verify email forwarding includes complete thread

**Prerequisites**: TC-005 passed (complaint has email thread)

**Test Steps**:
1. Open complaint with existing email thread
2. Click "Forward" on any email in the thread
3. Fill in:
   - To: another-email@example.com
   - Add message: "FYI - forwarding for your review"
4. Click "Send"

**Expected Result**:
- ✅ Forwarded email sent successfully
- ✅ Forward appears in Email Thread as "Sent"
- ✅ Recipient receives email with:
   - Original message headers (From, Date, Subject)
   - Complete email thread (if forwarding reply)
   - Professional Outlook-style formatting
   - No rendering issues (shadows, table artifacts)

**Pass/Fail**: _______

---

### TC-007: Email Thread Continuity - Multi-Reply
**Objective**: Verify email threading works with multiple back-and-forth exchanges

**Test Steps**:
1. Send inbound email to system
2. Reply from system
3. Recipient replies to system's reply
4. System replies again
5. Check Email Thread section

**Expected Result**:
- ✅ All emails appear in chronological order
- ✅ Each email shows correct direction (Received/Sent)
- ✅ Thread headers maintained (In-Reply-To, References)
- ✅ Emails properly nested/grouped by conversation
- ✅ No duplicate emails

**Pass/Fail**: _______

---

### TC-008: HTML Email Formatting - Rich Content
**Objective**: Verify rich HTML emails render correctly

**Test Steps**:
1. Compose email using Quill editor
2. Apply various formatting:
   - Bold, Italic, Underline text
   - Headings (H1, H2)
   - Bullet/Numbered lists
   - Links
   - Different font sizes
   - Text colors
3. Send email
4. Check received email

**Expected Result**:
- ✅ All formatting preserved in sent email
- ✅ Recipient sees properly formatted email
- ✅ No broken HTML tags
- ✅ Clean professional appearance

**Pass/Fail**: _______

---

### TC-009: Email with Attachments - Send
**Objective**: Verify email attachments sending (if implemented)

**Test Steps**:
1. Compose email
2. Add attachment (PDF, image, document)
3. Send email

**Expected Result**:
- ✅ Email sent with attachment
- ✅ Attachment received by recipient
- ✅ Attachment size/type validated

**Pass/Fail**: _______

**Status**: ⚠️ Feature not yet implemented

---

### TC-010: Email Polling Interval Configuration
**Objective**: Verify polling interval can be configured

**Test Steps**:
1. Navigate to Email Server Configuration
2. Edit existing server
3. Change "Polling Interval" to 2 minutes
4. Save configuration
5. Send test inbound email
6. Measure time until complaint created

**Expected Result**:
- ✅ Polling interval updated successfully
- ✅ Emails polled at new interval (±30 seconds tolerance)
- ✅ System logs show correct polling schedule

**Pass/Fail**: _______

---

### TC-011: Multiple Email Servers - Priority
**Objective**: Verify system handles multiple configured email servers

**Test Steps**:
1. Configure Gmail account (Server A)
2. Configure Outlook account (Server B)
3. Set Server A as "Primary" or "Active"
4. Send outbound email
5. Check which server was used

**Expected Result**:
- ✅ Both servers configured successfully
- ✅ Outbound emails use active/primary server
- ✅ Inbound polling works for both servers
- ✅ Complaints created from either server

**Pass/Fail**: _______

---

### TC-012: Email Server Error Handling - Wrong Password
**Objective**: Verify graceful error handling for authentication failures

**Test Steps**:
1. Configure email server with incorrect password
2. Try to send email
3. Check error message

**Expected Result**:
- ✅ Clear error message: "Authentication failed - check username/password"
- ✅ No system crash
- ✅ Error logged in backend
- ✅ User can retry with correct credentials

**Pass/Fail**: _______

---

### TC-013: Email Server Error Handling - Wrong Server
**Objective**: Verify error handling for incorrect server settings

**Test Steps**:
1. Configure with wrong SMTP server (e.g., smtp.wrong.com)
2. Try to send email

**Expected Result**:
- ✅ Error message: "Cannot connect to SMTP server"
- ✅ Timeout within 30 seconds
- ✅ User can correct settings

**Pass/Fail**: _______

---

### TC-014: Email Character Encoding - Special Characters
**Objective**: Verify special characters and international text handled correctly

**Test Steps**:
1. Send email with:
   - Emojis: 😀 ✅ 📧
   - Special chars: & < > " '
   - Accented chars: àéîöü
   - Asian characters: 你好 こんにちは
2. Receive and display email

**Expected Result**:
- ✅ All characters displayed correctly
- ✅ No encoding errors (���)
- ✅ UTF-8 encoding used throughout

**Pass/Fail**: _______

---

### TC-015: Email Security - XSS Prevention
**Objective**: Verify malicious HTML/scripts are sanitized

**Test Steps**:
1. Send email with malicious content:
   ```html
   <script>alert('XSS')</script>
   <img src="x" onerror="alert('XSS')">
   ```
2. View email in Email Thread

**Expected Result**:
- ✅ Scripts removed/sanitized
- ✅ No JavaScript execution
- ✅ Safe HTML displayed
- ✅ Warning message if dangerous content detected

**Pass/Fail**: _______

---

### TC-016: Email Performance - Large Thread
**Objective**: Verify system handles long email threads efficiently

**Test Steps**:
1. Create complaint with 20+ emails in thread
2. Open complaint detail page
3. Load Email Thread section
4. Scroll through emails

**Expected Result**:
- ✅ Page loads within 3 seconds
- ✅ Emails displayed in correct order
- ✅ Smooth scrolling performance
- ✅ No browser lag/freeze

**Pass/Fail**: _______

---

### TC-017: Email Notification Rules Integration
**Objective**: Verify email system integrates with notification rules

**Test Steps**:
1. Configure notification rule: "Send email on complaint creation"
2. Create new complaint
3. Check Email Thread

**Expected Result**:
- ✅ Email sent automatically per rule
- ✅ Email appears in thread
- ✅ Correct template used
- ✅ Variables replaced correctly

**Pass/Fail**: _______

---

### TC-018: Email Search/Filter
**Objective**: Verify ability to search within email threads

**Test Steps**:
1. Open complaint with multiple emails
2. Use search/filter functionality (if available)
3. Search for specific text

**Expected Result**:
- ✅ Search highlights matching text
- ✅ Filters work correctly
- ✅ Can filter by Sent/Received

**Pass/Fail**: _______

**Status**: ⚠️ Feature may not be implemented

---

### TC-019: Email Timezone Handling
**Objective**: Verify email timestamps displayed in correct timezone

**Test Steps**:
1. Send email from different timezone
2. View email in Email Thread
3. Check timestamp

**Expected Result**:
- ✅ Timestamp shows in user's local timezone
- ✅ Tooltip shows original UTC time
- ✅ Consistent formatting

**Pass/Fail**: _______

---

### TC-020: Concurrent Email Operations
**Objective**: Verify system handles simultaneous email operations

**Test Steps**:
1. Have 3 users simultaneously:
   - User A: Send email
   - User B: Reply to email
   - User C: Forward email
2. Check all operations complete

**Expected Result**:
- ✅ All emails sent successfully
- ✅ No race conditions
- ✅ Thread integrity maintained
- ✅ No emails lost

**Pass/Fail**: _______

---

## Test Summary

| Test Case | Status | Notes |
|-----------|--------|-------|
| TC-001 | ⬜ | Gmail Configuration |
| TC-002 | ⬜ | Outlook Configuration |
| TC-003 | ⬜ | Send Email - Gmail |
| TC-004 | ⬜ | Receive Email - Gmail |
| TC-005 | ⬜ | Reply Functionality |
| TC-006 | ⬜ | Forward Functionality |
| TC-007 | ⬜ | Email Threading |
| TC-008 | ⬜ | HTML Formatting |
| TC-009 | ⬜ | Attachments |
| TC-010 | ⬜ | Polling Interval |
| TC-011 | ⬜ | Multiple Servers |
| TC-012 | ⬜ | Error - Wrong Password |
| TC-013 | ⬜ | Error - Wrong Server |
| TC-014 | ⬜ | Character Encoding |
| TC-015 | ⬜ | XSS Prevention |
| TC-016 | ⬜ | Performance |
| TC-017 | ⬜ | Notification Integration |
| TC-018 | ⬜ | Search/Filter |
| TC-019 | ⬜ | Timezone Handling |
| TC-020 | ⬜ | Concurrent Operations |

**Legend**: ⬜ Not Started | 🟡 In Progress | ✅ Passed | ❌ Failed

---

## Test Execution Notes

### Date: ___________
### Tester: ___________
### Environment: ___________

### Issues Found:
1.
2.
3.

### Recommendations:
1.
2.
3.
