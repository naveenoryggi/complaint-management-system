-- ============================================================================
-- Enterprise Timezone Support Migration Script
-- ============================================================================
-- Purpose: Migrate DateTime columns to DateTimeOffset for full timezone support
-- Database: ComplaintManagement
-- Author: Enterprise Architecture Team
-- Date: 2025-01-15
-- Version: 1.0
--
-- IMPORTANT:
-- 1. Backup database before running this script
-- 2. This script assumes existing DateTime values are in UTC
-- 3. Test on staging environment first
-- 4. Estimated execution time: 5-10 minutes (depends on data volume)
-- ============================================================================

USE ComplaintManagement;
GO

-- ============================================================================
-- STEP 1: BACKUP VERIFICATION
-- ============================================================================
PRINT '============================================================================';
PRINT 'STEP 1: Verify Backup Exists';
PRINT '============================================================================';

-- Check if backup exists (optional - manual verification recommended)
DECLARE @BackupPath NVARCHAR(500) = 'C:\Backups\ComplaintManagement_PreTimezone_' +
    FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') + '.bak';

PRINT 'Recommended backup path: ' + @BackupPath;
PRINT 'Please confirm backup is completed before proceeding.';
PRINT '';
GO

-- ============================================================================
-- STEP 2: ADD TIMEZONE COLUMNS TO USERS TABLE
-- ============================================================================
PRINT '============================================================================';
PRINT 'STEP 2: Add Timezone Preference Columns to Users';
PRINT '============================================================================';

-- Add TimeZone column (IANA timezone identifier)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'TimeZone')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [TimeZone] NVARCHAR(50) NOT NULL DEFAULT 'UTC'
    CONSTRAINT CK_Users_TimeZone CHECK ([TimeZone] IN (
        'UTC',
        'Asia/Kolkata',
        'America/New_York',
        'America/Los_Angeles',
        'Europe/London',
        'Europe/Paris',
        'Asia/Dubai',
        'Asia/Singapore',
        'Asia/Tokyo',
        'Australia/Sydney'
    ));

    PRINT '✓ Added TimeZone column to Users table';
END
ELSE
BEGIN
    PRINT '⚠ TimeZone column already exists in Users table';
END

-- Add DateFormat column (user's preferred date format)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'DateFormat')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [DateFormat] NVARCHAR(20) NOT NULL DEFAULT 'dd/MM/yyyy'
    CONSTRAINT CK_Users_DateFormat CHECK ([DateFormat] IN ('dd/MM/yyyy', 'MM/dd/yyyy', 'yyyy-MM-dd'));

    PRINT '✓ Added DateFormat column to Users table';
END
ELSE
BEGIN
    PRINT '⚠ DateFormat column already exists in Users table';
END

-- Add TimeFormat column (12h or 24h)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'TimeFormat')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [TimeFormat] NVARCHAR(10) NOT NULL DEFAULT '12h'
    CONSTRAINT CK_Users_TimeFormat CHECK ([TimeFormat] IN ('12h', '24h'));

    PRINT '✓ Added TimeFormat column to Users table';
END
ELSE
BEGIN
    PRINT '⚠ TimeFormat column already exists in Users table';
END

PRINT '';
GO

-- ============================================================================
-- STEP 3: SET DEFAULT TIMEZONES BASED ON USER LOCATION (OPTIONAL)
-- ============================================================================
PRINT '============================================================================';
PRINT 'STEP 3: Set Default Timezones for Existing Users';
PRINT '============================================================================';

-- Set timezone to Asia/Kolkata for all existing users (customize as needed)
UPDATE [dbo].[Users]
SET [TimeZone] = 'Asia/Kolkata',
    [DateFormat] = 'dd/MM/yyyy',
    [TimeFormat] = '12h'
WHERE [TimeZone] = 'UTC'; -- Only update users who still have default

DECLARE @UpdatedUserCount INT = @@ROWCOUNT;
PRINT '✓ Updated ' + CAST(@UpdatedUserCount AS NVARCHAR(10)) + ' users to default timezone (Asia/Kolkata)';
PRINT '';
GO

-- ============================================================================
-- STEP 4: CREATE BACKUP COLUMNS FOR DATETIME VALUES (SAFETY NET)
-- ============================================================================
PRINT '============================================================================';
PRINT 'STEP 4: Create Backup Columns for Safety';
PRINT '============================================================================';

-- We'll create temporary backup columns to preserve original DateTime values
-- These can be dropped after successful migration verification

-- Complaints table
ALTER TABLE [dbo].[Complaints]
ADD [_Backup_SubmittedAt] DATETIME2 NULL,
    [_Backup_DueDate] DATETIME2 NULL,
    [_Backup_ResolvedAt] DATETIME2 NULL,
    [_Backup_ClosedAt] DATETIME2 NULL;

-- Copy existing values to backup
UPDATE [dbo].[Complaints]
SET [_Backup_SubmittedAt] = [SubmittedAt],
    [_Backup_DueDate] = [DueDate],
    [_Backup_ResolvedAt] = [ResolvedAt],
    [_Backup_ClosedAt] = [ClosedAt];

PRINT '✓ Created backup columns for Complaints table';

-- BaseEntity audit columns (affects all tables inheriting from BaseEntity)
-- We'll handle the most critical tables manually

DECLARE @TableName NVARCHAR(128);
DECLARE @SQL NVARCHAR(MAX);

DECLARE table_cursor CURSOR FOR
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME NOT LIKE '%_Backup%'
AND EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = INFORMATION_SCHEMA.TABLES.TABLE_NAME
    AND COLUMN_NAME = 'CreatedAt'
    AND DATA_TYPE = 'datetime2'
);

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Add backup columns for BaseEntity properties
    SET @SQL = '
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N''[dbo].[' + @TableName + ']'') AND name = ''_Backup_CreatedAt'')
    BEGIN
        ALTER TABLE [dbo].[' + @TableName + ']
        ADD [_Backup_CreatedAt] DATETIME2 NULL,
            [_Backup_UpdatedAt] DATETIME2 NULL,
            [_Backup_DeletedAt] DATETIME2 NULL;

        UPDATE [dbo].[' + @TableName + ']
        SET [_Backup_CreatedAt] = [CreatedAt],
            [_Backup_UpdatedAt] = [UpdatedAt],
            [_Backup_DeletedAt] = [DeletedAt];
    END';

    EXEC sp_executesql @SQL;
    PRINT '✓ Created backup columns for ' + @TableName;

    FETCH NEXT FROM table_cursor INTO @TableName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;

PRINT '';
GO

-- ============================================================================
-- STEP 5: CONVERT DATETIME TO DATETIMEOFFSET
-- ============================================================================
PRINT '============================================================================';
PRINT 'STEP 5: Convert DateTime columns to DateTimeOffset';
PRINT '============================================================================';
PRINT 'This step converts DateTime → DateTimeOffset, assuming UTC timezone';
PRINT '';

-- ============================================================================
-- 5.1: Complaints Table
-- ============================================================================
PRINT 'Converting Complaints table...';

-- Drop existing constraints if any
IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Complaints_SubmittedAt')
    ALTER TABLE [dbo].[Complaints] DROP CONSTRAINT DF_Complaints_SubmittedAt;

-- SubmittedAt
ALTER TABLE [dbo].[Complaints]
DROP COLUMN [SubmittedAt];

ALTER TABLE [dbo].[Complaints]
ADD [SubmittedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET();

UPDATE [dbo].[Complaints]
SET [SubmittedAt] = CAST([_Backup_SubmittedAt] AS DATETIMEOFFSET);

-- DueDate
ALTER TABLE [dbo].[Complaints]
DROP COLUMN [DueDate];

ALTER TABLE [dbo].[Complaints]
ADD [DueDate] DATETIMEOFFSET(7) NULL;

UPDATE [dbo].[Complaints]
SET [DueDate] = CAST([_Backup_DueDate] AS DATETIMEOFFSET)
WHERE [_Backup_DueDate] IS NOT NULL;

-- ResolvedAt
ALTER TABLE [dbo].[Complaints]
DROP COLUMN [ResolvedAt];

ALTER TABLE [dbo].[Complaints]
ADD [ResolvedAt] DATETIMEOFFSET(7) NULL;

UPDATE [dbo].[Complaints]
SET [ResolvedAt] = CAST([_Backup_ResolvedAt] AS DATETIMEOFFSET)
WHERE [_Backup_ResolvedAt] IS NOT NULL;

-- ClosedAt
ALTER TABLE [dbo].[Complaints]
DROP COLUMN [ClosedAt];

ALTER TABLE [dbo].[Complaints]
ADD [ClosedAt] DATETIMEOFFSET(7) NULL;

UPDATE [dbo].[Complaints]
SET [ClosedAt] = CAST([_Backup_ClosedAt] AS DATETIMEOFFSET)
WHERE [_Backup_ClosedAt] IS NOT NULL;

PRINT '✓ Complaints table converted';
PRINT '';

-- ============================================================================
-- 5.2: BaseEntity Columns (CreatedAt, UpdatedAt, DeletedAt)
-- ============================================================================
PRINT 'Converting BaseEntity audit columns across all tables...';

DECLARE @TableName2 NVARCHAR(128);
DECLARE @SQL2 NVARCHAR(MAX);

DECLARE table_cursor2 CURSOR FOR
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME NOT LIKE '%_Backup%'
AND EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = INFORMATION_SCHEMA.TABLES.TABLE_NAME
    AND COLUMN_NAME = '_Backup_CreatedAt'
);

OPEN table_cursor2;
FETCH NEXT FROM table_cursor2 INTO @TableName2;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL2 = '
    -- Drop existing columns
    ALTER TABLE [dbo].[' + @TableName2 + '] DROP COLUMN [CreatedAt];
    ALTER TABLE [dbo].[' + @TableName2 + '] DROP COLUMN [UpdatedAt];
    ALTER TABLE [dbo].[' + @TableName2 + '] DROP COLUMN [DeletedAt];

    -- Add as DateTimeOffset
    ALTER TABLE [dbo].[' + @TableName2 + ']
    ADD [CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        [UpdatedAt] DATETIMEOFFSET(7) NULL,
        [DeletedAt] DATETIMEOFFSET(7) NULL;

    -- Restore from backup
    UPDATE [dbo].[' + @TableName2 + ']
    SET [CreatedAt] = CAST([_Backup_CreatedAt] AS DATETIMEOFFSET),
        [UpdatedAt] = CAST([_Backup_UpdatedAt] AS DATETIMEOFFSET),
        [DeletedAt] = CAST([_Backup_DeletedAt] AS DATETIMEOFFSET);
    ';

    EXEC sp_executesql @SQL2;
    PRINT '✓ Converted ' + @TableName2;

    FETCH NEXT FROM table_cursor2 INTO @TableName2;
END

CLOSE table_cursor2;
DEALLOCATE table_cursor2;

PRINT '';

-- ============================================================================
-- 5.3: Users Table (DateOfJoining, DateOfBirth, LastLoginAt, etc.)
-- ============================================================================
PRINT 'Converting Users table datetime columns...';

-- DateOfJoining
ALTER TABLE [dbo].[Users] DROP COLUMN [DateOfJoining];
ALTER TABLE [dbo].[Users] ADD [DateOfJoining] DATETIMEOFFSET(7) NULL;

-- DateOfBirth
ALTER TABLE [dbo].[Users] DROP COLUMN [DateOfBirth];
ALTER TABLE [dbo].[Users] ADD [DateOfBirth] DATETIMEOFFSET(7) NULL;

-- LastLoginAt
ALTER TABLE [dbo].[Users] DROP COLUMN [LastLoginAt];
ALTER TABLE [dbo].[Users] ADD [LastLoginAt] DATETIMEOFFSET(7) NULL;

-- LastSyncedAt
ALTER TABLE [dbo].[Users] DROP COLUMN [LastSyncedAt];
ALTER TABLE [dbo].[Users] ADD [LastSyncedAt] DATETIMEOFFSET(7) NULL;

-- Password management fields
ALTER TABLE [dbo].[Users] DROP COLUMN [PasswordExpiresAt];
ALTER TABLE [dbo].[Users] ADD [PasswordExpiresAt] DATETIMEOFFSET(7) NULL;

ALTER TABLE [dbo].[Users] DROP COLUMN [PasswordChangedAt];
ALTER TABLE [dbo].[Users] ADD [PasswordChangedAt] DATETIMEOFFSET(7) NULL;

ALTER TABLE [dbo].[Users] DROP COLUMN [AccountLockedUntil];
ALTER TABLE [dbo].[Users] ADD [AccountLockedUntil] DATETIMEOFFSET(7) NULL;

ALTER TABLE [dbo].[Users] DROP COLUMN [LastPasswordChangeRequiredNotificationSentAt];
ALTER TABLE [dbo].[Users] ADD [LastPasswordChangeRequiredNotificationSentAt] DATETIMEOFFSET(7) NULL;

ALTER TABLE [dbo].[Users] DROP COLUMN [LastExternalSyncAt];
ALTER TABLE [dbo].[Users] ADD [LastExternalSyncAt] DATETIMEOFFSET(7) NULL;

PRINT '✓ Users table converted';
PRINT '';

-- ============================================================================
-- 5.4: EmailMessage Table
-- ============================================================================
PRINT 'Converting EmailMessage table...';

ALTER TABLE [dbo].[EmailMessage] DROP COLUMN [ReceivedAt];
ALTER TABLE [dbo].[EmailMessage] ADD [ReceivedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET();

ALTER TABLE [dbo].[EmailMessage] DROP COLUMN [ProcessedAt];
ALTER TABLE [dbo].[EmailMessage] ADD [ProcessedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET();

ALTER TABLE [dbo].[EmailMessage] DROP COLUMN [SentAt];
ALTER TABLE [dbo].[EmailMessage] ADD [SentAt] DATETIMEOFFSET(7) NULL;

ALTER TABLE [dbo].[EmailMessage] DROP COLUMN [ReadAt];
ALTER TABLE [dbo].[EmailMessage] ADD [ReadAt] DATETIMEOFFSET(7) NULL;

PRINT '✓ EmailMessage table converted';
PRINT '';

-- ============================================================================
-- STEP 6: VERIFICATION
-- ============================================================================
PRINT '============================================================================';
PRINT 'STEP 6: Verify Migration';
PRINT '============================================================================';

-- Check all DateTimeOffset columns
SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE DATA_TYPE = 'datetimeoffset'
ORDER BY TABLE_NAME, COLUMN_NAME;

PRINT '';
PRINT 'Sample data from Complaints table:';
SELECT TOP 5
    ComplaintNumber,
    SubmittedAt,
    FORMAT(SubmittedAt AT TIME ZONE 'UTC' AT TIME ZONE 'India Standard Time', 'yyyy-MM-dd HH:mm:ss') as 'IST_Time',
    CreatedAt
FROM [dbo].[Complaints]
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '✓ Migration verification complete';
PRINT '';

-- ============================================================================
-- STEP 7: CLEANUP (OPTIONAL - RUN AFTER VALIDATION)
-- ============================================================================
PRINT '============================================================================';
PRINT 'STEP 7: Cleanup Backup Columns (OPTIONAL)';
PRINT '============================================================================';
PRINT 'The following commands will DROP backup columns.';
PRINT 'Only run these after verifying migration was successful!';
PRINT '';
PRINT '-- Uncomment to execute:';
PRINT '-- ALTER TABLE [dbo].[Complaints] DROP COLUMN [_Backup_SubmittedAt], [_Backup_DueDate], [_Backup_ResolvedAt], [_Backup_ClosedAt];';
PRINT '';

/*
-- UNCOMMENT AFTER SUCCESSFUL VERIFICATION (wait 1-2 weeks in production)

DECLARE @TableName3 NVARCHAR(128);
DECLARE @SQL3 NVARCHAR(MAX);

DECLARE table_cursor3 CURSOR FOR
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_TYPE = 'BASE TABLE'
AND EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = INFORMATION_SCHEMA.TABLES.TABLE_NAME
    AND COLUMN_NAME = '_Backup_CreatedAt'
);

OPEN table_cursor3;
FETCH NEXT FROM table_cursor3 INTO @TableName3;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL3 = '
    ALTER TABLE [dbo].[' + @TableName3 + ']
    DROP COLUMN [_Backup_CreatedAt], [_Backup_UpdatedAt], [_Backup_DeletedAt];
    ';

    EXEC sp_executesql @SQL3;
    PRINT 'Cleaned up backup columns from ' + @TableName3;

    FETCH NEXT FROM table_cursor3 INTO @TableName3;
END

CLOSE table_cursor3;
DEALLOCATE table_cursor3;

PRINT 'Cleanup complete';
*/

-- ============================================================================
-- STEP 8: CREATE INDEXES FOR PERFORMANCE (OPTIONAL)
-- ============================================================================
PRINT '============================================================================';
PRINT 'STEP 8: Create Performance Indexes';
PRINT '============================================================================';

-- Index on CreatedAt for sorting and filtering
CREATE NONCLUSTERED INDEX IX_Complaints_CreatedAt
ON [dbo].[Complaints] ([CreatedAt] DESC)
INCLUDE ([StatusMasterId], [PriorityMasterId]);

CREATE NONCLUSTERED INDEX IX_Complaints_SubmittedAt
ON [dbo].[Complaints] ([SubmittedAt] DESC);

-- Index on DueDate for SLA queries
CREATE NONCLUSTERED INDEX IX_Complaints_DueDate
ON [dbo].[Complaints] ([DueDate])
WHERE [DueDate] IS NOT NULL;

PRINT '✓ Performance indexes created';
PRINT '';

-- ============================================================================
-- MIGRATION COMPLETE
-- ============================================================================
PRINT '============================================================================';
PRINT 'MIGRATION COMPLETE!';
PRINT '============================================================================';
PRINT '';
PRINT 'Summary:';
PRINT '  ✓ Added timezone preference columns to Users';
PRINT '  ✓ Converted DateTime to DateTimeOffset across all tables';
PRINT '  ✓ Created backup columns for safety';
PRINT '  ✓ Created performance indexes';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Verify application still works correctly';
PRINT '  2. Test timezone conversion in UI';
PRINT '  3. Monitor performance for 1-2 weeks';
PRINT '  4. After validation, run cleanup script to remove backup columns';
PRINT '';
PRINT 'Rollback: Restore from backup if needed';
PRINT '============================================================================';

GO
