using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Roles;

/// <summary>
/// Request to update an existing role
/// </summary>
public class UpdateRoleRequest
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int? EscalationLevel { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }
}
