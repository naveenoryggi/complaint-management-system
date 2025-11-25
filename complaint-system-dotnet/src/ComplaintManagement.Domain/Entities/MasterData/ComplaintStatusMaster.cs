namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Master table for complaint statuses - allows dynamic status configuration
/// </summary>
public class ComplaintStatusMaster : BaseEntity
{
    /// <summary>
    /// Status name (e.g., "Submitted", "In Progress", "Resolved")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Status code for programmatic reference (e.g., "SUBMITTED", "IN_PROGRESS")
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
    /// Icon class for UI display (e.g., "bi-check-circle")
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
    /// Whether this is a final/closed status
    /// </summary>
    public bool IsFinal { get; set; } = false;

    /// <summary>
    /// Company ID (foreign key) - null means global/system status
    /// </summary>
    public Guid? CompanyId { get; set; }

    // Navigation properties
    /// <summary>
    /// Company this status belongs to (null for system statuses)
    /// </summary>
    public Company? Company { get; set; }
}
