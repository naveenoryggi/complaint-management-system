using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Roles;

/// <summary>
/// Request to assign a role to a user
/// </summary>
public class AssignRoleToUserRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid RoleId { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsPrimary { get; set; } = false;

    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Scope IDs for contextual role assignment (optional)
    /// </summary>
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }
}
