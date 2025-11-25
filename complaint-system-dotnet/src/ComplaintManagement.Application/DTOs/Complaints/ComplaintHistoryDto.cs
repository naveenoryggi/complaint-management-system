namespace ComplaintManagement.Application.DTOs.Complaints;

public class ComplaintHistoryDto
{
    public Guid ComplaintId { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public List<ComplaintHistoryEventDto> Events { get; set; } = new();
}

public class ComplaintHistoryEventDto
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty; // Comment, Escalation, StatusChange, Assignment, etc.
    public string Description { get; set; } = string.Empty;
    public string? PerformedBy { get; set; }
    public string? PerformedByName { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
