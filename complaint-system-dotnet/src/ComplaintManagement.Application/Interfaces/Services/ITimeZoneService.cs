namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for timezone conversions and business hours calculations
/// Supports IANA timezone identifiers (e.g., "Asia/Kolkata", "America/New_York")
/// </summary>
public interface ITimeZoneService
{
    /// <summary>
    /// Get effective timezone for an entity using hierarchy: User -> Branch -> Company -> UTC
    /// </summary>
    /// <param name="userTimeZone">User's preferred timezone (highest priority)</param>
    /// <param name="branchTimeZone">Branch timezone (second priority)</param>
    /// <param name="companyTimeZone">Company default timezone (third priority)</param>
    /// <returns>Effective timezone identifier (falls back to UTC if all null)</returns>
    string GetEffectiveTimeZone(
        string? userTimeZone,
        string? branchTimeZone,
        string? companyTimeZone);

    /// <summary>
    /// Convert UTC DateTime to target timezone
    /// </summary>
    /// <param name="utcDateTime">DateTime in UTC (must have Kind = Utc)</param>
    /// <param name="targetTimeZoneId">IANA timezone identifier (e.g., "Asia/Kolkata")</param>
    /// <returns>DateTime in target timezone</returns>
    /// <exception cref="ArgumentException">Thrown if utcDateTime is not UTC</exception>
    DateTime ConvertFromUtc(DateTime utcDateTime, string targetTimeZoneId);

    /// <summary>
    /// Convert local DateTime to UTC
    /// </summary>
    /// <param name="localDateTime">DateTime in source timezone</param>
    /// <param name="sourceTimeZoneId">IANA timezone identifier (e.g., "Asia/Kolkata")</param>
    /// <returns>DateTime in UTC</returns>
    DateTime ConvertToUtc(DateTime localDateTime, string sourceTimeZoneId);

    /// <summary>
    /// Get current time in target timezone
    /// </summary>
    /// <param name="timeZoneId">IANA timezone identifier</param>
    /// <returns>Current DateTime in target timezone</returns>
    DateTime GetCurrentTimeInTimeZone(string timeZoneId);

    /// <summary>
    /// Check if a datetime is within working hours in target timezone
    /// </summary>
    /// <param name="utcDateTime">DateTime in UTC</param>
    /// <param name="timeZoneId">Timezone to check working hours in</param>
    /// <param name="workStart">Start of working hours (e.g., 09:00)</param>
    /// <param name="workEnd">End of working hours (e.g., 17:00)</param>
    /// <returns>True if datetime falls within working hours</returns>
    bool IsWithinWorkingHours(
        DateTime utcDateTime,
        string timeZoneId,
        TimeSpan workStart,
        TimeSpan workEnd);

    /// <summary>
    /// Check if a date is a working day
    /// </summary>
    /// <param name="dateTime">Date to check</param>
    /// <param name="workingDays">Set of working days (e.g., Mon-Fri)</param>
    /// <returns>True if date is a working day</returns>
    bool IsWorkingDay(DateTime dateTime, HashSet<DayOfWeek> workingDays);
}
