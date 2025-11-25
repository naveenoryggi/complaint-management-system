using ComplaintManagement.Domain.Entities.Communication;

namespace ComplaintManagement.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for EmailMessage entity
/// </summary>
public interface IEmailMessageRepository : IRepository<EmailMessage>
{
    /// <summary>
    /// Get all email messages for a specific complaint
    /// </summary>
    Task<IEnumerable<EmailMessage>> GetByComplaintIdAsync(Guid complaintId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get email message by Message-ID header
    /// </summary>
    Task<EmailMessage?> GetByMessageIdAsync(string messageId, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get last inbound email for a complaint (for threading)
    /// </summary>
    Task<EmailMessage?> GetLastInboundEmailForComplaintAsync(Guid complaintId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get email messages by thread ID
    /// </summary>
    Task<IEnumerable<EmailMessage>> GetByThreadIdAsync(Guid threadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get email statistics for a company
    /// </summary>
    Task<EmailStatisticsData> GetEmailStatisticsAsync(Guid companyId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Email statistics data model
/// </summary>
public class EmailStatisticsData
{
    public int TotalEmails { get; set; }
    public int InboundEmails { get; set; }
    public int OutboundEmails { get; set; }
    public int ProcessedEmails { get; set; }
    public int FailedEmails { get; set; }
    public int SpamEmails { get; set; }
    public int InternalNotes { get; set; }
    public int EmailsLast24Hours { get; set; }
    public int EmailsLast7Days { get; set; }
    public int EmailsLast30Days { get; set; }
}
