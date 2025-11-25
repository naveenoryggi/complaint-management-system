namespace ComplaintManagement.Domain.Entities.Complaints;

/// <summary>
/// Represents a category/type of complaint (e.g., Attendance, Salary, HRMS Issues)
/// </summary>
public class ComplaintCategory : BaseEntity
{
    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Parent category ID (for hierarchical categories)
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Default priority for complaints in this category
    /// </summary>
    public int DefaultPriority { get; set; } = 1; // Normal

    /// <summary>
    /// Is category active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Workflow ID - defines which workflow this category uses
    /// Null means no specific workflow (will use global default)
    /// </summary>
    public Guid? WorkflowId { get; set; }

    // Navigation properties
    /// <summary>
    /// Parent category
    /// </summary>
    public ComplaintCategory? ParentCategory { get; set; }

    /// <summary>
    /// Child categories
    /// </summary>
    public ICollection<ComplaintCategory> SubCategories { get; set; } = new List<ComplaintCategory>();

    /// <summary>
    /// Complaints in this category
    /// </summary>
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    /// <summary>
    /// Workflows defined for this category
    /// </summary>
    public ICollection<Workflows.CategoryWorkflow> Workflows { get; set; } = new List<Workflows.CategoryWorkflow>();
}
