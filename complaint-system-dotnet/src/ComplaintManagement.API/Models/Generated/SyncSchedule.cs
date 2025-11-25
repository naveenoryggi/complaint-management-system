using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class SyncSchedule
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ScheduleType { get; set; } = null!;

    public string TimeOfDay { get; set; } = null!;

    public int? DayValue { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime? LastRunAt { get; set; }

    public DateTime? NextRunAt { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
