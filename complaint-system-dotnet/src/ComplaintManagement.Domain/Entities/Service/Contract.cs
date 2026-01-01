using ComplaintManagement.Domain.Entities.CRM;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.SLA;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Service contract entity representing AMC, Warranty, SLA, or Support agreements
/// </summary>
public class Contract : BaseEntity
{
    /// <summary>
    /// Company that owns this contract
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Customer this contract is for
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Partner who sold/manages this contract (optional)
    /// </summary>
    public Guid? PartnerId { get; set; }

    #region Identity

    /// <summary>
    /// Auto-generated contract number (e.g., CTR-2024-00001)
    /// </summary>
    public string ContractNumber { get; set; } = string.Empty;

    /// <summary>
    /// Contract name/title
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of contract
    /// </summary>
    public ContractType Type { get; set; }

    /// <summary>
    /// Detailed description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Reference to external contract/agreement
    /// </summary>
    public string? ExternalContractId { get; set; }

    /// <summary>
    /// Purchase order number
    /// </summary>
    public string? PONumber { get; set; }

    /// <summary>
    /// Invoice number
    /// </summary>
    public string? InvoiceNumber { get; set; }

    #endregion

    #region Dates

    /// <summary>
    /// Contract start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Contract end date
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Date when contract was signed
    /// </summary>
    public DateTime? SignedDate { get; set; }

    /// <summary>
    /// Date when contract was activated
    /// </summary>
    public DateTime? ActivatedDate { get; set; }

    /// <summary>
    /// Date of last service under this contract
    /// </summary>
    public DateTime? LastServiceDate { get; set; }

    #endregion

    #region Renewal

    /// <summary>
    /// Whether contract auto-renews
    /// </summary>
    public bool AutoRenew { get; set; }

    /// <summary>
    /// Renewal period in months
    /// </summary>
    public int? RenewalPeriodMonths { get; set; }

    /// <summary>
    /// Days before expiry to send renewal notice
    /// </summary>
    public int? RenewalNoticeDays { get; set; }

    /// <summary>
    /// Date of last renewal
    /// </summary>
    public DateTime? LastRenewalDate { get; set; }

    /// <summary>
    /// Next scheduled renewal date
    /// </summary>
    public DateTime? NextRenewalDate { get; set; }

    /// <summary>
    /// Number of times renewed
    /// </summary>
    public int RenewalCount { get; set; }

    /// <summary>
    /// Maximum number of renewals allowed (null = unlimited)
    /// </summary>
    public int? MaxRenewals { get; set; }

    #endregion

    #region Financials

    /// <summary>
    /// Total contract value
    /// </summary>
    public decimal? ContractValue { get; set; }

    /// <summary>
    /// Annual contract value (for multi-year)
    /// </summary>
    public decimal? AnnualValue { get; set; }

    /// <summary>
    /// Monthly contract value
    /// </summary>
    public decimal? MonthlyValue { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "INR";

    /// <summary>
    /// Billing frequency
    /// </summary>
    public BillingFrequency BillingFrequency { get; set; }

    /// <summary>
    /// Payment terms (e.g., Net 30)
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Tax rate percentage
    /// </summary>
    public decimal? TaxRate { get; set; }

    /// <summary>
    /// Amount paid so far
    /// </summary>
    public decimal? AmountPaid { get; set; }

    /// <summary>
    /// Outstanding balance
    /// </summary>
    public decimal? OutstandingBalance { get; set; }

    #endregion

    #region Coverage

    /// <summary>
    /// Type of coverage
    /// </summary>
    public CoverageType CoverageType { get; set; }

    /// <summary>
    /// Includes onsite support
    /// </summary>
    public bool IncludesOnsite { get; set; }

    /// <summary>
    /// Includes remote support
    /// </summary>
    public bool IncludesRemote { get; set; }

    /// <summary>
    /// Includes parts replacement
    /// </summary>
    public bool IncludesParts { get; set; }

    /// <summary>
    /// Includes labor
    /// </summary>
    public bool IncludesLabor { get; set; }

    /// <summary>
    /// Includes travel expenses
    /// </summary>
    public bool IncludesTravel { get; set; }

    /// <summary>
    /// Includes consumables
    /// </summary>
    public bool IncludesConsumables { get; set; }

    /// <summary>
    /// Total support hours included
    /// </summary>
    public int? IncludedHours { get; set; }

    /// <summary>
    /// Support hours used
    /// </summary>
    public int? UsedHours { get; set; }

    /// <summary>
    /// Remaining support hours
    /// </summary>
    public int? RemainingHours => IncludedHours.HasValue ? IncludedHours.Value - (UsedHours ?? 0) : null;

    /// <summary>
    /// Number of included incidents
    /// </summary>
    public int? IncludedIncidents { get; set; }

    /// <summary>
    /// Number of incidents used
    /// </summary>
    public int? UsedIncidents { get; set; }

    /// <summary>
    /// Number of preventive maintenance visits included
    /// </summary>
    public int? IncludedPMVisits { get; set; }

    /// <summary>
    /// Number of PM visits completed
    /// </summary>
    public int? CompletedPMVisits { get; set; }

    /// <summary>
    /// JSON list of excluded items/scenarios
    /// </summary>
    public string? ExcludedItems { get; set; }

    /// <summary>
    /// JSON list of included items/scenarios
    /// </summary>
    public string? IncludedItems { get; set; }

    #endregion

    #region SLA

    /// <summary>
    /// Reference to SLA settings
    /// </summary>
    public Guid? SLASettingsId { get; set; }

    /// <summary>
    /// Response time in hours
    /// </summary>
    public int? ResponseTimeHours { get; set; }

    /// <summary>
    /// Resolution time in hours
    /// </summary>
    public int? ResolutionTimeHours { get; set; }

    /// <summary>
    /// Support hours type
    /// </summary>
    public SupportHoursType? SupportHoursType { get; set; }

    /// <summary>
    /// Custom support hours definition (JSON)
    /// </summary>
    public string? SupportHoursDefinition { get; set; }

    /// <summary>
    /// Uptime guarantee percentage
    /// </summary>
    public decimal? UptimeGuarantee { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Current contract status
    /// </summary>
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    /// <summary>
    /// Reason for termination (if terminated)
    /// </summary>
    public string? TerminationReason { get; set; }

    /// <summary>
    /// Date of termination
    /// </summary>
    public DateTime? TerminatedDate { get; set; }

    /// <summary>
    /// User who terminated the contract
    /// </summary>
    public Guid? TerminatedBy { get; set; }

    #endregion

    #region Documents

    /// <summary>
    /// URL to contract document
    /// </summary>
    public string? DocumentUrl { get; set; }

    /// <summary>
    /// JSON array of additional document URLs
    /// </summary>
    public string? AdditionalDocuments { get; set; }

    /// <summary>
    /// Contract terms and conditions (JSON)
    /// </summary>
    public string? Terms { get; set; }

    /// <summary>
    /// Special conditions or notes
    /// </summary>
    public string? SpecialConditions { get; set; }

    /// <summary>
    /// Internal notes
    /// </summary>
    public string? Notes { get; set; }

    #endregion

    #region Assignment

    /// <summary>
    /// Primary account manager for this contract
    /// </summary>
    public Guid? AccountManagerId { get; set; }

    /// <summary>
    /// Technical account manager
    /// </summary>
    public Guid? TechnicalManagerId { get; set; }

    #endregion

    #region Custom Fields

    /// <summary>
    /// JSON for custom fields
    /// </summary>
    public string? CustomFields { get; set; }

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public string? Tags { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Company that owns this contract
    /// </summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>
    /// Customer for this contract
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Partner who sold this contract
    /// </summary>
    public virtual Partner? Partner { get; set; }

    /// <summary>
    /// SLA settings for this contract
    /// </summary>
    public virtual SLASettings? SLASettings { get; set; }

    /// <summary>
    /// Items covered under this contract
    /// </summary>
    public virtual ICollection<ContractItem> Items { get; set; } = new List<ContractItem>();

    /// <summary>
    /// Renewal history
    /// </summary>
    public virtual ICollection<ContractRenewalHistory> RenewalHistory { get; set; } = new List<ContractRenewalHistory>();

    #endregion
}
