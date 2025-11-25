using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class SystemConfiguration
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public int OauthTokenRefreshIntervalMinutes { get; set; }

    public int OauthTokenExpiryWarningDays { get; set; }

    public int DefaultEmailPollingIntervalSeconds { get; set; }

    public int MaxEmailsFetchPerPoll { get; set; }

    public bool AutoResponseEnabled { get; set; }

    public int AutoResponseMaxRetryAttempts { get; set; }

    public int AutoResponseRetryDelaySeconds { get; set; }

    public bool EmailRateLimitingEnabled { get; set; }

    public int MaxEmailsPerHour { get; set; }

    public bool StatusChangeNotificationsEnabled { get; set; }

    public bool AssignmentNotificationsEnabled { get; set; }

    public bool EscalationNotificationsEnabled { get; set; }

    public string DefaultTimezone { get; set; } = null!;

    public string DateFormat { get; set; } = null!;

    public string TimeFormat { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }
}
