using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Represents a stock category for classifying assets
/// Allows admin to define custom categories like Sales Stock, Fixed Asset, Aftersales Stock, Faulty Stock, etc.
/// </summary>
public class StockCategory : BaseEntity
{
    /// <summary>
    /// Company that owns this stock category
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Unique code for the category (e.g., SALES, FIXED, AFTERSALES, FAULTY)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the category
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the category
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Color code for visual identification (hex format)
    /// </summary>
    public string? ColorCode { get; set; }

    /// <summary>
    /// Icon name for visual identification
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Whether assets in this category are available for sale
    /// </summary>
    public bool IsForSale { get; set; }

    /// <summary>
    /// Whether assets in this category are company fixed assets
    /// </summary>
    public bool IsFixedAsset { get; set; }

    /// <summary>
    /// Whether assets in this category are service/replacement stock
    /// </summary>
    public bool IsServiceStock { get; set; }

    /// <summary>
    /// Whether assets in this category are defective/faulty
    /// </summary>
    public bool IsFaulty { get; set; }

    /// <summary>
    /// Whether this category is available for demos
    /// </summary>
    public bool IsDemo { get; set; }

    /// <summary>
    /// Whether assets in this category require special handling
    /// </summary>
    public bool RequiresSpecialHandling { get; set; }

    /// <summary>
    /// Whether this category allows assignment to customers
    /// </summary>
    public bool AllowsCustomerAssignment { get; set; } = true;

    /// <summary>
    /// Whether this category allows assignment to employees
    /// </summary>
    public bool AllowsEmployeeAssignment { get; set; } = true;

    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this category is active and can be used
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a system-defined category that cannot be deleted
    /// </summary>
    public bool IsSystem { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Company that owns this category
    /// </summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>
    /// Assets in this category
    /// </summary>
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    #endregion
}
