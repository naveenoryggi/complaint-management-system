using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Domain.Entities.Complaints;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for sending automated email responses based on complaint events
/// Integrates with EmailTicketingService and NotificationDispatcher
/// </summary>
public interface IAutoResponseService
{
    /// <summary>
    /// Sends auto-acknowledgment email when a complaint is created
    /// Uses template system with variable substitution
    /// </summary>
    /// <param name="complaint">The newly created complaint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendComplaintCreatedAutoResponseAsync(
        Complaint complaint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends status change notification to complainant
    /// Different templates for different status transitions
    /// </summary>
    /// <param name="complaint">The complaint with updated status</param>
    /// <param name="oldStatusName">Previous status name</param>
    /// <param name="newStatusName">New status name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendStatusChangeAutoResponseAsync(
        Complaint complaint,
        string oldStatusName,
        string newStatusName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends assignment notification to the assigned handler
    /// Optionally notifies the previous handler
    /// </summary>
    /// <param name="complaint">The complaint being assigned</param>
    /// <param name="previousHandlerId">Previous handler user ID (if reassignment)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendAssignmentAutoResponseAsync(
        Complaint complaint,
        Guid? previousHandlerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends resolution notification to complainant
    /// Includes resolution details
    /// </summary>
    /// <param name="complaint">The resolved complaint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendResolutionAutoResponseAsync(
        Complaint complaint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends comment notification to relevant parties
    /// Configurable based on comment type/visibility
    /// </summary>
    /// <param name="complaint">The complaint with new comment</param>
    /// <param name="commentText">Comment text</param>
    /// <param name="isInternal">Whether comment is internal (not sent to complainant)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendCommentAutoResponseAsync(
        Complaint complaint,
        string commentText,
        bool isInternal = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends escalation notification to relevant parties
    /// Notifies escalated handlers and managers
    /// </summary>
    /// <param name="complaint">The escalated complaint</param>
    /// <param name="escalationLevel">New escalation level</param>
    /// <param name="escalationReason">Reason for escalation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendEscalationAutoResponseAsync(
        Complaint complaint,
        int escalationLevel,
        string escalationReason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends SLA breach warning notification
    /// Alerts when complaint is approaching or has breached SLA
    /// </summary>
    /// <param name="complaint">The complaint approaching/breaching SLA</param>
    /// <param name="isBreach">True if already breached, false if warning</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendSLAWarningAutoResponseAsync(
        Complaint complaint,
        bool isBreach = false,
        CancellationToken cancellationToken = default);
}
