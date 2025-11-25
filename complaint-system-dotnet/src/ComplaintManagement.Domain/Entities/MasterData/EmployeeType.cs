namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Represents an employee type/classification
/// </summary>
public class EmployeeType : BaseEntity
{
    /// <summary>
    /// Company ID (foreign key)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Employee type name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Employee type code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Employee type description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Is employee type active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Oryggi employee type ID for sync
    /// </summary>
    public string? OryggiEmployeeTypeId { get; set; }

    // Navigation properties
    /// <summary>
    /// Parent company
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Users with this employee type
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}
