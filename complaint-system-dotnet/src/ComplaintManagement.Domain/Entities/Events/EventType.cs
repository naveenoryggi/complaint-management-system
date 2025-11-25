using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Events;

/// <summary>
/// Defines system events that can trigger actions
/// </summary>
public class EventType : BaseEntity
{
    /// <summary>
    /// Event name (e.g., "Complaint Created", "Status Changed")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Event code for programmatic reference (e.g., "COMPLAINT_CREATED", "STATUS_CHANGED")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of when this event occurs
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Entity type this event applies to (e.g., "Complaint", "User")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Category for grouping events
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Is this event active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is this a system event (cannot be deleted)?
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Available data fields for this event (as JSON)
    /// Example: {"complaintNumber": "string", "status": "string", "priority": "string"}
    /// </summary>
    public string? AvailableFields { get; set; }

    /// <summary>
    /// Icon class for UI display
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Company ID (null = global/system event)
    /// </summary>
    public Guid? CompanyId { get; set; }

    // Navigation properties
    /// <summary>
    /// Company this event belongs to
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Communication rules triggered by this event
    /// </summary>
    public ICollection<EventCommunicationRule> CommunicationRules { get; set; } = new List<EventCommunicationRule>();
}
