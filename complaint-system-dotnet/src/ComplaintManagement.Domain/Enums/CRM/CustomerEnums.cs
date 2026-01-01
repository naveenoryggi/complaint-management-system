namespace ComplaintManagement.Domain.Enums.CRM;

/// <summary>
/// Type of customer relationship
/// </summary>
public enum CustomerType
{
    /// <summary>
    /// Direct customer - no partner involvement
    /// </summary>
    Direct = 1,

    /// <summary>
    /// Partner-managed customer
    /// </summary>
    PartnerManaged = 2,

    /// <summary>
    /// Hybrid - both direct and partner support
    /// </summary>
    Hybrid = 3
}

/// <summary>
/// Customer lifecycle status
/// </summary>
public enum CustomerStatus
{
    /// <summary>
    /// Lead - potential customer
    /// </summary>
    Lead = 0,

    /// <summary>
    /// Active customer
    /// </summary>
    Active = 1,

    /// <summary>
    /// Inactive customer
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Suspended customer
    /// </summary>
    Suspended = 3,

    /// <summary>
    /// Churned - lost customer
    /// </summary>
    Churned = 4
}

/// <summary>
/// Customer location type
/// </summary>
public enum LocationType
{
    /// <summary>
    /// Head office / Corporate office
    /// </summary>
    HeadOffice = 1,

    /// <summary>
    /// Branch office
    /// </summary>
    Branch = 2,

    /// <summary>
    /// Warehouse / Storage facility
    /// </summary>
    Warehouse = 3,

    /// <summary>
    /// Site / Project location
    /// </summary>
    Site = 4,

    /// <summary>
    /// Data Center
    /// </summary>
    DataCenter = 5,

    /// <summary>
    /// Factory / Manufacturing unit
    /// </summary>
    Factory = 6,

    /// <summary>
    /// Retail store
    /// </summary>
    Retail = 7,

    /// <summary>
    /// Remote / Home office
    /// </summary>
    Remote = 8
}

/// <summary>
/// Contact role within the organization
/// </summary>
public enum ContactRole
{
    /// <summary>
    /// Primary contact for the account
    /// </summary>
    Primary = 1,

    /// <summary>
    /// Technical contact
    /// </summary>
    Technical = 2,

    /// <summary>
    /// Billing contact
    /// </summary>
    Billing = 3,

    /// <summary>
    /// Executive/Decision maker
    /// </summary>
    Executive = 4,

    /// <summary>
    /// Support contact
    /// </summary>
    Support = 5,

    /// <summary>
    /// Procurement contact
    /// </summary>
    Procurement = 6,

    /// <summary>
    /// IT Administrator
    /// </summary>
    ITAdmin = 7,

    /// <summary>
    /// End user
    /// </summary>
    EndUser = 8
}

/// <summary>
/// Portal access role
/// </summary>
public enum PortalRole
{
    /// <summary>
    /// Viewer - can only view tickets
    /// </summary>
    Viewer = 1,

    /// <summary>
    /// User - can create and manage own tickets
    /// </summary>
    User = 2,

    /// <summary>
    /// Admin - can manage all organization tickets
    /// </summary>
    Admin = 3
}

/// <summary>
/// Customer segment/size
/// </summary>
public enum CustomerSegment
{
    /// <summary>
    /// Small and Medium Business
    /// </summary>
    SMB = 1,

    /// <summary>
    /// Mid-Market
    /// </summary>
    MidMarket = 2,

    /// <summary>
    /// Enterprise
    /// </summary>
    Enterprise = 3,

    /// <summary>
    /// Government
    /// </summary>
    Government = 4,

    /// <summary>
    /// Education
    /// </summary>
    Education = 5,

    /// <summary>
    /// Non-Profit
    /// </summary>
    NonProfit = 6
}
