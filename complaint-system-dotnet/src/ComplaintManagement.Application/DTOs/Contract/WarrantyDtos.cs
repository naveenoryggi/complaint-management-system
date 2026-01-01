using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.DTOs.Contract;

/// <summary>
/// Full warranty DTO for API responses
/// </summary>
public class WarrantyDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // Identity
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WarrantyType Type { get; set; }
    public string TypeName => Type.ToString();

    // Duration
    public int DurationMonths { get; set; }
    public int? ExtendedDurationMonths { get; set; }
    public int? GracePeriodDays { get; set; }
    public int? MaxDurationMonths { get; set; }

    // Coverage
    public CoverageType CoverageType { get; set; }
    public string CoverageTypeName => CoverageType.ToString();
    public bool IncludesParts { get; set; }
    public bool IncludesLabor { get; set; }
    public bool IncludesOnsite { get; set; }
    public bool IncludesShipping { get; set; }
    public string? CoveredItems { get; set; }
    public string? ExcludedItems { get; set; }
    public string? Terms { get; set; }

    // Conditions
    public bool IsTransferable { get; set; }
    public bool RequiresRegistration { get; set; }
    public int? RegistrationDeadlineDays { get; set; }
    public bool RequiresProofOfPurchase { get; set; }

    // Extended Warranty Pricing
    public decimal? ExtendedWarrantyPrice { get; set; }
    public decimal? ExtendedWarrantyPricePercent { get; set; }
    public string Currency { get; set; } = "INR";

    // Service Levels
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public SupportHoursType? SupportHoursType { get; set; }
    public string? SupportHoursTypeName => SupportHoursType?.ToString();

    // Claims
    public int? MaxClaims { get; set; }
    public decimal? MaxClaimValue { get; set; }
    public decimal? MaxClaimValuePercent { get; set; }
    public decimal? DeductibleAmount { get; set; }

    // Status
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Warranty summary for list views
/// </summary>
public class WarrantySummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public WarrantyType Type { get; set; }
    public string TypeName => Type.ToString();
    public int DurationMonths { get; set; }
    public int? ExtendedDurationMonths { get; set; }
    public CoverageType CoverageType { get; set; }
    public string CoverageTypeName => CoverageType.ToString();
    public bool IncludesParts { get; set; }
    public bool IncludesLabor { get; set; }
    public decimal? ExtendedWarrantyPrice { get; set; }
    public string Currency { get; set; } = "INR";
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public string? ProductName { get; set; }
    public string? CategoryName { get; set; }
}

/// <summary>
/// Warranty lookup for dropdowns
/// </summary>
public class WarrantyLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName => $"{Code} - {Name}";
    public WarrantyType Type { get; set; }
    public int DurationMonths { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Request to create a new warranty
/// </summary>
public class CreateWarrantyRequest
{
    public Guid? ProductId { get; set; }
    public Guid? CategoryId { get; set; }

    // Identity
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WarrantyType Type { get; set; }

    // Duration
    public int DurationMonths { get; set; }
    public int? ExtendedDurationMonths { get; set; }
    public int? GracePeriodDays { get; set; }
    public int? MaxDurationMonths { get; set; }

    // Coverage
    public CoverageType CoverageType { get; set; } = CoverageType.FullCoverage;
    public bool IncludesParts { get; set; } = true;
    public bool IncludesLabor { get; set; } = true;
    public bool IncludesOnsite { get; set; }
    public bool IncludesShipping { get; set; }
    public string? CoveredItems { get; set; }
    public string? ExcludedItems { get; set; }
    public string? Terms { get; set; }

    // Conditions
    public bool IsTransferable { get; set; }
    public bool RequiresRegistration { get; set; }
    public int? RegistrationDeadlineDays { get; set; }
    public bool RequiresProofOfPurchase { get; set; } = true;
    public string? Conditions { get; set; }

    // Extended Warranty Pricing
    public decimal? ExtendedWarrantyPrice { get; set; }
    public decimal? ExtendedWarrantyPricePercent { get; set; }
    public string Currency { get; set; } = "INR";

    // Service Levels
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public SupportHoursType? SupportHoursType { get; set; }

    // Claims
    public int? MaxClaims { get; set; }
    public decimal? MaxClaimValue { get; set; }
    public decimal? MaxClaimValuePercent { get; set; }
    public decimal? DeductibleAmount { get; set; }

    // Status
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }

    // Metadata
    public string? Notes { get; set; }
    public string? CustomFields { get; set; }
    public string? ExternalWarrantyId { get; set; }
}

/// <summary>
/// Request to update an existing warranty
/// </summary>
public class UpdateWarrantyRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WarrantyType Type { get; set; }

    // Duration
    public int DurationMonths { get; set; }
    public int? ExtendedDurationMonths { get; set; }
    public int? GracePeriodDays { get; set; }
    public int? MaxDurationMonths { get; set; }

    // Coverage
    public CoverageType CoverageType { get; set; }
    public bool IncludesParts { get; set; }
    public bool IncludesLabor { get; set; }
    public bool IncludesOnsite { get; set; }
    public bool IncludesShipping { get; set; }
    public string? CoveredItems { get; set; }
    public string? ExcludedItems { get; set; }
    public string? Terms { get; set; }

    // Conditions
    public bool IsTransferable { get; set; }
    public bool RequiresRegistration { get; set; }
    public int? RegistrationDeadlineDays { get; set; }
    public bool RequiresProofOfPurchase { get; set; }
    public string? Conditions { get; set; }

    // Extended Warranty Pricing
    public decimal? ExtendedWarrantyPrice { get; set; }
    public decimal? ExtendedWarrantyPricePercent { get; set; }
    public string Currency { get; set; } = "INR";

    // Service Levels
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public SupportHoursType? SupportHoursType { get; set; }

    // Claims
    public int? MaxClaims { get; set; }
    public decimal? MaxClaimValue { get; set; }
    public decimal? MaxClaimValuePercent { get; set; }
    public decimal? DeductibleAmount { get; set; }

    // Status
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }

    // Metadata
    public string? Notes { get; set; }
    public string? CustomFields { get; set; }
    public string? ExternalWarrantyId { get; set; }
}

/// <summary>
/// Warranty search request
/// </summary>
public class WarrantySearchRequest
{
    public string? SearchTerm { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? CategoryId { get; set; }
    public WarrantyType? Type { get; set; }
    public CoverageType? CoverageType { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDefault { get; set; }
    public int? MinDurationMonths { get; set; }
    public int? MaxDurationMonths { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "DisplayOrder";
    public bool SortDescending { get; set; }
}

/// <summary>
/// Warranty check result for a product/asset
/// </summary>
public class WarrantyCheckResult
{
    public bool IsUnderWarranty { get; set; }
    public Guid? WarrantyId { get; set; }
    public string? WarrantyCode { get; set; }
    public string? WarrantyName { get; set; }
    public WarrantyType? WarrantyType { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public int? DaysRemaining { get; set; }
    public CoverageType? CoverageType { get; set; }
    public bool? IncludesParts { get; set; }
    public bool? IncludesLabor { get; set; }
    public bool? IncludesOnsite { get; set; }
    public bool IsExtendable { get; set; }
    public decimal? ExtendedWarrantyPrice { get; set; }
    public string? Message { get; set; }
}
