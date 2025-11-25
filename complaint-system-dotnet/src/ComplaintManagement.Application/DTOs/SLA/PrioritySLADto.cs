namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// Priority to SLA Level mapping DTO
/// </summary>
public class PrioritySLADto
{
    public Guid Id { get; set; }
    public Guid PriorityId { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public Guid SLALevelId { get; set; }
    public string SLALevelName { get; set; } = string.Empty;
    public string SLALevelColorCode { get; set; } = "#4CAF50";
    public int? OverrideResponseTime { get; set; }
    public int? OverrideResolutionTime { get; set; }
    public int EffectiveResponseTimeMinutes { get; set; }
    public int EffectiveResolutionTimeMinutes { get; set; }
    public string EffectiveResponseTimeDisplay { get; set; } = string.Empty;
    public string EffectiveResolutionTimeDisplay { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request to create priority SLA mapping
/// </summary>
public class CreatePrioritySLARequest
{
    public Guid PriorityId { get; set; }
    public Guid SLALevelId { get; set; }
    public int? OverrideResponseTime { get; set; }
    public int? OverrideResolutionTime { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request to update priority SLA mapping
/// </summary>
public class UpdatePrioritySLARequest
{
    public Guid SLALevelId { get; set; }
    public int? OverrideResponseTime { get; set; }
    public int? OverrideResolutionTime { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Bulk request to update multiple priority SLA mappings
/// </summary>
public class BulkUpdatePrioritySLARequest
{
    public List<PrioritySLAMapping> Mappings { get; set; } = new();
}

public class PrioritySLAMapping
{
    public Guid PriorityId { get; set; }
    public Guid SLALevelId { get; set; }
    public int? OverrideResponseTime { get; set; }
    public int? OverrideResolutionTime { get; set; }
    public bool IsActive { get; set; } = true;
}
