using ComplaintManagement.Application.Common.Models;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email using the specified or default email server settings
    /// </summary>
    Task<Result> SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        string? from = null,
        string? fromName = null,
        Guid? emailServerSettingsId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email to multiple recipients
    /// </summary>
    Task<Result> SendEmailAsync(
        IEnumerable<string> toAddresses,
        string subject,
        string body,
        bool isHtml = true,
        string? from = null,
        string? fromName = null,
        Guid? emailServerSettingsId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests email server settings by sending a test email
    /// </summary>
    Task<Result> TestEmailServerAsync(
        Guid emailServerSettingsId,
        string testRecipient,
        CancellationToken cancellationToken = default);
}
