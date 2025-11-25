using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Domain.Entities.Events;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for dispatching notifications based on system events
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>
    /// Dispatches notifications for a system event
    /// </summary>
    /// <param name="eventCode">Event code (e.g., COMPLAINT_CREATED, COMPLAINT_ASSIGNED)</param>
    /// <param name="entityId">ID of the entity that triggered the event</param>
    /// <param name="data">Event data for template processing</param>
    /// <param name="companyId">Company ID for multi-tenant filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> DispatchEventNotificationsAsync(
        string eventCode,
        Guid entityId,
        Dictionary<string, object> data,
        Guid? companyId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new event type in the system
    /// </summary>
    Task<Result<EventType>> RegisterEventTypeAsync(
        string code,
        string name,
        string entityType,
        string? description = null,
        string? category = null,
        List<string>? availableFields = null,
        Guid? companyId = null,
        CancellationToken cancellationToken = default);
}
