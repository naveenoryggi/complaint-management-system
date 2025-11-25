using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class Section
{
    public Guid Id { get; set; }

    public Guid DepartmentId { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? HeadId { get; set; }

    public bool IsActive { get; set; }

    public string? OryggiSectionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public Guid? HrResponsibleId { get; set; }

    public Guid? SecondaryHeadId { get; set; }

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<EscalationPolicy> EscalationPolicies { get; set; } = new List<EscalationPolicy>();

    public virtual User? Head { get; set; }

    public virtual User? HrResponsible { get; set; }

    public virtual ICollection<ResourcePool> ResourcePools { get; set; } = new List<ResourcePool>();

    public virtual User? SecondaryHead { get; set; }

    public virtual ICollection<UserComplaintRole> UserComplaintRoles { get; set; } = new List<UserComplaintRole>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
