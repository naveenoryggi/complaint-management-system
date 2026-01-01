using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Domain.Entities.CRM;

/// <summary>
/// Represents a project associated with a customer
/// </summary>
public class Project : BaseEntity
{
    #region Identity

    /// <summary>
    /// Service provider company ID (foreign key)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Project code (e.g., PRJ-001)
    /// </summary>
    public string ProjectCode { get; set; } = string.Empty;

    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Project status
    /// </summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    #endregion

    #region Customer Association

    /// <summary>
    /// Customer ID (foreign key)
    /// </summary>
    public Guid CustomerId { get; set; }

    #endregion

    #region Timeline

    /// <summary>
    /// Planned start date
    /// </summary>
    public DateTime? PlannedStartDate { get; set; }

    /// <summary>
    /// Planned end date
    /// </summary>
    public DateTime? PlannedEndDate { get; set; }

    /// <summary>
    /// Actual start date
    /// </summary>
    public DateTime? ActualStartDate { get; set; }

    /// <summary>
    /// Actual end date
    /// </summary>
    public DateTime? ActualEndDate { get; set; }

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int ProgressPercentage { get; set; } = 0;

    #endregion

    #region Budget

    /// <summary>
    /// Planned budget amount
    /// </summary>
    public decimal? Budget { get; set; }

    /// <summary>
    /// Actual cost incurred
    /// </summary>
    public decimal? ActualCost { get; set; }

    /// <summary>
    /// Currency for budget (default INR)
    /// </summary>
    public string Currency { get; set; } = "INR";

    #endregion

    #region Team

    /// <summary>
    /// Project manager (employee) ID
    /// </summary>
    public Guid? ProjectManagerId { get; set; }

    #endregion

    #region Additional Information

    /// <summary>
    /// Priority level (1-5)
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// Tags for categorization (JSON array)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Custom fields (JSON object)
    /// </summary>
    public string? CustomFields { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Service provider company
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Associated customer
    /// </summary>
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// Project manager
    /// </summary>
    public Employee? ProjectManager { get; set; }

    /// <summary>
    /// Project milestones
    /// </summary>
    public ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();

    /// <summary>
    /// Project tasks
    /// </summary>
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

    /// <summary>
    /// Project documents
    /// </summary>
    public ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();

    /// <summary>
    /// Project team members
    /// </summary>
    public ICollection<ProjectTeamMember> TeamMembers { get; set; } = new List<ProjectTeamMember>();

    /// <summary>
    /// Complaints/tickets related to this project
    /// </summary>
    public ICollection<Complaints.Complaint> Complaints { get; set; } = new List<Complaints.Complaint>();

    #endregion
}
