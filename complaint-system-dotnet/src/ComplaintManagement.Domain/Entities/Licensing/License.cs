using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Licensing;

/// <summary>
/// Represents a software license for a company.
/// Controls which modules and features are available.
/// </summary>
public class License : BaseEntity
{
    /// <summary>
    /// Company this license belongs to
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Encrypted license key for validation
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// Type of license (Trial, Standard, Professional, Enterprise)
    /// </summary>
    public LicenseType Type { get; set; } = LicenseType.Trial;

    /// <summary>
    /// Current status of the license
    /// </summary>
    public LicenseStatus Status { get; set; } = LicenseStatus.Pending;

    /// <summary>
    /// Friendly name for the license
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Description or notes about the license
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Date when the license was issued
    /// </summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>
    /// Date when the license expires (null = perpetual)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Grace period in days after expiration
    /// </summary>
    public int GracePeriodDays { get; set; } = 7;

    /// <summary>
    /// Maximum number of users allowed (null = unlimited)
    /// </summary>
    public int? MaxUsers { get; set; }

    /// <summary>
    /// Maximum number of customers allowed (null = unlimited)
    /// </summary>
    public int? MaxCustomers { get; set; }

    /// <summary>
    /// Maximum number of products allowed (null = unlimited)
    /// </summary>
    public int? MaxProducts { get; set; }

    /// <summary>
    /// Maximum number of assets allowed (null = unlimited)
    /// </summary>
    public int? MaxAssets { get; set; }

    /// <summary>
    /// Maximum number of contracts allowed (null = unlimited)
    /// </summary>
    public int? MaxContracts { get; set; }

    /// <summary>
    /// Whether the license is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Email of the person who activated the license
    /// </summary>
    public string? ActivatedBy { get; set; }

    /// <summary>
    /// Date when the license was activated
    /// </summary>
    public DateTime? ActivatedAt { get; set; }

    /// <summary>
    /// Additional license data stored as JSON
    /// (e.g., custom limits, features, metadata)
    /// </summary>
    public string? LicenseData { get; set; }

    /// <summary>
    /// License signature for validation
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// Hardware/Machine ID for binding (optional)
    /// </summary>
    public string? MachineId { get; set; }

    /// <summary>
    /// Last validation check timestamp
    /// </summary>
    public DateTime? LastValidatedAt { get; set; }

    /// <summary>
    /// Number of validation failures
    /// </summary>
    public int ValidationFailures { get; set; }

    // Navigation Properties

    /// <summary>
    /// Company that owns this license
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Module activations for this license
    /// </summary>
    public ICollection<LicenseModuleActivation> ModuleActivations { get; set; } = new List<LicenseModuleActivation>();
}
