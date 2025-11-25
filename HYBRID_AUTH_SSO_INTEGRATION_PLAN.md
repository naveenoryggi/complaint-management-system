# Hybrid Authentication & SSO Integration Plan
## Active Directory, SSO, and Third-Party Authentication Support

**Date:** November 9, 2025
**Feature:** Hybrid Authentication System with AD/SSO Integration
**Status:** Design & Planning Phase

---

## 📋 Table of Contents

1. [Overview](#1-overview)
2. [Authentication Scenarios](#2-authentication-scenarios)
3. [Architecture Design](#3-architecture-design)
4. [Database Schema Updates](#4-database-schema-updates)
5. [Authentication Provider Abstraction](#5-authentication-provider-abstraction)
6. [Active Directory Integration](#6-active-directory-integration)
7. [SSO Integration (SAML/OAuth/OIDC)](#7-sso-integration)
8. [Third-Party Authentication](#8-third-party-authentication)
9. [User Provisioning & Sync](#9-user-provisioning--sync)
10. [Security Considerations](#10-security-considerations)
11. [Implementation Plan](#11-implementation-plan)
12. [Testing Strategy](#12-testing-strategy)

---

## 1. Overview

### 1.1 Business Requirements

The complaint management system must support **multiple authentication methods**:

1. **Local Authentication** (existing)
   - Email/Password
   - Employee Code/Password
   - Phone/Password

2. **Active Directory Authentication** (NEW)
   - Windows Integrated Authentication
   - LDAP authentication
   - AD user sync

3. **Single Sign-On (SSO)** (NEW)
   - SAML 2.0
   - OAuth 2.0 / OpenID Connect (OIDC)
   - Azure AD / Entra ID
   - Google Workspace
   - Okta, Auth0, etc.

4. **Third-Party Authentication** (NEW)
   - Custom authentication APIs
   - Existing HRMS systems (like Oryggi)
   - External identity providers

### 1.2 Key Design Principles

✅ **Provider Agnostic:** Support multiple authentication providers simultaneously
✅ **Graceful Fallback:** If AD/SSO unavailable, fall back to local auth
✅ **User Choice:** Users can choose authentication method at login
✅ **Admin Control:** Admins can configure which providers are enabled
✅ **Just-in-Time Provisioning:** Auto-create users from AD/SSO on first login
✅ **Attribute Mapping:** Map AD/SSO attributes to local user properties
✅ **Audit Trail:** Log all authentication attempts regardless of provider

### 1.3 Microsoft Teams Comparison

**How Microsoft Teams Handles This:**
- Primary authentication via Azure AD (SSO)
- Supports guest users with different identity providers
- Seamless switching between work/school and personal accounts
- Just-in-time user provisioning
- Automatic attribute sync from Azure AD
- Single logout across all connected apps

---

## 2. Authentication Scenarios

### 2.1 Scenario Matrix

| User Type | Primary Auth | Password Management | SSO Support |
|-----------|--------------|---------------------|-------------|
| **Employee (AD User)** | Active Directory | Managed in AD | ✅ Yes |
| **Employee (Local)** | Local Database | Self-managed in app | ⚠️ Optional |
| **External User** | Third-Party SSO | External provider | ✅ Yes |
| **Contractor** | Local or SSO | Depends on config | ⚠️ Optional |
| **Admin** | Any method | Depends on source | ✅ Yes |

### 2.2 Authentication Flow Examples

#### **Scenario 1: AD Employee First Login**
```
1. User visits login page
2. Enters AD username (e.g., john.doe@company.com)
3. System detects AD domain → redirects to AD authentication
4. User authenticates against Active Directory
5. AD returns success + user attributes
6. System checks if user exists in local DB:
   - If NO: Create new user (JIT provisioning)
   - If YES: Update user attributes from AD
7. Generate JWT token
8. User logged in → redirected to dashboard
9. No password managed in local system
```

#### **Scenario 2: SSO User (Azure AD/SAML)**
```
1. User visits login page
2. Clicks "Sign in with Microsoft" button
3. Redirected to Azure AD login page
4. User authenticates with Microsoft account
5. Azure AD redirects back with SAML assertion/OAuth token
6. System validates SAML assertion
7. Extracts user claims (email, name, groups, etc.)
8. JIT provisioning: Create/update user in local DB
9. Map AD groups to local roles
10. Generate JWT token
11. User logged in
```

#### **Scenario 3: Third-Party HRMS Authentication**
```
1. User enters employee code
2. System detects employee is from Oryggi HRMS
3. Makes API call to Oryggi authentication endpoint
4. Oryggi validates credentials and returns user data
5. System syncs user data to local DB
6. Generate JWT token
7. User logged in
```

#### **Scenario 4: Hybrid User (Fallback)**
```
1. User tries AD authentication → AD server is down
2. System detects AD unavailable
3. Shows fallback option: "Use local password"
4. User enters local password (if configured)
5. Authenticates against local database
6. User logged in with limited features
7. Audit log records fallback authentication
```

---

## 3. Architecture Design

### 3.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Frontend (Angular)                      │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │ Login Page  │  │ SSO Redirect │  │ Profile Settings │   │
│  └─────────────┘  └──────────────┘  └──────────────────┘   │
└──────────────┬──────────────┬──────────────┬───────────────┘
               │              │              │
               ▼              ▼              ▼
┌─────────────────────────────────────────────────────────────┐
│                  API Gateway / Middleware                   │
│              (Authentication Orchestrator)                   │
└──────────────┬──────────────┬──────────────┬───────────────┘
               │              │              │
      ┌────────▼─────┐ ┌─────▼──────┐ ┌────▼──────────┐
      │ Local Auth   │ │  AD Auth   │ │  SSO Auth     │
      │ Provider     │ │  Provider  │ │  Provider     │
      └──────────────┘ └────────────┘ └───────────────┘
               │              │              │
               └──────────────┴──────────────┘
                              │
                              ▼
                    ┌─────────────────────┐
                    │   User Repository   │
                    │   (Database)        │
                    └─────────────────────┘
```

### 3.2 Provider Pattern

**Interface-based design:**
```csharp
public interface IAuthenticationProvider
{
    string ProviderName { get; }
    AuthenticationProviderType ProviderType { get; }

    Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken);

    Task<UserInfo> GetUserInfoAsync(
        string externalUserId,
        CancellationToken cancellationToken);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);

    Task<bool> ValidateConfigurationAsync(CancellationToken cancellationToken);
}

public enum AuthenticationProviderType
{
    Local,
    ActiveDirectory,
    AzureAD,
    SAML,
    OAuth,
    OIDC,
    Custom
}
```

### 3.3 Authentication Orchestrator

**Centralized authentication logic:**
```csharp
public class AuthenticationOrchestrator
{
    private readonly IEnumerable<IAuthenticationProvider> _providers;
    private readonly IUserProvisioningService _provisioning;

    public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
    {
        // 1. Determine authentication provider
        var provider = await DetermineProviderAsync(request);

        // 2. Authenticate with provider
        var authResult = await provider.AuthenticateAsync(request);

        if (!authResult.IsSuccess)
            return LoginResponse.Failed(authResult.ErrorMessage);

        // 3. Get/Create user (JIT provisioning)
        var user = await _provisioning.GetOrCreateUserAsync(authResult);

        // 4. Sync user attributes
        await _provisioning.SyncUserAttributesAsync(user, authResult.UserInfo);

        // 5. Map roles and permissions
        await _provisioning.SyncRolesAsync(user, authResult.Groups);

        // 6. Generate JWT token
        var token = _jwtService.GenerateToken(user);

        // 7. Audit log
        await _auditService.LogAuthenticationAsync(user, provider, authResult);

        return LoginResponse.Success(token, user);
    }
}
```

---

## 4. Database Schema Updates

### 4.1 Enhanced User Table

**Add authentication-related columns:**

```sql
ALTER TABLE Users ADD COLUMN AuthenticationProvider NVARCHAR(50) NULL DEFAULT 'Local';
ALTER TABLE Users ADD COLUMN ExternalUserId NVARCHAR(500) NULL; -- AD objectId, AAD oid, etc.
ALTER TABLE Users ADD COLUMN ExternalUsername NVARCHAR(200) NULL; -- AD sAMAccountName, UPN
ALTER TABLE Users ADD COLUMN IdentityProvider NVARCHAR(100) NULL; -- Azure AD tenant, AD domain
ALTER TABLE Users ADD COLUMN LastExternalSync DATETIME NULL;
ALTER TABLE Users ADD COLUMN ExternalSyncEnabled BIT DEFAULT 0;
ALTER TABLE Users ADD COLUMN SSOEnabled BIT DEFAULT 1;
ALTER TABLE Users ADD COLUMN LocalPasswordEnabled BIT DEFAULT 1; -- Allow local password as fallback
ALTER TABLE Users ADD COLUMN PreferredAuthMethod NVARCHAR(50) NULL; -- User's preferred method
```

**Updated User entity:**
```csharp
public class User : BaseEntity
{
    // ... existing properties

    // AUTHENTICATION PROPERTIES

    /// <summary>
    /// Authentication provider used (Local, AD, AzureAD, SAML, etc.)
    /// </summary>
    public AuthenticationProviderType AuthenticationProvider { get; set; } = AuthenticationProviderType.Local;

    /// <summary>
    /// External user ID from identity provider (AD objectGuid, Azure AD oid, etc.)
    /// </summary>
    public string? ExternalUserId { get; set; }

    /// <summary>
    /// External username (AD sAMAccountName, UPN, etc.)
    /// </summary>
    public string? ExternalUsername { get; set; }

    /// <summary>
    /// Identity provider identifier (AD domain, Azure AD tenant ID, etc.)
    /// </summary>
    public string? IdentityProvider { get; set; }

    /// <summary>
    /// When user attributes were last synced from external provider
    /// </summary>
    public DateTime? LastExternalSync { get; set; }

    /// <summary>
    /// Enable automatic sync from external provider
    /// </summary>
    public bool ExternalSyncEnabled { get; set; } = false;

    /// <summary>
    /// Enable SSO for this user
    /// </summary>
    public bool SSOEnabled { get; set; } = true;

    /// <summary>
    /// Allow local password authentication as fallback
    /// </summary>
    public bool LocalPasswordEnabled { get; set; } = true;

    /// <summary>
    /// User's preferred authentication method
    /// </summary>
    public string? PreferredAuthMethod { get; set; }
}

public enum AuthenticationProviderType
{
    Local,
    ActiveDirectory,
    AzureAD,
    SAML,
    OAuth,
    OIDC,
    Oryggi,
    Custom
}
```

### 4.2 New Table: AuthenticationProviders

**Store provider configurations:**

```sql
CREATE TABLE AuthenticationProviders (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyId UNIQUEIDENTIFIER NOT NULL,

    ProviderType NVARCHAR(50) NOT NULL, -- Local, AD, AzureAD, SAML, etc.
    ProviderName NVARCHAR(100) NOT NULL, -- Display name

    IsEnabled BIT NOT NULL DEFAULT 1,
    IsDefault BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,

    -- Connection Settings (JSON)
    ConnectionSettings NVARCHAR(MAX) NULL, -- JSON configuration

    -- AD Specific
    ADDomain NVARCHAR(200) NULL,
    ADServer NVARCHAR(200) NULL,
    ADPort INT NULL,
    ADUseSSL BIT DEFAULT 1,
    ADBaseDN NVARCHAR(500) NULL,
    ADServiceAccountUsername NVARCHAR(200) NULL,
    ADServiceAccountPassword NVARCHAR(500) NULL, -- Encrypted

    -- SAML Specific
    SAMLEntityId NVARCHAR(500) NULL,
    SAMLSSOUrl NVARCHAR(500) NULL,
    SAMLCertificate NVARCHAR(MAX) NULL,
    SAMLSigningCertificate NVARCHAR(MAX) NULL,

    -- OAuth/OIDC Specific
    OAuthClientId NVARCHAR(500) NULL,
    OAuthClientSecret NVARCHAR(500) NULL, -- Encrypted
    OAuthAuthorizationEndpoint NVARCHAR(500) NULL,
    OAuthTokenEndpoint NVARCHAR(500) NULL,
    OAuthUserInfoEndpoint NVARCHAR(500) NULL,
    OAuthScopes NVARCHAR(500) NULL,

    -- Azure AD Specific
    AzureADTenantId NVARCHAR(100) NULL,
    AzureADClientId NVARCHAR(100) NULL,
    AzureADClientSecret NVARCHAR(500) NULL, -- Encrypted

    -- Attribute Mapping (JSON)
    AttributeMapping NVARCHAR(MAX) NULL,

    -- JIT Provisioning Settings
    JITProvisioningEnabled BIT DEFAULT 1,
    AutoAssignRole UNIQUEIDENTIFIER NULL, -- Default role for new users
    AutoAssignDepartment UNIQUEIDENTIFIER NULL,

    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_AuthenticationProviders_Company FOREIGN KEY (CompanyId)
        REFERENCES Companies(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AuthenticationProviders_AutoAssignRole FOREIGN KEY (AutoAssignRole)
        REFERENCES ComplaintRoles(Id)
);

CREATE INDEX IX_AuthenticationProviders_CompanyId ON AuthenticationProviders(CompanyId);
CREATE INDEX IX_AuthenticationProviders_ProviderType ON AuthenticationProviders(ProviderType);
```

**C# Entity:**
```csharp
public class AuthenticationProvider
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }

    public AuthenticationProviderType ProviderType { get; set; }
    public string ProviderName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    // Configuration stored as JSON
    public string? ConnectionSettings { get; set; }

    // Active Directory
    public string? ADDomain { get; set; }
    public string? ADServer { get; set; }
    public int? ADPort { get; set; }
    public bool ADUseSSL { get; set; } = true;
    public string? ADBaseDN { get; set; }
    public string? ADServiceAccountUsername { get; set; }
    public string? ADServiceAccountPassword { get; set; } // Encrypted

    // SAML
    public string? SAMLEntityId { get; set; }
    public string? SAMLSSOUrl { get; set; }
    public string? SAMLCertificate { get; set; }
    public string? SAMLSigningCertificate { get; set; }

    // OAuth/OIDC
    public string? OAuthClientId { get; set; }
    public string? OAuthClientSecret { get; set; } // Encrypted
    public string? OAuthAuthorizationEndpoint { get; set; }
    public string? OAuthTokenEndpoint { get; set; }
    public string? OAuthUserInfoEndpoint { get; set; }
    public string? OAuthScopes { get; set; }

    // Azure AD
    public string? AzureADTenantId { get; set; }
    public string? AzureADClientId { get; set; }
    public string? AzureADClientSecret { get; set; } // Encrypted

    // Attribute Mapping (JSON)
    public string? AttributeMapping { get; set; }

    // JIT Provisioning
    public bool JITProvisioningEnabled { get; set; } = true;
    public Guid? AutoAssignRole { get; set; }
    public Guid? AutoAssignDepartment { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Company Company { get; set; } = null!;
    public ComplaintRole? DefaultRole { get; set; }
}
```

### 4.3 New Table: ExternalUserMappings

**Map external identities to local users:**

```sql
CREATE TABLE ExternalUserMappings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    AuthenticationProviderId UNIQUEIDENTIFIER NOT NULL,

    ExternalUserId NVARCHAR(500) NOT NULL, -- objectGuid, oid, nameID, etc.
    ExternalUsername NVARCHAR(200) NULL,
    ExternalEmail NVARCHAR(200) NULL,

    Attributes NVARCHAR(MAX) NULL, -- JSON of additional attributes

    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    LastSyncedAt DATETIME NULL,

    CONSTRAINT FK_ExternalUserMappings_User FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ExternalUserMappings_Provider FOREIGN KEY (AuthenticationProviderId)
        REFERENCES AuthenticationProviders(Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IX_ExternalUserMappings_External
    ON ExternalUserMappings(AuthenticationProviderId, ExternalUserId);
CREATE INDEX IX_ExternalUserMappings_UserId ON ExternalUserMappings(UserId);
```

**C# Entity:**
```csharp
public class ExternalUserMapping
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AuthenticationProviderId { get; set; }

    public string ExternalUserId { get; set; } = string.Empty;
    public string? ExternalUsername { get; set; }
    public string? ExternalEmail { get; set; }

    public string? Attributes { get; set; } // JSON

    public DateTime CreatedAt { get; set; }
    public DateTime? LastSyncedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public AuthenticationProvider Provider { get; set; } = null!;
}
```

### 4.4 New Table: AuthenticationAuditLog

**Enhanced audit logging:**

```sql
CREATE TABLE AuthenticationAuditLog (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NULL, -- NULL if user not found
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    AuthenticationProviderId UNIQUEIDENTIFIER NULL,

    AuthenticationMethod NVARCHAR(50) NOT NULL,
    Username NVARCHAR(200) NOT NULL,

    Success BIT NOT NULL,
    FailureReason NVARCHAR(500) NULL,

    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,

    ExternalUserId NVARCHAR(500) NULL,
    ExternalProvider NVARCHAR(100) NULL,

    AdditionalInfo NVARCHAR(MAX) NULL, -- JSON

    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_AuthenticationAuditLog_User FOREIGN KEY (UserId)
        REFERENCES Users(Id),
    CONSTRAINT FK_AuthenticationAuditLog_Company FOREIGN KEY (CompanyId)
        REFERENCES Companies(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AuthenticationAuditLog_Provider FOREIGN KEY (AuthenticationProviderId)
        REFERENCES AuthenticationProviders(Id)
);

CREATE INDEX IX_AuthenticationAuditLog_UserId ON AuthenticationAuditLog(UserId);
CREATE INDEX IX_AuthenticationAuditLog_CreatedAt ON AuthenticationAuditLog(CreatedAt DESC);
CREATE INDEX IX_AuthenticationAuditLog_CompanyId ON AuthenticationAuditLog(CompanyId);
```

---

## 5. Authentication Provider Abstraction

### 5.1 Core Interfaces

```csharp
public interface IAuthenticationProvider
{
    string ProviderName { get; }
    AuthenticationProviderType ProviderType { get; }

    Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken = default);

    Task<UserInfo> GetUserInfoAsync(
        string externalUserId,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetUserGroupsAsync(
        string externalUserId,
        CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    Task<bool> ValidateConfigurationAsync(CancellationToken cancellationToken = default);

    Task<bool> SyncUserAsync(
        User user,
        CancellationToken cancellationToken = default);
}

public class AuthenticationRequest
{
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? SamlToken { get; set; }
    public string? OAuthCode { get; set; }
    public string? OAuthToken { get; set; }
    public Dictionary<string, string> AdditionalParameters { get; set; } = new();
}

public class AuthenticationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExternalUserId { get; set; }
    public UserInfo? UserInfo { get; set; }
    public List<string> Groups { get; set; } = new();
    public Dictionary<string, object> Claims { get; set; } = new();
}

public class UserInfo
{
    public string ExternalUserId { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Manager { get; set; }
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();
}
```

### 5.2 Provider Factory

```csharp
public interface IAuthenticationProviderFactory
{
    IAuthenticationProvider CreateProvider(AuthenticationProvider config);
    IEnumerable<IAuthenticationProvider> GetAllProviders(Guid companyId);
    IAuthenticationProvider GetDefaultProvider(Guid companyId);
    IAuthenticationProvider? GetProviderByType(
        Guid companyId,
        AuthenticationProviderType type);
}

public class AuthenticationProviderFactory : IAuthenticationProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUnitOfWork _unitOfWork;

    public IAuthenticationProvider CreateProvider(AuthenticationProvider config)
    {
        return config.ProviderType switch
        {
            AuthenticationProviderType.Local =>
                _serviceProvider.GetRequiredService<LocalAuthenticationProvider>(),

            AuthenticationProviderType.ActiveDirectory =>
                new ActiveDirectoryProvider(config, _serviceProvider),

            AuthenticationProviderType.AzureAD =>
                new AzureADProvider(config, _serviceProvider),

            AuthenticationProviderType.SAML =>
                new SAMLProvider(config, _serviceProvider),

            AuthenticationProviderType.OAuth =>
                new OAuthProvider(config, _serviceProvider),

            AuthenticationProviderType.OIDC =>
                new OIDCProvider(config, _serviceProvider),

            AuthenticationProviderType.Oryggi =>
                new OryggiAuthenticationProvider(config, _serviceProvider),

            _ => throw new NotSupportedException($"Provider type {config.ProviderType} not supported")
        };
    }
}
```

---

## 6. Active Directory Integration

### 6.1 Active Directory Provider Implementation

```csharp
public class ActiveDirectoryProvider : IAuthenticationProvider
{
    private readonly AuthenticationProvider _config;
    private readonly ILogger<ActiveDirectoryProvider> _logger;

    public string ProviderName => "Active Directory";
    public AuthenticationProviderType ProviderType => AuthenticationProviderType.ActiveDirectory;

    public async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Option 1: LDAP Authentication
            using var connection = new LdapConnection(
                new LdapDirectoryIdentifier(_config.ADServer, _config.ADPort ?? 389));

            if (_config.ADUseSSL)
                connection.SessionOptions.SecureSocketLayer = true;

            // Bind with user credentials
            var userDN = BuildUserDN(request.Username);
            connection.Bind(new NetworkCredential(userDN, request.Password));

            // Authentication successful
            var userInfo = await GetUserInfoAsync(request.Username, cancellationToken);
            var groups = await GetUserGroupsAsync(request.Username, cancellationToken);

            return new AuthenticationResult
            {
                IsSuccess = true,
                ExternalUserId = userInfo.ExternalUserId,
                UserInfo = userInfo,
                Groups = groups
            };
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "AD authentication failed for {Username}", request.Username);
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Invalid username or password"
            };
        }
    }

    public async Task<UserInfo> GetUserInfoAsync(
        string username,
        CancellationToken cancellationToken)
    {
        using var connection = CreateServiceConnection();

        var searchFilter = $"(&(objectClass=user)(sAMAccountName={username}))";
        var searchRequest = new SearchRequest(
            _config.ADBaseDN,
            searchFilter,
            SearchScope.Subtree,
            "objectGuid", "sAMAccountName", "userPrincipalName",
            "givenName", "sn", "mail", "telephoneNumber",
            "title", "department", "manager"
        );

        var response = (SearchResponse)connection.SendRequest(searchRequest);

        if (response.Entries.Count == 0)
            throw new UserNotFoundException($"AD user {username} not found");

        var entry = response.Entries[0];

        return new UserInfo
        {
            ExternalUserId = GetAttributeValue<Guid>(entry, "objectGuid").ToString(),
            Username = GetAttributeValue<string>(entry, "sAMAccountName"),
            Email = GetAttributeValue<string>(entry, "mail"),
            FirstName = GetAttributeValue<string>(entry, "givenName"),
            LastName = GetAttributeValue<string>(entry, "sn"),
            Phone = GetAttributeValue<string>(entry, "telephoneNumber"),
            JobTitle = GetAttributeValue<string>(entry, "title"),
            Department = GetAttributeValue<string>(entry, "department")
        };
    }

    public async Task<List<string>> GetUserGroupsAsync(
        string username,
        CancellationToken cancellationToken)
    {
        using var connection = CreateServiceConnection();

        var searchFilter = $"(&(objectClass=user)(sAMAccountName={username}))";
        var searchRequest = new SearchRequest(
            _config.ADBaseDN,
            searchFilter,
            SearchScope.Subtree,
            "memberOf"
        );

        var response = (SearchResponse)connection.SendRequest(searchRequest);
        var entry = response.Entries[0];

        var groups = new List<string>();
        if (entry.Attributes.Contains("memberOf"))
        {
            foreach (string group in entry.Attributes["memberOf"])
            {
                // Extract CN from DN
                var cn = group.Split(',')[0].Replace("CN=", "");
                groups.Add(cn);
            }
        }

        return groups;
    }

    private LdapConnection CreateServiceConnection()
    {
        var connection = new LdapConnection(
            new LdapDirectoryIdentifier(_config.ADServer, _config.ADPort ?? 389));

        if (_config.ADUseSSL)
            connection.SessionOptions.SecureSocketLayer = true;

        // Bind with service account
        connection.Bind(new NetworkCredential(
            _config.ADServiceAccountUsername,
            DecryptPassword(_config.ADServiceAccountPassword)
        ));

        return connection;
    }
}
```

### 6.2 Windows Integrated Authentication (Optional)

**For intranet scenarios:**

```csharp
// In Program.cs
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

// Middleware to detect Windows auth
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true &&
        context.User.Identity.AuthenticationType == "Negotiate")
    {
        var windowsIdentity = (WindowsIdentity)context.User.Identity;
        var username = windowsIdentity.Name; // DOMAIN\username

        // Auto-login user
        var authService = context.RequestServices.GetRequiredService<IAuthenticationService>();
        var result = await authService.WindowsAuthLoginAsync(username);

        if (result.IsSuccess)
        {
            context.Items["JwtToken"] = result.Token;
        }
    }

    await next();
});
```

---

## 7. SSO Integration (SAML/OAuth/OIDC)

### 7.1 Azure AD / Entra ID Integration

**Using Microsoft.Identity.Web:**

```csharp
// In Program.cs
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        options.Instance = "https://login.microsoftonline.com/";
        options.TenantId = configuration["AzureAd:TenantId"];
        options.ClientId = configuration["AzureAd:ClientId"];
        options.ClientSecret = configuration["AzureAd:ClientSecret"];
        options.CallbackPath = "/signin-oidc";

        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = async context =>
            {
                // JIT user provisioning
                var claims = context.Principal.Claims;
                var email = claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var oid = claims.FirstOrDefault(c => c.Type == "oid")?.Value;

                var authService = context.HttpContext.RequestServices
                    .GetRequiredService<IAuthenticationService>();

                await authService.ProcessSSOLoginAsync(
                    providerType: AuthenticationProviderType.AzureAD,
                    externalUserId: oid,
                    claims: claims.ToDictionary(c => c.Type, c => (object)c.Value)
                );
            }
        };
    });
```

**Azure AD Provider:**

```csharp
public class AzureADProvider : IAuthenticationProvider
{
    public async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        // Azure AD authentication happens via redirect
        // This method processes the callback

        if (string.IsNullOrEmpty(request.OAuthCode))
            throw new InvalidOperationException("OAuth authorization code required");

        // Exchange code for tokens
        var tokenClient = new ConfidentialClientApplication(
            _config.AzureADClientId,
            $"https://login.microsoftonline.com/{_config.AzureADTenantId}",
            "https://yourapp.com/signin-oidc",
            new ClientCredential(_config.AzureADClientSecret),
            null, null);

        var result = await tokenClient.AcquireTokenByAuthorizationCodeAsync(
            new[] { "User.Read" },
            request.OAuthCode);

        // Get user info from Microsoft Graph
        var graphClient = new GraphServiceClient(
            new DelegateAuthenticationProvider(req =>
            {
                req.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.AccessToken);
                return Task.CompletedTask;
            }));

        var user = await graphClient.Me.Request().GetAsync();

        return new AuthenticationResult
        {
            IsSuccess = true,
            ExternalUserId = user.Id,
            UserInfo = new UserInfo
            {
                ExternalUserId = user.Id,
                Email = user.Mail ?? user.UserPrincipalName,
                FirstName = user.GivenName,
                LastName = user.Surname,
                JobTitle = user.JobTitle,
                Department = user.Department
            }
        };
    }
}
```

### 7.2 SAML 2.0 Integration

**Using Sustainsys.Saml2:**

```csharp
public class SAMLProvider : IAuthenticationProvider
{
    private readonly AuthenticationProvider _config;

    public async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.SamlToken))
            throw new InvalidOperationException("SAML assertion required");

        // Parse SAML assertion
        var samlResponse = new Saml2Response(
            request.SamlToken,
            new Saml2Id(),
            null);

        // Validate signature
        var certificate = new X509Certificate2(
            Convert.FromBase64String(_config.SAMLCertificate));

        if (!samlResponse.IsSignedByAny(new[] { certificate }))
            throw new SecurityException("Invalid SAML signature");

        // Extract claims
        var assertion = samlResponse.Saml2Assertions.First();
        var claims = assertion.Statements
            .OfType<Saml2AttributeStatement>()
            .SelectMany(s => s.Attributes)
            .ToDictionary(
                a => a.Name,
                a => a.Values.First()
            );

        return new AuthenticationResult
        {
            IsSuccess = true,
            ExternalUserId = claims["NameID"].ToString(),
            UserInfo = new UserInfo
            {
                ExternalUserId = claims["NameID"].ToString(),
                Email = claims["email"].ToString(),
                FirstName = claims["givenName"].ToString(),
                LastName = claims["surname"].ToString()
            },
            Claims = claims.ToDictionary(k => k.Key, v => (object)v.Value)
        };
    }
}
```

### 7.3 Generic OAuth 2.0 / OIDC

```csharp
public class OAuthProvider : IAuthenticationProvider
{
    public async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        // Exchange authorization code for access token
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _config.OAuthTokenEndpoint);
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = request.OAuthCode,
            ["client_id"] = _config.OAuthClientId,
            ["client_secret"] = DecryptSecret(_config.OAuthClientSecret),
            ["redirect_uri"] = "https://yourapp.com/auth/callback"
        });

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(tokenRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<OAuthTokenResponse>();

        // Get user info
        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, _config.OAuthUserInfoEndpoint);
        userInfoRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var userInfoResponse = await httpClient.SendAsync(userInfoRequest, cancellationToken);
        var userInfo = await userInfoResponse.Content.ReadFromJsonAsync<OAuthUserInfo>();

        return new AuthenticationResult
        {
            IsSuccess = true,
            ExternalUserId = userInfo.Sub,
            UserInfo = MapOAuthUserInfo(userInfo)
        };
    }
}
```

---

## 8. Third-Party Authentication

### 8.1 Oryggi HRMS Integration

```csharp
public class OryggiAuthenticationProvider : IAuthenticationProvider
{
    public async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        // Call Oryggi authentication API
        var oryggiClient = _httpClientFactory.CreateClient("Oryggi");

        var authRequest = new
        {
            employeeCode = request.Username,
            password = request.Password,
            companyCode = _config.ConnectionSettings
        };

        var response = await oryggiClient.PostAsJsonAsync(
            "/api/auth/validate",
            authRequest,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Invalid credentials"
            };
        }

        var oryggiUser = await response.Content.ReadFromJsonAsync<OryggiUserResponse>();

        return new AuthenticationResult
        {
            IsSuccess = true,
            ExternalUserId = oryggiUser.EmployeeId,
            UserInfo = new UserInfo
            {
                ExternalUserId = oryggiUser.EmployeeId,
                Username = oryggiUser.EmployeeCode,
                Email = oryggiUser.Email,
                FirstName = oryggiUser.FirstName,
                LastName = oryggiUser.LastName,
                JobTitle = oryggiUser.Designation,
                Department = oryggiUser.Department
            }
        };
    }
}
```

### 8.2 Custom API Authentication

```csharp
public class CustomAPIAuthenticationProvider : IAuthenticationProvider
{
    public async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        // Parse connection settings
        var settings = JsonSerializer.Deserialize<CustomAPISettings>(
            _config.ConnectionSettings);

        var httpClient = _httpClientFactory.CreateClient();

        // Call custom authentication endpoint
        var authRequest = new HttpRequestMessage(HttpMethod.Post, settings.AuthEndpoint);
        authRequest.Headers.Add("X-API-Key", settings.ApiKey);
        authRequest.Content = JsonContent.Create(new
        {
            username = request.Username,
            password = request.Password,
            additionalParams = request.AdditionalParameters
        });

        var response = await httpClient.SendAsync(authRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = await response.Content.ReadAsStringAsync()
            };
        }

        var apiResponse = await response.Content
            .ReadFromJsonAsync<CustomAPIAuthResponse>(cancellationToken);

        return new AuthenticationResult
        {
            IsSuccess = apiResponse.Success,
            ExternalUserId = apiResponse.UserId,
            UserInfo = MapCustomAPIUser(apiResponse.UserData)
        };
    }
}
```

---

## 9. User Provisioning & Sync

### 9.1 Just-in-Time (JIT) Provisioning

```csharp
public interface IUserProvisioningService
{
    Task<User> GetOrCreateUserAsync(
        AuthenticationResult authResult,
        Guid companyId,
        AuthenticationProvider provider,
        CancellationToken cancellationToken = default);

    Task SyncUserAttributesAsync(
        User user,
        UserInfo externalUserInfo,
        CancellationToken cancellationToken = default);

    Task SyncRolesAsync(
        User user,
        List<string> externalGroups,
        AuthenticationProvider provider,
        CancellationToken cancellationToken = default);
}

public class UserProvisioningService : IUserProvisioningService
{
    public async Task<User> GetOrCreateUserAsync(
        AuthenticationResult authResult,
        Guid companyId,
        AuthenticationProvider provider,
        CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _unitOfWork.Users
            .GetByExternalUserIdAsync(
                provider.Id,
                authResult.ExternalUserId,
                cancellationToken);

        if (existingUser != null)
        {
            // Update existing user
            await SyncUserAttributesAsync(existingUser, authResult.UserInfo, cancellationToken);
            return existingUser;
        }

        // Check if JIT provisioning is enabled
        if (!provider.JITProvisioningEnabled)
            throw new UnauthorizedAccessException("User not found and JIT provisioning is disabled");

        // Create new user
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            AuthenticationProvider = provider.ProviderType,
            ExternalUserId = authResult.ExternalUserId,
            ExternalUsername = authResult.UserInfo.Username,
            IdentityProvider = provider.ProviderName,
            Email = authResult.UserInfo.Email,
            FirstName = authResult.UserInfo.FirstName,
            LastName = authResult.UserInfo.LastName,
            Phone = authResult.UserInfo.Phone,
            JobTitle = authResult.UserInfo.JobTitle,
            IsActive = true,
            SSOEnabled = true,
            LocalPasswordEnabled = false, // SSO user, no local password
            LastExternalSync = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(newUser, cancellationToken);

        // Create external user mapping
        var mapping = new ExternalUserMapping
        {
            Id = Guid.NewGuid(),
            UserId = newUser.Id,
            AuthenticationProviderId = provider.Id,
            ExternalUserId = authResult.ExternalUserId,
            ExternalUsername = authResult.UserInfo.Username,
            ExternalEmail = authResult.UserInfo.Email,
            Attributes = JsonSerializer.Serialize(authResult.UserInfo.AdditionalAttributes),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ExternalUserMappings.AddAsync(mapping, cancellationToken);

        // Auto-assign default role if configured
        if (provider.AutoAssignRole.HasValue)
        {
            var roleAssignment = new UserComplaintRole
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                ComplaintRoleId = provider.AutoAssignRole.Value,
                AssignedAt = DateTime.UtcNow,
                IsActive = true,
                IsPrimary = true
            };

            await _unitOfWork.UserComplaintRoles.AddAsync(roleAssignment, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "JIT provisioned new user {Email} from {Provider}",
            newUser.Email, provider.ProviderName);

        return newUser;
    }

    public async Task SyncUserAttributesAsync(
        User user,
        UserInfo externalUserInfo,
        CancellationToken cancellationToken)
    {
        // Update user attributes from external source
        user.FirstName = externalUserInfo.FirstName;
        user.LastName = externalUserInfo.LastName;
        user.Email = externalUserInfo.Email;
        user.Phone = externalUserInfo.Phone;
        user.JobTitle = externalUserInfo.JobTitle;
        user.LastExternalSync = DateTime.UtcNow;

        // Map department if available
        if (!string.IsNullOrEmpty(externalUserInfo.Department))
        {
            var department = await _unitOfWork.Departments
                .GetByNameAsync(user.CompanyId, externalUserInfo.Department, cancellationToken);

            if (department != null)
                user.DepartmentId = department.Id;
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncRolesAsync(
        User user,
        List<string> externalGroups,
        AuthenticationProvider provider,
        CancellationToken cancellationToken)
    {
        // Map external groups to local roles
        var attributeMapping = JsonSerializer.Deserialize<AttributeMapping>(
            provider.AttributeMapping ?? "{}");

        if (attributeMapping?.GroupToRoleMapping == null)
            return;

        foreach (var group in externalGroups)
        {
            if (attributeMapping.GroupToRoleMapping.TryGetValue(group, out var roleId))
            {
                // Check if user already has this role
                var hasRole = await _unitOfWork.UserComplaintRoles
                    .ExistsAsync(user.Id, roleId, cancellationToken);

                if (!hasRole)
                {
                    var roleAssignment = new UserComplaintRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ComplaintRoleId = roleId,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.UserComplaintRoles.AddAsync(
                        roleAssignment, cancellationToken);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class AttributeMapping
{
    public Dictionary<string, Guid> GroupToRoleMapping { get; set; } = new();
    public Dictionary<string, string> AttributeMapping { get; set; } = new();
}
```

### 9.2 Background Sync Service

```csharp
public class UserSyncBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Sync users from external providers every hour
                await SyncAllUsersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing users from external providers");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task SyncAllUsersAsync(CancellationToken cancellationToken)
    {
        var providers = await _unitOfWork.AuthenticationProviders
            .GetAllEnabledAsync(cancellationToken);

        foreach (var provider in providers)
        {
            if (!provider.ExternalSyncEnabled)
                continue;

            var authProvider = _providerFactory.CreateProvider(provider);

            // Get all users for this provider
            var users = await _unitOfWork.Users
                .GetByAuthProviderAsync(provider.Id, cancellationToken);

            foreach (var user in users)
            {
                try
                {
                    await authProvider.SyncUserAsync(user, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to sync user {UserId} from {Provider}",
                        user.Id, provider.ProviderName);
                }
            }
        }
    }
}
```

---

## 10. Security Considerations

### 10.1 Credential Storage

**All secrets must be encrypted:**
```csharp
public interface ISecretEncryptionService
{
    string EncryptSecret(string plaintext);
    string DecryptSecret(string ciphertext);
}

// Use Data Protection API
public class SecretEncryptionService : ISecretEncryptionService
{
    private readonly IDataProtector _protector;

    public SecretEncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("AuthenticationSecrets");
    }

    public string EncryptSecret(string plaintext)
    {
        return _protector.Protect(plaintext);
    }

    public string DecryptSecret(string ciphertext)
    {
        return _protector.Unprotect(ciphertext);
    }
}
```

### 10.2 Token Validation

**Validate all external tokens:**
```csharp
public class TokenValidator
{
    public async Task<bool> ValidateSAMLAssertionAsync(string samlToken)
    {
        // Validate signature
        // Check expiration
        // Verify audience
        // Check issuer
    }

    public async Task<bool> ValidateJWTAsync(string jwtToken)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _config.Issuer,
            ValidAudience = _config.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Secret))
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(jwtToken, validationParameters, out var validatedToken);

        return validatedToken != null;
    }
}
```

### 10.3 Rate Limiting

**Prevent brute force attacks:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("sso-login", config =>
    {
        config.Window = TimeSpan.FromMinutes(5);
        config.PermitLimit = 10;
        config.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("ad-login", config =>
    {
        config.Window = TimeSpan.FromMinutes(5);
        config.PermitLimit = 5;
        config.QueueLimit = 0;
    });
});
```

---

## 11. Implementation Plan

### Phase 1: Foundation (Week 1-2)

**Week 1: Database & Core Infrastructure**
- [ ] Create database migrations for new tables
- [ ] Add authentication provider columns to Users table
- [ ] Create AuthenticationProviders entity
- [ ] Create ExternalUserMappings entity
- [ ] Create AuthenticationAuditLog entity
- [ ] Implement provider factory pattern
- [ ] Create base authentication interfaces

**Week 2: Provider Abstraction & Local Auth**
- [ ] Implement IAuthenticationProvider interface
- [ ] Create AuthenticationOrchestrator
- [ ] Refactor existing local auth to use provider pattern
- [ ] Implement IUserProvisioningService
- [ ] Create attribute mapping system
- [ ] Write unit tests

### Phase 2: Active Directory (Week 3-4)

**Week 3: AD Integration**
- [ ] Implement ActiveDirectoryProvider
- [ ] LDAP authentication
- [ ] AD user attribute retrieval
- [ ] AD group membership
- [ ] Service account configuration
- [ ] Test with real AD server

**Week 4: AD Features & Testing**
- [ ] Windows Integrated Authentication (optional)
- [ ] AD user sync background service
- [ ] JIT provisioning for AD users
- [ ] Group to role mapping
- [ ] Comprehensive testing
- [ ] Documentation

### Phase 3: Azure AD / SSO (Week 5-6)

**Week 5: Azure AD**
- [ ] Implement AzureADProvider
- [ ] OAuth/OIDC flow
- [ ] Microsoft Graph integration
- [ ] JIT provisioning from Azure AD
- [ ] Azure AD group sync
- [ ] Test with Azure AD tenant

**Week 6: SAML & Generic SSO**
- [ ] Implement SAMLProvider
- [ ] SAML assertion validation
- [ ] Generic OAuth2 provider
- [ ] OIDC provider
- [ ] SSO provider admin UI
- [ ] Testing with multiple providers

### Phase 4: Frontend & UX (Week 7-8)

**Week 7: Login UI**
- [ ] Enhanced login page with provider selection
- [ ] SSO redirect handling
- [ ] OAuth callback handling
- [ ] SAML callback handling
- [ ] Provider detection logic
- [ ] Loading states and errors

**Week 8: Admin UI**
- [ ] Authentication provider management
- [ ] Provider configuration UI
- [ ] Attribute mapping UI
- [ ] User provisioning logs
- [ ] Authentication audit logs
- [ ] Test end-to-end flows

### Phase 5: Testing & Deployment (Week 9-10)

**Week 9: Integration Testing**
- [ ] E2E tests for all providers
- [ ] Security testing
- [ ] Performance testing
- [ ] Failover testing
- [ ] Migration testing
- [ ] Fix issues

**Week 10: Documentation & Deployment**
- [ ] Admin documentation
- [ ] User documentation
- [ ] API documentation
- [ ] Deployment guide
- [ ] Training materials
- [ ] Production deployment

---

## 12. Testing Strategy

### 12.1 Unit Tests

```csharp
[Fact]
public async Task ActiveDirectoryProvider_ValidCredentials_AuthenticatesSuccessfully()
{
    // Arrange
    var provider = new ActiveDirectoryProvider(adConfig, logger);
    var request = new AuthenticationRequest
    {
        Username = "testuser",
        Password = "ValidPassword123"
    };

    // Act
    var result = await provider.AuthenticateAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.UserInfo);
    Assert.Equal("testuser@domain.com", result.UserInfo.Email);
}

[Fact]
public async Task UserProvisioningService_NewADUser_CreatesUserWithMapping()
{
    // Arrange
    var authResult = new AuthenticationResult
    {
        IsSuccess = true,
        ExternalUserId = "ad-guid-123",
        UserInfo = new UserInfo
        {
            Email = "newuser@company.com",
            FirstName = "John",
            LastName = "Doe"
        }
    };

    // Act
    var user = await _provisioningService.GetOrCreateUserAsync(
        authResult, companyId, adProvider);

    // Assert
    Assert.NotNull(user);
    Assert.Equal("newuser@company.com", user.Email);
    Assert.Equal(AuthenticationProviderType.ActiveDirectory, user.AuthenticationProvider);

    var mapping = await _unitOfWork.ExternalUserMappings
        .GetByUserIdAsync(user.Id);
    Assert.NotNull(mapping);
    Assert.Equal("ad-guid-123", mapping.ExternalUserId);
}
```

### 12.2 Integration Tests

```csharp
[Fact]
public async Task EndToEnd_ADLogin_CreatesUserAndGeneratesToken()
{
    // Arrange
    var client = _factory.CreateClient();
    var loginRequest = new
    {
        username = "testuser@domain.com",
        password = "ADPassword123",
        authProvider = "ActiveDirectory"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

    // Assert
    response.EnsureSuccessStatusCode();
    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

    Assert.NotNull(loginResponse.Token);
    Assert.Equal("testuser@domain.com", loginResponse.User.Email);
    Assert.True(loginResponse.User.SSOEnabled);

    // Verify user was created in database
    var user = await _dbContext.Users
        .FirstOrDefaultAsync(u => u.Email == "testuser@domain.com");
    Assert.NotNull(user);
    Assert.Equal(AuthenticationProviderType.ActiveDirectory, user.AuthenticationProvider);
}
```

---

**CONTINUED IN NEXT SECTION...**

This plan provides a comprehensive foundation for hybrid authentication. Would you like me to:

1. Continue with more detailed sections?
2. Focus on a specific provider (AD, Azure AD, SAML)?
3. Create implementation code for Phase 1?
4. Design the frontend UI for provider selection?
