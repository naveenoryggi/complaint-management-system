using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Product;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Warranty definition that can be associated with products or categories
/// </summary>
public class Warranty : BaseEntity
{
    /// <summary>
    /// Company that owns this warranty definition
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Product this warranty applies to (null for category-level)
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Category this warranty applies to (null for product-level)
    /// </summary>
    public Guid? CategoryId { get; set; }

    #region Identity

    /// <summary>
    /// Warranty code (e.g., WTY-STD-12)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Warranty name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of warranty
    /// </summary>
    public WarrantyType Type { get; set; }

    #endregion

    #region Duration

    /// <summary>
    /// Standard warranty duration in months
    /// </summary>
    public int DurationMonths { get; set; }

    /// <summary>
    /// Extended warranty duration in months (purchasable)
    /// </summary>
    public int? ExtendedDurationMonths { get; set; }

    /// <summary>
    /// Grace period in days after warranty expires
    /// </summary>
    public int? GracePeriodDays { get; set; }

    /// <summary>
    /// Maximum warranty duration (for extensions)
    /// </summary>
    public int? MaxDurationMonths { get; set; }

    #endregion

    #region Coverage

    /// <summary>
    /// JSON list of what's covered
    /// </summary>
    public string? CoveredItems { get; set; }

    /// <summary>
    /// JSON list of exclusions
    /// </summary>
    public string? ExcludedItems { get; set; }

    /// <summary>
    /// Detailed warranty terms
    /// </summary>
    public string? Terms { get; set; }

    /// <summary>
    /// Coverage type for this warranty
    /// </summary>
    public CoverageType CoverageType { get; set; } = CoverageType.FullCoverage;

    /// <summary>
    /// Includes parts replacement
    /// </summary>
    public bool IncludesParts { get; set; } = true;

    /// <summary>
    /// Includes labor
    /// </summary>
    public bool IncludesLabor { get; set; } = true;

    /// <summary>
    /// Includes onsite service
    /// </summary>
    public bool IncludesOnsite { get; set; }

    /// <summary>
    /// Includes shipping for repairs
    /// </summary>
    public bool IncludesShipping { get; set; }

    #endregion

    #region Conditions

    /// <summary>
    /// Whether warranty is transferable to new owner
    /// </summary>
    public bool IsTransferable { get; set; }

    /// <summary>
    /// Whether product registration is required
    /// </summary>
    public bool RequiresRegistration { get; set; }

    /// <summary>
    /// Days after purchase to register
    /// </summary>
    public int? RegistrationDeadlineDays { get; set; }

    /// <summary>
    /// Requires proof of purchase
    /// </summary>
    public bool RequiresProofOfPurchase { get; set; } = true;

    /// <summary>
    /// Additional conditions (JSON)
    /// </summary>
    public string? Conditions { get; set; }

    #endregion

    #region Extended Warranty Pricing

    /// <summary>
    /// Price for extended warranty
    /// </summary>
    public decimal? ExtendedWarrantyPrice { get; set; }

    /// <summary>
    /// Extended warranty price as percentage of product price
    /// </summary>
    public decimal? ExtendedWarrantyPricePercent { get; set; }

    /// <summary>
    /// Currency for pricing
    /// </summary>
    public string Currency { get; set; } = "INR";

    #endregion

    #region Service Levels

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

    #endregion

    #region Claims

    /// <summary>
    /// Maximum number of claims allowed
    /// </summary>
    public int? MaxClaims { get; set; }

    /// <summary>
    /// Maximum claim value
    /// </summary>
    public decimal? MaxClaimValue { get; set; }

    /// <summary>
    /// Maximum claim value as percentage of product price
    /// </summary>
    public decimal? MaxClaimValuePercent { get; set; }

    /// <summary>
    /// Deductible amount per claim
    /// </summary>
    public decimal? DeductibleAmount { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Whether this warranty is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the default warranty for product/category
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// Notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Custom fields (JSON)
    /// </summary>
    public string? CustomFields { get; set; }

    /// <summary>
    /// External warranty ID
    /// </summary>
    public string? ExternalWarrantyId { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Company that owns this warranty
    /// </summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>
    /// Associated product
    /// </summary>
    public virtual Domain.Entities.Product.Product? Product { get; set; }

    /// <summary>
    /// Associated category
    /// </summary>
    public virtual ProductCategory? Category { get; set; }

    #endregion
}
