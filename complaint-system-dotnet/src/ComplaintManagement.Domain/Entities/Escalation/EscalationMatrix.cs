using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Escalation;

/// <summary>
/// Represents an escalation matrix configuration for complaints
/// Defines when and how complaints should be escalated based on various criteria
/// </summary>
public class EscalationMatrix : BaseEntity
{
    /// <summary>
    /// Matrix name (e.g., "HR Complaints Escalation", "Critical Issues Escalation")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of this escalation matrix
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Company ID (foreign key) - matrix applies to this company
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Category ID (foreign key) - matrix applies to this category
    /// Optional: null means applies to all categories
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Branch ID (foreign key) - matrix applies to this branch
    /// Optional: null means applies to all branches
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Department ID (foreign key) - matrix applies to this department
    /// Optional: null means applies to all departments
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Whether this matrix is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Priority for matrix selection (higher = higher priority)
    /// When multiple matrices match, the one with highest priority is used
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Whether auto-escalation is enabled (triggered by SLA breach)
    /// </summary>
    public bool EnableAutoEscalation { get; set; } = true;

    /// <summary>
    /// Whether to send email notifications on escalation
    /// </summary>
    public bool SendEmailNotifications { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Company this matrix applies to
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Category this matrix applies to (if specific)
    /// </summary>
    public ComplaintCategory? Category { get; set; }

    /// <summary>
    /// Branch this matrix applies to (if specific)
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Department this matrix applies to (if specific)
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Escalation levels defined for this matrix
    /// </summary>
    public ICollection<EscalationLevel> EscalationLevels { get; set; } = new List<EscalationLevel>();
}
