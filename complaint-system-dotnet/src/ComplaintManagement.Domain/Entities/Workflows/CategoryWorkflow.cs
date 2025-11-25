using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Workflows;

/// <summary>
/// Defines a workflow configuration for a specific category
/// Allows each category to have its own set of statuses and transition rules
/// </summary>
public class CategoryWorkflow : BaseEntity
{
    /// <summary>
    /// Category this workflow applies to
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Workflow name (e.g., "Standard HR Workflow", "IT Ticket Workflow")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Workflow description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this workflow is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the default workflow for the category
    /// </summary>
    public bool IsDefault { get; set; } = true;

    /// <summary>
    /// Company ID (for multi-tenant scenarios) - null means global/system workflow
    /// </summary>
    public Guid? CompanyId { get; set; }

    // Navigation properties
    /// <summary>
    /// Category this workflow belongs to
    /// </summary>
    public ComplaintCategory Category { get; set; } = null!;

    /// <summary>
    /// Company this workflow belongs to (null for global workflows)
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Statuses available in this workflow
    /// </summary>
    public ICollection<CategoryWorkflowStatus> WorkflowStatuses { get; set; } = new List<CategoryWorkflowStatus>();

    /// <summary>
    /// Allowed transitions between statuses in this workflow
    /// </summary>
    public ICollection<CategoryWorkflowTransition> Transitions { get; set; } = new List<CategoryWorkflowTransition>();
}
