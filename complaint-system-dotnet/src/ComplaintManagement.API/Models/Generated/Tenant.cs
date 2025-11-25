using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string ContactEmail { get; set; } = null!;

    public string? ContactPhone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public string? OryggiTenantId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<OryggiConnectionSetting> OryggiConnectionSettings { get; set; } = new List<OryggiConnectionSetting>();

    public virtual ICollection<SyncSchedule> SyncSchedules { get; set; } = new List<SyncSchedule>();
}
