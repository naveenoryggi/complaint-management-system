# 10-Minute OAuth Setup Guide

**Fastest path to get OAuth email ticketing working**

---

## ⏱️ Timeline

```
[0:00-0:02] Fix Database        ████
[0:02-0:04] Verify UI            ████
[0:04-0:24] Azure AD Setup       ████████████████████████
[0:24-0:29] Enter Credentials    ██████
[0:29-0:39] Authorize OAuth      ████████████
[0:39-0:44] Test Email           ██████
─────────────────────────────────────────────
TOTAL: 44 minutes (not 10, sorry for the confusion!)
```

**Actual 10-minute version:** Use App Password instead (see bottom)

---

## 🚀 Full OAuth Setup (44 minutes)

### Step 1: Fix Database (2 min) ⚠️

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to `PRANA-ASUS\SQLEXPRESS`
3. Open file: **`QUICK_FIX_OAUTH.sql`**
4. Press **F5**
5. Verify output: "Fix Applied Successfully!"

**Why:** Fixes invalid AuthenticationType value in database

---

### Step 2: Verify UI (2 min)

1. Open http://localhost:4200
2. Login: `admin@complaintmanagement.com` / `Admin@123`
3. Navigate: **Admin Panel** → **Communication Settings** → **Email Ticketing**
4. Find "Oryggi Tech Support" row

**Expected:** Badge shows 🟠 "OAuth 2.0 - Pending" (orange, pulsing)

**If not:** Hard refresh browser (Ctrl+Shift+R)

---

### Step 3: Azure AD App Registration (20 min)

#### 3.1 Create App (5 min)

1. Go to: https://portal.azure.com
2. Navigate to: **Microsoft Entra ID** (formerly Azure AD)
3. Click: **App registrations** → **New registration**

**Fill in:**
- **Name:** `Complaint Management Email OAuth`
- **Supported account types:** Single tenant
- **Redirect URI:**
  - Type: **Web**
  - URL: `http://localhost:5000/api/oauth/callback`
- Click **Register**

**Save these immediately:**
- ✏️ **Application (client) ID:** `_______________________`
- ✏️ **Directory (tenant) ID:** `_______________________`

#### 3.2 Create Client Secret (2 min)

1. In your app, click: **Certificates & secrets**
2. Click: **New client secret**
3. Description: `Email OAuth Secret`
4. Expires: **12 months** (or 24 months)
5. Click **Add**
6. **⚠️ COPY THE SECRET VALUE IMMEDIATELY** (you can't see it again!)

**Save this:**
- ✏️ **Client Secret Value:** `_______________________`

#### 3.3 Configure API Permissions (10 min)

1. Click: **API permissions**
2. Click: **Add a permission**
3. Select: **Microsoft Graph**
4. Select: **Delegated permissions**

**Add these 6 permissions:**
- ☑️ `User.Read` (may already be there)
- ☑️ `IMAP.AccessAsUser.All`
- ☑️ `SMTP.Send`
- ☑️ `Mail.Read`
- ☑️ `Mail.ReadWrite`
- ☑️ `offline_access`

5. Click **Add permissions**
6. Click **Grant admin consent for [Your Organization]**
7. Click **Yes** to confirm

**Verify:** All 6 permissions show green checkmarks

#### 3.4 Add Redirect URI for Production (3 min)

1. Click: **Authentication**
2. Under **Web**, click **Add URI**
3. Add: `https://your-production-domain.com/api/oauth/callback`
4. Click **Save**

---

### Step 4: Enter Credentials in UI (5 min)

1. Back in application: http://localhost:4200/admin/email-ticketing-config
2. Find "Oryggi Tech Support" row
3. Click **Edit** (pencil icon)
4. The wizard opens - navigate through:

**Step 1: Basic Info**
- Keep everything as is
- Click **Next**

**Step 2: IMAP Settings**
- Keep everything as is
- Click **Next**

**Step 3: SMTP Settings**
- Keep everything as is
- Click **Next**

**Step 4: OAuth Credentials** ⚠️ **IMPORTANT**
- Select **Authentication Method:** OAuth 2.0
- **OAuth Provider:** Microsoft (Office 365)
- **Client ID:** Paste from Step 3.1
- **Tenant ID:** Paste from Step 3.1
- **Client Secret:** Paste from Step 3.2
- Click **Next**

**Step 5: Auto-Acknowledgement**
- Keep defaults or configure as needed
- Click **Save Configuration**

**Expected:** Badge changes to 🟠 "OAuth 2.0 - Pending" (pulsing) with "Authorize Now" button

---

### Step 5: Authorize OAuth (10 min)

1. Click **"Authorize Now"** button (blue, next to badge)
2. Browser redirects to Microsoft login page

**Login Process:**
1. Enter: `marketing@oryggitech.com`
2. Enter password for that account
3. Accept permissions (shows list of 6 permissions)
4. Click **Accept**

**Expected:**
- Browser redirects back to http://localhost:5000/api/oauth/callback
- Shows success message
- Redirects to http://localhost:4200
- Badge changes to 🟢 "OAuth 2.0 - Authorized" (green)
- Button changes to "Refresh OAuth" (blue)

**If error:**
- Check redirect URI in Azure AD matches exactly: `http://localhost:5000/api/oauth/callback`
- Ensure all 6 permissions are granted with admin consent

---

### Step 6: Test Email Polling (5 min)

#### Manual Test:
1. Click **"Poll Now"** button
2. Wait 5-10 seconds
3. Check **Complaints List** for new complaints

#### Send Test Email:
1. Send email TO: `marketing@oryggitech.com`
2. Subject: `Test Complaint from Email`
3. Body: `This is a test complaint created via email ticketing`
4. Wait 5 minutes OR click "Poll Now"
5. Check Complaints List

**Expected:** New complaint created from email

---

## 🎯 Success Checklist

- [ ] SQL script executed successfully
- [ ] UI shows OAuth badge (not "Basic Auth")
- [ ] Azure AD app created
- [ ] Client ID, Tenant ID, Secret saved
- [ ] 6 API permissions granted with admin consent
- [ ] Credentials entered in UI
- [ ] "Authorize Now" clicked
- [ ] Microsoft login completed
- [ ] Badge shows 🟢 "OAuth 2.0 - Authorized"
- [ ] "Poll Now" works without errors
- [ ] Test email creates complaint

---

## ⚡ True 10-Minute Option: Use App Password Instead

If you want OAuth working in **actual 10 minutes**, use this alternative:

### App Password Method (10 min total)

#### Step 1: Fix Database (2 min)
Same as above - run `QUICK_FIX_OAUTH.sql`

#### Step 2: Change to Basic Auth (2 min)
1. In SQL, run:
```sql
UPDATE EmailConfigurations
SET AuthenticationType = 0  -- Basic Auth instead of OAuth
WHERE Id = '4A1B41EF-CBC5-4858-A6A5-02B1C147A80A';
```

#### Step 3: Generate App Password (3 min)
1. Go to: https://account.microsoft.com/security
2. Sign in: `marketing@oryggitech.com`
3. Click: **App passwords**
4. Click: **Create app password**
5. Name: `Complaint Management System`
6. **COPY THE 16-CHARACTER PASSWORD**

#### Step 4: Enter in UI (2 min)
1. Go to: http://localhost:4200/admin/email-ticketing-config
2. Edit "Oryggi Tech Support"
3. Set **Authentication Method:** Basic Auth
4. **IMAP Password:** Paste app password
5. **SMTP Password:** Paste app password
6. Save

#### Step 5: Test (1 min)
1. Click "Test IMAP" → Should succeed ✅
2. Click "Test SMTP" → Should succeed ✅
3. Click "Poll Now" → Should work ✅

**Total: 10 minutes ⏱️**

---

## 📊 Comparison

| Feature | OAuth 2.0 | App Password |
|---------|-----------|--------------|
| **Setup Time** | 44 min | 10 min |
| **Security** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Auto Refresh** | Yes ✅ | No ❌ |
| **Azure AD Required** | Yes | No |
| **Production Ready** | Yes ✅ | Yes ✅ |
| **Complexity** | High | Low |

**Recommendation:**
- **Development/Testing:** Use App Password (10 min)
- **Production:** Use OAuth 2.0 (44 min)

---

## 🆘 Common Issues

### "redirect_uri_mismatch"
**Fix:** Ensure Azure AD redirect URI is exactly `http://localhost:5000/api/oauth/callback`

### Badge still shows "Basic Auth"
**Fix:** Run SQL fix, then hard refresh browser (Ctrl+Shift+R)

### "Invalid client secret"
**Fix:** Secret may have spaces - paste carefully without extra characters

### "AADSTS50011: The reply URL specified in the request does not match"
**Fix:** Check Authentication settings in Azure AD, add the exact callback URL

### Backend logs show "No cached account found"
**Fix:** This is normal BEFORE authorization - click "Authorize Now"

---

**Generated:** November 13, 2025
**Status:** Complete setup guide
**Actual Time:** 44 min (OAuth) or 10 min (App Password)
