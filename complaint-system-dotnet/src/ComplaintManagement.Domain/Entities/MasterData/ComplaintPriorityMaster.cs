namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Master table for complaint priorities - allows dynamic priority configuration
/// </summary>
public class ComplaintPriorityMaster : BaseEntity
{
    /// <summary>
    /// Priority name (e.g., "Low", "Medium", "High", "Critical")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Priority code for programmatic reference (e.g., "LOW", "HIGH", "CRITICAL")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of the priority level
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for sorting in UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Priority level (1-10, higher = more urgent)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Color code for UI display (e.g., "#FF5722", "red")
    /// </summary>
    public string? ColorCode { get; set; }

    /// <summary>
    /// Icon class for UI display (e.g., "bi-exclamation-triangle")
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Whether this priority is active/available for use
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a default/system priority that cannot be deleted
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Company ID (foreign key) - null means global/system priority
    /// </summary>
    public Guid? CompanyId { get; set; }

    // Navigation properties
    /// <summary>
    /// Company this priority belongs to (null for system priorities)
    /// </summary>
    public Company? Company { get; set; }
}
