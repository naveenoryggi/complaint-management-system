namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// SLA Level/Tier DTO
/// </summary>
public class SLALevelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public string ColorCode { get; set; } = "#4CAF50";
    public int DefaultResponseTime { get; set; }
    public string ResponseTimeUnit { get; set; } = "Hours";
    public int DefaultResolutionTime { get; set; }
    public string ResolutionTimeUnit { get; set; } = "Hours";
    public int ResponseTimeInMinutes { get; set; }
    public int ResolutionTimeInMinutes { get; set; }
    public string ResponseTimeDisplay { get; set; } = string.Empty;
    public string ResolutionTimeDisplay { get; set; } = string.Empty;
    public Guid? CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request to create a new SLA level
/// </summary>
public class CreateSLALevelRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public string ColorCode { get; set; } = "#4CAF50";
    public int DefaultResponseTime { get; set; }
    public string ResponseTimeUnit { get; set; } = "Hours";
    public int DefaultResolutionTime { get; set; }
    public string ResolutionTimeUnit { get; set; } = "Hours";
}

/// <summary>
/// Request to update an existing SLA level
/// </summary>
public class UpdateSLALevelRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public string ColorCode { get; set; } = "#4CAF50";
    public int DefaultResponseTime { get; set; }
    public string ResponseTimeUnit { get; set; } = "Hours";
    public int DefaultResolutionTime { get; set; }
    public string ResolutionTimeUnit { get; set; } = "Hours";
}
