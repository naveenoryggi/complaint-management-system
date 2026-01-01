using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Domain.Entities.CRM;

/// <summary>
/// Represents a task within a project
/// </summary>
public class ProjectTask : BaseEntity
{
    #region Identity

    /// <summary>
    /// Project ID (foreign key)
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Milestone ID (optional, foreign key)
    /// </summary>
    public Guid? MilestoneId { get; set; }

    /// <summary>
    /// Task title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Task description
    /// </summary>
    public string? Description { get; set; }

    #endregion

    #region Status & Priority

    /// <summary>
    /// Task status
    /// </summary>
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.ToDo;

    /// <summary>
    /// Task priority
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    #endregion

    #region Assignment

    /// <summary>
    /// Assigned employee ID (foreign key)
    /// </summary>
    public Guid? AssigneeId { get; set; }

    #endregion

    #region Timeline

    /// <summary>
    /// Due date for the task
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Date when task was completed
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    #endregion

    #region Time Tracking

    /// <summary>
    /// Estimated hours to complete
    /// </summary>
    public int? EstimatedHours { get; set; }

    /// <summary>
    /// Actual hours spent
    /// </summary>
    public int? ActualHours { get; set; }

    #endregion

    #region Additional Information

    /// <summary>
    /// Tags for categorization (comma-separated)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Task notes
    /// </summary>
    public string? Notes { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Parent project
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Parent milestone (if assigned)
    /// </summary>
    public ProjectMilestone? Milestone { get; set; }

    /// <summary>
    /// Assigned employee
    /// </summary>
    public Employee? Assignee { get; set; }

    #endregion
}
