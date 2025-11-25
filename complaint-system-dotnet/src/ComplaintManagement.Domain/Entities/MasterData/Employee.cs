namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Represents an employee synced from Oryggi HRMS
/// </summary>
public class Employee : BaseEntity
{
    /// <summary>
    /// Tenant ID (foreign key)
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Section ID (foreign key)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Employee code/number
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Primary phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Alternate phone number
    /// </summary>
    public string? AlternatePhone { get; set; }

    /// <summary>
    /// Date of joining the organization
    /// </summary>
    public DateTime? DateOfJoining { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Reporting manager employee ID (for hierarchy)
    /// </summary>
    public Guid? ManagerId { get; set; }

    /// <summary>
    /// Is employee active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Oryggi employee ID for sync
    /// </summary>
    public string? OryggiEmployeeId { get; set; }

    // Navigation properties
    /// <summary>
    /// Parent tenant
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Section the employee belongs to
    /// </summary>
    public Section? Section { get; set; }

    /// <summary>
    /// Reporting manager
    /// </summary>
    public Employee? Manager { get; set; }

    /// <summary>
    /// Direct reports (subordinates)
    /// </summary>
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
}
