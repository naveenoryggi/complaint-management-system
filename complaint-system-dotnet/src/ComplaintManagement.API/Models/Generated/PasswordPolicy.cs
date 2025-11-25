using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class PasswordPolicy
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public int MinimumLength { get; set; }

    public bool RequireUppercase { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireDigit { get; set; }

    public bool RequireSpecialCharacter { get; set; }

    public int PasswordExpirationDays { get; set; }

    public int PasswordExpirationWarningDays { get; set; }

    public int MaxFailedLoginAttempts { get; set; }

    public int AccountLockoutDurationMinutes { get; set; }

    public int PasswordHistoryCount { get; set; }

    public int MinimumPasswordAgeDays { get; set; }

    public bool EnablePasswordComplexity { get; set; }

    public bool AllowSkipPasswordChange { get; set; }

    public bool SendPasswordExpirationEmails { get; set; }

    public bool SendPasswordSetEmails { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Company Company { get; set; } = null!;
}
