namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Communication channels supported by the system
/// </summary>
public enum CommunicationChannel
{
    /// <summary>
    /// Email
    /// </summary>
    Email = 0,

    /// <summary>
    /// SMS text message
    /// </summary>
    SMS = 1,

    /// <summary>
    /// WhatsApp message
    /// </summary>
    WhatsApp = 2,

    /// <summary>
    /// Push notification (mobile/web)
    /// </summary>
    PushNotification = 3,

    /// <summary>
    /// In-app notification
    /// </summary>
    InApp = 4,

    /// <summary>
    /// Slack message
    /// </summary>
    Slack = 5,

    /// <summary>
    /// Microsoft Teams message
    /// </summary>
    Teams = 6
}
