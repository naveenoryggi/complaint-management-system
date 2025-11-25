using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class UserComplaintRole
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ComplaintRoleId { get; set; }

    public Guid? CompanyId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? SectionId { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ComplaintRole ComplaintRole { get; set; } = null!;

    public virtual Department? Department { get; set; }

    public virtual Section? Section { get; set; }

    public virtual User User { get; set; } = null!;
}
