using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Licensing;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for managing software licenses and module configurations
/// </summary>
[ApiController]
[Route("api/licenses")]
[Authorize]
public class LicenseController : ControllerBase
{
    private readonly ILicenseService _licenseService;
    private readonly ILogger<LicenseController> _logger;

    public LicenseController(ILicenseService licenseService, ILogger<LicenseController> logger)
    {
        _licenseService = licenseService;
        _logger = logger;
    }

    /// <summary>
    /// Get license information for the current company
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType(typeof(Result<LicenseInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLicenseInfo(CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _licenseService.GetLicenseInfoAsync(companyId.Value, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license info");
            return StatusCode(500, new { message = "Error retrieving license information" });
        }
    }

    /// <summary>
    /// Get all available modules with their enabled status
    /// </summary>
    [HttpGet("modules")]
    [ProducesResponseType(typeof(Result<List<ModuleConfigDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModules(CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _licenseService.GetModuleConfigurationsAsync(companyId.Value, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting modules");
            return StatusCode(500, new { message = "Error retrieving modules" });
        }
    }

    /// <summary>
    /// Get all module metadata (static information about all available modules)
    /// </summary>
    [HttpGet("modules/metadata")]
    [ProducesResponseType(typeof(List<ModuleMetadataDto>), StatusCodes.Status200OK)]
    public IActionResult GetModulesMetadata()
    {
        try
        {
            var metadata = _licenseService.GetAllModulesMetadata();
            return Ok(Result<List<ModuleMetadataDto>>.Success(metadata));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting modules metadata");
            return StatusCode(500, new { message = "Error retrieving modules metadata" });
        }
    }

    /// <summary>
    /// Check if a specific module is enabled for the current company
    /// </summary>
    [HttpGet("modules/{moduleCode}/enabled")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsModuleEnabled(string moduleCode, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            if (!Enum.TryParse<Domain.Enums.LicenseModule>(moduleCode, true, out var module))
            {
                // Try to find by code
                var metadata = _licenseService.GetAllModulesMetadata()
                    .FirstOrDefault(m => m.Code.Equals(moduleCode, StringComparison.OrdinalIgnoreCase));

                if (metadata == null)
                    return BadRequest(new { message = $"Invalid module code: {moduleCode}" });

                module = metadata.Module;
            }

            var isEnabled = await _licenseService.IsModuleEnabledAsync(companyId.Value, module, cancellationToken);
            return Ok(Result<object>.Success(new { isEnabled, moduleCode, module = module.ToString() }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking module status");
            return StatusCode(500, new { message = "Error checking module status" });
        }
    }

    /// <summary>
    /// Validate the current license
    /// </summary>
    [HttpGet("validate")]
    [ProducesResponseType(typeof(Result<LicenseValidationResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateLicense(CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _licenseService.ValidateLicenseAsync(companyId.Value, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license");
            return StatusCode(500, new { message = "Error validating license" });
        }
    }

    /// <summary>
    /// Activate a license using a license key
    /// </summary>
    [HttpPost("activate")]
    [HasPermission("ManageSettings")]
    [ProducesResponseType(typeof(Result<LicenseInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateLicense([FromBody] ActivateLicenseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

            var result = await _licenseService.ActivateLicenseAsync(
                companyId.Value,
                request.LicenseKey,
                userEmail,
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            _logger.LogInformation("License activated for company {CompanyId} by {User}", companyId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating license");
            return StatusCode(500, new { message = "Error activating license" });
        }
    }

    /// <summary>
    /// Get license limits and current usage
    /// </summary>
    [HttpGet("limits")]
    [ProducesResponseType(typeof(Result<LicenseLimitCheckResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckLicenseLimits(CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _licenseService.CheckLicenseLimitsAsync(companyId.Value, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking license limits");
            return StatusCode(500, new { message = "Error checking license limits" });
        }
    }

    /// <summary>
    /// Create a trial license for the current company (admin only)
    /// </summary>
    [HttpPost("trial")]
    [HasPermission("ManageSettings")]
    [ProducesResponseType(typeof(Result<LicenseInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTrialLicense([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _licenseService.CreateTrialLicenseAsync(companyId.Value, days, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            _logger.LogInformation("Trial license created for company {CompanyId}", companyId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating trial license");
            return StatusCode(500, new { message = "Error creating trial license" });
        }
    }

    /// <summary>
    /// Enable a module for the current company's license
    /// </summary>
    [HttpPost("modules/{moduleCode}/enable")]
    [HasPermission("ManageSettings")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnableModule(string moduleCode, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _licenseService.EnableModuleAsync(companyId.Value, moduleCode, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            _logger.LogInformation("Module {ModuleCode} enabled for company {CompanyId}", moduleCode, companyId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling module {ModuleCode}", moduleCode);
            return StatusCode(500, new { message = "Error enabling module" });
        }
    }

    #region Private Methods

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private Guid? GetCurrentCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : null;
    }

    #endregion
}
