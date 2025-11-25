using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Domain.Entities.Settings;

/// <summary>
/// Email (SMTP) server configuration
/// Supports both Basic Authentication and OAuth 2.0 (Microsoft 365, Gmail, etc.)
/// </summary>
public class EmailServerSettings : BaseEntity
{
    /// <summary>
    /// Configuration name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Authentication method: Basic (username/password) or OAuth2
    /// </summary>
    public EmailAuthenticationType AuthenticationType { get; set; } = EmailAuthenticationType.Basic;

    /// <summary>
    /// SMTP server host
    /// </summary>
    [Required(ErrorMessage = "SMTP host is required")]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
    public int Port { get; set; } = 587;

    /// <summary>
    /// Use SSL/TLS?
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// SMTP username (email address)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// SMTP password (for Basic authentication only)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// From email address
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// From display name
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// Reply-to email address
    /// </summary>
    public string? ReplyToEmail { get; set; }

    // OAuth 2.0 Settings (for Office 365, Gmail, etc.)
    /// <summary>
    /// OAuth 2.0 Client ID (from Azure AD or Google Console)
    /// </summary>
    public string? OAuthClientId { get; set; }

    /// <summary>
    /// OAuth 2.0 Client Secret (encrypted)
    /// </summary>
    public string? OAuthClientSecret { get; set; }

    /// <summary>
    /// OAuth 2.0 Tenant ID (for Azure AD/Office 365)
    /// </summary>
    public string? OAuthTenantId { get; set; }

    /// <summary>
    /// OAuth 2.0 Access Token (for SMTP authentication)
    /// </summary>
    public string? OAuthAccessToken { get; set; }

    /// <summary>
    /// OAuth 2.0 Refresh Token (to get new access tokens)
    /// </summary>
    public string? OAuthRefreshToken { get; set; }

    /// <summary>
    /// When the OAuth access token expires
    /// </summary>
    public DateTime? OAuthTokenExpiresAt { get; set; }

    /// <summary>
    /// OAuth 2.0 Scopes (e.g., "https://outlook.office365.com/SMTP.Send")
    /// </summary>
    public string? OAuthScopes { get; set; }

    /// <summary>
    /// How often to refresh OAuth tokens (in minutes, 5-120).
    /// Recommended: Set to half the token's expiry time (e.g., 30 min for 1-hour tokens).
    /// </summary>
    public int? OAuthTokenRefreshIntervalMinutes { get; set; }

    /// <summary>
    /// Is this configuration active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is this the default configuration?
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Maximum emails per hour (rate limiting)
    /// </summary>
    public int? MaxEmailsPerHour { get; set; }

    /// <summary>
    /// Timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Company ID (null = global)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Test connection notes
    /// </summary>
    public string? TestNotes { get; set; }

    /// <summary>
    /// Last successful test date
    /// </summary>
    public DateTime? LastTestedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Company
    /// </summary>
    public Company? Company { get; set; }
}
