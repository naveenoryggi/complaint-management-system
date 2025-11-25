using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class Department
{
    public Guid Id { get; set; }

    public Guid BranchId { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? ManagerId { get; set; }

    public bool IsActive { get; set; }

    public string? OryggiDepartmentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public Guid? HrResponsibleId { get; set; }

    public Guid? SecondaryManagerId { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    public virtual ICollection<EscalationLevel> EscalationLevels { get; set; } = new List<EscalationLevel>();

    public virtual ICollection<EscalationMatrix> EscalationMatrices { get; set; } = new List<EscalationMatrix>();

    public virtual ICollection<EscalationPolicy> EscalationPolicies { get; set; } = new List<EscalationPolicy>();

    public virtual User? HrResponsible { get; set; }

    public virtual User? Manager { get; set; }

    public virtual ICollection<ResourcePool> ResourcePools { get; set; } = new List<ResourcePool>();

    public virtual User? SecondaryManager { get; set; }

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<UserComplaintRole> UserComplaintRoles { get; set; } = new List<UserComplaintRole>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
