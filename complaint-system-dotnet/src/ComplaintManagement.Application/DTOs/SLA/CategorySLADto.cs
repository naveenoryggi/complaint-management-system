namespace ComplaintManagement.Application.DTOs.SLA;

/// <summary>
/// Category to SLA Level mapping DTO
/// </summary>
public class CategorySLADto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
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
/// Request to create category SLA mapping
/// </summary>
public class CreateCategorySLARequest
{
    public Guid CategoryId { get; set; }
    public Guid SLALevelId { get; set; }
    public int? OverrideResponseTime { get; set; }
    public int? OverrideResolutionTime { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request to update category SLA mapping
/// </summary>
public class UpdateCategorySLARequest
{
    public Guid SLALevelId { get; set; }
    public int? OverrideResponseTime { get; set; }
    public int? OverrideResolutionTime { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Bulk request to update multiple category SLA mappings
/// </summary>
public class BulkUpdateCategorySLARequest
{
    public List<CategorySLAMapping> Mappings { get; set; } = new();
}

public class CategorySLAMapping
{
    public Guid CategoryId { get; set; }
    public Guid SLALevelId { get; set; }
    public int? OverrideResponseTime { get; set; }
    public int? OverrideResolutionTime { get; set; }
    public bool IsActive { get; set; } = true;
}
