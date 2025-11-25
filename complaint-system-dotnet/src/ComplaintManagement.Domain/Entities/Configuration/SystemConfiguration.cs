using ComplaintManagement.Domain.Entities;

namespace ComplaintManagement.Domain.Entities.Configuration;

/// <summary>
/// System-wide configuration settings that can be managed from the frontend
/// Singleton pattern - only one record should exist in the database
/// </summary>
public class SystemConfiguration : BaseEntity
{
    /// <summary>
    /// Company this configuration belongs to
    /// Allows different companies to have different system settings in multi-tenant setup
    /// </summary>
    public Guid CompanyId { get; set; }

    // OAuth Token Management Settings

    /// <summary>
    /// How often (in minutes) the background service checks and refreshes OAuth tokens
    /// Default: 30 minutes (recommended for 1-hour token expiry)
    /// Range: 5-120 minutes
    /// </summary>
    public int OAuthTokenRefreshIntervalMinutes { get; set; } = 30;

    /// <summary>
    /// Refresh tokens this many days before expiry
    /// Default: 7 days
    /// Range: 1-30 days
    /// </summary>
    public int OAuthTokenExpiryWarningDays { get; set; } = 7;

    // Email Polling Settings

    /// <summary>
    /// Default polling interval for new email configurations (in seconds)
    /// Can be overridden per email configuration
    /// Default: 300 seconds (5 minutes)
    /// Range: 60-3600 seconds (1 minute - 1 hour)
    /// </summary>
    public int DefaultEmailPollingIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Maximum number of emails to fetch in a single polling operation
    /// Default: 50
    /// Range: 10-500
    /// </summary>
    public int MaxEmailsFetchPerPoll { get; set; } = 50;

    // Auto-Response Settings

    /// <summary>
    /// Enable automatic acknowledgment emails when complaints are created via email
    /// Default: true
    /// </summary>
    public bool AutoResponseEnabled { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts for failed auto-responses
    /// Default: 3
    /// Range: 0-10
    /// </summary>
    public int AutoResponseMaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts (in seconds)
    /// Default: 60 seconds
    /// Range: 30-600 seconds
    /// </summary>
    public int AutoResponseRetryDelaySeconds { get; set; } = 60;

    // Rate Limiting Settings

    /// <summary>
    /// Enable rate limiting for email sending to prevent spam
    /// Default: true
    /// </summary>
    public bool EmailRateLimitingEnabled { get; set; } = true;

    /// <summary>
    /// Maximum emails that can be sent per hour per company
    /// Default: 100
    /// Range: 10-1000
    /// </summary>
    public int MaxEmailsPerHour { get; set; } = 100;

    // Notification Settings

    /// <summary>
    /// Enable email notifications for complaint status changes
    /// Default: true
    /// </summary>
    public bool StatusChangeNotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Enable email notifications for complaint assignments
    /// Default: true
    /// </summary>
    public bool AssignmentNotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Enable email notifications for complaint escalations
    /// Default: true
    /// </summary>
    public bool EscalationNotificationsEnabled { get; set; } = true;

    // Timezone Settings

    /// <summary>
    /// Default timezone for the company (IANA timezone identifier)
    /// Examples: "Asia/Kolkata", "America/New_York", "UTC"
    /// Default: "Asia/Kolkata" (IST)
    /// </summary>
    public string DefaultTimezone { get; set; } = "Asia/Kolkata";

    /// <summary>
    /// Date format for displaying dates
    /// Examples: "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd"
    /// Default: "dd/MM/yyyy"
    /// </summary>
    public string DateFormat { get; set; } = "dd/MM/yyyy";

    /// <summary>
    /// Time format for displaying times
    /// Examples: "HH:mm", "hh:mm tt", "HH:mm:ss"
    /// Default: "hh:mm tt" (12-hour with AM/PM)
    /// </summary>
    public string TimeFormat { get; set; } = "hh:mm tt";

    // Validation

    /// <summary>
    /// Validate configuration values before saving
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        // OAuth Token Refresh Interval
        if (OAuthTokenRefreshIntervalMinutes < 5 || OAuthTokenRefreshIntervalMinutes > 120)
        {
            errors.Add("OAuth Token Refresh Interval must be between 5 and 120 minutes.");
        }

        // OAuth Token Expiry Warning
        if (OAuthTokenExpiryWarningDays < 1 || OAuthTokenExpiryWarningDays > 30)
        {
            errors.Add("OAuth Token Expiry Warning must be between 1 and 30 days.");
        }

        // Email Polling Interval
        if (DefaultEmailPollingIntervalSeconds < 60 || DefaultEmailPollingIntervalSeconds > 3600)
        {
            errors.Add("Default Email Polling Interval must be between 60 and 3600 seconds (1 minute - 1 hour).");
        }

        // Max Emails Fetch
        if (MaxEmailsFetchPerPoll < 10 || MaxEmailsFetchPerPoll > 500)
        {
            errors.Add("Max Emails Fetch Per Poll must be between 10 and 500.");
        }

        // Auto-Response Retry
        if (AutoResponseMaxRetryAttempts < 0 || AutoResponseMaxRetryAttempts > 10)
        {
            errors.Add("Auto-Response Max Retry Attempts must be between 0 and 10.");
        }

        // Auto-Response Retry Delay
        if (AutoResponseRetryDelaySeconds < 30 || AutoResponseRetryDelaySeconds > 600)
        {
            errors.Add("Auto-Response Retry Delay must be between 30 and 600 seconds.");
        }

        // Max Emails Per Hour
        if (MaxEmailsPerHour < 10 || MaxEmailsPerHour > 1000)
        {
            errors.Add("Max Emails Per Hour must be between 10 and 1000.");
        }

        // Timezone
        if (string.IsNullOrWhiteSpace(DefaultTimezone))
        {
            errors.Add("Default Timezone is required.");
        }

        // Date Format
        if (string.IsNullOrWhiteSpace(DateFormat))
        {
            errors.Add("Date Format is required.");
        }

        // Time Format
        if (string.IsNullOrWhiteSpace(TimeFormat))
        {
            errors.Add("Time Format is required.");
        }

        return errors.Count == 0;
    }
}
