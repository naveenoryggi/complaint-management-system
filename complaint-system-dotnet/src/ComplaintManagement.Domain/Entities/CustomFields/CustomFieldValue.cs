using ComplaintManagement.Domain.Entities.Complaints;

namespace ComplaintManagement.Domain.Entities.CustomFields;

/// <summary>
/// Stores the actual value of a custom field for a specific entity instance
/// </summary>
public class CustomFieldValue : BaseEntity
{
    /// <summary>
    /// Reference to the field definition
    /// </summary>
    public Guid CustomFieldDefinitionId { get; set; }

    /// <summary>
    /// ID of the entity this value belongs to (e.g., ComplaintId)
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Type of entity (e.g., "Complaint", "User", etc.)
    /// </summary>
    public string EntityType { get; set; } = "Complaint";

    /// <summary>
    /// The actual value stored as string (will be parsed based on field type)
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// For number fields - stored as decimal for precision
    /// </summary>
    public decimal? NumericValue { get; set; }

    /// <summary>
    /// For date/datetime fields
    /// </summary>
    public DateTime? DateValue { get; set; }

    /// <summary>
    /// For boolean fields
    /// </summary>
    public bool? BooleanValue { get; set; }

    /// <summary>
    /// For complex/multi-value fields (JSON)
    /// </summary>
    public string? JsonValue { get; set; }

    // Navigation properties
    /// <summary>
    /// The field definition
    /// </summary>
    public CustomFieldDefinition FieldDefinition { get; set; } = null!;

    /// <summary>
    /// The complaint this value belongs to (if EntityType = "Complaint")
    /// </summary>
    public Complaint? Complaint { get; set; }
}
