namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// SLA status display DTO for single complaint
/// Used in complaint detail view SLA info panel
/// </summary>
public class SLAStatusDisplayDto
{
    public Guid ComplaintId { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;

    // SLA Configuration
    public string SLALevelName { get; set; } = string.Empty;
    public string SLASource { get; set; } = string.Empty; // "Priority Mapping", "Category Mapping", "System Default"
    public bool WorkingHoursApplied { get; set; }

    // Response SLA
    public DateTime? ResponseDeadline { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public double ResponseProgress { get; set; }
    public bool ResponseBreached { get; set; }
    public int ResponseRemainingMinutes { get; set; }
    public string ResponseTimeRemaining { get; set; } = string.Empty;

    // Resolution SLA
    public DateTime? ResolutionDeadline { get; set; }
    public int ResolutionTimeMinutes { get; set; }
    public double ResolutionProgress { get; set; }
    public bool ResolutionBreached { get; set; }
    public int ResolutionRemainingMinutes { get; set; }
    public string ResolutionTimeRemaining { get; set; } = string.Empty;

    // Overall Status
    public string Status { get; set; } = "OnTrack"; // OnTrack, Warning, Critical, Breached
    public string StatusColor { get; set; } = "#4CAF50"; // Green, Yellow, Orange, Red
    public string BadgeText { get; set; } = string.Empty; // "On Track", "Warning", "Critical", "BREACHED"
    public bool ShowWarning { get; set; }
    public bool IsCritical { get; set; }

    // Pause Status
    public bool IsPaused { get; set; }
    public string? PauseReason { get; set; }
    public DateTime? PausedAt { get; set; }

    // Calculation Info
    public DateTime CalculatedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}
