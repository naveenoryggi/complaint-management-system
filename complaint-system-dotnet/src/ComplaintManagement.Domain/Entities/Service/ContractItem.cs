using ComplaintManagement.Domain.Entities.Product;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Individual item (product/asset) covered under a contract
/// </summary>
public class ContractItem : BaseEntity
{
    /// <summary>
    /// Parent contract
    /// </summary>
    public Guid ContractId { get; set; }

    /// <summary>
    /// Product being covered (for product-level coverage)
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Specific asset being covered (for asset-level coverage)
    /// </summary>
    public Guid? AssetId { get; set; }

    #region Item Details

    /// <summary>
    /// Item description (denormalized for history)
    /// </summary>
    public string ItemDescription { get; set; } = string.Empty;

    /// <summary>
    /// Serial number(s) covered - JSON array
    /// </summary>
    public string? SerialNumbers { get; set; }

    /// <summary>
    /// Quantity of items covered
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Unit of measurement
    /// </summary>
    public string? UnitOfMeasure { get; set; }

    #endregion

    #region Coverage Dates

    /// <summary>
    /// Coverage start date (may differ from contract start)
    /// </summary>
    public DateTime? CoverageStartDate { get; set; }

    /// <summary>
    /// Coverage end date (may differ from contract end)
    /// </summary>
    public DateTime? CoverageEndDate { get; set; }

    /// <summary>
    /// Date when item was added to contract
    /// </summary>
    public DateTime? AddedDate { get; set; }

    /// <summary>
    /// Date when item was removed from contract
    /// </summary>
    public DateTime? RemovedDate { get; set; }

    #endregion

    #region Financials

    /// <summary>
    /// Value of this item in the contract
    /// </summary>
    public decimal? ItemValue { get; set; }

    /// <summary>
    /// Discount percentage for this item
    /// </summary>
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// Currency
    /// </summary>
    public string Currency { get; set; } = "INR";

    #endregion

    #region Service Level Override

    /// <summary>
    /// Override response time for this item (hours)
    /// </summary>
    public int? ResponseTimeHours { get; set; }

    /// <summary>
    /// Override resolution time for this item (hours)
    /// </summary>
    public int? ResolutionTimeHours { get; set; }

    /// <summary>
    /// Priority level for this item
    /// </summary>
    public int? PriorityLevel { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Current status of this item's coverage
    /// </summary>
    public ContractItemStatus Status { get; set; } = ContractItemStatus.Covered;

    /// <summary>
    /// Whether this item is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Location

    /// <summary>
    /// Location where this item is installed
    /// </summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// Site/building information
    /// </summary>
    public string? SiteInfo { get; set; }

    #endregion

    #region Maintenance

    /// <summary>
    /// Last maintenance date for this item
    /// </summary>
    public DateTime? LastMaintenanceDate { get; set; }

    /// <summary>
    /// Next scheduled maintenance
    /// </summary>
    public DateTime? NextMaintenanceDate { get; set; }

    /// <summary>
    /// Maintenance interval in days
    /// </summary>
    public int? MaintenanceIntervalDays { get; set; }

    /// <summary>
    /// Number of service calls for this item
    /// </summary>
    public int ServiceCallCount { get; set; }

    #endregion

    #region Notes

    /// <summary>
    /// Special conditions for this item
    /// </summary>
    public string? SpecialConditions { get; set; }

    /// <summary>
    /// Internal notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Custom fields (JSON)
    /// </summary>
    public string? CustomFields { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Parent contract
    /// </summary>
    public virtual Contract Contract { get; set; } = null!;

    /// <summary>
    /// Associated product
    /// </summary>
    public virtual Domain.Entities.Product.Product? Product { get; set; }

    /// <summary>
    /// Associated asset (specific unit covered)
    /// </summary>
    public virtual Asset? Asset { get; set; }

    #endregion
}
