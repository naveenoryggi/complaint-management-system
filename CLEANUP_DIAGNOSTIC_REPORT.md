# Cleanup Button Diagnostic Report

## Issue Summary
User clicks cleanup button on IN_PROGRESS sync, but status remains IN_PROGRESS instead of changing to FAILED.

## Root Cause Analysis

### Critical Finding: Dual ID Properties
The `SyncLog` entity has TWO identifier properties:

1. **`Id`** (inherited from `BaseEntity`)
   - Type: `Guid`
   - Likely the actual PRIMARY KEY in database

2. **`SyncLogId`** (defined in `SyncLog`)
   - Type: `Guid`
   - Custom property, possibly just a regular column

### Code Analysis

**API History Endpoint** (OryggiSyncController.cs:127)
```csharp
syncLogId = s.SyncLogId  // Returns SyncLogId to frontend
```

**Cleanup Method** (SqlDiagnosticsService.cs:321-322)
```csharp
var sync = await _context.SyncLogs
    .Where(s => s.SyncLogId == syncLogId && !s.IsDeleted)  // Queries by SyncLogId
    .FirstOrDefaultAsync();
```

**Frontend Component** (oryggi-sync.component.ts:597)
```typescript
this.syncService.cleanupSingleSync(syncLogId)  // Passes syncLogId from table
```

## Potential Issues

### Scenario 1: SyncLogId is NOT the Primary Key
- Database has `Id` as PK, `SyncLogId` as regular column
- Frontend passes `SyncLogId` value
- Backend searches WHERE `SyncLogId` = value
- If `SyncLogId` values are not populated or don't match, query returns null
- Cleanup fails silently

### Scenario 2: Guid Mismatch
- `SyncLogId` and `Id` contain different Guid values
- Frontend shows `SyncLogId` in table
- User clicks cleanup with that `SyncLogId`
- Backend finds record but `Id` is different
- Update might fail due to concurrency or tracking issues

## Recommended Fixes

### Option 1: Use Id as Primary Key (Recommended)
Modify all code to use `Id` instead of `SyncLogId`:

1. **Update History Endpoint**
   ```csharp
   syncLogId = s.Id  // Change to use Id
   ```

2. **Update Cleanup Method**
   ```csharp
   var sync = await _context.SyncLogs
       .Where(s => s.Id == syncLogId && !s.IsDeleted)  // Use Id
       .FirstOrDefaultAsync();
   ```

### Option 2: Configure SyncLogId as Primary Key
Add EF Core configuration:

```csharp
// In DbContext OnModelCreating
modelBuilder.Entity<SyncLog>()
    .HasKey(s => s.SyncLogId);

modelBuilder.Entity<SyncLog>()
    .Ignore(s => s.Id);  // Ignore inherited Id property
```

### Option 3: Diagnostic Endpoint (For Testing)
Add temporary endpoint to verify schema:

```csharp
[HttpGet("diagnostics/sync-schema/{syncLogId}")]
public async Task<IActionResult> GetSyncSchema(Guid syncLogId)
{
    var sync = await _context.SyncLogs
        .Where(s => !s.IsDeleted)
        .Select(s => new {
            s.Id,
            s.SyncLogId,
            s.Status,
            s.SyncType,
            Match = s.SyncLogId == syncLogId ? "MATCH" : "NO MATCH"
        })
        .Take(10)
        .ToListAsync();

    return Ok(new { records = sync });
}
```

## Testing Steps

### 1. Verify Database Schema
Run SQL query to check actual primary key:
```sql
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SyncLogs'
ORDER BY ORDINAL_POSITION;

SELECT
    CONSTRAINT_NAME,
    COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_NAME = 'SyncLogs'
    AND CONSTRAINT_NAME LIKE 'PK%';
```

### 2. Check Data Values
```sql
SELECT TOP 5
    Id,
    SyncLogId,
    Status,
    SyncType,
    SyncStartedAt,
    CASE WHEN Id = SyncLogId THEN 'SAME' ELSE 'DIFFERENT' END AS IdComparison
FROM SyncLogs
WHERE Status = 'IN_PROGRESS'
ORDER BY SyncStartedAt DESC;
```

### 3. Test Cleanup with Known ID
1. Get a sync record from the table
2. Note both `Id` and `SyncLogId` values
3. Try cleanup with `SyncLogId`
4. Try cleanup with `Id`
5. Compare results

## Next Steps

1. ✅ **Identify actual primary key in database**
2. ✅ **Verify Id vs SyncLogId values**
3. ✅ **Apply appropriate fix (Option 1 or 2)**
4. ✅ **Test cleanup functionality**
5. ✅ **Update all related code consistently**

## Files to Modify (if using Option 1)

1. `OryggiSyncController.cs` - Line 127 (history endpoint)
2. `SqlDiagnosticsService.cs` - Lines 321-322 (cleanup query)
3. No frontend changes needed (just pass the value from API)

## Current Status
- ⏸️ Awaiting database schema verification
- ⏸️ Awaiting user testing with specific syncLogId
- ⏸️ Ready to implement fix once issue confirmed

---
Generated: 2025-10-19
Location: C:\Users\Navin Chandra\Pictures\Complaint management system\
