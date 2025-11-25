using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.SLA;

/// <summary>
/// SLA Level/Tier definition (e.g., Standard, Premium, Enterprise)
/// Defines response and resolution time targets for complaints
/// </summary>
public class SLALevel : BaseEntity
{
    /// <summary>
    /// SLA level name (e.g., "Standard", "Premium", "Enterprise")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of this SLA level
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order (lower numbers appear first)
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Whether this SLA level is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Color code for visual identification (hex format, e.g., "#4CAF50")
    /// </summary>
    public string ColorCode { get; set; } = "#4CAF50";

    /// <summary>
    /// Default response time value (first contact with customer)
    /// </summary>
    public int DefaultResponseTime { get; set; }

    /// <summary>
    /// Time unit for response time
    /// </summary>
    public TimeUnit ResponseTimeUnit { get; set; } = TimeUnit.Hours;

    /// <summary>
    /// Default resolution time value (issue fully resolved)
    /// </summary>
    public int DefaultResolutionTime { get; set; }

    /// <summary>
    /// Time unit for resolution time
    /// </summary>
    public TimeUnit ResolutionTimeUnit { get; set; } = TimeUnit.Hours;

    /// <summary>
    /// Company/Organization ID (for multi-tenant scenarios)
    /// </summary>
    public Guid? CompanyId { get; set; }

    // Navigation properties
    /// <summary>
    /// Categories assigned to this SLA level
    /// </summary>
    public ICollection<CategorySLA> CategorySLAs { get; set; } = new List<CategorySLA>();

    /// <summary>
    /// Priorities assigned to this SLA level
    /// </summary>
    public ICollection<PrioritySLA> PrioritySLAs { get; set; } = new List<PrioritySLA>();

    /// <summary>
    /// Calculate response time in minutes
    /// </summary>
    public int GetResponseTimeInMinutes()
    {
        return ResponseTimeUnit switch
        {
            TimeUnit.Minutes => DefaultResponseTime,
            TimeUnit.Hours => DefaultResponseTime * 60,
            TimeUnit.Days => DefaultResponseTime * 24 * 60,
            TimeUnit.Weeks => DefaultResponseTime * 7 * 24 * 60,
            _ => DefaultResponseTime * 60
        };
    }

    /// <summary>
    /// Calculate resolution time in minutes
    /// </summary>
    public int GetResolutionTimeInMinutes()
    {
        return ResolutionTimeUnit switch
        {
            TimeUnit.Minutes => DefaultResolutionTime,
            TimeUnit.Hours => DefaultResolutionTime * 60,
            TimeUnit.Days => DefaultResolutionTime * 24 * 60,
            TimeUnit.Weeks => DefaultResolutionTime * 7 * 24 * 60,
            _ => DefaultResolutionTime * 60
        };
    }

    /// <summary>
    /// Get formatted response time display
    /// </summary>
    public string GetResponseTimeDisplay()
    {
        return $"{DefaultResponseTime} {GetTimeUnitLabel(ResponseTimeUnit, DefaultResponseTime)}";
    }

    /// <summary>
    /// Get formatted resolution time display
    /// </summary>
    public string GetResolutionTimeDisplay()
    {
        return $"{DefaultResolutionTime} {GetTimeUnitLabel(ResolutionTimeUnit, DefaultResolutionTime)}";
    }

    private static string GetTimeUnitLabel(TimeUnit unit, int value)
    {
        var suffix = value == 1 ? "" : "s";
        return unit switch
        {
            TimeUnit.Minutes => $"minute{suffix}",
            TimeUnit.Hours => $"hour{suffix}",
            TimeUnit.Days => $"day{suffix}",
            TimeUnit.Weeks => $"week{suffix}",
            _ => "hour" + suffix
        };
    }
}
