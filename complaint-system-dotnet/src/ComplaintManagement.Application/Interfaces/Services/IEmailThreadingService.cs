using ComplaintManagement.Application.DTOs.Email;
using ComplaintManagement.Domain.Entities.Communication;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for email threading and reply functionality
/// </summary>
public interface IEmailThreadingService
{
    /// <summary>
    /// Send reply to email from complaint ticket
    /// Supports Reply, Reply All, Forward, and Private Notes
    /// </summary>
    Task<(bool Success, string Message, EmailMessage? EmailMessage)> SendReplyAsync(
        SendEmailReplyRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all participants in a complaint email thread
    /// </summary>
    Task<List<ComplaintEmailParticipant>> GetThreadParticipantsAsync(
        Guid complaintId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark email as read by user
    /// </summary>
    Task MarkAsReadAsync(Guid emailMessageId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark all emails in complaint as read
    /// </summary>
    Task MarkAllAsReadAsync(Guid complaintId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get unread email count for a complaint
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid complaintId, Guid userId, CancellationToken cancellationToken = default);
}
