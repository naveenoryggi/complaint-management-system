using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class PasswordResetToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public Guid UserId { get; set; }

    public string Email { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }

    public string? RequestIpAddress { get; set; }

    public string? ResetIpAddress { get; set; }

    public string? RequestUserAgent { get; set; }

    public string? ResetUserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual User User { get; set; } = null!;
}
