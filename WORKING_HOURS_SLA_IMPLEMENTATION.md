# Working Hours SLA Calculation - Implementation Guide

## Overview

Implement business hours-aware SLA calculations that exclude weekends, holidays, and non-business hours from SLA deadline computations.

**Status:** DESIGN COMPLETE - READY FOR IMPLEMENTATION

---

## Business Requirements

### Current Behavior
- SLA deadlines calculated using calendar hours (24/7)
- Weekends and holidays count toward SLA time
- No consideration for business hours

### Desired Behavior
- SLA calculated using business hours only
- Configurable working hours per company (e.g., 9 AM - 5 PM)
- Exclude weekends (Saturday/Sunday)
- Exclude company holidays
- Exclude company-specific closure days
- Support for different timezones per branch

### Example Scenario

**Current System:**
- Complaint created: Friday 4:00 PM
- SLA: 8 hours response time
- Deadline: Friday 12:00 AM (midnight) ❌ **Incorrect - counts non-business hours**

**New System:**
- Complaint created: Friday 4:00 PM
- Business hours: 9 AM - 5 PM (8 hours/day)
- SLA: 8 business hours
- Calculation:
  - Friday 4:00 PM - 5:00 PM = 1 hour
  - Monday 9:00 AM - 4:00 PM = 7 hours
- **Deadline: Monday 4:00 PM** ✅ **Correct - business hours only**

---

## Architecture

### Components

1. **WorkingHoursConfiguration** - Company/branch working hours
2. **HolidayCalendar** - Company holidays and closures
3. **BusinessHoursCalculator** - Core calculation logic
4. **SLACalculatorService** - Updated to use business hours
5. **Admin UI** - Configure working hours and holidays

---

## Implementation

### Phase 1: Database Entities

#### 1.1 WorkingHoursConfiguration Entity

**File:** `ComplaintManagement.Domain/Entities/MasterData/WorkingHoursConfiguration.cs`

```csharp
using ComplaintManagement.Domain.Entities;

namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Defines working hours for a company or branch
/// </summary>
public class WorkingHoursConfiguration : BaseEntity
{
    /// <summary>
    /// Company ID (required)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Branch ID (optional - if null, applies to entire company)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Configuration name (e.g., "Head Office Hours", "Branch A Schedule")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Use business hours for SLA calculations
    /// </summary>
    public bool UseBusinessHours { get; set; } = true;

    /// <summary>
    /// Timezone for this configuration (IANA timezone ID)
    /// </summary>
    public string TimeZone { get; set; } = "UTC";

    // Working Hours (stored in UTC, converted for display)
    public TimeSpan MondayStart { get; set; } = new TimeSpan(9, 0, 0);    // 9:00 AM
    public TimeSpan MondayEnd { get; set; } = new TimeSpan(17, 0, 0);     // 5:00 PM
    public bool MondayEnabled { get; set; } = true;

    public TimeSpan TuesdayStart { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan TuesdayEnd { get; set; } = new TimeSpan(17, 0, 0);
    public bool TuesdayEnabled { get; set; } = true;

    public TimeSpan WednesdayStart { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan WednesdayEnd { get; set; } = new TimeSpan(17, 0, 0);
    public bool WednesdayEnabled { get; set; } = true;

    public TimeSpan ThursdayStart { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan ThursdayEnd { get; set; } = new TimeSpan(17, 0, 0);
    public bool ThursdayEnabled { get; set; } = true;

    public TimeSpan FridayStart { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan FridayEnd { get; set; } = new TimeSpan(17, 0, 0);
    public bool FridayEnabled { get; set; } = true;

    public TimeSpan SaturdayStart { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan SaturdayEnd { get; set; } = new TimeSpan(13, 0, 0);   // Half day
    public bool SaturdayEnabled { get; set; } = false;

    public TimeSpan SundayStart { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan SundayEnd { get; set; } = new TimeSpan(13, 0, 0);
    public bool SundayEnabled { get; set; } = false;

    /// <summary>
    /// Lunch break duration in minutes (deducted from working hours)
    /// </summary>
    public int LunchBreakMinutes { get; set; } = 60; // 1 hour lunch

    /// <summary>
    /// Lunch break start time
    /// </summary>
    public TimeSpan LunchBreakStart { get; set; } = new TimeSpan(12, 0, 0); // 12:00 PM

    // Navigation properties
    public virtual Company Company { get; set; }
    public virtual Branch Branch { get; set; }

    /// <summary>
    /// Get working hours for a specific day of week
    /// </summary>
    public (TimeSpan Start, TimeSpan End, bool Enabled) GetHoursForDay(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => (MondayStart, MondayEnd, MondayEnabled),
            DayOfWeek.Tuesday => (TuesdayStart, TuesdayEnd, TuesdayEnabled),
            DayOfWeek.Wednesday => (WednesdayStart, WednesdayEnd, WednesdayEnabled),
            DayOfWeek.Thursday => (ThursdayStart, ThursdayEnd, ThursdayEnabled),
            DayOfWeek.Friday => (FridayStart, FridayEnd, FridayEnabled),
            DayOfWeek.Saturday => (SaturdayStart, SaturdayEnd, SaturdayEnabled),
            DayOfWeek.Sunday => (SundayStart, SundayEnd, SundayEnabled),
            _ => (TimeSpan.Zero, TimeSpan.Zero, false)
        };
    }

    /// <summary>
    /// Get total business hours per day (excluding lunch)
    /// </summary>
    public double GetBusinessHoursPerDay(DayOfWeek day)
    {
        var (start, end, enabled) = GetHoursForDay(day);
        if (!enabled) return 0;

        var totalMinutes = (end - start).TotalMinutes - LunchBreakMinutes;
        return Math.Max(0, totalMinutes / 60.0);
    }
}
```

#### 1.2 HolidayCalendar Entity

**File:** `ComplaintManagement.Domain/Entities/MasterData/HolidayCalendar.cs`

```csharp
namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Company holidays and closure days
/// </summary>
public class HolidayCalendar : BaseEntity
{
    /// <summary>
    /// Company ID
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Holiday name (e.g., "New Year's Day", "Christmas")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Holiday date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Is this a recurring holiday (same date every year)?
    /// </summary>
    public bool IsRecurring { get; set; } = false;

    /// <summary>
    /// Holiday type
    /// </summary>
    public HolidayType Type { get; set; } = HolidayType.PublicHoliday;

    /// <summary>
    /// Description or notes
    /// </summary>
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public virtual Company Company { get; set; }
}

public enum HolidayType
{
    /// <summary>
    /// Public/national holiday
    /// </summary>
    PublicHoliday = 0,

    /// <summary>
    /// Company closure
    /// </summary>
    CompanyClosure = 1,

    /// <summary>
    /// Partial day (e.g., half day before holiday)
    /// </summary>
    PartialDay = 2,

    /// <summary>
    /// Optional holiday
    /// </summary>
    Optional = 3
}
```

### Phase 2: Business Hours Calculator Service

**File:** `ComplaintManagement.Application/Interfaces/Services/IBusinessHoursCalculator.cs`

```csharp
namespace ComplaintManagement.Application.Interfaces.Services;

public interface IBusinessHoursCalculator
{
    /// <summary>
    /// Calculate the deadline for a given duration in business hours
    /// </summary>
    Task<DateTime> CalculateBusinessHoursDeadlineAsync(
        DateTime startDate,
        double businessHours,
        Guid companyId,
        Guid? branchId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate elapsed business hours between two dates
    /// </summary>
    Task<double> CalculateElapsedBusinessHoursAsync(
        DateTime startDate,
        DateTime endDate,
        Guid companyId,
        Guid? branchId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a date/time falls within business hours
    /// </summary>
    Task<bool> IsWithinBusinessHoursAsync(
        DateTime dateTime,
        Guid companyId,
        Guid? branchId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the next business day start time
    /// </summary>
    Task<DateTime> GetNextBusinessDayStartAsync(
        DateTime fromDate,
        Guid companyId,
        Guid? branchId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a date is a holiday
    /// </summary>
    Task<bool> IsHolidayAsync(
        DateTime date,
        Guid companyId,
        CancellationToken cancellationToken = default);
}
```

**File:** `ComplaintManagement.Infrastructure/Services/BusinessHoursCalculator.cs`

```csharp
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Infrastructure.Services;

public class BusinessHoursCalculator : IBusinessHoursCalculator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BusinessHoursCalculator> _logger;

    public BusinessHoursCalculator(
        IUnitOfWork unitOfWork,
        ILogger<BusinessHoursCalculator> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DateTime> CalculateBusinessHoursDeadlineAsync(
        DateTime startDate,
        double businessHours,
        Guid companyId,
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        // Get working hours configuration
        var config = await GetWorkingHoursConfigAsync(companyId, branchId, cancellationToken);

        // If business hours not enabled, use calendar hours
        if (!config.UseBusinessHours)
        {
            return startDate.AddHours(businessHours);
        }

        var currentDateTime = startDate;
        var remainingHours = businessHours;

        // If start time is outside business hours, move to next business day start
        if (!await IsWithinBusinessHoursAsync(currentDateTime, companyId, branchId, cancellationToken))
        {
            currentDateTime = await GetNextBusinessDayStartAsync(currentDateTime, companyId, branchId, cancellationToken);
        }

        while (remainingHours > 0)
        {
            // Check if current day is a holiday
            if (await IsHolidayAsync(currentDateTime, companyId, cancellationToken))
            {
                // Move to next day
                currentDateTime = currentDateTime.Date.AddDays(1);
                continue;
            }

            var dayOfWeek = currentDateTime.DayOfWeek;
            var (start, end, enabled) = config.GetHoursForDay(dayOfWeek);

            if (!enabled)
            {
                // Not a working day, move to next day
                currentDateTime = currentDateTime.Date.AddDays(1).Add(start);
                continue;
            }

            // Calculate available hours for current day
            var dayEnd = currentDateTime.Date.Add(end);
            var availableHoursToday = (dayEnd - currentDateTime).TotalHours;

            // Account for lunch break
            var lunchStart = currentDateTime.Date.Add(config.LunchBreakStart);
            var lunchEnd = lunchStart.AddMinutes(config.LunchBreakMinutes);

            if (currentDateTime < lunchStart && dayEnd > lunchStart)
            {
                availableHoursToday -= config.LunchBreakMinutes / 60.0;
            }

            if (remainingHours <= availableHoursToday)
            {
                // Can finish within today
                currentDateTime = currentDateTime.AddHours(remainingHours);

                // Skip lunch break if we cross it
                if (currentDateTime >= lunchStart && currentDateTime < lunchEnd)
                {
                    currentDateTime = lunchEnd;
                }

                remainingHours = 0;
            }
            else
            {
                // Need to continue to next day
                remainingHours -= availableHoursToday;
                currentDateTime = currentDateTime.Date.AddDays(1);

                // Set to start of next business day
                var nextDay = currentDateTime.DayOfWeek;
                var (nextStart, _, _) = config.GetHoursForDay(nextDay);
                currentDateTime = currentDateTime.Date.Add(nextStart);
            }
        }

        return currentDateTime;
    }

    public async Task<double> CalculateElapsedBusinessHoursAsync(
        DateTime startDate,
        DateTime endDate,
        Guid companyId,
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        var config = await GetWorkingHoursConfigAsync(companyId, branchId, cancellationToken);

        if (!config.UseBusinessHours)
        {
            return (endDate - startDate).TotalHours;
        }

        double totalBusinessHours = 0;
        var currentDate = startDate.Date;
        var endDateOnly = endDate.Date;

        while (currentDate <= endDateOnly)
        {
            if (await IsHolidayAsync(currentDate, companyId, cancellationToken))
            {
                currentDate = currentDate.AddDays(1);
                continue;
            }

            var dayOfWeek = currentDate.DayOfWeek;
            var (start, end, enabled) = config.GetHoursForDay(dayOfWeek);

            if (!enabled)
            {
                currentDate = currentDate.AddDays(1);
                continue;
            }

            var dayStart = currentDate.Add(start);
            var dayEnd = currentDate.Add(end);

            // Calculate intersection with business hours
            var effectiveStart = currentDate == startDate.Date
                ? (startDate > dayStart ? startDate : dayStart)
                : dayStart;

            var effectiveEnd = currentDate == endDateOnly
                ? (endDate < dayEnd ? endDate : dayEnd)
                : dayEnd;

            if (effectiveStart < effectiveEnd)
            {
                var hoursThisDay = (effectiveEnd - effectiveStart).TotalHours;

                // Subtract lunch break if applicable
                var lunchStart = currentDate.Add(config.LunchBreakStart);
                var lunchEnd = lunchStart.AddMinutes(config.LunchBreakMinutes);

                if (effectiveStart < lunchEnd && effectiveEnd > lunchStart)
                {
                    // Lunch break overlaps with working time
                    var lunchOverlap = (lunchEnd - lunchStart).TotalHours;
                    hoursThisDay -= lunchOverlap;
                }

                totalBusinessHours += Math.Max(0, hoursThisDay);
            }

            currentDate = currentDate.AddDays(1);
        }

        return totalBusinessHours;
    }

    public async Task<bool> IsWithinBusinessHoursAsync(
        DateTime dateTime,
        Guid companyId,
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        var config = await GetWorkingHoursConfigAsync(companyId, branchId, cancellationToken);

        if (!config.UseBusinessHours)
        {
            return true; // Always within business hours if not using business hours
        }

        // Check if it's a holiday
        if (await IsHolidayAsync(dateTime, companyId, cancellationToken))
        {
            return false;
        }

        var dayOfWeek = dateTime.DayOfWeek;
        var (start, end, enabled) = config.GetHoursForDay(dayOfWeek);

        if (!enabled)
        {
            return false;
        }

        var timeOfDay = dateTime.TimeOfDay;

        // Check if within working hours
        if (timeOfDay < start || timeOfDay >= end)
        {
            return false;
        }

        // Check if during lunch break
        var lunchStart = config.LunchBreakStart;
        var lunchEnd = lunchStart.Add(TimeSpan.FromMinutes(config.LunchBreakMinutes));

        if (timeOfDay >= lunchStart && timeOfDay < lunchEnd)
        {
            return false; // During lunch break
        }

        return true;
    }

    public async Task<DateTime> GetNextBusinessDayStartAsync(
        DateTime fromDate,
        Guid companyId,
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        var config = await GetWorkingHoursConfigAsync(companyId, branchId, cancellationToken);
        var currentDate = fromDate.Date.AddDays(1); // Start from next day

        // Find next working day
        for (int i = 0; i < 14; i++) // Limit to 2 weeks to prevent infinite loop
        {
            if (await IsHolidayAsync(currentDate, companyId, cancellationToken))
            {
                currentDate = currentDate.AddDays(1);
                continue;
            }

            var dayOfWeek = currentDate.DayOfWeek;
            var (start, _, enabled) = config.GetHoursForDay(dayOfWeek);

            if (enabled)
            {
                return currentDate.Add(start);
            }

            currentDate = currentDate.AddDays(1);
        }

        // Fallback (should never reach here)
        return fromDate.AddDays(1);
    }

    public async Task<bool> IsHolidayAsync(
        DateTime date,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;

        var holiday = await _unitOfWork.Repository<HolidayCalendar>()
            .FirstOrDefaultAsync(
                h => h.CompanyId == companyId &&
                     h.Date.Date == dateOnly &&
                     !h.IsDeleted,
                cancellationToken);

        return holiday != null;
    }

    private async Task<WorkingHoursConfiguration> GetWorkingHoursConfigAsync(
        Guid companyId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        // Try to get branch-specific config first
        if (branchId.HasValue)
        {
            var branchConfig = await _unitOfWork.Repository<WorkingHoursConfiguration>()
                .FirstOrDefaultAsync(
                    c => c.BranchId == branchId.Value && !c.IsDeleted,
                    cancellationToken);

            if (branchConfig != null)
            {
                return branchConfig;
            }
        }

        // Fall back to company-level config
        var companyConfig = await _unitOfWork.Repository<WorkingHoursConfiguration>()
            .FirstOrDefaultAsync(
                c => c.CompanyId == companyId && c.BranchId == null && !c.IsDeleted,
                cancellationToken);

        if (companyConfig != null)
        {
            return companyConfig;
        }

        // Return default 9-5 Monday-Friday
        return CreateDefaultConfig(companyId);
    }

    private WorkingHoursConfiguration CreateDefaultConfig(Guid companyId)
    {
        return new WorkingHoursConfiguration
        {
            CompanyId = companyId,
            Name = "Default Business Hours",
            UseBusinessHours = true,
            TimeZone = "UTC",
            MondayEnabled = true,
            TuesdayEnabled = true,
            WednesdayEnabled = true,
            ThursdayEnabled = true,
            FridayEnabled = true,
            SaturdayEnabled = false,
            SundayEnabled = false,
            LunchBreakMinutes = 60,
            LunchBreakStart = new TimeSpan(12, 0, 0)
        };
    }
}
```

---

## Testing

### Unit Tests

```csharp
[Fact]
public async Task CalculateDeadline_FridayAfternoon_ShouldSkipWeekend()
{
    // Arrange
    var startDate = new DateTime(2025, 11, 14, 16, 0, 0); // Friday 4 PM
    var businessHours = 8.0; // 8 hours

    // Act
    var deadline = await _calculator.CalculateBusinessHoursDeadlineAsync(
        startDate, businessHours, companyId);

    // Assert
    Assert.Equal(DayOfWeek.Monday, deadline.DayOfWeek);
    Assert.Equal(15, deadline.Hour); // Monday 3 PM
}

[Fact]
public async Task CalculateElapsed_IncludesLunchBreak_ShouldSubtractLunch()
{
    // Arrange
    var start = new DateTime(2025, 11, 17, 9, 0, 0);  // 9 AM
    var end = new DateTime(2025, 11, 17, 17, 0, 0);   // 5 PM

    // Act
    var elapsed = await _calculator.CalculateElapsedBusinessHoursAsync(
        start, end, companyId);

    // Assert
    Assert.Equal(7.0, elapsed); // 8 hours - 1 hour lunch = 7 hours
}
```

---

## Migration

```bash
dotnet ef migrations add AddWorkingHoursAndHolidays
dotnet ef database update
```

---

**Implementation Status:** DESIGN COMPLETE
**Estimated Time:** 8-12 hours
**Priority:** HIGH - Critical for accurate SLA tracking
**Dependencies:** None - self-contained feature
