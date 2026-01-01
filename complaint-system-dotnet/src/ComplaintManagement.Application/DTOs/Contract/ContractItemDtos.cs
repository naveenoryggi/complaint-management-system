using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.DTOs.Contract;

/// <summary>
/// Full contract item DTO
/// </summary>
public class ContractItemDto
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public Guid? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public Guid? AssetId { get; set; }
    public string? AssetTag { get; set; }
    public string? SerialNumber { get; set; }

    // Item Details
    public string ItemDescription { get; set; } = string.Empty;
    public string? SerialNumbers { get; set; }
    public int Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }

    // Coverage
    public DateTime? CoverageStartDate { get; set; }
    public DateTime? CoverageEndDate { get; set; }
    public bool IsCovered { get; set; }
    public int? DaysRemaining { get; set; }

    // Financials
    public decimal? ItemValue { get; set; }
    public decimal? DiscountPercent { get; set; }
    public string Currency { get; set; } = "INR";

    // Service Level
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public int? PriorityLevel { get; set; }

    // Status
    public ContractItemStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public bool IsActive { get; set; }

    // Location
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? SiteInfo { get; set; }

    // Maintenance
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public int? MaintenanceIntervalDays { get; set; }
    public int ServiceCallCount { get; set; }

    // Notes
    public string? SpecialConditions { get; set; }
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Contract item summary for lists
/// </summary>
public class ContractItemSummaryDto
{
    public Guid Id { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? AssetTag { get; set; }
    public string? SerialNumber { get; set; }
    public int Quantity { get; set; }
    public DateTime? CoverageEndDate { get; set; }
    public bool IsCovered { get; set; }
    public ContractItemStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public bool IsActive { get; set; }
}

/// <summary>
/// Request to create a contract item
/// </summary>
public class CreateContractItemRequest
{
    public Guid? ProductId { get; set; }
    public Guid? AssetId { get; set; }

    // Item Details
    public string ItemDescription { get; set; } = string.Empty;
    public string? SerialNumbers { get; set; }
    public int Quantity { get; set; } = 1;
    public string? UnitOfMeasure { get; set; }

    // Coverage
    public DateTime? CoverageStartDate { get; set; }
    public DateTime? CoverageEndDate { get; set; }

    // Financials
    public decimal? ItemValue { get; set; }
    public decimal? DiscountPercent { get; set; }
    public string Currency { get; set; } = "INR";

    // Service Level Override
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public int? PriorityLevel { get; set; }

    // Location
    public Guid? LocationId { get; set; }
    public string? SiteInfo { get; set; }

    // Maintenance
    public int? MaintenanceIntervalDays { get; set; }

    // Notes
    public string? SpecialConditions { get; set; }
    public string? Notes { get; set; }
    public string? CustomFields { get; set; }
}

/// <summary>
/// Request to update a contract item
/// </summary>
public class UpdateContractItemRequest
{
    public string ItemDescription { get; set; } = string.Empty;
    public string? SerialNumbers { get; set; }
    public int Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }

    // Coverage
    public DateTime? CoverageStartDate { get; set; }
    public DateTime? CoverageEndDate { get; set; }

    // Financials
    public decimal? ItemValue { get; set; }
    public decimal? DiscountPercent { get; set; }

    // Service Level Override
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public int? PriorityLevel { get; set; }

    // Status
    public ContractItemStatus Status { get; set; }
    public bool IsActive { get; set; }

    // Location
    public Guid? LocationId { get; set; }
    public string? SiteInfo { get; set; }

    // Maintenance
    public int? MaintenanceIntervalDays { get; set; }

    // Notes
    public string? SpecialConditions { get; set; }
    public string? Notes { get; set; }
    public string? CustomFields { get; set; }
}
