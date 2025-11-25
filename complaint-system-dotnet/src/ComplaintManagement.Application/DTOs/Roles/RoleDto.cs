using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.DTOs.Roles;

/// <summary>
/// Data transfer object for complaint role
/// </summary>
public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoleType RoleType { get; set; }
    public int EscalationLevel { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for role permission
/// </summary>
public class PermissionDto
{
    public Guid Id { get; set; }
    public PermissionType PermissionType { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
}
