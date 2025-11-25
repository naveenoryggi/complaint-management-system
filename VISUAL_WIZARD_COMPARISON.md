# Email Ticketing Wizard - Visual Comparison

## EXPECTED WIZARD (6 Steps) vs ACTUAL WIZARD (5 Steps)

---

## EXPECTED WIZARD FLOW (What Should Exist)

```
┌─────────────────────────────────────────────────────────────┐
│                    STEP 1 OF 6                              │
│                                                             │
│        📧 Select Your Email Provider                        │
│                                                             │
│   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐       │
│   │  Office 365 │  │    Gmail    │  │ Outlook.com │       │
│   └─────────────┘  └─────────────┘  └─────────────┘       │
│                                                             │
│                    [Next: Enter Email Address]             │
└─────────────────────────────────────────────────────────────┘

                            ↓

┌─────────────────────────────────────────────────────────────┐
│                    STEP 2 OF 6                              │
│                                                             │
│        ✉️ Enter Your Email Address                          │
│                                                             │
│   Email Address: [support@company.com              ]       │
│   Display Name:  [Support Team                     ]       │
│                                                             │
│   This email will be used for IMAP (receiving) and         │
│   SMTP (sending)                                            │
│                                                             │
│   [Back]                      [Next: SMTP Account Setup]   │
└─────────────────────────────────────────────────────────────┘

                            ↓

┌─────────────────────────────────────────────────────────────┐
│                    STEP 3 OF 6                              │
│            ⚙️ SMTP ACCOUNT SELECTION                         │
│                    (MISSING!!!)                             │
│                                                             │
│   Choose how you want to send outgoing emails:              │
│                                                             │
│   ○ Use Same Account (Recommended)                          │
│     Use support@company.com for both receiving AND sending  │
│                                                             │
│   ● Use Separate Sending Account (Advanced)                 │
│     Receive on one account, send from different account     │
│                                                             │
│     ┌────────────────────────────────────────────┐         │
│     │ SMTP From Email:                            │         │
│     │ [noreply@company.com                ]       │         │
│     │                                             │         │
│     │ SMTP From Name:                             │         │
│     │ [No Reply Bot                       ]       │         │
│     │                                             │         │
│     │ Authentication Type:                        │         │
│     │ ○ OAuth 2.0 (Recommended)                   │         │
│     │   • Client ID: [                    ]       │         │
│     │   • Tenant ID: [                    ]       │         │
│     │   • Client Secret: [                ]       │         │
│     │                                             │         │
│     │ ○ Basic Authentication                      │         │
│     │   • Username: [                     ]       │         │
│     │   • Password: [                     ]       │         │
│     └────────────────────────────────────────────┘         │
│                                                             │
│   [Back]                      [Next: Configure OAuth]      │
└─────────────────────────────────────────────────────────────┘

                            ↓

┌─────────────────────────────────────────────────────────────┐
│                    STEP 4 OF 6                              │
│                                                             │
│        🔐 Configure OAuth Application                       │
│                                                             │
│   (OAuth setup for IMAP receiving account)                  │
│                                                             │
│   Client ID:     [                                  ]       │
│   Tenant ID:     [                                  ]       │
│   Client Secret: [                                  ]       │
│                                                             │
│   [Back]                      [Next: Additional Settings]  │
└─────────────────────────────────────────────────────────────┘

                            ↓

┌─────────────────────────────────────────────────────────────┐
│                    STEP 5 OF 6                              │
│                                                             │
│        ⚙️ Configure Additional Settings                     │
│                                                             │
│   Polling Interval:     [2 minutes (Recommended) ▼]        │
│   OAuth Token Refresh:  [Use System Default      ▼]        │
│   IMAP Folder:          [INBOX                    ]        │
│                                                             │
│   ☑ Enable Email Ticketing                                 │
│   ☑ Send Auto-Acknowledgement                              │
│                                                             │
│   [Back]                      [Next: Authorize Access]     │
└─────────────────────────────────────────────────────────────┘

                            ↓

┌─────────────────────────────────────────────────────────────┐
│                    STEP 6 OF 6                              │
│                                                             │
│        ✅ Authorize Email Access                            │
│                                                             │
│   Final Step: Grant Permissions                            │
│                                                             │
│   1. Sign in with your support@company.com account         │
│   2. Review the requested permissions                      │
│   3. Click "Accept" to grant access                        │
│   4. You'll be redirected back automatically               │
│                                                             │
│   [Back]                      [Save & Authorize Access]    │
└─────────────────────────────────────────────────────────────┘
```

---

## ACTUAL WIZARD FLOW (What Currently Exists)

```
┌─────────────────────────────────────────────────────────────┐
│                    STEP 1 OF 5                              │
│                                                             │
│        📧 Select Your Email Provider                        │
│                                                             │
│   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐       │
│   │  Office 365 │  │    Gmail    │  │ Outlook.com │       │
│   └─────────────┘  └─────────────┘  └─────────────┘       │
│                                                             │
│                    [Next: Enter Email Address]             │
└─────────────────────────────────────────────────────────────┘

                            ↓

┌─────────────────────────────────────────────────────────────┐
│                    STEP 2 OF 5                              │
│                                                             │
│        ✉️ Enter Your Email Address                          │
│                                                             │
│   Email Address: [support@company.com              ]       │
│   Display Name:  [Support Team                     ]       │
│                                                             │
│   This email will be used for IMAP (receiving) and         │
│   SMTP (sending)                                            │
│                                                             │
│   [Back]                      [Next: Azure AD Setup]       │
└─────────────────────────────────────────────────────────────┘

                            ↓

                    ❌ STEP 3 MISSING ❌
              (SMTP Account Selection should be here)

                            ↓

┌─────────────────────────────────────────────────────────────┐
│                    STEP 3 OF 5                              │
│                  (Should be STEP 4)                         │
│                                                             │
│        🔐 Configure OAuth Application                       │
│                                                             │
│   Client ID:     [                                  ]       │
│   Tenant ID:     [                                  ]       │
│   Client Secret: [                                  ]       │
│                                                             │
│   [Back]                      [Next: Configure Settings]   │
└─────────────────────────────────────────────────────────────┘

                            ↓

┌─────────────────────────────────────────────────────────────┐
│                    STEP 4 OF 5                              │
│                  (Should be STEP 5)                         │
│                                                             │
│        ⚙️ Configure Additional Settings                     │
│                                                             │
│   Polling Interval:     [2 minutes (Recommended) ▼]        │
│   OAuth Token Refresh:  [Use System Default      ▼]        │
│   IMAP Folder:          [INBOX                    ]        │
│                                                             │
│   ☑ Enable Email Ticketing                                 │
│   ☑ Send Auto-Acknowledgement                              │
│                                                             │
│   [Back]                      [Next: Authorize Access]     │
└─────────────────────────────────────────────────────────────┘

                            ↓

┌─────────────────────────────────────────────────────────────┐
│                    STEP 5 OF 5                              │
│                  (Should be STEP 6)                         │
│                                                             │
│        ✅ Authorize Email Access                            │
│                                                             │
│   Final Step: Grant Permissions                            │
│                                                             │
│   [Back]                      [Save & Authorize Access]    │
└─────────────────────────────────────────────────────────────┘
```

---

## THE PROBLEM

### Missing Step 3 Means:

**Current Behavior:**
- User selects email: support@company.com
- System uses support@company.com for BOTH:
  - ✅ Receiving emails (IMAP)
  - ✅ Sending emails (SMTP)

**Cannot Do:**
- ❌ Receive on support@company.com
- ❌ Send from noreply@company.com
- ❌ Different SMTP authentication
- ❌ Security separation

---

## USE CASE EXAMPLES

### Use Case 1: No-Reply Setup

**CURRENT (Forced):**
```
Customer → support@company.com (IMAP) → System creates ticket
System → support@company.com (SMTP) → Auto-acknowledgement to customer
```

**DESIRED (Blocked):**
```
Customer → support@company.com (IMAP) → System creates ticket
System → noreply@company.com (SMTP) → Auto-acknowledgement to customer
```

Why? Prevent customers from replying to automated emails.

---

### Use Case 2: Different Branding

**CURRENT (Forced):**
```
Customer emails: support@company.com
Customer receives from: support@company.com
```

**DESIRED (Blocked):**
```
Customer emails: support@company.com (IMAP)
Customer receives from: info@company.com (SMTP)
```

Why? Different sender identity for branding.

---

### Use Case 3: Security Separation

**CURRENT (Forced):**
```
Account: support@company.com
Permissions: Read emails + Send emails
Risk: If compromised, attacker can read AND send
```

**DESIRED (Blocked):**
```
IMAP Account: tickets@company.com (Read-only)
SMTP Account: notifications@company.com (Send-only)
Risk: If one compromised, limited blast radius
```

Why? Minimize security risk.

---

## IMPACT ANALYSIS

### What Works Today
✅ Can configure email ticketing with OAuth 2.0
✅ Can receive customer emails
✅ Can send auto-acknowledgements
✅ Can poll for new emails
✅ System creates tickets automatically

### What's Blocked
❌ Cannot use separate SMTP account
❌ Cannot implement no-reply setups
❌ Cannot separate sender branding
❌ Cannot implement security best practices
❌ Cannot use different SMTP authentication

---

## COMPARISON TABLE

| Capability | Expected | Actual | Status |
|------------|----------|--------|--------|
| 6-step wizard | ✅ | ❌ | FAIL |
| 5-step wizard | ❌ | ✅ | (Wrong) |
| Select provider | ✅ | ✅ | PASS |
| Enter email | ✅ | ✅ | PASS |
| **SMTP account selection** | **✅** | **❌** | **FAIL** |
| Configure OAuth | ✅ | ✅ | PASS |
| Additional settings | ✅ | ✅ | PASS |
| Authorize access | ✅ | ✅ | PASS |
| Use same account | ✅ | ✅ (forced) | PASS |
| Use separate SMTP | ✅ | ❌ | FAIL |
| SMTP OAuth | ✅ | ❌ | FAIL |
| SMTP Basic Auth | ✅ | ❌ | FAIL |

---

## STEP 3 DETAILED REQUIREMENTS

### Radio Button Options

**Option 1: Use Same Account (Default)**
```
● Use Same Account (Recommended)

  ✓ Easier to set up
  ✓ Single OAuth authorization
  ✓ Recommended for most users

  Use support@company.com for both receiving and sending
```

**Option 2: Use Separate Sending Account (Advanced)**
```
○ Use Separate Sending Account (Advanced)

  ✓ Professional no-reply setup
  ✓ Different sender branding
  ✓ Enhanced security separation

  Use a different email address for sending

  ┌─────────────────────────────────────┐
  │ SMTP Configuration                  │
  ├─────────────────────────────────────┤
  │ From Email: [noreply@company.com  ] │
  │ From Name:  [No Reply Bot         ] │
  │                                     │
  │ Authentication Type:                │
  │ ○ OAuth 2.0 (Recommended)           │
  │   Client ID:     [              ]   │
  │   Tenant ID:     [              ]   │
  │   Client Secret: [              ]   │
  │                                     │
  │ ○ Basic Authentication (Legacy)     │
  │   Username: [                   ]   │
  │   Password: [                   ]   │
  └─────────────────────────────────────┘
```

---

## VALIDATION RULES

### When "Use Same Account" selected:
- No additional validation needed
- Proceed to next step immediately

### When "Use Separate Sending Account" selected:
- SMTP From Email: Required, must be valid email format
- SMTP From Name: Required, min 2 characters
- Authentication Type: Required (OAuth or Basic)

**If OAuth selected:**
- SMTP OAuth Client ID: Required, GUID format
- SMTP OAuth Tenant ID: Required, GUID format
- SMTP OAuth Client Secret: Required, min 10 characters

**If Basic Auth selected:**
- SMTP Username: Required, usually email format
- SMTP Password: Required, min 8 characters

---

## DATABASE IMPACT

### New Columns in EmailConfiguration Table

```typescript
usesSeparateSmtp: boolean;           // Default: false
smtpFromEmail: string | null;        // If separate: email address
smtpFromName: string | null;         // If separate: display name
smtpAuthType: 'oauth' | 'basic' | null; // If separate: auth type

// OAuth fields (if smtpAuthType === 'oauth')
smtpOAuthClientId: string | null;
smtpOAuthTenantId: string | null;
smtpOAuthClientSecret: string | null; // Encrypted
smtpOAuthAccessToken: string | null;  // Encrypted
smtpOAuthRefreshToken: string | null; // Encrypted
smtpOAuthTokenExpiry: Date | null;

// Basic auth fields (if smtpAuthType === 'basic')
smtpUsername: string | null;
smtpPassword: string | null;          // Encrypted
```

---

## IMPLEMENTATION CHECKLIST

### Frontend (4-6 hours)
- [ ] Add Step 3 between current Step 2 and Step 3
- [ ] Create radio button UI for account selection
- [ ] Implement conditional fields for separate SMTP
- [ ] Add validation logic
- [ ] Update step numbers (3→4, 4→5, 5→6)

### Backend (4-6 hours)
- [ ] Add new columns to database
- [ ] Update EmailConfiguration model
- [ ] Update API endpoints (POST, PUT)
- [ ] Add validation for SMTP fields
- [ ] Encrypt sensitive SMTP credentials

### OAuth Flow (4-6 hours)
- [ ] Implement separate OAuth flow for SMTP
- [ ] Store separate OAuth tokens
- [ ] Implement token refresh for SMTP
- [ ] Handle token expiry

### Testing (8-11 hours)
- [ ] Unit tests for Step 3 component
- [ ] Integration tests for API
- [ ] E2E tests for wizard flow
- [ ] Test email sending from separate SMTP
- [ ] Test OAuth authorization for SMTP

---

## VISUAL SUMMARY

```
EXPECTED WIZARD:  [1] → [2] → [3 SMTP] → [4 OAuth] → [5 Settings] → [6 Auth]
                                  ↑
                              MISSING!

ACTUAL WIZARD:    [1] → [2] → ❌ → [3 OAuth] → [4 Settings] → [5 Auth]
```

**Fix Required:** Insert Step 3 (SMTP Account Selection)

---

## QUICK STATS

| Metric | Value |
|--------|-------|
| Expected Steps | 6 |
| Actual Steps | 5 |
| Missing Steps | 1 (Step 3) |
| Blocked Use Cases | 3+ |
| Implementation Time | 25-37 hours |
| Priority | CRITICAL |
| Production Ready | NO |

---

## NEXT STEPS

1. ✅ Review this visual comparison
2. ⏳ Read STEP3_IMPLEMENTATION_CHECKLIST.md
3. ⏳ Implement Step 3 in wizard
4. ⏳ Test thoroughly
5. ⏳ Deploy to production

**Status:** CRITICAL ISSUE - Cannot deploy without Step 3

---

**END OF VISUAL COMPARISON**
