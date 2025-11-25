using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ComplaintRole
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string RoleType { get; set; } = null!;

    public int EscalationLevel { get; set; }

    public bool IsSystemRole { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ICollection<AuthenticationProvider> AuthenticationProviders { get; set; } = new List<AuthenticationProvider>();

    public virtual ICollection<ComplaintRolePermission> ComplaintRolePermissions { get; set; } = new List<ComplaintRolePermission>();

    public virtual ICollection<UserComplaintRole> UserComplaintRoles { get; set; } = new List<UserComplaintRole>();
}
