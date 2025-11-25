using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Settings;

/// <summary>
/// User-specific dashboard preferences
/// </summary>
public class DashboardPreferences : BaseEntity
{
    /// <summary>
    /// User ID who owns these preferences
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Selected status widgets (JSON array of status IDs)
    /// </summary>
    public string StatusWidgets { get; set; } = "[]";

    /// <summary>
    /// Dashboard layout type (grid-4, grid-3, list, etc.)
    /// </summary>
    public string Layout { get; set; } = "grid-4";

    /// <summary>
    /// Show trend indicators on widgets
    /// </summary>
    public bool ShowTrends { get; set; } = true;

    /// <summary>
    /// Show percentage changes
    /// </summary>
    public bool ShowPercentages { get; set; } = true;

    /// <summary>
    /// Auto-refresh interval in seconds (0 = disabled)
    /// </summary>
    public int AutoRefreshInterval { get; set; } = 0;

    /// <summary>
    /// Date range for statistics (7, 30, 90 days)
    /// </summary>
    public int DateRangeDays { get; set; } = 30;

    /// <summary>
    /// Theme preference (light, dark, auto)
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// Additional widget configuration (JSON)
    /// </summary>
    public string? WidgetConfig { get; set; }

    // Navigation properties
    /// <summary>
    /// User who owns these preferences
    /// </summary>
    public User? User { get; set; }
}
