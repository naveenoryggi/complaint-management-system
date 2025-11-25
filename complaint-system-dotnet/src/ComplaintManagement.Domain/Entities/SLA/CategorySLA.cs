using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.SLA;

/// <summary>
/// Maps complaint categories to SLA levels
/// Defines which SLA applies to complaints in each category
/// </summary>
public class CategorySLA : BaseEntity
{
    /// <summary>
    /// Category ID
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// SLA Level ID
    /// </summary>
    public Guid SLALevelId { get; set; }

    /// <summary>
    /// Override response time (optional - if null, uses SLA level default)
    /// </summary>
    public int? OverrideResponseTime { get; set; }

    /// <summary>
    /// Override resolution time (optional - if null, uses SLA level default)
    /// </summary>
    public int? OverrideResolutionTime { get; set; }

    /// <summary>
    /// Whether this category SLA is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Category
    /// </summary>
    public ComplaintCategory Category { get; set; } = null!;

    /// <summary>
    /// SLA Level
    /// </summary>
    public SLALevel SLALevel { get; set; } = null!;

    /// <summary>
    /// Get effective response time in minutes
    /// </summary>
    public int GetEffectiveResponseTimeMinutes()
    {
        if (OverrideResponseTime.HasValue)
        {
            return OverrideResponseTime.Value * 60; // Assuming override is in hours
        }
        return SLALevel.GetResponseTimeInMinutes();
    }

    /// <summary>
    /// Get effective resolution time in minutes
    /// </summary>
    public int GetEffectiveResolutionTimeMinutes()
    {
        if (OverrideResolutionTime.HasValue)
        {
            return OverrideResolutionTime.Value * 60; // Assuming override is in hours
        }
        return SLALevel.GetResolutionTimeInMinutes();
    }
}
