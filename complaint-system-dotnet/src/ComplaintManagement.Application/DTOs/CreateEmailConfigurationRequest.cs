using ComplaintManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs;

/// <summary>
/// DTO for creating a new email configuration
/// </summary>
public class CreateEmailConfigurationRequest
{
    [Required]
    public string FromName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    // Authentication Type
    public EmailAuthenticationType AuthenticationType { get; set; } = EmailAuthenticationType.OAuth2;

    // IMAP Settings
    [Required]
    public string ImapHost { get; set; } = string.Empty;

    public int ImapPort { get; set; } = 993;

    public bool ImapUseSsl { get; set; } = true;

    public string ImapUsername { get; set; } = string.Empty; // Not required for OAuth

    public string ImapPassword { get; set; } = string.Empty; // Not required for OAuth

    public string ImapFolder { get; set; } = "INBOX";

    // SMTP Settings
    [Required]
    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public bool SmtpUseSsl { get; set; } = true;

    public string SmtpUsername { get; set; } = string.Empty; // Not required for OAuth

    public string SmtpPassword { get; set; } = string.Empty; // Not required for OAuth

    // OAuth 2.0 Settings (for Office 365, Gmail, etc.)
    public string? OAuthClientId { get; set; }
    public string? OAuthClientSecret { get; set; }
    public string? OAuthTenantId { get; set; } // For Azure AD/Office 365

    /// <summary>
    /// How often to refresh OAuth tokens for this email account (in minutes, 5-120).
    /// If null, uses system-wide default from SystemConfiguration.
    /// Recommended: Set to half the token's expiry time (e.g., 30 min for 1-hour tokens).
    /// </summary>
    [Range(5, 120, ErrorMessage = "OAuth token refresh interval must be between 5 and 120 minutes")]
    public int? OAuthTokenRefreshIntervalMinutes { get; set; }

    // Configuration
    public int PollingIntervalMinutes { get; set; } = 5;
    public bool IsEnabled { get; set; } = true;
    public bool SendAutoAcknowledgement { get; set; } = true;
    public string? AutoAcknowledgementTemplateId { get; set; }
    public bool EnableThreading { get; set; } = true;
    public int ThreadTimeoutDays { get; set; } = 30;
    public long MaxAttachmentSizeBytes { get; set; } = 10485760;
    public string AllowedAttachmentExtensions { get; set; } = ".pdf,.doc,.docx,.txt,.png,.jpg,.jpeg,.gif";
}
