# NOTIFICATION SYSTEM - QUICK FIX GUIDE

## Critical Issues Found

Two critical API URL mismatches were identified that prevent the notification system from working.

---

## Fix #1: Template Service API URL

**File:** `complaint-system-angular/src/app/services/template.service.ts`
**Line:** 24

### Change:
```typescript
// BEFORE (WRONG):
private apiUrl = `${environment.apiUrl}/communication/templates`;

// AFTER (CORRECT):
private apiUrl = `${environment.apiUrl}/communication-templates`;
```

---

## Fix #2: Notification Rule Service API URLs

**File:** `complaint-system-angular/src/app/services/notification-rule.service.ts`
**Lines:** 24-25

### Change:
```typescript
// BEFORE (WRONG):
private apiUrl = `${environment.apiUrl}/communication/notification-rules`;
private eventTypesUrl = `${environment.apiUrl}/communication/event-types`;

// AFTER (CORRECT):
private apiUrl = `${environment.apiUrl}/event-communication-rules`;
private eventTypesUrl = `${environment.apiUrl}/event-types`;
```

---

## How to Apply Fixes

1. Open `template.service.ts` in your code editor
2. Change line 24 as shown above
3. Open `notification-rule.service.ts` in your code editor  
4. Change lines 24-25 as shown above
5. Save both files
6. Rebuild Angular: `ng build` or restart `ng serve`

---

## Verification Steps

After applying fixes:

1. Open browser to http://localhost:4200
2. Login as admin
3. Navigate to Event Type Management - should load
4. Navigate to Template Management - should load (was broken)
5. Navigate to Notification Rule Management - should load (was broken)
6. Check browser console - should have NO 404 errors
7. Try creating a template - should work
8. Try creating a notification rule - should work

---

## Expected Results

- All management pages load successfully
- No 404 errors in browser console
- CRUD operations work on all three modules
- Dropdowns populate correctly with data

---

**Time to Fix:** 5 minutes
**Time to Test:** 15 minutes
**Impact:** CRITICAL - System will not work without these fixes
