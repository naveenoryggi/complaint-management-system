using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Licensing;

/// <summary>
/// Represents the activation of a specific module for a license.
/// Each license can have multiple module activations.
/// </summary>
public class LicenseModuleActivation : BaseEntity
{
    /// <summary>
    /// License this activation belongs to
    /// </summary>
    public Guid LicenseId { get; set; }

    /// <summary>
    /// The module being activated
    /// </summary>
    public LicenseModule Module { get; set; }

    /// <summary>
    /// Whether the module is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Date when the module was enabled
    /// </summary>
    public DateTime? EnabledAt { get; set; }

    /// <summary>
    /// Module-specific expiration date (overrides license expiry if set)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Module-specific configuration stored as JSON
    /// (e.g., feature flags, limits, settings)
    /// </summary>
    public string? ModuleConfig { get; set; }

    /// <summary>
    /// Module-specific limits stored as JSON
    /// (e.g., max items for this module)
    /// </summary>
    public string? ModuleLimits { get; set; }

    /// <summary>
    /// Display order for UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Notes about this module activation
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties

    /// <summary>
    /// Parent license
    /// </summary>
    public License License { get; set; } = null!;
}
