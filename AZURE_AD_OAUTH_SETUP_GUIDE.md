# Azure AD OAuth 2.0 Setup Guide for Email Ticketing
## Complete Step-by-Step Configuration

**Date:** November 13, 2025
**System:** Complaint Management System - Email Ticketing Module
**Purpose:** Configure Microsoft Office 365 OAuth 2.0 for automated email processing

---

## Prerequisites

✅ **You Need:**
1. Microsoft 365 Business/Enterprise subscription
2. Azure AD admin access (Global Administrator or Application Administrator role)
3. Email account to monitor: `marketing@oryggitech.com`
4. Application running at: `http://localhost:4200` (frontend) and `http://localhost:5000` (backend)

✅ **Time Required:** 30-45 minutes

---

## Part 1: Azure AD App Registration (20 minutes)

### Step 1: Access Azure Portal

1. Open browser and navigate to: **https://portal.azure.com**
2. Sign in with your Microsoft 365 admin account
3. In the search bar at the top, type: **"Azure Active Directory"**
4. Click on **Azure Active Directory** in the results

### Step 2: Create App Registration

1. In the left sidebar, click **App registrations**
2. Click **+ New registration** button at the top
3. Fill in the registration form:

```
Application Name: Complaint Management Email Integration
Supported Account Types: ☑ Accounts in this organizational directory only (Single tenant)
Redirect URI:
  Platform: Web
  URI: http://localhost:5000/api/oauth/callback
```

4. Click **Register** button at the bottom
5. **IMPORTANT:** After registration, you'll see the app overview page
6. **Copy and save** these values immediately:

```
Application (client) ID: [Copy this - you'll need it]
Directory (tenant) ID: [Copy this - you'll need it]
```

**Screenshot Tip:** Take a screenshot of this page for reference

### Step 3: Create Client Secret

1. In the left sidebar of your app, click **Certificates & secrets**
2. Click the **Client secrets** tab
3. Click **+ New client secret** button
4. Fill in the form:

```
Description: Complaint Management Email Access
Expires: 24 months (recommended for production)
```

5. Click **Add**
6. **CRITICAL:** Immediately copy the **Value** field (not the Secret ID)
7. **Save it now** - you won't be able to see it again!

```
Client Secret Value: [Copy and save this immediately!]
```

**⚠️ WARNING:** This secret is shown only ONCE. If you lose it, you'll need to create a new one.

### Step 4: Configure API Permissions

1. In the left sidebar, click **API permissions**
2. Click **+ Add a permission**
3. Click **Microsoft Graph**
4. Click **Delegated permissions**
5. Search for and select these permissions:

```
☑ Mail.Read            - Read user mail
☑ Mail.ReadWrite       - Read and write access to user mail
☑ IMAP.AccessAsUser.All - Access mailboxes via IMAP
☑ SMTP.Send            - Send mail as a user
☑ User.Read            - Sign in and read user profile
☑ offline_access       - Maintain access to data
```

6. Click **Add permissions** button at the bottom
7. **IMPORTANT:** Click **Grant admin consent for [Your Organization]**
8. Click **Yes** to confirm
9. Verify all permissions show **"Granted for [Your Organization]"** with green checkmarks

**Screenshot Tip:** Take a screenshot showing all permissions granted

### Step 5: Configure Authentication

1. In the left sidebar, click **Authentication**
2. Under **Platform configurations**, you should see your Web redirect URI
3. Scroll down to **Advanced settings**
4. Under **Allow public client flows**, ensure it's set to **No**
5. Under **Supported account types**, verify **Single tenant** is selected
6. Click **Save** at the top

---

## Part 2: Application Configuration (10 minutes)

### Step 6: Prepare Your Credentials

You should now have these three values from Azure AD:

```
Client ID (Application ID): xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
Tenant ID (Directory ID): xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
Client Secret: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

### Step 7: Enter Credentials in Application

1. Open your browser to: **http://localhost:4200**
2. Log in with admin credentials:
   - Username: `admin@complaintmanagement.com`
   - Password: `Admin@123`

3. Navigate to: **Admin Panel** → **Communication Settings** → **Email Ticketing Configuration**

4. You should see one existing configuration card for "Oryggi Tech Support"
   - Current badge: **"OAuth 2.0 - Pending"** (orange, pulsing)
   - Button visible: **"Authorize Now"**

5. Click the **Edit** button (pencil icon) on the configuration card

6. The OAuth wizard will open. Click **Next** through the pages until you reach **"OAuth Credentials"**

7. Fill in the form with your Azure AD credentials:

```
Authentication Type: OAuth 2.0
Email Provider: Microsoft Office 365
Client ID: [Paste from Step 6]
Tenant ID: [Paste from Step 6]
Client Secret: [Paste from Step 6]
```

8. Click **Save Configuration** (do NOT click "Save & Authorize Access" yet)

9. You'll be returned to the main page. The badge should still show **"OAuth 2.0 - Pending"**

---

## Part 3: OAuth Authorization Flow (10 minutes)

### Step 8: Complete Authorization

1. On the Email Ticketing Configuration page, you should now see:
   - Badge: **"OAuth 2.0 - Pending"** (orange, pulsing)
   - Button: **"Authorize Now"** (yellow/warning color)

2. Click the **"Authorize Now"** button

3. You'll be redirected to Microsoft login page:
   - URL will be: `https://login.microsoftonline.com/...`

4. **Sign in** with the email account you want to monitor:
   - Email: `marketing@oryggitech.com`
   - Password: [Your Office 365 password]

5. You'll see a **consent screen** asking for permissions:

```
Complaint Management Email Integration wants to:
☑ Read your mail
☑ Read and write mail in your mailbox
☑ Access mailboxes via IMAP
☑ Send mail as you
☑ Maintain access to data you have given it access to
☑ Sign you in and read your profile
```

6. **Review the permissions** and click **Accept**

7. You'll be redirected back to your application:
   - URL: `http://localhost:5000/api/oauth/callback?code=...`
   - Then: `http://localhost:4200/admin/communication/email-ticketing`

8. **Verify Authorization Success:**
   - Badge should now show: **"OAuth 2.0 - Authorized"** (green color)
   - Button should change to: **"Refresh OAuth"** (blue color)
   - Last polled: Will update within 5 minutes

---

## Part 4: Testing & Verification (5 minutes)

### Step 9: Test Email Polling

1. On the configuration card, click **"Poll Now"** button
2. Watch for the spinning icon
3. Check the browser console (F12) for any errors
4. Backend logs should show:

```
✅ Expected: "Successfully authenticated with IMAP server"
❌ Not Expected: "No cached account found" or "Failed to refresh access token"
```

### Step 10: Send Test Email

1. From another email account (Gmail, Outlook, etc.), send an email to: `marketing@oryggitech.com`
2. Subject: "Test Complaint - Broken Equipment"
3. Body: "The coffee machine in Building A is not working. Please send a technician."

4. Wait 5 minutes for automatic polling, or click **"Poll Now"** again

5. Navigate to: **Complaints** → **Complaint List**

6. You should see a new complaint created from the email:
   - Title: "Test Complaint - Broken Equipment"
   - Description: "The coffee machine in Building A is not working..."
   - Status: "New" or "Open"
   - Created from: Email Ticketing System

---

## Troubleshooting

### Issue: "redirect_uri_mismatch" Error

**Cause:** Redirect URI doesn't match exactly

**Solution:**
1. In Azure AD app registration, go to **Authentication**
2. Verify redirect URI is exactly: `http://localhost:5000/api/oauth/callback`
3. No trailing slash, exact match including port number
4. Click **Save**

### Issue: "AADSTS50011: The reply URL does not match"

**Cause:** Application is configured for different callback URL

**Solution:**
1. Check your backend `appsettings.json`:
   ```json
   "OAuth": {
     "RedirectUri": "http://localhost:5000/api/oauth/callback"
   }
   ```
2. Ensure it matches Azure AD configuration

### Issue: "Insufficient privileges to complete the operation"

**Cause:** Missing API permissions or admin consent not granted

**Solution:**
1. Azure AD → App registrations → Your app → API permissions
2. Verify all 6 permissions are present
3. Click **Grant admin consent**
4. Refresh the page and verify green checkmarks

### Issue: "No cached account found" Error (Backend Logs)

**Cause:** Authorization flow not completed

**Solution:**
1. This is expected BEFORE you click "Authorize Now"
2. Complete Step 8 (OAuth Authorization Flow)
3. After authorization, this error should disappear

### Issue: Token Expired After 60 Days

**Cause:** Refresh token expired (Microsoft limitation)

**Solution:**
1. Click **"Authorize Now"** button again
2. Re-authenticate with your Office 365 account
3. Background service will handle token refresh automatically for next 60 days

---

## Security Best Practices

### Production Deployment

When deploying to production, update these settings:

1. **Azure AD Redirect URI:**
   ```
   Change from: http://localhost:5000/api/oauth/callback
   Change to: https://your-production-domain.com/api/oauth/callback
   ```

2. **Application Configuration:**
   ```json
   "OAuth": {
     "RedirectUri": "https://your-production-domain.com/api/oauth/callback",
     "TokenRefreshIntervalMinutes": 60,
     "TokenExpiryWarningDays": 7
   }
   ```

3. **Client Secret Storage:**
   - Store in Azure Key Vault or equivalent
   - Never commit to source control
   - Use environment variables in production

### Token Management

The system includes automatic token refresh:
- **Refresh Interval:** Every 60 minutes (configurable)
- **Warning Period:** 7 days before expiry (configurable)
- **Background Service:** `OAuthTokenRefreshBackgroundService`
- **Logs Location:** Backend console output

---

## Support & Documentation

### Backend Services Running

1. **OAuth Token Refresh Service:**
   - Runs every 60 minutes
   - Checks tokens expiring within 7 days
   - Automatically refreshes before expiry
   - Logs: `info: OAuthTokenRefreshBackgroundService`

2. **Email Polling Service:**
   - Runs every 5 minutes (configurable)
   - Fetches new emails from configured accounts
   - Creates complaints from emails
   - Logs: `info: EmailPollingBackgroundService`

### Configuration Files

- **Backend:** `complaint-system-dotnet/src/ComplaintManagement.API/appsettings.json`
- **Frontend:** Angular environment files (no OAuth config needed)

### Key Code Files

- **OAuth Service:** `ComplaintManagement.Infrastructure/Services/EmailOAuthService.cs`
- **Token Refresh:** `OAuthTokenRefreshBackgroundService.cs`
- **Email Processing:** `EmailTicketingService.cs`
- **Frontend Component:** `email-ticketing-config.component.ts`

---

## Next Steps

After completing this setup:

1. ✅ OAuth is configured and authorized
2. ✅ Email polling is active (every 5 minutes)
3. ✅ Automatic token refresh enabled
4. ✅ Email-to-complaint workflow operational

**You can now:**
- Monitor incoming emails automatically
- Create complaints from emails
- Respond to complaints via email
- Track email thread conversations

---

## FAQ

**Q: How often does the system check for new emails?**
A: Every 5 minutes by default. Configurable in `EmailConfiguration.PollingIntervalMinutes`.

**Q: What happens if my token expires?**
A: The background service automatically refreshes tokens 7 days before expiry. If refresh fails, you'll need to re-authorize.

**Q: Can I use multiple email accounts?**
A: Yes! Click "Add Email Configuration" to set up additional accounts. Each needs separate OAuth authorization.

**Q: Does this work with Gmail?**
A: Yes! The wizard supports Gmail OAuth 2.0. Follow similar steps in Google Cloud Console.

**Q: Is my email password stored in the database?**
A: No! OAuth 2.0 uses tokens, not passwords. Only encrypted tokens are stored.

---

## Summary Checklist

Before you start:
- [ ] Azure AD admin access
- [ ] Office 365 email account
- [ ] Application running locally

Azure AD configuration:
- [ ] App registration created
- [ ] Client ID copied
- [ ] Tenant ID copied
- [ ] Client secret created and copied
- [ ] 6 API permissions added
- [ ] Admin consent granted
- [ ] Redirect URI configured

Application configuration:
- [ ] Logged into application as admin
- [ ] Edited email configuration
- [ ] Entered Client ID, Tenant ID, Secret
- [ ] Saved configuration

Authorization:
- [ ] Clicked "Authorize Now"
- [ ] Signed in with Office 365 account
- [ ] Accepted permissions
- [ ] Badge shows "OAuth 2.0 - Authorized" (green)

Testing:
- [ ] Clicked "Poll Now" - no errors
- [ ] Sent test email
- [ ] Complaint created from email
- [ ] System operational

---

**Congratulations!** 🎉
Your OAuth 2.0 email ticketing system is now fully configured and operational!

For technical support or questions about this setup, refer to the test reports:
- `OAUTH_PROVIDER_SWITCHING_TEST_REPORT.md`
- `OAUTH_EMAIL_TICKETING_E2E_TEST_REPORT.md`
- `OAUTH_UI_IMPROVEMENTS_TEST_REPORT.md`
