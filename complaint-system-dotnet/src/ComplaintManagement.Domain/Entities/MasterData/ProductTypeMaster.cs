namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Master table for product types - allows dynamic type configuration
/// </summary>
public class ProductTypeMaster : BaseEntity
{
    /// <summary>
    /// Type name (e.g., "Physical", "Digital", "Service")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type code for programmatic reference (e.g., "PHYSICAL", "DIGITAL")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product type
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for sorting in UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Color code for UI display (e.g., "#2196F3", "blue")
    /// </summary>
    public string? ColorCode { get; set; }

    /// <summary>
    /// Background color for badges
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Icon class for UI display (e.g., "fa-box", "fa-cloud")
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Whether this type requires inventory tracking
    /// </summary>
    public bool RequiresInventory { get; set; } = true;

    /// <summary>
    /// Whether this type supports serial numbers
    /// </summary>
    public bool SupportsSerialNumbers { get; set; } = false;

    /// <summary>
    /// Whether this type is active/available for use
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a default/system type that cannot be deleted
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Whether this type is the default for new products
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Company ID (foreign key) - null means global/system type
    /// </summary>
    public Guid? CompanyId { get; set; }

    // Navigation properties
    public Company? Company { get; set; }
}
