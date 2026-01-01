using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Licensing;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Licensing;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for managing software licenses and module activations
/// </summary>
public class LicenseService : ILicenseService
{
    private readonly ComplaintDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LicenseService> _logger;
    private const string CacheKeyPrefix = "License_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    // Static module metadata
    private static readonly List<ModuleMetadataDto> _moduleMetadata = new()
    {
        new ModuleMetadataDto
        {
            Module = LicenseModule.Core,
            Code = "CORE",
            Name = "Complaint Management",
            Description = "Core complaint management system with ticketing, escalation, and SLA tracking",
            Icon = "fa-ticket-alt",
            Route = "/dashboard",
            Color = "#0057FF",
            DisplayOrder = 0,
            IsCore = true,
            Features = new() { "Ticket Management", "Escalation", "SLA Tracking", "Dashboard", "Reports" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.CRM_Customer,
            Code = "CRM_CUSTOMER",
            Name = "Customer Management",
            Description = "Partner and customer relationship management with multi-location support",
            Icon = "fa-users",
            Route = "/crm/customers",
            Color = "#10B981",
            DisplayOrder = 1,
            IsCore = false,
            Features = new() { "Partner Management", "Customer Management", "Multi-Location", "Contact Management" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.CRM_Product,
            Code = "CRM_PRODUCT",
            Name = "Product Catalog",
            Description = "Product catalog with SKU, pricing, and category management",
            Icon = "fa-boxes",
            Route = "/crm/products",
            Color = "#8B5CF6",
            DisplayOrder = 2,
            IsCore = false,
            Features = new() { "Product Categories", "Product Variants", "Pricing", "Inventory" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.CRM_Sales,
            Code = "CRM_SALES",
            Name = "Sales CRM",
            Description = "Sales pipeline, quotes, and order management",
            Icon = "fa-chart-line",
            Route = "/crm/sales",
            Color = "#F59E0B",
            DisplayOrder = 3,
            IsCore = false,
            Features = new() { "Leads", "Opportunities", "Quotes", "Orders", "Pipeline" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.ServiceContract,
            Code = "SERVICE_CONTRACT",
            Name = "Contracts & Warranty",
            Description = "Service contracts, warranties, and coverage management",
            Icon = "fa-file-contract",
            Route = "/service/contracts",
            Color = "#EC4899",
            DisplayOrder = 4,
            IsCore = false,
            Features = new() { "AMC Management", "Warranty Tracking", "Coverage Check", "Renewals" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.AssetManagement,
            Code = "ASSET_MGMT",
            Name = "Asset Management",
            Description = "Track customer assets, installations, and service history",
            Icon = "fa-server",
            Route = "/service/assets",
            Color = "#06B6D4",
            DisplayOrder = 5,
            IsCore = false,
            Features = new() { "Asset Registry", "Installation Tracking", "Service History", "Depreciation" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.CustomerPortal,
            Code = "CUSTOMER_PORTAL",
            Name = "Customer Portal",
            Description = "Self-service portal for customers and partners",
            Icon = "fa-globe",
            Route = "/portal",
            Color = "#14B8A6",
            DisplayOrder = 6,
            IsCore = false,
            Features = new() { "Ticket Submission", "Ticket Tracking", "Knowledge Base", "Self-Service" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.AdvancedReporting,
            Code = "ADV_REPORTING",
            Name = "Advanced Reporting",
            Description = "Business intelligence dashboards and custom reports",
            Icon = "fa-chart-bar",
            Route = "/reports/advanced",
            Color = "#6366F1",
            DisplayOrder = 7,
            IsCore = false,
            Features = new() { "Custom Dashboards", "BI Analytics", "Export", "Scheduling" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.APIAccess,
            Code = "API_ACCESS",
            Name = "API Access",
            Description = "REST API access for third-party integrations",
            Icon = "fa-plug",
            Route = "/settings/api",
            Color = "#84CC16",
            DisplayOrder = 8,
            IsCore = false,
            Features = new() { "REST API", "Webhooks", "OAuth Clients", "Rate Limiting" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.WhiteLabeling,
            Code = "WHITE_LABEL",
            Name = "White Labeling",
            Description = "Custom branding and white-label options",
            Icon = "fa-paint-brush",
            Route = "/settings/branding",
            Color = "#F97316",
            DisplayOrder = 9,
            IsCore = false,
            Features = new() { "Custom Logo", "Custom Colors", "Custom Domain", "Email Branding" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.EmailTicketing,
            Code = "EMAIL_TICKETING",
            Name = "Email Ticketing",
            Description = "Create and manage tickets via email",
            Icon = "fa-envelope",
            Route = "/settings/email-ticketing",
            Color = "#EF4444",
            DisplayOrder = 10,
            IsCore = false,
            Features = new() { "Email-to-Ticket", "Auto-Reply", "Threading", "Attachments" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.MultiLanguage,
            Code = "MULTI_LANG",
            Name = "Multi-Language",
            Description = "Multi-language support for global teams",
            Icon = "fa-language",
            Route = "/settings/languages",
            Color = "#A855F7",
            DisplayOrder = 11,
            IsCore = false,
            Features = new() { "UI Translations", "Email Templates", "Dynamic Content" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.AdvancedSLA,
            Code = "ADV_SLA",
            Name = "Advanced SLA",
            Description = "Advanced SLA rules and business hours",
            Icon = "fa-clock",
            Route = "/settings/sla",
            Color = "#0EA5E9",
            DisplayOrder = 12,
            IsCore = false,
            Features = new() { "Business Calendars", "Holiday Rules", "Priority Matrix", "SLA Alerts" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.WorkflowAutomation,
            Code = "WORKFLOW_AUTO",
            Name = "Workflow Automation",
            Description = "Advanced workflow rules and automation",
            Icon = "fa-cogs",
            Route = "/settings/workflows",
            Color = "#22C55E",
            DisplayOrder = 13,
            IsCore = false,
            Features = new() { "Custom Workflows", "Triggers", "Actions", "Conditions" }
        },
        new ModuleMetadataDto
        {
            Module = LicenseModule.CRM_Project,
            Code = "CRM_PROJECT",
            Name = "Project Management",
            Description = "Full project management with milestones, tasks, documents, and team members",
            Icon = "fa-diagram-project",
            Route = "/admin/projects",
            Color = "#3B82F6",
            DisplayOrder = 14,
            IsCore = false,
            Features = new() { "Project Tracking", "Milestones", "Task Management", "Documents", "Team Members", "Budget Tracking" }
        }
    };

    public LicenseService(ComplaintDbContext context, IMemoryCache cache, ILogger<LicenseService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsModuleEnabledAsync(Guid companyId, LicenseModule module, CancellationToken cancellationToken = default)
    {
        // Core module is always enabled
        if (module == LicenseModule.Core)
            return true;

        var cacheKey = $"{CacheKeyPrefix}{companyId}_Module_{module}";

        if (_cache.TryGetValue(cacheKey, out bool isEnabled))
            return isEnabled;

        var license = await GetActiveLicenseAsync(companyId, cancellationToken);
        if (license == null)
            return false;

        // Check if license is valid
        if (!IsLicenseValid(license))
            return false;

        // Check module activation
        var activation = await _context.LicenseModuleActivations
            .FirstOrDefaultAsync(a => a.LicenseId == license.Id && a.Module == module && a.IsEnabled && !a.IsDeleted, cancellationToken);

        isEnabled = activation != null && (activation.ExpiresAt == null || activation.ExpiresAt > DateTime.UtcNow);

        _cache.Set(cacheKey, isEnabled, CacheDuration);
        return isEnabled;
    }

    public async Task<List<LicenseModule>> GetEnabledModulesAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{companyId}_EnabledModules";

        if (_cache.TryGetValue(cacheKey, out List<LicenseModule>? cachedModules) && cachedModules != null)
            return cachedModules;

        var enabledModules = new List<LicenseModule> { LicenseModule.Core }; // Core is always enabled

        var license = await GetActiveLicenseAsync(companyId, cancellationToken);
        if (license == null || !IsLicenseValid(license))
        {
            _cache.Set(cacheKey, enabledModules, CacheDuration);
            return enabledModules;
        }

        var activations = await _context.LicenseModuleActivations
            .Where(a => a.LicenseId == license.Id && a.IsEnabled && !a.IsDeleted)
            .Where(a => a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow)
            .Select(a => a.Module)
            .ToListAsync(cancellationToken);

        enabledModules.AddRange(activations);

        _cache.Set(cacheKey, enabledModules.Distinct().ToList(), CacheDuration);
        return enabledModules.Distinct().ToList();
    }

    public async Task<Result<LicenseInfoDto>> GetLicenseInfoAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var license = await GetActiveLicenseAsync(companyId, cancellationToken);

        if (license == null)
        {
            return Result<LicenseInfoDto>.Success(new LicenseInfoDto
            {
                CompanyId = companyId,
                LicenseType = "None",
                Status = "No License",
                IsActive = false,
                DaysRemaining = 0,
                IsExpired = true,
                EnabledModules = new List<ModuleActivationDto>
                {
                    new ModuleActivationDto
                    {
                        ModuleCode = "CORE",
                        ModuleName = "Complaint Management",
                        Description = "Core complaint management (limited)",
                        Icon = "fa-ticket-alt",
                        Route = "/dashboard",
                        IsEnabled = true,
                        DisplayOrder = 0
                    }
                }
            });
        }

        var activations = await _context.LicenseModuleActivations
            .Where(a => a.LicenseId == license.Id && !a.IsDeleted)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync(cancellationToken);

        var dto = new LicenseInfoDto
        {
            Id = license.Id,
            CompanyId = license.CompanyId,
            Name = license.Name,
            LicenseType = license.Type.ToString(),
            Status = license.Status.ToString(),
            IssuedAt = license.IssuedAt,
            ExpiresAt = license.ExpiresAt,
            ActivatedAt = license.ActivatedAt,
            ActivatedBy = license.ActivatedBy,
            IsActive = license.IsActive && IsLicenseValid(license),
            DaysRemaining = CalculateDaysRemaining(license),
            IsExpired = license.ExpiresAt.HasValue && license.ExpiresAt.Value < DateTime.UtcNow,
            IsInGracePeriod = IsInGracePeriod(license),
            MaxUsers = license.MaxUsers,
            MaxCustomers = license.MaxCustomers,
            MaxProducts = license.MaxProducts,
            MaxAssets = license.MaxAssets,
            MaxContracts = license.MaxContracts,
            CurrentUsers = await _context.Users.CountAsync(u => u.CompanyId == companyId && !u.IsDeleted, cancellationToken),
            EnabledModules = activations.Select(a =>
            {
                var meta = _moduleMetadata.FirstOrDefault(m => m.Module == a.Module);
                return new ModuleActivationDto
                {
                    ModuleCode = meta?.Code ?? a.Module.ToString(),
                    ModuleName = meta?.Name ?? a.Module.ToString(),
                    Description = meta?.Description,
                    Icon = meta?.Icon,
                    Route = meta?.Route,
                    IsEnabled = a.IsEnabled && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow),
                    EnabledAt = a.EnabledAt,
                    ExpiresAt = a.ExpiresAt,
                    DisplayOrder = a.DisplayOrder,
                    ModuleConfig = a.ModuleConfig
                };
            }).ToList()
        };

        // Always add Core module
        if (!dto.EnabledModules.Any(m => m.ModuleCode == "CORE"))
        {
            var coreMeta = _moduleMetadata.First(m => m.Module == LicenseModule.Core);
            dto.EnabledModules.Insert(0, new ModuleActivationDto
            {
                ModuleCode = coreMeta.Code,
                ModuleName = coreMeta.Name,
                Description = coreMeta.Description,
                Icon = coreMeta.Icon,
                Route = coreMeta.Route,
                IsEnabled = true,
                DisplayOrder = 0
            });
        }

        return Result<LicenseInfoDto>.Success(dto);
    }

    public async Task<Result<LicenseInfoDto>> ActivateLicenseAsync(Guid companyId, string licenseKey, string activatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, use a simple validation - in production, this would validate against a license server
            var licenseData = ParseLicenseKey(licenseKey);
            if (licenseData == null)
                return Result<LicenseInfoDto>.Failure("Invalid license key", "The provided license key is not valid");

            // Check if license already exists
            var existingLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.CompanyId == companyId && l.IsActive && !l.IsDeleted, cancellationToken);

            if (existingLicense != null)
            {
                existingLicense.IsActive = false;
                existingLicense.UpdatedAt = DateTime.UtcNow;
            }

            // Create new license
            var license = new License
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                LicenseKey = licenseKey,
                Type = licenseData.Type,
                Status = LicenseStatus.Active,
                Name = $"{licenseData.Type} License",
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = licenseData.ExpiresAt,
                MaxUsers = licenseData.MaxUsers,
                MaxCustomers = licenseData.MaxCustomers,
                MaxProducts = licenseData.MaxProducts,
                IsActive = true,
                ActivatedBy = activatedBy,
                ActivatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Licenses.Add(license);

            // Add module activations based on license type
            var modules = GetModulesForLicenseType(licenseData.Type);
            foreach (var module in modules)
            {
                _context.LicenseModuleActivations.Add(new LicenseModuleActivation
                {
                    Id = Guid.NewGuid(),
                    LicenseId = license.Id,
                    Module = module,
                    IsEnabled = true,
                    EnabledAt = DateTime.UtcNow,
                    DisplayOrder = _moduleMetadata.FirstOrDefault(m => m.Module == module)?.DisplayOrder ?? 0,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Clear cache
            ClearLicenseCache(companyId);

            _logger.LogInformation("License activated for company {CompanyId}, Type: {LicenseType}", companyId, licenseData.Type);

            return await GetLicenseInfoAsync(companyId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating license for company {CompanyId}", companyId);
            return Result<LicenseInfoDto>.Failure("Activation failed", ex.Message);
        }
    }

    public async Task<Result<LicenseValidationResult>> ValidateLicenseAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var license = await GetActiveLicenseAsync(companyId, cancellationToken);

        var result = new LicenseValidationResult
        {
            IsValid = false,
            Status = "Invalid",
            DaysRemaining = 0
        };

        if (license == null)
        {
            result.Message = "No active license found";
            result.Errors.Add("No license");
            return Result<LicenseValidationResult>.Success(result);
        }

        result.ExpiresAt = license.ExpiresAt;
        result.DaysRemaining = CalculateDaysRemaining(license);

        if (license.Status == LicenseStatus.Revoked || license.Status == LicenseStatus.Suspended)
        {
            result.Status = license.Status.ToString();
            result.Message = $"License is {license.Status.ToString().ToLower()}";
            result.Errors.Add($"License status: {license.Status}");
            return Result<LicenseValidationResult>.Success(result);
        }

        if (license.ExpiresAt.HasValue && license.ExpiresAt.Value < DateTime.UtcNow)
        {
            if (IsInGracePeriod(license))
            {
                result.IsValid = true;
                result.Status = "Grace Period";
                result.Message = "License is in grace period";
                result.Warnings.Add($"License expired. Grace period ends in {license.GracePeriodDays - (DateTime.UtcNow - license.ExpiresAt.Value).Days} days");
            }
            else
            {
                result.Status = "Expired";
                result.Message = "License has expired";
                result.Errors.Add("License expired");
            }
            return Result<LicenseValidationResult>.Success(result);
        }

        result.IsValid = true;
        result.Status = "Active";
        result.Message = "License is valid";

        if (result.DaysRemaining <= 30)
        {
            result.Warnings.Add($"License expires in {result.DaysRemaining} days");
        }

        return Result<LicenseValidationResult>.Success(result);
    }

    public async Task<Result<ModuleLimitsDto>> GetModuleLimitsAsync(Guid companyId, LicenseModule module, CancellationToken cancellationToken = default)
    {
        var license = await GetActiveLicenseAsync(companyId, cancellationToken);
        var meta = _moduleMetadata.FirstOrDefault(m => m.Module == module);

        var limits = new ModuleLimitsDto
        {
            ModuleCode = meta?.Code ?? module.ToString(),
            ModuleName = meta?.Name ?? module.ToString()
        };

        if (license == null)
            return Result<ModuleLimitsDto>.Success(limits);

        // Set limits based on license
        limits.Limits["Users"] = license.MaxUsers;
        limits.Limits["Customers"] = license.MaxCustomers;
        limits.Limits["Products"] = license.MaxProducts;
        limits.Limits["Assets"] = license.MaxAssets;
        limits.Limits["Contracts"] = license.MaxContracts;

        // Get current usage
        limits.CurrentUsage["Users"] = await _context.Users.CountAsync(u => u.CompanyId == companyId && !u.IsDeleted, cancellationToken);

        // Check if limits exceeded
        foreach (var limit in limits.Limits.Where(l => l.Value.HasValue))
        {
            if (limits.CurrentUsage.TryGetValue(limit.Key, out var current))
            {
                limits.LimitExceeded[limit.Key] = current >= limit.Value;
            }
        }

        return Result<ModuleLimitsDto>.Success(limits);
    }

    public async Task<Result<List<ModuleConfigDto>>> GetModuleConfigurationsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var enabledModules = await GetEnabledModulesAsync(companyId, cancellationToken);

        var configs = _moduleMetadata.Select(meta => new ModuleConfigDto
        {
            ModuleCode = meta.Code,
            ModuleName = meta.Name,
            Description = meta.Description,
            Icon = meta.Icon,
            Route = meta.Route,
            Color = meta.Color,
            IsEnabled = enabledModules.Contains(meta.Module),
            IsAvailable = !meta.IsCore, // Core is always available, others depend on license
            DisplayOrder = meta.DisplayOrder,
            Features = meta.Features.Select(f => new ModuleFeatureDto
            {
                FeatureCode = f.Replace(" ", "_").ToUpper(),
                FeatureName = f,
                IsEnabled = enabledModules.Contains(meta.Module)
            }).ToList()
        }).OrderBy(c => c.DisplayOrder).ToList();

        return Result<List<ModuleConfigDto>>.Success(configs);
    }

    public async Task<Result<LicenseInfoDto>> CreateTrialLicenseAsync(Guid companyId, int trialDays = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if company already has a license
            var existingLicense = await _context.Licenses
                .AnyAsync(l => l.CompanyId == companyId && !l.IsDeleted, cancellationToken);

            if (existingLicense)
                return Result<LicenseInfoDto>.Failure("License exists", "Company already has a license");

            var license = new License
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                LicenseKey = GenerateTrialLicenseKey(),
                Type = LicenseType.Trial,
                Status = LicenseStatus.Active,
                Name = "Trial License",
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(trialDays),
                MaxUsers = 10,
                MaxCustomers = 50,
                MaxProducts = 100,
                IsActive = true,
                ActivatedAt = DateTime.UtcNow,
                ActivatedBy = "System",
                GracePeriodDays = 7,
                CreatedAt = DateTime.UtcNow
            };

            _context.Licenses.Add(license);

            // Add all modules for trial
            var trialModules = new[]
            {
                LicenseModule.Core,
                LicenseModule.CRM_Customer,
                LicenseModule.CRM_Product,
                LicenseModule.ServiceContract,
                LicenseModule.AssetManagement,
                LicenseModule.CustomerPortal
            };

            foreach (var module in trialModules)
            {
                var meta = _moduleMetadata.FirstOrDefault(m => m.Module == module);
                _context.LicenseModuleActivations.Add(new LicenseModuleActivation
                {
                    Id = Guid.NewGuid(),
                    LicenseId = license.Id,
                    Module = module,
                    IsEnabled = true,
                    EnabledAt = DateTime.UtcNow,
                    ExpiresAt = license.ExpiresAt,
                    DisplayOrder = meta?.DisplayOrder ?? 0,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Trial license created for company {CompanyId}", companyId);

            return await GetLicenseInfoAsync(companyId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating trial license for company {CompanyId}", companyId);
            return Result<LicenseInfoDto>.Failure("Creation failed", ex.Message);
        }
    }

    public async Task<Result<LicenseLimitCheckResult>> CheckLicenseLimitsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var license = await GetActiveLicenseAsync(companyId, cancellationToken);
        var result = new LicenseLimitCheckResult();

        if (license == null)
            return Result<LicenseLimitCheckResult>.Success(result);

        // Check user limits
        if (license.MaxUsers.HasValue)
        {
            var currentUsers = await _context.Users.CountAsync(u => u.CompanyId == companyId && !u.IsDeleted, cancellationToken);
            var percentUsed = (int)((double)currentUsers / license.MaxUsers.Value * 100);

            if (currentUsers >= license.MaxUsers.Value)
            {
                result.HasExceededLimits = true;
                result.Violations.Add(new LimitViolation
                {
                    LimitType = "Users",
                    Limit = license.MaxUsers.Value,
                    Current = currentUsers,
                    Message = $"User limit exceeded: {currentUsers}/{license.MaxUsers.Value}"
                });
            }
            else if (percentUsed >= 80)
            {
                result.Warnings.Add(new LimitWarning
                {
                    LimitType = "Users",
                    Limit = license.MaxUsers.Value,
                    Current = currentUsers,
                    PercentUsed = percentUsed,
                    Message = $"User limit at {percentUsed}%: {currentUsers}/{license.MaxUsers.Value}"
                });
            }
        }

        return Result<LicenseLimitCheckResult>.Success(result);
    }

    public List<ModuleMetadataDto> GetAllModulesMetadata()
    {
        return _moduleMetadata.ToList();
    }

    public async Task<Result<bool>> EnableModuleAsync(Guid companyId, string moduleCode, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find the module by code
            var moduleMetadata = _moduleMetadata.FirstOrDefault(m => m.Code.Equals(moduleCode, StringComparison.OrdinalIgnoreCase));
            if (moduleMetadata == null)
            {
                // Try parsing as enum value
                if (!Enum.TryParse<LicenseModule>(moduleCode, true, out var parsedModule))
                {
                    return Result<bool>.Failure("Invalid module", $"Module '{moduleCode}' not found");
                }
                moduleMetadata = _moduleMetadata.FirstOrDefault(m => m.Module == parsedModule);
            }

            if (moduleMetadata == null)
                return Result<bool>.Failure("Invalid module", $"Module '{moduleCode}' not found");

            var license = await GetActiveLicenseAsync(companyId, cancellationToken);
            if (license == null)
                return Result<bool>.Failure("No license", "No active license found for company");

            // Check if module activation already exists
            var existingActivation = await _context.LicenseModuleActivations
                .FirstOrDefaultAsync(a => a.LicenseId == license.Id && a.Module == moduleMetadata.Module && !a.IsDeleted, cancellationToken);

            if (existingActivation != null)
            {
                if (existingActivation.IsEnabled)
                    return Result<bool>.Success(true, "Module already enabled");

                // Re-enable the existing activation
                existingActivation.IsEnabled = true;
                existingActivation.EnabledAt = DateTime.UtcNow;
                existingActivation.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new activation
                var activation = new LicenseModuleActivation
                {
                    Id = Guid.NewGuid(),
                    LicenseId = license.Id,
                    Module = moduleMetadata.Module,
                    IsEnabled = true,
                    EnabledAt = DateTime.UtcNow,
                    DisplayOrder = moduleMetadata.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };
                _context.LicenseModuleActivations.Add(activation);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Clear cache
            ClearLicenseCache(companyId);

            _logger.LogInformation("Module {ModuleCode} enabled for company {CompanyId}", moduleCode, companyId);
            return Result<bool>.Success(true, "Module enabled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling module {ModuleCode} for company {CompanyId}", moduleCode, companyId);
            return Result<bool>.Failure("Error", ex.Message);
        }
    }

    #region Private Helper Methods

    private async Task<License?> GetActiveLicenseAsync(Guid companyId, CancellationToken cancellationToken)
    {
        return await _context.Licenses
            .FirstOrDefaultAsync(l => l.CompanyId == companyId && l.IsActive && !l.IsDeleted, cancellationToken);
    }

    private bool IsLicenseValid(License license)
    {
        if (!license.IsActive)
            return false;

        if (license.Status == LicenseStatus.Revoked || license.Status == LicenseStatus.Suspended)
            return false;

        if (license.ExpiresAt.HasValue)
        {
            var graceEndDate = license.ExpiresAt.Value.AddDays(license.GracePeriodDays);
            if (DateTime.UtcNow > graceEndDate)
                return false;
        }

        return true;
    }

    private bool IsInGracePeriod(License license)
    {
        if (!license.ExpiresAt.HasValue)
            return false;

        if (DateTime.UtcNow <= license.ExpiresAt.Value)
            return false;

        var graceEndDate = license.ExpiresAt.Value.AddDays(license.GracePeriodDays);
        return DateTime.UtcNow <= graceEndDate;
    }

    private int CalculateDaysRemaining(License license)
    {
        if (!license.ExpiresAt.HasValue)
            return int.MaxValue; // Perpetual

        var days = (license.ExpiresAt.Value - DateTime.UtcNow).Days;
        return Math.Max(0, days);
    }

    private LicenseData? ParseLicenseKey(string licenseKey)
    {
        // Simple license key format: TYPE-USERS-CUSTOMERS-PRODUCTS-EXPIRY-SIGNATURE
        // Example: ENT-100-500-1000-20251231-ABC123
        // In production, this would be encrypted and validated against a license server

        try
        {
            var parts = licenseKey.Split('-');
            if (parts.Length < 5)
                return null;

            var type = parts[0].ToUpper() switch
            {
                "TRIAL" => LicenseType.Trial,
                "BASIC" => LicenseType.Basic,
                "STD" => LicenseType.Standard,
                "PRO" => LicenseType.Professional,
                "ENT" => LicenseType.Enterprise,
                _ => LicenseType.Custom
            };

            return new LicenseData
            {
                Type = type,
                MaxUsers = int.TryParse(parts[1], out var users) ? users : null,
                MaxCustomers = int.TryParse(parts[2], out var customers) ? customers : null,
                MaxProducts = int.TryParse(parts[3], out var products) ? products : null,
                ExpiresAt = DateTime.TryParseExact(parts[4], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var expiry)
                    ? expiry
                    : DateTime.UtcNow.AddYears(1)
            };
        }
        catch
        {
            return null;
        }
    }

    private List<LicenseModule> GetModulesForLicenseType(LicenseType type)
    {
        var modules = new List<LicenseModule> { LicenseModule.Core };

        switch (type)
        {
            case LicenseType.Enterprise:
                modules.AddRange(new[]
                {
                    LicenseModule.CRM_Customer,
                    LicenseModule.CRM_Product,
                    LicenseModule.CRM_Sales,
                    LicenseModule.ServiceContract,
                    LicenseModule.AssetManagement,
                    LicenseModule.CustomerPortal,
                    LicenseModule.AdvancedReporting,
                    LicenseModule.APIAccess,
                    LicenseModule.WhiteLabeling,
                    LicenseModule.EmailTicketing,
                    LicenseModule.MultiLanguage,
                    LicenseModule.AdvancedSLA,
                    LicenseModule.WorkflowAutomation,
                    LicenseModule.CRM_Project
                });
                break;

            case LicenseType.Professional:
                modules.AddRange(new[]
                {
                    LicenseModule.CRM_Customer,
                    LicenseModule.CRM_Product,
                    LicenseModule.ServiceContract,
                    LicenseModule.AssetManagement,
                    LicenseModule.CustomerPortal,
                    LicenseModule.EmailTicketing,
                    LicenseModule.AdvancedSLA,
                    LicenseModule.CRM_Project
                });
                break;

            case LicenseType.Standard:
                modules.AddRange(new[]
                {
                    LicenseModule.CRM_Customer,
                    LicenseModule.CRM_Product,
                    LicenseModule.ServiceContract,
                    LicenseModule.EmailTicketing
                });
                break;

            case LicenseType.Basic:
                modules.Add(LicenseModule.EmailTicketing);
                break;

            case LicenseType.Trial:
                modules.AddRange(new[]
                {
                    LicenseModule.CRM_Customer,
                    LicenseModule.CRM_Product,
                    LicenseModule.ServiceContract,
                    LicenseModule.AssetManagement,
                    LicenseModule.CustomerPortal
                });
                break;
        }

        return modules;
    }

    private string GenerateTrialLicenseKey()
    {
        return $"TRIAL-10-50-100-{DateTime.UtcNow.AddDays(30):yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    private void ClearLicenseCache(Guid companyId)
    {
        // Clear all cache entries for this company
        foreach (var module in Enum.GetValues<LicenseModule>())
        {
            _cache.Remove($"{CacheKeyPrefix}{companyId}_Module_{module}");
        }
        _cache.Remove($"{CacheKeyPrefix}{companyId}_EnabledModules");
    }

    private class LicenseData
    {
        public LicenseType Type { get; set; }
        public int? MaxUsers { get; set; }
        public int? MaxCustomers { get; set; }
        public int? MaxProducts { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    #endregion
}
