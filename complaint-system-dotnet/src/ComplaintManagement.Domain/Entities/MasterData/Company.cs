namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Represents a company within a tenant
/// </summary>
public class Company : BaseEntity
{
    /// <summary>
    /// Tenant ID (foreign key)
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Company name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Company code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Company description
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
    /// Company address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Is company active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Oryggi company ID for sync
    /// </summary>
    public string? OryggiCompanyId { get; set; }

    /// <summary>
    /// Company logo URL/path
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Company logo file name
    /// </summary>
    public string? LogoFileName { get; set; }

    /// <summary>
    /// Company logo content type (e.g., image/png)
    /// </summary>
    public string? LogoContentType { get; set; }

    /// <summary>
    /// Primary manager/head of the company (optional)
    /// </summary>
    public Guid? ManagerId { get; set; }

    /// <summary>
    /// Secondary/deputy manager for the company (optional)
    /// </summary>
    public Guid? SecondaryManagerId { get; set; }

    /// <summary>
    /// HR responsible for this company (optional)
    /// </summary>
    public Guid? HrResponsibleId { get; set; }

    /// <summary>
    /// Default timezone for the company (IANA timezone identifier)
    /// Used as fallback when user or branch timezone is not set
    /// Default: "Asia/Kolkata"
    /// </summary>
    public string DefaultTimeZone { get; set; } = "Asia/Kolkata";

    /// <summary>
    /// Default locale for the company
    /// e.g., "en-US", "en-GB", "ar-SA"
    /// </summary>
    public string? DefaultLocale { get; set; }

    // Navigation properties
    /// <summary>
    /// Parent tenant
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Primary manager of the company
    /// </summary>
    public User? Manager { get; set; }

    /// <summary>
    /// Secondary/deputy manager of the company
    /// </summary>
    public User? SecondaryManager { get; set; }

    /// <summary>
    /// HR responsible for the company
    /// </summary>
    public User? HrResponsible { get; set; }

    /// <summary>
    /// Branches belonging to this company
    /// </summary>
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();

    /// <summary>
    /// Users belonging to this company
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}
