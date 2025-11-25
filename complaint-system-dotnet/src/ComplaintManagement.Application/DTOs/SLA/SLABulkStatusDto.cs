namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// Bulk SLA status request
/// </summary>
public class SLABulkStatusRequest
{
    public List<Guid> ComplaintIds { get; set; } = new();
}

/// <summary>
/// Bulk SLA status response
/// </summary>
public class SLABulkStatusResponse
{
    public Dictionary<Guid, SLAStatusSummaryDto> Statuses { get; set; } = new();
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
}

/// <summary>
/// SLA status summary for complaint list display
/// Lightweight version for tables/grids
/// </summary>
public class SLAStatusSummaryDto
{
    public Guid ComplaintId { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;

    // Primary deadline (resolution or response)
    public DateTime? PrimaryDeadline { get; set; }
    public int RemainingMinutes { get; set; }
    public double PercentageComplete { get; set; }

    // Status indicators
    public string Status { get; set; } = "OnTrack"; // OnTrack, Warning, Critical, Breached
    public string StatusColor { get; set; } = "#4CAF50";
    public string BadgeText { get; set; } = string.Empty;
    public string TimeRemaining { get; set; } = string.Empty; // "2h 30m", "OVERDUE 1h 15m"
    public string UrgencyLevel { get; set; } = "low"; // low, medium, high, critical

    // Flags
    public bool IsBreached { get; set; }
    public bool IsWarning { get; set; }
    public bool IsCritical { get; set; }
    public bool IsPaused { get; set; }
}
