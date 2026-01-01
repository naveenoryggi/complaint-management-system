using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.DTOs.Contract;

/// <summary>
/// Full contract DTO for API responses
/// </summary>
public class ContractDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? PartnerId { get; set; }
    public string? PartnerName { get; set; }

    // Identity
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ContractType Type { get; set; }
    public string TypeName => Type.ToString();
    public string? Description { get; set; }
    public string? ExternalContractId { get; set; }
    public string? PONumber { get; set; }
    public string? InvoiceNumber { get; set; }

    // Dates
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? SignedDate { get; set; }
    public DateTime? ActivatedDate { get; set; }
    public int DaysRemaining { get; set; }
    public bool IsExpiringSoon { get; set; }

    // Renewal
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public int? RenewalNoticeDays { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public int RenewalCount { get; set; }

    // Financials
    public decimal? ContractValue { get; set; }
    public decimal? AnnualValue { get; set; }
    public decimal? MonthlyValue { get; set; }
    public string Currency { get; set; } = "INR";
    public BillingFrequency BillingFrequency { get; set; }
    public string BillingFrequencyName => BillingFrequency.ToString();
    public string? PaymentTerms { get; set; }

    // Coverage
    public CoverageType CoverageType { get; set; }
    public string CoverageTypeName => CoverageType.ToString();
    public bool IncludesOnsite { get; set; }
    public bool IncludesRemote { get; set; }
    public bool IncludesParts { get; set; }
    public bool IncludesLabor { get; set; }
    public int? IncludedHours { get; set; }
    public int? UsedHours { get; set; }
    public int? RemainingHours { get; set; }
    public int? IncludedIncidents { get; set; }
    public int? UsedIncidents { get; set; }

    // SLA
    public Guid? SLASettingsId { get; set; }
    public string? SLASettingsName { get; set; }
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public SupportHoursType? SupportHoursType { get; set; }
    public string? SupportHoursTypeName => SupportHoursType?.ToString();

    // Status
    public ContractStatus Status { get; set; }
    public string StatusName => Status.ToString();

    // Items
    public int ItemCount { get; set; }
    public List<ContractItemSummaryDto> Items { get; set; } = new();

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Contract summary for list views
/// </summary>
public class ContractSummaryDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ContractType Type { get; set; }
    public string TypeName => Type.ToString();
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? PartnerName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysRemaining { get; set; }
    public bool IsExpiringSoon { get; set; }
    public decimal? ContractValue { get; set; }
    public string Currency { get; set; } = "INR";
    public ContractStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public CoverageType CoverageType { get; set; }
    public string CoverageTypeName => CoverageType.ToString();
    public int ItemCount { get; set; }
    public bool AutoRenew { get; set; }
}

/// <summary>
/// Contract lookup for dropdowns
/// </summary>
public class ContractLookupDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName => $"{ContractNumber} - {Name}";
    public ContractType Type { get; set; }
    public ContractStatus Status { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive => Status == ContractStatus.Active && EndDate > DateTime.UtcNow;
}

/// <summary>
/// Request to create a new contract
/// </summary>
public class CreateContractRequest
{
    public Guid CustomerId { get; set; }
    public Guid? PartnerId { get; set; }

    // Identity
    public string Name { get; set; } = string.Empty;
    public ContractType Type { get; set; }
    public string? Description { get; set; }
    public string? ExternalContractId { get; set; }
    public string? PONumber { get; set; }
    public string? InvoiceNumber { get; set; }

    // Dates
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? SignedDate { get; set; }

    // Renewal
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public int? RenewalNoticeDays { get; set; }

    // Financials
    public decimal? ContractValue { get; set; }
    public decimal? AnnualValue { get; set; }
    public decimal? MonthlyValue { get; set; }
    public string Currency { get; set; } = "INR";
    public BillingFrequency BillingFrequency { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? TaxRate { get; set; }

    // Coverage
    public CoverageType CoverageType { get; set; }
    public bool IncludesOnsite { get; set; }
    public bool IncludesRemote { get; set; } = true;
    public bool IncludesParts { get; set; } = true;
    public bool IncludesLabor { get; set; } = true;
    public bool IncludesTravel { get; set; }
    public bool IncludesConsumables { get; set; }
    public int? IncludedHours { get; set; }
    public int? IncludedIncidents { get; set; }
    public int? IncludedPMVisits { get; set; }
    public string? ExcludedItems { get; set; }
    public string? IncludedItems { get; set; }

    // SLA
    public Guid? SLASettingsId { get; set; }
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public SupportHoursType? SupportHoursType { get; set; }
    public decimal? UptimeGuarantee { get; set; }

    // Documents
    public string? DocumentUrl { get; set; }
    public string? Terms { get; set; }
    public string? SpecialConditions { get; set; }
    public string? Notes { get; set; }

    // Assignment
    public Guid? AccountManagerId { get; set; }
    public Guid? TechnicalManagerId { get; set; }

    // Custom
    public string? CustomFields { get; set; }
    public string? Tags { get; set; }

    // Items
    public List<CreateContractItemRequest>? Items { get; set; }
}

/// <summary>
/// Request to update an existing contract
/// </summary>
public class UpdateContractRequest
{
    public string Name { get; set; } = string.Empty;
    public ContractType Type { get; set; }
    public string? Description { get; set; }
    public string? ExternalContractId { get; set; }
    public string? PONumber { get; set; }
    public string? InvoiceNumber { get; set; }

    // Dates
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? SignedDate { get; set; }

    // Renewal
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public int? RenewalNoticeDays { get; set; }

    // Financials
    public decimal? ContractValue { get; set; }
    public decimal? AnnualValue { get; set; }
    public decimal? MonthlyValue { get; set; }
    public string Currency { get; set; } = "INR";
    public BillingFrequency BillingFrequency { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? TaxRate { get; set; }

    // Coverage
    public CoverageType CoverageType { get; set; }
    public bool IncludesOnsite { get; set; }
    public bool IncludesRemote { get; set; }
    public bool IncludesParts { get; set; }
    public bool IncludesLabor { get; set; }
    public bool IncludesTravel { get; set; }
    public bool IncludesConsumables { get; set; }
    public int? IncludedHours { get; set; }
    public int? IncludedIncidents { get; set; }
    public int? IncludedPMVisits { get; set; }
    public string? ExcludedItems { get; set; }
    public string? IncludedItems { get; set; }

    // SLA
    public Guid? SLASettingsId { get; set; }
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public SupportHoursType? SupportHoursType { get; set; }
    public decimal? UptimeGuarantee { get; set; }

    // Documents
    public string? DocumentUrl { get; set; }
    public string? Terms { get; set; }
    public string? SpecialConditions { get; set; }
    public string? Notes { get; set; }

    // Assignment
    public Guid? AccountManagerId { get; set; }
    public Guid? TechnicalManagerId { get; set; }

    // Custom
    public string? CustomFields { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Request to activate a contract
/// </summary>
public class ActivateContractRequest
{
    public DateTime? ActivatedDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to renew a contract
/// </summary>
public class RenewContractRequest
{
    public DateTime NewStartDate { get; set; }
    public DateTime NewEndDate { get; set; }
    public int RenewalPeriodMonths { get; set; }
    public decimal? NewValue { get; set; }
    public decimal? PriceChangePercent { get; set; }
    public decimal? RenewalDiscount { get; set; }
    public string? PONumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public ContractType? NewType { get; set; }
    public CoverageType? NewCoverageType { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to terminate a contract
/// </summary>
public class TerminateContractRequest
{
    public DateTime TerminatedDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// Search request for contracts
/// </summary>
public class ContractSearchRequest
{
    public string? SearchTerm { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? PartnerId { get; set; }
    public ContractType? Type { get; set; }
    public ContractStatus? Status { get; set; }
    public CoverageType? CoverageType { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public bool? ExpiringWithinDays { get; set; }
    public int? ExpiringDays { get; set; } = 30;
    public bool? AutoRenew { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "EndDate";
    public bool SortDescending { get; set; }
}

/// <summary>
/// Contract coverage check result
/// </summary>
public class CoverageCheckResult
{
    public bool IsCovered { get; set; }
    public Guid? ContractId { get; set; }
    public string? ContractNumber { get; set; }
    public string? ContractName { get; set; }
    public ContractType? ContractType { get; set; }
    public CoverageType? CoverageType { get; set; }
    public DateTime? CoverageEndDate { get; set; }
    public int? DaysRemaining { get; set; }
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public bool? IncludesParts { get; set; }
    public bool? IncludesLabor { get; set; }
    public bool? IncludesOnsite { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Contract statistics
/// </summary>
public class ContractStatisticsDto
{
    public int TotalContracts { get; set; }
    public int ActiveContracts { get; set; }
    public int ExpiredContracts { get; set; }
    public int ExpiringIn30Days { get; set; }
    public int ExpiringIn60Days { get; set; }
    public int ExpiringIn90Days { get; set; }
    public int PendingRenewal { get; set; }
    public decimal TotalContractValue { get; set; }
    public decimal ActiveContractValue { get; set; }
    public Dictionary<string, int> ContractsByType { get; set; } = new();
    public Dictionary<string, int> ContractsByStatus { get; set; } = new();
    public Dictionary<string, int> ContractsByCoverageType { get; set; } = new();
}
