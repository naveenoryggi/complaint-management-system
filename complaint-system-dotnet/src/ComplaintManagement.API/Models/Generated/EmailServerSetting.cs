using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EmailServerSetting
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Host { get; set; } = null!;

    public int Port { get; set; }

    public bool UseSsl { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string FromEmail { get; set; } = null!;

    public string? FromName { get; set; }

    public string? ReplyToEmail { get; set; }

    public bool IsActive { get; set; }

    public bool IsDefault { get; set; }

    public int? MaxEmailsPerHour { get; set; }

    public int TimeoutSeconds { get; set; }

    public Guid? CompanyId { get; set; }

    public string? TestNotes { get; set; }

    public DateTime? LastTestedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public int AuthenticationType { get; set; }

    public string? OauthAccessToken { get; set; }

    public string? OauthClientId { get; set; }

    public string? OauthClientSecret { get; set; }

    public string? OauthRefreshToken { get; set; }

    public string? OauthScopes { get; set; }

    public string? OauthTenantId { get; set; }

    public DateTime? OauthTokenExpiresAt { get; set; }

    public int? OauthTokenRefreshIntervalMinutes { get; set; }

    public virtual Company? Company { get; set; }
}
