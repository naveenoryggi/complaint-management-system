using ComplaintManagement.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for timezone conversions and business hours calculations
/// Implements timezone-aware SLA calculations using IANA timezone identifiers
/// </summary>
public class TimeZoneService : ITimeZoneService
{
    private readonly ILogger<TimeZoneService> _logger;

    public TimeZoneService(ILogger<TimeZoneService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get effective timezone using hierarchy: User -> Branch -> Company -> UTC
    /// </summary>
    public string GetEffectiveTimeZone(
        string? userTimeZone,
        string? branchTimeZone,
        string? companyTimeZone)
    {
        var effectiveTimeZone = userTimeZone
            ?? branchTimeZone
            ?? companyTimeZone
            ?? "UTC";

        _logger.LogDebug(
            "Resolved timezone hierarchy: User={User}, Branch={Branch}, Company={Company} -> Effective={Effective}",
            userTimeZone ?? "null",
            branchTimeZone ?? "null",
            companyTimeZone ?? "null",
            effectiveTimeZone);

        return effectiveTimeZone;
    }

    /// <summary>
    /// Convert UTC DateTime to target timezone
    /// </summary>
    public DateTime ConvertFromUtc(DateTime utcDateTime, string targetTimeZoneId)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("DateTime must be UTC", nameof(utcDateTime));
        }

        try
        {
            var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(targetTimeZoneId);
            var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, targetTimeZone);

            _logger.LogDebug(
                "Converted UTC {UtcTime} to {TimeZone} = {LocalTime}",
                utcDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                targetTimeZoneId,
                convertedTime.ToString("yyyy-MM-dd HH:mm:ss"));

            return convertedTime;
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogWarning(ex,
                "Timezone {TimeZone} not found, falling back to UTC",
                targetTimeZoneId);
            return utcDateTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error converting from UTC to {TimeZone}, falling back to UTC",
                targetTimeZoneId);
            return utcDateTime;
        }
    }

    /// <summary>
    /// Convert local DateTime to UTC
    /// </summary>
    public DateTime ConvertToUtc(DateTime localDateTime, string sourceTimeZoneId)
    {
        try
        {
            var sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(sourceTimeZoneId);
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified),
                sourceTimeZone);

            _logger.LogDebug(
                "Converted {TimeZone} {LocalTime} to UTC = {UtcTime}",
                sourceTimeZoneId,
                localDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                utcTime.ToString("yyyy-MM-dd HH:mm:ss"));

            return utcTime;
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogWarning(ex,
                "Timezone {TimeZone} not found, treating as UTC",
                sourceTimeZoneId);
            return DateTime.SpecifyKind(localDateTime, DateTimeKind.Utc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error converting from {TimeZone} to UTC, treating as UTC",
                sourceTimeZoneId);
            return DateTime.SpecifyKind(localDateTime, DateTimeKind.Utc);
        }
    }

    /// <summary>
    /// Get current time in target timezone
    /// </summary>
    public DateTime GetCurrentTimeInTimeZone(string timeZoneId)
    {
        return ConvertFromUtc(DateTime.UtcNow, timeZoneId);
    }

    /// <summary>
    /// Check if a datetime is within working hours in target timezone
    /// </summary>
    public bool IsWithinWorkingHours(
        DateTime utcDateTime,
        string timeZoneId,
        TimeSpan workStart,
        TimeSpan workEnd)
    {
        var localTime = ConvertFromUtc(utcDateTime, timeZoneId);
        var isWithin = localTime.TimeOfDay >= workStart && localTime.TimeOfDay < workEnd;

        _logger.LogDebug(
            "Checking working hours: {UtcTime} UTC -> {LocalTime} {TimeZone}, Hours: {Start}-{End} -> Within: {Result}",
            utcDateTime.ToString("HH:mm:ss"),
            localTime.ToString("HH:mm:ss"),
            timeZoneId,
            workStart.ToString(@"hh\:mm"),
            workEnd.ToString(@"hh\:mm"),
            isWithin);

        return isWithin;
    }

    /// <summary>
    /// Check if a date is a working day
    /// </summary>
    public bool IsWorkingDay(DateTime dateTime, HashSet<DayOfWeek> workingDays)
    {
        return workingDays.Contains(dateTime.DayOfWeek);
    }
}
