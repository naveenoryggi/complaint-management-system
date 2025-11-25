namespace ComplaintManagement.Application.DTOs.Email;

/// <summary>
/// Type of email reply action
/// </summary>
public enum ReplyType
{
    /// <summary>
    /// Reply to sender only
    /// </summary>
    Reply = 1,

    /// <summary>
    /// Reply to all participants (To + CC)
    /// </summary>
    ReplyAll = 2,

    /// <summary>
    /// Forward to new recipients
    /// </summary>
    Forward = 3,

    /// <summary>
    /// New email in thread
    /// </summary>
    NewEmail = 4,

    /// <summary>
    /// Internal note only (not sent via email)
    /// </summary>
    PrivateNote = 5
}
