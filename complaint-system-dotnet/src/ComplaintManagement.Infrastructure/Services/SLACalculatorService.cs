using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for calculating SLA deadlines based on complaint properties and SLA configurations
/// Priority order: Priority Mapping > Category Mapping > SLA Level > Priority Master (legacy) > Category Default (legacy)
/// </summary>
public class SLACalculatorService : ISLACalculatorService
{
    private readonly ComplaintDbContext _dbContext;
    private readonly ILogger<SLACalculatorService> _logger;
    private readonly ITimeZoneService _timeZoneService;
    private const int DEFAULT_SLA_HOURS = 48; // System default if no SLA configured

    public SLACalculatorService(
        ComplaintDbContext dbContext,
        ILogger<SLACalculatorService> logger,
        ITimeZoneService timeZoneService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _timeZoneService = timeZoneService;
    }

    public async Task<SLACalculationResult> CalculateSLADeadlineAsync(
        Guid categoryId,
        Guid? priorityMasterId,
        Guid companyId,
        DateTime startTime,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Calculating SLA for Category: {CategoryId}, Priority: {PriorityId}, Company: {CompanyId}, TimeZone: {TimeZone}",
            categoryId, priorityMasterId, companyId, timeZoneId ?? "UTC");

        // Step 1: Get SLA Settings to check if working hours should be applied
        var slaSettings = await _dbContext.SLASettings
            .Where(s => s.CompanyId == companyId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        bool workingHoursOnly = slaSettings?.WorkingHoursOnly ?? false;
        bool slaEnabled = slaSettings?.IsEnabled ?? false;

        // Get effective timezone (use provided timezone, or SLA settings timezone, or UTC)
        string effectiveTimeZone = timeZoneId ?? slaSettings?.Timezone ?? "UTC";

        if (!slaEnabled)
        {
            _logger.LogInformation("SLA is disabled for company {CompanyId}", companyId);
            return new SLACalculationResult
            {
                Source = SLASource.SystemDefault,
                Notes = "SLA system is disabled",
                WorkingHoursApplied = false
            };
        }

        // Step 2: Try Priority-SLA Mapping (highest priority)
        if (priorityMasterId.HasValue)
        {
            var priorityMapping = await _dbContext.PrioritySLAs
                .Include(p => p.SLALevel)
                .Where(p => p.PriorityId == priorityMasterId.Value
                    && p.SLALevel.CompanyId == companyId
                    && p.IsActive
                    && !p.IsDeleted
                    && !p.SLALevel.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (priorityMapping != null)
            {
                _logger.LogInformation("Using Priority-SLA mapping for priority {PriorityId}", priorityMasterId);
                return CalculateFromMapping(
                    priorityMapping.GetEffectiveResponseTimeMinutes(),
                    priorityMapping.GetEffectiveResolutionTimeMinutes(),
                    priorityMapping.SLALevelId,
                    priorityMapping.SLALevel.Name,
                    SLASource.PriorityMapping,
                    startTime,
                    workingHoursOnly,
                    slaSettings,
                    effectiveTimeZone);
            }
        }

        // Step 3: Try Category-SLA Mapping
        var categoryMapping = await _dbContext.CategorySLAs
            .Include(c => c.SLALevel)
            .Where(c => c.CategoryId == categoryId
                && c.SLALevel.CompanyId == companyId
                && c.IsActive
                && !c.IsDeleted
                && !c.SLALevel.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (categoryMapping != null)
        {
            _logger.LogInformation("Using Category-SLA mapping for category {CategoryId}", categoryId);
            return CalculateFromMapping(
                categoryMapping.GetEffectiveResponseTimeMinutes(),
                categoryMapping.GetEffectiveResolutionTimeMinutes(),
                categoryMapping.SLALevelId,
                categoryMapping.SLALevel.Name,
                SLASource.CategoryMapping,
                startTime,
                workingHoursOnly,
                slaSettings,
                effectiveTimeZone);
        }

        // Step 4: System default (fallback)
        // Note: Legacy fallbacks (Priority Master SLA fields and Category Default SLA Hours)
        // have been removed. All SLA configuration now uses the SLA Management system.
        _logger.LogWarning("No SLA configuration found, using system default: {Hours} hours", DEFAULT_SLA_HOURS);

        return CalculateFromMinutes(
            0,
            DEFAULT_SLA_HOURS * 60,
            SLASource.SystemDefault,
            startTime,
            workingHoursOnly,
            slaSettings,
            $"System default: {DEFAULT_SLA_HOURS}h",
            effectiveTimeZone);
    }

    private SLACalculationResult CalculateFromMapping(
        int responseMinutes,
        int resolutionMinutes,
        Guid slaLevelId,
        string slaLevelName,
        SLASource source,
        DateTime startTime,
        bool workingHoursOnly,
        Domain.Entities.SLA.SLASettings? settings,
        string timeZoneId)
    {
        return CalculateFromMinutes(
            responseMinutes,
            resolutionMinutes,
            source,
            startTime,
            workingHoursOnly,
            settings,
            $"SLA Level: {slaLevelName}",
            timeZoneId,
            slaLevelId,
            slaLevelName);
    }

    private SLACalculationResult CalculateFromMinutes(
        int responseMinutes,
        int resolutionMinutes,
        SLASource source,
        DateTime startTime,
        bool workingHoursOnly,
        Domain.Entities.SLA.SLASettings? settings,
        string notes,
        string timeZoneId,
        Guid? slaLevelId = null,
        string? slaLevelName = null)
    {
        DateTime? responseDeadline = null;
        DateTime? resolutionDeadline = null;

        if (responseMinutes > 0)
        {
            responseDeadline = workingHoursOnly
                ? CalculateWorkingHoursDeadline(startTime, responseMinutes, settings, timeZoneId)
                : startTime.AddMinutes(responseMinutes);
        }

        if (resolutionMinutes > 0)
        {
            resolutionDeadline = workingHoursOnly
                ? CalculateWorkingHoursDeadline(startTime, resolutionMinutes, settings, timeZoneId)
                : startTime.AddMinutes(resolutionMinutes);
        }

        // Primary deadline is resolution if available, otherwise response
        DateTime? primaryDeadline = resolutionDeadline ?? responseDeadline;

        return new SLACalculationResult
        {
            ResponseDeadline = responseDeadline,
            ResolutionDeadline = resolutionDeadline,
            PrimaryDeadline = primaryDeadline,
            SLALevelId = slaLevelId,
            SLALevelName = slaLevelName,
            ResponseTimeMinutes = responseMinutes,
            ResolutionTimeMinutes = resolutionMinutes,
            Source = source,
            WorkingHoursApplied = workingHoursOnly,
            Notes = notes
        };
    }

    private DateTime CalculateWorkingHoursDeadline(
        DateTime startTime,
        int minutes,
        Domain.Entities.SLA.SLASettings? settings,
        string timeZoneId)
    {
        // If no settings or working hours not configured, fall back to simple calculation
        if (settings == null ||
            !settings.WorkingHoursStart.HasValue ||
            !settings.WorkingHoursEnd.HasValue)
        {
            _logger.LogWarning("Working hours not configured, using simple calculation");
            return startTime.AddMinutes(minutes);
        }

        // Get working hours from TimeSpan properties
        var workStart = settings.WorkingHoursStart.Value;
        var workEnd = settings.WorkingHoursEnd.Value;

        // Parse working days (e.g., "1,2,3,4,5" for Mon-Fri)
        var workingDays = ParseWorkingDays(settings.WorkingDays);
        if (workingDays.Count == 0)
        {
            workingDays = new HashSet<DayOfWeek> {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday
            };
        }

        // CRITICAL FIX: Convert UTC to company/branch timezone first
        var localStartTime = _timeZoneService.ConvertFromUtc(startTime, timeZoneId);

        _logger.LogDebug(
            "Calculating working hours deadline: UTC Start={UtcStart}, Local Start={LocalStart}, TimeZone={TimeZone}, Minutes={Minutes}",
            startTime.ToString("yyyy-MM-dd HH:mm:ss"),
            localStartTime.ToString("yyyy-MM-dd HH:mm:ss"),
            timeZoneId,
            minutes);

        // Calculate deadline in local time
        var currentTime = localStartTime;
        var remainingMinutes = minutes;

        while (remainingMinutes > 0)
        {
            // Skip to next working day if current day is not a working day
            while (!workingDays.Contains(currentTime.DayOfWeek))
            {
                currentTime = currentTime.Date.AddDays(1).Add(workStart);
            }

            // If before working hours, move to start of working hours
            var currentTimeOfDay = currentTime.TimeOfDay;
            if (currentTimeOfDay < workStart)
            {
                currentTime = currentTime.Date.Add(workStart);
                currentTimeOfDay = workStart;
            }

            // If after working hours, move to next working day
            if (currentTimeOfDay >= workEnd)
            {
                currentTime = currentTime.Date.AddDays(1).Add(workStart);
                continue;
            }

            // Calculate minutes available in this working period
            var endOfDay = currentTime.Date.Add(workEnd);
            var minutesUntilEndOfDay = (int)(endOfDay - currentTime).TotalMinutes;

            if (remainingMinutes <= minutesUntilEndOfDay)
            {
                // Can finish within this working day
                currentTime = currentTime.AddMinutes(remainingMinutes);
                remainingMinutes = 0;
            }
            else
            {
                // Use up this working day and continue tomorrow
                remainingMinutes -= minutesUntilEndOfDay;
                currentTime = currentTime.Date.AddDays(1).Add(workStart);
            }
        }

        // CRITICAL FIX: Convert back to UTC before returning
        var utcDeadline = _timeZoneService.ConvertToUtc(currentTime, timeZoneId);

        _logger.LogDebug(
            "Working hours deadline calculated: Local Deadline={LocalDeadline}, UTC Deadline={UtcDeadline}",
            currentTime.ToString("yyyy-MM-dd HH:mm:ss"),
            utcDeadline.ToString("yyyy-MM-dd HH:mm:ss"));

        return utcDeadline;
    }

    private HashSet<DayOfWeek> ParseWorkingDays(string workingDaysString)
    {
        var workingDays = new HashSet<DayOfWeek>();

        if (string.IsNullOrEmpty(workingDaysString))
            return workingDays;

        var days = workingDaysString.Split(',');
        foreach (var day in days)
        {
            if (int.TryParse(day.Trim(), out int dayNum))
            {
                // Convert 0-6 to DayOfWeek (0=Sunday, 1=Monday, etc.)
                if (dayNum >= 0 && dayNum <= 6)
                {
                    workingDays.Add((DayOfWeek)dayNum);
                }
            }
        }

        return workingDays;
    }

    public bool IsSLABreached(DateTime? dueDate, DateTime? currentTime = null)
    {
        if (!dueDate.HasValue)
            return false;

        var now = currentTime ?? DateTime.UtcNow;
        return now > dueDate.Value;
    }

    public double GetTimeRemainingMinutes(DateTime? dueDate, DateTime? currentTime = null)
    {
        if (!dueDate.HasValue)
            return double.MaxValue;

        var now = currentTime ?? DateTime.UtcNow;
        return (dueDate.Value - now).TotalMinutes;
    }

    public double GetSLAPercentageComplete(DateTime startTime, DateTime? dueDate, DateTime? currentTime = null)
    {
        if (!dueDate.HasValue)
            return 0;

        var now = currentTime ?? DateTime.UtcNow;
        var totalMinutes = (dueDate.Value - startTime).TotalMinutes;
        var elapsedMinutes = (now - startTime).TotalMinutes;

        if (totalMinutes <= 0)
            return 100;

        return (elapsedMinutes / totalMinutes) * 100.0;
    }
}
