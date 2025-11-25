using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.SLA;

/// <summary>
/// Global SLA system settings
/// Defines working hours, holidays, and SLA calculation rules
/// </summary>
public class SLASettings : BaseEntity
{
    /// <summary>
    /// Whether SLA system is enabled globally
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Calculate SLA only during working hours (pause timer outside working hours)
    /// </summary>
    public bool WorkingHoursOnly { get; set; } = false;

    /// <summary>
    /// Working hours start time (e.g., "09:00")
    /// </summary>
    public TimeSpan? WorkingHoursStart { get; set; } = new TimeSpan(9, 0, 0);

    /// <summary>
    /// Working hours end time (e.g., "17:00")
    /// </summary>
    public TimeSpan? WorkingHoursEnd { get; set; } = new TimeSpan(17, 0, 0);

    /// <summary>
    /// Working days as comma-separated string (0=Sunday, 1=Monday, ..., 6=Saturday)
    /// Example: "1,2,3,4,5" for Monday-Friday
    /// </summary>
    public string WorkingDays { get; set; } = "1,2,3,4,5";

    /// <summary>
    /// Automatically trigger escalation when SLA is breached
    /// </summary>
    public bool AutoEscalateOnBreach { get; set; } = true;

    /// <summary>
    /// Escalation warning threshold as percentage of SLA time (0-100)
    /// Example: 80 = trigger escalation at 80% of SLA time elapsed
    /// </summary>
    public int EscalationThresholdPercent { get; set; } = 80;

    /// <summary>
    /// Send notification before SLA breach
    /// </summary>
    public bool NotifyBeforeBreach { get; set; } = true;

    /// <summary>
    /// Minutes before breach to send warning notification
    /// </summary>
    public int NotifyBeforeBreachMinutes { get; set; } = 30;

    /// <summary>
    /// Pause SLA timer when complaint status is "Pending Info"
    /// </summary>
    public bool PauseSLAOnPendingInfo { get; set; } = true;

    /// <summary>
    /// Exclude holidays from SLA calculation
    /// </summary>
    public bool ExcludeHolidays { get; set; } = true;

    /// <summary>
    /// Timezone for SLA calculations (e.g., "UTC", "America/New_York")
    /// </summary>
    public string Timezone { get; set; } = "UTC";

    /// <summary>
    /// Company/Organization ID (for multi-tenant scenarios)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Get working days as array of integers
    /// </summary>
    public int[] GetWorkingDaysArray()
    {
        if (string.IsNullOrWhiteSpace(WorkingDays))
            return Array.Empty<int>();

        return WorkingDays
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => int.TryParse(d.Trim(), out var day) ? day : -1)
            .Where(d => d >= 0 && d <= 6)
            .ToArray();
    }

    /// <summary>
    /// Check if a specific day of week is a working day
    /// </summary>
    public bool IsWorkingDay(DayOfWeek dayOfWeek)
    {
        var workingDays = GetWorkingDaysArray();
        return workingDays.Contains((int)dayOfWeek);
    }

    /// <summary>
    /// Check if a specific time is within working hours
    /// </summary>
    public bool IsWithinWorkingHours(TimeSpan time)
    {
        if (!WorkingHoursOnly || WorkingHoursStart == null || WorkingHoursEnd == null)
            return true;

        return time >= WorkingHoursStart.Value && time < WorkingHoursEnd.Value;
    }

    /// <summary>
    /// Check if a specific datetime is within working time (day + hours)
    /// </summary>
    public bool IsWorkingTime(DateTime dateTime)
    {
        if (!WorkingHoursOnly)
            return true;

        return IsWorkingDay(dateTime.DayOfWeek) && IsWithinWorkingHours(dateTime.TimeOfDay);
    }
}
