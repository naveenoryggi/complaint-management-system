namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Represents the status of an escalation
/// </summary>
public enum EscalationStatus
{
    /// <summary>
    /// Escalation is pending (scheduled but not triggered)
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Escalation has been triggered
    /// </summary>
    Triggered = 1,

    /// <summary>
    /// Escalation has been acknowledged by the handler
    /// </summary>
    Acknowledged = 2,

    /// <summary>
    /// Escalation has been resolved
    /// </summary>
    Resolved = 3,

    /// <summary>
    /// Escalation has been cancelled/skipped
    /// </summary>
    Cancelled = 4
}
