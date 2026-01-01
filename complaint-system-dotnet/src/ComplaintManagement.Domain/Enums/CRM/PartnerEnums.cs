namespace ComplaintManagement.Domain.Enums.CRM;

/// <summary>
/// Type of partner relationship
/// </summary>
public enum PartnerType
{
    /// <summary>
    /// Reseller - sells products/services
    /// </summary>
    Reseller = 1,

    /// <summary>
    /// System Integrator - implements solutions
    /// </summary>
    SystemIntegrator = 2,

    /// <summary>
    /// Distributor - distributes to resellers
    /// </summary>
    Distributor = 3,

    /// <summary>
    /// Affiliate - referral partner
    /// </summary>
    Affiliate = 4,

    /// <summary>
    /// OEM - Original Equipment Manufacturer
    /// </summary>
    OEM = 5,

    /// <summary>
    /// MSP - Managed Service Provider
    /// </summary>
    MSP = 6,

    /// <summary>
    /// Consultant - advisory partner
    /// </summary>
    Consultant = 7
}

/// <summary>
/// Partner tier/level
/// </summary>
public enum PartnerTier
{
    /// <summary>
    /// Standard partner
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Bronze tier
    /// </summary>
    Bronze = 2,

    /// <summary>
    /// Silver tier
    /// </summary>
    Silver = 3,

    /// <summary>
    /// Gold tier
    /// </summary>
    Gold = 4,

    /// <summary>
    /// Platinum tier (highest)
    /// </summary>
    Platinum = 5
}

/// <summary>
/// Partner status
/// </summary>
public enum PartnerStatus
{
    /// <summary>
    /// Prospect - potential partner
    /// </summary>
    Prospect = 0,

    /// <summary>
    /// Active partner
    /// </summary>
    Active = 1,

    /// <summary>
    /// Inactive partner
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Suspended partner
    /// </summary>
    Suspended = 3,

    /// <summary>
    /// Terminated partnership
    /// </summary>
    Terminated = 4
}
