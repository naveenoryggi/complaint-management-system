using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Domain.Entities.CRM;

/// <summary>
/// Represents a milestone within a project
/// </summary>
public class ProjectMilestone : BaseEntity
{
    #region Identity

    /// <summary>
    /// Project ID (foreign key)
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Milestone name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Milestone description
    /// </summary>
    public string? Description { get; set; }

    #endregion

    #region Status & Timeline

    /// <summary>
    /// Milestone status
    /// </summary>
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

    /// <summary>
    /// Due date for this milestone
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Date when milestone was completed
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    #endregion

    #region Ordering

    /// <summary>
    /// Display order for milestones
    /// </summary>
    public int SortOrder { get; set; } = 0;

    #endregion

    #region Additional Information

    /// <summary>
    /// Completion notes
    /// </summary>
    public string? Notes { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Parent project
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Tasks associated with this milestone
    /// </summary>
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

    #endregion
}
