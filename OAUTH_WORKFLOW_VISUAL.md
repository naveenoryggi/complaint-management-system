# OAuth 2.0 Email Ticketing - Visual Workflow

**Visual guide to understand the complete OAuth flow**

---

## 🎯 Current Status → Target Status

```
CURRENT STATE (Before Database Fix):
┌─────────────────────────────────────┐
│  Email Configuration Card           │
│  ┌───────────────────────────────┐  │
│  │ 📧 Oryggi Tech Support        │  │
│  │ marketing@oryggitech.com      │  │
│  │                               │  │
│  │ Badge: [Basic Auth] 🔵        │  │  ← WRONG! (AuthType=2)
│  │                               │  │
│  │ [Edit] [Poll Now] [Delete]    │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘

TARGET STATE (After Database Fix):
┌─────────────────────────────────────┐
│  Email Configuration Card           │
│  ┌───────────────────────────────┐  │
│  │ 📧 Oryggi Tech Support        │  │
│  │ marketing@oryggitech.com      │  │
│  │                               │  │
│  │ Badge: [OAuth 2.0-Expired] 🔴 │  │  ← CORRECT! (Token expired)
│  │                               │  │
│  │ [Authorize Now] [Poll Now]    │  │  ← New button appears!
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘
```

---

## 📊 OAuth Badge State Machine

```
┌─────────────────────────────────────────────────────────┐
│                    Badge States                         │
└─────────────────────────────────────────────────────────┘

1️⃣ NOT CONFIGURED (Gray, No Animation)
   ↓ User configures OAuth credentials

2️⃣ PENDING AUTHORIZATION (Orange, Pulsing ⚡)
   ↓ User clicks "Authorize Now"
   ↓ Signs in with Microsoft
   ↓ Accepts permissions

3️⃣ AUTHORIZED (Green, No Animation)
   ↓ After 60 days...

4️⃣ EXPIRED (Red, No Animation)
   ↓ User clicks "Refresh OAuth"
   ↓ Re-authenticates

   ↺ Back to AUTHORIZED

Alternative:
2️⃣ → Delete credentials → 1️⃣ NOT CONFIGURED
```

---

## 🔄 Complete OAuth Authorization Flow

```
USER BROWSER                 ANGULAR APP              .NET API              MICROSOFT
    │                            │                       │                      │
    │  1. Click "Authorize Now"  │                       │                      │
    ├────────────────────────────>                       │                      │
    │                            │                       │                      │
    │                            │  2. GET /api/oauth/authorize?configId=xxx   │
    │                            ├───────────────────────>                      │
    │                            │                       │                      │
    │                            │  3. Build OAuth URL   │                      │
    │                            │     with Client ID    │                      │
    │                            │     Tenant ID, Scopes │                      │
    │                            │                       │                      │
    │                            │  4. Return redirect   │                      │
    │                            │<───────────────────────┤                      │
    │                            │                       │                      │
    │  5. Redirect to Microsoft  │                       │                      │
    ├────────────────────────────┴───────────────────────┴──────────────────────>
    │                                                                            │
    │  6. Microsoft Login Page                                                  │
    │     Enter: marketing@oryggitech.com / password                            │
    ├────────────────────────────────────────────────────────────────────────────>
    │                                                                            │
    │  7. Consent Screen                                                        │
    │     "Complaint Management wants to:"                                      │
    │     ☑ Read your mail                                                      │
    │     ☑ Send mail as you                                                    │
    │     [Accept] [Cancel]                                                     │
    ├────────────────────────────────────────────────────────────────────────────>
    │                                                                            │
    │  8. Redirect with authorization code                                      │
    <────────────────────────────────────────────────────────────────────────────┤
    │  http://localhost:5000/api/oauth/callback?code=xxxxx                      │
    │                                                                            │
    │                            │                       │  9. Exchange code     │
    │                            │                       │     for access token  │
    │                            │                       ├──────────────────────>│
    │                            │                       │                       │
    │                            │                       │  10. Return tokens:   │
    │                            │                       │      - access_token   │
    │                            │                       │      - refresh_token  │
    │                            │                       │      - expires_in     │
    │                            │                       <──────────────────────┤
    │                            │                       │                       │
    │                            │  11. Store in DB:     │                       │
    │                            │      OAuthAccessToken │                       │
    │                            │      OAuthRefreshToken│                       │
    │                            │      OAuthTokenExpiresAt                      │
    │                            │                       │                       │
    │  12. Redirect to success page                      │                       │
    <────────────────────────────────────────────────────┤                       │
    │  http://localhost:4200/admin/communication/email-ticketing?success=true   │
    │                            │                       │                       │
    │  13. UI updates badge:     │                       │                       │
    │      "OAuth 2.0 - Authorized" (green)              │                       │
    │      Button: "Refresh OAuth"                       │                       │
```

---

## 🔐 Azure AD Configuration Checklist

```
AZURE AD PORTAL (portal.azure.com)
┌────────────────────────────────────────┐
│ Azure Active Directory                 │
│                                        │
│ ├─ App Registrations                  │
│    └─ New Registration                │
│       ├─ Name: Complaint Management   │
│       ├─ Account Type: Single Tenant  │
│       └─ Redirect URI:                │
│          http://localhost:5000/api/oauth/callback
│                                        │
│ ├─ Certificates & Secrets             │
│    └─ New Client Secret               │
│       ├─ Description: Email Access    │
│       ├─ Expires: 24 months           │
│       └─ Value: [COPY THIS!] 🔑       │
│                                        │
│ ├─ API Permissions                    │
│    ├─ Microsoft Graph                 │
│    │  ├─ Mail.Read            ✅      │
│    │  ├─ Mail.ReadWrite       ✅      │
│    │  ├─ IMAP.AccessAsUser.All ✅     │
│    │  ├─ SMTP.Send            ✅      │
│    │  ├─ User.Read            ✅      │
│    │  └─ offline_access       ✅      │
│    └─ Grant Admin Consent     ✅      │
│                                        │
│ ├─ Overview                           │
│    ├─ Application (client) ID: [COPY] │
│    └─ Directory (tenant) ID: [COPY]   │
└────────────────────────────────────────┘
```

---

## 🎨 UI Components Breakdown

```
EMAIL TICKETING CONFIGURATION PAGE
═══════════════════════════════════════════════

┌─ Header ──────────────────────────────────┐
│ 📧 Email Ticketing Configuration          │
│ [+ Add Email Configuration]               │
└───────────────────────────────────────────┘

┌─ Configuration Card ──────────────────────┐
│                                           │
│  📧 Oryggi Tech Support                   │
│  marketing@oryggitech.com                 │
│  ────────────────────────────────────     │
│  🔐 [OAuth 2.0 - Pending] ⚡ (pulsing)    │
│  📬 IMAP: outlook.office365.com:993       │
│  📤 SMTP: smtp.office365.com:587          │
│  🕐 Poll: Every 5 minutes                 │
│  📅 Last polled: 2 minutes ago            │
│  ⏰ OAuth expires: Nov 13, 2025           │
│  ────────────────────────────────────     │
│  [⚡ Authorize Now] [🔄 Poll Now]          │
│  [✏️ Edit] [🗑️ Delete]                     │
│                                           │
└───────────────────────────────────────────┘

Badge Color Guide:
🟢 Green   = OAuth 2.0 - Authorized (token valid)
🟠 Orange  = OAuth 2.0 - Pending (needs authorization) ⚡ PULSING
🔴 Red     = OAuth 2.0 - Expired (token expired)
⚪ Gray    = OAuth 2.0 - Not Configured
🔵 Blue    = Basic Auth (username/password)
```

---

## 🔧 Backend Services Running

```
.NET BACKGROUND SERVICES (Always Running)
══════════════════════════════════════════

┌─ Email Polling Service ────────────────┐
│ ⏰ Runs every: 5 minutes               │
│ 📧 Checks: marketing@oryggitech.com    │
│ 🔄 Action: Fetch new emails via IMAP  │
│ 📝 Creates: Complaints from emails     │
│ 📊 Status: Running                     │
└────────────────────────────────────────┘

┌─ OAuth Token Refresh Service ─────────┐
│ ⏰ Runs every: 60 minutes              │
│ 🔍 Checks: Tokens expiring in 7 days  │
│ 🔄 Action: Refresh access token       │
│ 🔐 Uses: Refresh token from DB        │
│ 📊 Status: Running                     │
└────────────────────────────────────────┘

┌─ Auto-Escalation Service ─────────────┐
│ ⏰ Runs every: 30 seconds              │
│ 🔍 Checks: Overdue complaints         │
│ 📊 Status: Running                     │
└────────────────────────────────────────┘

┌─ Oryggi Sync Service ─────────────────┐
│ ⏰ Runs: Based on schedule             │
│ 🔄 Syncs: User data from Oryggi       │
│ 📊 Status: Running                     │
└────────────────────────────────────────┘
```

---

## 📧 Email-to-Complaint Creation Flow

```
INCOMING EMAIL                 POLLING SERVICE           COMPLAINT CREATION
     │                              │                           │
     │  1. Email arrives at         │                           │
     │     marketing@oryggitech.com │                           │
     │                              │                           │
     │  2. Wait for polling         │                           │
     │     (max 5 minutes)          │                           │
     │                              │                           │
     │                              │  3. Connect to IMAP       │
     │                              │     using OAuth token     │
     │                              │                           │
     │                              │  4. Fetch unread emails   │
     │                              │                           │
     │                              │  5. Parse email:          │
     │                              │     - From: sender email  │
     │                              │     - Subject: title      │
     │                              │     - Body: description   │
     │                              │     - Attachments         │
     │                              │                           │
     │                              ├───────────────────────────>
     │                              │  6. Create complaint:     │
     │                              │     - Title: email subject│
     │                              │     - Description: body   │
     │                              │     - Complainant: sender │
     │                              │     - Status: New         │
     │                              │     - Priority: Normal    │
     │                              │                           │
     │                              │  7. Store email message:  │
     │                              │     - Direction: Inbound  │
     │                              │     - Link to complaint   │
     │                              │     - Store attachments   │
     │                              │                           │
     │                              │  8. Mark email as read    │
     │                              │                           │
     │                              │  9. Send auto-acknowledgement
     │                              │     (if enabled)          │
     │                              │                           │
     │  ✅ Complaint created!       │                           │
     │  User can see it in          │                           │
     │  Complaint List              │                           │
```

---

## 🎯 Testing Checklist

```
PRE-AUTHORIZATION TESTING:
☐ Database fix applied (AuthenticationType = 1)
☐ UI shows "OAuth 2.0 - Pending" badge (orange, pulsing)
☐ "Authorize Now" button visible and enabled
☐ Badge NOT showing "Basic Auth"

AZURE AD CONFIGURATION:
☐ App registration created
☐ Client ID copied
☐ Tenant ID copied
☐ Client secret created and saved
☐ 6 API permissions added and granted
☐ Redirect URI configured correctly

APPLICATION CONFIGURATION:
☐ Logged in as admin
☐ Navigated to Email Ticketing Configuration
☐ Clicked Edit on "Oryggi Tech Support"
☐ Entered Client ID, Tenant ID, Secret in wizard
☐ Saved configuration successfully

AUTHORIZATION FLOW:
☐ Clicked "Authorize Now" button
☐ Redirected to Microsoft login
☐ Signed in with marketing@oryggitech.com
☐ Accepted permissions on consent screen
☐ Redirected back to application
☐ Badge changed to "OAuth 2.0 - Authorized" (green)
☐ Button changed to "Refresh OAuth"

EMAIL POLLING:
☐ Clicked "Poll Now" button
☐ Backend logs show successful IMAP connection
☐ No "No cached account found" errors
☐ Sent test email to marketing@oryggitech.com
☐ Waited 5 minutes (or clicked "Poll Now")
☐ New complaint appeared in Complaint List
☐ Complaint details match email content

SUCCESS CRITERIA: All ☐ become ✅
```

---

## 🚨 Common Error Messages & Solutions

```
ERROR: "No cached account found for marketing@oryggitech.com"
├─ CAUSE: Authorization not completed yet
├─ WHEN: Before clicking "Authorize Now"
└─ SOLUTION: Click "Authorize Now" and complete Microsoft login

ERROR: "redirect_uri_mismatch"
├─ CAUSE: Redirect URI doesn't match Azure AD
├─ WHEN: During authorization redirect
└─ SOLUTION: Verify Azure AD has http://localhost:5000/api/oauth/callback

ERROR: "AADSTS50011: The reply URL does not match"
├─ CAUSE: Redirect URI mismatch in appsettings.json
├─ WHEN: During authorization callback
└─ SOLUTION: Check backend appsettings.json OAuth.RedirectUri

ERROR: "Failed to refresh access token"
├─ CAUSE: Refresh token expired (after 60 days)
├─ WHEN: Automatic token refresh attempts
└─ SOLUTION: Click "Refresh OAuth" to re-authorize

ERROR: "Insufficient privileges to complete the operation"
├─ CAUSE: Missing API permissions or admin consent
├─ WHEN: During IMAP/SMTP operations
└─ SOLUTION: Grant admin consent for all 6 permissions in Azure AD

ERROR: Badge still shows "Basic Auth"
├─ CAUSE: Database AuthenticationType not updated
├─ WHEN: After loading Email Ticketing page
└─ SOLUTION: Run fix-db-auth-type.sql and refresh browser (Ctrl+F5)
```

---

**Ready to proceed?** Start with the database fix in `OAUTH_QUICK_START.md`! 🚀
