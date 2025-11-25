using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Roles;

/// <summary>
/// Links users to complaint roles with organizational scope
/// </summary>
public class UserComplaintRole : BaseEntity
{
    /// <summary>
    /// User ID (foreign key)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Complaint role ID (foreign key)
    /// </summary>
    public Guid ComplaintRoleId { get; set; }

    /// <summary>
    /// Company ID scope (null = all companies)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Branch ID scope (null = all branches within company)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Department ID scope (null = all departments within branch)
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Section ID scope (null = all sections within department)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Effective start date
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Effective end date (null = no expiry)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Is this the primary role for the user
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Is role assignment active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Notes about the role assignment
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    /// <summary>
    /// User
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Complaint role
    /// </summary>
    public ComplaintRole ComplaintRole { get; set; } = null!;

    /// <summary>
    /// Company scope (if applicable)
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Branch scope (if applicable)
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Department scope (if applicable)
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Section scope (if applicable)
    /// </summary>
    public Section? Section { get; set; }
}
