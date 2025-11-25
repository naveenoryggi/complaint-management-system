namespace ComplaintManagement.Application.DTOs.Dashboard;

/// <summary>
/// DTO for complete dashboard statistics
/// </summary>
public class DashboardStatisticsDto
{
    /// <summary>
    /// Status widgets with individual statistics
    /// </summary>
    public List<StatusWidgetDto> StatusWidgets { get; set; } = new();

    /// <summary>
    /// Total complaints count
    /// </summary>
    public int TotalComplaints { get; set; }

    /// <summary>
    /// Active complaints (non-final status)
    /// </summary>
    public int ActiveComplaints { get; set; }

    /// <summary>
    /// Completed complaints (final status)
    /// </summary>
    public int CompletedComplaints { get; set; }

    /// <summary>
    /// Overdue complaints (past SLA)
    /// </summary>
    public int OverdueComplaints { get; set; }

    /// <summary>
    /// Complaints created today
    /// </summary>
    public int TodayComplaints { get; set; }

    /// <summary>
    /// Complaints created this week
    /// </summary>
    public int WeekComplaints { get; set; }

    /// <summary>
    /// Complaints created this month
    /// </summary>
    public int MonthComplaints { get; set; }

    /// <summary>
    /// Average resolution time in hours
    /// </summary>
    public double? AverageResolutionTime { get; set; }

    /// <summary>
    /// Date range for which statistics were calculated
    /// </summary>
    public int DateRangeDays { get; set; }

    /// <summary>
    /// Timestamp when statistics were generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}
