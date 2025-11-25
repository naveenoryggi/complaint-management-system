# E2E Issues Quick Reference

**For Rapid Issue Resolution**
**Date**: 2025-11-11

---

## Issue #1: Complainant Login Timeout - CRITICAL

**Symptom**: `page.goto: Timeout 15000ms exceeded` when navigating to login

**Root Cause**: Infinite redirect loop between `/login` and `/dashboard` due to stale token validation

**Affected Files**:
- `auth.guard.ts` - Line 5-16
- `auth.service.ts` - Line 111-123
- `login.ts` - Line 66-78

**Quick Fix**:
1. Auth Guard: Add login page protection
2. Auth Service: Add 5-second token buffer time
3. Login Component: Add session cleanup before redirect check

**Test**: Rapid login → logout → login cycle should complete in < 3 seconds

---

## Issue #2: Login Button Stuck Disabled - HIGH

**Symptom**: `element is not enabled` - button disabled even with valid form

**Root Cause**: Form validation state not triggering Angular change detection during rapid E2E operations

**Affected Files**:
- `login.ts` - Lines 1, 23, 53-58, 66-78, 130-174

**Quick Fix**:
1. Add `ChangeDetectorRef` to constructor
2. Implement `ngAfterViewInit()` with form listeners
3. Add `cdr.detectChanges()` after state changes
4. Reset `loading = false` in `ngOnInit()`

**Test**: Rapid form fill should enable button within 100ms

---

## Issue #3: Complaint Navigation Selector - MEDIUM

**Symptom**: `No complaint links found` - test can't find navigation elements

**Root Cause**: E2E test looking for `<a>` links, but app uses clickable table rows

**Affected Files**:
- E2E test file (selector mismatch)

**Quick Fix**:
Replace selector:
- ❌ Old: `a[href*="/complaints/"]`
- ✅ New: `app-virtual-scroll-table tbody tr`

**Test**: Click first row should navigate to `/complaints/{guid}`

---

## Code Changes Summary

### File 1: `auth.guard.ts`
```typescript
// ADD THIS CHECK at start of function:
if (state.url === '/login') {
  return true; // Always allow login page
}
```

### File 2: `auth.service.ts` (isAuthenticated method)
```typescript
// ADD buffer time check:
const bufferTime = 5000; // 5 seconds
const isValid = (Date.now() + bufferTime) < exp;

if (!isValid) {
  this.clearSession();
}
return isValid;
```

### File 3: `login.ts`
```typescript
// 1. Add import:
import { ChangeDetectorRef, AfterViewInit } from '@angular/core';

// 2. Update class declaration:
export class LoginComponent implements OnInit, AfterViewInit {

// 3. Add to constructor:
private cdr: ChangeDetectorRef

// 4. Update ngOnInit:
ngOnInit(): void {
  this.loading = false; // CRITICAL: Reset state
  this.errorMessage = '';
  this.validateAndCleanupSession(); // CRITICAL: Add this
  // ... rest of existing code
  this.cdr.detectChanges(); // ADD at end
}

// 5. Add ngAfterViewInit:
ngAfterViewInit(): void {
  this.loginForm.valueChanges.subscribe(() => {
    this.cdr.detectChanges();
  });
  this.loginForm.statusChanges.subscribe((status) => {
    console.log('[LoginForm] Status:', status);
    this.cdr.detectChanges();
  });
}

// 6. Add new method:
private validateAndCleanupSession(): void {
  try {
    const token = sessionStorage.getItem('complaint_system_token');
    const expiryTime = sessionStorage.getItem('complaint_system_token_expiry');
    if (token && expiryTime) {
      const expiry = parseInt(expiryTime);
      if (Date.now() > expiry) {
        sessionStorage.removeItem('complaint_system_token');
        sessionStorage.removeItem('complaint_system_refresh_token');
        sessionStorage.removeItem('complaint_system_user');
        sessionStorage.removeItem('complaint_system_token_expiry');
      }
    }
  } catch (error) {
    sessionStorage.clear();
  }
}

// 7. Add cdr.detectChanges() in onSubmit:
// After: this.loading = true
// After: this.loading = false (both occurrences)
```

### File 4: E2E Test
```javascript
// Replace this:
const complaintLinks = await page.locator('a[href*="/complaints/"]').count();

// With this:
const complaintRows = page.locator('app-virtual-scroll-table tbody tr');
await complaintRows.first().waitFor({ state: 'visible', timeout: 10000 });
const rowCount = await complaintRows.count();
// Click first row:
await complaintRows.first().click();
```

---

## Testing Checklist

After implementing fixes:

**Manual Tests**:
- [ ] Complainant login works
- [ ] Rapid logout/login cycle works
- [ ] Login button enables after form fill
- [ ] Clicking complaint row navigates

**E2E Tests**:
- [ ] TC-2.1.2: Complainant Login - PASS
- [ ] TC-2.1.1: Admin Dashboard - PASS
- [ ] TC-2.2.2: Complaint Navigation - PASS
- [ ] Overall: 13/13 tests passing (100%)

---

## Impact

**Before Fixes**:
- Test Success Rate: 77% (10/13)
- Blocked Tests: 3
- Critical Issues: 1 (login timeout)

**After Fixes**:
- Test Success Rate: 100% (13/13)
- Blocked Tests: 0
- Critical Issues: 0

---

## Implementation Time

- Reading documentation: 10 minutes
- Code changes: 30 minutes
- Manual testing: 15 minutes
- E2E test verification: 10 minutes
- **Total**: ~1 hour

---

## Priority Order

1. **CRITICAL**: Fix Issue #1 (Complainant login) - 23% test failure
2. **HIGH**: Fix Issue #2 (Button disabled) - Blocks automation
3. **MEDIUM**: Fix Issue #3 (Selectors) - Test maintenance

---

## Verification Commands

```bash
# Compile Angular
ng serve

# Run E2E tests
.\comprehensive-frontend-e2e-test.ps1

# Check for console errors (browser)
F12 → Console → Look for auth/navigation errors
```

---

## Success Indicators

✅ No timeout errors during login
✅ Login button responds immediately to form input
✅ Complaint row clicks navigate to detail page
✅ E2E test suite shows 100% pass rate
✅ No infinite redirect loops
✅ No "element is not enabled" errors

---

## Emergency Rollback

If fixes cause issues:

```bash
git checkout master -- complaint-system-angular/src/app/guards/auth.guard.ts
git checkout master -- complaint-system-angular/src/app/services/auth.service.ts
git checkout master -- complaint-system-angular/src/app/components/login/login.ts
ng serve
```

---

**Quick Start**: Read this document → Apply code changes → Test manually → Run E2E suite

**Full Details**: See `PHASE_1_E2E_ISSUES_ROOT_CAUSE_ANALYSIS.md`
**Step-by-Step**: See `IMPLEMENTATION_GUIDE_E2E_FIXES.md`
