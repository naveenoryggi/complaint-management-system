using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class PasswordHistory
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
