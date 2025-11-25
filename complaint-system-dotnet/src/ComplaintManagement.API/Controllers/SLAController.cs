using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.SLA;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.SLA;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/sla")]
[Authorize]
public class SLAController : ControllerBase
{
    private readonly ComplaintDbContext _dbContext;
    private readonly ILogger<SLAController> _logger;
    private readonly ISLACalculatorService _slaCalculator;

    public SLAController(
        ComplaintDbContext dbContext,
        ILogger<SLAController> logger,
        ISLACalculatorService slaCalculator)
    {
        _dbContext = dbContext;
        _logger = logger;
        _slaCalculator = slaCalculator;
    }

    #region Global SLA Settings

    [HttpGet("settings")]
    [HasPermission("ViewSLA")]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return Unauthorized(new { isSuccess = false, message = "Company ID not found in token" });
            }

            var settings = await _dbContext.SLASettings.FirstOrDefaultAsync(s => s.CompanyId == companyId);

            if (settings == null)
            {
                return Ok(new
                {
                    isSuccess = true,
                    data = new SLASettingsDto { Id = Guid.Empty, IsEnabled = false, CompanyId = companyId },
                    message = "Default SLA settings (not configured yet)"
                });
            }

            return Ok(new { isSuccess = true, data = MapToSettingsDto(settings), message = "SLA settings retrieved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SLA settings");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    [HttpPut("settings")]
    [HasPermission("ManageSLA")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSLASettingsRequest request)
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return Unauthorized(new { isSuccess = false, message = "Company ID not found" });
            }

            var settings = await _dbContext.SLASettings.FirstOrDefaultAsync(s => s.CompanyId == companyId);

            if (settings == null)
            {
                settings = new SLASettings
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.SLASettings.Add(settings);
            }

            settings.IsEnabled = request.IsEnabled;
            settings.WorkingHoursOnly = request.WorkingHoursOnly;
            settings.WorkingHoursStart = request.WorkingHoursStart;
            settings.WorkingHoursEnd = request.WorkingHoursEnd;
            settings.WorkingDays = request.WorkingDays;
            settings.ExcludeHolidays = request.ExcludeHolidays;
            settings.AutoEscalateOnBreach = request.AutoEscalateOnBreach;
            settings.EscalationThresholdPercent = request.EscalationThresholdPercent;
            settings.NotifyBeforeBreach = request.NotifyBeforeBreach;
            settings.NotifyBeforeBreachMinutes = request.NotifyBeforeBreachMinutes;
            settings.PauseSLAOnPendingInfo = request.PauseSLAOnPendingInfo;
            settings.Timezone = request.Timezone;
            settings.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new { isSuccess = true, data = MapToSettingsDto(settings), message = "Settings updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SLA settings");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    #endregion

    #region SLA Levels

    [HttpGet("levels")]
    [HasPermission("ViewSLA")]
    public async Task<IActionResult> GetLevels()
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return Unauthorized(new { isSuccess = false, message = "Company ID not found" });
            }

            var levels = await _dbContext.SLALevels
                .Where(l => l.CompanyId == companyId || l.CompanyId == null)
                .OrderBy(l => l.Order)
                .ToListAsync();

            var dtos = levels.Select(MapToLevelDto).ToList();
            return Ok(new { isSuccess = true, data = dtos, message = "Levels retrieved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SLA levels");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    [HttpGet("levels/{id}")]
    [HasPermission("ViewSLA")]
    public async Task<IActionResult> GetLevelById(Guid id)
    {
        try
        {
            var level = await _dbContext.SLALevels.FindAsync(id);
            if (level == null)
            {
                return NotFound(new { isSuccess = false, message = "Level not found" });
            }

            return Ok(new { isSuccess = true, data = MapToLevelDto(level), message = "Level retrieved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving level");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    [HttpPost("levels")]
    [HasPermission("ManageSLA")]
    public async Task<IActionResult> CreateLevel([FromBody] CreateSLALevelRequest request)
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return Unauthorized(new { isSuccess = false, message = "Company ID not found" });
            }

            var level = new SLALevel
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Order = request.Order,
                IsActive = request.IsActive,
                ColorCode = request.ColorCode,
                DefaultResponseTime = request.DefaultResponseTime,
                ResponseTimeUnit = Enum.Parse<TimeUnit>(request.ResponseTimeUnit),
                DefaultResolutionTime = request.DefaultResolutionTime,
                ResolutionTimeUnit = Enum.Parse<TimeUnit>(request.ResolutionTimeUnit),
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.SLALevels.Add(level);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLevelById), new { id = level.Id },
                new { isSuccess = true, data = MapToLevelDto(level), message = "Level created" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating level");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    [HttpPut("levels/{id}")]
    [HasPermission("ManageSLA")]
    public async Task<IActionResult> UpdateLevel(Guid id, [FromBody] UpdateSLALevelRequest request)
    {
        try
        {
            var level = await _dbContext.SLALevels.FindAsync(id);
            if (level == null)
            {
                return NotFound(new { isSuccess = false, message = "Level not found" });
            }

            level.Name = request.Name;
            level.Description = request.Description;
            level.Order = request.Order;
            level.IsActive = request.IsActive;
            level.ColorCode = request.ColorCode;
            level.DefaultResponseTime = request.DefaultResponseTime;
            level.ResponseTimeUnit = Enum.Parse<TimeUnit>(request.ResponseTimeUnit);
            level.DefaultResolutionTime = request.DefaultResolutionTime;
            level.ResolutionTimeUnit = Enum.Parse<TimeUnit>(request.ResolutionTimeUnit);
            level.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new { isSuccess = true, data = MapToLevelDto(level), message = "Level updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating level");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    [HttpDelete("levels/{id}")]
    [HasPermission("ManageSLA")]
    public async Task<IActionResult> DeleteLevel(Guid id)
    {
        try
        {
            var level = await _dbContext.SLALevels.FindAsync(id);
            if (level == null)
            {
                return NotFound(new { isSuccess = false, message = "Level not found" });
            }

            var hasMappings = await _dbContext.CategorySLAs.AnyAsync(c => c.SLALevelId == id) ||
                            await _dbContext.PrioritySLAs.AnyAsync(p => p.SLALevelId == id);

            if (hasMappings)
            {
                return BadRequest(new { isSuccess = false, message = "Cannot delete - in use by mappings" });
            }

            _dbContext.SLALevels.Remove(level);
            await _dbContext.SaveChangesAsync();

            return Ok(new { isSuccess = true, message = "Level deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting level");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    #endregion

    #region Category SLA Mappings

    /// <summary>
    /// Get all category SLA mappings
    /// </summary>
    [HttpGet("category-mappings")]
    [HasPermission("ViewSLA")]
    public async Task<IActionResult> GetCategoryMappings()
    {
        try
        {
            var companyId = GetCompanyId();

            var mappings = await _dbContext.CategorySLAs
                .Include(c => c.Category)
                .Include(c => c.SLALevel)
                .Where(c => c.SLALevel.CompanyId == companyId && !c.IsDeleted && !c.Category.IsDeleted)
                .Select(c => new CategorySLADto
                {
                    Id = c.Id,
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category.Name,
                    SLALevelId = c.SLALevelId,
                    SLALevelName = c.SLALevel.Name,
                    SLALevelColorCode = c.SLALevel.ColorCode,
                    OverrideResponseTime = c.OverrideResponseTime,
                    OverrideResolutionTime = c.OverrideResolutionTime,
                    EffectiveResponseTimeMinutes = c.OverrideResponseTime ?? c.SLALevel.GetResponseTimeInMinutes(),
                    EffectiveResolutionTimeMinutes = c.OverrideResolutionTime ?? c.SLALevel.GetResolutionTimeInMinutes(),
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return Ok(new { isSuccess = true, data = mappings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category mappings");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create or update category SLA mapping
    /// </summary>
    [HttpPost("category-mappings")]
    [HasPermission("ManageSLA")]
    public async Task<IActionResult> SaveCategoryMapping([FromBody] CreateCategorySLARequest request)
    {
        try
        {
            var companyId = GetCompanyId();

            // Check if mapping already exists
            var existing = await _dbContext.CategorySLAs
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId && !c.IsDeleted);

            if (existing != null)
            {
                // Update existing
                existing.SLALevelId = request.SLALevelId;
                existing.OverrideResponseTime = request.OverrideResponseTime;
                existing.OverrideResolutionTime = request.OverrideResolutionTime;
                existing.IsActive = request.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new
                var mapping = new CategorySLA
                {
                    Id = Guid.NewGuid(),
                    CategoryId = request.CategoryId,
                    SLALevelId = request.SLALevelId,
                    OverrideResponseTime = request.OverrideResponseTime,
                    OverrideResolutionTime = request.OverrideResolutionTime,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.CategorySLAs.Add(mapping);
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { isSuccess = true, message = "Category mapping saved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving category mapping");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Bulk update category mappings
    /// </summary>
    [HttpPost("category-mappings/bulk")]
    [HasPermission("ManageSLA")]
    public async Task<IActionResult> BulkUpdateCategoryMappings([FromBody] BulkUpdateCategorySLARequest request)
    {
        try
        {
            var companyId = GetCompanyId();

            foreach (var mapping in request.Mappings)
            {
                var existing = await _dbContext.CategorySLAs
                    .FirstOrDefaultAsync(c => c.CategoryId == mapping.CategoryId && !c.IsDeleted);

                if (existing != null)
                {
                    existing.SLALevelId = mapping.SLALevelId;
                    existing.OverrideResponseTime = mapping.OverrideResponseTime;
                    existing.OverrideResolutionTime = mapping.OverrideResolutionTime;
                    existing.IsActive = mapping.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newMapping = new CategorySLA
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = mapping.CategoryId,
                        SLALevelId = mapping.SLALevelId,
                        OverrideResponseTime = mapping.OverrideResponseTime,
                        OverrideResolutionTime = mapping.OverrideResolutionTime,
                        IsActive = mapping.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.CategorySLAs.Add(newMapping);
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { isSuccess = true, message = $"{request.Mappings.Count} mappings saved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating category mappings");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Delete category SLA mapping
    /// </summary>
    [HttpDelete("category-mappings/{id}")]
    [HasPermission("ManageSLA")]
    public async Task<IActionResult> DeleteCategoryMapping(Guid id)
    {
        try
        {
            var mapping = await _dbContext.CategorySLAs.FindAsync(id);
            if (mapping == null)
            {
                return NotFound(new { isSuccess = false, message = "Mapping not found" });
            }

            mapping.IsDeleted = true;
            mapping.DeletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return Ok(new { isSuccess = true, message = "Mapping deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category mapping");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    #endregion

    #region Priority SLA Mappings

    /// <summary>
    /// Get all priority SLA mappings
    /// </summary>
    [HttpGet("priority-mappings")]
    [HasPermission("ViewSLA")]
    public async Task<IActionResult> GetPriorityMappings()
    {
        try
        {
            var companyId = GetCompanyId();

            var mappings = await _dbContext.PrioritySLAs
                .Include(p => p.Priority)
                .Include(p => p.SLALevel)
                .Where(p => p.SLALevel.CompanyId == companyId && !p.IsDeleted && !p.Priority.IsDeleted)
                .Select(p => new PrioritySLADto
                {
                    Id = p.Id,
                    PriorityId = p.PriorityId,
                    PriorityName = p.Priority.Name,
                    SLALevelId = p.SLALevelId,
                    SLALevelName = p.SLALevel.Name,
                    SLALevelColorCode = p.SLALevel.ColorCode,
                    OverrideResponseTime = p.OverrideResponseTime,
                    OverrideResolutionTime = p.OverrideResolutionTime,
                    EffectiveResponseTimeMinutes = p.OverrideResponseTime ?? p.SLALevel.GetResponseTimeInMinutes(),
                    EffectiveResolutionTimeMinutes = p.OverrideResolutionTime ?? p.SLALevel.GetResolutionTimeInMinutes(),
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return Ok(new { isSuccess = true, data = mappings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting priority mappings");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create or update priority SLA mapping
    /// </summary>
    [HttpPost("priority-mappings")]
    [HasPermission("ManageSLA")]
    public async Task<IActionResult> SavePriorityMapping([FromBody] CreatePrioritySLARequest request)
    {
        try
        {
            var companyId = GetCompanyId();

            // Check if mapping already exists
            var existing = await _dbContext.PrioritySLAs
                .FirstOrDefaultAsync(p => p.PriorityId == request.PriorityId && !p.IsDeleted);

            if (existing != null)
            {
                // Update existing
                existing.SLALevelId = request.SLALevelId;
                existing.OverrideResponseTime = request.OverrideResponseTime;
                existing.OverrideResolutionTime = request.OverrideResolutionTime;
                existing.IsActive = request.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new
                var mapping = new PrioritySLA
                {
                    Id = Guid.NewGuid(),
                    PriorityId = request.PriorityId,
                    SLALevelId = request.SLALevelId,
                    OverrideResponseTime = request.OverrideResponseTime,
                    OverrideResolutionTime = request.OverrideResolutionTime,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.PrioritySLAs.Add(mapping);
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { isSuccess = true, message = "Priority mapping saved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving priority mapping");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Bulk update priority mappings
    /// </summary>
    [HttpPost("priority-mappings/bulk")]
    [HasPermission("ManageSLA")]
    public async Task<IActionResult> BulkUpdatePriorityMappings([FromBody] BulkUpdatePrioritySLARequest request)
    {
        try
        {
            var companyId = GetCompanyId();

            foreach (var mapping in request.Mappings)
            {
                var existing = await _dbContext.PrioritySLAs
                    .FirstOrDefaultAsync(p => p.PriorityId == mapping.PriorityId && !p.IsDeleted);

                if (existing != null)
                {
                    existing.SLALevelId = mapping.SLALevelId;
                    existing.OverrideResponseTime = mapping.OverrideResponseTime;
                    existing.OverrideResolutionTime = mapping.OverrideResolutionTime;
                    existing.IsActive = mapping.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newMapping = new PrioritySLA
                    {
                        Id = Guid.NewGuid(),
                        PriorityId = mapping.PriorityId,
                        SLALevelId = mapping.SLALevelId,
                        OverrideResponseTime = mapping.OverrideResponseTime,
                        OverrideResolutionTime = mapping.OverrideResolutionTime,
                        IsActive = mapping.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.PrioritySLAs.Add(newMapping);
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { isSuccess = true, message = $"{request.Mappings.Count} mappings saved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating priority mappings");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Delete priority SLA mapping
    /// </summary>
    [HttpDelete("priority-mappings/{id}")]
    [HasPermission("ManageSLA")]
    public async Task<IActionResult> DeletePriorityMapping(Guid id)
    {
        try
        {
            var mapping = await _dbContext.PrioritySLAs.FindAsync(id);
            if (mapping == null)
            {
                return NotFound(new { isSuccess = false, message = "Mapping not found" });
            }

            mapping.IsDeleted = true;
            mapping.DeletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return Ok(new { isSuccess = true, message = "Mapping deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting priority mapping");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    #endregion

    #region SLA Status and Display Endpoints

    /// <summary>
    /// Get SLA status for a single complaint (for complaint detail view)
    /// </summary>
    [HttpGet("status/{complaintId}")]
    [HasPermission("ViewComplaints")]
    public async Task<IActionResult> GetComplaintSLAStatus(Guid complaintId)
    {
        try
        {
            var companyId = GetCompanyId();

            var complaint = await _dbContext.Complaints
                .Include(c => c.Category)
                .Include(c => c.PriorityMaster)
                .FirstOrDefaultAsync(c => c.Id == complaintId && c.CompanyId == companyId);

            if (complaint == null)
            {
                return NotFound(new { isSuccess = false, message = "Complaint not found" });
            }

            // Calculate SLA - ensure DateTime is UTC
            var submittedAtUtc = complaint.SubmittedAt.Kind == DateTimeKind.Utc
                ? complaint.SubmittedAt
                : DateTime.SpecifyKind(complaint.SubmittedAt, DateTimeKind.Utc);

            var slaResult = await _slaCalculator.CalculateSLADeadlineAsync(
                complaint.CategoryId,
                complaint.PriorityMasterId,
                companyId,
                submittedAtUtc);

            // Build status display
            var status = new SLAStatusDisplayDto
            {
                ComplaintId = complaint.Id,
                ComplaintNumber = complaint.ComplaintNumber,
                SLALevelName = slaResult.SLALevelName ?? "System Default",
                SLASource = slaResult.Source.ToString(),
                WorkingHoursApplied = slaResult.WorkingHoursApplied,

                // Response SLA
                ResponseDeadline = slaResult.ResponseDeadline,
                ResponseTimeMinutes = slaResult.ResponseTimeMinutes,
                ResponseRemainingMinutes = slaResult.ResponseDeadline.HasValue
                    ? (int)_slaCalculator.GetTimeRemainingMinutes(slaResult.ResponseDeadline.Value)
                    : 0,
                ResponseProgress = slaResult.ResponseDeadline.HasValue
                    ? _slaCalculator.GetSLAPercentageComplete(complaint.SubmittedAt, slaResult.ResponseDeadline.Value)
                    : 0,
                ResponseBreached = slaResult.ResponseDeadline.HasValue && _slaCalculator.IsSLABreached(slaResult.ResponseDeadline.Value),
                ResponseTimeRemaining = slaResult.ResponseDeadline.HasValue
                    ? FormatTimeRemaining(_slaCalculator.GetTimeRemainingMinutes(slaResult.ResponseDeadline.Value))
                    : "N/A",

                // Resolution SLA
                ResolutionDeadline = slaResult.ResolutionDeadline,
                ResolutionTimeMinutes = slaResult.ResolutionTimeMinutes,
                ResolutionRemainingMinutes = slaResult.ResolutionDeadline.HasValue
                    ? (int)_slaCalculator.GetTimeRemainingMinutes(slaResult.ResolutionDeadline.Value)
                    : 0,
                ResolutionProgress = slaResult.ResolutionDeadline.HasValue
                    ? _slaCalculator.GetSLAPercentageComplete(complaint.SubmittedAt, slaResult.ResolutionDeadline.Value)
                    : 0,
                ResolutionBreached = slaResult.ResolutionDeadline.HasValue && _slaCalculator.IsSLABreached(slaResult.ResolutionDeadline.Value),
                ResolutionTimeRemaining = slaResult.ResolutionDeadline.HasValue
                    ? FormatTimeRemaining(_slaCalculator.GetTimeRemainingMinutes(slaResult.ResolutionDeadline.Value))
                    : "N/A",

                CalculatedAt = DateTime.UtcNow,
                Notes = slaResult.Notes ?? ""
            };

            // Determine overall status
            var primaryDeadline = slaResult.ResolutionDeadline ?? slaResult.ResponseDeadline;
            if (primaryDeadline.HasValue)
            {
                var progress = _slaCalculator.GetSLAPercentageComplete(complaint.SubmittedAt, primaryDeadline);
                if (progress >= 100)
                {
                    status.Status = "Breached";
                    status.StatusColor = "#dc3545"; // Red
                    status.BadgeText = "BREACHED";
                    status.IsCritical = true;
                }
                else if (progress >= 90)
                {
                    status.Status = "Critical";
                    status.StatusColor = "#fd7e14"; // Orange
                    status.BadgeText = "Critical";
                    status.IsCritical = true;
                    status.ShowWarning = true;
                }
                else if (progress >= 70)
                {
                    status.Status = "Warning";
                    status.StatusColor = "#ffc107"; // Yellow
                    status.BadgeText = "Warning";
                    status.ShowWarning = true;
                }
                else
                {
                    status.Status = "OnTrack";
                    status.StatusColor = "#28a745"; // Green
                    status.BadgeText = "On Track";
                }
            }

            return Ok(new { isSuccess = true, data = status, message = "SLA status retrieved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SLA status for complaint {ComplaintId}", complaintId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get SLA status for multiple complaints (for complaint list view)
    /// </summary>
    [HttpPost("status/bulk")]
    [HasPermission("ViewComplaints")]
    public async Task<IActionResult> GetBulkComplaintSLAStatus([FromBody] SLABulkStatusRequest request)
    {
        try
        {
            var companyId = GetCompanyId();

            if (request.ComplaintIds == null || !request.ComplaintIds.Any())
            {
                return BadRequest(new { isSuccess = false, message = "Complaint IDs are required" });
            }

            var complaints = await _dbContext.Complaints
                .Include(c => c.Category)
                .Include(c => c.PriorityMaster)
                .Where(c => request.ComplaintIds.Contains(c.Id) && c.CompanyId == companyId)
                .ToListAsync();

            var response = new SLABulkStatusResponse
            {
                TotalCount = request.ComplaintIds.Count,
                SuccessCount = 0,
                FailedCount = 0
            };

            foreach (var complaint in complaints)
            {
                try
                {
                    var slaResult = await _slaCalculator.CalculateSLADeadlineAsync(
                        complaint.CategoryId,
                        complaint.PriorityMasterId,
                        companyId,
                        complaint.SubmittedAt);

                    var primaryDeadline = slaResult.ResolutionDeadline ?? slaResult.ResponseDeadline;
                    var remainingMinutes = primaryDeadline.HasValue
                        ? (int)_slaCalculator.GetTimeRemainingMinutes(primaryDeadline.Value)
                        : 0;
                    var progress = primaryDeadline.HasValue
                        ? _slaCalculator.GetSLAPercentageComplete(complaint.SubmittedAt, primaryDeadline.Value)
                        : 0;

                    var summary = new SLAStatusSummaryDto
                    {
                        ComplaintId = complaint.Id,
                        ComplaintNumber = complaint.ComplaintNumber,
                        PrimaryDeadline = primaryDeadline,
                        RemainingMinutes = remainingMinutes,
                        PercentageComplete = progress,
                        TimeRemaining = FormatTimeRemaining(remainingMinutes),
                        IsBreached = primaryDeadline.HasValue && _slaCalculator.IsSLABreached(primaryDeadline)
                    };

                    // Determine status and urgency level
                    if (progress >= 100)
                    {
                        summary.Status = "Breached";
                        summary.StatusColor = "#dc3545";
                        summary.BadgeText = "BREACHED";
                        summary.UrgencyLevel = "critical";
                        summary.IsCritical = true;
                    }
                    else if (progress >= 90)
                    {
                        summary.Status = "Critical";
                        summary.StatusColor = "#fd7e14";
                        summary.BadgeText = "Critical";
                        summary.UrgencyLevel = "high";
                        summary.IsCritical = true;
                        summary.IsWarning = true;
                    }
                    else if (progress >= 70)
                    {
                        summary.Status = "Warning";
                        summary.StatusColor = "#ffc107";
                        summary.BadgeText = "Warning";
                        summary.UrgencyLevel = "medium";
                        summary.IsWarning = true;
                    }
                    else
                    {
                        summary.Status = "OnTrack";
                        summary.StatusColor = "#28a745";
                        summary.BadgeText = "On Track";
                        summary.UrgencyLevel = "low";
                    }

                    response.Statuses[complaint.Id] = summary;
                    response.SuccessCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error calculating SLA for complaint {ComplaintId}", complaint.Id);
                    response.FailedCount++;
                }
            }

            response.FailedCount = response.TotalCount - response.SuccessCount;

            return Ok(new { isSuccess = true, data = response, message = $"{response.SuccessCount} SLA statuses retrieved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk SLA status");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get applicable SLA configuration for a category/priority combination
    /// </summary>
    [HttpPost("applicable")]
    [HasPermission("ViewSLA")]
    public async Task<IActionResult> GetApplicableSLA([FromBody] ApplicableSLARequest request)
    {
        try
        {
            var companyId = GetCompanyId();

            if (!request.CategoryId.HasValue)
            {
                return BadRequest(new { isSuccess = false, message = "Category ID is required" });
            }

            var category = await _dbContext.ComplaintCategories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value && !c.IsDeleted);
            if (category == null)
            {
                return NotFound(new { isSuccess = false, message = "Category not found" });
            }

            var priority = request.PriorityId.HasValue
                ? await _dbContext.ComplaintPriorityMasters.FirstOrDefaultAsync(p => p.Id == request.PriorityId.Value && !p.IsDeleted)
                : null;

            // Calculate SLA to determine which configuration applies
            var slaResult = await _slaCalculator.CalculateSLADeadlineAsync(
                request.CategoryId.Value,
                request.PriorityId,
                companyId,
                DateTime.UtcNow);

            var applicableSLA = new ApplicableSLADto
            {
                CategoryId = request.CategoryId.Value,
                CategoryName = category.Name,
                PriorityId = request.PriorityId,
                PriorityName = priority?.Name ?? "N/A",
                SLALevelId = slaResult.SLALevelId,
                SLALevelName = slaResult.SLALevelName ?? "System Default",
                Source = slaResult.Source.ToString(),
                Priority = slaResult.Source == SLASource.PriorityMapping ? 1 :
                           slaResult.Source == SLASource.CategoryMapping ? 2 : 3,
                ResponseTimeMinutes = slaResult.ResponseTimeMinutes,
                ResolutionTimeMinutes = slaResult.ResolutionTimeMinutes,
                ResponseTimeDisplay = FormatMinutesToTime(slaResult.ResponseTimeMinutes),
                ResolutionTimeDisplay = FormatMinutesToTime(slaResult.ResolutionTimeMinutes),
                WorkingHoursOnly = slaResult.WorkingHoursApplied,
                IsActive = true,
                Notes = slaResult.Notes ?? ""
            };

            return Ok(new { isSuccess = true, data = applicableSLA, message = "Applicable SLA retrieved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applicable SLA");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get SLA timeline for a complaint
    /// </summary>
    [HttpGet("timeline/{complaintId}")]
    [HasPermission("ViewComplaints")]
    public async Task<IActionResult> GetSLATimeline(Guid complaintId)
    {
        try
        {
            var companyId = GetCompanyId();

            var complaint = await _dbContext.Complaints
                .Include(c => c.Category)
                .Include(c => c.PriorityMaster)
                .FirstOrDefaultAsync(c => c.Id == complaintId && c.CompanyId == companyId);

            if (complaint == null)
            {
                return NotFound(new { isSuccess = false, message = "Complaint not found" });
            }

            var slaResult = await _slaCalculator.CalculateSLADeadlineAsync(
                complaint.CategoryId,
                complaint.PriorityMasterId,
                companyId,
                complaint.SubmittedAt);

            var timeline = new SLATimelineDto
            {
                ComplaintId = complaint.Id,
                ComplaintNumber = complaint.ComplaintNumber,
                StartTime = complaint.SubmittedAt,
                ResponseDeadline = slaResult.ResponseDeadline,
                ResolutionDeadline = slaResult.ResolutionDeadline,
                ResponseBreached = slaResult.ResponseDeadline.HasValue && _slaCalculator.IsSLABreached(slaResult.ResponseDeadline),
                ResolutionBreached = slaResult.ResolutionDeadline.HasValue && _slaCalculator.IsSLABreached(slaResult.ResolutionDeadline),
                CurrentStatus = complaint.StatusMaster?.Name ?? "Unknown"
            };

            // Add timeline events
            timeline.Events.Add(new SLATimelineEventDto
            {
                Timestamp = complaint.SubmittedAt,
                EventType = "Started",
                Description = "Complaint submitted - SLA timer started",
                Icon = "bi-play-circle",
                Color = "#0d6efd"
            });

            if (slaResult.ResponseDeadline.HasValue)
            {
                var responseRemaining = _slaCalculator.GetTimeRemainingMinutes(slaResult.ResponseDeadline.Value);
                timeline.Events.Add(new SLATimelineEventDto
                {
                    Timestamp = slaResult.ResponseDeadline.Value,
                    EventType = responseRemaining >= 0 ? "ResponseDue" : "ResponseBreached",
                    Description = responseRemaining >= 0
                        ? "Response SLA deadline"
                        : "Response SLA breached",
                    Icon = responseRemaining >= 0 ? "bi-clock" : "bi-exclamation-triangle",
                    Color = responseRemaining >= 0 ? "#ffc107" : "#dc3545"
                });
            }

            if (slaResult.ResolutionDeadline.HasValue)
            {
                var resolutionRemaining = _slaCalculator.GetTimeRemainingMinutes(slaResult.ResolutionDeadline.Value);
                timeline.Events.Add(new SLATimelineEventDto
                {
                    Timestamp = slaResult.ResolutionDeadline.Value,
                    EventType = resolutionRemaining >= 0 ? "ResolutionDue" : "ResolutionBreached",
                    Description = resolutionRemaining >= 0
                        ? "Resolution SLA deadline"
                        : "Resolution SLA breached",
                    Icon = resolutionRemaining >= 0 ? "bi-clock" : "bi-exclamation-triangle",
                    Color = resolutionRemaining >= 0 ? "#ffc107" : "#dc3545"
                });
            }

            // Sort events chronologically
            timeline.Events = timeline.Events.OrderBy(e => e.Timestamp).ToList();

            return Ok(new { isSuccess = true, data = timeline, message = "Timeline retrieved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SLA timeline for complaint {ComplaintId}", complaintId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get SLA coverage matrix showing configuration across categories and priorities
    /// </summary>
    [HttpGet("coverage-matrix")]
    [HasPermission("ViewSLA")]
    public async Task<IActionResult> GetSLACoverageMatrix()
    {
        try
        {
            var companyId = GetCompanyId();

            var matrix = new SLACoverageMatrixDto
            {
                GeneratedAt = DateTime.UtcNow
            };

            // Get all active categories
            var categories = await _dbContext.ComplaintCategories
                .Where(c => !c.IsDeleted && c.IsActive)
                .ToListAsync();

            var categoryMappings = await _dbContext.CategorySLAs
                .Include(c => c.SLALevel)
                .Where(c => c.SLALevel.CompanyId == companyId && !c.IsDeleted)
                .ToDictionaryAsync(c => c.CategoryId);

            // Add category rows
            foreach (var category in categories)
            {
                var hasMapping = categoryMappings.TryGetValue(category.Id, out var mapping);
                matrix.Rows.Add(new SLACoverageRowDto
                {
                    EntityId = category.Id,
                    EntityName = category.Name,
                    EntityType = "Category",
                    HasSLA = hasMapping && mapping.IsActive,
                    SLALevelName = hasMapping ? mapping.SLALevel.Name : null,
                    ResponseTimeMinutes = hasMapping ? mapping.GetEffectiveResponseTimeMinutes() : null,
                    ResolutionTimeMinutes = hasMapping ? mapping.GetEffectiveResolutionTimeMinutes() : null,
                    Source = hasMapping && mapping.IsActive ? "CategoryMapping" : "SystemDefault",
                    CoverageStatus = hasMapping && mapping.IsActive ? "Configured" : "Using Default",
                    StatusColor = hasMapping && mapping.IsActive ? "#28a745" : "#6c757d"
                });
            }

            // Get all active priorities
            var priorities = await _dbContext.ComplaintPriorityMasters
                .Where(p => p.CompanyId == companyId && !p.IsDeleted && p.IsActive)
                .ToListAsync();

            var priorityMappings = await _dbContext.PrioritySLAs
                .Include(p => p.SLALevel)
                .Where(p => p.SLALevel.CompanyId == companyId && !p.IsDeleted)
                .ToDictionaryAsync(p => p.PriorityId);

            // Add priority rows
            foreach (var priority in priorities)
            {
                var hasMapping = priorityMappings.TryGetValue(priority.Id, out var mapping);
                matrix.Rows.Add(new SLACoverageRowDto
                {
                    EntityId = priority.Id,
                    EntityName = priority.Name,
                    EntityType = "Priority",
                    HasSLA = hasMapping && mapping.IsActive,
                    SLALevelName = hasMapping ? mapping.SLALevel.Name : null,
                    ResponseTimeMinutes = hasMapping ? mapping.GetEffectiveResponseTimeMinutes() : null,
                    ResolutionTimeMinutes = hasMapping ? mapping.GetEffectiveResolutionTimeMinutes() : null,
                    Source = hasMapping && mapping.IsActive ? "PriorityMapping" : "SystemDefault",
                    CoverageStatus = hasMapping && mapping.IsActive ? "Configured" : "Using Default",
                    StatusColor = hasMapping && mapping.IsActive ? "#28a745" : "#6c757d"
                });
            }

            // Calculate summary
            matrix.TotalCategories = categories.Count();
            matrix.CategoriesWithSLA = matrix.Rows.Count(r => r.EntityType == "Category" && r.HasSLA);
            matrix.CategoriesWithoutSLA = matrix.TotalCategories - matrix.CategoriesWithSLA;
            matrix.CoveragePercentage = matrix.TotalCategories > 0
                ? (double)matrix.CategoriesWithSLA / matrix.TotalCategories * 100
                : 0;

            matrix.TotalPriorities = priorities.Count();
            matrix.PrioritiesWithSLA = matrix.Rows.Count(r => r.EntityType == "Priority" && r.HasSLA);
            matrix.PrioritiesWithoutSLA = matrix.TotalPriorities - matrix.PrioritiesWithSLA;

            return Ok(new { isSuccess = true, data = matrix, message = "Coverage matrix retrieved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SLA coverage matrix");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get all complaints with SLA warnings (approaching or breaching SLA)
    /// </summary>
    [HttpGet("warnings")]
    [HasPermission("ViewComplaints")]
    public async Task<IActionResult> GetSLAWarnings()
    {
        try
        {
            var companyId = GetCompanyId();

            // Get all active complaints
            var complaints = await _dbContext.Complaints
                .Include(c => c.Category)
                .Include(c => c.PriorityMaster)
                .Include(c => c.StatusMaster)
                .Include(c => c.AssignedTo)
                .Where(c => c.CompanyId == companyId && !c.IsDeleted)
                .Where(c => c.StatusMaster == null || !c.StatusMaster.IsFinal) // Only active complaints
                .ToListAsync();

            var warnings = new List<SLAWarningDto>();

            foreach (var complaint in complaints)
            {
                var slaResult = await _slaCalculator.CalculateSLADeadlineAsync(
                    complaint.CategoryId,
                    complaint.PriorityMasterId,
                    companyId,
                    complaint.SubmittedAt);

                var primaryDeadline = slaResult.ResolutionDeadline ?? slaResult.ResponseDeadline;
                if (!primaryDeadline.HasValue) continue;

                var progress = _slaCalculator.GetSLAPercentageComplete(
                    complaint.SubmittedAt,
                    primaryDeadline.Value);

                // Only include if warning (70%+) or breached (100%+)
                if (progress < 70) continue;

                var remainingMinutes = (int)_slaCalculator.GetTimeRemainingMinutes(primaryDeadline.Value);

                var warning = new SLAWarningDto
                {
                    ComplaintId = complaint.Id,
                    ComplaintNumber = complaint.ComplaintNumber,
                    Title = complaint.Title,
                    CategoryName = complaint.Category?.Name ?? "Unknown",
                    PriorityName = complaint.PriorityMaster?.Name ?? "Unknown",
                    ResponseDeadline = slaResult.ResponseDeadline,
                    ResolutionDeadline = slaResult.ResolutionDeadline,
                    ResponseRemainingMinutes = slaResult.ResponseDeadline.HasValue
                        ? (int)_slaCalculator.GetTimeRemainingMinutes(slaResult.ResponseDeadline.Value)
                        : 0,
                    ResolutionRemainingMinutes = remainingMinutes,
                    PercentageComplete = progress,
                    TimeRemaining = FormatTimeRemaining(remainingMinutes),
                    AssignedToUserId = complaint.AssignedToId,
                    AssignedToUserName = complaint.AssignedTo?.FullName ?? "Unassigned",
                    SubmittedAt = complaint.SubmittedAt,
                    CalculatedAt = DateTime.UtcNow
                };

                // Determine warning level
                if (progress >= 100)
                {
                    warning.WarningLevel = "Breached";
                    warning.WarningColor = "#dc3545";
                }
                else if (progress >= 90)
                {
                    warning.WarningLevel = "Critical";
                    warning.WarningColor = "#fd7e14";
                }
                else
                {
                    warning.WarningLevel = "Warning";
                    warning.WarningColor = "#ffc107";
                }

                warnings.Add(warning);
            }

            // Sort by urgency (highest percentage first)
            warnings = warnings.OrderByDescending(w => w.PercentageComplete).ToList();

            var response = new SLAWarningsResponse
            {
                Warnings = warnings,
                TotalWarnings = warnings.Count,
                BreachedCount = warnings.Count(w => w.WarningLevel == "Breached"),
                CriticalCount = warnings.Count(w => w.WarningLevel == "Critical"),
                WarningCount = warnings.Count(w => w.WarningLevel == "Warning"),
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(new { isSuccess = true, data = response, message = $"{warnings.Count} warnings found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SLA warnings");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred" });
        }
    }

    #endregion

    #region Helper Methods

    private SLASettingsDto MapToSettingsDto(SLASettings settings)
    {
        return new SLASettingsDto
        {
            Id = settings.Id,
            IsEnabled = settings.IsEnabled,
            WorkingHoursOnly = settings.WorkingHoursOnly,
            WorkingHoursStart = settings.WorkingHoursStart,
            WorkingHoursEnd = settings.WorkingHoursEnd,
            WorkingDays = settings.WorkingDays,
            ExcludeHolidays = settings.ExcludeHolidays,
            AutoEscalateOnBreach = settings.AutoEscalateOnBreach,
            EscalationThresholdPercent = settings.EscalationThresholdPercent,
            NotifyBeforeBreach = settings.NotifyBeforeBreach,
            NotifyBeforeBreachMinutes = settings.NotifyBeforeBreachMinutes,
            PauseSLAOnPendingInfo = settings.PauseSLAOnPendingInfo,
            Timezone = settings.Timezone,
            CompanyId = settings.CompanyId,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }

    private SLALevelDto MapToLevelDto(SLALevel level)
    {
        return new SLALevelDto
        {
            Id = level.Id,
            Name = level.Name,
            Description = level.Description,
            Order = level.Order,
            IsActive = level.IsActive,
            ColorCode = level.ColorCode,
            DefaultResponseTime = level.DefaultResponseTime,
            ResponseTimeUnit = level.ResponseTimeUnit.ToString(),
            DefaultResolutionTime = level.DefaultResolutionTime,
            ResolutionTimeUnit = level.ResolutionTimeUnit.ToString(),
            ResponseTimeInMinutes = level.GetResponseTimeInMinutes(),
            ResolutionTimeInMinutes = level.GetResolutionTimeInMinutes(),
            ResponseTimeDisplay = level.GetResponseTimeDisplay(),
            ResolutionTimeDisplay = level.GetResolutionTimeDisplay(),
            CompanyId = level.CompanyId,
            CreatedAt = level.CreatedAt,
            UpdatedAt = level.UpdatedAt
        };
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get company ID from the current user's claims
    /// </summary>
    private Guid GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
        {
            throw new UnauthorizedAccessException("Company ID not found in token");
        }
        return companyId;
    }

    /// <summary>
    /// Format remaining time in minutes to human-readable string
    /// </summary>
    private string FormatTimeRemaining(double minutes)
    {
        if (minutes < 0)
        {
            var absMinutes = Math.Abs(minutes);
            if (absMinutes < 60)
                return $"OVERDUE {absMinutes:F0}m";
            if (absMinutes < 1440)
                return $"OVERDUE {absMinutes / 60:F0}h {absMinutes % 60:F0}m";
            return $"OVERDUE {absMinutes / 1440:F0}d {(absMinutes % 1440) / 60:F0}h";
        }

        if (minutes < 60)
            return $"{minutes:F0}m";
        if (minutes < 1440)
            return $"{minutes / 60:F0}h {minutes % 60:F0}m";
        return $"{minutes / 1440:F0}d {(minutes % 1440) / 60:F0}h";
    }

    /// <summary>
    /// Format minutes to time display (e.g., "2 hours", "24 hours", "3 days")
    /// </summary>
    private string FormatMinutesToTime(int minutes)
    {
        if (minutes == 0)
            return "Not set";
        if (minutes < 60)
            return $"{minutes} minute{(minutes != 1 ? "s" : "")}";
        if (minutes < 1440)
        {
            var hours = minutes / 60;
            return $"{hours} hour{(hours != 1 ? "s" : "")}";
        }
        var days = minutes / 1440;
        return $"{days} day{(days != 1 ? "s" : "")}";
    }

    #endregion
}
