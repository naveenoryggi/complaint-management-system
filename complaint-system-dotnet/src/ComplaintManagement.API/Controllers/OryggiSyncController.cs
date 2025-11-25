using ComplaintManagement.Application.DTOs.Sync;
using ComplaintManagement.Infrastructure.Data;
using ComplaintManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/OryggiSync")]
[Authorize]
public class OryggiSyncController : ControllerBase
{
    private readonly OryggiSyncService _syncService;
    private readonly ComplaintDbContext _context;
    private readonly ILogger<OryggiSyncController> _logger;
    private readonly ISqlDiagnosticsService _diagnosticsService;

    public OryggiSyncController(
        OryggiSyncService syncService,
        ComplaintDbContext context,
        ILogger<OryggiSyncController> logger,
        ISqlDiagnosticsService diagnosticsService)
    {
        _syncService = syncService;
        _context = context;
        _logger = logger;
        _diagnosticsService = diagnosticsService;
    }

    /// <summary>
    /// Manually trigger a full sync from Oryggi database
    /// </summary>
    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerSync([FromBody] TriggerSyncRequest? request = null)
    {
        try
        {
            // Get tenant ID from request or use default tenant
            Guid tenantId;

            if (request?.TenantId != null && request.TenantId != Guid.Empty)
            {
                tenantId = request.TenantId.Value;
                _logger.LogInformation("Manual sync triggered by user for tenant {TenantId}", tenantId);
            }
            else
            {
                // Fetch default tenant from database
                var defaultTenant = await _context.Tenants
                    .Where(t => t.Code == "DEFAULT")
                    .FirstOrDefaultAsync();

                if (defaultTenant == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No tenant ID provided and no default tenant found in the system"
                    });
                }

                tenantId = defaultTenant.Id;
                _logger.LogInformation("Manual sync triggered by user for default tenant {TenantId} ({TenantName})",
                    tenantId, defaultTenant.Name);
            }

            var result = await _syncService.SyncAllAsync(tenantId, "MANUAL");

            if (result.Status == "SUCCESS")
            {
                return Ok(new
                {
                    success = true,
                    message = "Sync completed successfully",
                    data = new
                    {
                        syncLogId = result.SyncLogId,
                        duration = result.Duration?.TotalSeconds,
                        companies = new { created = result.CompaniesCreated, updated = result.CompaniesUpdated, total = result.CompaniesProcessed },
                        branches = new { created = result.BranchesCreated, updated = result.BranchesUpdated, total = result.BranchesProcessed },
                        departments = new { created = result.DepartmentsCreated, updated = result.DepartmentsUpdated, total = result.DepartmentsProcessed },
                        sections = new { created = result.SectionsCreated, updated = result.SectionsUpdated, total = result.SectionsProcessed },
                        employees = new { created = result.EmployeesCreated, updated = result.EmployeesUpdated, total = result.EmployeesProcessed }
                    }
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Sync failed",
                    error = result.ErrorMessage,
                    details = result.ErrorDetails
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering manual sync");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while triggering sync",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get sync history for a tenant
    /// </summary>
    [HttpGet("history/{tenantId}")]
    public async Task<IActionResult> GetSyncHistory(Guid tenantId, [FromQuery] int count = 10)
    {
        try
        {
            var history = await _syncService.GetSyncHistoryAsync(tenantId, count);

            return Ok(new
            {
                success = true,
                data = history.Select(s => new
                {
                    syncLogId = s.Id,  // Use Id (primary key) instead of SyncLogId
                    syncType = s.SyncType,
                    status = s.Status,
                    startedAt = s.SyncStartedAt,
                    completedAt = s.SyncCompletedAt,
                    duration = s.Duration?.TotalSeconds,
                    companies = new { created = s.CompaniesCreated, updated = s.CompaniesUpdated, total = s.CompaniesProcessed },
                    branches = new { created = s.BranchesCreated, updated = s.BranchesUpdated, total = s.BranchesProcessed },
                    departments = new { created = s.DepartmentsCreated, updated = s.DepartmentsUpdated, total = s.DepartmentsProcessed },
                    sections = new { created = s.SectionsCreated, updated = s.SectionsUpdated, total = s.SectionsProcessed },
                    employees = new { created = s.EmployeesCreated, updated = s.EmployeesUpdated, total = s.EmployeesProcessed },
                    users = new { created = s.UsersCreated, updated = s.UsersUpdated, total = s.UsersProcessed },
                    error = s.ErrorMessage
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync history");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving sync history",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get latest sync status for a tenant
    /// </summary>
    [HttpGet("status/{tenantId}")]
    public async Task<IActionResult> GetSyncStatus(Guid tenantId)
    {
        try
        {
            var latestSync = await _syncService.GetLatestSyncAsync(tenantId);

            if (latestSync == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "No sync history found",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    syncLogId = latestSync.SyncLogId,
                    syncType = latestSync.SyncType,
                    status = latestSync.Status,
                    startedAt = latestSync.SyncStartedAt,
                    completedAt = latestSync.SyncCompletedAt,
                    duration = latestSync.Duration?.TotalSeconds,
                    companies = new { created = latestSync.CompaniesCreated, updated = latestSync.CompaniesUpdated, total = latestSync.CompaniesProcessed },
                    branches = new { created = latestSync.BranchesCreated, updated = latestSync.BranchesUpdated, total = latestSync.BranchesProcessed },
                    departments = new { created = latestSync.DepartmentsCreated, updated = latestSync.DepartmentsUpdated, total = latestSync.DepartmentsProcessed },
                    sections = new { created = latestSync.SectionsCreated, updated = latestSync.SectionsUpdated, total = latestSync.SectionsProcessed },
                    employees = new { created = latestSync.EmployeesCreated, updated = latestSync.EmployeesUpdated, total = latestSync.EmployeesProcessed },
                    error = latestSync.ErrorMessage
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync status");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving sync status",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get all sync schedules
    /// </summary>
    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules()
    {
        try
        {
            var schedules = await _context.SyncSchedules
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.ScheduleType)
                .ThenBy(s => s.TimeOfDay)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = schedules.Select(s => new
                {
                    id = s.Id,
                    tenantId = s.TenantId,
                    scheduleType = s.ScheduleType,
                    timeOfDay = s.TimeOfDay,
                    dayValue = s.DayValue,
                    isEnabled = s.IsEnabled,
                    lastRunAt = s.LastRunAt,
                    nextRunAt = s.NextRunAt,
                    description = s.Description,
                    createdAt = s.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync schedules");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving sync schedules",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get sync schedule by ID
    /// </summary>
    [HttpGet("schedules/{id}")]
    public async Task<IActionResult> GetSchedule(Guid id)
    {
        try
        {
            var schedule = await _context.SyncSchedules
                .Where(s => s.Id == id && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Schedule not found"
                });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    id = schedule.Id,
                    tenantId = schedule.TenantId,
                    scheduleType = schedule.ScheduleType,
                    timeOfDay = schedule.TimeOfDay,
                    dayValue = schedule.DayValue,
                    isEnabled = schedule.IsEnabled,
                    lastRunAt = schedule.LastRunAt,
                    nextRunAt = schedule.NextRunAt,
                    description = schedule.Description,
                    createdAt = schedule.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync schedule");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving sync schedule",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Create a new sync schedule
    /// </summary>
    [HttpPost("schedules")]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest request)
    {
        try
        {
            // Get default tenant if not provided
            Guid tenantId;
            if (request.TenantId != null && request.TenantId != Guid.Empty)
            {
                tenantId = request.TenantId.Value;
            }
            else
            {
                var defaultTenant = await _context.Tenants
                    .Where(t => t.Code == "DEFAULT")
                    .FirstOrDefaultAsync();

                if (defaultTenant == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No tenant ID provided and no default tenant found"
                    });
                }
                tenantId = defaultTenant.Id;
            }

            // Validate schedule type
            if (!new[] { "Daily", "Weekly", "Monthly" }.Contains(request.ScheduleType))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid schedule type. Must be Daily, Weekly, or Monthly"
                });
            }

            // Validate day value
            if (request.ScheduleType == "Weekly" && (request.DayValue < 0 || request.DayValue > 6))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "For Weekly schedules, DayValue must be 0-6 (Sunday-Saturday)"
                });
            }

            if (request.ScheduleType == "Monthly" && (request.DayValue < 1 || request.DayValue > 31))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "For Monthly schedules, DayValue must be 1-31"
                });
            }

            var schedule = new ComplaintManagement.Domain.Entities.SyncSchedule
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ScheduleType = request.ScheduleType,
                TimeOfDay = request.TimeOfDay,
                DayValue = request.DayValue,
                IsEnabled = request.IsEnabled ?? true,
                Description = request.Description,
                NextRunAt = CalculateNextRun(request.ScheduleType, request.TimeOfDay, request.DayValue)
            };

            _context.SyncSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created sync schedule {ScheduleId} for tenant {TenantId}", schedule.Id, tenantId);

            return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, new
            {
                success = true,
                message = "Schedule created successfully",
                data = new
                {
                    id = schedule.Id,
                    tenantId = schedule.TenantId,
                    scheduleType = schedule.ScheduleType,
                    timeOfDay = schedule.TimeOfDay,
                    dayValue = schedule.DayValue,
                    isEnabled = schedule.IsEnabled,
                    nextRunAt = schedule.NextRunAt,
                    description = schedule.Description
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sync schedule");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while creating sync schedule",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Update an existing sync schedule
    /// </summary>
    [HttpPut("schedules/{id}")]
    public async Task<IActionResult> UpdateSchedule(Guid id, [FromBody] UpdateScheduleRequest request)
    {
        try
        {
            var schedule = await _context.SyncSchedules
                .Where(s => s.Id == id && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Schedule not found"
                });
            }

            // Validate schedule type
            if (!new[] { "Daily", "Weekly", "Monthly" }.Contains(request.ScheduleType))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid schedule type. Must be Daily, Weekly, or Monthly"
                });
            }

            // Update fields
            schedule.ScheduleType = request.ScheduleType;
            schedule.TimeOfDay = request.TimeOfDay;
            schedule.DayValue = request.DayValue;
            schedule.IsEnabled = request.IsEnabled;
            schedule.Description = request.Description;
            schedule.NextRunAt = CalculateNextRun(request.ScheduleType, request.TimeOfDay, request.DayValue);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated sync schedule {ScheduleId}", id);

            return Ok(new
            {
                success = true,
                message = "Schedule updated successfully",
                data = new
                {
                    id = schedule.Id,
                    tenantId = schedule.TenantId,
                    scheduleType = schedule.ScheduleType,
                    timeOfDay = schedule.TimeOfDay,
                    dayValue = schedule.DayValue,
                    isEnabled = schedule.IsEnabled,
                    nextRunAt = schedule.NextRunAt,
                    description = schedule.Description
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sync schedule");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while updating sync schedule",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Delete a sync schedule
    /// </summary>
    [HttpDelete("schedules/{id}")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        try
        {
            var schedule = await _context.SyncSchedules
                .Where(s => s.Id == id && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Schedule not found"
                });
            }

            // Soft delete
            _context.SyncSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted sync schedule {ScheduleId}", id);

            return Ok(new
            {
                success = true,
                message = "Schedule deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sync schedule");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while deleting sync schedule",
                error = ex.Message
            });
        }
    }

    private DateTime? CalculateNextRun(string scheduleType, string timeOfDay, int? dayValue)
    {
        try
        {
            var timeParts = timeOfDay.Split(':');
            if (timeParts.Length != 2)
                return null;

            var hour = int.Parse(timeParts[0]);
            var minute = int.Parse(timeParts[1]);
            var now = DateTime.UtcNow;

            DateTime nextRun = scheduleType switch
            {
                "Daily" => new DateTime(now.Year, now.Month, now.Day, hour, minute, 0, DateTimeKind.Utc),
                "Weekly" => CalculateNextWeeklyRun(now, dayValue ?? 0, hour, minute),
                "Monthly" => CalculateNextMonthlyRun(now, dayValue ?? 1, hour, minute),
                _ => now
            };

            // If calculated time is in the past, move to next occurrence
            if (nextRun <= now)
            {
                nextRun = scheduleType switch
                {
                    "Daily" => nextRun.AddDays(1),
                    "Weekly" => nextRun.AddDays(7),
                    "Monthly" => nextRun.AddMonths(1),
                    _ => nextRun
                };
            }

            return nextRun;
        }
        catch
        {
            return null;
        }
    }

    private DateTime CalculateNextWeeklyRun(DateTime now, int dayOfWeek, int hour, int minute)
    {
        var daysUntilTarget = ((int)dayOfWeek - (int)now.DayOfWeek + 7) % 7;
        var targetDate = now.Date.AddDays(daysUntilTarget);
        return new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, hour, minute, 0, DateTimeKind.Utc);
    }

    private DateTime CalculateNextMonthlyRun(DateTime now, int dayOfMonth, int hour, int minute)
    {
        var targetDate = new DateTime(now.Year, now.Month, Math.Min(dayOfMonth, DateTime.DaysInMonth(now.Year, now.Month)));
        return new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, hour, minute, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Get SQL diagnostics including blocked processes, long-running queries, etc.
    /// </summary>
    [HttpGet("diagnostics")]
    public async Task<IActionResult> GetDiagnostics()
    {
        try
        {
            var diagnostics = await _diagnosticsService.GetDiagnosticsAsync();

            return Ok(new
            {
                success = true,
                data = diagnostics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SQL diagnostics");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving SQL diagnostics",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get SQL health check summary
    /// </summary>
    [HttpGet("diagnostics/health")]
    public async Task<IActionResult> GetHealthCheck()
    {
        try
        {
            var healthCheck = await _diagnosticsService.GetHealthCheckAsync();

            return Ok(new
            {
                success = true,
                data = healthCheck
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SQL health check");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving SQL health check",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Kill a specific SQL session
    /// </summary>
    [HttpPost("diagnostics/kill-session")]
    public async Task<IActionResult> KillSession([FromBody] KillSessionRequest request)
    {
        try
        {
            var result = await _diagnosticsService.KillSessionAsync(request.SessionId, request.Reason);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error killing SQL session {SessionId}", request.SessionId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while killing SQL session",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Mark all stuck syncs as failed
    /// </summary>
    [HttpPost("diagnostics/cleanup-stuck-syncs")]
    public async Task<IActionResult> CleanupStuckSyncs()
    {
        try
        {
            var success = await _diagnosticsService.MarkStuckSyncsAsFailedAsync();

            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = "Stuck syncs cleaned up successfully"
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to cleanup stuck syncs"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up stuck syncs");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while cleaning up stuck syncs",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Mark a single stuck sync as failed by its sync log ID
    /// </summary>
    [HttpPost("diagnostics/cleanup-sync/{syncLogId}")]
    public async Task<IActionResult> CleanupSingleSync(Guid syncLogId)
    {
        try
        {
            var success = await _diagnosticsService.MarkSingleSyncAsFailedAsync(syncLogId);

            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = "Sync cleaned up successfully"
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to cleanup sync. It may not exist or is not in IN_PROGRESS status."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up sync {SyncLogId}", syncLogId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while cleaning up sync",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Manually sync a single employee by their CorpEmpCode
    /// </summary>
    [HttpPost("employee/{corpEmpCode}")]
    public async Task<IActionResult> SyncSingleEmployee(string corpEmpCode, [FromQuery] Guid? tenantId = null)
    {
        try
        {
            // Get tenant ID from query parameter or use default tenant
            Guid actualTenantId;

            if (tenantId != null && tenantId != Guid.Empty)
            {
                actualTenantId = tenantId.Value;
                _logger.LogInformation("Manual employee sync triggered for CorpEmpCode {CorpEmpCode}, tenant {TenantId}", corpEmpCode, actualTenantId);
            }
            else
            {
                // Fetch default tenant from database
                var defaultTenant = await _context.Tenants
                    .Where(t => t.Code == "DEFAULT" && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (defaultTenant == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No tenant ID provided and no default tenant found in the system"
                    });
                }

                actualTenantId = defaultTenant.Id;
                _logger.LogInformation("Manual employee sync triggered for CorpEmpCode {CorpEmpCode}, default tenant {TenantId} ({TenantName})",
                    corpEmpCode, actualTenantId, defaultTenant.Name);
            }

            var (success, message, syncLog) = await _syncService.SyncSingleEmployeeAsync(actualTenantId, corpEmpCode);

            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = message,
                    data = new
                    {
                        syncLogId = syncLog?.SyncLogId,
                        status = syncLog?.Status,
                        duration = syncLog?.Duration?.TotalSeconds,
                        employee = new
                        {
                            created = syncLog?.EmployeesCreated ?? 0,
                            updated = syncLog?.EmployeesUpdated ?? 0,
                            failed = syncLog?.EmployeesFailed ?? 0
                        },
                        user = new
                        {
                            created = syncLog?.UsersCreated ?? 0,
                            updated = syncLog?.UsersUpdated ?? 0,
                            failed = syncLog?.UsersFailed ?? 0
                        }
                    }
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = message
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing employee with CorpEmpCode {CorpEmpCode}", corpEmpCode);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while syncing employee",
                error = ex.Message
            });
        }
    }
}

public class TriggerSyncRequest
{
    public Guid? TenantId { get; set; }
}

public class CreateScheduleRequest
{
    public Guid? TenantId { get; set; }
    public string ScheduleType { get; set; } = null!; // "Daily", "Weekly", "Monthly"
    public string TimeOfDay { get; set; } = null!; // HH:mm format
    public int? DayValue { get; set; } // For Weekly (0-6) or Monthly (1-31)
    public bool? IsEnabled { get; set; }
    public string? Description { get; set; }
}

public class UpdateScheduleRequest
{
    public string ScheduleType { get; set; } = null!;
    public string TimeOfDay { get; set; } = null!;
    public int? DayValue { get; set; }
    public bool IsEnabled { get; set; }
    public string? Description { get; set; }
}
