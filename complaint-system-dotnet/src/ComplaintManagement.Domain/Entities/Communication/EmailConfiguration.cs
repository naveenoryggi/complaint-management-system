using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;
using System.Text.Json.Serialization;

namespace ComplaintManagement.Domain.Entities.Communication;

/// <summary>
/// Email configuration for IMAP/SMTP integration per company
/// Stores credentials and settings for email ticketing system
/// </summary>
public class EmailConfiguration : BaseEntity
{
    public Guid CompanyId { get; set; }

    [JsonIgnore]
    public Company? Company { get; set; }

    // Authentication Type
    /// <summary>
    /// Authentication method: Basic (username/password) or OAuth2
    /// </summary>
    public EmailAuthenticationType AuthenticationType { get; set; } = EmailAuthenticationType.Basic;

    // IMAP Settings (Incoming Mail)
    public string ImapHost { get; set; } = string.Empty;
    public int ImapPort { get; set; } = 993; // Default SSL port
    public bool ImapUseSsl { get; set; } = true;
    public string ImapUsername { get; set; } = string.Empty;
    public string ImapPassword { get; set; } = string.Empty; // Used for Basic auth
    public string ImapFolder { get; set; } = "INBOX"; // Folder to monitor

    // SMTP Settings (Outgoing Mail)
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587; // Default TLS port
    public bool SmtpUseSsl { get; set; } = true;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty; // Used for Basic auth
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;

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
    /// OAuth 2.0 Access Token (for IMAP/SMTP authentication)
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
    /// OAuth 2.0 Scopes (e.g., "https://outlook.office365.com/IMAP.AccessAsUser.All")
    /// </summary>
    public string? OAuthScopes { get; set; }

    /// <summary>
    /// How often to refresh OAuth tokens for THIS email account (in minutes, 5-120).
    /// If null, uses system-wide default from SystemConfiguration.
    /// Recommended: Set to half the token's expiry time (e.g., 30 min for 1-hour tokens).
    /// </summary>
    public int? OAuthTokenRefreshIntervalMinutes { get; set; }

    // Polling Configuration
    public int PollingIntervalMinutes { get; set; } = 5; // How often to check for new emails (for backward compatibility)
    public int? PollingIntervalSeconds { get; set; } // Takes precedence over minutes if set (30, 60, 120, etc.)
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastPolledAt { get; set; }

    // Auto-Acknowledgement Settings
    public bool SendAutoAcknowledgement { get; set; } = true;
    public string? AutoAcknowledgementTemplateId { get; set; }

    [JsonIgnore]
    public CommunicationTemplate? AutoAcknowledgementTemplate { get; set; }

    // Thread Detection Settings
    public bool EnableThreading { get; set; } = true;
    public int ThreadTimeoutDays { get; set; } = 30; // Consider threads dead after X days

    // Attachment Settings
    public long MaxAttachmentSizeBytes { get; set; } = 10485760; // 10MB default
    public string AllowedAttachmentExtensions { get; set; } = ".pdf,.doc,.docx,.txt,.png,.jpg,.jpeg,.gif"; // Comma-separated

    // ============================================================
    // SEPARATE SMTP ACCOUNT SUPPORT
    // Allows using different email accounts for receiving (IMAP) and sending (SMTP)
    // Example: Receive from support@company.com, send from noreply@company.com
    // ============================================================

    /// <summary>
    /// Use a different email account for sending responses (SMTP)?
    /// If false (default), uses the same account as IMAP for both receiving and sending.
    /// If true, uses separate credentials/OAuth for SMTP.
    /// </summary>
    public bool UseSeparateSmtpAccount { get; set; } = false;

    /// <summary>
    /// Separate SMTP authentication type (only used if UseSeparateSmtpAccount = true)
    /// Can be different from main AuthenticationType (e.g., OAuth for IMAP, Basic for SMTP)
    /// </summary>
    public EmailAuthenticationType? SmtpAuthenticationType { get; set; }

    /// <summary>
    /// Separate SMTP username (only used if UseSeparateSmtpAccount = true)
    /// </summary>
    public string? SmtpSeparateUsername { get; set; }

    /// <summary>
    /// Separate SMTP password for basic auth (only used if UseSeparateSmtpAccount = true)
    /// </summary>
    public string? SmtpSeparatePassword { get; set; }

    /// <summary>
    /// Separate SMTP From Email (only used if UseSeparateSmtpAccount = true)
    /// Example: Use support@company.com for IMAP, but send from noreply@company.com
    /// </summary>
    public string? SmtpSeparateFromEmail { get; set; }

    /// <summary>
    /// Separate SMTP From Name (only used if UseSeparateSmtpAccount = true)
    /// </summary>
    public string? SmtpSeparateFromName { get; set; }

    // Separate SMTP OAuth 2.0 fields (only used if UseSeparateSmtpAccount = true AND SmtpAuthenticationType = OAuth2)

    /// <summary>
    /// Separate SMTP OAuth 2.0 Client ID (only used if UseSeparateSmtpAccount = true)
    /// </summary>
    public string? SmtpSeparateOAuthClientId { get; set; }

    /// <summary>
    /// Separate SMTP OAuth 2.0 Client Secret (only used if UseSeparateSmtpAccount = true)
    /// </summary>
    public string? SmtpSeparateOAuthClientSecret { get; set; }

    /// <summary>
    /// Separate SMTP OAuth 2.0 Tenant ID (only used if UseSeparateSmtpAccount = true)
    /// </summary>
    public string? SmtpSeparateOAuthTenantId { get; set; }

    /// <summary>
    /// Separate SMTP OAuth 2.0 Access Token (only used if UseSeparateSmtpAccount = true)
    /// </summary>
    public string? SmtpSeparateOAuthAccessToken { get; set; }

    /// <summary>
    /// Separate SMTP OAuth 2.0 Refresh Token (only used if UseSeparateSmtpAccount = true)
    /// </summary>
    public string? SmtpSeparateOAuthRefreshToken { get; set; }

    /// <summary>
    /// Separate SMTP OAuth token expiry (only used if UseSeparateSmtpAccount = true)
    /// </summary>
    public DateTime? SmtpSeparateOAuthTokenExpiresAt { get; set; }

    /// <summary>
    /// Separate SMTP OAuth scopes (only used if UseSeparateSmtpAccount = true)
    /// </summary>
    public string? SmtpSeparateOAuthScopes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
