# OAuth 2.0 Quick Start Guide - Next Steps

**Date:** November 13, 2025
**Status:** ✅ Ready to Configure OAuth
**Time Required:** 30-45 minutes

---

## Current Status

✅ **Backend**: Running on http://localhost:5000
✅ **Frontend**: Running on http://localhost:4200
✅ **OAuth UI Improvements**: Deployed and tested
✅ **Azure AD Setup Guide**: Created (`AZURE_AD_OAUTH_SETUP_GUIDE.md`)
⚠️ **Database Fix Needed**: `authenticationType` needs updating

---

## Step 1: Fix Database (2 minutes) ⚠️ **DO THIS FIRST**

**Option A - Using SQL Server Management Studio (Recommended):**

1. Open SQL Server Management Studio (SSMS)
2. Connect to: `PRANA-ASUS\SQLEXPRESS`
3. Open the file: `fix-db-auth-type.sql`
4. Execute the script (F5)
5. Verify the output shows `AuthenticationType = 1`

**Option B - Using Command Line:**

```powershell
# Navigate to project directory
cd "C:\Users\Navin Chandra\Pictures\Complaint management system"

# Run the SQL file (if you have sqlcmd installed)
sqlcmd -S "PRANA-ASUS\SQLEXPRESS" -d ComplaintManagementDb -i fix-db-auth-type.sql
```

**What This Does:**
- Changes `AuthenticationType` from `2` (invalid) to `1` (OAuth 2.0)
- After this, the UI will correctly show OAuth status badges

---

## Step 2: Verify UI Changes (2 minutes)

1. **Open your browser** to: http://localhost:4200
2. **Log in** with: `admin@complaintmanagement.com` / `Admin@123`
3. **Navigate to**: Admin Panel → Communication Settings → Email Ticketing Configuration
4. **Look for** the "Oryggi Tech Support" configuration card
5. **Check the badge** - it should now show:
   - "OAuth 2.0 - Expired" (red badge) if the token expired
   - "OAuth 2.0 - Pending" (orange pulsing badge) if no token
   - ~~"Basic Auth"~~ (this should be gone now)

**Screenshot Location:**
- Current state: `.playwright-mcp/.playwright-mcp/oauth-ui-test-01-before-fix-basic-auth-badge.png`
- After fix: Take a new screenshot for comparison

---

## Step 3: Azure AD App Registration (20 minutes)

Now that the database is fixed, follow the comprehensive guide to set up Azure AD:

**Open the guide:** `AZURE_AD_OAUTH_SETUP_GUIDE.md`

**Quick checklist:**
- [ ] Step 1-2: Create app registration in Azure portal
- [ ] Step 3: Create client secret (save it immediately!)
- [ ] Step 4: Configure API permissions (6 permissions required)
- [ ] Step 5: Grant admin consent

**What You'll Get:**
- Client ID (Application ID): `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- Tenant ID (Directory ID): `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- Client Secret: `xxxxxxxxxxxxxxxxxxxxxxxxx` (save this!)

---

## Step 4: Enter Credentials in Application (5 minutes)

1. **In the Email Ticketing Configuration page**, click **Edit** (pencil icon) on "Oryggi Tech Support"
2. **The OAuth wizard will open** - navigate to the "OAuth Credentials" step
3. **Fill in the form:**
   - Authentication Type: OAuth 2.0
   - Email Provider: Microsoft Office 365
   - Client ID: [Paste from Azure AD]
   - Tenant ID: [Paste from Azure AD]
   - Client Secret: [Paste from Azure AD]
4. **Click "Save Configuration"** (not "Save & Authorize" yet)
5. **Return to main page** - badge should still show "OAuth 2.0 - Pending" (orange, pulsing)

---

## Step 5: Complete OAuth Authorization Flow (10 minutes)

1. **Click "Authorize Now"** button on the configuration card
2. **You'll be redirected** to Microsoft login page
3. **Sign in** with: `marketing@oryggitech.com` and your Office 365 password
4. **Accept permissions** on the consent screen
5. **You'll be redirected back** to the application
6. **Verify success:**
   - Badge changes to: "OAuth 2.0 - Authorized" (green)
   - Button changes to: "Refresh OAuth" (blue)
   - Last polled: Will update within 5 minutes

---

## Step 6: Test Email Polling (5 minutes)

1. **Click "Poll Now"** button
2. **Check backend logs** for: `✅ Successfully authenticated with IMAP server`
3. **Send test email** to `marketing@oryggitech.com`:
   - From: Any email account
   - Subject: "Test - Coffee Machine Broken"
   - Body: "The coffee machine in Building A needs repair"
4. **Wait 5 minutes** or click "Poll Now" again
5. **Navigate to**: Complaints → Complaint List
6. **Verify**: New complaint created from email

---

## Troubleshooting

### Issue: Badge still shows "Basic Auth" after database fix

**Solution:**
1. Clear browser cache (Ctrl+Shift+Delete)
2. Refresh the page (Ctrl+F5)
3. Check backend logs for any errors
4. Verify database update: Run `SELECT AuthenticationType FROM EmailConfigurations WHERE FromEmail = 'marketing@oryggitech.com'` - should return `1`

### Issue: "redirect_uri_mismatch" error during authorization

**Solution:**
1. In Azure AD app registration → Authentication
2. Verify redirect URI is exactly: `http://localhost:5000/api/oauth/callback`
3. No trailing slash, exact port number
4. Click Save

### Issue: "AADSTS50011: The reply URL does not match"

**Solution:**
1. Check backend `appsettings.json`:
   ```json
   "OAuth": {
     "RedirectUri": "http://localhost:5000/api/oauth/callback"
   }
   ```
2. Must match Azure AD configuration exactly

### Issue: Backend shows "No cached account found"

**Solution:**
- This is **expected** before you click "Authorize Now"
- After authorization, this error should disappear
- If it persists, click "Refresh OAuth" to re-authorize

---

## Expected Timeline

| Step | Time | Status |
|------|------|--------|
| Fix Database | 2 min | ⏳ Pending |
| Verify UI | 2 min | ⏳ Pending |
| Azure AD Setup | 20 min | ⏳ Pending |
| Enter Credentials | 5 min | ⏳ Pending |
| OAuth Authorization | 10 min | ⏳ Pending |
| Test Email Polling | 5 min | ⏳ Pending |
| **Total** | **44 min** | |

---

## Success Criteria

You'll know everything is working when:

- ✅ Badge shows "OAuth 2.0 - Authorized" (green)
- ✅ "Poll Now" button works without errors
- ✅ Backend logs show successful IMAP authentication
- ✅ Test email creates a complaint in the system
- ✅ Complaint appears in Complaint List within 5 minutes
- ✅ Background polling runs every 5 minutes automatically

---

## Important Notes

1. **Save your Client Secret immediately** - Azure only shows it once!
2. **Keep credentials secure** - don't commit to version control
3. **Token refresh is automatic** - happens every 60 minutes
4. **Token lifespan: 60 days** - you'll need to re-authorize after expiry
5. **Production deployment** - update redirect URI to your production domain

---

## What's Next After This?

Once OAuth is working:

1. **Production Setup**:
   - Update redirect URI in Azure AD
   - Use Azure Key Vault for secrets
   - Configure production email address

2. **Additional Features**:
   - Set up automatic acknowledgement emails
   - Configure email threading
   - Add email attachment handling
   - Set up multiple email accounts (if needed)

3. **Testing**:
   - Test email-to-complaint creation
   - Test complaint-to-email replies
   - Test email threading
   - Test auto-acknowledgement

---

## Need Help?

**Documentation:**
- Full guide: `AZURE_AD_OAUTH_SETUP_GUIDE.md`
- Test reports: `OAUTH_*.md` files in project root
- Backend code: `ComplaintManagement.Infrastructure/Services/EmailOAuthService.cs`
- Frontend code: `complaint-system-angular/src/app/components/admin/email-ticketing-config/`

**Common Issues:**
- See "Troubleshooting" section in `AZURE_AD_OAUTH_SETUP_GUIDE.md`
- Check backend console for detailed error messages
- Check browser console (F12) for frontend errors

---

**Ready to start?** Begin with **Step 1: Fix Database** above! 🚀
