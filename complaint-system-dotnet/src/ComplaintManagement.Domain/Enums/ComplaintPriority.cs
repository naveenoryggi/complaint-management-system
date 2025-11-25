namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Represents the priority level of a complaint
/// </summary>
public enum ComplaintPriority
{
    /// <summary>
    /// Low priority - No immediate action required
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority - Standard processing time
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority - Requires expedited attention
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority - Requires immediate attention
    /// </summary>
    Critical = 3,

    /// <summary>
    /// Urgent priority - Highest priority level
    /// </summary>
    Urgent = 4
}
