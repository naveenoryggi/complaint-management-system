using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Licensing;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for managing software licenses and module activations
/// </summary>
public interface ILicenseService
{
    /// <summary>
    /// Checks if a specific module is enabled for a company
    /// </summary>
    Task<bool> IsModuleEnabledAsync(Guid companyId, LicenseModule module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all enabled modules for a company
    /// </summary>
    Task<List<LicenseModule>> GetEnabledModulesAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets license information for a company
    /// </summary>
    Task<Result<LicenseInfoDto>> GetLicenseInfoAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a license using a license key
    /// </summary>
    Task<Result<LicenseInfoDto>> ActivateLicenseAsync(Guid companyId, string licenseKey, string activatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the current license for a company
    /// </summary>
    Task<Result<LicenseValidationResult>> ValidateLicenseAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets module limits for a specific module
    /// </summary>
    Task<Result<ModuleLimitsDto>> GetModuleLimitsAsync(Guid companyId, LicenseModule module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all module configurations for a company
    /// </summary>
    Task<Result<List<ModuleConfigDto>>> GetModuleConfigurationsAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a trial license for a new company
    /// </summary>
    Task<Result<LicenseInfoDto>> CreateTrialLicenseAsync(Guid companyId, int trialDays = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the company has exceeded any license limits
    /// </summary>
    Task<Result<LicenseLimitCheckResult>> CheckLicenseLimitsAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets module metadata for UI display
    /// </summary>
    List<ModuleMetadataDto> GetAllModulesMetadata();

    /// <summary>
    /// Enables a module for a company's license
    /// </summary>
    Task<Result<bool>> EnableModuleAsync(Guid companyId, string moduleCode, CancellationToken cancellationToken = default);
}
