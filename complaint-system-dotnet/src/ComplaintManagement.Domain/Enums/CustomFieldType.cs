namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Types of custom fields that can be defined
/// </summary>
public enum CustomFieldType
{
    /// <summary>
    /// Single-line text input
    /// </summary>
    Text = 0,

    /// <summary>
    /// Multi-line text area
    /// </summary>
    TextArea = 1,

    /// <summary>
    /// Numeric input (integer or decimal)
    /// </summary>
    Number = 2,

    /// <summary>
    /// Date picker
    /// </summary>
    Date = 3,

    /// <summary>
    /// Date and time picker
    /// </summary>
    DateTime = 4,

    /// <summary>
    /// Email input with validation
    /// </summary>
    Email = 5,

    /// <summary>
    /// Phone number input
    /// </summary>
    Phone = 6,

    /// <summary>
    /// URL input with validation
    /// </summary>
    Url = 7,

    /// <summary>
    /// Single-select dropdown
    /// </summary>
    Dropdown = 8,

    /// <summary>
    /// Multi-select dropdown
    /// </summary>
    MultiSelect = 9,

    /// <summary>
    /// Radio button group
    /// </summary>
    Radio = 10,

    /// <summary>
    /// Checkbox group
    /// </summary>
    Checkbox = 11,

    /// <summary>
    /// Single checkbox (yes/no)
    /// </summary>
    Boolean = 12,

    /// <summary>
    /// File upload
    /// </summary>
    File = 13,

    /// <summary>
    /// Rich text editor
    /// </summary>
    RichText = 14,

    /// <summary>
    /// Color picker
    /// </summary>
    Color = 15,

    /// <summary>
    /// Rating (stars)
    /// </summary>
    Rating = 16
}
