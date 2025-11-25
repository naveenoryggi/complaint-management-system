using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class DashboardPreference
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string StatusWidgets { get; set; } = null!;

    public string Layout { get; set; } = null!;

    public bool ShowTrends { get; set; }

    public bool ShowPercentages { get; set; }

    public int AutoRefreshInterval { get; set; }

    public int DateRangeDays { get; set; }

    public string? Theme { get; set; }

    public string? WidgetConfig { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual User User { get; set; } = null!;
}
