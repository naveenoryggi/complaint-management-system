using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class SmsGatewaySetting
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Provider { get; set; } = null!;

    public string? ApiUrl { get; set; }

    public string? AccountSid { get; set; }

    public string? AuthToken { get; set; }

    public string? FromNumber { get; set; }

    public string? SenderName { get; set; }

    public bool IsActive { get; set; }

    public bool IsDefault { get; set; }

    public int? MaxSmsPerHour { get; set; }

    public decimal? CostPerSms { get; set; }

    public int TimeoutSeconds { get; set; }

    public Guid? CompanyId { get; set; }

    public string? AdditionalConfig { get; set; }

    public string? TestNotes { get; set; }

    public DateTime? LastTestedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Company? Company { get; set; }
}
