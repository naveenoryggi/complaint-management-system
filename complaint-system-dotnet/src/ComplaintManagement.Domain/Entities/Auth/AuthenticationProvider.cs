using ComplaintManagement.Domain.Entities;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Roles;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Auth;

/// <summary>
/// Configuration for external authentication providers (AD, SSO, OAuth, SAML)
/// Supports multiple providers per company for hybrid authentication
/// </summary>
public class AuthenticationProvider : BaseEntity
{
    /// <summary>
    /// Company this provider belongs to
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Type of authentication provider
    /// </summary>
    public AuthenticationProviderType ProviderType { get; set; }

    /// <summary>
    /// Display name for the provider (e.g., "Company Active Directory", "Google SSO")
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this provider is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Whether this is the default provider for the company
    /// Used when user doesn't explicitly select a provider
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Priority/order for provider selection
    /// Lower number = higher priority
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Email domain that triggers this provider (e.g., "@company.com")
    /// Auto-selects provider based on user's email domain
    /// </summary>
    public string? EmailDomain { get; set; }

    // ========== Active Directory / LDAP Settings ==========

    /// <summary>
    /// AD domain (e.g., "company.local")
    /// </summary>
    public string? ADDomain { get; set; }

    /// <summary>
    /// AD server address (e.g., "ldap.company.com" or "192.168.1.10")
    /// </summary>
    public string? ADServer { get; set; }

    /// <summary>
    /// AD server port (default: 389 for LDAP, 636 for LDAPS)
    /// </summary>
    public int? ADPort { get; set; }

    /// <summary>
    /// AD base DN for user searches (e.g., "DC=company,DC=local")
    /// </summary>
    public string? ADBaseDN { get; set; }

    /// <summary>
    /// AD user filter template (e.g., "(&(objectClass=user)(sAMAccountName={0}))")
    /// </summary>
    public string? ADUserFilter { get; set; }

    /// <summary>
    /// Use SSL/TLS for LDAP connection
    /// </summary>
    public bool? ADUseSSL { get; set; }

    /// <summary>
    /// AD service account username for binding (optional)
    /// </summary>
    public string? ADServiceAccountUsername { get; set; }

    /// <summary>
    /// AD service account password (encrypted)
    /// </summary>
    public string? ADServiceAccountPasswordEncrypted { get; set; }

    // ========== SAML Settings ==========

    /// <summary>
    /// SAML Entity ID (Issuer)
    /// </summary>
    public string? SAMLEntityId { get; set; }

    /// <summary>
    /// SAML SSO URL (Identity Provider login URL)
    /// </summary>
    public string? SAMLSSOUrl { get; set; }

    /// <summary>
    /// SAML SLO URL (Single Logout URL)
    /// </summary>
    public string? SAMLSLOUrl { get; set; }

    /// <summary>
    /// SAML X.509 Certificate (PEM format)
    /// Used to verify SAML assertions
    /// </summary>
    public string? SAMLCertificate { get; set; }

    /// <summary>
    /// SAML signing algorithm (e.g., "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")
    /// </summary>
    public string? SAMLSigningAlgorithm { get; set; }

    // ========== OAuth 2.0 / OIDC Settings ==========

    /// <summary>
    /// OAuth Client ID
    /// </summary>
    public string? OAuthClientId { get; set; }

    /// <summary>
    /// OAuth Client Secret (encrypted)
    /// </summary>
    public string? OAuthClientSecretEncrypted { get; set; }

    /// <summary>
    /// OAuth Authorization endpoint URL
    /// </summary>
    public string? OAuthAuthorizationUrl { get; set; }

    /// <summary>
    /// OAuth Token endpoint URL
    /// </summary>
    public string? OAuthTokenUrl { get; set; }

    /// <summary>
    /// OAuth UserInfo endpoint URL
    /// </summary>
    public string? OAuthUserInfoUrl { get; set; }

    /// <summary>
    /// OAuth scopes (comma-separated, e.g., "openid,profile,email")
    /// </summary>
    public string? OAuthScopes { get; set; }

    /// <summary>
    /// OAuth redirect URI (callback URL)
    /// </summary>
    public string? OAuthRedirectUri { get; set; }

    // ========== Azure AD Specific Settings ==========

    /// <summary>
    /// Azure AD Tenant ID (GUID)
    /// </summary>
    public string? AzureADTenantId { get; set; }

    /// <summary>
    /// Azure AD Application (Client) ID
    /// </summary>
    public string? AzureADClientId { get; set; }

    /// <summary>
    /// Azure AD Client Secret (encrypted)
    /// </summary>
    public string? AzureADClientSecretEncrypted { get; set; }

    /// <summary>
    /// Azure AD instance (default: "https://login.microsoftonline.com/")
    /// </summary>
    public string? AzureADInstance { get; set; }

    // ========== Custom API Settings ==========

    /// <summary>
    /// Custom API authentication endpoint URL
    /// </summary>
    public string? CustomAPIEndpoint { get; set; }

    /// <summary>
    /// Custom API authentication method (GET, POST)
    /// </summary>
    public string? CustomAPIMethod { get; set; }

    /// <summary>
    /// Custom API headers (JSON format)
    /// </summary>
    public string? CustomAPIHeaders { get; set; }

    /// <summary>
    /// Custom API request body template (JSON format)
    /// </summary>
    public string? CustomAPIRequestTemplate { get; set; }

    /// <summary>
    /// Custom API API key/token (encrypted)
    /// </summary>
    public string? CustomAPIKeyEncrypted { get; set; }

    // ========== Just-in-Time Provisioning Settings ==========

    /// <summary>
    /// Enable automatic user provisioning on first login
    /// </summary>
    public bool JITProvisioningEnabled { get; set; } = true;

    /// <summary>
    /// Auto-assign this role to JIT provisioned users
    /// </summary>
    public Guid? AutoAssignRoleId { get; set; }

    /// <summary>
    /// Sync user attributes on every login
    /// </summary>
    public bool SyncAttributesOnLogin { get; set; } = true;

    /// <summary>
    /// Attribute mapping configuration (JSON format)
    /// Maps external attributes to local user properties
    /// Example: {"email": "mail", "firstName": "givenName", "lastName": "sn"}
    /// </summary>
    public string? AttributeMapping { get; set; }

    /// <summary>
    /// Group to role mapping configuration (JSON format)
    /// Maps external groups to local roles
    /// Example: {"AD_Admins": "Admin", "AD_Users": "User"}
    /// </summary>
    public string? GroupToRoleMapping { get; set; }

    // ========== Audit Fields ==========

    /// <summary>
    /// When this provider was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created this provider
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// When this provider was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who last updated this provider
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Last time this provider was successfully used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Last time provider availability was checked
    /// </summary>
    public DateTime? LastHealthCheckAt { get; set; }

    /// <summary>
    /// Whether the last health check was successful
    /// </summary>
    public bool? LastHealthCheckSuccess { get; set; }

    // ========== Navigation Properties ==========

    /// <summary>
    /// Navigation property to the company
    /// </summary>
    public virtual Company? Company { get; set; }

    /// <summary>
    /// Navigation property to the auto-assign role
    /// </summary>
    public virtual ComplaintRole? AutoAssignRole { get; set; }

    /// <summary>
    /// External user mappings using this provider
    /// </summary>
    public virtual ICollection<ExternalUserMapping> ExternalUserMappings { get; set; } = new List<ExternalUserMapping>();
}
