# Email Server Configuration Reference

**Quick Reference Guide for Email Ticketing System**
**Version:** 1.0  |  **Date:** November 13, 2025

---

## Table of Contents

1. [Office 365 (Microsoft 365)](#office-365-microsoft-365)
2. [Gmail (Google Workspace)](#gmail-google-workspace)
3. [Outlook.com (Personal)](#outlookcom-personal)
4. [Yahoo Mail](#yahoo-mail)
5. [GoDaddy Workspace](#godaddy-workspace)
6. [Custom IMAP/SMTP](#custom-imapsmtp)
7. [Troubleshooting](#troubleshooting)

---

## Office 365 (Microsoft 365)

### OAuth 2.0 Configuration (Recommended)

```yaml
Authentication Type: OAuth 2.0
IMAP Settings:
  Host: outlook.office365.com
  Port: 993
  Encryption: SSL/TLS

SMTP Settings:
  Host: smtp.office365.com
  Port: 587
  Encryption: STARTTLS

OAuth Requirements:
  - Client ID (Application ID)
  - Tenant ID (Directory ID)
  - Client Secret

Callback URL: http://localhost:5000/api/oauth/callback (or your domain)
```

### Setup Steps:
1. **Go to Azure Portal:** https://portal.azure.com
2. **Navigate to:** Azure Active Directory → App registrations
3. **Create New Registration:**
   - Name: `Complaint Management Email`
   - Account type: "Accounts in this organizational directory only"
   - Redirect URI: `http://localhost:5000/api/oauth/callback`
4. **Copy Application (client) ID** from Overview page
5. **Copy Directory (tenant) ID** from Overview page
6. **Create Client Secret:**
   - Go to "Certificates & secrets"
   - Click "+ New client secret"
   - Description: `Email Access Secret`
   - Expires: 24 months (recommended)
   - Copy the **Value** (not the Secret ID)
7. **Add API Permissions:**
   - Go to "API permissions"
   - Click "+ Add a permission"
   - Select "Microsoft Graph"
   - Select "Delegated permissions"
   - Add: `IMAP.AccessAsUser.All`
   - Add: `SMTP.Send`
   - Click "Grant admin consent" for your organization
8. **Save credentials in application**

### Notes:
- ✅ OAuth 2.0 is **required** for Office 365 (Basic Auth disabled as of Oct 2022)
- ✅ Supports multi-factor authentication
- ✅ Automatic token refresh
- ⚠️ Admin consent required for organizational email accounts

---

## Gmail (Google Workspace)

### OAuth 2.0 Configuration (Recommended)

```yaml
Authentication Type: OAuth 2.0
IMAP Settings:
  Host: imap.gmail.com
  Port: 993
  Encryption: SSL/TLS

SMTP Settings:
  Host: smtp.gmail.com
  Port: 587
  Encryption: STARTTLS

OAuth Requirements:
  - Client ID
  - Client Secret
  - (No Tenant ID needed)

Callback URL: http://localhost:5000/api/oauth/callback
```

### Setup Steps:
1. **Go to Google Cloud Console:** https://console.cloud.google.com
2. **Create New Project:**
   - Name: `Complaint Management System`
3. **Enable Gmail API:**
   - Go to "APIs & Services" → "Library"
   - Search for "Gmail API"
   - Click "Enable"
4. **Configure OAuth Consent Screen:**
   - Go to "APIs & Services" → "OAuth consent screen"
   - User Type: Internal (for Google Workspace) or External
   - App name: `Complaint Management`
   - User support email: Your email
   - Developer contact: Your email
   - Scopes: Add `https://mail.google.com/`
5. **Create OAuth Credentials:**
   - Go to "APIs & Services" → "Credentials"
   - Click "+ CREATE CREDENTIALS" → "OAuth client ID"
   - Application type: "Web application"
   - Name: `Complaint Management Email Client`
   - Authorized redirect URIs: `http://localhost:5000/api/oauth/callback`
   - Click "CREATE"
6. **Copy Client ID** and **Client Secret**
7. **Save credentials in application**

### Alternative: App Password (Basic Auth)

```yaml
Authentication Type: Basic
Username: your.email@gmail.com
Password: [16-character App Password]
```

**To Generate App Password:**
1. Go to Google Account settings
2. Security → 2-Step Verification (must be enabled)
3. App passwords → Select app: "Mail" → Select device: "Other"
4. Copy the 16-character password

### Notes:
- ✅ OAuth 2.0 recommended for security
- ✅ App Password works but requires 2FA enabled
- ⚠️ "Less secure app access" no longer available
- ⚠️ OAuth requires Google Cloud Console project setup

---

## Outlook.com (Personal)

### OAuth 2.0 Configuration

```yaml
Authentication Type: OAuth 2.0
IMAP Settings:
  Host: outlook.office365.com
  Port: 993
  Encryption: SSL/TLS

SMTP Settings:
  Host: smtp-mail.outlook.com
  Port: 587
  Encryption: STARTTLS

OAuth Requirements:
  - Client ID
  - Tenant ID (for personal: common)
  - Client Secret

Callback URL: http://localhost:5000/api/oauth/callback
```

### Setup Steps:
Similar to Office 365, but:
- Use **personal Microsoft account**
- Tenant ID: Use `common` or `consumers`
- Register app at: https://portal.azure.com

### Notes:
- ✅ OAuth 2.0 required (Basic Auth disabled)
- ✅ Works with personal Microsoft accounts
- ⚠️ Slightly different from Office 365 business accounts

---

## Yahoo Mail

### Basic Authentication with App Password

```yaml
Authentication Type: Basic
IMAP Settings:
  Host: imap.mail.yahoo.com
  Port: 993
  Encryption: SSL/TLS

SMTP Settings:
  Host: smtp.mail.yahoo.com
  Port: 587 or 465
  Encryption: STARTTLS (587) or SSL (465)

Credentials:
  Username: your.email@yahoo.com
  Password: [App Password]
```

### Setup Steps:
1. **Go to Yahoo Account Security:** https://login.yahoo.com/account/security
2. **Enable Two-Step Verification** (if not enabled)
3. **Generate App Password:**
   - Scroll to "App passwords"
   - Click "Generate app password"
   - Select "Other App"
   - Name: `Complaint Management System`
   - Click "Generate"
   - Copy the 16-character password
4. **Use App Password** (not your Yahoo password) in the email configuration

### Notes:
- ✅ App Password required (regular password doesn't work)
- ✅ Two-factor authentication must be enabled
- ⚠️ Yahoo doesn't support OAuth 2.0 for IMAP/SMTP
- ⚠️ Free Yahoo Mail has limitations on IMAP access

---

## GoDaddy Workspace

### Basic Authentication

```yaml
Authentication Type: Basic
IMAP Settings:
  Host: imap.secureserver.net
  Port: 993
  Encryption: SSL/TLS

SMTP Settings:
  Host: smtpout.secureserver.net
  Port: 465 (SSL) or 587 (TLS)
  Encryption: SSL (465) or STARTTLS (587)

Credentials:
  Username: your.email@yourdomain.com (full email address)
  Password: [Your GoDaddy email password]
```

### Setup Steps:
1. Ensure IMAP is enabled in GoDaddy Workspace settings
2. Use your full email address as username
3. Use your GoDaddy email password

### Notes:
- ✅ Basic authentication supported
- ✅ Works with custom domain emails
- ⚠️ IMAP must be enabled in Workspace settings
- ⚠️ Port 465 recommended for GoDaddy

---

## Custom IMAP/SMTP

### Basic Configuration Template

```yaml
Authentication Type: Basic
IMAP Settings:
  Host: [Your IMAP server]
  Port: 143 (non-SSL) or 993 (SSL)
  Encryption: None/STARTTLS (143) or SSL/TLS (993)

SMTP Settings:
  Host: [Your SMTP server]
  Port: 25/587 (TLS) or 465 (SSL)
  Encryption: STARTTLS (587) or SSL (465)

Credentials:
  Username: [Usually full email address]
  Password: [Your email password]
```

### Common Server Combinations:

#### cPanel/WHM Hosting
```
IMAP: mail.yourdomain.com:993 (SSL)
SMTP: mail.yourdomain.com:587 (TLS)
Username: your.email@yourdomain.com
```

#### Plesk Hosting
```
IMAP: mail.yourdomain.com:993 (SSL)
SMTP: mail.yourdomain.com:587 (TLS)
Username: your.email@yourdomain.com
```

#### Exchange Server (On-Premise)
```
IMAP: exchange.yourdomain.com:993 (SSL)
SMTP: exchange.yourdomain.com:587 (TLS)
Username: DOMAIN\username or email@domain.com
```

### Notes:
- ⚠️ Contact your email provider for exact settings
- ⚠️ Some servers use different ports
- ⚠️ Username format varies (email vs. username)
- ✅ Most modern servers support SSL/TLS

---

## Port Reference Table

| Protocol | Port | Encryption | Usage |
|----------|------|------------|-------|
| IMAP | 143 | None/STARTTLS | Unencrypted or upgrade to TLS |
| IMAP | 993 | SSL/TLS | Encrypted from start (recommended) |
| SMTP | 25 | None | Legacy, often blocked |
| SMTP | 587 | STARTTLS | Modern submission port (recommended) |
| SMTP | 465 | SSL/TLS | Legacy SSL port (still widely used) |
| POP3 | 110 | None/STARTTLS | Unencrypted (not recommended) |
| POP3 | 995 | SSL/TLS | Encrypted POP3 |

**Recommendation:** Always use encrypted connections (ports 993 for IMAP, 587 or 465 for SMTP)

---

## OAuth vs Basic Authentication

| Feature | OAuth 2.0 | Basic Auth |
|---------|-----------|------------|
| **Security** | ✅ High - No passwords stored | ⚠️ Lower - Passwords stored |
| **Token Refresh** | ✅ Automatic | ❌ Manual password update |
| **Revocation** | ✅ Easy - From provider settings | ⚠️ Must change password |
| **MFA Support** | ✅ Yes | ⚠️ Requires app passwords |
| **Setup Complexity** | ⚠️ More complex (Azure AD, Google Cloud) | ✅ Simple |
| **Office 365** | ✅ Required | ❌ Disabled |
| **Gmail** | ✅ Recommended | ⚠️ App Password only |
| **Yahoo** | ❌ Not available | ✅ App Password required |
| **Custom Servers** | ⚠️ Depends on server | ✅ Usually supported |

**Recommendation:** Use OAuth 2.0 whenever possible for better security.

---

## Troubleshooting

### Issue: Authentication Failed

**Symptoms:**
- "Invalid credentials" error
- "Authentication failed" message
- Connection times out

**Solutions:**
1. **Verify credentials:**
   - Username is correct format (email vs. username)
   - Password is correct (try logging in to webmail)
   - For OAuth: Client ID, Tenant ID, Secret are correct
2. **Check server settings:**
   - IMAP/SMTP host names are correct
   - Ports are correct
   - SSL/TLS settings match server requirements
3. **Provider-specific:**
   - **Office 365:** Ensure OAuth is set up, Basic Auth is disabled
   - **Gmail:** Use App Password if Basic Auth, or OAuth
   - **Yahoo:** Must use App Password, not regular password
4. **Firewall:**
   - Ensure ports 993 (IMAP) and 587/465 (SMTP) are not blocked
   - Check if corporate firewall blocks email ports

---

### Issue: Connection Timeout

**Symptoms:**
- Request hangs
- "Connection timed out" error

**Solutions:**
1. **Verify host is reachable:**
   ```bash
   ping imap.gmail.com
   telnet imap.gmail.com 993
   ```
2. **Check firewall rules:**
   - Allow outbound connections on IMAP/SMTP ports
   - Whitelist email server IPs if needed
3. **Try different ports:**
   - If 993 doesn't work, try 143 with STARTTLS
   - If 587 doesn't work, try 465
4. **Network issues:**
   - Test from different network
   - Check if ISP blocks email ports

---

### Issue: SSL/TLS Errors

**Symptoms:**
- "SSL handshake failed"
- "Certificate validation error"

**Solutions:**
1. **Verify SSL/TLS setting matches port:**
   - Port 993 (IMAP) → SSL/TLS enabled
   - Port 143 (IMAP) → SSL/TLS disabled or STARTTLS
   - Port 465 (SMTP) → SSL/TLS enabled
   - Port 587 (SMTP) → STARTTLS
2. **Certificate issues:**
   - Ensure server certificate is valid
   - Check if self-signed cert needs to be trusted
3. **Update libraries:**
   - Ensure SSL/TLS libraries are up to date

---

### Issue: OAuth Token Expired

**Symptoms:**
- "Token expired" error
- Emails stop being fetched
- Warning: "Token expires soon"

**Solutions:**
1. **Refresh token:**
   - Click "Refresh OAuth" button in configuration
   - Re-authorize the application
2. **Token expiry:**
   - OAuth tokens typically expire after 90 days
   - System should auto-refresh, but manual refresh may be needed
3. **Revoked access:**
   - Check if app access was revoked in provider settings
   - Re-authorize if needed

---

### Issue: Emails Not Being Fetched

**Symptoms:**
- Manual poll returns 0 emails
- No new complaints created

**Solutions:**
1. **Check IMAP folder:**
   - Verify folder name (usually "INBOX")
   - Case-sensitive on some servers
   - For Gmail: Use "INBOX" or "[Gmail]/All Mail"
2. **Verify there are unread emails:**
   - Check webmail for new messages
   - Ensure emails are in the configured folder
3. **Polling interval:**
   - Check when last poll occurred
   - Verify configuration is enabled
4. **Email filters:**
   - Ensure emails aren't filtered out
   - Check spam folder

---

## Testing Your Configuration

### Quick Test Checklist:

1. **Test IMAP Connection:**
   ```
   Click "Test IMAP" button in configuration
   Expected: Green success message
   ```

2. **Test SMTP Connection:**
   ```
   Click "Test SMTP" button in configuration
   Expected: Green success message
   ```

3. **Manual Poll:**
   ```
   Click "Poll Now" button
   Expected: X emails fetched, Y complaints created
   ```

4. **Send Test Email:**
   ```
   Send email to configured address
   Wait for polling interval (or click "Poll Now")
   Verify complaint is created
   ```

5. **Reply Test:**
   ```
   Reply to a complaint email
   Verify reply is sent via SMTP
   Check recipient receives reply
   ```

---

## Security Best Practices

1. **Use OAuth 2.0** whenever possible
2. **Never share** Client Secrets or passwords
3. **Rotate secrets** regularly (every 6-12 months)
4. **Use strong passwords** for Basic Auth
5. **Enable 2FA** on email accounts
6. **Monitor access logs** for suspicious activity
7. **Revoke unused** OAuth tokens/app passwords
8. **Use HTTPS** for callback URLs in production
9. **Encrypt database** where credentials are stored
10. **Limit permissions** to only what's needed (IMAP read, SMTP send)

---

## Quick Start Commands

### Test IMAP Connection (Telnet):
```bash
telnet imap.gmail.com 993
# Or with OpenSSL
openssl s_client -connect imap.gmail.com:993
```

### Test SMTP Connection:
```bash
telnet smtp.gmail.com 587
# Or with OpenSSL
openssl s_client -starttls smtp -connect smtp.gmail.com:587
```

### Check DNS Records:
```bash
nslookup imap.gmail.com
nslookup smtp.gmail.com
```

---

## Additional Resources

- **Office 365 OAuth:** https://learn.microsoft.com/en-us/exchange/client-developer/legacy-protocols/how-to-authenticate-an-imap-pop-smtp-application-by-using-oauth
- **Gmail OAuth:** https://developers.google.com/gmail/imap/imap-smtp
- **IMAP RFC:** https://tools.ietf.org/html/rfc3501
- **SMTP RFC:** https://tools.ietf.org/html/rfc5321
- **OAuth 2.0 RFC:** https://tools.ietf.org/html/rfc6749

---

**Document Version:** 1.0
**Last Updated:** November 13, 2025
**Maintained By:** Development Team
