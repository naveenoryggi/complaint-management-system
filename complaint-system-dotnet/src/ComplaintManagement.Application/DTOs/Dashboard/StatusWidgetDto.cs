namespace ComplaintManagement.Application.DTOs.Dashboard;

/// <summary>
/// DTO for individual status widget with statistics
/// </summary>
public class StatusWidgetDto
{
    /// <summary>
    /// Status master ID
    /// </summary>
    public Guid StatusId { get; set; }

    /// <summary>
    /// Status code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Status name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Status description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Color code for visual representation
    /// </summary>
    public string ColorCode { get; set; } = "#6366f1";

    /// <summary>
    /// Icon class (Bootstrap Icons or custom)
    /// </summary>
    public string IconClass { get; set; } = "bi-circle";

    /// <summary>
    /// Current count of complaints with this status
    /// </summary>
    public int CurrentCount { get; set; }

    /// <summary>
    /// Count from previous period (for trend calculation)
    /// </summary>
    public int PreviousCount { get; set; }

    /// <summary>
    /// Percentage change from previous period
    /// </summary>
    public decimal PercentageChange { get; set; }

    /// <summary>
    /// Trend direction (up, down, stable)
    /// </summary>
    public string Trend { get; set; } = "stable";

    /// <summary>
    /// Is this a final/terminal status
    /// </summary>
    public bool IsFinal { get; set; }

    /// <summary>
    /// Average time in this status (in hours)
    /// </summary>
    public double? AverageTimeInStatus { get; set; }
}
