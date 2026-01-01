namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Source/channel from which the ticket was created
/// </summary>
public enum TicketSource
{
    /// <summary>
    /// Created by internal users (employees)
    /// </summary>
    Internal = 0,

    /// <summary>
    /// Created via customer portal
    /// </summary>
    Portal = 1,

    /// <summary>
    /// Created via email
    /// </summary>
    Email = 2,

    /// <summary>
    /// Created via phone call
    /// </summary>
    Phone = 3,

    /// <summary>
    /// Created via WhatsApp
    /// </summary>
    WhatsApp = 4,

    /// <summary>
    /// Created via API integration
    /// </summary>
    API = 5,

    /// <summary>
    /// Created via web form
    /// </summary>
    WebForm = 6,

    /// <summary>
    /// Created via mobile app
    /// </summary>
    MobileApp = 7,

    /// <summary>
    /// Created via SMS
    /// </summary>
    SMS = 8,

    /// <summary>
    /// Created via social media
    /// </summary>
    Social = 9
}
