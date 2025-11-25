using ComplaintManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Roles;

/// <summary>
/// Request to create a new role
/// </summary>
public class CreateRoleRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Role type - defaults to Complainant if not specified
    /// </summary>
    [EnumDataType(typeof(RoleType), ErrorMessage = "Invalid role type")]
    public RoleType RoleType { get; set; } = RoleType.Complainant;

    public int EscalationLevel { get; set; } = 0;

    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// List of permission types to grant to this role
    /// </summary>
    public List<PermissionType> Permissions { get; set; } = new();
}
