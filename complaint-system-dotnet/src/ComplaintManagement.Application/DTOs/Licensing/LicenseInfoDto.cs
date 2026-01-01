using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.DTOs.Licensing;

/// <summary>
/// License information DTO for API responses
/// </summary>
public class LicenseInfoDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string? Name { get; set; }
    public string LicenseType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public string? ActivatedBy { get; set; }
    public bool IsActive { get; set; }
    public int DaysRemaining { get; set; }
    public bool IsExpired { get; set; }
    public bool IsInGracePeriod { get; set; }

    // Limits
    public int? MaxUsers { get; set; }
    public int? MaxCustomers { get; set; }
    public int? MaxProducts { get; set; }
    public int? MaxAssets { get; set; }
    public int? MaxContracts { get; set; }

    // Current usage
    public int CurrentUsers { get; set; }
    public int CurrentCustomers { get; set; }
    public int CurrentProducts { get; set; }
    public int CurrentAssets { get; set; }
    public int CurrentContracts { get; set; }

    // Module information
    public List<ModuleActivationDto> EnabledModules { get; set; } = new();
}

/// <summary>
/// Module activation details
/// </summary>
public class ModuleActivationDto
{
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? EnabledAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int DisplayOrder { get; set; }
    public string? ModuleConfig { get; set; }
}

/// <summary>
/// License validation result
/// </summary>
public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
    public int DaysRemaining { get; set; }
}

/// <summary>
/// Module limits information
/// </summary>
public class ModuleLimitsDto
{
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public Dictionary<string, int?> Limits { get; set; } = new();
    public Dictionary<string, int> CurrentUsage { get; set; } = new();
    public Dictionary<string, bool> LimitExceeded { get; set; } = new();
}

/// <summary>
/// Module configuration for UI
/// </summary>
public class ModuleConfigDto
{
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public string? Color { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsAvailable { get; set; }
    public int DisplayOrder { get; set; }
    public List<ModuleFeatureDto> Features { get; set; } = new();
}

/// <summary>
/// Module feature details
/// </summary>
public class ModuleFeatureDto
{
    public string FeatureCode { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
}

/// <summary>
/// License limit check result
/// </summary>
public class LicenseLimitCheckResult
{
    public bool HasExceededLimits { get; set; }
    public List<LimitViolation> Violations { get; set; } = new();
    public List<LimitWarning> Warnings { get; set; } = new();
}

public class LimitViolation
{
    public string LimitType { get; set; } = string.Empty;
    public int Limit { get; set; }
    public int Current { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class LimitWarning
{
    public string LimitType { get; set; } = string.Empty;
    public int Limit { get; set; }
    public int Current { get; set; }
    public int PercentUsed { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Module metadata for UI display (static information)
/// </summary>
public class ModuleMetadataDto
{
    public LicenseModule Module { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsCore { get; set; }
    public List<string> Features { get; set; } = new();
}

/// <summary>
/// Request to activate a license
/// </summary>
public class ActivateLicenseRequest
{
    public string LicenseKey { get; set; } = string.Empty;
}
