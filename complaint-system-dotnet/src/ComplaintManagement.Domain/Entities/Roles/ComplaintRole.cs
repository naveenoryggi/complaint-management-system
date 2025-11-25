using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Roles;

/// <summary>
/// Represents a role in the complaint management system
/// </summary>
public class ComplaintRole : BaseEntity
{
    /// <summary>
    /// Role name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role code/identifier
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Role description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Role type enum
    /// </summary>
    public RoleType RoleType { get; set; }

    /// <summary>
    /// Escalation level (0-5, 0 = not a handler role)
    /// </summary>
    public int EscalationLevel { get; set; } = 0;

    /// <summary>
    /// Is system role (cannot be modified)
    /// </summary>
    public bool IsSystemRole { get; set; } = false;

    /// <summary>
    /// Is role active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for UI
    /// </summary>
    public int DisplayOrder { get; set; }

    // Navigation properties
    /// <summary>
    /// User role assignments
    /// </summary>
    public ICollection<UserComplaintRole> UserRoles { get; set; } = new List<UserComplaintRole>();

    /// <summary>
    /// Role permissions
    /// </summary>
    public ICollection<ComplaintRolePermission> RolePermissions { get; set; } = new List<ComplaintRolePermission>();
}
