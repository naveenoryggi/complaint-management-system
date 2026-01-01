namespace ComplaintManagement.Domain.Enums.Service;

/// <summary>
/// Type of service contract
/// </summary>
public enum ContractType
{
    /// <summary>
    /// Standard manufacturer warranty
    /// </summary>
    Warranty = 1,

    /// <summary>
    /// Annual Maintenance Contract
    /// </summary>
    AMC = 2,

    /// <summary>
    /// Service Level Agreement
    /// </summary>
    SLA = 3,

    /// <summary>
    /// Support pack (hours-based)
    /// </summary>
    SupportPack = 4,

    /// <summary>
    /// Subscription-based service
    /// </summary>
    Subscription = 5,

    /// <summary>
    /// Extended warranty beyond standard
    /// </summary>
    ExtendedWarranty = 6,

    /// <summary>
    /// Comprehensive maintenance agreement
    /// </summary>
    ComprehensiveAMC = 7,

    /// <summary>
    /// Non-comprehensive (parts only)
    /// </summary>
    NonComprehensiveAMC = 8
}

/// <summary>
/// Status of a contract
/// </summary>
public enum ContractStatus
{
    /// <summary>
    /// Contract is being drafted
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Awaiting approval/signature
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Contract is active and in effect
    /// </summary>
    Active = 2,

    /// <summary>
    /// Contract has expired naturally
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Contract was renewed
    /// </summary>
    Renewed = 4,

    /// <summary>
    /// Contract was terminated early
    /// </summary>
    Terminated = 5,

    /// <summary>
    /// Contract was cancelled before activation
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Contract is suspended temporarily
    /// </summary>
    Suspended = 7,

    /// <summary>
    /// Contract is pending renewal
    /// </summary>
    PendingRenewal = 8
}

/// <summary>
/// Type of coverage provided by contract
/// </summary>
public enum CoverageType
{
    /// <summary>
    /// Full coverage - parts, labor, and travel
    /// </summary>
    FullCoverage = 1,

    /// <summary>
    /// Parts only - labor charged separately
    /// </summary>
    PartsOnly = 2,

    /// <summary>
    /// Labor only - parts charged separately
    /// </summary>
    LaborOnly = 3,

    /// <summary>
    /// Extended warranty coverage
    /// </summary>
    ExtendedWarranty = 4,

    /// <summary>
    /// Preventive maintenance only
    /// </summary>
    PreventiveMaintenance = 5,

    /// <summary>
    /// Corrective maintenance only
    /// </summary>
    CorrectiveMaintenance = 6,

    /// <summary>
    /// Software support only
    /// </summary>
    SoftwareSupport = 7,

    /// <summary>
    /// Hardware support only
    /// </summary>
    HardwareSupport = 8,

    /// <summary>
    /// Comprehensive (parts + labor + consumables)
    /// </summary>
    Comprehensive = 9
}

/// <summary>
/// Billing frequency for contracts
/// </summary>
public enum BillingFrequency
{
    /// <summary>
    /// One-time payment
    /// </summary>
    OneTime = 0,

    /// <summary>
    /// Monthly billing
    /// </summary>
    Monthly = 1,

    /// <summary>
    /// Quarterly billing
    /// </summary>
    Quarterly = 2,

    /// <summary>
    /// Semi-annual billing
    /// </summary>
    SemiAnnual = 3,

    /// <summary>
    /// Annual billing
    /// </summary>
    Annual = 4,

    /// <summary>
    /// Bi-annual (every 2 years)
    /// </summary>
    BiAnnual = 5
}

/// <summary>
/// Status of contract item coverage
/// </summary>
public enum ContractItemStatus
{
    /// <summary>
    /// Item is covered under contract
    /// </summary>
    Covered = 1,

    /// <summary>
    /// Coverage is pending activation
    /// </summary>
    PendingActivation = 2,

    /// <summary>
    /// Coverage has expired
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Coverage was terminated
    /// </summary>
    Terminated = 4,

    /// <summary>
    /// Item was replaced
    /// </summary>
    Replaced = 5,

    /// <summary>
    /// Coverage is suspended
    /// </summary>
    Suspended = 6
}

/// <summary>
/// Type of warranty
/// </summary>
public enum WarrantyType
{
    /// <summary>
    /// Standard manufacturer warranty
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Extended warranty (purchased)
    /// </summary>
    Extended = 2,

    /// <summary>
    /// Limited warranty with exclusions
    /// </summary>
    Limited = 3,

    /// <summary>
    /// Lifetime warranty
    /// </summary>
    Lifetime = 4,

    /// <summary>
    /// Prorated warranty (value decreases over time)
    /// </summary>
    Prorated = 5,

    /// <summary>
    /// Express warranty (specific promise)
    /// </summary>
    Express = 6,

    /// <summary>
    /// Implied warranty (legal protection)
    /// </summary>
    Implied = 7
}

/// <summary>
/// Result of warranty claim
/// </summary>
public enum WarrantyClaimResult
{
    /// <summary>
    /// Claim is pending review
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Claim approved - repair authorized
    /// </summary>
    ApprovedRepair = 1,

    /// <summary>
    /// Claim approved - replacement authorized
    /// </summary>
    ApprovedReplacement = 2,

    /// <summary>
    /// Claim approved - refund authorized
    /// </summary>
    ApprovedRefund = 3,

    /// <summary>
    /// Claim denied - not covered
    /// </summary>
    DeniedNotCovered = 4,

    /// <summary>
    /// Claim denied - warranty expired
    /// </summary>
    DeniedExpired = 5,

    /// <summary>
    /// Claim denied - misuse/damage
    /// </summary>
    DeniedMisuse = 6,

    /// <summary>
    /// Claim denied - unauthorized modification
    /// </summary>
    DeniedModification = 7,

    /// <summary>
    /// Claim partially approved
    /// </summary>
    PartiallyApproved = 8
}

/// <summary>
/// Renewal action taken
/// </summary>
public enum RenewalAction
{
    /// <summary>
    /// Contract was auto-renewed
    /// </summary>
    AutoRenewed = 1,

    /// <summary>
    /// Contract was manually renewed
    /// </summary>
    ManualRenewal = 2,

    /// <summary>
    /// Renewal was declined by customer
    /// </summary>
    Declined = 3,

    /// <summary>
    /// Renewal pending customer decision
    /// </summary>
    Pending = 4,

    /// <summary>
    /// Contract upgraded during renewal
    /// </summary>
    Upgraded = 5,

    /// <summary>
    /// Contract downgraded during renewal
    /// </summary>
    Downgraded = 6,

    /// <summary>
    /// Contract lapsed without renewal
    /// </summary>
    Lapsed = 7
}

/// <summary>
/// Support hours type
/// </summary>
public enum SupportHoursType
{
    /// <summary>
    /// 8x5 - Business hours, weekdays
    /// </summary>
    BusinessHours = 1,

    /// <summary>
    /// 12x5 - Extended hours, weekdays
    /// </summary>
    ExtendedHours = 2,

    /// <summary>
    /// 24x5 - Round the clock, weekdays
    /// </summary>
    TwentyFourByFive = 3,

    /// <summary>
    /// 8x7 - Business hours, all days
    /// </summary>
    BusinessHoursAllDays = 4,

    /// <summary>
    /// 12x7 - Extended hours, all days
    /// </summary>
    ExtendedHoursAllDays = 5,

    /// <summary>
    /// 24x7 - Round the clock, all days
    /// </summary>
    TwentyFourBySeven = 6,

    /// <summary>
    /// Custom hours as defined in contract
    /// </summary>
    Custom = 7
}
