using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ComplaintRolePermission
{
    public Guid Id { get; set; }

    public Guid ComplaintRoleId { get; set; }

    public string PermissionType { get; set; } = null!;

    public bool IsGranted { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ComplaintRole ComplaintRole { get; set; } = null!;
}
