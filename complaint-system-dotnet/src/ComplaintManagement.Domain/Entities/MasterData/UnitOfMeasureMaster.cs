namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Master table for units of measure - allows dynamic unit configuration
/// </summary>
public class UnitOfMeasureMaster : BaseEntity
{
    /// <summary>
    /// Unit name (e.g., "Each", "Box", "Kilogram")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unit code/symbol (e.g., "EA", "BOX", "KG")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Abbreviated symbol for display (e.g., "ea", "kg", "L")
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Description of the unit
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for sorting in UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Category of unit (e.g., "Count", "Weight", "Volume", "Length", "Time")
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether this unit is active/available for use
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a default/system unit that cannot be deleted
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Whether this unit is the default for new products
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Company ID (foreign key) - null means global/system unit
    /// </summary>
    public Guid? CompanyId { get; set; }

    // Navigation properties
    public Company? Company { get; set; }
}
