namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// Global SLA settings DTO
/// </summary>
public class SLASettingsDto
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool WorkingHoursOnly { get; set; } = false;
    public TimeSpan? WorkingHoursStart { get; set; }
    public TimeSpan? WorkingHoursEnd { get; set; }
    public string WorkingDays { get; set; } = "1,2,3,4,5"; // Comma-separated day numbers
    public bool ExcludeHolidays { get; set; } = true;
    public bool AutoEscalateOnBreach { get; set; } = true;
    public int EscalationThresholdPercent { get; set; } = 80;
    public bool NotifyBeforeBreach { get; set; } = true;
    public int NotifyBeforeBreachMinutes { get; set; } = 30;
    public bool PauseSLAOnPendingInfo { get; set; } = true;
    public string Timezone { get; set; } = "UTC";
    public Guid? CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request to update global SLA settings
/// </summary>
public class UpdateSLASettingsRequest
{
    public bool IsEnabled { get; set; } = true;
    public bool WorkingHoursOnly { get; set; } = false;
    public TimeSpan? WorkingHoursStart { get; set; }
    public TimeSpan? WorkingHoursEnd { get; set; }
    public string WorkingDays { get; set; } = "1,2,3,4,5";
    public bool ExcludeHolidays { get; set; } = true;
    public bool AutoEscalateOnBreach { get; set; } = true;
    public int EscalationThresholdPercent { get; set; } = 80;
    public bool NotifyBeforeBreach { get; set; } = true;
    public int NotifyBeforeBreachMinutes { get; set; } = 30;
    public bool PauseSLAOnPendingInfo { get; set; } = true;
    public string Timezone { get; set; } = "UTC";
}
