# Office 365 Email Ticketing Setup Guide

## Overview

This guide provides **TWO approaches** to enable Office 365 email ticketing:

1. **🚀 Quick Start: App Passwords** (5 minutes) - Immediate solution
2. **🔐 Production: OAuth 2.0** (Complete implementation roadmap)

---

## ✅ Current Implementation Status

### Completed
- ✅ OAuth 2.0 database schema added to `EmailConfiguration` entity
- ✅ Database migration applied successfully
- ✅ Frontend UI for email configuration (with provider presets)
- ✅ Backend API for CRUD operations
- ✅ Connection testing functionality (IMAP/SMTP)
- ✅ Email polling service infrastructure

### Required for OAuth 2.0
- ⏳ Azure AD App Registration (manual step)
- ⏳ OAuth token acquisition flow
- ⏳ OAuth token refresh service
- ⏳ IMAP/SMTP OAuth authentication implementation

---

## 🚀 Approach 1: App Passwords (Recommended for Testing)

### Why Use App Passwords?
- ✅ Works with existing code immediately
- ✅ No code changes required
- ✅ Perfect for testing and development
- ✅ Simpler to set up

### Prerequisites
- Office 365 account with admin access
- Multi-Factor Authentication (MFA) must be enabled

### Step-by-Step Instructions

#### Step 1: Enable Multi-Factor Authentication
1. Go to [Microsoft Account Security](https://account.microsoft.com/security)
2. Sign in with `marketing@oryggitech.com`
3. Navigate to **Security** > **Advanced security options**
4. Enable **Two-step verification** if not already enabled

#### Step 2: Generate App Password
1. Go to [App Passwords Page](https://account.microsoft.com/security/apppasswords)
2. Click **Create a new app password**
3. Enter name: `Complaint Management Email Ticketing`
4. Copy the generated 16-character password (e.g., `abcd-efgh-ijkl-mnop`)

#### Step 3: Update Email Configuration
1. Navigate to: **http://localhost:4200/admin/email-ticketing-config**
2. Click **Edit** on the Oryggi Tech Support configuration
3. Replace the password fields:
   - **IMAP Password**: Paste the 16-character app password
   - **SMTP Password**: Paste the same 16-character app password
4. Click **Save Configuration**

#### Step 4: Test Connections
1. Click **Test IMAP** - Should show success ✅
2. Click **Test SMTP** - Should show success ✅
3. Click **Poll Now** to test email retrieval

###Expected Result
```
✅ IMAP: Connection successful
✅ SMTP: Connection successful
✅ Ready to poll for emails and create tickets
```

---

## 🔐 Approach 2: OAuth 2.0 Implementation (Production-Grade)

### Architecture Overview

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│   Angular   │────>│  ASP.NET     │────>│  Azure AD   │
│   Frontend  │<────│  Backend     │<────│  (OAuth)    │
└─────────────┘     └──────────────┘     └─────────────┘
                           │
                           ▼
                    ┌──────────────┐
                    │   Office365  │
                    │ IMAP/SMTP    │
                    └──────────────┘
```

### Phase 1: Azure AD App Registration (Manual - 15 minutes)

#### 1.1 Create App Registration
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Configure:
   - **Name**: `Complaint Management Email Integration`
   - **Supported account types**: `Accounts in this organizational directory only`
   - **Redirect URI**:
     - Type: `Web`
     - URI: `http://localhost:5000/api/oauth/callback` (dev)
     - URI: `https://yourapp.com/api/oauth/callback` (production)
5. Click **Register**

#### 1.2 Configure API Permissions
1. In the app registration, go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Choose **Delegated permissions**
5. Add these permissions:
   ```
   - Mail.Read
   - Mail.ReadWrite
   - Mail.Send
   - IMAP.AccessAsUser.All
   - SMTP.Send
   ```
6. Click **Add permissions**
7. Click **Grant admin consent** (requires admin)

#### 1.3 Create Client Secret
1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Description: `Email Ticketing Secret`
4. Expiry: `24 months`
5. Click **Add**
6. **IMPORTANT**: Copy the secret value immediately (it won't be shown again)

#### 1.4 Note Configuration Values
Copy these values for later use:
```
Application (client) ID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
Directory (tenant) ID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
Client secret value: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

### Phase 2: Backend Implementation

#### 2.1 Install Required NuGet Packages
```bash
cd complaint-system-dotnet/src/ComplaintManagement.Infrastructure
dotnet add package Microsoft.Identity.Client
dotnet add package Azure.Identity
```

#### 2.2 Create OAuth Token Service
Create `EmailOAuthService.cs`:
```csharp
using Microsoft.Identity.Client;

namespace ComplaintManagement.Infrastructure.Services;

public class EmailOAuthService
{
    private readonly IConfidentialClientApplication _app;

    public EmailOAuthService(IConfiguration configuration)
    {
        _app = ConfidentialClientApplicationBuilder
            .Create(configuration["AzureAd:ClientId"])
            .WithClientSecret(configuration["AzureAd:ClientSecret"])
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{configuration["AzureAd:TenantId"]}"))
            .Build();
    }

    public async Task<string> GetAccessTokenAsync(string refreshToken)
    {
        var scopes = new[] {
            "https://outlook.office365.com/IMAP.AccessAsUser.All",
            "https://outlook.office365.com/SMTP.Send"
        };

        try
        {
            var result = await _app.AcquireTokenByRefreshToken(
                scopes,
                refreshToken
            ).ExecuteAsync();

            return result.AccessToken;
        }
        catch (MsalException ex)
        {
            // Token expired - need to re-authenticate
            throw new UnauthorizedAccessException("OAuth token expired", ex);
        }
    }

    public string GetAuthorizationUrl(string state)
    {
        var scopes = new[] {
            "https://outlook.office365.com/IMAP.AccessAsUser.All",
            "https://outlook.office365.com/SMTP.Send",
            "offline_access"
        };

        var authUrl = _app.GetAuthorizationRequestUrl(scopes)
            .WithRedirectUri("http://localhost:5000/api/oauth/callback")
            .WithExtraQueryParameters($"state={state}")
            .ExecuteAsync()
            .Result;

        return authUrl.ToString();
    }

    public async Task<AuthenticationResult> ExchangeCodeForTokenAsync(string code)
    {
        var scopes = new[] {
            "https://outlook.office365.com/IMAP.AccessAsUser.All",
            "https://outlook.office365.com/SMTP.Send"
        };

        var result = await _app.AcquireTokenByAuthorizationCode(scopes, code)
            .WithRedirectUri("http://localhost:5000/api/oauth/callback")
            .ExecuteAsync();

        return result;
    }
}
```

#### 2.3 Create OAuth Callback Controller
Create `OAuthCallbackController.cs`:
```csharp
[ApiController]
[Route("api/oauth")]
public class OAuthCallbackController : ControllerBase
{
    private readonly EmailOAuthService _oauthService;
    private readonly IUnitOfWork _unitOfWork;

    public OAuthCallbackController(EmailOAuthService oauthService, IUnitOfWork unitOfWork)
    {
        _oauthService = oauthService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
    {
        try
        {
            // Exchange authorization code for tokens
            var tokenResult = await _oauthService.ExchangeCodeForTokenAsync(code);

            // Parse state to get email config ID
            var configId = Guid.Parse(state);

            // Update email configuration with tokens
            var config = await _unitOfWork.Repository<EmailConfiguration>()
                .GetByIdAsync(configId);

            if (config == null)
                return NotFound();

            config.AuthenticationType = EmailAuthenticationType.OAuth2;
            config.OAuthAccessToken = tokenResult.AccessToken;
            config.OAuthRefreshToken = tokenResult.RefreshToken;
            config.OAuthTokenExpiresAt = tokenResult.ExpiresOn.UtcDateTime;

            await _unitOfWork.SaveChangesAsync();

            // Redirect back to Angular with success
            return Redirect($"http://localhost:4200/admin/email-ticketing-config?oauth=success");
        }
        catch (Exception ex)
        {
            return Redirect($"http://localhost:4200/admin/email-ticketing-config?oauth=error&message={ex.Message}");
        }
    }

    [HttpGet("authorize/{configId}")]
    public IActionResult InitiateOAuth(Guid configId)
    {
        var authUrl = _oauthService.GetAuthorizationUrl(configId.ToString());
        return Redirect(authUrl);
    }
}
```

#### 2.4 Update EmailTicketingService for OAuth

Modify `EmailTicketingService.cs` to support OAuth authentication:
```csharp
private async Task<ImapClient> GetImapClientAsync(EmailConfiguration config)
{
    var client = new ImapClient();
    await client.ConnectAsync(config.ImapHost, config.ImapPort, config.ImapUseSsl);

    if (config.AuthenticationType == EmailAuthenticationType.OAuth2)
    {
        // Check if token needs refresh
        if (config.OAuthTokenExpiresAt <= DateTime.UtcNow.AddMinutes(5))
        {
            var newToken = await _oauthService.GetAccessTokenAsync(config.OAuthRefreshToken);
            config.OAuthAccessToken = newToken;
            await _unitOfWork.SaveChangesAsync();
        }

        // Authenticate with OAuth token
        var oauth2 = new SaslMechanismOAuth2(config.ImapUsername, config.OAuthAccessToken);
        await client.AuthenticateAsync(oauth2);
    }
    else
    {
        // Basic authentication
        await client.AuthenticateAsync(config.ImapUsername, config.ImapPassword);
    }

    return client;
}
```

### Phase 3: Angular Frontend Implementation

#### 3.1 Add OAuth Button to Email Config Component
Update `email-ticketing-config.component.html`:
```html
<button *ngIf="config.authenticationType === 'OAuth2'"
        (click)="authorizeOAuth(config.id)"
        class="btn btn-primary">
  <i class="fas fa-key"></i> Authorize with Microsoft
</button>
```

#### 3.2 Implement OAuth Flow
Update `email-ticketing-config.component.ts`:
```typescript
authorizeOAuth(configId: string): void {
  // Open OAuth authorization in new window
  window.location.href = `http://localhost:5000/api/oauth/authorize/${configId}`;
}

ngOnInit(): void {
  // Check for OAuth callback
  this.route.queryParams.subscribe(params => {
    if (params['oauth'] === 'success') {
      this.showSuccess('OAuth authorization successful!');
      this.loadConfigurations();
    } else if (params['oauth'] === 'error') {
      this.showError(`OAuth error: ${params['message']}`);
    }
  });
}
```

### Phase 4: Configuration

#### 4.1 Update appsettings.json
```json
{
  "AzureAd": {
    "ClientId": "YOUR_CLIENT_ID_FROM_STEP_1.4",
    "ClientSecret": "YOUR_CLIENT_SECRET_FROM_STEP_1.4",
    "TenantId": "YOUR_TENANT_ID_FROM_STEP_1.4"
  }
}
```

#### 4.2 Register Services in DependencyInjection.cs
```csharp
services.AddSingleton<EmailOAuthService>();
```

### Phase 5: Testing OAuth Flow

1. Navigate to Email Ticketing Configuration
2. Create/edit configuration and select **OAuth2** as authentication type
3. Click **Authorize with Microsoft**
4. Sign in with Office 365 account
5. Grant permissions
6. Redirected back to app with tokens saved
7. Test IMAP/SMTP connections
8. Verify email polling works

---

## 📊 Comparison: App Password vs OAuth 2.0

| Feature | App Password | OAuth 2.0 |
|---------|-------------|-----------|
| **Setup Time** | 5 minutes | 2-3 hours |
| **Code Changes** | None | Significant |
| **Security** | Good | Excellent |
| **Token Refresh** | N/A (no expiry) | Automatic |
| **User Experience** | Simple | Seamless |
| **Admin Consent** | Not required | Required once |
| **Production Ready** | ✅ Yes | ✅ Yes (after implementation) |
| **Best For** | Testing, small deployments | Enterprise, large scale |

---

## 🎯 Recommended Approach

### For Immediate Testing
**Use App Passwords** - Get the system working today and validate the email ticketing functionality.

### For Production Deployment
**Implement OAuth 2.0** - Follow Phase 1-5 above for enterprise-grade security and seamless user experience.

### Hybrid Approach
1. **Week 1**: Deploy with App Passwords to validate functionality
2. **Week 2-3**: Implement OAuth 2.0 in parallel
3. **Week 4**: Migrate existing configurations to OAuth 2.0
4. **Result**: Zero downtime, validated system, production-ready

---

## 🐛 Troubleshooting

### App Password Issues
- **"LOGIN failed"**: Verify 2FA is enabled on the account
- **"Invalid credentials"**: Regenerate app password and update configuration
- **"Account locked"**: Check Microsoft security alerts

### OAuth 2.0 Issues
- **"Redirect URI mismatch"**: Verify callback URL matches Azure AD registration
- **"Insufficient permissions"**: Check admin consent was granted
- **"Token expired"**: Implement token refresh logic properly
- **"AADSTS error codes"**: Search Microsoft docs for specific error code

---

## 📚 Additional Resources

- [Microsoft Graph Mail API](https://docs.microsoft.com/en-us/graph/api/resources/mail-api-overview)
- [OAuth 2.0 Authorization Code Flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow)
- [MailKit OAuth 2.0 Examples](https://github.com/jstedfast/MailKit/blob/master/FAQ.md#gmail-application-specific-passwords)
- [Azure AD App Registration Guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)

---

## ✅ Next Steps

### Immediate (App Password Route)
1. Generate app password for `marketing@oryggitech.com`
2. Update email configuration with app password
3. Test IMAP/SMTP connections
4. Poll for emails and create test tickets

### Long-term (OAuth 2.0 Route)
1. Complete Phase 1: Azure AD App Registration
2. Implement Phase 2: Backend OAuth Service
3. Build Phase 3: Angular OAuth Flow
4. Configure Phase 4: Application Settings
5. Test Phase 5: End-to-end OAuth Flow

---

**Last Updated**: November 11, 2025
**Status**: Database schema ready ✅ | OAuth infrastructure prepared ✅
**Current Auth Method**: Basic (ready for App Password)
**Future Auth Method**: OAuth 2.0 (implementation roadmap provided)
