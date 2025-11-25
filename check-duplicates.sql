-- Check for duplicate OryggiEmployeeId in ComplaintManagementDb

USE ComplaintManagementDb;
GO

-- Check Employees table for duplicates
SELECT 'Employees Table Duplicates' as TableName;
SELECT OryggiEmployeeId, COUNT(*) as DuplicateCount
FROM Employees
WHERE OryggiEmployeeId IS NOT NULL
GROUP BY OryggiEmployeeId
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- Check Users table for duplicates
SELECT 'Users Table Duplicates' as TableName;
SELECT OryggiEmployeeId, COUNT(*) as DuplicateCount
FROM Users
WHERE OryggiEmployeeId IS NOT NULL
GROUP BY OryggiEmployeeId
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- Count NULL values
SELECT
    'Employees with NULL OryggiEmployeeId' as Info,
    COUNT(*) as Count
FROM Employees
WHERE OryggiEmployeeId IS NULL
UNION ALL
SELECT
    'Users with NULL OryggiEmployeeId',
    COUNT(*)
FROM Users
WHERE OryggiEmployeeId IS NULL;

-- Total counts
SELECT
    'Total Employees' as Info,
    COUNT(*) as Count
FROM Employees
WHERE IsDeleted = 0
UNION ALL
SELECT
    'Total Users',
    COUNT(*)
FROM Users
WHERE IsDeleted = 0;
