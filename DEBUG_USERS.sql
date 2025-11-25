-- DEBUG: Check why users aren't showing up

-- 1. Total users in database (including inactive)
SELECT
    'Total Users (including inactive)' AS CheckType,
    COUNT(*) AS Count
FROM Users
WHERE IsDeleted = 0;

-- 2. Active users only (what the API returns)
SELECT
    'Active Users (what UI shows)' AS CheckType,
    COUNT(*) AS Count
FROM Users
WHERE IsDeleted = 0 AND IsActive = 1;

-- 3. Recently synced users with details
SELECT TOP 20
    EmployeeCode,
    FirstName + ' ' + LastName AS FullName,
    Email,
    IsActive,
    IsDeleted,
    CompanyId,
    CreatedAt,
    LastSyncedAt,
    OryggiEmployeeId
FROM Users
ORDER BY CreatedAt DESC;

-- 4. Check if users have CompanyId = EMPTY GUID (this could cause issues)
SELECT
    'Users with Empty GUID CompanyId' AS CheckType,
    COUNT(*) AS Count
FROM Users
WHERE IsDeleted = 0
  AND (CompanyId = '00000000-0000-0000-0000-000000000000' OR CompanyId IS NULL);

-- 5. Check last sync log details
SELECT TOP 1
    'Last Sync Details' AS Info,
    SyncType,
    Status,
    StartedAt,
    CompletedAt,
    EmployeesProcessed,
    EmployeesCreated,
    EmployeesUpdated,
    UsersCreated,
    CompaniesProcessed,
    CompaniesCreated,
    BranchesProcessed,
    BranchesCreated,
    DepartmentsProcessed,
    DepartmentsCreated,
    SectionsProcessed,
    SectionsCreated,
    ErrorMessage
FROM SyncLogs
WHERE IsDeleted = 0
ORDER BY StartedAt DESC;

-- 6. Check if admin user exists
SELECT
    'Admin User Check' AS CheckType,
    EmployeeCode,
    Email,
    IsActive,
    IsDeleted
FROM Users
WHERE Email = 'admin@complaintmanagement.com';

-- 7. Check companies
SELECT
    'Companies' AS CheckType,
    Id,
    Name,
    OryggiCompanyId,
    IsDeleted,
    IsActive
FROM Companies
WHERE IsDeleted = 0;
