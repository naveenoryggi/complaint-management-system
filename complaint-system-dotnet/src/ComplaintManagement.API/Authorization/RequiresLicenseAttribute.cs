using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ComplaintManagement.API.Authorization;

/// <summary>
/// Attribute that requires specific licensed modules to access a controller or action.
/// Returns HTTP 402 (Payment Required) if the module is not licensed.
/// Automatically skips check for [AllowAnonymous] endpoints.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequiresLicenseAttribute : Attribute, IAsyncActionFilter
{
    private readonly LicenseModule[] _requiredModules;

    /// <summary>
    /// Creates a new RequiresLicenseAttribute that requires the specified modules
    /// </summary>
    /// <param name="modules">The modules required to access this resource</param>
    public RequiresLicenseAttribute(params LicenseModule[] modules)
    {
        _requiredModules = modules;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var endpoint = context.HttpContext.GetEndpoint();

        // Skip license check for [AllowAnonymous] endpoints
        if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            await next();
            return;
        }

        // Skip license check for [SkipLicenseCheck] endpoints
        if (endpoint?.Metadata?.GetMetadata<SkipLicenseCheckAttribute>() != null)
        {
            await next();
            return;
        }

        var licenseService = context.HttpContext.RequestServices.GetRequiredService<ILicenseService>();
        var companyId = GetCompanyIdFromClaims(context.HttpContext.User);

        if (companyId == null)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Unauthorized",
                message = "Company context not found",
                code = "NO_COMPANY_CONTEXT"
            });
            return;
        }

        foreach (var module in _requiredModules)
        {
            // Core module is always enabled
            if (module == LicenseModule.Core)
                continue;

            var isEnabled = await licenseService.IsModuleEnabledAsync(companyId.Value, module);

            if (!isEnabled)
            {
                var moduleMetadata = licenseService.GetAllModulesMetadata()
                    .FirstOrDefault(m => m.Module == module);

                context.Result = new ObjectResult(new
                {
                    error = "License Required",
                    message = $"The '{moduleMetadata?.Name ?? module.ToString()}' module is not licensed for your organization.",
                    code = "MODULE_NOT_LICENSED",
                    moduleRequired = module.ToString(),
                    moduleCode = moduleMetadata?.Code ?? module.ToString(),
                    moduleName = moduleMetadata?.Name ?? module.ToString(),
                    upgradeUrl = "/settings/license"
                })
                {
                    StatusCode = StatusCodes.Status402PaymentRequired
                };
                return;
            }
        }

        await next();
    }

    private Guid? GetCompanyIdFromClaims(ClaimsPrincipal user)
    {
        var companyIdClaim = user.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim))
            return null;

        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : null;
    }
}
