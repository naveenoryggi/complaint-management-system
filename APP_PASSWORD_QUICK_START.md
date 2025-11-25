# 🚀 App Password Quick Start Guide

## Generate Your Office 365 App Password (5 Minutes)

### Step 1: Enable Two-Factor Authentication (If Not Already Enabled)
1. Go to: https://account.microsoft.com/security
2. Sign in with: **marketing@oryggitech.com**
3. Click **Advanced security options**
4. Under **Two-step verification**, ensure it's **ON**
   - If OFF, click "Set up two-step verification" and follow the prompts

### Step 2: Generate App Password
1. Go to: https://account.microsoft.com/security/apppasswords
2. Sign in if prompted
3. Click **Create a new app password**
4. Name it: **Complaint Management Email Ticketing**
5. **IMPORTANT**: Copy the 16-character password shown
   - Format: `abcd-efgh-ijkl-mnop` (with dashes)
   - You can only see this once!

### Step 3: Provide the Password to Me
Once you have the app password, simply paste it here and I'll configure the system using Playwright.

---

## ✅ What Happens Next

Once you provide the app password, I will:
1. Use Playwright to navigate to the Email Configuration page
2. Edit the "Oryggi Tech Support" configuration
3. Update both IMAP and SMTP passwords with your app password
4. Save the configuration
5. Test IMAP connection
6. Test SMTP connection
7. Verify email polling is ready

---

## 📧 Email Configuration Details

**Account**: marketing@oryggitech.com
**IMAP Server**: outlook.office365.com:993 (SSL)
**SMTP Server**: outlook.office365.com:587 (TLS)

---

**Ready?** Just paste your 16-character app password below and I'll handle the rest!
