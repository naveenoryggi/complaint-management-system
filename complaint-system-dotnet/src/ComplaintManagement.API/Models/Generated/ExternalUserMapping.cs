using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ExternalUserMapping
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid AuthenticationProviderId { get; set; }

    public string ExternalUserId { get; set; } = null!;

    public string? ExternalUsername { get; set; }

    public string? ExternalEmail { get; set; }

    public string? ExternalDisplayName { get; set; }

    public string? Attributes { get; set; }

    public string? ExternalGroups { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public bool? LastSyncSuccess { get; set; }

    public string? LastSyncDetails { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual AuthenticationProvider AuthenticationProvider { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
