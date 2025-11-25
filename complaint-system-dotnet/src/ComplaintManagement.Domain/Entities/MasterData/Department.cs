namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Represents a department within a branch
/// </summary>
public class Department : BaseEntity
{
    /// <summary>
    /// Branch ID (foreign key)
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Department name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Department code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Department description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Primary manager/head of the department (optional)
    /// </summary>
    public Guid? ManagerId { get; set; }

    /// <summary>
    /// Secondary/deputy manager for the department (optional)
    /// </summary>
    public Guid? SecondaryManagerId { get; set; }

    /// <summary>
    /// HR responsible for this department (optional)
    /// </summary>
    public Guid? HrResponsibleId { get; set; }

    /// <summary>
    /// Is department active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Oryggi department ID for sync
    /// </summary>
    public string? OryggiDepartmentId { get; set; }

    // Navigation properties
    /// <summary>
    /// Parent branch
    /// </summary>
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// Primary department manager
    /// </summary>
    public User? Manager { get; set; }

    /// <summary>
    /// Secondary department manager
    /// </summary>
    public User? SecondaryManager { get; set; }

    /// <summary>
    /// HR responsible for this department
    /// </summary>
    public User? HrResponsible { get; set; }

    /// <summary>
    /// Sections in this department
    /// </summary>
    public ICollection<Section> Sections { get; set; } = new List<Section>();

    /// <summary>
    /// Users in this department
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}
