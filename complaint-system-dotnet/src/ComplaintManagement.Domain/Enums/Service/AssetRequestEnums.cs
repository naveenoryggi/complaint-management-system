namespace ComplaintManagement.Domain.Enums.Service;

/// <summary>
/// Type of asset request
/// </summary>
public enum AssetRequestType
{
    /// <summary>
    /// Request to return an assigned asset
    /// </summary>
    Return = 1,

    /// <summary>
    /// Request for asset assignment to a user
    /// </summary>
    Assignment = 2,

    /// <summary>
    /// Request to transfer asset between users/stores
    /// </summary>
    Transfer = 3
}

/// <summary>
/// Status of an asset request in the approval workflow
/// </summary>
public enum AssetRequestStatus
{
    /// <summary>
    /// Request saved but not yet submitted
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Request submitted, awaiting initial approval
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Request escalated to secondary approver
    /// </summary>
    Escalated = 2,

    /// <summary>
    /// Request approved and processed
    /// </summary>
    Approved = 3,

    /// <summary>
    /// Request rejected by approver
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Request cancelled by requester
    /// </summary>
    Cancelled = 5
}

/// <summary>
/// Action taken during approval workflow
/// </summary>
public enum ApprovalAction
{
    /// <summary>
    /// Request was submitted
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// Request was approved
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Request was rejected
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Request was escalated to next level
    /// </summary>
    Escalated = 4,

    /// <summary>
    /// Request was reassigned to different approver
    /// </summary>
    Reassigned = 5,

    /// <summary>
    /// Request was cancelled
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Comment added to request
    /// </summary>
    CommentAdded = 7
}

/// <summary>
/// Purpose of an asset assignment
/// </summary>
public enum AssignmentPurpose
{
    /// <summary>
    /// Regular internal use
    /// </summary>
    InternalUse = 1,

    /// <summary>
    /// Demo or evaluation purposes
    /// </summary>
    Demo = 2,

    /// <summary>
    /// Testing or development
    /// </summary>
    Testing = 3,

    /// <summary>
    /// Loaner while main asset is repaired
    /// </summary>
    Loaner = 4,

    /// <summary>
    /// Temporary project assignment
    /// </summary>
    Project = 5,

    /// <summary>
    /// Training purposes
    /// </summary>
    Training = 6
}

/// <summary>
/// Role a user can have in a store
/// </summary>
public enum StoreRole
{
    /// <summary>
    /// Primary manager with full approval authority
    /// </summary>
    PrimaryManager = 1,

    /// <summary>
    /// Secondary manager for escalations
    /// </summary>
    SecondaryManager = 2,

    /// <summary>
    /// Regular store staff
    /// </summary>
    Staff = 3,

    /// <summary>
    /// Store supervisor (between staff and manager)
    /// </summary>
    Supervisor = 4
}
