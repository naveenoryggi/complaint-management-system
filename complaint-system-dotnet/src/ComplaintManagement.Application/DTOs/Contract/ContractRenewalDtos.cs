using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.DTOs.Contract;

/// <summary>
/// Contract renewal history DTO
/// </summary>
public class ContractRenewalHistoryDto
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public int RenewalNumber { get; set; }

    // Period
    public DateTime PreviousEndDate { get; set; }
    public DateTime NewStartDate { get; set; }
    public DateTime NewEndDate { get; set; }
    public int RenewalPeriodMonths { get; set; }

    // Action
    public RenewalAction Action { get; set; }
    public string ActionName => Action.ToString();
    public DateTime RenewalDate { get; set; }
    public Guid? ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }

    // Financials
    public decimal? PreviousValue { get; set; }
    public decimal? NewValue { get; set; }
    public decimal? PriceChangePercent { get; set; }
    public string Currency { get; set; } = "INR";
    public decimal? RenewalDiscount { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? PONumber { get; set; }

    // Changes
    public ContractType? PreviousType { get; set; }
    public ContractType? NewType { get; set; }
    public CoverageType? PreviousCoverageType { get; set; }
    public CoverageType? NewCoverageType { get; set; }
    public string? ChangesSummary { get; set; }

    // Communication
    public DateTime? NoticeSentDate { get; set; }
    public DateTime? CustomerResponseDate { get; set; }
    public string? CustomerResponse { get; set; }

    // Notes
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Renewal history summary for list views
/// </summary>
public class ContractRenewalSummaryDto
{
    public Guid Id { get; set; }
    public int RenewalNumber { get; set; }
    public RenewalAction Action { get; set; }
    public string ActionName => Action.ToString();
    public DateTime RenewalDate { get; set; }
    public DateTime NewStartDate { get; set; }
    public DateTime NewEndDate { get; set; }
    public decimal? PreviousValue { get; set; }
    public decimal? NewValue { get; set; }
    public decimal? PriceChangePercent { get; set; }
    public string Currency { get; set; } = "INR";
    public string? ProcessedByName { get; set; }
}

/// <summary>
/// Expiring contracts report
/// </summary>
public class ExpiringContractsReportDto
{
    public int TotalExpiring { get; set; }
    public decimal TotalValue { get; set; }
    public string Currency { get; set; } = "INR";
    public List<ExpiringContractDto> Contracts { get; set; } = new();
}

/// <summary>
/// Individual expiring contract
/// </summary>
public class ExpiringContractDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public ContractType Type { get; set; }
    public string TypeName => Type.ToString();
    public DateTime EndDate { get; set; }
    public int DaysRemaining { get; set; }
    public decimal? ContractValue { get; set; }
    public string Currency { get; set; } = "INR";
    public bool AutoRenew { get; set; }
    public ContractStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public Guid? AccountManagerId { get; set; }
    public string? AccountManagerName { get; set; }
    public DateTime? LastContactDate { get; set; }
    public string? RenewalNotes { get; set; }
}

/// <summary>
/// Renewal reminder settings
/// </summary>
public class RenewalReminderSettings
{
    public int FirstReminderDays { get; set; } = 90;
    public int SecondReminderDays { get; set; } = 60;
    public int ThirdReminderDays { get; set; } = 30;
    public int FinalReminderDays { get; set; } = 7;
    public bool SendCustomerReminders { get; set; } = true;
    public bool SendAccountManagerReminders { get; set; } = true;
    public string? ReminderEmailTemplate { get; set; }
}
