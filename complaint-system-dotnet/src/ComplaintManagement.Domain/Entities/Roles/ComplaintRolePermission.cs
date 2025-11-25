using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Roles;

/// <summary>
/// Links complaint roles to permissions
/// </summary>
public class ComplaintRolePermission : BaseEntity
{
    /// <summary>
    /// Complaint role ID (foreign key)
    /// </summary>
    public Guid ComplaintRoleId { get; set; }

    /// <summary>
    /// Permission type
    /// </summary>
    public PermissionType PermissionType { get; set; }

    /// <summary>
    /// Is permission granted
    /// </summary>
    public bool IsGranted { get; set; } = true;

    /// <summary>
    /// Permission notes/description
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    /// <summary>
    /// Parent complaint role
    /// </summary>
    public ComplaintRole ComplaintRole { get; set; } = null!;
}
