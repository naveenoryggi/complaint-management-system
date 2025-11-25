using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Events;

/// <summary>
/// Logs when events occur in the system
/// </summary>
public class EventLog : BaseEntity
{
    /// <summary>
    /// Event type ID
    /// </summary>
    public Guid EventTypeId { get; set; }

    /// <summary>
    /// Entity ID that triggered the event
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Entity type
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// User who triggered the event (if applicable)
    /// </summary>
    public Guid? TriggeredByUserId { get; set; }

    /// <summary>
    /// Event data/payload (as JSON)
    /// </summary>
    public string? EventData { get; set; }

    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Was this event processed successfully?
    /// </summary>
    public bool IsProcessed { get; set; } = false;

    /// <summary>
    /// Processing error message (if any)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of communication actions triggered
    /// </summary>
    public int ActionsTriggered { get; set; } = 0;

    // Navigation properties
    /// <summary>
    /// Event type definition
    /// </summary>
    public EventType EventType { get; set; } = null!;

    /// <summary>
    /// User who triggered the event
    /// </summary>
    public User? TriggeredByUser { get; set; }
}
