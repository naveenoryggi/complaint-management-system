namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// SLA calculation result DTO
/// </summary>
public class SLACalculationDto
{
    public Guid ComplaintId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime ResponseDeadline { get; set; }
    public DateTime ResolutionDeadline { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
    public int ElapsedMinutes { get; set; }
    public int RemainingMinutes { get; set; }
    public double PercentageComplete { get; set; }
    public bool IsBreached { get; set; }
    public bool IsWarning { get; set; }
    public string Status { get; set; } = "OnTrack"; // OnTrack, Warning, Breached
    public string TimeRemaining { get; set; } = string.Empty;
    public bool IsPaused { get; set; }
    public string? PauseReason { get; set; }
    public DateTime? PausedAt { get; set; }
}

/// <summary>
/// SLA timer/countdown DTO for real-time display
/// </summary>
public class SLATimerDto
{
    public Guid ComplaintId { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public DateTime ResponseDeadline { get; set; }
    public DateTime ResolutionDeadline { get; set; }
    public string TimeUntilResponse { get; set; } = string.Empty;
    public string TimeUntilResolution { get; set; } = string.Empty;
    public double ResponseProgress { get; set; }
    public double ResolutionProgress { get; set; }
    public string StatusColor { get; set; } = "#4CAF50";
    public bool ShowWarning { get; set; }
    public bool IsCritical { get; set; }
}

/// <summary>
/// SLA breach notification DTO
/// </summary>
public class SLABreachDto
{
    public Guid ComplaintId { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public string BreachType { get; set; } = "Resolution"; // Response or Resolution
    public DateTime BreachedAt { get; set; }
    public int MinutesOverdue { get; set; }
    public string SLALevelName { get; set; } = string.Empty;
    public Guid AssignedToUserId { get; set; }
    public string AssignedToUserName { get; set; } = string.Empty;
    public bool EscalationTriggered { get; set; }
}
