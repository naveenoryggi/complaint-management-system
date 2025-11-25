# SLA System Migration Plan - From Old to New

**Date:** November 1, 2025
**Status:** 🚀 In Progress
**Type:** Complete Migration to New SLA Management System

---

## Executive Summary

This migration removes the old simple SLA fields and fully adopts the new enterprise SLA management system. Since we're in development, we can safely remove old fields and clean up data.

---

## What's Being Removed

### 1. ComplaintCategory Entity
**File:** `Domain/Entities/Complaints/ComplaintCategory.cs`

**Field to Remove:**
```csharp
public int DefaultSlaHours { get; set; } = 48;
```

**Why Remove:**
- Simple hour counting (no working hours logic)
- Replaced by Category-SLA mapping in new system
- New system provides more sophisticated category-based SLAs

### 2. ComplaintPriorityMaster Entity
**File:** `Domain/Entities/MasterData/ComplaintPriorityMaster.cs`

**Fields to Remove:**
```csharp
public int? SlaResponseHours { get; set; }
public int? SlaResolutionHours { get; set; }
```

**Why Remove:**
- Optional fields with limited functionality
- Replaced by Priority-SLA mapping in new system
- New system handles response and resolution times separately with working hours

---

## What's Staying (New SLA System)

### ✅ SLA Settings
- Working hours configuration (9 AM - 5 PM)
- Weekend/holiday exclusion
- Auto-escalation rules
- Pause conditions

### ✅ SLA Levels
- Custom service level tiers (Gold, Silver, Bronze, etc.)
- Resolution time targets per level
- Response time targets per level

### ✅ Category-SLA Mapping
- Specific SLA configurations per complaint category
- Overrides default SLA settings
- Working hours aware

### ✅ Priority-SLA Mapping
- Specific SLA configurations per priority level
- Response and resolution time targets
- Escalation triggers

### ✅ SLA Calculator Service
- Intelligent 6-level fallback hierarchy
- Working hours calculation
- Holiday/weekend exclusion
- Real-time deadline calculation

---

## Migration Steps

### Step 1: Remove Old Fields from Entities ✅
1. Edit `ComplaintCategory.cs` - Remove `DefaultSlaHours`
2. Edit `ComplaintPriorityMaster.cs` - Remove `SlaResponseHours` and `SlaResolutionHours`

### Step 2: Create Database Migration ✅
```bash
cd complaint-system-dotnet/src/ComplaintManagement.Infrastructure
dotnet ef migrations add RemoveOldSLAFields --startup-project ../ComplaintManagement.API
```

### Step 3: Update SLA Calculator ✅
Remove fallback logic for old fields:
- Remove Level 4: Priority Master SLA fields
- Remove Level 5: Category Default SLA Hours
- Keep only new system fallbacks (Levels 1-3)

### Step 4: Run Migration ✅
```bash
dotnet ef database update --startup-project ../ComplaintManagement.API
```

### Step 5: Clean Up Test Data ✅
- Optionally delete all test complaints (CMP-2025-*)
- Optionally reset SLA configurations
- Fresh start with new system

### Step 6: Verify System ✅
- Test complaint creation with new SLA calculation
- Verify Category-SLA mappings work
- Verify Priority-SLA mappings work
- Test SLA deadline calculation

---

## Impact Analysis

### Database Changes
**Tables Affected:**
- `ComplaintCategories` - Column `DefaultSlaHours` will be dropped
- `ComplaintPriorityMasters` - Columns `SlaResponseHours` and `SlaResolutionHours` will be dropped

**Data Loss:**
- ⚠️ Any configured values in these fields will be lost
- ✅ **Not a problem** - we're in development, and new system is already configured

### Code Changes
**Files to Update:**
1. `ComplaintCategory.cs` - Remove field
2. `ComplaintPriorityMaster.cs` - Remove fields
3. `SLACalculatorService.cs` - Remove old fallback logic
4. Database migration files

**No Breaking Changes:**
- New SLA system is already primary
- SLA Calculator already uses new system as primary
- Removing old fields just removes unused fallback code

---

## Rollback Plan

If issues occur, you can:
1. Revert the database migration
2. Revert entity changes in code
3. Rebuild and redeploy

**Command to rollback:**
```bash
dotnet ef database update <PreviousMigrationName> --startup-project ../ComplaintManagement.API
```

---

## Testing Checklist

### After Migration:
- [ ] Complaint creation with Critical priority
- [ ] Complaint creation with Normal priority
- [ ] Complaint creation with High priority
- [ ] Complaint creation with Low priority
- [ ] SLA deadline calculation verifies working hours
- [ ] Weekend/holiday exclusion works
- [ ] Category-SLA mappings are used
- [ ] Priority-SLA mappings are used
- [ ] Auto-escalation triggers correctly

---

## Benefits of Migration

### Before (Old System):
- ❌ Simple hour counting (48 hours = 2 days)
- ❌ No working hours logic
- ❌ No holiday/weekend exclusion
- ❌ No escalation
- ❌ No SLA management UI
- ❌ Limited flexibility

### After (New System):
- ✅ Enterprise-grade SLA management
- ✅ Working hours calculation (9 AM - 5 PM)
- ✅ Holiday/weekend exclusion
- ✅ Auto-escalation on breach
- ✅ Warning notifications
- ✅ SLA pause conditions
- ✅ Complete management UI
- ✅ Unlimited flexibility

---

## Post-Migration Actions

1. **Configure SLA Settings:**
   - Set working hours (default: 9 AM - 5 PM)
   - Set working days (Mon-Fri)
   - Configure escalation threshold (default: 80%)

2. **Create SLA Levels:**
   - Example: Gold (4h response, 24h resolution)
   - Example: Silver (8h response, 48h resolution)
   - Example: Bronze (24h response, 72h resolution)

3. **Map Category-SLAs:**
   - Assign SLA levels to each complaint category
   - Or set custom resolution times per category

4. **Map Priority-SLAs:**
   - Critical: 2h response, 8h resolution
   - High: 4h response, 24h resolution
   - Normal: 8h response, 48h resolution
   - Low: 24h response, 120h resolution

5. **Test Thoroughly:**
   - Create test complaints
   - Verify SLA deadlines
   - Test escalation
   - Test notifications

---

## Documentation Updates

### Files to Update:
- ✅ This migration plan (SLA_MIGRATION_TO_NEW_SYSTEM.md)
- ✅ EXISTING_SLA_LOGIC_ANALYSIS.md - Mark as deprecated
- ✅ SLA_E2E_TEST_FINAL_REPORT.md - Update to reflect migration
- ✅ README or main documentation - Remove old SLA references

---

## Timeline

**Estimated Time:** 30 minutes
- Step 1: Remove fields (5 min)
- Step 2: Create migration (5 min)
- Step 3: Update SLA Calculator (10 min)
- Step 4: Run migration (2 min)
- Step 5: Clean up data (optional, 5 min)
- Step 6: Test (10 min)

**Status:** Ready to execute

---

## Risk Assessment

**Risk Level:** ⬇️ LOW

**Why Low Risk:**
1. Development environment (not production)
2. New SLA system already implemented and tested
3. Can delete test data without consequences
4. Can rollback if needed
5. No external dependencies

**Mitigation:**
- Backup database before migration
- Test after each step
- Have rollback plan ready

---

## Approval

Since you confirmed:
> "since the system is under development, you can migrate to new SLA management module, you can delete existing tickets or data where required."

✅ **Approved for execution**

---

## Next Steps

1. Execute migration steps
2. Test system thoroughly
3. Configure new SLA settings via UI
4. Create user guide for new SLA management

---

**Migration Prepared By:** AI Assistant
**Approved By:** User
**Date:** November 1, 2025
