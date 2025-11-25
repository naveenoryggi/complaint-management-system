namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// SLA timeline DTO showing SLA events over time
/// </summary>
public class SLATimelineDto
{
    public Guid ComplaintId { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;

    public List<SLATimelineEventDto> Events { get; set; } = new();

    // Summary
    public DateTime StartTime { get; set; }
    public DateTime? ResponseDeadline { get; set; }
    public DateTime? ResolutionDeadline { get; set; }
    public bool ResponseBreached { get; set; }
    public bool ResolutionBreached { get; set; }
    public int TotalPausedMinutes { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
}

/// <summary>
/// SLA timeline event
/// </summary>
public class SLATimelineEventDto
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty; // Started, ResponseMet, ResponseBreached, Paused, Resumed, ResolutionMet, ResolutionBreached, EscalationTriggered
    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string Icon { get; set; } = "bi-clock"; // Bootstrap icon class
    public string Color { get; set; } = "#6c757d"; // Gray for neutral events
}
