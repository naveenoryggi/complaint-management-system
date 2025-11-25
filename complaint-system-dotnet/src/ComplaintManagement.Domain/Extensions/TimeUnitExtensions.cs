using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Extensions;

/// <summary>
/// Extension methods for TimeUnit enum
/// </summary>
public static class TimeUnitExtensions
{
    /// <summary>
    /// Convert time value and unit to total hours
    /// </summary>
    public static double ToHours(this TimeUnit timeUnit, int value)
    {
        return timeUnit switch
        {
            TimeUnit.Minutes => value / 60.0,
            TimeUnit.Hours => value,
            TimeUnit.Days => value * 24.0,
            TimeUnit.Weeks => value * 24.0 * 7.0,
            _ => value
        };
    }

    /// <summary>
    /// Convert time value and unit to total minutes
    /// </summary>
    public static double ToMinutes(this TimeUnit timeUnit, int value)
    {
        return timeUnit switch
        {
            TimeUnit.Minutes => value,
            TimeUnit.Hours => value * 60.0,
            TimeUnit.Days => value * 60.0 * 24.0,
            TimeUnit.Weeks => value * 60.0 * 24.0 * 7.0,
            _ => value
        };
    }

    /// <summary>
    /// Get display string for time unit
    /// </summary>
    public static string GetDisplayString(this TimeUnit timeUnit, int value)
    {
        var unit = timeUnit switch
        {
            TimeUnit.Minutes => value == 1 ? "minute" : "minutes",
            TimeUnit.Hours => value == 1 ? "hour" : "hours",
            TimeUnit.Days => value == 1 ? "day" : "days",
            TimeUnit.Weeks => value == 1 ? "week" : "weeks",
            _ => "hours"
        };

        return $"{value} {unit}";
    }

    /// <summary>
    /// Get short display string for time unit (e.g., "2h", "3d", "1w")
    /// </summary>
    public static string GetShortDisplayString(this TimeUnit timeUnit, int value)
    {
        var unit = timeUnit switch
        {
            TimeUnit.Minutes => "m",
            TimeUnit.Hours => "h",
            TimeUnit.Days => "d",
            TimeUnit.Weeks => "w",
            _ => "h"
        };

        return $"{value}{unit}";
    }
}
