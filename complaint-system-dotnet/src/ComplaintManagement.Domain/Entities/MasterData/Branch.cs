namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Represents a branch of a company
/// </summary>
public class Branch : BaseEntity
{
    /// <summary>
    /// Company ID (foreign key)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Branch name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Branch code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Branch description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Contact email
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Contact phone
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Branch address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Is branch active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Oryggi branch ID for sync
    /// </summary>
    public string? OryggiBranchId { get; set; }

    /// <summary>
    /// Primary manager/head of the branch (optional)
    /// </summary>
    public Guid? ManagerId { get; set; }

    /// <summary>
    /// Secondary/deputy manager for the branch (optional)
    /// </summary>
    public Guid? SecondaryManagerId { get; set; }

    /// <summary>
    /// HR responsible for this branch (optional)
    /// </summary>
    public Guid? HrResponsibleId { get; set; }

    /// <summary>
    /// Branch-specific timezone (IANA timezone identifier)
    /// Overrides company default timezone for this branch location
    /// e.g., "America/New_York" for NY branch, "Europe/London" for London branch
    /// If null, uses Company.DefaultTimeZone
    /// </summary>
    public string? TimeZone { get; set; }

    // Navigation properties
    /// <summary>
    /// Parent company
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Primary manager of the branch
    /// </summary>
    public User? Manager { get; set; }

    /// <summary>
    /// Secondary/deputy manager of the branch
    /// </summary>
    public User? SecondaryManager { get; set; }

    /// <summary>
    /// HR responsible for the branch
    /// </summary>
    public User? HrResponsible { get; set; }

    /// <summary>
    /// Departments in this branch
    /// </summary>
    public ICollection<Department> Departments { get; set; } = new List<Department>();

    /// <summary>
    /// Users in this branch
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}
