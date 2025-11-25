using ComplaintManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Roles;

/// <summary>
/// Request to update permissions for a role
/// </summary>
public class UpdateRolePermissionsRequest
{
    [Required]
    public List<RolePermissionUpdate> Permissions { get; set; } = new();
}

public class RolePermissionUpdate
{
    [Required]
    public PermissionType PermissionType { get; set; }

    [Required]
    public bool IsGranted { get; set; }
}
