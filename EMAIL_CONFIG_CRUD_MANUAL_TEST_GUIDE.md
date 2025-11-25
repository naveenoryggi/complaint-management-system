# Email Ticketing Configuration - CRUD Operations & Features Manual Test Guide

**Version:** 1.0
**Date:** November 13, 2025
**Purpose:** Comprehensive manual testing guide for all email configuration CRUD operations and features

---

## Table of Contents

1. [Pre-requisites](#pre-requisites)
2. [CREATE Operations](#create-operations)
3. [READ Operations](#read-operations)
4. [UPDATE Operations](#update-operations)
5. [DELETE Operations](#delete-operations)
6. [Feature Testing](#feature-testing)
7. [Validation Testing](#validation-testing)
8. [Email Server Configurations](#email-server-configurations)
9. [Test Checklist](#test-checklist)

---

## Pre-requisites

### Required Access:
- ✅ Admin user account
- ✅ Backend API running (http://localhost:5000)
- ✅ Frontend Angular app running (http://localhost:4200)
- ✅ Valid authentication token

### Test Data Preparation:
- Office 365 OAuth credentials (Client ID, Tenant ID, Client Secret)
- Gmail OAuth credentials (Client ID, Client Secret)
- Custom IMAP/SMTP server details (if testing custom servers)

---

## CREATE Operations

### Test 1.1: Create Office 365 OAuth Configuration

**Steps:**
1. Navigate to Email Ticketing Configuration page
2. Click "Add Email Configuration" button
3. **Verify:** Form opens with OAuth wizard

**OAuth Information Banner:**
4. **Verify:** Green banner explaining OAuth benefits
5. **Verify:** Lists: "No passwords stored", "Automatic token refresh", "Required for Office 365 & Gmail"

**Authentication Method:**
6. **Verify:** OAuth 2.0 is pre-selected with green "Secure" badge
7. **Verify:** Basic Authentication option shows yellow "Legacy" badge

**Step 1: Provider Selection**
8. Click "Office 365" provider card
9. **Verify:** Card highlights with blue border
10. **Verify:** Server settings auto-populate:
    - IMAP Host: `outlook.office365.com`
    - IMAP Port: `993`
    - SMTP Host: `smtp.office365.com`
    - SMTP Port: `587`

**Step 2: Email Configuration**
11. Click "Next" button
12. Enter email: `support@company.com`
13. Enter display name: `Company Support Team`
14. **Verify:** Required field indicators (asterisk)

**Step 3: Setup Instructions**
15. Click "Next" button
16. **Verify:** Azure AD configuration steps displayed
17. **Verify:** Numbered list with 8 steps
18. **Verify:** Callback URL is copyable: `http://localhost:5000/api/oauth/callback`
19. Click copy icon on callback URL
20. **Verify:** "Copied to clipboard" message appears

**Step 4: OAuth Credentials**
21. Click "Next" button
22. Enter Client ID: `12345678-abcd-1234-abcd-123456789abc`
23. Enter Tenant ID: `87654321-dcba-4321-dcba-cba987654321`
24. Enter Client Secret: `test-secret-value`
25. **Verify:** Password field masks the secret
26. **Verify:** Help tooltips appear next to field labels

**Step 5: Authorization**
27. Click "Next" button
28. **Verify:** Authorization panel displayed
29. **Verify:** Security reassurance message shown
30. Click "Save & Authorize" button
31. **Expected:** Redirect to Microsoft OAuth login page (if backend OAuth is configured)
32. **Fallback:** If not configured, validation error should appear

**Expected Result:**
- ✅ Configuration saved to database
- ✅ OAuth redirect initiated (if backend ready)
- ✅ All form data validated
- ✅ User receives feedback (success/error message)

---

### Test 1.2: Create Gmail OAuth Configuration

**Steps:**
1. Click "Add Email Configuration"
2. Select "OAuth 2.0" authentication type
3. Click "Gmail" provider card
4. **Verify:** Server settings auto-populate:
   - IMAP Host: `imap.gmail.com`
   - IMAP Port: `993`
   - SMTP Host: `smtp.gmail.com`
   - SMTP Port: `587`
5. Enter email: `support@companymail.com`
6. Enter display name: `Gmail Support`
7. Navigate to Step 3
8. **Verify:** Gmail tab available in instructions
9. Click "Gmail" tab
10. **Verify:** Google Cloud Console instructions shown
11. Navigate to Step 4
12. Enter Client ID: `gmail-client-id-test-12345`
13. **Note:** Gmail doesn't require Tenant ID (field should be hidden or optional)
14. Enter Client Secret: `gmail-secret-test-67890`
15. Click "Save & Authorize"

**Expected Result:**
- ✅ Gmail configuration saved
- ✅ Different instruction set for Gmail shown
- ✅ Tenant ID not required

---

### Test 1.3: Create Outlook.com OAuth Configuration

**Steps:**
1. Click "Add Email Configuration"
2. Select "OAuth 2.0"
3. Click "Outlook.com" provider card
4. **Verify:** Settings populate:
   - IMAP Host: `outlook.office365.com`
   - IMAP Port: `993`
   - SMTP Host: `smtp-mail.outlook.com`
   - SMTP Port: `587`
5. Enter email: `personaluser@outlook.com`
6. Enter display name: `Personal Outlook`
7. Complete OAuth credentials
8. Save configuration

**Expected Result:**
- ✅ Outlook.com configuration saved
- ✅ Different from Office 365 business

---

### Test 1.4: Create Custom IMAP/SMTP with Basic Auth

**Steps:**
1. Click "Add Email Configuration"
2. Click "Basic Authentication" option
3. **Verify:** Yellow "Legacy" badge shown
4. **Verify:** Warning message: "May not work for Gmail/Office 365"
5. Click "Custom IMAP/SMTP" provider
6. Enter email: `support@customserver.com`
7. Enter display name: `Custom Server Support`
8. Enter IMAP details:
   - Host: `mail.customserver.com`
   - Port: `993`
   - SSL: Enabled
   - Username: `support@customserver.com`
   - Password: `CustomPassword123!`
9. Enter SMTP details:
   - Host: `smtp.customserver.com`
   - Port: `587`
   - SSL: Enabled
   - Username: `support@customserver.com`
   - Password: `CustomPassword123!`
10. Configure additional settings:
    - Polling Interval: `5` minutes
    - IMAP Folder: `INBOX`
    - Enable Threading: ✓
    - Thread Timeout: `7` days
    - Send Auto-Acknowledgement: ✓
11. Click "Save"

**Expected Result:**
- ✅ Basic auth configuration saved
- ✅ No OAuth redirect
- ✅ Credentials stored encrypted

---

## READ Operations

### Test 2.1: List All Configurations

**Steps:**
1. Navigate to Email Ticketing Configuration page
2. **Verify:** All saved configurations displayed in cards/list

**For Each Configuration Card, Verify:**
- ✅ Email address displayed
- ✅ Display name shown
- ✅ Provider icon/name visible
- ✅ Status badge (Enabled/Disabled)
- ✅ Authentication type (OAuth/Basic) indicated
- ✅ Last polled time displayed (if available)
- ✅ Action buttons visible (Edit, Delete, Toggle, Test, Poll)

**Expected Result:**
- All configurations visible
- Clear visual hierarchy
- Status at a glance

---

### Test 2.2: Empty State

**Steps:**
1. If no configurations exist, or delete all configurations
2. **Verify:** Empty state message displayed
3. **Verify:** Message suggests creating first configuration
4. **Verify:** Icon or illustration shown
5. **Verify:** "Add Email Configuration" button prominent

**Expected Result:**
- ✅ Helpful empty state
- ✅ Clear call-to-action

---

### Test 2.3: View Configuration Details

**Steps:**
1. Locate a configuration card
2. **Verify:** Following information visible:
   - From Email
   - From Name
   - IMAP Host and Port
   - SMTP Host and Port
   - Polling Interval
   - Last Polled timestamp
   - OAuth Token Expiry (if OAuth)
   - Created/Updated dates

**Expected Result:**
- ✅ All key information visible without opening edit form
- ✅ OAuth token expiry warning if expires soon (< 7 days)

---

## UPDATE Operations

### Test 3.1: Edit Display Name

**Steps:**
1. Click "Edit" button on a configuration
2. Edit form opens with current values pre-filled
3. **Verify:** All fields show existing values
4. Modify display name: Append "(Updated)"
5. **Verify:** Other fields remain unchanged
6. Click "Save" button
7. **Verify:** Success message appears
8. **Verify:** Configuration card shows updated name

**Expected Result:**
- ✅ Only changed field updated
- ✅ Immediate visual feedback

---

### Test 3.2: Update Polling Interval

**Steps:**
1. Edit a configuration
2. Change polling interval from `5` to `10` minutes
3. Save configuration
4. **Verify:** New interval reflected in card

**Expected Result:**
- ✅ Polling interval updated
- ✅ Next poll scheduled based on new interval

---

### Test 3.3: Update Server Settings

**Steps:**
1. Edit a custom IMAP configuration
2. Change IMAP port from `993` to `143` (non-SSL)
3. Disable SSL for IMAP
4. Save configuration
5. **Verify:** Settings updated

**Expected Result:**
- ✅ Server settings updatable
- ✅ SSL toggle works

---

### Test 3.4: Update OAuth Credentials

**Steps:**
1. Edit an OAuth configuration
2. Update Client Secret to new value
3. Save configuration
4. **Expected:** May trigger re-authorization flow

**Expected Result:**
- ✅ Credentials can be updated
- ✅ Re-authorization handled properly

---

## DELETE Operations

### Test 4.1: Delete Configuration with Confirmation

**Steps:**
1. Click "Delete" button on a configuration
2. **Verify:** Confirmation dialog appears
3. **Verify:** Dialog shows:
   - Configuration email address
   - Warning message
   - "Cancel" and "Confirm" buttons
4. Click "Cancel"
5. **Verify:** Configuration NOT deleted
6. Click "Delete" again
7. Click "Confirm" in dialog
8. **Verify:** Configuration removed from list
9. **Verify:** Success message: "Configuration deleted successfully"

**Expected Result:**
- ✅ Confirmation required
- ✅ Safe deletion process
- ✅ Immediate UI update

---

### Test 4.2: Delete Last Configuration

**Steps:**
1. If only one configuration remains, delete it
2. **Verify:** Empty state appears after deletion

**Expected Result:**
- ✅ Graceful handling of zero configurations

---

## Feature Testing

### Test 5.1: Toggle Enable/Disable

**Steps:**
1. Locate an enabled configuration (green "Enabled" badge)
2. Click "Disable" button
3. **Verify:** Confirmation dialog appears
4. Confirm action
5. **Verify:** Badge changes to gray "Disabled"
6. **Verify:** Configuration card visually dimmed/grayed out
7. Click "Enable" button
8. Confirm action
9. **Verify:** Badge returns to green "Enabled"
10. **Verify:** Configuration card returns to normal appearance

**Expected Result:**
- ✅ Toggle works bidirectionally
- ✅ Visual feedback immediate
- ✅ Disabled configs don't poll emails

---

### Test 5.2: Test IMAP Connection

**Steps:**
1. Click "Test IMAP" button on a configuration
2. **Verify:** Loading indicator appears
3. Wait for response
4. **If successful:**
   - ✅ Green success message: "IMAP connection successful"
   - ✅ Details shown: "Connected to server, authenticated"
5. **If failed:**
   - ❌ Red error message with details
   - ❌ Helpful error explanation

**Expected Result:**
- ✅ Connection test provides feedback
- ✅ Errors are descriptive

---

### Test 5.3: Test SMTP Connection

**Steps:**
1. Click "Test SMTP" button
2. **Verify:** Loading indicator
3. Wait for response
4. **If successful:**
   - ✅ Green success message
   - ✅ "SMTP connection successful"
5. **If failed:**
   - ❌ Error message with details

**Expected Result:**
- ✅ SMTP test independent of IMAP
- ✅ Clear feedback provided

---

### Test 5.4: Poll Emails Now

**Steps:**
1. Click "Poll Now" button on an enabled configuration
2. **Verify:** Loading indicator appears
3. Wait for polling to complete
4. **Verify:** Success message shows:
   - Number of emails fetched
   - Number of complaints created
   - "Last polled" timestamp updates
5. Navigate to Complaints list
6. **Verify:** New complaints created from emails

**Expected Result:**
- ✅ Manual polling works
- ✅ Emails converted to complaints
- ✅ Immediate feedback

---

### Test 5.5: Refresh OAuth Token

**Steps:**
1. Locate an OAuth configuration with expiring token (< 7 days)
2. **Verify:** Warning badge/message shown
3. Click "Refresh OAuth" button
4. **Expected:** Redirect to OAuth provider
5. Grant permissions again
6. **Expected:** Redirect back to application
7. **Verify:** Token expiry updated to 90 days in future
8. **Verify:** Warning removed

**Expected Result:**
- ✅ Token refresh process works
- ✅ User can re-authorize easily

---

## Validation Testing

### Test 6.1: Required Fields Validation

**Steps:**
1. Click "Add Email Configuration"
2. Click "Save" without filling any fields
3. **Verify:** Validation errors displayed:
   - "Email address is required"
   - "Display name is required"
   - "Client ID is required" (if OAuth)
   - "Client Secret is required" (if OAuth)
4. **Verify:** Form does not submit
5. **Verify:** Focus moves to first error field

**Expected Result:**
- ✅ All required fields validated
- ✅ Clear error messages

---

### Test 6.2: Email Format Validation

**Steps:**
1. Open create form
2. Enter invalid email: `not-an-email`
3. Tab out of field
4. **Verify:** Error message: "Invalid email format"
5. Enter valid email: `support@company.com`
6. **Verify:** Error clears

**Expected Result:**
- ✅ Real-time email validation
- ✅ Helpful error messages

---

### Test 6.3: Port Number Validation

**Steps:**
1. Open create form with custom server
2. Enter IMAP port: `99999` (out of range)
3. **Verify:** Error: "Port must be between 1 and 65535"
4. Enter port: `-1`
5. **Verify:** Error: "Port must be positive"
6. Enter port: `abc`
7. **Verify:** Error: "Port must be a number"

**Expected Result:**
- ✅ Port validation works
- ✅ Only valid ports accepted

---

### Test 6.4: Polling Interval Validation

**Steps:**
1. Set polling interval to `0`
2. **Verify:** Error: "Polling interval must be at least 1 minute"
3. Set to `1441` (> 24 hours)
4. **Verify:** Warning: "Long intervals may delay email processing"

**Expected Result:**
- ✅ Reasonable intervals enforced
- ✅ Warnings for extreme values

---

## Email Server Configurations

### Supported Email Servers

#### 1. Office 365 (OAuth)
```
Authentication: OAuth 2.0
IMAP Host: outlook.office365.com
IMAP Port: 993 (SSL)
SMTP Host: smtp.office365.com
SMTP Port: 587 (TLS)
Required: Client ID, Tenant ID, Client Secret
```

#### 2. Gmail (OAuth)
```
Authentication: OAuth 2.0
IMAP Host: imap.gmail.com
IMAP Port: 993 (SSL)
SMTP Host: smtp.gmail.com
SMTP Port: 587 (TLS)
Required: Client ID, Client Secret
Note: Enable "Less secure app access" or use App Password for Basic Auth
```

#### 3. Outlook.com (OAuth)
```
Authentication: OAuth 2.0
IMAP Host: outlook.office365.com
IMAP Port: 993 (SSL)
SMTP Host: smtp-mail.outlook.com
SMTP Port: 587 (TLS)
Required: Client ID, Tenant ID, Client Secret
```

#### 4. Yahoo Mail
```
Authentication: Basic or App Password
IMAP Host: imap.mail.yahoo.com
IMAP Port: 993 (SSL)
SMTP Host: smtp.mail.yahoo.com
SMTP Port: 587 (TLS)
Note: Generate App Password from Yahoo Account Security
```

#### 5. GoDaddy Workspace Email
```
Authentication: Basic
IMAP Host: imap.secureserver.net
IMAP Port: 993 (SSL)
SMTP Host: smtpout.secureserver.net
SMTP Port: 465 (SSL) or 587 (TLS)
Username: Full email address
```

#### 6. Custom IMAP/SMTP Server
```
Authentication: Basic
IMAP Host: [Custom]
IMAP Port: 143 (non-SSL) or 993 (SSL)
SMTP Host: [Custom]
SMTP Port: 25, 465 (SSL), or 587 (TLS)
Username: Usually email address
```

---

## Test Checklist

### CRUD Operations
- [ ] **CREATE**: Office 365 OAuth configuration
- [ ] **CREATE**: Gmail OAuth configuration
- [ ] **CREATE**: Outlook.com OAuth configuration
- [ ] **CREATE**: Custom IMAP Basic Auth configuration
- [ ] **READ**: List all configurations
- [ ] **READ**: View configuration details
- [ ] **READ**: Empty state display
- [ ] **UPDATE**: Modify display name
- [ ] **UPDATE**: Change polling interval
- [ ] **UPDATE**: Update server settings
- [ ] **UPDATE**: Update OAuth credentials
- [ ] **DELETE**: Delete with confirmation
- [ ] **DELETE**: Delete last configuration

### Features
- [ ] Toggle Enable/Disable
- [ ] Test IMAP connection
- [ ] Test SMTP connection
- [ ] Poll emails manually
- [ ] Refresh OAuth token
- [ ] View token expiry warning
- [ ] Copy callback URL to clipboard

### Validation
- [ ] Required fields validation
- [ ] Email format validation
- [ ] Port number validation
- [ ] Polling interval validation
- [ ] GUID format validation (Client ID, Tenant ID)

### UI/UX
- [ ] OAuth information banner displays
- [ ] Authentication type selector works
- [ ] Provider cards highlight on selection
- [ ] Wizard step indicators show progress
- [ ] Completed steps show checkmarks
- [ ] Next/Back navigation works
- [ ] Form cancellation works
- [ ] Loading indicators appear during operations
- [ ] Success messages display
- [ ] Error messages are helpful

### Email Servers
- [ ] Office 365 server settings auto-populate
- [ ] Gmail server settings auto-populate
- [ ] Yahoo server settings available
- [ ] GoDaddy server settings available
- [ ] Custom server manual entry works

### Security
- [ ] Passwords are masked in forms
- [ ] Client Secrets are masked
- [ ] OAuth tokens not displayed in UI
- [ ] Credentials encrypted in database
- [ ] OAuth redirect uses HTTPS (production)

---

## Test Execution Log Template

```
Test Date: __________
Tester Name: __________
Environment: __________

Test 1.1 - Create Office 365 OAuth:
  Result: [ ] PASS [ ] FAIL [ ] BLOCKED
  Notes: _________________________________

Test 1.2 - Create Gmail OAuth:
  Result: [ ] PASS [ ] FAIL [ ] BLOCKED
  Notes: _________________________________

[... continue for all tests ...]

Summary:
  Total Tests: ___
  Passed: ___
  Failed: ___
  Blocked: ___
  Pass Rate: ___%

Issues Found:
1. _________________________________
2. _________________________________

Recommendations:
1. _________________________________
2. _________________________________
```

---

## Automated Test Script

An automated Playwright test script is available:
- **File:** `test-email-config-crud-comprehensive.js`
- **Usage:** `node test-email-config-crud-comprehensive.js`
- **Output:** Screenshots + JSON report + Markdown report

---

## Appendix: Common Issues & Troubleshooting

### Issue 1: OAuth Redirect Fails
**Symptom:** After clicking "Save & Authorize", redirect doesn't happen
**Cause:** Backend OAuth endpoints not implemented
**Solution:** Implement `/api/oauth/authorize/{id}` endpoint

### Issue 2: Test Connection Times Out
**Symptom:** IMAP/SMTP test hangs or times out
**Cause:** Incorrect server settings or firewall
**Solution:** Verify host/port, check firewall rules

### Issue 3: Emails Not Polling
**Symptom:** Manual poll returns 0 emails
**Cause:** OAuth token expired, wrong folder, or no new emails
**Solution:** Refresh OAuth token, verify IMAP folder name

### Issue 4: Validation Errors Not Clearing
**Symptom:** Error messages persist after fixing fields
**Cause:** Form validation not re-triggering
**Solution:** Tab out of field or click Save again

---

**End of Manual Test Guide**

**Document Version:** 1.0
**Last Updated:** November 13, 2025
**Maintained By:** Development Team
