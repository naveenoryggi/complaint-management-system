-- Quick Sync Status Check
-- Run this to see if sync is working

-- 1. Check if sync has run
PRINT '=== SYNC LOGS ==='
SELECT TOP 5
    SyncLogId,
    SyncType,
    Status,
    StartedAt,
    CompletedAt,
    DATEDIFF(SECOND, StartedAt, COALESCE(CompletedAt, GETDATE())) AS DurationSeconds,
    EmployeesProcessed,
    EmployeesCreated,
    EmployeesUpdated,
    UsersCreated,
    ErrorMessage
FROM SyncLogs
WHERE IsDeleted = 0
ORDER BY StartedAt DESC;

-- 2. Check user count
PRINT '=== USER COUNT ==='
SELECT
    COUNT(*) AS TotalUsers,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveUsers,
    SUM(CASE WHEN CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS CreatedToday
FROM Users
WHERE IsDeleted = 0;

-- 3. Check if there are any Oryggi employees available
PRINT '=== ORYGGI EMPLOYEE COUNT (Source) ==='
-- You may need to change the database name
-- SELECT COUNT(*) AS AvailableEmployees
-- FROM OryggiHRMS.dbo.EmployeeMaster
-- WHERE (Active = 1 OR Active IS NULL)
--   AND Ecode != 1
--   AND (CorpEmpCode IS NULL OR CorpEmpCode NOT LIKE '%_%');
PRINT 'Run the above query against your Oryggi database to check source data';

-- 4. Sample users
PRINT '=== SAMPLE USERS ==='
SELECT TOP 10
    Id,
    EmployeeCode,
    FirstName,
    LastName,
    Email,
    IsActive,
    CreatedAt,
    LastSyncedAt
FROM Users
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;

-- 5. Check for sync errors
PRINT '=== SYNC ERRORS ==='
SELECT TOP 5
    SyncLogId,
    SyncType,
    Status,
    StartedAt,
    ErrorMessage,
    LEFT(ErrorDetails, 500) AS ErrorDetails
FROM SyncLogs
WHERE Status = 'FAILED' AND IsDeleted = 0
ORDER BY StartedAt DESC;

-- 6. Check if sync is currently running
PRINT '=== CURRENTLY RUNNING SYNCS ==='
SELECT
    SyncLogId,
    SyncType,
    Status,
    StartedAt,
    DATEDIFF(MINUTE, StartedAt, GETDATE()) AS MinutesRunning
FROM SyncLogs
WHERE Status = 'IN_PROGRESS' AND IsDeleted = 0;
