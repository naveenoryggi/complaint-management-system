namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Email authentication methods supported by the system
/// </summary>
public enum EmailAuthenticationType
{
    /// <summary>
    /// Basic authentication using username and password
    /// Works with: Gmail (app passwords), custom SMTP servers
    /// </summary>
    Basic = 1,

    /// <summary>
    /// OAuth 2.0 authentication (modern authentication)
    /// Required for: Office 365, Gmail (without app passwords)
    /// </summary>
    OAuth2 = 2
}
