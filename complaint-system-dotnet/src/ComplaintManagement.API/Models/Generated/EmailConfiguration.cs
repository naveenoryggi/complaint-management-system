using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EmailConfiguration
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string ImapHost { get; set; } = null!;

    public int ImapPort { get; set; }

    public bool ImapUseSsl { get; set; }

    public string ImapUsername { get; set; } = null!;

    public string ImapPassword { get; set; } = null!;

    public string ImapFolder { get; set; } = null!;

    public string SmtpHost { get; set; } = null!;

    public int SmtpPort { get; set; }

    public bool SmtpUseSsl { get; set; }

    public string SmtpUsername { get; set; } = null!;

    public string SmtpPassword { get; set; } = null!;

    public string FromEmail { get; set; } = null!;

    public string FromName { get; set; } = null!;

    public int PollingIntervalMinutes { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime? LastPolledAt { get; set; }

    public bool SendAutoAcknowledgement { get; set; }

    public string? AutoAcknowledgementTemplateId { get; set; }

    public Guid? AutoAcknowledgementTemplateId1 { get; set; }

    public bool EnableThreading { get; set; }

    public int ThreadTimeoutDays { get; set; }

    public long MaxAttachmentSizeBytes { get; set; }

    public string AllowedAttachmentExtensions { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

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

    public int? PollingIntervalSeconds { get; set; }

    public int? OauthTokenRefreshIntervalMinutes { get; set; }

    public int? SmtpAuthenticationType { get; set; }

    public string? SmtpSeparateFromEmail { get; set; }

    public string? SmtpSeparateFromName { get; set; }

    public string? SmtpSeparateOauthAccessToken { get; set; }

    public string? SmtpSeparateOauthClientId { get; set; }

    public string? SmtpSeparateOauthClientSecret { get; set; }

    public string? SmtpSeparateOauthRefreshToken { get; set; }

    public string? SmtpSeparateOauthScopes { get; set; }

    public string? SmtpSeparateOauthTenantId { get; set; }

    public DateTime? SmtpSeparateOauthTokenExpiresAt { get; set; }

    public string? SmtpSeparatePassword { get; set; }

    public string? SmtpSeparateUsername { get; set; }

    public bool UseSeparateSmtpAccount { get; set; }

    public virtual CommunicationTemplate? AutoAcknowledgementTemplateId1Navigation { get; set; }

    public virtual Company Company { get; set; } = null!;
}
