using ComplaintManagement.Domain.Entities.Communication;

namespace ComplaintManagement.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for EmailAttachment entity
/// </summary>
public interface IEmailAttachmentRepository : IRepository<EmailAttachment>
{
    /// <summary>
    /// Get all attachments for a specific email message
    /// </summary>
    Task<IEnumerable<EmailAttachment>> GetByEmailMessageIdAsync(Guid emailMessageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get attachment by Content-ID (for inline images)
    /// </summary>
    Task<EmailAttachment?> GetByContentIdAsync(string contentId, Guid emailMessageId, CancellationToken cancellationToken = default);
}
