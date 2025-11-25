# Enterprise Timezone Management Best Practices

## Executive Summary

This document outlines how major enterprise systems (Salesforce, SAP, Microsoft Dynamics) handle timezone management and provides industry-standard best practices for implementing timezone support in enterprise applications.

---

## Table of Contents

1. [Industry Standards](#industry-standards)
2. [How Salesforce Handles Timezones](#how-salesforce-handles-timezones)
3. [How SAP Handles Timezones](#how-sap-handles-timezones)
4. [How Microsoft Dynamics 365 Handles Timezones](#how-microsoft-dynamics-365-handles-timezones)
5. [How ServiceNow Handles Timezones](#how-servicenow-handles-timezones)
6. [Best Practices Summary](#best-practices-summary)
7. [Common Pitfalls to Avoid](#common-pitfalls-to-avoid)

---

## Industry Standards

### ISO 8601 Standard

**What is it?**
- International standard for representing dates and times
- Ensures consistent date/time exchange across systems
- Unambiguous timezone representation

**Formats:**
```
UTC:                    2025-01-15T14:30:00Z
With Offset:            2025-01-15T14:30:00+05:30
With Milliseconds:      2025-01-15T14:30:00.123Z
With Timezone:          2025-01-15T14:30:00+05:30[Asia/Kolkata]
```

**Why Use ISO 8601?**
- Globally recognized
- Sortable (lexicographic order matches chronological order)
- Parseable by all modern programming languages
- Avoids MM/DD/YYYY vs DD/MM/YYYY confusion
- Includes timezone information

### IANA Time Zone Database

**What is it?**
- Standard source of timezone data (tzdata)
- Used by Unix/Linux, Java, .NET, JavaScript
- Canonical timezone names: `Asia/Kolkata`, `America/New_York`, `Europe/London`

**Why Use IANA Names?**
- Handles DST (Daylight Saving Time) automatically
- Historical timezone changes
- Political timezone changes (e.g., Russia timezone changes in 2014)
- More precise than "GMT+5:30" (which doesn't account for DST)

---

## How Salesforce Handles Timezones

### Architecture

```
User Profile → Timezone Setting → All Dates/Times Displayed in User TZ
```

### Key Features

**1. User-Level Timezone Setting**
- Each user has a timezone in their profile
- Setting: `Setup → My Settings → Personal → Language & Time Zone`
- Over 100+ timezones supported (full IANA list)

**2. Storage Strategy**
```
Database:       All dates stored in UTC (Coordinated Universal Time)
API Response:   ISO 8601 format with 'Z' suffix (UTC)
UI Display:     Converted to user's timezone on the fly
```

**3. UI Presentation**

```html
<!-- Example: Email received timestamp -->
<span class="timestamp">
  Jan 15, 2025 2:30 PM PST
  <span class="timezone-abbr">PST</span>
</span>

<!-- Hover tooltip shows absolute time -->
<div class="tooltip">
  Wednesday, January 15, 2025 at 2:30 PM
  Pacific Standard Time (GMT-8)
</div>
```

**4. Relative Time Display**
- "5 minutes ago"
- "Yesterday at 3:30 PM"
- "Last Monday"
- Falls back to absolute date after 7 days

**5. API Behavior**

**Request (Creating a Record):**
```json
POST /services/data/v57.0/sobjects/Case
{
  "Subject": "Support Request",
  "CreatedDate": "2025-01-15T14:30:00Z"
}
```

**Response (Reading a Record):**
```json
{
  "Id": "500xx000000001",
  "Subject": "Support Request",
  "CreatedDate": "2025-01-15T14:30:00.000+0000",
  "LastModifiedDate": "2025-01-15T14:30:00.000+0000"
}
```

**6. Timezone Conversion Functions (Apex)**
```apex
// Convert to user's timezone
DateTime utcTime = DateTime.now();
String userTz = UserInfo.getTimeZone().getID(); // 'America/Los_Angeles'
String formatted = utcTime.format('MMM d, yyyy h:mm a', userTz);
// Output: "Jan 15, 2025 6:30 AM"
```

### Salesforce UI Components

**Date/Time Picker:**
- Always displays in user's timezone
- Stores as UTC in database
- Label shows timezone: "Due Date (PST)"

**Reports & Dashboards:**
- All timestamps converted to report viewer's timezone
- Timezone shown in footer: "Times displayed in Pacific Standard Time"

---

## How SAP Handles Timezones

### Architecture

```
User Master Data (TCODE: SU01) → Time Zone Field → System-Wide Conversion
```

### Key Features

**1. Timezone Tables**
```
TTZZ  - Time zones
TTZCR - Timezone-Country Assignment
TTZCU - Timezone Customizing
```

**2. User Master Record**
- Field: `TZONE` in table `USR01`
- Set via: `SU01` → User Master Maintenance
- Default from system locale if not set

**3. ABAP Functions for Conversion**

```abap
* Convert UTC to user timezone
DATA: lv_timestamp TYPE timestamp,
      lv_timezone  TYPE timezone.

lv_timezone = sy-zonlo.  "Current user's timezone

CALL FUNCTION 'CONVERT_TIME_INPUT'
  EXPORTING
    input_timezone       = 'UTC'
    target_timezone      = lv_timezone
    input_timestamp      = lv_timestamp
  IMPORTING
    output_timestamp     = lv_timestamp.
```

**4. System Settings**

**Instance Profile Parameters:**
```
TTZCU:      Timezone customizing table
LOCAL_TZ:   Server timezone
USER_TZ:    Use user-specific timezone for display
```

**5. Database Storage**
```sql
-- All timestamps stored in UTC
CREATE TABLE COMPLAINTS (
  CREATED_AT TIMESTAMP,        -- UTC
  MODIFIED_AT TIMESTAMP,       -- UTC
  USER_TZ VARCHAR(6)           -- User's timezone at creation time
)
```

**6. S/4HANA Fiori Apps**
- Use browser's `Intl.DateTimeFormat` API
- Timezone selector in user settings
- Displays timezone abbreviation (EST, PST, IST)

### SAP Best Practices

1. **Always store in UTC** (TIMESTAMP type)
2. **Convert only for display** (never store local time)
3. **Log timezone with audit records** (know what timezone user saw)
4. **Handle DST transitions** (use ABAP conversion functions)
5. **Test across timezones** (especially for scheduled jobs)

---

## How Microsoft Dynamics 365 Handles Timezones

### Architecture

```
SQL Server (datetimeoffset) → Business Layer (DateTimeOffset) → UI (User TZ)
```

### Key Features

**1. Database Column Type: `datetimeoffset`**

```sql
CREATE TABLE Complaint (
    ComplaintId UNIQUEIDENTIFIER PRIMARY KEY,
    CreatedOn datetimeoffset(7) NOT NULL,  -- Stores: 2025-01-15 14:30:00 +05:30
    ModifiedOn datetimeoffset(7)
);
```

**Why `datetimeoffset`?**
- Stores both UTC time AND offset
- Example: `2025-01-15 14:30:00 +05:30`
- Can reconstruct original timezone
- No ambiguity during DST transitions

**2. .NET Data Type: `DateTimeOffset`**

```csharp
// Entity Framework entity
public class Complaint
{
    public Guid ComplaintId { get; set; }

    // GOOD: Preserves timezone offset
    public DateTimeOffset CreatedOn { get; set; }

    // BAD: Loses timezone information
    // public DateTime CreatedOn { get; set; }
}
```

**3. User Settings**
- Setting: `Settings → Personalization Settings → Time Zone`
- Over 150+ timezones (full IANA list)
- Can override per user or per session

**4. Web API Response**

```json
{
  "complaintid": "12345678-1234-1234-1234-123456789012",
  "createdon": "2025-01-15T14:30:00+05:30",
  "modifiedon": "2025-01-15T15:45:00+05:30"
}
```

**5. Client-Side Conversion (TypeScript)**

```typescript
// Dynamics uses moment.js or date-fns
import { formatInTimeZone } from 'date-fns-tz';

const userTimezone = userSettings.timezone; // 'America/New_York'
const displayDate = formatInTimeZone(
  '2025-01-15T14:30:00+05:30',
  userTimezone,
  'MMM d, yyyy h:mm a zzz'
);
// Output: "Jan 15, 2025 4:00 AM EST"
```

**6. Server-Side Timezone Conversion (C#)**

```csharp
using NodaTime;

// Get user's timezone
var userTz = DateTimeZoneProviders.Tzdb["America/New_York"];

// Convert DateTimeOffset to user's local time
var utcTime = DateTimeOffset.UtcNow;
var zonedTime = Instant.FromDateTimeOffset(utcTime)
                       .InZone(userTz);

// Format for display
var formatted = zonedTime.ToString(
    "MMMM d, yyyy h:mm tt",
    CultureInfo.InvariantCulture
);
```

### Dynamics 365 Best Practices

1. **Use `DateTimeOffset` everywhere** (not `DateTime`)
2. **Store with offset** (preserves original timezone context)
3. **Convert on display** (server or client side)
4. **Log timezone changes** (audit trail)
5. **Handle null timezones** (default to UTC)

---

## How ServiceNow Handles Timezones

### Architecture

```
Database (UTC) → GlideDateTime → Session Timezone → UI Display
```

### Key Features

**1. Session Timezone**
- Set at login based on user profile
- Can be changed mid-session
- Affects all date/time displays

**2. GlideDateTime Object**

```javascript
// Server-side script (GlideRecord)
var gr = new GlideRecord('incident');
gr.get('INC0001234');

// Get as UTC
var utcTime = gr.sys_created_on.getDisplayValue(); // '2025-01-15 14:30:00'

// Get in user's timezone (automatic conversion)
var userTime = gr.sys_created_on.getLocalTime(); // '2025-01-15 20:00:00' (for IST user)

// Get as JavaScript Date object
var jsDate = gr.sys_created_on.getGlideDateTime().getNumericValue();
```

**3. Client-Side (UI)**

```javascript
// Client script
var created = g_form.getValue('sys_created_on');
// Already in user's timezone format
// Display: "2025-01-15 20:00:00" (for IST user)

// Format for display
var formatted = new GlideDateTime(created).getLocalDate();
// Display: "15-01-2025" (based on user's date format preference)
```

**4. REST API**

**Request (Create Incident):**
```json
POST /api/now/table/incident
{
  "short_description": "Server down",
  "sys_created_on": "2025-01-15 14:30:00"  // Assumed to be in session timezone
}
```

**Response (Read Incident):**
```json
{
  "result": {
    "sys_id": "abc123",
    "short_description": "Server down",
    "sys_created_on": "2025-01-15 14:30:00",  // UTC
    "sys_created_on_display": "Jan 15, 2025 8:00 PM"  // User's timezone (IST)
  }
}
```

**5. System Properties**

```
glide.sys.date_format:           dd-MM-yyyy (India: dd-MM-yyyy, US: MM-dd-yyyy)
glide.sys.time_format:           HH:mm:ss (or h:mm:ss a for 12-hour)
glide.ui.date_time_format:       dd-MM-yyyy HH:mm:ss
```

### ServiceNow Best Practices

1. **Always work in UTC on server** (`GlideDateTime.getNumericValue()`)
2. **Let framework handle display** (automatic conversion to user TZ)
3. **Use display values for UI** (`getDisplayValue()` vs `getValue()`)
4. **Test with multiple timezones** (use timezone simulator)
5. **Document timezone assumptions** (especially for integrations)

---

## Best Practices Summary

### The Golden Rules

| Rule | Description | Example |
|------|-------------|---------|
| **Store in UTC** | Always store timestamps in UTC in the database | `2025-01-15T14:30:00Z` |
| **Transmit with Timezone** | API responses should include timezone offset | `2025-01-15T14:30:00+05:30` |
| **Convert for Display** | Convert to user's timezone only for display | "Jan 15, 2025 8:00 PM IST" |
| **Use ISO 8601** | Standard format for date/time interchange | `2025-01-15T14:30:00Z` |
| **Log Timezone Context** | Audit logs should record user's timezone | "Created by User X (EST)" |

### The Implementation Pattern

```
┌─────────────────────────────────────────────────────────────────┐
│                     ENTERPRISE TIMEZONE FLOW                     │
└─────────────────────────────────────────────────────────────────┘

  USER INPUT                 BACKEND                    DATABASE
  ─────────────             ─────────                  ─────────────

  2025-01-15 8:00 PM  →     Convert to UTC      →     2025-01-15 14:30:00Z
  (User's Timezone)         (Server-Side)             (Always UTC)

  ↓                               ↓                           ↓

  User sets TZ in profile    Store user TZ          Index for queries
  (Asia/Kolkata)             preference             (Efficient sorting)


  DATABASE                   BACKEND                    USER DISPLAY
  ─────────────             ─────────                  ─────────────

  2025-01-15 14:30:00Z  →   Read user's TZ      →     Jan 15, 2025 8:00 PM IST
  (Always UTC)               Convert to user TZ        (User's Timezone)
                             Add TZ abbreviation       + Relative time
                                                       ("5 mins ago")
```

### Technology-Specific Recommendations

#### .NET / C#
```csharp
// Use DateTimeOffset, NOT DateTime
public DateTimeOffset CreatedAt { get; set; }

// Database: datetimeoffset(7)
// API: ISO 8601 with offset
// Client: Convert based on user TZ preference
```

#### Angular / TypeScript
```typescript
// Use date-fns-tz for timezone conversions
import { formatInTimeZone } from 'date-fns-tz';

const userTz = this.authService.currentUser.timezone;
const display = formatInTimeZone(
  apiDate,
  userTz,
  'MMM d, yyyy h:mm a zzz'
);
```

#### SQL Server
```sql
-- Use datetimeoffset(7), NOT datetime
CREATE TABLE Complaints (
  Id uniqueidentifier PRIMARY KEY,
  CreatedAt datetimeoffset(7) NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
```

---

## Common Pitfalls to Avoid

### 1. Using `DateTime` Instead of `DateTimeOffset`

**Problem:**
```csharp
// BAD: Loses timezone information
public DateTime CreatedAt { get; set; }
```

**Solution:**
```csharp
// GOOD: Preserves timezone offset
public DateTimeOffset CreatedAt { get; set; }
```

**Why?**
- `DateTime` has a `Kind` property (Utc, Local, Unspecified) but it's unreliable
- `DateTimeOffset` always includes the offset (+05:30, -08:00, etc.)
- Prevents bugs during DST transitions

---

### 2. Storing Local Time in Database

**Problem:**
```sql
-- BAD: Ambiguous during DST transitions
INSERT INTO Complaints (CreatedAt) VALUES ('2025-03-10 02:30:00');
-- Is this before or after DST? Unclear!
```

**Solution:**
```sql
-- GOOD: Store UTC with offset
INSERT INTO Complaints (CreatedAt) VALUES ('2025-03-10 02:30:00 -05:00');
-- Unambiguous: This is EST, before DST
```

---

### 3. Client-Side Timezone Conversion

**Problem:**
```typescript
// BAD: Relies on client's system clock
const localDate = new Date(apiResponse.createdAt);
// If user's computer clock is wrong, display is wrong!
```

**Solution:**
```typescript
// GOOD: Trust the server's UTC time, convert based on user preference
const userTz = this.authService.currentUser.timezone || 'UTC';
const display = formatInTimeZone(
  apiResponse.createdAt,
  userTz,
  'MMM d, yyyy h:mm a zzz'
);
```

---

### 4. Not Handling DST Transitions

**Problem:**
```csharp
// BAD: Manual offset calculation
var localTime = utcTime.AddHours(5.5); // Assumes IST never changes
```

**Solution:**
```csharp
// GOOD: Use TimeZoneInfo for automatic DST handling
var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, istZone);
```

**Note:** India doesn't observe DST, but many countries do!

---

### 5. Ignoring User Preferences

**Problem:**
```typescript
// BAD: Hardcoded timezone
const display = formatInTimeZone(date, 'Asia/Kolkata', 'MMM d, yyyy');
// What if user is in US office?
```

**Solution:**
```typescript
// GOOD: Use user's timezone preference
const userTz = this.authService.currentUser.timezone ||
               Intl.DateTimeFormat().resolvedOptions().timeZone;
const display = formatInTimeZone(date, userTz, 'MMM d, yyyy h:mm a zzz');
```

---

### 6. Not Showing Timezone in UI

**Problem:**
```html
<!-- BAD: User doesn't know which timezone -->
<span>Created: Jan 15, 2025 8:00 PM</span>
```

**Solution:**
```html
<!-- GOOD: Show timezone abbreviation -->
<span>Created: Jan 15, 2025 8:00 PM IST</span>

<!-- BETTER: Add tooltip with full timezone -->
<span title="India Standard Time (UTC+5:30)">
  Created: Jan 15, 2025 8:00 PM IST
</span>
```

---

### 7. Not Testing Across Timezones

**Problem:**
- Develop and test only in your local timezone
- Deploy to production, users in different timezones see bugs

**Solution:**
- **Test with multiple timezones:**
  - UTC (baseline)
  - EST/PST (negative offset, has DST)
  - IST (positive offset, no DST)
  - JST (positive offset, no DST, different from IST)
- **Test DST transitions:**
  - March 10, 2025 2:00 AM EST → EDT (spring forward)
  - November 3, 2025 2:00 AM EDT → EST (fall back)

---

## Conclusion

**Key Takeaways:**

1. **Store in UTC** - Always, no exceptions
2. **Use `DateTimeOffset` in .NET** - Not `DateTime`
3. **Transmit with ISO 8601** - Include timezone offset
4. **Convert for Display Only** - Based on user preference
5. **Show Timezone in UI** - Users need context
6. **Test Thoroughly** - Multiple timezones and DST transitions

**Enterprise systems like Salesforce, SAP, and Dynamics 365 all follow this pattern:**
- User-level timezone setting
- UTC storage
- Automatic conversion for display
- Clear UI indication of timezone
- Robust handling of DST and timezone changes

By following these best practices, your complaint management system will handle timezones as reliably as these industry leaders.

---

## References

- [ISO 8601 Standard](https://www.iso.org/iso-8601-date-and-time-format.html)
- [IANA Time Zone Database](https://www.iana.org/time-zones)
- [Salesforce Time Zone Management](https://help.salesforce.com/s/articleView?id=sf.admin_supported_timezone.htm)
- [SAP Time Zone Handling](https://help.sap.com/docs/SAP_NETWEAVER_750/saphelp_nw75/en-US/48/f21c7d6e6d19f0e10000000a421937/content.htm)
- [Microsoft Dynamics 365 Time Zones](https://learn.microsoft.com/en-us/power-platform/admin/manage-time-zones)
- [.NET DateTimeOffset](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset)
- [NodaTime Library](https://nodatime.org/) (Recommended for .NET timezone handling)
