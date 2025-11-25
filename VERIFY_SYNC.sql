-- ================================================
-- Oryggi Sync Verification Queries
-- ================================================
-- Use these queries to verify sync is working properly

-- 1. Check total users synced
SELECT COUNT(*) AS TotalUsers
FROM Users
WHERE IsDeleted = 0;

-- 2. Check users synced today
SELECT COUNT(*) AS UsersS yncedToday
FROM Users
WHERE IsDeleted = 0
  AND CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE);

-- 3. View sample of synced users
SELECT TOP 10
    Id,
    EmployeeCode,
    FirstName + ' ' + LastName AS FullName,
    Email,
    JobTitle,
    IsActive,
    CreatedAt,
    LastSyncedAt,
    OryggiEmployeeId
FROM Users
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;

-- 4. Check sync logs
SELECT TOP 5
    SyncLogId,
    SyncType,
    Status,
    StartedAt,
    CompletedAt,
    Duration,
    CompaniesProcessed,
    CompaniesCreated,
    CompaniesUpdated,
    BranchesProcessed,
    BranchesCreated,
    BranchesUpdated,
    DepartmentsProcessed,
    DepartmentsCreated,
    DepartmentsUpdated,
    SectionsProcessed,
    SectionsCreated,
    SectionsUpdated,
    EmployeesProcessed,
    EmployeesCreated,
    EmployeesUpdated,
    UsersCreated
FROM SyncLogs
WHERE IsDeleted = 0
ORDER BY StartedAt DESC;

-- 5. Check latest sync status
SELECT TOP 1
    SyncType,
    Status,
    StartedAt,
    CompletedAt,
    DATEDIFF(SECOND, StartedAt, CompletedAt) AS DurationSeconds,
    EmployeesProcessed,
    EmployeesCreated,
    EmployeesUpdated,
    UsersCreated,
    ErrorMessage
FROM SyncLogs
WHERE IsDeleted = 0
ORDER BY StartedAt DESC;

-- 6. Check if sync is currently running
SELECT
    SyncType,
    Status,
    StartedAt,
    DATEDIFF(MINUTE, StartedAt, GETDATE()) AS MinutesRunning,
    EmployeesProcessed
FROM SyncLogs
WHERE Status = 'IN_PROGRESS'
  AND IsDeleted = 0;

-- 7. Count users by company
SELECT
    c.Name AS CompanyName,
    COUNT(u.Id) AS UserCount
FROM Users u
INNER JOIN Companies c ON u.CompanyId = c.Id
WHERE u.IsDeleted = 0
  AND c.IsDeleted = 0
GROUP BY c.Name
ORDER BY UserCount DESC;

-- 8. Check users created vs updated in last sync
SELECT
    'Users Created' AS Metric,
    UsersCreated AS Count
FROM SyncLogs
WHERE IsDeleted = 0
ORDER BY StartedAt DESC
OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY

UNION ALL

SELECT
    'Employees Created' AS Metric,
    EmployeesCreated AS Count
FROM SyncLogs
WHERE IsDeleted = 0
ORDER BY StartedAt DESC
OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY

UNION ALL

SELECT
    'Employees Updated' AS Metric,
    EmployeesUpdated AS Count
FROM SyncLogs
WHERE IsDeleted = 0
ORDER BY StartedAt DESC
OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;

-- 9. Check sync schedules
SELECT
    Id,
    ScheduleType,
    TimeOfDay,
    DayValue,
    IsEnabled,
    LastRunAt,
    NextRunAt,
    Description,
    CreatedAt
FROM SyncSchedules
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;

-- 10. Verify employee filtering (check no admin or underscore employees)
-- This should return 0 rows if filtering is working correctly
SELECT
    OryggiEmployeeId,
    EmployeeCode,
    FirstName + ' ' + LastName AS FullName,
    Email
FROM Users
WHERE IsDeleted = 0
  AND (OryggiEmployeeId = '1' OR EmployeeCode LIKE '%_%')
ORDER BY CreatedAt DESC;

-- 11. Check Oryggi source database employee count (if accessible)
-- Run this against the Oryggi database to compare
/*
USE OryggiHRMS;  -- Change to your Oryggi database name
GO

SELECT COUNT(*) AS OryggiEmployeeCount
FROM EmployeeMaster
WHERE (Active = 1 OR Active IS NULL)
  AND Ecode != 1
  AND (CorpEmpCode IS NULL OR CorpEmpCode NOT LIKE '%_%');
*/

-- 12. Summary report
SELECT
    'Total Companies' AS Metric,
    CAST(COUNT(*) AS VARCHAR(50)) AS Value
FROM Companies
WHERE IsDeleted = 0

UNION ALL

SELECT
    'Total Branches',
    CAST(COUNT(*) AS VARCHAR(50))
FROM Branches
WHERE IsDeleted = 0

UNION ALL

SELECT
    'Total Departments',
    CAST(COUNT(*) AS VARCHAR(50))
FROM Departments
WHERE IsDeleted = 0

UNION ALL

SELECT
    'Total Sections',
    CAST(COUNT(*) AS VARCHAR(50))
FROM Sections
WHERE IsDeleted = 0

UNION ALL

SELECT
    'Total Users',
    CAST(COUNT(*) AS VARCHAR(50))
FROM Users
WHERE IsDeleted = 0

UNION ALL

SELECT
    'Active Users',
    CAST(COUNT(*) AS VARCHAR(50))
FROM Users
WHERE IsDeleted = 0 AND IsActive = 1

UNION ALL

SELECT
    'Total Sync Runs',
    CAST(COUNT(*) AS VARCHAR(50))
FROM SyncLogs
WHERE IsDeleted = 0

UNION ALL

SELECT
    'Successful Syncs',
    CAST(COUNT(*) AS VARCHAR(50))
FROM SyncLogs
WHERE Status = 'SUCCESS' AND IsDeleted = 0

UNION ALL

SELECT
    'Failed Syncs',
    CAST(COUNT(*) AS VARCHAR(50))
FROM SyncLogs
WHERE Status = 'FAILED' AND IsDeleted = 0;
