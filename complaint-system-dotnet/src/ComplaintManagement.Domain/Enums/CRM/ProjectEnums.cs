namespace ComplaintManagement.Domain.Enums.CRM;

/// <summary>
/// Project lifecycle status
/// </summary>
public enum ProjectStatus
{
    /// <summary>
    /// Draft - initial state, not yet started
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Planning - in planning phase
    /// </summary>
    Planning = 1,

    /// <summary>
    /// In Progress - actively being worked on
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// On Hold - temporarily paused
    /// </summary>
    OnHold = 3,

    /// <summary>
    /// Completed - successfully finished
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Cancelled - terminated before completion
    /// </summary>
    Cancelled = 5
}

/// <summary>
/// Milestone status
/// </summary>
public enum MilestoneStatus
{
    /// <summary>
    /// Pending - not yet started
    /// </summary>
    Pending = 0,

    /// <summary>
    /// In Progress - currently being worked on
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Completed - finished successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Delayed - past due date
    /// </summary>
    Delayed = 3
}

/// <summary>
/// Project task status
/// </summary>
public enum ProjectTaskStatus
{
    /// <summary>
    /// To Do - not started
    /// </summary>
    ToDo = 0,

    /// <summary>
    /// In Progress - being worked on
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// In Review - pending review
    /// </summary>
    InReview = 2,

    /// <summary>
    /// Completed - finished
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Blocked - cannot proceed
    /// </summary>
    Blocked = 4
}

/// <summary>
/// Task priority level
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Low priority
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium priority
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High priority
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority
    /// </summary>
    Critical = 3
}

/// <summary>
/// Project document category
/// </summary>
public enum DocumentCategory
{
    /// <summary>
    /// Project proposal
    /// </summary>
    Proposal = 0,

    /// <summary>
    /// Contract document
    /// </summary>
    Contract = 1,

    /// <summary>
    /// Technical specification
    /// </summary>
    Specification = 2,

    /// <summary>
    /// Design document
    /// </summary>
    Design = 3,

    /// <summary>
    /// Report/Analysis
    /// </summary>
    Report = 4,

    /// <summary>
    /// Meeting notes
    /// </summary>
    MeetingNotes = 5,

    /// <summary>
    /// Other/Miscellaneous
    /// </summary>
    Other = 6
}
