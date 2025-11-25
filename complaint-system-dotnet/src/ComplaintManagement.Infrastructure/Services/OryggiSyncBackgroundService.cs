using ComplaintManagement.Domain.Configuration;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComplaintManagement.Infrastructure.Services;

public class OryggiSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OryggiSyncBackgroundService> _logger;
    private readonly OryggiSettings _settings;

    public OryggiSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OryggiSyncBackgroundService> logger,
        IOptions<OryggiSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Oryggi sync is disabled in configuration");
            return;
        }

        _logger.LogInformation("Oryggi sync background service started");

        // Sync on startup if configured
        if (_settings.SyncOnStartup)
        {
            _logger.LogInformation("Performing startup sync");
            await PerformSyncForAllTenantsAsync("STARTUP", stoppingToken);
        }

        // Schedule-based sync loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExecuteScheduledSyncsAsync(stoppingToken);

                // Check every minute for schedules
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Oryggi sync background service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Oryggi sync background service");
                // Wait before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task CheckAndExecuteScheduledSyncsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var appContext = scope.ServiceProvider.GetRequiredService<ComplaintDbContext>();
            var now = DateTime.UtcNow;

            // Get all enabled schedules that are due to run
            var dueSchedules = await appContext.SyncSchedules
                .Where(s => s.IsEnabled && !s.IsDeleted &&
                           (s.NextRunAt == null || s.NextRunAt <= now))
                .ToListAsync(cancellationToken);

            if (!dueSchedules.Any())
                return;

            _logger.LogInformation("Found {Count} schedules due to run", dueSchedules.Count);

            foreach (var schedule in dueSchedules)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogInformation("Executing scheduled sync for schedule {ScheduleId} ({ScheduleType})",
                        schedule.Id, schedule.ScheduleType);

                    // Execute sync for the tenant
                    var syncService = scope.ServiceProvider.GetRequiredService<OryggiSyncService>();
                    var result = await syncService.SyncAllAsync(schedule.TenantId, "SCHEDULED");

                    // Update schedule run times
                    schedule.LastRunAt = now;
                    schedule.NextRunAt = CalculateNextRun(schedule);

                    await appContext.SaveChangesAsync(cancellationToken);

                    if (result.Status == "SUCCESS")
                    {
                        _logger.LogInformation("Scheduled sync {ScheduleId} completed successfully. Next run at {NextRun}",
                            schedule.Id, schedule.NextRunAt);
                    }
                    else
                    {
                        _logger.LogError("Scheduled sync {ScheduleId} failed: {Error}",
                            schedule.Id, result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing scheduled sync {ScheduleId}", schedule.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking and executing scheduled syncs");
        }
    }

    private DateTime? CalculateNextRun(Domain.Entities.SyncSchedule schedule)
    {
        try
        {
            var timeParts = schedule.TimeOfDay.Split(':');
            if (timeParts.Length != 2)
                return null;

            var hour = int.Parse(timeParts[0]);
            var minute = int.Parse(timeParts[1]);
            var now = DateTime.UtcNow;

            DateTime nextRun = schedule.ScheduleType switch
            {
                "Daily" => new DateTime(now.Year, now.Month, now.Day, hour, minute, 0, DateTimeKind.Utc).AddDays(1),
                "Weekly" => CalculateNextWeeklyRun(now, schedule.DayValue ?? 0, hour, minute),
                "Monthly" => CalculateNextMonthlyRun(now, schedule.DayValue ?? 1, hour, minute),
                _ => now.AddHours(24)
            };

            // If calculated time is still in the past, add another period
            while (nextRun <= now)
            {
                nextRun = schedule.ScheduleType switch
                {
                    "Daily" => nextRun.AddDays(1),
                    "Weekly" => nextRun.AddDays(7),
                    "Monthly" => nextRun.AddMonths(1),
                    _ => nextRun.AddDays(1)
                };
            }

            return nextRun;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next run for schedule {ScheduleId}", schedule.Id);
            return null;
        }
    }

    private DateTime CalculateNextWeeklyRun(DateTime now, int dayOfWeek, int hour, int minute)
    {
        var daysUntilTarget = ((int)dayOfWeek - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0) daysUntilTarget = 7; // Next week
        var targetDate = now.Date.AddDays(daysUntilTarget);
        return new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, hour, minute, 0, DateTimeKind.Utc);
    }

    private DateTime CalculateNextMonthlyRun(DateTime now, int dayOfMonth, int hour, int minute)
    {
        var nextMonth = now.AddMonths(1);
        var targetDay = Math.Min(dayOfMonth, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
        var targetDate = new DateTime(nextMonth.Year, nextMonth.Month, targetDay, hour, minute, 0, DateTimeKind.Utc);
        return targetDate;
    }

    private async Task PerformSyncForAllTenantsAsync(string syncType, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var appContext = scope.ServiceProvider.GetRequiredService<ComplaintDbContext>();
            var syncService = scope.ServiceProvider.GetRequiredService<OryggiSyncService>();

            // Get all active tenants
            var tenants = await appContext.Tenants
                .Where(t => t.IsActive)
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Starting {SyncType} sync for {Count} tenants", syncType, tenants.Count);

            foreach (var tenantId in tenants)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogInformation("Syncing tenant {TenantId}", tenantId);
                    var result = await syncService.SyncAllAsync(tenantId, syncType);

                    if (result.Status == "SUCCESS")
                    {
                        _logger.LogInformation("Tenant {TenantId} synced successfully. Companies: {Companies}, Branches: {Branches}, Departments: {Departments}, Sections: {Sections}, Employees: {Employees}",
                            tenantId,
                            result.CompaniesCreated + result.CompaniesUpdated,
                            result.BranchesCreated + result.BranchesUpdated,
                            result.DepartmentsCreated + result.DepartmentsUpdated,
                            result.SectionsCreated + result.SectionsUpdated,
                            result.EmployeesCreated + result.EmployeesUpdated);
                    }
                    else
                    {
                        _logger.LogError("Tenant {TenantId} sync failed: {Error}", tenantId, result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing tenant {TenantId}", tenantId);
                    // Continue with next tenant
                }
            }

            _logger.LogInformation("{SyncType} sync completed for all tenants", syncType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing sync for all tenants");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Oryggi sync background service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
