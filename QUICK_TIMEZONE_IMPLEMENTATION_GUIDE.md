# Quick Timezone Implementation Guide

## TL;DR - What You Need to Know

The frontend stores user data in **sessionStorage** via **AuthService** using a **BehaviorSubject** pattern. Adding timezone support requires:

1. Add `timezone?: string` to User interface
2. Detect browser timezone on login (1 line of code)
3. Update DateService to read `user.timezone` instead of `environment.timezone`
4. Update Pipe to read `user.timezone`

**Total Changes**: 4 files, ~20 lines of code

---

## Key Discovery: Excellent Architecture Already Exists

### How User Data is Currently Stored

```typescript
// Location: src/app/services/auth.service.ts

// STORAGE MECHANISM (Lines 74-84)
private handleAuthenticationSuccess(data: {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User
}): void {
  // Store in sessionStorage
  sessionStorage.setItem(this.tokenKey, data.token);
  sessionStorage.setItem(this.userKey, JSON.stringify(data.user));  // ← USER HERE

  // Update reactive state
  this.currentUserSubject.next(data.user);  // ← REACTIVE UPDATE
}

// TWO ACCESS PATTERNS
public get currentUserValue(): User | null {
  return this.currentUserSubject.value;  // ← SYNCHRONOUS (for services)
}

public currentUser: Observable<User | null>;  // ← REACTIVE (for components)
```

**Key Insight**: Any property added to backend User model automatically flows through to frontend.

---

## Current Timezone System (Hardcoded)

### Problem: All Users See Same Timezone

```typescript
// File: src/environments/environment.ts
export const environment = {
  timezone: 'Asia/Kolkata',  // ← HARDCODED FOR ALL USERS
  dateFormat: 'dd/MM/yyyy',
  timeFormat: 'hh:mm a'
};

// File: src/app/services/date.service.ts
export class DateService {
  private readonly timezone = environment.timezone;  // ← GLOBAL TIMEZONE

  formatDate(dateString: string): string {
    return utcDate.toLocaleString('en-IN', {
      timeZone: this.timezone  // ← SAME FOR EVERYONE
    });
  }
}
```

**Impact**:
- User in New York sees dates in India time
- User in London sees dates in India time
- Confusing and unprofessional

---

## Solution: User-Specific Timezone

### Step 1: Update User Interface (2 minutes)

```typescript
// File: src/app/models/user.model.ts

export interface User {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  jobTitle?: string;
  companyId: string;
  companyName: string;
  branchId?: string;
  departmentId?: string;
  sectionId?: string;
  employeeTypeId?: string;
  managerId?: string;
  roles: UserRole[];
  permissions: string[];
  isActive?: boolean;

  // ADD THESE TWO LINES ↓
  timezone?: string;  // IANA timezone (e.g., "America/New_York")
  locale?: string;    // BCP 47 locale (e.g., "en-US")
}

export interface LoginRequest {
  email: string;
  password: string;
  timezone?: string;  // ADD THIS LINE
}
```

**Critical**:
- Fields MUST be optional (`?`) for backward compatibility
- Use IANA timezone format (e.g., "America/New_York", not "EST")

---

### Step 2: Detect Browser Timezone on Login (5 minutes)

```typescript
// File: src/app/components/login/login.ts

export class LoginComponent implements OnInit, AfterViewInit {
  // ... existing code ...

  // ADD THIS METHOD ↓
  private detectBrowserTimezone(): string {
    try {
      const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone;
      if (!timezone || timezone === '') {
        console.warn('Browser did not return timezone, using UTC');
        return 'UTC';
      }
      console.log('Detected browser timezone:', timezone);
      return timezone;
    } catch (error) {
      console.error('Error detecting browser timezone:', error);
      return 'UTC';
    }
  }

  // MODIFY THIS METHOD ↓
  onSubmit(): void {
    if (this.loginForm.invalid) {
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    // CHANGE THIS SECTION ↓
    const credentials = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password,
      timezone: this.detectBrowserTimezone()  // ← ADD THIS LINE
    };

    this.authService.login(credentials).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.handleRememberMe();
          this.router.navigate([this.returnUrl]);
        } else {
          this.errorMessage = response.message || 'Login failed. Please check your credentials and try again.';
          this.loading = false;
          this.cdr.detectChanges();
        }
      },
      error: (error) => {
        console.error('Login error:', error);
        this.errorMessage = error.error?.message || 'An error occurred during login. Please try again.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
```

**What This Does**:
- Detects timezone from browser OS settings
- Sends timezone to backend with login credentials
- Backend stores timezone in database
- Backend returns timezone in login response
- Frontend automatically stores timezone in sessionStorage

**Example Output**:
- New York user: `"America/New_York"`
- London user: `"Europe/London"`
- Mumbai user: `"Asia/Kolkata"`
- Dubai user: `"Asia/Dubai"`

---

### Step 3: Update DateService to Use User Timezone (10 minutes)

```typescript
// File: src/app/services/date.service.ts

import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';  // ← ADD THIS IMPORT

@Injectable({
  providedIn: 'root'
})
export class DateService {
  // REMOVE THIS ↓
  // private readonly timezone = environment.timezone || 'Asia/Kolkata';
  // private readonly locale = 'en-IN';

  // ADD THIS ↓
  constructor(private authService: AuthService) {}

  // ADD THESE TWO METHODS ↓
  private getTimezone(): string {
    const user = this.authService.currentUserValue;
    return user?.timezone || environment.timezone || 'UTC';
  }

  private getLocale(): string {
    const user = this.authService.currentUserValue;
    return user?.locale || 'en-US';
  }

  // MODIFY ALL FORMAT METHODS TO USE getTimezone() and getLocale()
  formatDate(dateString: string | null | undefined, includeSeconds: boolean = false): string {
    if (!dateString) return '-';

    try {
      const dateStr = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const utcDate = new Date(dateStr);

      if (isNaN(utcDate.getTime())) {
        return 'Invalid Date';
      }

      const options: Intl.DateTimeFormatOptions = {
        timeZone: this.getTimezone(),  // ← CHANGED
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
      };

      if (includeSeconds) {
        options.second = '2-digit';
      }

      return utcDate.toLocaleString(this.getLocale(), options);  // ← CHANGED
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }

  formatDateShort(dateString: string | null | undefined): string {
    if (!dateString) return '-';

    try {
      const dateStr = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const utcDate = new Date(dateStr);

      if (isNaN(utcDate.getTime())) {
        return 'Invalid Date';
      }

      return utcDate.toLocaleString(this.getLocale(), {  // ← CHANGED
        timeZone: this.getTimezone(),  // ← CHANGED
        day: 'numeric',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }

  formatTime(dateString: string | null | undefined, includeSeconds: boolean = false): string {
    if (!dateString) return '-';

    try {
      const dateStr = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const utcDate = new Date(dateStr);

      if (isNaN(utcDate.getTime())) {
        return 'Invalid Date';
      }

      const options: Intl.DateTimeFormatOptions = {
        timeZone: this.getTimezone(),  // ← CHANGED
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
      };

      if (includeSeconds) {
        options.second = '2-digit';
      }

      return utcDate.toLocaleTimeString(this.getLocale(), options);  // ← CHANGED
    } catch (error) {
      console.error('Error formatting time:', error);
      return 'Invalid Date';
    }
  }

  formatDateOnly(dateString: string | null | undefined): string {
    if (!dateString) return '-';

    try {
      const dateStr = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const utcDate = new Date(dateStr);

      if (isNaN(utcDate.getTime())) {
        return 'Invalid Date';
      }

      return utcDate.toLocaleDateString(this.getLocale(), {  // ← CHANGED
        timeZone: this.getTimezone(),  // ← CHANGED
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }

  // getRelativeTime() and isToday() methods remain mostly the same
  // but should also use this.getTimezone() and this.getLocale()
}
```

**Key Changes**:
1. Inject `AuthService` in constructor
2. Add `getTimezone()` method that reads from current user
3. Add `getLocale()` method that reads from current user
4. Replace all hardcoded `this.timezone` with `this.getTimezone()`
5. Replace all hardcoded `this.locale` with `this.getLocale()`

**Fallback Chain**:
```
user.timezone → environment.timezone → 'UTC'
user.locale → 'en-US'
```

---

### Step 4: Update UTC to Local Pipe (10 minutes)

```typescript
// File: src/app/pipes/utc-to-local.pipe.ts

import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AuthService } from '../services/auth.service';  // ← ADD THIS IMPORT

@Pipe({
  name: 'utcToLocal',
  standalone: true
})
export class UtcToLocalPipe implements PipeTransform {
  // MODIFY CONSTRUCTOR ↓
  constructor(
    private datePipe: DatePipe,
    private authService: AuthService  // ← ADD THIS
  ) {}

  transform(value: string | Date | null | undefined, format: string = 'dd/MM/yyyy hh:mm a'): string | null {
    if (!value) {
      return null;
    }

    try {
      const utcDate = typeof value === 'string' ? new Date(value) : value;

      if (isNaN(utcDate.getTime())) {
        return null;
      }

      // GET USER TIMEZONE AND LOCALE ↓
      const user = this.authService.currentUserValue;
      const timezone = user?.timezone || 'UTC';
      const locale = user?.locale || 'en-US';

      const options: Intl.DateTimeFormatOptions = {
        timeZone: timezone,  // ← CHANGED
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
      };

      if (format === 'short') {
        return utcDate.toLocaleString(locale, {  // ← CHANGED
          timeZone: timezone,  // ← CHANGED
          day: 'numeric',
          month: 'short',
          year: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        });
      } else if (format === 'medium') {
        return utcDate.toLocaleString(locale, {  // ← CHANGED
          timeZone: timezone,  // ← CHANGED
          day: 'numeric',
          month: 'short',
          year: 'numeric',
          hour: '2-digit',
          minute: '2-digit',
          second: '2-digit'
        });
      } else if (format.includes('dd/MM/yyyy')) {
        return utcDate.toLocaleString(locale, options);  // ← CHANGED
      } else {
        // For custom formats, we need to handle timezone offset manually
        // This is a limitation of DatePipe which doesn't support timeZone option
        const istTime = new Date(utcDate.getTime() + this.getTimezoneOffset(timezone));
        return this.datePipe.transform(istTime, format) || null;
      }
    } catch (error) {
      console.error('Error converting UTC date to local time:', error);
      return null;
    }
  }

  // ADD THIS HELPER METHOD ↓
  private getTimezoneOffset(timezone: string): number {
    try {
      const now = new Date();
      const utcTime = now.getTime();
      const localTime = new Date(now.toLocaleString('en-US', { timeZone: timezone })).getTime();
      return localTime - utcTime;
    } catch {
      return 0;
    }
  }
}
```

**Key Changes**:
1. Inject `AuthService` in constructor
2. Get user timezone from `authService.currentUserValue`
3. Get user locale from `authService.currentUserValue`
4. Use user-specific timezone and locale in formatting

---

## Testing Your Changes

### Test 1: Verify Login Sends Timezone

```typescript
// Open browser console during login
// You should see:
console.log('Detected browser timezone:', timezone);
// Output example: "America/New_York"
```

### Test 2: Verify Backend Stores Timezone

```sql
-- Check database after login
SELECT Id, Email, Timezone, Locale FROM Users WHERE Email = 'test@example.com';
-- Should show detected timezone
```

### Test 3: Verify Frontend Uses User Timezone

```typescript
// In any component, check:
const user = this.authService.currentUserValue;
console.log('User timezone:', user?.timezone);
// Should show user's timezone from database

// Check sessionStorage:
const storedUser = JSON.parse(sessionStorage.getItem('complaint_system_user'));
console.log('Stored user timezone:', storedUser?.timezone);
// Should match database value
```

### Test 4: Verify Date Formatting

```typescript
// In component:
const testDate = '2025-11-15T14:30:00Z';
const formatted = this.dateService.formatDate(testDate);
console.log('Formatted date:', formatted);

// Expected outputs by timezone:
// America/New_York: "11/15/2025 09:30 AM"
// Europe/London: "15/11/2025 02:30 PM"
// Asia/Kolkata: "15/11/2025 08:00 PM"
```

---

## Backend Changes Required

### Prerequisites for Frontend to Work

The backend MUST implement these changes first:

#### 1. Database Migration
```sql
ALTER TABLE Users ADD COLUMN Timezone NVARCHAR(50) NULL;
ALTER TABLE Users ADD COLUMN Locale NVARCHAR(10) NULL;
```

#### 2. User Entity Update
```csharp
public class User
{
    // ... existing properties ...
    public string? Timezone { get; set; }
    public string? Locale { get; set; }
}
```

#### 3. Login Endpoint Update
```csharp
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Timezone { get; set; }  // NEW
}

public async Task<LoginResponse> Login(LoginRequest request)
{
    var user = await _userRepository.GetByEmail(request.Email);

    // Update timezone if provided
    if (!string.IsNullOrEmpty(request.Timezone))
    {
        user.Timezone = request.Timezone;

        // Auto-detect locale from timezone
        user.Locale = DetectLocaleFromTimezone(request.Timezone);

        await _userRepository.Update(user);
    }

    // Return user with timezone in response
    return new LoginResponse
    {
        User = user,  // Includes timezone and locale
        Token = token,
        RefreshToken = refreshToken
    };
}
```

#### 4. Locale Detection Helper
```csharp
private string DetectLocaleFromTimezone(string timezone)
{
    return timezone switch
    {
        var tz when tz.StartsWith("America/") => "en-US",
        var tz when tz.StartsWith("Europe/London") => "en-GB",
        var tz when tz.StartsWith("Europe/") => "en-GB",
        var tz when tz.StartsWith("Asia/Kolkata") => "en-IN",
        var tz when tz.StartsWith("Asia/Dubai") => "en-AE",
        var tz when tz.StartsWith("Asia/") => "en-US",
        var tz when tz.StartsWith("Australia/") => "en-AU",
        _ => "en-US"
    };
}
```

---

## Common Timezone Values

### North America
- `America/New_York` - Eastern Time (US)
- `America/Chicago` - Central Time (US)
- `America/Denver` - Mountain Time (US)
- `America/Los_Angeles` - Pacific Time (US)
- `America/Phoenix` - Arizona (no DST)
- `America/Toronto` - Toronto
- `America/Vancouver` - Vancouver

### Europe
- `Europe/London` - UK (GMT/BST)
- `Europe/Paris` - Central European Time
- `Europe/Berlin` - Central European Time
- `Europe/Rome` - Central European Time
- `Europe/Madrid` - Central European Time
- `Europe/Athens` - Eastern European Time
- `Europe/Moscow` - Moscow Time

### Asia
- `Asia/Kolkata` - India Standard Time
- `Asia/Dubai` - Gulf Standard Time
- `Asia/Singapore` - Singapore Time
- `Asia/Tokyo` - Japan Standard Time
- `Asia/Shanghai` - China Standard Time
- `Asia/Hong_Kong` - Hong Kong Time
- `Asia/Bangkok` - Indochina Time

### Middle East
- `Asia/Riyadh` - Arabia Standard Time
- `Asia/Kuwait` - Arabia Standard Time
- `Asia/Qatar` - Arabia Standard Time
- `Asia/Bahrain` - Arabia Standard Time

### Australia
- `Australia/Sydney` - Australian Eastern Time
- `Australia/Melbourne` - Australian Eastern Time
- `Australia/Perth` - Australian Western Time
- `Australia/Adelaide` - Australian Central Time

### UTC
- `UTC` - Coordinated Universal Time (fallback)

---

## Troubleshooting

### Issue 1: User Timezone is Undefined
**Symptom**: `user.timezone` is undefined after login

**Causes**:
1. Backend didn't return timezone in login response
2. Backend didn't save timezone to database
3. Login request didn't include timezone

**Fix**:
```typescript
// Check what backend returned
console.log('Login response:', response);
console.log('User timezone:', response.data.user.timezone);

// Check if timezone was sent in request
console.log('Login request:', credentials);
console.log('Timezone sent:', credentials.timezone);
```

### Issue 2: Dates Still Show Wrong Timezone
**Symptom**: Dates display in wrong timezone even after login

**Causes**:
1. DateService still using `environment.timezone`
2. Pipe still using hardcoded timezone
3. User object in sessionStorage doesn't have timezone

**Fix**:
```typescript
// Check DateService
const user = this.authService.currentUserValue;
console.log('User timezone in DateService:', user?.timezone);

// Check sessionStorage
const storedUser = JSON.parse(sessionStorage.getItem('complaint_system_user'));
console.log('Stored user:', storedUser);
console.log('Stored timezone:', storedUser?.timezone);

// Check environment fallback
console.log('Environment timezone:', environment.timezone);
```

### Issue 3: Browser Timezone Detection Fails
**Symptom**: `detectBrowserTimezone()` returns 'UTC'

**Causes**:
1. Browser too old (pre-2016)
2. Browser privacy settings blocking timezone detection
3. JavaScript error in detection code

**Fix**:
```typescript
// Test manually in browser console
console.log(Intl.DateTimeFormat().resolvedOptions().timeZone);
// Should return timezone like "America/New_York"

// If undefined, fallback to UTC is correct behavior
```

---

## Performance Impact

### Minimal Performance Cost

1. **Timezone Detection**:
   - Happens ONCE on login
   - Browser API call: < 1ms
   - No ongoing cost

2. **Timezone Lookup**:
   - Synchronous getter: `authService.currentUserValue.timezone`
   - No network call, no subscription
   - Execution time: < 0.1ms

3. **Date Formatting**:
   - Intl API is browser-native and highly optimized
   - No external libraries needed
   - Comparable performance to hardcoded timezone

4. **Memory**:
   - User object: +2 string properties (~50-100 bytes)
   - No additional subscriptions
   - No memory leaks

**Conclusion**: Performance impact is negligible.

---

## Summary Checklist

### Frontend Changes
- [ ] Add `timezone?: string` and `locale?: string` to User interface
- [ ] Add `timezone?: string` to LoginRequest interface
- [ ] Add `detectBrowserTimezone()` method to LoginComponent
- [ ] Send timezone with login credentials
- [ ] Inject AuthService into DateService
- [ ] Add `getTimezone()` and `getLocale()` methods to DateService
- [ ] Update all DateService methods to use `getTimezone()` and `getLocale()`
- [ ] Inject AuthService into UtcToLocalPipe
- [ ] Update Pipe to use user timezone and locale

### Backend Changes (Prerequisites)
- [ ] Add Timezone and Locale columns to Users table
- [ ] Update User entity with timezone properties
- [ ] Accept timezone in login request
- [ ] Store timezone to database on login
- [ ] Return timezone in login response
- [ ] Implement locale detection from timezone

### Testing
- [ ] Login as user → Check browser console for detected timezone
- [ ] Check database → User record has timezone
- [ ] Check sessionStorage → User object has timezone
- [ ] Format a date → Displays in user timezone
- [ ] Login from different timezone → Timezone updates

---

## Estimated Implementation Time

- **User Model Changes**: 5 minutes
- **Login Component Update**: 10 minutes
- **DateService Refactoring**: 15 minutes
- **Pipe Update**: 10 minutes
- **Testing**: 20 minutes

**Total Frontend**: ~1 hour

**Total Backend**: ~2-3 hours (database, entity, endpoints)

**Grand Total**: 3-4 hours for complete timezone support
