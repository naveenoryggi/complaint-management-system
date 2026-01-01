using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.CRM;

/// <summary>
/// Represents a team member assigned to a project
/// </summary>
public class ProjectTeamMember : BaseEntity
{
    #region Identity

    /// <summary>
    /// Project ID (foreign key)
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Employee ID (foreign key)
    /// </summary>
    public Guid EmployeeId { get; set; }

    #endregion

    #region Role Information

    /// <summary>
    /// Role in the project (e.g., Developer, Tester, Analyst)
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Allocation percentage (0-100)
    /// </summary>
    public int? AllocationPercentage { get; set; }

    #endregion

    #region Timeline

    /// <summary>
    /// Date when member joined the project
    /// </summary>
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when member left the project (null if still active)
    /// </summary>
    public DateTime? LeftDate { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Whether the member is currently active on the project
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Additional Information

    /// <summary>
    /// Notes about the team member's involvement
    /// </summary>
    public string? Notes { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Parent project
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Assigned employee
    /// </summary>
    public Employee Employee { get; set; } = null!;

    #endregion
}
