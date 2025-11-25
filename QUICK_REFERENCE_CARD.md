# Email Modules Testing - Quick Reference Card

## CRITICAL FINDING

**MISSING FEATURE:** Step 3 - SMTP Account Selection in Email Ticketing Wizard

**Status:** NOT PRODUCTION READY - Implement Step 3 first

---

## What's Missing?

### Current Wizard (5 Steps)
```
Step 1: Select Provider
Step 2: Enter Email
Step 3: Configure OAuth  <-- Should be Step 4
Step 4: Additional Settings <-- Should be Step 5
Step 5: Authorize           <-- Should be Step 6
```

### Expected Wizard (6 Steps)
```
Step 1: Select Provider
Step 2: Enter Email
Step 3: SMTP Account Selection  <-- MISSING
Step 4: Configure OAuth
Step 5: Additional Settings
Step 6: Authorize
```

---

## Missing Step 3: SMTP Account Selection

Users should be able to choose:

**Option 1: Use Same Account (Default)**
- IMAP and SMTP use the same email
- Example: support@company.com for both receiving and sending

**Option 2: Use Separate Sending Account (Advanced)**
- IMAP receives on one email
- SMTP sends from different email
- Example: Receive on support@, send from noreply@

### When "Use Separate Sending Account" selected:

**Required Fields:**
- SMTP From Email: noreply@company.com
- SMTP From Name: "No Reply Bot"
- Authentication Type: OAuth 2.0 or Basic Auth

**If OAuth 2.0:**
- SMTP OAuth Client ID
- SMTP OAuth Tenant ID
- SMTP OAuth Client Secret

**If Basic Auth:**
- SMTP Username
- SMTP Password

---

## Use Cases Being Blocked

1. **No-Reply Setup**
   - Receive: support@company.com
   - Send: noreply@company.com
   - Why: Prevent replies to auto-acknowledgements

2. **Different Branding**
   - Receive: support@company.com
   - Send: info@company.com
   - Why: Different sender identity

3. **Security Separation**
   - Receive: tickets@company.com (limited permissions)
   - Send: notifications@company.com (send-only)
   - Why: Minimize security risk

---

## Test Results Summary

### Email Settings Module
- Navigation: PASS
- Form Fields: PASS
- OAuth Support: FAIL (not supported)
- Status: PASS (with limitations)

### Email Ticketing Module
- Navigation: PASS
- System Settings: PASS
- Wizard Steps 1-2: PASS
- **Wizard Step 3 (SMTP): FAIL (MISSING)**
- Wizard Steps 4-5: PASS
- Status: FAIL (critical feature missing)

---

## Implementation Effort

**Estimated Time:** 25-37 hours
**Recommended Sprint:** 2 sprints

**Breakdown:**
- Frontend UI: 4-6 hours
- Backend API: 4-6 hours
- OAuth Flow: 4-6 hours
- Database Migration: 1-2 hours
- Testing: 8-11 hours
- Documentation: 2-3 hours
- QA: 2-3 hours

---

## Priority Actions

1. Read [CRITICAL_FINDINGS_SUMMARY.md](./CRITICAL_FINDINGS_SUMMARY.md)
2. Review [STEP3_IMPLEMENTATION_CHECKLIST.md](./STEP3_IMPLEMENTATION_CHECKLIST.md)
3. Implement Step 3 in Email Ticketing wizard
4. Test thoroughly
5. Deploy to staging
6. QA approval
7. Production deployment

---

## Database Changes Needed

```sql
ALTER TABLE EmailConfiguration
ADD COLUMN usesSeparateSmtp BOOLEAN DEFAULT FALSE,
ADD COLUMN smtpFromEmail VARCHAR(255),
ADD COLUMN smtpFromName VARCHAR(255),
ADD COLUMN smtpAuthType VARCHAR(20),
ADD COLUMN smtpOAuthClientId VARCHAR(255),
ADD COLUMN smtpOAuthTenantId VARCHAR(255),
ADD COLUMN smtpOAuthClientSecret TEXT,
ADD COLUMN smtpUsername VARCHAR(255),
ADD COLUMN smtpPassword TEXT;
```

---

## API Endpoints to Update

- `POST /api/email-configuration` - Accept new SMTP fields
- `PUT /api/email-configuration/:id` - Handle SMTP updates
- `POST /api/email-configuration/:id/test-smtp` - Test SMTP connection

---

## Frontend Components to Update

- `email-ticketing-config.component.ts` - Add Step 3
- `email-ticketing-config.component.html` - Add Step 3 UI
- `email-ticketing-config.component.scss` - Add Step 3 styles

---

## Quick Comparison

| Feature | Email Settings | Email Ticketing |
|---------|----------------|-----------------|
| Purpose | Send notifications | Email-to-ticket |
| Direction | Outbound only | Inbound + Outbound |
| Auth | Basic only | OAuth only |
| OAuth | NO | YES |
| Separate SMTP | N/A | NO (MISSING) |
| Production Ready | YES | NO |

---

## Both Modules Required?

**YES** - They serve different purposes:

- **Email Settings:** System sends notifications
- **Email Ticketing:** Customers send emails, become tickets

They work together, cannot be merged.

---

## Documentation

All test reports and implementation guides:

1. **EMAIL_TESTING_INDEX.md** - Navigation guide
2. **EMAIL_MODULES_COMPREHENSIVE_TEST_REPORT.md** - Full test report
3. **CRITICAL_FINDINGS_SUMMARY.md** - Executive summary
4. **STEP3_IMPLEMENTATION_CHECKLIST.md** - Implementation guide
5. **QUICK_REFERENCE_CARD.md** - This file

---

## Screenshots Evidence

Located in: `.playwright-mcp\.playwright-mcp\`

- test-01 to test-03: Email Settings
- test-04 to test-07: Email Ticketing

---

## Contact

**Tester:** Elite QA Automation Engineer (Claude Code)
**Date:** 2025-11-17
**Status:** CRITICAL ISSUE - Do not deploy without fixing

---

**Bottom Line:** Implement Step 3 before production deployment. See STEP3_IMPLEMENTATION_CHECKLIST.md for details.
