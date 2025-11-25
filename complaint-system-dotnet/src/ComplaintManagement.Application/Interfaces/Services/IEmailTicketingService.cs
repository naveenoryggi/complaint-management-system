using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Domain.Entities.Communication;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for email ticketing operations (IMAP fetching, threading, ticket creation)
/// </summary>
public interface IEmailTicketingService
{
    /// <summary>
    /// Fetches new emails from IMAP server and processes them into tickets
    /// </summary>
    Task<Result<EmailProcessingResult>> FetchAndProcessEmailsAsync(
        Guid emailConfigurationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email reply from a ticket/complaint
    /// </summary>
    Task<Result<Guid>> SendTicketReplyAsync(
        Guid complaintId,
        string toEmail,
        string subject,
        string body,
        bool isHtml,
        string? ccEmails = null,
        string? bccEmails = null,
        Guid? sendByUserId = null,
        bool isInternal = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an auto-acknowledgement email for a new ticket
    /// </summary>
    Task<Result> SendAutoAcknowledgementAsync(
        Guid complaintId,
        string toEmail,
        EmailConfiguration config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests IMAP connection
    /// </summary>
    Task<Result> TestImapConnectionAsync(
        EmailConfiguration config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests SMTP connection
    /// </summary>
    Task<Result> TestSmtpConnectionAsync(
        EmailConfiguration config,
        string testRecipient,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes email threading - finds parent email/complaint based on headers
    /// </summary>
    Task<Guid?> FindParentComplaintFromThreadingAsync(
        string messageId,
        string? inReplyTo,
        string? references,
        string subject,
        Guid companyId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of email processing operation
/// </summary>
public class EmailProcessingResult
{
    public int TotalEmailsFetched { get; set; }
    public int NewTicketsCreated { get; set; }
    public int ExistingTicketsUpdated { get; set; }
    public int EmailsIgnored { get; set; }
    public int EmailsFailed { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
