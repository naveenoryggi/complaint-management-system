namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Defines the types of licenses available.
/// </summary>
public enum LicenseType
{
    /// <summary>
    /// Trial license with limited features and time
    /// </summary>
    Trial = 0,

    /// <summary>
    /// Basic license with core features
    /// </summary>
    Basic = 1,

    /// <summary>
    /// Standard license with common features
    /// </summary>
    Standard = 2,

    /// <summary>
    /// Professional license with advanced features
    /// </summary>
    Professional = 3,

    /// <summary>
    /// Enterprise license with all features
    /// </summary>
    Enterprise = 4,

    /// <summary>
    /// Custom license with specific feature set
    /// </summary>
    Custom = 5
}

/// <summary>
/// License status enumeration
/// </summary>
public enum LicenseStatus
{
    /// <summary>
    /// License is pending activation
    /// </summary>
    Pending = 0,

    /// <summary>
    /// License is active and valid
    /// </summary>
    Active = 1,

    /// <summary>
    /// License has expired
    /// </summary>
    Expired = 2,

    /// <summary>
    /// License has been suspended
    /// </summary>
    Suspended = 3,

    /// <summary>
    /// License has been revoked
    /// </summary>
    Revoked = 4,

    /// <summary>
    /// License is in grace period after expiration
    /// </summary>
    GracePeriod = 5
}
