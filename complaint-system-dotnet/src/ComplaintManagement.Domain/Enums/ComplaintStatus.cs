namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Represents the current status of a complaint in its lifecycle
/// </summary>
public enum ComplaintStatus
{
    /// <summary>
    /// Complaint has been submitted but not yet reviewed
    /// </summary>
    Submitted = 0,

    /// <summary>
    /// Complaint is being reviewed by the assigned handler
    /// </summary>
    UnderReview = 1,

    /// <summary>
    /// Complaint is currently being investigated
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Complaint has been escalated to a higher level
    /// </summary>
    Escalated = 3,

    /// <summary>
    /// Complaint is awaiting information from the complainant
    /// </summary>
    PendingInfo = 4,

    /// <summary>
    /// Complaint has been resolved
    /// </summary>
    Resolved = 5,

    /// <summary>
    /// Complaint has been closed (final state)
    /// </summary>
    Closed = 6,

    /// <summary>
    /// Complaint has been rejected/dismissed
    /// </summary>
    Rejected = 7,

    /// <summary>
    /// Complaint has been reopened after closure
    /// </summary>
    Reopened = 8
}
