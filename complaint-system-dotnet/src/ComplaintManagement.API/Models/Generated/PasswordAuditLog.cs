using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class PasswordAuditLog
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Action { get; set; } = null!;

    public Guid? PerformedBy { get; set; }

    public bool Success { get; set; }

    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual User? PerformedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
