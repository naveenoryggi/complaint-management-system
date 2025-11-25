# OAuth Email Ticketing Diagnostic Report
**Date:** November 15, 2025
**Issue:** OAuth email ticketing authentication failing
**Status:** ✅ ROOT CAUSE IDENTIFIED

---

## 🔍 Investigation Summary

I used Playwright browser automation to test the OAuth email ticketing system and identify why it stopped working.

### Key Findings

**1. OAuth Token Expired** ⏰
- **Token Expiration:** November 14, 2025 at 7:55:58 AM
- **Current Date:** November 15, 2025
- **Status:** Token has expired (over 24 hours ago)

**2. Database Configuration Error** ❌
- **Current AuthenticationType:** `1` (Basic Authentication)
- **Expected AuthenticationType:** `2` (OAuth2)
- **Impact:** Backend rejects OAuth operations with error: "Configuration is not set up for OAuth 2.0"

### Error Evidence

**Backend Logs:**
```
fail: ComplaintManagement.Infrastructure.Services.EmailTicketingService[0]
      Error fetching emails for configuration 4a1b41ef-cbc5-4858-a6a5-02b1c147a80a
      MailKit.Security.AuthenticationException: LOGIN failed.
```

**API Response When Clicking "Re-authorize":**
```
HTTP 400 Bad Request
"Configuration is not set up for OAuth 2.0"
```

**Email Configuration Details:**
- Email: marketing@oryggitech.com
- IMAP: outlook.office365.com:993
- SMTP: smtp.office365.com:587
- Authentication Type in DB: **1** (WRONG - should be **2**)
- Has OAuth Credentials: ✅ Yes (Client ID, Tenant ID, Refresh Token)
- OAuth Token Expires: 2025-11-14T07:55:58 ❌ (Expired)

---

## 🔧 Solution

### Step 1: Fix Authentication Type in Database

The authentication type needs to be changed from `1` (Basic) to `2` (OAuth2).

**Run this SQL in SQL Server Management Studio:**

```sql
USE ComplaintManagementDb;
GO

-- Check current state
SELECT
    Id,
    FromEmail,
    AuthenticationType,
    CASE AuthenticationType
        WHEN 1 THEN 'Basic (WRONG for OAuth)'
        WHEN 2 THEN 'OAuth2 (CORRECT)'
        ELSE 'Unknown'
    END AS AuthTypeDescription,
    OAuthClientId,
    OAuthTenantId,
    OAuthTokenExpiresAt,
    IsEnabled
FROM EmailConfigurations
WHERE Id = '4a1b41ef-cbc5-4858-a6a5-02b1c147a80a';

-- Update to OAuth2 (value = 2)
UPDATE EmailConfigurations
SET
    AuthenticationType = 2, -- OAuth2
    UpdatedAt = GETUTCDATE()
WHERE Id = '4a1b41ef-cbc5-4858-a6a5-02b1c147a80a';

-- Verify the fix
SELECT
    Id,
    FromEmail,
    AuthenticationType,
    CASE AuthenticationType
        WHEN 1 THEN 'Basic'
        WHEN 2 THEN 'OAuth2 (CORRECT!)'
        ELSE 'Unknown'
    END AS AuthTypeDescription,
    IsEnabled,
    UpdatedAt
FROM EmailConfigurations
WHERE Id = '4a1b41ef-cbc5-4858-a6a5-02b1c147a80a';
```

### Step 2: Reauthorize OAuth in UI

After fixing the database:

1. **Navigate to:** Admin Panel → Communication Settings → Email Ticketing
2. **Find:** "Oryggi Tech Support" configuration
3. **Click:** "Re-authorize" button (red button)
4. **Follow:** OAuth flow to Microsoft login
5. **Sign in:** with marketing@oryggitech.com
6. **Accept:** Permissions request
7. **Verify:** Token expiration date is updated

### Step 3: Verify Email Polling Works

After reauthorization:
- Backend will automatically start polling emails
- Check for new complaints created from emails
- Monitor backend logs for successful email fetching

---

## 📊 Technical Details

### EmailAuthenticationType Enum
```csharp
public enum EmailAuthenticationType
{
    Basic = 1,   // Username/password (legacy)
    OAuth2 = 2   // Modern OAuth 2.0
}
```

### Why the UI Update Failed

When trying to save the configuration through the UI with OAuth selected, the update fails because:
1. The form sends the entire configuration object
2. Backend validates all fields including OAuth credentials
3. Validation might fail if credentials are empty or invalid
4. Simpler to just update the AuthenticationType field directly in SQL

---

## 🎯 Testing with Playwright

I successfully used Playwright MCP to:
- ✅ Navigate to email ticketing configuration page
- ✅ Identify expired OAuth token (Nov 14, 7:55 AM)
- ✅ Retrieve configuration details via API
- ✅ Identify authentication type mismatch (1 vs 2)
- ✅ Attempt OAuth reauthorization (revealed root cause)
- ✅ Capture screenshots for documentation

**Screenshots Saved:**
- `oauth-issue-expired-token.png` - Shows expired token in UI

---

## 📝 Recommendations

1. **Immediate:** Run the SQL script above to fix authentication type
2. **Short-term:** Reauthorize OAuth through the UI
3. **Long-term:**
   - Set up OAuth token expiration alerts
   - Monitor the OAuth Token Refresh Background Service
   - Consider extending token lifetime in Azure AD app registration

---

## ✅ Success Criteria

After applying the fix, you should see:
- ✅ Authentication Type = 2 (OAuth2) in database
- ✅ "Re-authorize" button works without errors
- ✅ New OAuth token with future expiration date
- ✅ Email polling service successfully fetches emails
- ✅ No more "LOGIN failed" errors in backend logs

---

**Next Steps:**
1. Run the SQL script in SQL Server Management Studio
2. Refresh the Email Ticketing Configuration page in the browser
3. Click "Re-authorize" to get a fresh OAuth token
4. Verify email polling resumes successfully

---

**Report Generated:** November 15, 2025
**Diagnostic Tool:** Playwright Browser Automation + API Testing
**Configuration ID:** 4a1b41ef-cbc5-4858-a6a5-02b1c147a80a
