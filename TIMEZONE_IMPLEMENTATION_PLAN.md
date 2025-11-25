# Timezone Implementation Plan
## Enterprise-Grade Multi-Timezone Support for Complaint Management System

---

## Executive Summary

This document provides a comprehensive, step-by-step implementation plan to add enterprise-grade timezone support to the Complaint Management System, following best practices from Salesforce, SAP, and Microsoft Dynamics 365.

**Current State:**
- Dates stored as `DateTime` (loses timezone context)
- Hardcoded IST (Asia/Kolkata) timezone in frontend
- No user timezone preference
- No timezone indication in UI

**Target State:**
- Dates stored as `DateTimeOffset` (preserves timezone)
- User-configurable timezone preference
- Automatic timezone conversion based on user location
- Clear timezone indicators in UI
- Full DST support

**Estimated Effort:** 5-7 days
**Risk Level:** Medium (requires database migration)
**Rollback Strategy:** Database backup before migration

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Phase 1: Backend Changes (2-3 days)](#phase-1-backend-changes)
3. [Phase 2: Frontend Changes (2-3 days)](#phase-2-frontend-changes)
4. [Phase 3: Testing (1 day)](#phase-3-testing)
5. [Phase 4: Migration & Deployment (1 day)](#phase-4-migration--deployment)
6. [Rollback Plan](#rollback-plan)
7. [Monitoring & Validation](#monitoring--validation)

---

## Prerequisites

### Required Knowledge
- Entity Framework Core migrations
- SQL Server `datetimeoffset` data type
- TypeScript/Angular date handling
- ISO 8601 date format standard

### Required Tools
- SQL Server Management Studio (for migration verification)
- .NET 8 SDK
- Node.js 18+ (for Angular)
- Postman/curl (for API testing)

### Backup Requirements
```sql
-- Backup database before migration
BACKUP DATABASE ComplaintManagement
TO DISK = 'C:\Backups\ComplaintManagement_PreTimezone_20250115.bak'
WITH FORMAT, COMPRESSION, STATS = 10;
```

---

## Phase 1: Backend Changes (2-3 days)

### Step 1.1: Add Timezone to User Entity (30 minutes)

**File:** `complaint-system-dotnet/src/ComplaintManagement.Domain/Entities/MasterData/User.cs`

**Add property after line 117:**
```csharp
/// <summary>
/// User's preferred timezone for displaying dates/times
/// IANA timezone identifier (e.g., "Asia/Kolkata", "America/New_York")
/// </summary>
public string TimeZone { get; set; } = "UTC";

/// <summary>
/// User's preferred date format
/// Options: "dd/MM/yyyy" (India/UK), "MM/dd/yyyy" (US), "yyyy-MM-dd" (ISO)
/// </summary>
public string DateFormat { get; set; } = "dd/MM/yyyy";

/// <summary>
/// User's preferred time format
/// Options: "24h" (24-hour), "12h" (12-hour AM/PM)
/// </summary>
public string TimeFormat { get; set; } = "12h";
```

**Validation:**
```csharp
// Add to User entity
public static readonly HashSet<string> ValidTimezones = new()
{
    "UTC",
    "Asia/Kolkata",           // IST (India)
    "America/New_York",       // EST/EDT (US East)
    "America/Los_Angeles",    // PST/PDT (US West)
    "Europe/London",          // GMT/BST (UK)
    "Europe/Paris",           // CET/CEST (Europe)
    "Asia/Dubai",             // GST (UAE)
    "Asia/Singapore",         // SGT (Singapore)
    "Asia/Tokyo",             // JST (Japan)
    "Australia/Sydney",       // AEDT/AEST (Australia)
};

public bool IsValidTimeZone()
{
    return ValidTimezones.Contains(TimeZone);
}
```

---

### Step 1.2: Update Entity Configuration (15 minutes)

**File:** `complaint-system-dotnet/src/ComplaintManagement.Infrastructure/Data/Configurations/MasterData/UserConfiguration.cs`

**Add to `Configure` method:**
```csharp
// Configure timezone properties
builder.Property(u => u.TimeZone)
    .HasMaxLength(50)
    .IsRequired()
    .HasDefaultValue("UTC");

builder.Property(u => u.DateFormat)
    .HasMaxLength(20)
    .IsRequired()
    .HasDefaultValue("dd/MM/yyyy");

builder.Property(u => u.TimeFormat)
    .HasMaxLength(10)
    .IsRequired()
    .HasDefaultValue("12h");
```

---

### Step 1.3: Migrate DateTime to DateTimeOffset (2 hours)

**Critical Step:** This changes the database schema and requires careful migration.

#### 1.3.1: Update BaseEntity

**File:** `complaint-system-dotnet/src/ComplaintManagement.Domain/Entities/BaseEntity.cs`

**Replace (lines 14-41):**
```csharp
/// <summary>
/// Record creation timestamp (stored as UTC with offset)
/// </summary>
public DateTimeOffset CreatedAt { get; set; }

/// <summary>
/// User ID who created this record
/// </summary>
public Guid? CreatedBy { get; set; }

/// <summary>
/// Last modification timestamp (stored as UTC with offset)
/// </summary>
public DateTimeOffset? UpdatedAt { get; set; }

/// <summary>
/// User ID who last modified this record
/// </summary>
public Guid? UpdatedBy { get; set; }

/// <summary>
/// Soft delete flag
/// </summary>
public bool IsDeleted { get; set; }

/// <summary>
/// Deletion timestamp (stored as UTC with offset)
/// </summary>
public DateTimeOffset? DeletedAt { get; set; }

/// <summary>
/// User ID who deleted this record
/// </summary>
public Guid? DeletedBy { get; set; }
```

#### 1.3.2: Update Other Entities with DateTime Properties

**Files to update:**

1. **Complaint.cs** (lines 107-124):
```csharp
/// <summary>
/// Date/time when complaint was submitted (with timezone offset)
/// </summary>
public DateTimeOffset SubmittedAt { get; set; }

/// <summary>
/// Date/time when complaint should be resolved (SLA deadline)
/// </summary>
public DateTimeOffset? DueDate { get; set; }

/// <summary>
/// Date/time when complaint was resolved
/// </summary>
public DateTimeOffset? ResolvedAt { get; set; }

/// <summary>
/// Date/time when complaint was closed
/// </summary>
public DateTimeOffset? ClosedAt { get; set; }
```

2. **User.cs** (lines 81, 86, 106, 116, 124, 141, 159, 165, 196):
```csharp
public DateTimeOffset? DateOfJoining { get; set; }
public DateTimeOffset? DateOfBirth { get; set; }
public DateTimeOffset? LastLoginAt { get; set; }
public DateTimeOffset? LastSyncedAt { get; set; }
public DateTimeOffset? PasswordExpiresAt { get; set; }
public DateTimeOffset? PasswordChangedAt { get; set; }
public DateTimeOffset? AccountLockedUntil { get; set; }
public DateTimeOffset? LastPasswordChangeRequiredNotificationSentAt { get; set; }
public DateTimeOffset? LastExternalSyncAt { get; set; }
```

3. **EmailMessage.cs** (lines 37-39, 67, 79):
```csharp
public DateTimeOffset ReceivedAt { get; set; }
public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
public DateTimeOffset? SentAt { get; set; }
public DateTimeOffset? ReadAt { get; set; }
public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
```

---

### Step 1.4: Create Entity Framework Migration (1 hour)

**Run migration command:**
```bash
cd complaint-system-dotnet/src/ComplaintManagement.Infrastructure

dotnet ef migrations add AddTimezoneSupport \
  --project ../ComplaintManagement.Infrastructure \
  --startup-project ../ComplaintManagement.API \
  --context ComplaintDbContext \
  --output-dir Data/Migrations
```

**Expected migration file content:**
```csharp
public partial class AddTimezoneSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add timezone columns to Users table
        migrationBuilder.AddColumn<string>(
            name: "TimeZone",
            table: "Users",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "UTC");

        migrationBuilder.AddColumn<string>(
            name: "DateFormat",
            table: "Users",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "dd/MM/yyyy");

        migrationBuilder.AddColumn<string>(
            name: "TimeFormat",
            table: "Users",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            defaultValue: "12h");

        // Convert DateTime columns to DateTimeOffset
        // IMPORTANT: Assume existing DateTime values are UTC

        // BaseEntity columns (affects all tables)
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreatedAt",
            table: "Complaints",
            type: "datetimeoffset(7)",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2");

        // ... (similar for all DateTime → DateTimeOffset conversions)
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Rollback: Remove timezone columns
        migrationBuilder.DropColumn(name: "TimeZone", table: "Users");
        migrationBuilder.DropColumn(name: "DateFormat", table: "Users");
        migrationBuilder.DropColumn(name: "TimeFormat", table: "Users");

        // Rollback: Convert DateTimeOffset back to DateTime
        // ... (reverse of Up migration)
    }
}
```

---

### Step 1.5: Update DTOs to Include Timezone (30 minutes)

**File:** `complaint-system-dotnet/src/ComplaintManagement.Application/DTOs/Auth/UserDto.cs`

**Add properties:**
```csharp
public string TimeZone { get; set; } = "UTC";
public string DateFormat { get; set; } = "dd/MM/yyyy";
public string TimeFormat { get; set; } = "12h";
```

**File:** `complaint-system-dotnet/src/ComplaintManagement.Application/DTOs/Complaints/ComplaintDto.cs`

**Update date properties (ensure they're DateTimeOffset):**
```csharp
public DateTimeOffset SubmittedAt { get; set; }
public DateTimeOffset? DueDate { get; set; }
public DateTimeOffset? ResolvedAt { get; set; }
public DateTimeOffset? ClosedAt { get; set; }
public DateTimeOffset CreatedAt { get; set; }
public DateTimeOffset? UpdatedAt { get; set; }
```

---

### Step 1.6: Configure JSON Serialization (15 minutes)

**File:** `complaint-system-dotnet/src/ComplaintManagement.API/Program.cs`

**Update JSON options (around line 99):**
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use numeric enum values for better frontend compatibility
        // Frontend sends and receives enum values as numbers (0, 1, 2, etc.)

        // Handle circular references in entity navigation properties
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        // ===== TIMEZONE SUPPORT =====
        // Serialize DateTimeOffset as ISO 8601 with timezone offset
        // Example: "2025-01-15T14:30:00+05:30"
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
```

**Verification:** DateTimeOffset will automatically serialize as ISO 8601:
```json
{
  "createdAt": "2025-01-15T14:30:00+05:30"
}
```

---

### Step 1.7: Add Timezone Controller Endpoints (45 minutes)

**Create new file:** `complaint-system-dotnet/src/ComplaintManagement.API/Controllers/TimeZoneController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeZoneController : ControllerBase
{
    /// <summary>
    /// Get list of supported timezones
    /// </summary>
    [HttpGet("supported")]
    public ActionResult<IEnumerable<TimeZoneInfo>> GetSupportedTimeZones()
    {
        var timezones = new List<object>
        {
            new { id = "UTC", displayName = "UTC (Coordinated Universal Time)", offset = "+00:00" },
            new { id = "Asia/Kolkata", displayName = "India Standard Time (IST)", offset = "+05:30" },
            new { id = "America/New_York", displayName = "Eastern Time (US & Canada)", offset = "-05:00" },
            new { id = "America/Los_Angeles", displayName = "Pacific Time (US & Canada)", offset = "-08:00" },
            new { id = "Europe/London", displayName = "London (GMT/BST)", offset = "+00:00" },
            new { id = "Europe/Paris", displayName = "Central European Time", offset = "+01:00" },
            new { id = "Asia/Dubai", displayName = "Dubai (Gulf Standard Time)", offset = "+04:00" },
            new { id = "Asia/Singapore", displayName = "Singapore Time", offset = "+08:00" },
            new { id = "Asia/Tokyo", displayName = "Japan Standard Time", offset = "+09:00" },
            new { id = "Australia/Sydney", displayName = "Australian Eastern Time", offset = "+11:00" }
        };

        return Ok(timezones);
    }

    /// <summary>
    /// Convert UTC time to specified timezone
    /// </summary>
    [HttpPost("convert")]
    public ActionResult<object> ConvertTime([FromBody] TimeConversionRequest request)
    {
        try
        {
            var utcTime = DateTimeOffset.Parse(request.UtcTime);
            var targetTz = TimeZoneInfo.FindSystemTimeZoneById(request.TargetTimeZone);
            var convertedTime = TimeZoneInfo.ConvertTime(utcTime, targetTz);

            return Ok(new
            {
                original = utcTime.ToString("o"),
                converted = convertedTime.ToString("o"),
                timezone = request.TargetTimeZone,
                displayFormat = convertedTime.ToString("MMMM d, yyyy h:mm tt"),
                abbreviation = targetTz.IsDaylightSavingTime(convertedTime)
                    ? targetTz.DaylightName
                    : targetTz.StandardName
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class TimeConversionRequest
{
    public string UtcTime { get; set; } = string.Empty;
    public string TargetTimeZone { get; set; } = "UTC";
}
```

---

### Step 1.8: Update User Settings Endpoint (30 minutes)

**File:** `complaint-system-dotnet/src/ComplaintManagement.API/Controllers/UsersController.cs`

**Add endpoint to update timezone:**
```csharp
/// <summary>
/// Update user timezone preferences
/// </summary>
[HttpPatch("{id}/timezone")]
[HasPermission("EditUsers")]
public async Task<ActionResult> UpdateUserTimezone(
    Guid id,
    [FromBody] UpdateUserTimezoneRequest request)
{
    var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
    if (user == null)
    {
        return NotFound(new { message = "User not found" });
    }

    // Validate timezone
    if (!User.ValidTimezones.Contains(request.TimeZone))
    {
        return BadRequest(new { message = "Invalid timezone" });
    }

    user.TimeZone = request.TimeZone;
    user.DateFormat = request.DateFormat ?? user.DateFormat;
    user.TimeFormat = request.TimeFormat ?? user.TimeFormat;

    await _unitOfWork.CompleteAsync();

    return Ok(new { message = "Timezone updated successfully" });
}

public class UpdateUserTimezoneRequest
{
    public string TimeZone { get; set; } = "UTC";
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
}
```

---

## Phase 2: Frontend Changes (2-3 days)

### Step 2.1: Install date-fns-tz Library (5 minutes)

```bash
cd complaint-system-angular
npm install date-fns date-fns-tz
```

**Verify package.json:**
```json
{
  "dependencies": {
    "date-fns": "^3.0.0",
    "date-fns-tz": "^2.0.0"
  }
}
```

---

### Step 2.2: Create Timezone Service (1 hour)

**Create file:** `complaint-system-angular/src/app/services/timezone.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { formatInTimeZone, toZonedTime } from 'date-fns-tz';
import { format } from 'date-fns';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class TimezoneService {
  constructor(private authService: AuthService) {}

  /**
   * Get current user's timezone preference
   * Falls back to browser timezone if not set
   */
  getUserTimezone(): string {
    const user = this.authService.currentUserValue;
    if (user?.timezone) {
      return user.timezone;
    }
    // Fallback to browser timezone
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
  }

  /**
   * Format a date/time in user's timezone
   * @param date - ISO 8601 date string or Date object
   * @param formatString - date-fns format string
   * @returns Formatted date string with timezone
   */
  formatInUserTimezone(
    date: string | Date | null | undefined,
    formatString: string = 'MMM d, yyyy h:mm a'
  ): string {
    if (!date) return '-';

    try {
      const userTz = this.getUserTimezone();
      const formattedDate = formatInTimeZone(date, userTz, formatString);

      // Add timezone abbreviation
      const tzAbbr = this.getTimezoneAbbreviation(date, userTz);
      return `${formattedDate} ${tzAbbr}`;
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }

  /**
   * Get timezone abbreviation (EST, PST, IST, etc.)
   */
  private getTimezoneAbbreviation(date: string | Date, timezone: string): string {
    try {
      const zonedDate = typeof date === 'string' ? new Date(date) : date;
      const formatted = formatInTimeZone(zonedDate, timezone, 'zzz');
      return formatted;
    } catch {
      return '';
    }
  }

  /**
   * Format date only (no time)
   */
  formatDateOnly(date: string | Date | null | undefined): string {
    if (!date) return '-';

    const user = this.authService.currentUserValue;
    const dateFormat = user?.dateFormat || 'dd/MM/yyyy';

    return this.formatInUserTimezone(date, dateFormat);
  }

  /**
   * Format time only (no date)
   */
  formatTimeOnly(date: string | Date | null | undefined): string {
    if (!date) return '-';

    const user = this.authService.currentUserValue;
    const timeFormat = user?.timeFormat === '24h' ? 'HH:mm' : 'h:mm a';

    return this.formatInUserTimezone(date, timeFormat);
  }

  /**
   * Get relative time (e.g., "5 minutes ago")
   */
  getRelativeTime(date: string | Date | null | undefined): string {
    if (!date) return '-';

    try {
      const now = new Date();
      const dateObj = typeof date === 'string' ? new Date(date) : date;
      const diffMs = now.getTime() - dateObj.getTime();
      const diffMins = Math.floor(diffMs / (1000 * 60));
      const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
      const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

      if (diffMins < 1) return 'Just now';
      if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
      if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
      if (diffDays === 1) return 'Yesterday';
      if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;

      // Fall back to formatted date for older dates
      return this.formatInUserTimezone(date, 'MMM d, yyyy');
    } catch (error) {
      console.error('Error calculating relative time:', error);
      return 'Invalid Date';
    }
  }

  /**
   * Get list of supported timezones
   */
  getSupportedTimezones(): Array<{ id: string; displayName: string; offset: string }> {
    return [
      { id: 'UTC', displayName: 'UTC (Coordinated Universal Time)', offset: '+00:00' },
      { id: 'Asia/Kolkata', displayName: 'India Standard Time (IST)', offset: '+05:30' },
      { id: 'America/New_York', displayName: 'Eastern Time (US & Canada)', offset: '-05:00' },
      { id: 'America/Los_Angeles', displayName: 'Pacific Time (US & Canada)', offset: '-08:00' },
      { id: 'Europe/London', displayName: 'London (GMT/BST)', offset: '+00:00' },
      { id: 'Europe/Paris', displayName: 'Central European Time', offset: '+01:00' },
      { id: 'Asia/Dubai', displayName: 'Dubai (Gulf Standard Time)', offset: '+04:00' },
      { id: 'Asia/Singapore', displayName: 'Singapore Time', offset: '+08:00' },
      { id: 'Asia/Tokyo', displayName: 'Japan Standard Time', offset: '+09:00' },
      { id: 'Australia/Sydney', displayName: 'Australian Eastern Time', offset: '+11:00' }
    ];
  }
}
```

---

### Step 2.3: Update User Model (5 minutes)

**File:** `complaint-system-angular/src/app/models/user.model.ts`

**Add properties (after line 25):**
```typescript
timezone?: string;        // IANA timezone identifier
dateFormat?: string;      // User's preferred date format
timeFormat?: string;      // '12h' or '24h'
```

---

### Step 2.4: Create Timezone Pipe (30 minutes)

**Create file:** `complaint-system-angular/src/app/pipes/timezone.pipe.ts`

```typescript
import { Pipe, PipeTransform } from '@angular/core';
import { TimezoneService } from '../services/timezone.service';

/**
 * Format dates in user's timezone
 * Usage: {{ date | timezone }}
 * Usage: {{ date | timezone:'short' }}
 * Usage: {{ date | timezone:'MMM d, yyyy h:mm a' }}
 */
@Pipe({
  name: 'timezone',
  standalone: true
})
export class TimezonePipe implements PipeTransform {
  constructor(private timezoneService: TimezoneService) {}

  transform(
    value: string | Date | null | undefined,
    format: 'short' | 'medium' | 'long' | 'dateOnly' | 'timeOnly' | 'relative' | string = 'short'
  ): string {
    if (!value) return '-';

    switch (format) {
      case 'short':
        return this.timezoneService.formatInUserTimezone(value, 'MMM d, yyyy h:mm a');
      case 'medium':
        return this.timezoneService.formatInUserTimezone(value, 'MMMM d, yyyy h:mm a');
      case 'long':
        return this.timezoneService.formatInUserTimezone(value, 'EEEE, MMMM d, yyyy h:mm a');
      case 'dateOnly':
        return this.timezoneService.formatDateOnly(value);
      case 'timeOnly':
        return this.timezoneService.formatTimeOnly(value);
      case 'relative':
        return this.timezoneService.getRelativeTime(value);
      default:
        // Custom format string
        return this.timezoneService.formatInUserTimezone(value, format);
    }
  }
}
```

---

### Step 2.5: Replace Hardcoded IST with User Timezone (1 hour)

**Update files to use new timezone pipe:**

1. **Replace `utcToLocal` pipe with `timezone` pipe everywhere**

**Find and replace across project:**
```bash
# Search for:  | utcToLocal
# Replace with: | timezone
```

2. **Update DateService (if still used directly)**

**File:** `complaint-system-angular/src/app/services/date.service.ts`

**Replace with:**
```typescript
import { Injectable } from '@angular/core';
import { TimezoneService } from './timezone.service';

/**
 * DEPRECATED: Use TimezoneService instead
 * This service is kept for backward compatibility
 */
@Injectable({
  providedIn: 'root'
})
export class DateService {
  constructor(private timezoneService: TimezoneService) {}

  formatDate(dateString: string | null | undefined, includeSeconds: boolean = false): string {
    const format = includeSeconds ? 'MMM d, yyyy h:mm:ss a' : 'MMM d, yyyy h:mm a';
    return this.timezoneService.formatInUserTimezone(dateString, format);
  }

  formatDateShort(dateString: string | null | undefined): string {
    return this.timezoneService.formatInUserTimezone(dateString, 'MMM d, yyyy h:mm a');
  }

  formatTime(dateString: string | null | undefined, includeSeconds: boolean = false): string {
    return this.timezoneService.formatTimeOnly(dateString);
  }

  formatDateOnly(dateString: string | null | undefined): string {
    return this.timezoneService.formatDateOnly(dateString);
  }

  getRelativeTime(dateString: string | null | undefined): string {
    return this.timezoneService.getRelativeTime(dateString);
  }

  isToday(dateString: string | null | undefined): boolean {
    // Implement using date-fns if needed
    return false;
  }

  getCurrentISTTimestamp(): string {
    return new Date().toISOString();
  }
}
```

---

### Step 2.6: Create Timezone Settings Component (1.5 hours)

**Create file:** `complaint-system-angular/src/app/components/settings/timezone-settings/timezone-settings.component.ts`

```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { UserService } from '../../../services/user.service';
import { TimezoneService } from '../../../services/timezone.service';

@Component({
  selector: 'app-timezone-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './timezone-settings.component.html',
  styleUrls: ['./timezone-settings.component.scss']
})
export class TimezoneSettingsComponent implements OnInit {
  selectedTimezone: string = 'UTC';
  selectedDateFormat: string = 'dd/MM/yyyy';
  selectedTimeFormat: string = '12h';

  timezones: Array<{ id: string; displayName: string; offset: string }> = [];

  dateFormats = [
    { value: 'dd/MM/yyyy', label: '31/01/2025 (DD/MM/YYYY)' },
    { value: 'MM/dd/yyyy', label: '01/31/2025 (MM/DD/YYYY)' },
    { value: 'yyyy-MM-dd', label: '2025-01-31 (YYYY-MM-DD)' }
  ];

  timeFormats = [
    { value: '12h', label: '12-hour (3:30 PM)' },
    { value: '24h', label: '24-hour (15:30)' }
  ];

  saving = false;
  saved = false;
  error: string | null = null;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private timezoneService: TimezoneService
  ) {}

  ngOnInit(): void {
    this.timezones = this.timezoneService.getSupportedTimezones();

    const user = this.authService.currentUserValue;
    if (user) {
      this.selectedTimezone = user.timezone || this.detectBrowserTimezone();
      this.selectedDateFormat = user.dateFormat || 'dd/MM/yyyy';
      this.selectedTimeFormat = user.timeFormat || '12h';
    }
  }

  detectBrowserTimezone(): string {
    const detected = Intl.DateTimeFormat().resolvedOptions().timeZone;
    const supported = this.timezones.find(tz => tz.id === detected);
    return supported ? detected : 'UTC';
  }

  saveSettings(): void {
    this.saving = true;
    this.saved = false;
    this.error = null;

    const user = this.authService.currentUserValue;
    if (!user) {
      this.error = 'User not authenticated';
      this.saving = false;
      return;
    }

    this.userService.updateUserTimezone(user.id, {
      timezone: this.selectedTimezone,
      dateFormat: this.selectedDateFormat,
      timeFormat: this.selectedTimeFormat
    }).subscribe({
      next: () => {
        this.saving = false;
        this.saved = true;

        // Update user in AuthService
        const updatedUser = { ...user };
        updatedUser.timezone = this.selectedTimezone;
        updatedUser.dateFormat = this.selectedDateFormat;
        updatedUser.timeFormat = this.selectedTimeFormat;

        sessionStorage.setItem('complaint_system_user', JSON.stringify(updatedUser));

        // Reload page to apply new timezone
        setTimeout(() => {
          window.location.reload();
        }, 1500);
      },
      error: (err) => {
        this.saving = false;
        this.error = err.error?.message || 'Failed to save timezone settings';
      }
    });
  }

  getExampleDate(): string {
    const now = new Date();
    return this.timezoneService.formatInUserTimezone(
      now.toISOString(),
      this.selectedDateFormat + ' ' + (this.selectedTimeFormat === '12h' ? 'h:mm a' : 'HH:mm')
    );
  }
}
```

**Create file:** `complaint-system-angular/src/app/components/settings/timezone-settings/timezone-settings.component.html`

```html
<div class="timezone-settings">
  <h2>Timezone & Format Settings</h2>

  <div class="alert alert-info">
    <i class="bi bi-info-circle"></i>
    All dates and times will be displayed in your selected timezone.
  </div>

  <div class="settings-form">
    <!-- Timezone Selection -->
    <div class="form-group">
      <label for="timezone">Timezone</label>
      <select
        id="timezone"
        class="form-control"
        [(ngModel)]="selectedTimezone"
        (change)="getExampleDate()">
        <option *ngFor="let tz of timezones" [value]="tz.id">
          {{ tz.displayName }} ({{ tz.offset }})
        </option>
      </select>
      <small class="form-text text-muted">
        Your browser timezone: {{ detectBrowserTimezone() }}
      </small>
    </div>

    <!-- Date Format Selection -->
    <div class="form-group">
      <label for="dateFormat">Date Format</label>
      <select
        id="dateFormat"
        class="form-control"
        [(ngModel)]="selectedDateFormat"
        (change)="getExampleDate()">
        <option *ngFor="let fmt of dateFormats" [value]="fmt.value">
          {{ fmt.label }}
        </option>
      </select>
    </div>

    <!-- Time Format Selection -->
    <div class="form-group">
      <label for="timeFormat">Time Format</label>
      <select
        id="timeFormat"
        class="form-control"
        [(ngModel)]="selectedTimeFormat"
        (change)="getExampleDate()">
        <option *ngFor="let fmt of timeFormats" [value]="fmt.value">
          {{ fmt.label }}
        </option>
      </select>
    </div>

    <!-- Preview -->
    <div class="form-group">
      <label>Preview</label>
      <div class="preview-box">
        {{ getExampleDate() }}
      </div>
    </div>

    <!-- Save Button -->
    <div class="form-actions">
      <button
        class="btn btn-primary"
        (click)="saveSettings()"
        [disabled]="saving">
        <span *ngIf="saving">
          <i class="bi bi-hourglass-split"></i> Saving...
        </span>
        <span *ngIf="!saving">
          <i class="bi bi-check-circle"></i> Save Settings
        </span>
      </button>

      <div *ngIf="saved" class="alert alert-success mt-3">
        <i class="bi bi-check-circle-fill"></i>
        Settings saved! Page will reload to apply changes...
      </div>

      <div *ngIf="error" class="alert alert-danger mt-3">
        <i class="bi bi-exclamation-triangle-fill"></i>
        {{ error }}
      </div>
    </div>
  </div>
</div>
```

---

### Step 2.7: Add Timezone Indicator to UI (30 minutes)

**Update header/navbar to show current timezone:**

**File:** Update your dashboard or header component

```html
<!-- Add timezone indicator to header -->
<div class="timezone-indicator">
  <i class="bi bi-clock"></i>
  {{ currentTime | timezone }}
  <span class="timezone-abbr">{{ getUserTimezone() }}</span>
</div>
```

---

## Phase 3: Testing (1 day)

### Step 3.1: Unit Testing

**Backend Tests:**
```csharp
// Test DateTimeOffset serialization
[Fact]
public void Complaint_CreatedAt_Should_Serialize_With_Offset()
{
    var complaint = new Complaint
    {
        CreatedAt = new DateTimeOffset(2025, 1, 15, 14, 30, 0, TimeSpan.FromHours(5.5))
    };

    var json = JsonSerializer.Serialize(complaint);

    Assert.Contains("2025-01-15T14:30:00+05:30", json);
}
```

**Frontend Tests:**
```typescript
// Test timezone conversion
describe('TimezoneService', () => {
  it('should format UTC date in IST', () => {
    const service = new TimezoneService(mockAuthService);
    const result = service.formatInUserTimezone(
      '2025-01-15T14:30:00Z',
      'MMM d, yyyy h:mm a'
    );
    expect(result).toContain('Jan 15, 2025 8:00 PM');
  });
});
```

---

### Step 3.2: Integration Testing

**Test Scenarios:**

1. **Create Complaint in EST, View in IST**
   - User in New York creates complaint at 2:00 PM EST
   - User in Mumbai views it as 12:30 AM IST (next day)

2. **DST Transition**
   - Create complaint before DST
   - View after DST
   - Verify time doesn't "jump"

3. **Timezone Preference Update**
   - User changes timezone from IST to PST
   - All dates update immediately

---

### Step 3.3: End-to-End Testing

**Test Matrix:**

| User Location | Timezone | Create Time | Expected Display (IST) | Expected Display (EST) |
|--------------|----------|-------------|------------------------|------------------------|
| Mumbai | Asia/Kolkata | 8:00 PM IST | Jan 15, 2025 8:00 PM IST | Jan 15, 2025 9:30 AM EST |
| New York | America/New_York | 2:00 PM EST | Jan 16, 2025 12:30 AM IST | Jan 15, 2025 2:00 PM EST |
| London | Europe/London | 10:00 AM GMT | Jan 15, 2025 3:30 PM IST | Jan 15, 2025 5:00 AM EST |

---

## Phase 4: Migration & Deployment (1 day)

### Step 4.1: Pre-Deployment Checklist

- [ ] Database backup completed
- [ ] Migration scripts tested on staging
- [ ] Frontend build successful
- [ ] All unit tests passing
- [ ] Integration tests passing
- [ ] Rollback plan documented
- [ ] User communication prepared

---

### Step 4.2: Apply Migration

```bash
# 1. Stop application
pm2 stop complaint-api
pm2 stop complaint-angular

# 2. Backup database
sqlcmd -S localhost -Q "BACKUP DATABASE ComplaintManagement TO DISK = 'C:\Backups\ComplaintManagement_PreTimezone_20250115.bak'"

# 3. Apply migration
cd complaint-system-dotnet/src/ComplaintManagement.API
dotnet ef database update --context ComplaintDbContext

# 4. Verify migration
sqlcmd -S localhost -d ComplaintManagement -Q "SELECT TOP 5 CreatedAt FROM Complaints"

# 5. Start application
pm2 start complaint-api
pm2 start complaint-angular
```

---

### Step 4.3: Post-Deployment Validation

**Smoke Tests:**
1. Login as admin
2. Navigate to Settings → Timezone
3. Change timezone from IST to EST
4. Verify all dates update
5. Create new complaint
6. Verify timestamp is correct

---

## Rollback Plan

### If Migration Fails

```sql
-- Restore from backup
USE master;
GO
ALTER DATABASE ComplaintManagement SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
RESTORE DATABASE ComplaintManagement
FROM DISK = 'C:\Backups\ComplaintManagement_PreTimezone_20250115.bak'
WITH REPLACE;
GO
ALTER DATABASE ComplaintManagement SET MULTI_USER;
GO
```

### If Frontend Issues Occur

```bash
# Revert to previous Angular build
cd complaint-system-angular
git checkout HEAD~1 -- dist/
pm2 restart complaint-angular
```

---

## Monitoring & Validation

### Metrics to Monitor

1. **API Response Times:** Should not increase significantly
2. **Database Query Performance:** Index datetime columns if needed
3. **User Timezone Adoption:** Track % of users with custom timezone
4. **Error Rates:** Monitor for timezone-related errors

### Validation Queries

```sql
-- Check timezone distribution
SELECT TimeZone, COUNT(*) as UserCount
FROM Users
WHERE IsDeleted = 0
GROUP BY TimeZone
ORDER BY UserCount DESC;

-- Verify DateTimeOffset columns
SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE DATA_TYPE = 'datetimeoffset';
```

---

## Success Criteria

- [ ] All dates stored as `DateTimeOffset` in database
- [ ] Users can set timezone preference
- [ ] All UI dates display in user's timezone
- [ ] Timezone abbreviation shown (IST, EST, etc.)
- [ ] API returns ISO 8601 with offset
- [ ] No performance degradation
- [ ] All tests passing
- [ ] Zero data loss during migration

---

## Next Steps (Optional Enhancements)

1. **Auto-detect timezone on first login**
2. **Email notifications in user's timezone**
3. **Timezone-aware reporting**
4. **Calendar integration (iCal export)**
5. **Mobile app timezone sync**

---

## Support & Documentation

- **Internal Wiki:** Link to timezone documentation
- **User Guide:** How to change timezone settings
- **API Documentation:** ISO 8601 format examples
- **Troubleshooting:** Common timezone issues

---

**Document Version:** 1.0
**Last Updated:** January 15, 2025
**Author:** Enterprise Architecture Team
**Review Date:** Quarterly
