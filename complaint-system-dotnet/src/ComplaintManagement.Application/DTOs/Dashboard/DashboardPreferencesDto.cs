namespace ComplaintManagement.Application.DTOs.Dashboard;

/// <summary>
/// DTO for user dashboard preferences
/// </summary>
public class DashboardPreferencesDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<Guid> StatusWidgets { get; set; } = new();
    public string Layout { get; set; } = "grid-4";
    public bool ShowTrends { get; set; } = true;
    public bool ShowPercentages { get; set; } = true;
    public int AutoRefreshInterval { get; set; } = 0;
    public int DateRangeDays { get; set; } = 30;
    public string? Theme { get; set; }
    public string? WidgetConfig { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
