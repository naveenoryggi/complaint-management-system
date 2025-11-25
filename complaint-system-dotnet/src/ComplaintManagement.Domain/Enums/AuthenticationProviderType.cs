namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Types of authentication providers supported by the system
/// </summary>
public enum AuthenticationProviderType
{
    /// <summary>
    /// Local database authentication (email + password)
    /// </summary>
    Local = 1,

    /// <summary>
    /// Active Directory / LDAP authentication
    /// </summary>
    ActiveDirectory = 2,

    /// <summary>
    /// Azure Active Directory (Microsoft Entra ID) via OAuth/OIDC
    /// </summary>
    AzureAD = 3,

    /// <summary>
    /// SAML 2.0 Single Sign-On
    /// </summary>
    SAML = 4,

    /// <summary>
    /// Generic OAuth 2.0 provider (Google, Okta, Auth0, etc.)
    /// </summary>
    OAuth = 5,

    /// <summary>
    /// Generic OpenID Connect provider
    /// </summary>
    OIDC = 6,

    /// <summary>
    /// Custom third-party API (e.g., Oryggi HRMS, custom systems)
    /// </summary>
    CustomAPI = 7,

    /// <summary>
    /// Windows Integrated Authentication (Negotiate/Kerberos)
    /// </summary>
    WindowsAuth = 8
}
