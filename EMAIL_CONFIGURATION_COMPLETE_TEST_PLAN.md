# 📧 EMAIL CONFIGURATION - COMPLETE TEST PLAN
## Comprehensive CRUD & Field Validation Testing

**Test Date:** November 17, 2025
**Scope:** All email configuration options, fields, and CRUD operations
**Modules:** Email Server Settings + Email Ticketing Configuration

---

## 🎯 USE CASE CLARIFICATION

### **Question:** How to configure email for sending notifications with OAuth, but NO inbound required?

### **Answer:** Use **Email Server Settings** (SMTP-only)

**Architecture:**
```
┌─────────────────────────────────────────────────────────────┐
│                  EMAIL SYSTEM ARCHITECTURE                   │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌───────────────────────┐    ┌─────────────────────────┐  │
│  │ Email Server Settings │    │ Email Ticketing Config  │  │
│  ├───────────────────────┤    ├─────────────────────────┤  │
│  │ Purpose:              │    │ Purpose:                │  │
│  │ • SMTP ONLY           │    │ • IMAP + SMTP           │  │
│  │ • Outbound emails     │    │ • Email-to-ticket       │  │
│  │ • Notifications       │    │ • Auto-create tickets   │  │
│  │                       │    │ • Support inbox         │  │
│  ├───────────────────────┤    ├─────────────────────────┤  │
│  │ Auth:                 │    │ Auth:                   │  │
│  │ ✅ OAuth 2.0          │    │ ✅ OAuth 2.0            │  │
│  │ ✅ Basic Auth         │    │ ✅ Basic Auth           │  │
│  │                       │    │ ✅ Separate SMTP        │  │
│  ├───────────────────────┤    ├─────────────────────────┤  │
│  │ Use When:             │    │ Use When:               │  │
│  │ • Send notifications  │    │ • Receive customer      │  │
│  │ • No inbound needed   │    │   emails                │  │
│  │ • Simple setup        │    │ • Auto-create tickets   │  │
│  │ ✅ YOUR USE CASE      │    │ • Full ticketing        │  │
│  └───────────────────────┘    └─────────────────────────┘  │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

**Recommendation:** Use **Email Server Settings** for notification-only scenarios.

---

## 📋 TEST PLAN OVERVIEW

### **Phase 1: Email Server Settings - Complete Testing**
- All CRUD operations
- All field validations
- OAuth 2.0 configuration
- Basic Auth configuration
- Special operations (Set Default, Test, Toggle)

### **Phase 2: Email Ticketing - Complete Testing**
- 6-step OAuth wizard
- All configuration options
- Separate SMTP account feature
- System settings panel

### **Phase 3: Integration Testing**
- Navigation between modules
- Data consistency
- Error handling

---

## 📝 PHASE 1: EMAIL SERVER SETTINGS

### **Test 1.1: CREATE - OAuth 2.0 (Office 365)**

**Fields to Test:**
- [ ] Name (required, max 200 chars)
- [ ] Provider Selection: Office 365
- [ ] Authentication Type: OAuth 2.0
- [ ] SMTP Host (auto-filled: smtp.office365.com)
- [ ] SMTP Port (auto-filled: 587)
- [ ] Use SSL (auto-checked)
- [ ] OAuth Client ID (required, GUID format)
- [ ] OAuth Client Secret (required)
- [ ] OAuth Tenant ID (required for Office 365)
- [ ] From Email (required, email format)
- [ ] From Name (required, max 100 chars)
- [ ] Reply-To Email (optional, email format)
- [ ] Timeout (10-300 seconds, default 30)
- [ ] Max Emails Per Hour (optional, >0)
- [ ] Is Active (checkbox, default true)
- [ ] Is Default (checkbox)

**Expected Behavior:**
- Auto-fill SMTP host and port based on provider
- OAuth fields appear when OAuth 2.0 selected
- Password fields hidden when OAuth selected
- Validation on save
- Success message on create
- Password/secret encrypted in database

**Test Data:**
```json
{
  "name": "Office 365 Notifications - OAuth",
  "provider": "Office365",
  "authenticationType": "OAuth2",
  "smtpHost": "smtp.office365.com",
  "smtpPort": 587,
  "useSsl": true,
  "oauthClientId": "12345678-1234-1234-1234-123456789012",
  "oauthClientSecret": "SecretValue123!",
  "oauthTenantId": "87654321-4321-4321-4321-210987654321",
  "fromEmail": "notifications@company.com",
  "fromName": "Complaint System Notifications",
  "replyToEmail": "support@company.com",
  "timeoutSeconds": 30,
  "maxEmailsPerHour": 100,
  "isActive": true,
  "isDefault": false
}
```

---

### **Test 1.2: CREATE - Basic Auth (Gmail)**

**Fields to Test:**
- [ ] Name
- [ ] Provider: Gmail
- [ ] Authentication Type: Basic Auth
- [ ] SMTP Host (auto: smtp.gmail.com)
- [ ] SMTP Port (auto: 587)
- [ ] Use SSL (auto-checked)
- [ ] Username (required, email format)
- [ ] Password (required, min 8 chars)
- [ ] From Email
- [ ] From Name
- [ ] All other fields same as OAuth

**Expected Behavior:**
- Password field appears for Basic Auth
- OAuth fields hidden
- Password encrypted on save
- Password field empty on edit (security)

**Test Data:**
```json
{
  "name": "Gmail SMTP - Basic Auth",
  "provider": "Gmail",
  "authenticationType": "BasicAuth",
  "smtpHost": "smtp.gmail.com",
  "smtpPort": 587,
  "useSsl": true,
  "username": "notifications@gmail.com",
  "password": "AppPassword123!",
  "fromEmail": "notifications@gmail.com",
  "fromName": "Support Team",
  "timeoutSeconds": 60,
  "isActive": true
}
```

---

### **Test 1.3: READ Operations**

**Test 1.3a: List All Servers**
- [ ] GET /api/email-settings
- [ ] Displays all active servers
- [ ] Shows default badge
- [ ] Shows active/inactive status
- [ ] Correct sorting (default first, then by name)

**Test 1.3b: Filter by Status**
- [ ] Click "All" - shows all servers
- [ ] Click "Active" - shows only active
- [ ] Click "Inactive" - shows only inactive
- [ ] Count updates correctly

**Test 1.3c: Search Functionality**
- [ ] Search by name
- [ ] Search by email
- [ ] Search by host
- [ ] Case-insensitive search
- [ ] Clear search resets results

**Test 1.3d: Get Single Server**
- [ ] GET /api/email-settings/{id}
- [ ] Returns complete details
- [ ] Password field empty (security)
- [ ] OAuth secret empty (security)

---

### **Test 1.4: UPDATE Operations**

**Test 1.4a: Edit Basic Settings**
- [ ] Update name
- [ ] Update from email
- [ ] Update from name
- [ ] Update timeout
- [ ] Save changes
- [ ] Verify persistence

**Test 1.4b: Change Authentication Type**
- [ ] Switch OAuth → Basic Auth
- [ ] Verify field changes
- [ ] Enter new password
- [ ] Save successfully

**Test 1.4c: Update OAuth Credentials**
- [ ] Change client ID
- [ ] Change client secret
- [ ] Change tenant ID
- [ ] Verify encryption on save

**Test 1.4d: Update Password (Basic Auth)**
- [ ] Password field empty on edit
- [ ] Enter new password
- [ ] Save and verify encrypted
- [ ] Leave empty = keep existing

**Expected Behavior:**
- Only modified fields updated
- Empty password = keep existing
- OAuth secrets re-encrypted
- Success message displayed
- List refreshes with changes

---

### **Test 1.5: DELETE Operations**

**Test 1.5a: Delete Non-Default Server**
- [ ] Click delete icon
- [ ] Confirmation dialog appears
- [ ] Confirm deletion
- [ ] Server immediately removed from list
- [ ] Count updates
- [ ] Success message shown

**Test 1.5b: Delete Default Server (Should Fail)**
- [ ] Try to delete default server
- [ ] Error message: "Cannot delete default server"
- [ ] Server remains in list
- [ ] No deletion occurs

**Test 1.5c: Delete Verification**
- [ ] Soft delete (IsDeleted = true)
- [ ] Not hard delete
- [ ] Refresh page - server still gone
- [ ] Database check - record exists with IsDeleted = true

---

### **Test 1.6: Special Operations**

**Test 1.6a: Set as Default**
- [ ] Click "Set as Default" on non-default server
- [ ] Success message appears
- [ ] Server shows "Default" badge
- [ ] Previous default loses badge
- [ ] Only ONE default exists
- [ ] No page reload required

**Test 1.6b: Toggle Active/Inactive**
- [ ] Click toggle switch
- [ ] Status changes immediately
- [ ] Filter counts update
- [ ] Toggle back works
- [ ] Active state persisted

**Test 1.6c: Test Email Connection**
- [ ] Click "Test" icon
- [ ] Enter recipient email
- [ ] Click "Send Test Email"
- [ ] Loading indicator shown
- [ ] Success/failure message displayed
- [ ] Last Tested timestamp updated
- [ ] Test notes saved

---

### **Test 1.7: Field Validation**

**Test 1.7a: Required Field Validation**
- [ ] Submit with empty Name → error
- [ ] Submit with empty Host → error
- [ ] Submit with empty Port → error
- [ ] Submit with empty Username → error
- [ ] Submit with empty Password (Basic) → error
- [ ] Submit with empty OAuth credentials → error
- [ ] Submit with empty From Email → error
- [ ] All error messages clear and helpful

**Test 1.7b: Format Validation**
- [ ] Invalid email format → error
- [ ] Invalid GUID format (OAuth) → error
- [ ] Port < 1 → error
- [ ] Port > 65535 → error
- [ ] Timeout < 10 → error
- [ ] Timeout > 300 → error
- [ ] Negative max emails → error

**Test 1.7c: Length Validation**
- [ ] Name > 200 chars → error
- [ ] From Name > 100 chars → error
- [ ] Password < 8 chars → error (if enforced)

---

## 📝 PHASE 2: EMAIL TICKETING CONFIGURATION

### **Test 2.1: CREATE - 6-Step OAuth Wizard (Office 365)**

**Step 1: Authentication Method**
- [ ] "OAuth 2.0 (Recommended)" option visible
- [ ] "Basic Authentication (Legacy)" option visible
- [ ] Security benefits listed
- [ ] Deprecation warnings shown
- [ ] Select OAuth 2.0
- [ ] Click Next → advances to Step 2

**Step 2: Provider Selection**
- [ ] "Office 365" option
- [ ] "Gmail" option
- [ ] "Outlook.com" option
- [ ] Provider descriptions clear
- [ ] Select Office 365
- [ ] IMAP host auto-fills: outlook.office365.com:993
- [ ] SMTP host auto-fills: smtp.office365.com:587
- [ ] Click Next → advances to Step 3

**Step 3: SMTP Account Selection (NEW FEATURE)**
- [ ] "Use Same Account" (Recommended) option
- [ ] "Use Separate Sending Account" (Advanced) option
- [ ] Educational content displayed
- [ ] Use cases explained
- [ ] Select "Use Same Account"
- [ ] Click Next → advances to Step 4

**Step 4: OAuth Credentials**
- [ ] Client ID field (with placeholder GUID)
- [ ] Client Secret field (with warning)
- [ ] Tenant ID field (for Office 365)
- [ ] Callback URL displayed with copy button
- [ ] Test copy button works
- [ ] Tabbed instructions (Office 365 / Gmail)
- [ ] 6-step Azure AD setup guide visible
- [ ] Enter test credentials
- [ ] Click Next → advances to Step 5

**Step 5: Additional Settings**
- [ ] From Email field
- [ ] From Name field
- [ ] Polling Interval dropdown (5 options)
- [ ] OAuth Token Refresh Interval dropdown
- [ ] IMAP Folder field (default: INBOX)
- [ ] "Enable Email Ticketing" checkbox (default: checked)
- [ ] "Send Auto-Acknowledgement" checkbox (default: checked)
- [ ] Smart defaults applied
- [ ] Click Next → advances to Step 6

**Step 6: Authorization**
- [ ] "Authorize Email Access" heading
- [ ] 4-step authorization process explained
- [ ] Security assurance badges
- [ ] "Your password never shared" message
- [ ] "Can revoke access" message
- [ ] "Save & Authorize Access" button
- [ ] Click Save
- [ ] Success/error message displayed

**Test Data for Complete Wizard:**
```json
{
  "authenticationType": "OAuth2",
  "provider": "Office365",
  "imapHost": "outlook.office365.com",
  "imapPort": 993,
  "imapUseSsl": true,
  "smtpHost": "smtp.office365.com",
  "smtpPort": 587,
  "smtpUseSsl": true,
  "useSeparateSmtpAccount": false,
  "oauthClientId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "oauthClientSecret": "OAuthSecretValue987!",
  "oauthTenantId": "ffffffff-eeee-dddd-cccc-bbbbbbbbbbbb",
  "fromEmail": "support@company.com",
  "fromName": "Support Team",
  "pollingIntervalMinutes": 2,
  "oauthTokenRefreshIntervalMinutes": 30,
  "imapFolder": "INBOX",
  "isEnabled": true,
  "sendAutoAcknowledgement": true
}
```

---

### **Test 2.2: CREATE - Basic Auth Configuration**

**Fields to Test:**
- [ ] Select "Basic Authentication" in Step 1
- [ ] No OAuth wizard (direct to form)
- [ ] IMAP Host
- [ ] IMAP Port
- [ ] IMAP Username
- [ ] IMAP Password
- [ ] SMTP Host
- [ ] SMTP Port
- [ ] SMTP Username
- [ ] SMTP Password
- [ ] From Email
- [ ] From Name
- [ ] Polling Interval
- [ ] Enable Email Ticketing
- [ ] Send Auto-Acknowledgement

**Expected Behavior:**
- Simpler form (no wizard)
- All fields visible at once
- Password encryption on save
- Save and verify creation

---

### **Test 2.3: CREATE - Separate SMTP Account**

**Configuration:**
- [ ] OAuth wizard Step 3: Select "Use Separate Sending Account"
- [ ] Additional SMTP fields appear
- [ ] Separate SMTP Host
- [ ] Separate SMTP Port
- [ ] Separate SMTP OAuth Client ID
- [ ] Separate SMTP OAuth Client Secret
- [ ] Separate SMTP OAuth Tenant ID
- [ ] Separate From Email
- [ ] Separate From Name
- [ ] Complete wizard
- [ ] Verify both accounts saved

**Use Case Example:**
```
IMAP (Receiving): support@company.com
SMTP (Sending):   noreply@company.com
```

---

### **Test 2.4: READ Operations**

**Test 2.4a: List Configurations**
- [ ] GET /api/email-configuration
- [ ] All configurations displayed
- [ ] From Email shown
- [ ] Auth Type badge (OAuth/Basic)
- [ ] Enabled/Disabled status
- [ ] Last Polled timestamp
- [ ] OAuth status indicator

**Test 2.4b: View Configuration Details**
- [ ] Click on configuration
- [ ] All details visible
- [ ] Passwords/secrets hidden
- [ ] OAuth status shown
- [ ] Polling statistics displayed

---

### **Test 2.5: UPDATE Operations**

**Test 2.5a: Edit Configuration**
- [ ] Click Edit
- [ ] Modify From Name
- [ ] Change polling interval
- [ ] Toggle auto-acknowledgement
- [ ] Save changes
- [ ] Verify persistence

**Test 2.5b: Toggle Enable/Disable**
- [ ] Click toggle
- [ ] Confirmation prompt (if polling active)
- [ ] Confirm
- [ ] Status changes
- [ ] Polling stops/starts
- [ ] Toggle back

---

### **Test 2.6: DELETE Operations**

- [ ] Click Delete
- [ ] Confirmation dialog
- [ ] Confirm deletion
- [ ] Configuration removed
- [ ] Success message
- [ ] Verify soft delete

---

### **Test 2.7: System Settings Panel**

**Test 2.7a: Open System Settings**
- [ ] Click "System Settings" button
- [ ] Panel/modal opens
- [ ] OAuth Token Refresh Interval field
- [ ] Default Email Polling Interval field
- [ ] Current values displayed

**Test 2.7b: Update System Settings**
- [ ] Change OAuth refresh: 30 → 45 minutes
- [ ] Change polling: 120 → 180 seconds
- [ ] Click Save
- [ ] Success message
- [ ] Reopen - verify changes persisted

**Test 2.7c: Reset to Defaults**
- [ ] Click "Reset to Defaults"
- [ ] Confirmation prompt
- [ ] Confirm
- [ ] Settings return to defaults
- [ ] Success message

---

## 🧪 PHASE 3: INTEGRATION TESTING

### **Test 3.1: Navigation Between Modules**
- [ ] Email Settings → Email Ticketing
- [ ] Email Ticketing → Email Settings
- [ ] No data loss during navigation
- [ ] Breadcrumbs work correctly

### **Test 3.2: Data Consistency**
- [ ] Create in Email Settings → verify in database
- [ ] Create in Email Ticketing → verify in database
- [ ] Update → changes persisted
- [ ] Delete → soft delete confirmed

### **Test 3.3: Error Handling**
- [ ] Network error simulation
- [ ] Invalid data submission
- [ ] Concurrent edit handling
- [ ] Session timeout handling

---

## 📊 TEST EXECUTION MATRIX

| Test Category | Total Tests | Expected Pass | Priority |
|---------------|-------------|---------------|----------|
| Email Settings CRUD | 20 | 20 | HIGH |
| Email Settings Validation | 15 | 15 | HIGH |
| Email Settings Special Ops | 6 | 6 | HIGH |
| Email Ticketing OAuth Wizard | 30 | 30 | HIGH |
| Email Ticketing CRUD | 12 | 12 | MEDIUM |
| Email Ticketing System Settings | 6 | 6 | MEDIUM |
| Integration Tests | 6 | 6 | MEDIUM |
| **TOTAL** | **95** | **95** | **100%** |

---

## 🎯 SUCCESS CRITERIA

### **All Tests Must:**
- ✅ Pass without errors
- ✅ Show appropriate validation messages
- ✅ Maintain data integrity
- ✅ Provide clear user feedback
- ✅ Handle edge cases gracefully
- ✅ Encrypt sensitive data
- ✅ Log security events

### **Performance Targets:**
- API response time < 200ms
- UI response immediate (<100ms)
- No console errors
- No memory leaks

---

## 📝 TEST EXECUTION NOTES

**Environment:**
- Frontend: http://localhost:4200
- Backend: http://localhost:5000
- Browser: Chrome (latest)
- Database: SQL Server (development)

**Test Data:**
- Use test OAuth credentials (non-production)
- Use test email addresses
- Clean up test data after execution

**Evidence:**
- Screenshots for each major test
- API response logs
- Database state verification
- Console error logs

---

**END OF TEST PLAN**

Ready for comprehensive execution!