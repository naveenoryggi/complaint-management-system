namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// Applicable SLA configuration DTO
/// Shows which SLA will be applied for a given category/priority combination
/// </summary>
public class ApplicableSLADto
{
    public Guid? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public Guid? PriorityId { get; set; }
    public string PriorityName { get; set; } = string.Empty;

    // SLA Configuration
    public Guid? SLALevelId { get; set; }
    public string SLALevelName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // PriorityMapping, CategoryMapping, SystemDefault
    public int Priority { get; set; } // 1=Highest, 2=Medium, 3=Lowest

    // Time Limits
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
    public string ResponseTimeDisplay { get; set; } = string.Empty; // "2 hours"
    public string ResolutionTimeDisplay { get; set; } = string.Empty; // "24 hours"

    // Settings
    public bool WorkingHoursOnly { get; set; }
    public bool IsActive { get; set; }

    // Additional Info
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Request for checking applicable SLA
/// </summary>
public class ApplicableSLARequest
{
    public Guid? CategoryId { get; set; }
    public Guid? PriorityId { get; set; }
}
