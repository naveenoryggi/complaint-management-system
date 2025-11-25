namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Types of actions that can be performed in assignment rules
/// </summary>
public enum AssignmentActionType
{
    /// <summary>
    /// Assign to a specific resource pool
    /// </summary>
    AssignToPool = 1,

    /// <summary>
    /// Escalate to a higher-level resource pool
    /// </summary>
    EscalateToPool = 2,

    /// <summary>
    /// Assign to a specific user
    /// </summary>
    AssignToUser = 3,

    /// <summary>
    /// Escalate to user's manager
    /// </summary>
    EscalateToManager = 4,

    /// <summary>
    /// Assign to organizational contacts (section incharge, branch manager, etc.)
    /// </summary>
    AssignToOrganizationalContact = 5,

    /// <summary>
    /// Keep complaint unassigned but mark for manual review
    /// </summary>
    FlagForManualReview = 6,

    /// <summary>
    /// No assignment - leave as is
    /// </summary>
    NoAction = 7,

    /// <summary>
    /// Create a new assignment task and notify relevant parties
    /// </summary>
    CreateAssignmentTask = 8,

    /// <summary>
    /// Route to external system or team
    /// </summary>
    RouteToExternal = 9
}