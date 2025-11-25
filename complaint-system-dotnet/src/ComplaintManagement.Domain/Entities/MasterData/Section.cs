namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Represents a section within a department
/// </summary>
public class Section : BaseEntity
{
    /// <summary>
    /// Department ID (foreign key)
    /// </summary>
    public Guid DepartmentId { get; set; }

    /// <summary>
    /// Section name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Section code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Section description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Primary section head/manager user ID (optional)
    /// </summary>
    public Guid? HeadId { get; set; }

    /// <summary>
    /// Secondary section head/manager user ID (optional)
    /// </summary>
    public Guid? SecondaryHeadId { get; set; }

    /// <summary>
    /// HR responsible for this section (optional)
    /// </summary>
    public Guid? HrResponsibleId { get; set; }

    /// <summary>
    /// Is section active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Oryggi section ID for sync
    /// </summary>
    public string? OryggiSectionId { get; set; }

    // Navigation properties
    /// <summary>
    /// Parent department
    /// </summary>
    public Department Department { get; set; } = null!;

    /// <summary>
    /// Primary section head
    /// </summary>
    public User? Head { get; set; }

    /// <summary>
    /// Secondary section head
    /// </summary>
    public User? SecondaryHead { get; set; }

    /// <summary>
    /// HR responsible for this section
    /// </summary>
    public User? HrResponsible { get; set; }

    /// <summary>
    /// Users in this section
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}
