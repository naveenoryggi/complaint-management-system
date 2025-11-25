# Database Migration Guide

Complete guide for managing database migrations in the Complaint Management System.

## Table of Contents
1. [Overview](#overview)
2. [Pre-Migration Preparation](#pre-migration-preparation)
3. [Migration Execution](#migration-execution)
4. [Post-Migration Verification](#post-migration-verification)
5. [Common Migration Scenarios](#common-migration-scenarios)
6. [Troubleshooting](#troubleshooting)
7. [Rollback Procedures](#rollback-procedures)

---

## Overview

The Complaint Management System uses Entity Framework Core Code-First migrations for database schema management. All migrations are tracked in the `__EFMigrationsHistory` table.

### Current Migrations (as of November 15, 2025)

1. `20251111171543_AddEmailTicketingSystem` - Email ticketing and OAuth support
2. `20251111193501_AddOAuthToEmailConfiguration` - OAuth configuration enhancements
3. `20251114130515_AddEmailThreadingSupport` - Email threading and conversation tracking
4. `20251114190307_AddEmailThreadingAndVisualIndicators` - Visual indicators for email status
5. `20251115080006_AddTimezoneAndLocalizationFields` - Timezone and localization support

---

## Pre-Migration Preparation

### 1. Backup Current Database

**CRITICAL:** Always backup before running migrations in production!

#### SQL Server Backup

```sql
-- Full backup
BACKUP DATABASE ComplaintManagementDb
TO DISK = 'C:\Backups\ComplaintDb_Pre_Migration_20251115.bak'
WITH FORMAT, INIT, NAME = 'Pre-Migration Full Backup', COMPRESSION;

-- Verify backup
RESTORE VERIFYONLY
FROM DISK = 'C:\Backups\ComplaintDb_Pre_Migration_20251115.bak';
```

#### Using SQL Server Management Studio

```
1. Right-click database > Tasks > Back Up
2. Select "Full" backup type
3. Choose disk destination
4. Set backup name: ComplaintDb_Pre_Migration_YYYYMMDD
5. Click OK
6. Verify completion
```

### 2. Document Current State

```sql
-- Count records in key tables
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users WHERE IsDeleted = 0
UNION ALL
SELECT 'Complaints', COUNT(*) FROM Complaints WHERE IsDeleted = 0
UNION ALL
SELECT 'Comments', COUNT(*) FROM Comments WHERE IsDeleted = 0
UNION ALL
SELECT 'Attachments', COUNT(*) FROM ComplaintAttachments WHERE IsDeleted = 0
UNION ALL
SELECT 'EmailMessages', COUNT(*) FROM EmailMessages WHERE IsDeleted = 0;

-- Check applied migrations
SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId;

-- Check database size
EXEC sp_spaceused;
```

Save results for comparison after migration.

### 3. Check Disk Space

```sql
-- Check available disk space
EXEC xp_fixeddrives;

-- Ensure at least 2x current database size is available
```

### 4. Schedule Maintenance Window

For production environments:
- Schedule during low-traffic hours (e.g., 2 AM - 4 AM)
- Notify users of planned downtime
- Prepare rollback plan
- Have DBA on standby

---

## Migration Execution

### Method 1: Using EF Core CLI (Recommended for Development/Staging)

#### Generate Migration Script

```bash
# Navigate to API project
cd complaint-system-dotnet/src/ComplaintManagement.API

# Generate idempotent script (safe to run multiple times)
dotnet ef migrations script --output migration-script.sql --idempotent --environment Production

# Generate script for specific migration range
dotnet ef migrations script FromMigration ToMigration --output migration-script.sql
```

#### Apply Migrations Directly

```bash
# Apply all pending migrations
dotnet ef database update --environment Production

# Apply to specific migration
dotnet ef database update 20251115080006_AddTimezoneAndLocalizationFields --environment Production
```

### Method 2: Using SQL Script (Recommended for Production)

#### Step 1: Generate Script

```bash
dotnet ef migrations script --output migration-script.sql --idempotent --environment Production
```

#### Step 2: Review Script

Open `migration-script.sql` and review:
- All DDL statements (CREATE, ALTER, DROP)
- Data migration statements (INSERT, UPDATE)
- Index creation statements
- Foreign key constraints

#### Step 3: Test on Staging

```sql
-- Restore production backup to staging
RESTORE DATABASE ComplaintManagementDb_Staging
FROM DISK = 'C:\Backups\ComplaintDb_Production.bak'
WITH MOVE 'ComplaintManagementDb' TO 'C:\SQL\Data\ComplaintDb_Staging.mdf',
     MOVE 'ComplaintManagementDb_log' TO 'C:\SQL\Logs\ComplaintDb_Staging_log.ldf',
     REPLACE;

-- Run migration script on staging
USE ComplaintManagementDb_Staging;
GO
-- Execute migration-script.sql
```

#### Step 4: Execute on Production

```sql
-- Stop application first to prevent connection issues
-- Run in SSMS with careful monitoring

USE ComplaintManagementDb;
GO

-- Execute migration script
-- (Copy content of migration-script.sql)

-- Verify completion
SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC;
```

### Method 3: Using Azure Data Studio

```
1. Open Azure Data Studio
2. Connect to database server
3. Open migration-script.sql
4. Review query plan
5. Execute query
6. Monitor progress in Messages tab
7. Verify completion
```

---

## Post-Migration Verification

### 1. Verify Migration History

```sql
-- Check latest migrations applied
SELECT TOP 5 MigrationId, ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId DESC;

-- Expected: Latest migration appears at top
```

### 2. Verify Schema Changes

```sql
-- Check new tables exist
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_NAME IN ('EmailMessages', 'EmailAttachments', 'EmailConfigurations',
                    'EmailResponseHistory', 'ComplaintEmailParticipants')
ORDER BY TABLE_NAME;

-- Check new columns exist
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_NAME = 'Users'
AND COLUMN_NAME IN ('PreferredTimeZone', 'PreferredLocale', 'PreferredDateFormat')
ORDER BY TABLE_NAME, COLUMN_NAME;
```

### 3. Verify Data Integrity

```sql
-- Check record counts (compare with pre-migration counts)
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users WHERE IsDeleted = 0
UNION ALL
SELECT 'Complaints', COUNT(*) FROM Complaints WHERE IsDeleted = 0
UNION ALL
SELECT 'Comments', COUNT(*) FROM Comments WHERE IsDeleted = 0;

-- Verify foreign key constraints
SELECT
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fc
    ON fk.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) IN ('EmailMessages', 'EmailAttachments', 'Complaints')
ORDER BY TableName;
```

### 4. Run Data Validation Queries

```sql
-- Check for null values in required fields
SELECT 'Complaints with null SubmittedAt' AS Issue, COUNT(*) AS Count
FROM Complaints WHERE SubmittedAt IS NULL OR SubmittedAt = '0001-01-01'
UNION ALL
SELECT 'Users with null Email', COUNT(*)
FROM Users WHERE Email IS NULL OR Email = ''
UNION ALL
SELECT 'Complaints with invalid StatusMasterId', COUNT(*)
FROM Complaints c
LEFT JOIN ComplaintStatusMasters s ON c.StatusMasterId = s.Id
WHERE s.Id IS NULL;

-- Verify data types
SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('Complaints', 'Users', 'EmailMessages')
ORDER BY TABLE_NAME, ORDINAL_POSITION;
```

### 5. Test Application Connectivity

```bash
# Test API health endpoint
curl https://api.your-domain.com/api/health/database

# Expected response:
# {
#   "status": "Healthy",
#   "database": "Connected",
#   "migrations": "Applied"
# }
```

### 6. Verify Seed Data

```sql
-- Check system roles exist
SELECT Code, Name, IsSystemRole FROM ComplaintRoles
WHERE IsSystemRole = 1 AND IsDeleted = 0
ORDER BY DisplayOrder;
-- Expected: Admin, Handler, Complainant

-- Check event types exist
SELECT Code, Name FROM EventTypes
WHERE IsDeleted = 0
ORDER BY Name;
-- Expected: COMPLAINT_CREATED, COMPLAINT_ASSIGNED, etc.

-- Check communication templates exist
SELECT Code, Name, IsSystemTemplate FROM CommunicationTemplates
WHERE IsSystemTemplate = 1 AND IsDeleted = 0
ORDER BY Name;
-- Expected: System templates for notifications
```

---

## Common Migration Scenarios

### Scenario 1: Adding New Entity

Example: Adding a new `KnowledgeBaseArticle` entity

**Migration File Structure:**
```csharp
public partial class AddKnowledgeBase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "KnowledgeBaseArticles",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Title = table.Column<string>(maxLength: 200, nullable: false),
                Content = table.Column<string>(nullable: false),
                CategoryId = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: true),
                IsDeleted = table.Column<bool>(nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_KnowledgeBaseArticles", x => x.Id);
                table.ForeignKey(
                    name: "FK_KnowledgeBaseArticles_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_KnowledgeBaseArticles_CategoryId",
            table: "KnowledgeBaseArticles",
            column: "CategoryId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "KnowledgeBaseArticles");
    }
}
```

### Scenario 2: Adding Column to Existing Table

Example: Adding `LastModifiedBy` to `Complaints`

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "LastModifiedBy",
        table: "Complaints",
        maxLength: 255,
        nullable: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "LastModifiedBy",
        table: "Complaints");
}
```

### Scenario 3: Data Migration

Example: Migrating old priority values to new master table

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Step 1: Add new column
    migrationBuilder.AddColumn<Guid>(
        name: "PriorityMasterId",
        table: "Complaints",
        nullable: true); // Nullable initially

    // Step 2: Migrate data
    migrationBuilder.Sql(@"
        UPDATE c
        SET c.PriorityMasterId = p.Id
        FROM Complaints c
        INNER JOIN PriorityMasters p ON c.Priority = p.Name
        WHERE c.PriorityMasterId IS NULL
    ");

    // Step 3: Make column required
    migrationBuilder.AlterColumn<Guid>(
        name: "PriorityMasterId",
        table: "Complaints",
        nullable: false);

    // Step 4: Drop old column
    migrationBuilder.DropColumn(
        name: "Priority",
        table: "Complaints");
}
```

### Scenario 4: Renaming Column

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "OldColumnName",
        table: "TableName",
        newName: "NewColumnName");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "NewColumnName",
        table: "TableName",
        newName: "OldColumnName");
}
```

---

## Troubleshooting

### Issue: Migration Fails with Foreign Key Violation

**Error:** `The ALTER TABLE statement conflicted with the FOREIGN KEY constraint`

**Solution:**
```sql
-- Temporarily disable foreign key checks
ALTER TABLE TableName NOCHECK CONSTRAINT ALL;

-- Run migration
-- (Execute migration script)

-- Re-enable foreign key checks
ALTER TABLE TableName WITH CHECK CHECK CONSTRAINT ALL;
```

### Issue: Migration Timeout

**Error:** `Execution Timeout Expired`

**Solution:**
```bash
# Increase command timeout in connection string
"DefaultConnection": "Server=...;Command Timeout=300;"

# Or split migration into smaller batches
```

### Issue: Duplicate Key Error

**Error:** `Cannot insert duplicate key`

**Solution:**
```sql
-- Find duplicates
SELECT ColumnName, COUNT(*)
FROM TableName
GROUP BY ColumnName
HAVING COUNT(*) > 1;

-- Resolve duplicates before migration
-- Then rerun migration
```

### Issue: Migration Already Applied

**Error:** `Migration '20251115080006_AddTimezone...' has already been applied`

**Solution:**
```bash
# Generate script excluding already-applied migrations
dotnet ef migrations script LastAppliedMigration --idempotent

# Or remove from history and reapply
DELETE FROM __EFMigrationsHistory WHERE MigrationId = '20251115080006_AddTimezone...';
```

---

## Rollback Procedures

### Option 1: Rollback to Previous Migration

```bash
# List all migrations
dotnet ef migrations list

# Rollback to specific migration
dotnet ef database update 20251114190307_AddEmailThreadingAndVisualIndicators --environment Production
```

### Option 2: Restore from Backup

```sql
-- Stop application first

-- Restore database
RESTORE DATABASE ComplaintManagementDb
FROM DISK = 'C:\Backups\ComplaintDb_Pre_Migration_20251115.bak'
WITH REPLACE, RECOVERY;

-- Verify restoration
SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC;
```

### Option 3: Generate Rollback Script

```bash
# Generate rollback from current to previous
dotnet ef migrations script CurrentMigration PreviousMigration --output rollback-script.sql

# Review and execute rollback-script.sql
```

---

## Best Practices

### 1. Always Test on Staging First
- Never run untested migrations on production
- Maintain staging database that mirrors production
- Test migration with production-like data volume

### 2. Use Idempotent Scripts
```bash
# Always use --idempotent flag
dotnet ef migrations script --idempotent
```
This ensures script can run multiple times safely.

### 3. Include Rollback in Every Migration
- Every `Up()` should have corresponding `Down()`
- Test rollback before deploying

### 4. Document Data Migrations
```csharp
// Always add comments for complex data migrations
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Migrate priority from string to master table reference
    // Step 1: Add new column...
    // Step 2: Copy data...
    // Step 3: Verify data integrity...
}
```

### 5. Monitor Long-Running Migrations
- Log migration start/end times
- Monitor SQL Server activity during migration
- Have rollback plan ready

### 6. Backup Before and After
```sql
-- Before migration
BACKUP DATABASE ComplaintManagementDb TO DISK = '..._PreMigration.bak';

-- After successful migration
BACKUP DATABASE ComplaintManagementDb TO DISK = '..._PostMigration.bak';
```

---

## Migration History Reference

### Migration Timeline

| Date | Migration | Description | Breaking Changes |
|------|-----------|-------------|------------------|
| 2025-11-11 | AddEmailTicketingSystem | Email ticketing foundation | None |
| 2025-11-11 | AddOAuthToEmailConfiguration | OAuth support | None |
| 2025-11-14 | AddEmailThreadingSupport | Email threading | None |
| 2025-11-14 | AddEmailThreadingAndVisualIndicators | Visual indicators | None |
| 2025-11-15 | AddTimezoneAndLocalizationFields | Timezone support | None |

### Schema Version Compatibility

| Application Version | Required Migration | Database Version |
|---------------------|-------------------|------------------|
| 1.0.0 | Initial | Base Schema |
| 1.1.0 | AddEmailTicketingSystem | Email Support |
| 1.2.0 | AddTimezoneAndLocalizationFields | Timezone Support |

---

## SQL Maintenance Scripts

### Rebuild Indexes

```sql
-- Rebuild all indexes (run monthly)
USE ComplaintManagementDb;
GO

DECLARE @TableName VARCHAR(255);
DECLARE TableCursor CURSOR FOR
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

OPEN TableCursor;
FETCH NEXT FROM TableCursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Rebuilding indexes for ' + @TableName;
    EXEC('ALTER INDEX ALL ON ' + @TableName + ' REBUILD');
    FETCH NEXT FROM TableCursor INTO @TableName;
END

CLOSE TableCursor;
DEALLOCATE TableCursor;
```

### Update Statistics

```sql
-- Update statistics (run weekly)
EXEC sp_updatestats;
```

### Check Database Integrity

```sql
-- Run monthly
DBCC CHECKDB (ComplaintManagementDb) WITH NO_INFOMSGS;
```

---

**Document Version:** 1.0
**Last Updated:** November 15, 2025
**Next Review:** February 15, 2026
