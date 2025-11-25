-- Check Escalated status configuration
USE ComplaintManagementDB;
GO

PRINT '=== 1. ALL ESCALATED STATUSES ===';
SELECT
    c.Name AS CompanyName,
    c.Id AS CompanyId,
    csm.Id AS StatusId,
    csm.Name AS StatusName,
    csm.Code,
    csm.ColorCode,
    csm.IsActive,
    csm.IsDeleted
FROM ComplaintStatusMasters csm
JOIN Companies c ON csm.CompanyId = c.Id
WHERE csm.Name = 'Escalated';

PRINT '';
PRINT '=== 2. CHECK COMPLAINT COMPANY ===';
SELECT
    c.Id AS ComplaintId,
    c.ComplaintNumber,
    c.CompanyId,
    co.Name AS CompanyName
FROM Complaints c
JOIN Companies co ON c.CompanyId = co.Id
WHERE c.Id = 'dc5f95da-92d1-40f9-8ed3-1b91f0b70c34';

PRINT '';
PRINT '=== 3. CHECK IF ESCALATED STATUS EXISTS FOR THIS COMPLAINT'S COMPANY ===';
SELECT
    csm.Id AS StatusId,
    csm.Name AS StatusName,
    csm.Code,
    csm.IsActive,
    csm.IsDeleted,
    LOWER(csm.Name) AS LowerCaseName
FROM ComplaintStatusMasters csm
WHERE csm.CompanyId = (
    SELECT CompanyId FROM Complaints WHERE Id = 'dc5f95da-92d1-40f9-8ed3-1b91f0b70c34'
)
AND csm.Name = 'Escalated';

PRINT '';
PRINT '=== 4. CHECK CASE-INSENSITIVE MATCH ===';
SELECT
    csm.Id AS StatusId,
    csm.Name AS StatusName,
    csm.Code,
    csm.IsActive,
    csm.IsDeleted,
    LOWER(csm.Name) AS LowerCaseName
FROM ComplaintStatusMasters csm
WHERE csm.CompanyId = (
    SELECT CompanyId FROM Complaints WHERE Id = 'dc5f95da-92d1-40f9-8ed3-1b91f0b70c34'
)
AND LOWER(csm.Name) = 'escalated';
