using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Dashboard;

/// <summary>
/// Request DTO for saving dashboard preferences
/// </summary>
public class SaveDashboardPreferencesRequest
{
    /// <summary>
    /// List of status IDs to display as widgets
    /// </summary>
    [Required]
    public List<Guid> StatusWidgets { get; set; } = new();

    /// <summary>
    /// Dashboard layout (grid-4, grid-3, list, etc.)
    /// </summary>
    [Required]
    [RegularExpression(@"^(grid-[2-6]|list)$", ErrorMessage = "Layout must be grid-2, grid-3, grid-4, grid-5, grid-6, or list")]
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
    [Range(0, 3600, ErrorMessage = "Auto refresh interval must be between 0 and 3600 seconds")]
    public int AutoRefreshInterval { get; set; } = 0;

    /// <summary>
    /// Date range for statistics (days)
    /// </summary>
    [Range(1, 365, ErrorMessage = "Date range must be between 1 and 365 days")]
    public int DateRangeDays { get; set; } = 30;

    /// <summary>
    /// Theme preference (light, dark, auto)
    /// </summary>
    [RegularExpression(@"^(light|dark|auto)$", ErrorMessage = "Theme must be light, dark, or auto")]
    public string? Theme { get; set; }

    /// <summary>
    /// Additional widget configuration (JSON)
    /// </summary>
    public string? WidgetConfig { get; set; }
}
