namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Master table for product statuses - allows dynamic status configuration
/// </summary>
public class ProductStatusMaster : BaseEntity
{
    /// <summary>
    /// Status name (e.g., "Draft", "Active", "Discontinued")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Status code for programmatic reference (e.g., "DRAFT", "ACTIVE")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of the status
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for sorting in UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Color code for UI display (e.g., "#4CAF50", "green")
    /// </summary>
    public string? ColorCode { get; set; }

    /// <summary>
    /// Background color for badges (e.g., "#E8F5E9")
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Icon class for UI display (e.g., "fa-check-circle")
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Whether this status is active/available for use
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a default/system status that cannot be deleted
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Whether this status represents a default for new products
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Company ID (foreign key) - null means global/system status
    /// </summary>
    public Guid? CompanyId { get; set; }

    // Navigation properties
    public Company? Company { get; set; }
}
