using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class OryggiConnectionSetting
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ServerAddress { get; set; } = null!;

    public int Port { get; set; }

    public string DatabaseName { get; set; } = null!;

    public string EncryptedUsername { get; set; } = null!;

    public string EncryptedPassword { get; set; } = null!;

    public bool UseWindowsAuthentication { get; set; }

    public bool EncryptConnection { get; set; }

    public bool TrustServerCertificate { get; set; }

    public int ConnectionTimeout { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastTestedAt { get; set; }

    public string? LastTestResult { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
