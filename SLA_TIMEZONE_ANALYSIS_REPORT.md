# SLA Timezone Awareness Analysis Report

**Date:** 2025-11-15
**Analyzed By:** Claude Code
**System:** Complaint Management System - SLA Calculator Service

---

## Executive Summary

**CRITICAL FINDINGS:**
1. **SLA deadlines are calculated in UTC without timezone awareness** - MAJOR BUG
2. **Business hours calculations ignore company/branch timezone** - CRITICAL ISSUE
3. **Auto-escalation triggers use UTC time comparisons** - INCORRECT
4. **No timezone conversion layer exists** - ARCHITECTURAL GAP

**Severity:** HIGH - This affects all SLA calculations and can result in incorrect deadlines

---

## 1. Current Implementation Analysis

### 1.1 SLA Calculator Service (`SLACalculatorService.cs`)

**Location:** `C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.Infrastructure\Services\SLACalculatorService.cs`

#### Key Methods Analyzed:

**`CalculateSLADeadlineAsync()` - Lines 27-121**
```csharp
public async Task<SLACalculationResult> CalculateSLADeadlineAsync(
    Guid categoryId,
    Guid? priorityMasterId,
    Guid companyId,
    DateTime startTime,  // <-- Always UTC, no timezone context
    CancellationToken cancellationToken = default)
{
    // ...
    bool workingHoursOnly = slaSettings?.WorkingHoursOnly ?? false;
    // ...
}
```

**ISSUE 1: `startTime` parameter is always in UTC**
- Called from `CreateComplaintCommandHandler.cs:69-75` with `DateTime.UtcNow`
- No timezone context is passed or considered
- Result: Deadlines calculated in UTC regardless of user's location

---

**`CalculateWorkingHoursDeadline()` - Lines 191-265**
```csharp
private DateTime CalculateWorkingHoursDeadline(
    DateTime startTime,  // <-- UTC DateTime
    int minutes,
    Domain.Entities.SLA.SLASettings? settings)
{
    // Get working hours from TimeSpan properties
    var workStart = settings.WorkingHoursStart.Value;  // e.g., 09:00
    var workEnd = settings.WorkingHoursEnd.Value;      // e.g., 17:00

    // Calculate deadline considering working hours
    var currentTime = startTime;  // <-- Still in UTC!
    // ... business hours logic ...
}
```

**ISSUE 2: Business hours calculations use UTC times**

**Example Bug Scenario:**
```
Company: Mumbai office (UTC+5:30)
Business Hours: 9 AM - 5 PM (Mumbai time)
SLA Settings.WorkingHoursStart: 09:00 (stored as TimeSpan)
SLA Settings.WorkingHoursEnd: 17:00 (stored as TimeSpan)

Complaint submitted at: 4:30 PM Mumbai time
  → Stored as: 11:00 AM UTC (SubmittedAt field)

Bug: SLA calculator compares:
  - 11:00 AM UTC (currentTime.TimeOfDay)
  - Against: 09:00 - 17:00 (business hours)
  - Result: Thinks it's 11 AM (working hours) when it's actually 4:30 PM Mumbai time!

Expected: Should convert UTC to Mumbai time first:
  - 11:00 AM UTC → 4:30 PM Asia/Kolkata
  - Check: Is 4:30 PM within 9 AM - 5 PM? YES
  - Calculate remaining working hours for today
```

---

### 1.2 Auto-Escalation Service (`AutoEscalationService.cs`)

**Location:** `C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.Infrastructure\Services\AutoEscalationService.cs`

**`CalculateHoursSinceLastAction()` - Lines 227-250**
```csharp
private double CalculateHoursSinceLastAction(
    Domain.Entities.Complaints.Complaint complaint,
    int? currentLevel)
{
    DateTime referenceTime;

    if (currentLevel == null)
    {
        referenceTime = complaint.CreatedAt;  // <-- UTC
    }
    else
    {
        // Calculate from last escalation
        referenceTime = lastEscalation?.EscalatedAt ?? complaint.CreatedAt;  // <-- UTC
    }

    var hoursSince = (DateTime.UtcNow - referenceTime).TotalHours;  // <-- UTC comparison
    return hoursSince;
}
```

**ISSUE 3: Auto-escalation time calculations ignore business hours and timezone**
- Calculates elapsed hours in UTC (calendar time)
- Does NOT respect "working hours only" setting
- Example: If complaint created Friday 4 PM and escalation trigger is 4 hours:
  - Current logic: Triggers Monday 8 AM UTC (incorrect - includes weekend)
  - Expected: Should trigger Monday 12 PM company-time (4 working hours from Friday 4 PM)

---

### 1.3 Escalation Service (`EscalationService.cs`)

**Location:** `C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.Infrastructure\Services\EscalationService.cs`

**`EscalateComplaintAsync()` - Lines 311-414**
```csharp
public async Task<EscalationHistory> EscalateComplaintAsync(...)
{
    // Calculate SLA breach info
    int? hoursOverdue = null;
    if (complaint.DueDate.HasValue && DateTime.UtcNow > complaint.DueDate.Value)
    {
        hoursOverdue = (int)(DateTime.UtcNow - complaint.DueDate.Value).TotalHours;  // <-- UTC
    }
    // ...
}
```

**ISSUE 4: Escalation breach calculations use UTC**
- Compares `DateTime.UtcNow` against `complaint.DueDate` (also UTC)
- While technically correct for UTC, doesn't account for display/notification purposes
- Users expect "2 hours overdue" to mean "2 hours past 5 PM company time", not UTC

---

## 2. Data Model Analysis

### 2.1 Timezone Fields Available

**Company Entity** (`Company.cs:88`)
```csharp
/// <summary>
/// Default timezone for the company (IANA timezone identifier)
/// Used as fallback when user or branch timezone is not set
/// Default: "Asia/Kolkata"
/// </summary>
public string DefaultTimeZone { get; set; } = "Asia/Kolkata";
```

**Branch Entity** (`Branch.cs:79-84`)
```csharp
/// <summary>
/// Branch-specific timezone (IANA timezone identifier)
/// Overrides company default timezone for this branch location
/// e.g., "America/New_York" for NY branch, "Europe/London" for London branch
/// If null, uses Company.DefaultTimeZone
/// </summary>
public string? TimeZone { get; set; }
```

**User Entity** (`User.cs:223-227`)
```csharp
/// <summary>
/// User's preferred timezone (IANA timezone identifier)
/// e.g., "America/New_York", "Europe/London", "Asia/Tokyo"
/// If null, falls back to Branch timezone, then Company default
/// </summary>
public string? PreferredTimeZone { get; set; }
```

**SLASettings Entity** (`SLASettings.cs:69-71`)
```csharp
/// <summary>
/// Timezone for SLA calculations (e.g., "UTC", "America/New_York")
/// </summary>
public string Timezone { get; set; } = "UTC";
```

**FINDING:** Timezone fields exist but are NOT being used in SLA calculations!

---

### 2.2 Timezone Resolution Hierarchy

**Expected Behavior:**
1. User.PreferredTimeZone (highest priority)
2. Branch.TimeZone (if user timezone not set)
3. Company.DefaultTimeZone (if branch timezone not set)
4. "UTC" (system fallback)

**Current Behavior:**
- SLA calculations always use UTC
- No timezone resolution logic exists

---

## 3. Critical Bugs Identified

### Bug #1: Business Hours Calculated in Wrong Timezone

**Severity:** CRITICAL
**Impact:** SLA deadlines incorrectly calculated for companies outside UTC

**Current Code Flow:**
```csharp
// CreateComplaintCommandHandler.cs:69
var submittedAt = DateTime.UtcNow;  // e.g., 2025-11-15 11:00:00 UTC

// SLACalculatorService.cs:70-75
var slaResult = await _slaCalculator.CalculateSLADeadlineAsync(
    request.CategoryId,
    priorityMasterId,
    request.CompanyId,
    submittedAt,  // <-- UTC time passed to calculator
    cancellationToken);

// SLACalculatorService.cs:161-163 (if workingHoursOnly = true)
responseDeadline = CalculateWorkingHoursDeadline(
    startTime,  // <-- Still UTC: 11:00:00 UTC
    responseMinutes,
    settings);

// SLACalculatorService.cs:232-244
var currentTimeOfDay = currentTime.TimeOfDay;  // <-- 11:00 (UTC)
if (currentTimeOfDay < workStart)  // <-- Comparing UTC 11:00 vs 09:00 (company time)
{
    currentTime = currentTime.Date.Add(workStart);
}
```

**Example Test Case:**
```
Input:
  - Company: Mumbai (Asia/Kolkata, UTC+5:30)
  - Business Hours: 9 AM - 5 PM (Mumbai time)
  - SLA: 4 business hours
  - Complaint submitted: 2025-11-15 16:30 IST (4:30 PM Mumbai)

Current Calculation (WRONG):
  - submittedAt = 2025-11-15 11:00 UTC
  - currentTimeOfDay = 11:00
  - Is 11:00 within 9:00-17:00? YES
  - Remaining today: 17:00 - 11:00 = 6 hours
  - Since SLA is 4 hours, deadline = 2025-11-15 15:00 UTC (8:30 PM Mumbai) ❌

Correct Calculation:
  - submittedAt = 2025-11-15 11:00 UTC
  - Convert to Mumbai time: 2025-11-15 16:30 IST
  - currentTimeOfDay = 16:30 (4:30 PM)
  - Is 16:30 within 9:00-17:00? YES
  - Remaining today: 17:00 - 16:30 = 0.5 hours (30 minutes)
  - Remaining SLA: 4 hours - 0.5 hours = 3.5 hours
  - Next working day: 2025-11-16 09:00 IST
  - Deadline: 2025-11-16 12:30 IST (7:00 UTC) ✓
```

---

### Bug #2: Auto-Escalation Doesn't Respect Working Hours

**Severity:** HIGH
**Impact:** Complaints escalate during non-working hours/weekends

**Current Code:**
```csharp
// AutoEscalationService.cs:248
var hoursSince = (DateTime.UtcNow - referenceTime).TotalHours;
```

**Problem:** Calculates elapsed calendar hours, not working hours

**Example:**
```
Input:
  - SLA Settings: WorkingHoursOnly = true
  - Business Hours: Mon-Fri 9 AM - 5 PM
  - Escalation Trigger: 4 hours
  - Complaint created: Friday 4:00 PM

Current Behavior (WRONG):
  - Friday 4:00 PM + 4 hours = Friday 8:00 PM
  - Auto-escalation triggers Friday 8:00 PM ❌

Expected Behavior:
  - Friday 4:00 PM + 1 hour (to EOD) = Friday 5:00 PM
  - Remaining: 3 hours
  - Next working day: Monday 9:00 AM
  - Monday 9:00 AM + 3 hours = Monday 12:00 PM
  - Auto-escalation triggers Monday 12:00 PM ✓
```

---

### Bug #3: SLA Settings Timezone Field Ignored

**Severity:** MEDIUM
**Impact:** Confusing configuration - timezone field exists but not used

**Current State:**
```csharp
// SLASettings.cs:71
public string Timezone { get; set; } = "UTC";
```

**Issue:** This field is stored but never referenced in calculations

---

## 4. Recommended Fixes

### 4.1 Immediate Fix - Add Timezone-Aware SLA Calculator

**Step 1: Create Timezone Service**

**File:** `C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.Infrastructure\Services\TimeZoneService.cs`

```csharp
using ComplaintManagement.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for timezone conversions and business hours calculations
/// </summary>
public class TimeZoneService : ITimeZoneService
{
    private readonly ILogger<TimeZoneService> _logger;

    public TimeZoneService(ILogger<TimeZoneService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get effective timezone for an entity (User → Branch → Company → UTC)
    /// </summary>
    public string GetEffectiveTimeZone(
        string? userTimeZone,
        string? branchTimeZone,
        string? companyTimeZone)
    {
        return userTimeZone
            ?? branchTimeZone
            ?? companyTimeZone
            ?? "UTC";
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
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, targetTimeZone);
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogWarning(ex,
                "Timezone {TimeZone} not found, falling back to UTC",
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
            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified),
                sourceTimeZone);
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogWarning(ex,
                "Timezone {TimeZone} not found, treating as UTC",
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
        return localTime.TimeOfDay >= workStart && localTime.TimeOfDay < workEnd;
    }

    /// <summary>
    /// Check if a date is a working day
    /// </summary>
    public bool IsWorkingDay(DateTime dateTime, HashSet<DayOfWeek> workingDays)
    {
        return workingDays.Contains(dateTime.DayOfWeek);
    }
}
```

---

**Step 2: Update SLA Calculator Service**

**File:** `SLACalculatorService.cs`

**Changes Required:**

```csharp
// Add timezone service dependency
private readonly ITimeZoneService _timeZoneService;

public SLACalculatorService(
    ComplaintDbContext dbContext,
    ILogger<SLACalculatorService> logger,
    ITimeZoneService timeZoneService)  // <-- ADD THIS
{
    _dbContext = dbContext;
    _logger = logger;
    _timeZoneService = timeZoneService;  // <-- ADD THIS
}

// Update method signature to accept timezone
public async Task<SLACalculationResult> CalculateSLADeadlineAsync(
    Guid categoryId,
    Guid? priorityMasterId,
    Guid companyId,
    DateTime startTime,
    string? timeZoneId = null,  // <-- ADD THIS
    CancellationToken cancellationToken = default)
{
    // ... existing code ...

    // Get effective timezone
    string effectiveTimeZone = timeZoneId ?? slaSettings?.Timezone ?? "UTC";

    // ... rest of method ...

    return CalculateFromMinutes(
        responseMinutes,
        resolutionMinutes,
        source,
        startTime,
        workingHoursOnly,
        settings,
        notes,
        effectiveTimeZone);  // <-- PASS TIMEZONE
}

// Update CalculateWorkingHoursDeadline
private DateTime CalculateWorkingHoursDeadline(
    DateTime startTime,
    int minutes,
    Domain.Entities.SLA.SLASettings? settings,
    string timeZoneId)  // <-- ADD THIS
{
    // CRITICAL FIX: Convert UTC to company timezone first
    var localStartTime = _timeZoneService.ConvertFromUtc(startTime, timeZoneId);

    // Get working hours
    var workStart = settings.WorkingHoursStart.Value;
    var workEnd = settings.WorkingHoursEnd.Value;
    var workingDays = ParseWorkingDays(settings.WorkingDays);

    // Calculate in local time
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
            currentTime = currentTime.AddMinutes(remainingMinutes);
            remainingMinutes = 0;
        }
        else
        {
            remainingMinutes -= minutesUntilEndOfDay;
            currentTime = currentTime.Date.AddDays(1).Add(workStart);
        }
    }

    // CRITICAL FIX: Convert back to UTC before returning
    return _timeZoneService.ConvertToUtc(currentTime, timeZoneId);
}
```

---

**Step 3: Update CreateComplaintCommandHandler**

```csharp
// In CreateComplaintCommandHandler.cs

// Get timezone from user → branch → company hierarchy
var user = await _unitOfWork.Users.GetByIdAsync(
    request.ComplainantId,
    cancellationToken);

var branch = user.BranchId.HasValue
    ? await _unitOfWork.Branches.GetByIdAsync(user.BranchId.Value, cancellationToken)
    : null;

var company = await _unitOfWork.Companies.GetByIdAsync(
    request.CompanyId,
    cancellationToken);

string effectiveTimeZone = user.PreferredTimeZone
    ?? branch?.TimeZone
    ?? company.DefaultTimeZone
    ?? "UTC";

// Calculate SLA with timezone awareness
var submittedAt = DateTime.UtcNow;
var slaResult = await _slaCalculator.CalculateSLADeadlineAsync(
    request.CategoryId,
    priorityMasterId,
    request.CompanyId,
    submittedAt,
    effectiveTimeZone,  // <-- PASS TIMEZONE
    cancellationToken);
```

---

**Step 4: Update AutoEscalationService**

```csharp
// In AutoEscalationService.cs

// Add timezone-aware hours calculation
private async Task<double> CalculateWorkingHoursSinceLastAction(
    Domain.Entities.Complaints.Complaint complaint,
    int? currentLevel)
{
    DateTime referenceTime = currentLevel == null
        ? complaint.CreatedAt
        : (await GetLastEscalationTime(complaint.Id)) ?? complaint.CreatedAt;

    // Get SLA settings for the company
    var slaSettings = await _dbContext.SLASettings
        .Where(s => s.CompanyId == complaint.CompanyId && !s.IsDeleted)
        .FirstOrDefaultAsync();

    if (slaSettings?.WorkingHoursOnly != true)
    {
        // Simple calendar hours
        return (DateTime.UtcNow - referenceTime).TotalHours;
    }

    // Get timezone
    var company = await _dbContext.Companies.FindAsync(complaint.CompanyId);
    string timeZone = company?.DefaultTimeZone ?? "UTC";

    // Calculate working hours elapsed
    return CalculateWorkingHoursElapsed(
        referenceTime,
        DateTime.UtcNow,
        slaSettings,
        timeZone);
}

private double CalculateWorkingHoursElapsed(
    DateTime startUtc,
    DateTime endUtc,
    SLASettings settings,
    string timeZoneId)
{
    // Convert to local time
    var startLocal = _timeZoneService.ConvertFromUtc(startUtc, timeZoneId);
    var endLocal = _timeZoneService.ConvertFromUtc(endUtc, timeZoneId);

    var workStart = settings.WorkingHoursStart.Value;
    var workEnd = settings.WorkingHoursEnd.Value;
    var workingDays = ParseWorkingDays(settings.WorkingDays);

    double totalMinutes = 0;
    var currentDay = startLocal.Date;

    while (currentDay <= endLocal.Date)
    {
        if (workingDays.Contains(currentDay.DayOfWeek))
        {
            var dayStart = currentDay.Add(workStart);
            var dayEnd = currentDay.Add(workEnd);

            var periodStart = currentDay == startLocal.Date
                ? (startLocal > dayStart ? startLocal : dayStart)
                : dayStart;

            var periodEnd = currentDay == endLocal.Date
                ? (endLocal < dayEnd ? endLocal : dayEnd)
                : dayEnd;

            if (periodEnd > periodStart)
            {
                totalMinutes += (periodEnd - periodStart).TotalMinutes;
            }
        }

        currentDay = currentDay.AddDays(1);
    }

    return totalMinutes / 60.0;
}
```

---

### 4.2 Interface Updates

**File:** `ISLACalculatorService.cs`

```csharp
Task<SLACalculationResult> CalculateSLADeadlineAsync(
    Guid categoryId,
    Guid? priorityMasterId,
    Guid companyId,
    DateTime startTime,
    string? timeZoneId = null,  // <-- ADD THIS
    CancellationToken cancellationToken = default);
```

**File:** `ITimeZoneService.cs` (NEW)

```csharp
namespace ComplaintManagement.Application.Interfaces.Services;

public interface ITimeZoneService
{
    string GetEffectiveTimeZone(
        string? userTimeZone,
        string? branchTimeZone,
        string? companyTimeZone);

    DateTime ConvertFromUtc(DateTime utcDateTime, string targetTimeZoneId);
    DateTime ConvertToUtc(DateTime localDateTime, string sourceTimeZoneId);
    DateTime GetCurrentTimeInTimeZone(string timeZoneId);

    bool IsWithinWorkingHours(
        DateTime utcDateTime,
        string timeZoneId,
        TimeSpan workStart,
        TimeSpan workEnd);

    bool IsWorkingDay(DateTime dateTime, HashSet<DayOfWeek> workingDays);
}
```

---

### 4.3 Dependency Injection Registration

**File:** `DependencyInjection.cs`

```csharp
// Add to service registration
services.AddScoped<ITimeZoneService, TimeZoneService>();
```

---

## 5. Testing Strategy

### 5.1 Unit Tests Required

**Test 1: UTC to Local Timezone Conversion**
```csharp
[Fact]
public void ConvertFromUtc_MumbaiTimezone_ConvertsCorrectly()
{
    // Arrange
    var utcTime = new DateTime(2025, 11, 15, 11, 0, 0, DateTimeKind.Utc);
    var timeZoneId = "Asia/Kolkata";

    // Act
    var localTime = _timeZoneService.ConvertFromUtc(utcTime, timeZoneId);

    // Assert
    Assert.Equal(16, localTime.Hour);  // 11:00 UTC + 5:30 = 16:30 IST
    Assert.Equal(30, localTime.Minute);
}
```

**Test 2: Business Hours Calculation in Timezone**
```csharp
[Fact]
public async Task CalculateSLADeadline_4HoursFrom430PM_NextDayNoon()
{
    // Arrange
    var submittedAt = new DateTime(2025, 11, 15, 11, 0, 0, DateTimeKind.Utc);
    // 4:30 PM Mumbai
    var categoryId = Guid.NewGuid();
    var companyId = Guid.NewGuid();
    var timeZone = "Asia/Kolkata";

    // SLA Settings: 9 AM - 5 PM, Monday-Friday
    var slaSettings = new SLASettings
    {
        WorkingHoursOnly = true,
        WorkingHoursStart = new TimeSpan(9, 0, 0),
        WorkingHoursEnd = new TimeSpan(17, 0, 0),
        WorkingDays = "1,2,3,4,5",
        CompanyId = companyId
    };

    // Act
    var result = await _slaCalculator.CalculateSLADeadlineAsync(
        categoryId,
        null,
        companyId,
        submittedAt,
        timeZone);

    // Assert
    // 4:30 PM + 0:30 (remaining today) = 5:00 PM (Friday EOD)
    // Next working day: Monday 9:00 AM
    // Monday 9:00 AM + 3:30 = Monday 12:30 PM
    var expectedDeadlineLocal = new DateTime(2025, 11, 18, 12, 30, 0);
    // Monday 12:30 PM Mumbai
    var expectedDeadlineUtc = new DateTime(2025, 11, 18, 7, 0, 0, DateTimeKind.Utc);
    // Monday 7:00 AM UTC

    Assert.Equal(expectedDeadlineUtc, result.PrimaryDeadline);
}
```

**Test 3: Auto-Escalation Working Hours**
```csharp
[Fact]
public async Task CalculateWorkingHoursSinceLastAction_WeekendExcluded()
{
    // Arrange
    var fridayAfternoon = new DateTime(2025, 11, 14, 11, 0, 0, DateTimeKind.Utc);
    // Friday 4:30 PM Mumbai
    var mondayMorning = new DateTime(2025, 11, 17, 4, 0, 0, DateTimeKind.Utc);
    // Monday 9:30 AM Mumbai

    // Act
    var hoursElapsed = CalculateWorkingHoursElapsed(
        fridayAfternoon,
        mondayMorning,
        slaSettings,
        "Asia/Kolkata");

    // Assert
    // Friday 4:30 PM - 5:00 PM = 0.5 hours
    // Saturday-Sunday = 0 hours (weekend)
    // Monday 9:00 AM - 9:30 AM = 0.5 hours
    // Total = 1 hour
    Assert.Equal(1.0, hoursElapsed, 0.01);
}
```

---

### 5.2 Integration Tests Required

**Test Scenario 1: End-to-End Complaint Creation**
```
1. Create company with timezone: Asia/Kolkata
2. Create SLA settings: 4 business hours, 9 AM - 5 PM, Mon-Fri, WorkingHoursOnly = true
3. Submit complaint at 4:30 PM Mumbai time (Friday)
4. Verify DueDate = Monday 12:30 PM Mumbai time (converted to UTC in database)
```

**Test Scenario 2: Auto-Escalation Across Timezone**
```
1. Create complaint Friday 4:30 PM Mumbai
2. Set escalation trigger: 4 working hours
3. Wait until Monday 12:30 PM Mumbai
4. Verify auto-escalation triggers
5. Verify auto-escalation does NOT trigger during weekend
```

---

## 6. Impact Assessment

### 6.1 Systems Affected
- ✅ SLA Calculator Service
- ✅ Auto-Escalation Service
- ✅ Escalation Service
- ✅ Complaint Creation Handler
- ⚠️ Dashboard Statistics (displays may need timezone conversion)
- ⚠️ Email Notifications (deadline timestamps)
- ⚠️ API Responses (should return deadlines in user's timezone)

### 6.2 Data Migration Required
**NO** - All existing deadlines stored in UTC remain valid. Only calculation logic changes.

### 6.3 Backward Compatibility
- Database schema: NO CHANGES required
- API contracts: OPTIONAL parameter added (backward compatible)
- Frontend: Should send user's timezone in requests (enhancement)

---

## 7. Performance Considerations

**Timezone Conversion Performance:**
- `TimeZoneInfo.ConvertTimeFromUtc()` - Fast (cached internally by .NET)
- IANA timezone lookup - Fast (pre-loaded by OS)
- Expected overhead: <1ms per SLA calculation

**Optimization:**
- Cache company/branch timezone lookups
- Pre-load SLA settings with timezone

---

## 8. Recommended Implementation Order

**Phase 1: Core Fixes (CRITICAL)**
1. Create `TimeZoneService.cs`
2. Update `SLACalculatorService.CalculateWorkingHoursDeadline()` to use timezone
3. Update `CreateComplaintCommandHandler` to pass timezone
4. Unit tests for timezone conversion

**Phase 2: Auto-Escalation (HIGH)**
1. Update `AutoEscalationService.CalculateHoursSinceLastAction()` for working hours
2. Add `CalculateWorkingHoursElapsed()` method
3. Integration tests for auto-escalation

**Phase 3: Display/API (MEDIUM)**
1. Update API responses to include timezone metadata
2. Update notification templates to show deadlines in company timezone
3. Update frontend to display times in user's timezone

**Phase 4: Advanced Features (LOW)**
1. Add timezone-aware holiday calendar support
2. Add per-category/priority timezone overrides
3. Add timezone audit trail

---

## 9. Code Snippets for Quick Fix

### Minimal Viable Fix (if full refactor not possible)

**Add to `SLACalculatorService.cs` before calculating working hours:**

```csharp
// QUICK FIX: Convert UTC to company timezone
private DateTime ConvertToCompanyTime(DateTime utcTime, string timeZoneId)
{
    try
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
    }
    catch
    {
        return utcTime; // Fallback to UTC
    }
}

private DateTime ConvertToUtc(DateTime localTime, string timeZoneId)
{
    try
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified),
            tz);
    }
    catch
    {
        return DateTime.SpecifyKind(localTime, DateTimeKind.Utc);
    }
}
```

**Update `CalculateWorkingHoursDeadline()` method:**

```csharp
// Line 191: Add timezone parameter
private DateTime CalculateWorkingHoursDeadline(
    DateTime startTime,
    int minutes,
    Domain.Entities.SLA.SLASettings? settings,
    string? timeZoneId = null)  // <-- ADD THIS
{
    // ... existing validation code ...

    string effectiveTimeZone = timeZoneId ?? settings?.Timezone ?? "UTC";

    // CRITICAL FIX: Convert to company timezone
    var currentTime = ConvertToCompanyTime(startTime, effectiveTimeZone);
    var remainingMinutes = minutes;

    // ... rest of existing logic (works in local time now) ...

    // CRITICAL FIX: Convert back to UTC before returning
    return ConvertToUtc(currentTime, effectiveTimeZone);
}
```

---

## 10. Summary

**Current State:**
- ❌ SLA deadlines calculated in UTC without timezone awareness
- ❌ Business hours (9 AM - 5 PM) interpreted as UTC, not company time
- ❌ Auto-escalation ignores working hours and timezone
- ✅ Timezone fields exist in database (not used)

**Required Changes:**
1. Create `TimeZoneService` for conversions
2. Update `SLACalculatorService.CalculateWorkingHoursDeadline()` to work in company timezone
3. Update `AutoEscalationService` to calculate working hours elapsed
4. Update complaint creation to pass timezone to SLA calculator

**Estimated Effort:**
- Core fixes: 8-12 hours
- Testing: 4-6 hours
- Total: 12-18 hours

**Risk Level:**
- Code changes: LOW (well-isolated to SLA service)
- Testing complexity: MEDIUM (timezone edge cases)
- Deployment risk: LOW (backward compatible)

---

## Files to Modify

1. **CREATE NEW:**
   - `ComplaintManagement.Infrastructure/Services/TimeZoneService.cs`
   - `ComplaintManagement.Application/Interfaces/Services/ITimeZoneService.cs`

2. **MODIFY EXISTING:**
   - `ComplaintManagement.Infrastructure/Services/SLACalculatorService.cs`
   - `ComplaintManagement.Infrastructure/Services/AutoEscalationService.cs`
   - `ComplaintManagement.Application/Features/Complaints/Handlers/CreateComplaintCommandHandler.cs`
   - `ComplaintManagement.Infrastructure/DependencyInjection.cs`

3. **TESTS TO ADD:**
   - `ComplaintManagement.Tests/Services/TimeZoneServiceTests.cs`
   - `ComplaintManagement.Tests/Services/SLACalculatorServiceTests_TimeZone.cs`
   - `ComplaintManagement.IntegrationTests/SLA/TimeZoneIntegrationTests.cs`

---

**END OF REPORT**
