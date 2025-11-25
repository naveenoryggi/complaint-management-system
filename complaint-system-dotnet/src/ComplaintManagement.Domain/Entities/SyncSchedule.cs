using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities;

public class SyncSchedule : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string ScheduleType { get; set; } = null!; // "Daily", "Weekly", "Monthly"

    /// <summary>
    /// Time of day to run sync (24-hour format HH:mm)
    /// </summary>
    [Required]
    [StringLength(5)]
    public string TimeOfDay { get; set; } = null!; // e.g., "02:00", "14:30"

    /// <summary>
    /// For Weekly: Day of week (0=Sunday, 6=Saturday)
    /// For Monthly: Day of month (1-31)
    /// For Daily: Not used
    /// </summary>
    public int? DayValue { get; set; }

    public bool IsEnabled { get; set; } = true;

    public DateTime? LastRunAt { get; set; }

    public DateTime? NextRunAt { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    // Navigation property
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;
}
