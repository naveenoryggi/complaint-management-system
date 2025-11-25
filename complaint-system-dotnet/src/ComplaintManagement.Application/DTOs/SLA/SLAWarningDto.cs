namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// SLA warning DTO for complaints approaching SLA breach
/// </summary>
public class SLAWarningDto
{
    public Guid ComplaintId { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string PriorityName { get; set; } = string.Empty;

    // SLA Info
    public DateTime? ResponseDeadline { get; set; }
    public DateTime? ResolutionDeadline { get; set; }
    public int ResponseRemainingMinutes { get; set; }
    public int ResolutionRemainingMinutes { get; set; }
    public double PercentageComplete { get; set; }

    // Warning Level
    public string WarningLevel { get; set; } = string.Empty; // Warning (70-90%), Critical (90-100%), Breached (>100%)
    public string WarningColor { get; set; } = "#ffc107"; // Yellow, Orange, or Red
    public string TimeRemaining { get; set; } = string.Empty;

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public string AssignedToUserName { get; set; } = string.Empty;

    // Metadata
    public DateTime SubmittedAt { get; set; }
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// SLA warnings response
/// </summary>
public class SLAWarningsResponse
{
    public List<SLAWarningDto> Warnings { get; set; } = new();

    // Summary
    public int TotalWarnings { get; set; }
    public int CriticalCount { get; set; }
    public int WarningCount { get; set; }
    public int BreachedCount { get; set; }

    public DateTime GeneratedAt { get; set; }
}
