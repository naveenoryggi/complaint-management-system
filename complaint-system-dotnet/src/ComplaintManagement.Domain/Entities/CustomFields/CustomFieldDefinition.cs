using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.CustomFields;

/// <summary>
/// Defines a custom field that can be added to complaints dynamically
/// </summary>
public class CustomFieldDefinition : BaseEntity
{
    /// <summary>
    /// Field name/label displayed to users
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Internal field key (unique identifier for programmatic access)
    /// </summary>
    public string FieldKey { get; set; } = string.Empty;

    /// <summary>
    /// Field description/help text
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Data type of the field
    /// </summary>
    public CustomFieldType FieldType { get; set; }

    /// <summary>
    /// Display order in forms
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Is this field required?
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Is this field active/visible?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is this field searchable?
    /// </summary>
    public bool IsSearchable { get; set; } = false;

    /// <summary>
    /// Default value (as JSON string)
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Validation rules (as JSON string)
    /// Examples: {"min": 0, "max": 100}, {"pattern": "^[A-Z]+$"}
    /// </summary>
    public string? ValidationRules { get; set; }

    /// <summary>
    /// Options for dropdown/radio/checkbox fields (as JSON array)
    /// Example: ["Option 1", "Option 2", "Option 3"]
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// Placeholder text for input fields
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Help text displayed below the field
    /// </summary>
    public string? HelpText { get; set; }

    /// <summary>
    /// Entity type this field applies to
    /// </summary>
    public string EntityType { get; set; } = "Complaint";

    /// <summary>
    /// Company ID (null = global/system field)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Section/group for organizing fields in UI
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// Icon class for UI display
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Is this field visible to end users (complainants)?
    /// </summary>
    public bool IsVisibleToComplainant { get; set; } = true;

    /// <summary>
    /// Is this field visible to handlers/staff?
    /// </summary>
    public bool IsVisibleToHandler { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Company this field belongs to
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Values for this field definition
    /// </summary>
    public ICollection<CustomFieldValue> FieldValues { get; set; } = new List<CustomFieldValue>();
}
