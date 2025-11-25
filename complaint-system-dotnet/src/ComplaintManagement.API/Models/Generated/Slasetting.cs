using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class Slasetting
{
    public Guid Id { get; set; }

    public bool IsEnabled { get; set; }

    public bool WorkingHoursOnly { get; set; }

    public TimeOnly? WorkingHoursStart { get; set; }

    public TimeOnly? WorkingHoursEnd { get; set; }

    public string WorkingDays { get; set; } = null!;

    public bool AutoEscalateOnBreach { get; set; }

    public int EscalationThresholdPercent { get; set; }

    public bool NotifyBeforeBreach { get; set; }

    public int NotifyBeforeBreachMinutes { get; set; }

    public bool PauseSlaonPendingInfo { get; set; }

    public bool ExcludeHolidays { get; set; }

    public string Timezone { get; set; } = null!;

    public Guid? CompanyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }
}
