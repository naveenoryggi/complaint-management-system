namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Preferred contact method for complainant
/// </summary>
public enum PreferredContactMethod
{
    /// <summary>
    /// Contact via email only
    /// </summary>
    Email = 0,

    /// <summary>
    /// Contact via phone only
    /// </summary>
    Phone = 1,

    /// <summary>
    /// Contact via both email and phone
    /// </summary>
    Both = 2,

    /// <summary>
    /// Contact via SMS
    /// </summary>
    SMS = 3,

    /// <summary>
    /// Contact via in-app notification only
    /// </summary>
    InApp = 4
}
