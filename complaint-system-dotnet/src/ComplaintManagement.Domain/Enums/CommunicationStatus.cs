namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Status of a communication attempt
/// </summary>
public enum CommunicationStatus
{
    /// <summary>
    /// Queued for sending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Currently being sent
    /// </summary>
    Sending = 1,

    /// <summary>
    /// Successfully sent
    /// </summary>
    Sent = 2,

    /// <summary>
    /// Delivered to recipient
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// Opened/read by recipient
    /// </summary>
    Read = 4,

    /// <summary>
    /// Failed to send
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Bounced back
    /// </summary>
    Bounced = 6,

    /// <summary>
    /// Marked as spam
    /// </summary>
    Spam = 7
}
