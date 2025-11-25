-- Fix Orphaned Complaint References Before Migration
-- This script ensures all complaints have valid StatusMasterId and PriorityMasterId

USE ComplaintManagementDB;
GO

PRINT '========================================';
PRINT 'Starting orphaned reference fix...';
PRINT '========================================';

-- Step 1: Find orphaned PriorityMasterId references
PRINT '';
PRINT 'Step 1: Checking for orphaned PriorityMasterId references...';

SELECT
    'Orphaned Priority References' as IssueType,
    COUNT(*) as Count
FROM Complaints c
LEFT JOIN ComplaintPriorityMasters pm ON c.PriorityMasterId = pm.Id
WHERE c.PriorityMasterId IS NOT NULL AND pm.Id IS NULL;

-- Step 2: Find orphaned StatusMasterId references
PRINT '';
PRINT 'Step 2: Checking for orphaned StatusMasterId references...';

SELECT
    'Orphaned Status References' as IssueType,
    COUNT(*) as Count
FROM Complaints c
LEFT JOIN ComplaintStatusMasters sm ON c.StatusMasterId = sm.Id
WHERE c.StatusMasterId IS NOT NULL AND sm.Id IS NULL;

-- Step 3: Find NULL PriorityMasterId
PRINT '';
PRINT 'Step 3: Checking for NULL PriorityMasterId...';

SELECT
    'NULL Priority References' as IssueType,
    COUNT(*) as Count
FROM Complaints
WHERE PriorityMasterId IS NULL;

-- Step 4: Find NULL StatusMasterId
PRINT '';
PRINT 'Step 4: Checking for NULL StatusMasterId...';

SELECT
    'NULL Status References' as IssueType,
    COUNT(*) as Count
FROM Complaints
WHERE StatusMasterId IS NULL;

-- Step 5: Get default Medium priority master
DECLARE @MediumPriorityId UNIQUEIDENTIFIER;
SELECT @MediumPriorityId = Id
FROM ComplaintPriorityMasters
WHERE Name = 'Medium' OR DisplayOrder = 3;

IF @MediumPriorityId IS NULL
BEGIN
    SELECT @MediumPriorityId = Id
    FROM ComplaintPriorityMasters
    WHERE DisplayOrder = (SELECT MIN(DisplayOrder) FROM ComplaintPriorityMasters);
END

PRINT '';
PRINT 'Default Medium Priority ID: ' + CAST(@MediumPriorityId AS NVARCHAR(50));

-- Step 6: Get default Submitted status master
DECLARE @SubmittedStatusId UNIQUEIDENTIFIER;
SELECT @SubmittedStatusId = Id
FROM ComplaintStatusMasters
WHERE Name = 'Submitted' OR DisplayOrder = 1;

IF @SubmittedStatusId IS NULL
BEGIN
    SELECT @SubmittedStatusId = Id
    FROM ComplaintStatusMasters
    WHERE DisplayOrder = (SELECT MIN(DisplayOrder) FROM ComplaintStatusMasters);
END

PRINT 'Default Submitted Status ID: ' + CAST(@SubmittedStatusId AS NVARCHAR(50));

-- Step 7: Fix NULL PriorityMasterId
PRINT '';
PRINT 'Step 7: Fixing NULL PriorityMasterId references...';

UPDATE Complaints
SET PriorityMasterId = @MediumPriorityId
WHERE PriorityMasterId IS NULL;

PRINT 'Updated ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' records';

-- Step 8: Fix orphaned PriorityMasterId
PRINT '';
PRINT 'Step 8: Fixing orphaned PriorityMasterId references...';

UPDATE c
SET c.PriorityMasterId = @MediumPriorityId
FROM Complaints c
LEFT JOIN ComplaintPriorityMasters pm ON c.PriorityMasterId = pm.Id
WHERE c.PriorityMasterId IS NOT NULL AND pm.Id IS NULL;

PRINT 'Updated ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' records';

-- Step 9: Fix NULL StatusMasterId
PRINT '';
PRINT 'Step 9: Fixing NULL StatusMasterId references...';

UPDATE Complaints
SET StatusMasterId = @SubmittedStatusId
WHERE StatusMasterId IS NULL;

PRINT 'Updated ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' records';

-- Step 10: Fix orphaned StatusMasterId
PRINT '';
PRINT 'Step 10: Fixing orphaned StatusMasterId references...';

UPDATE c
SET c.StatusMasterId = @SubmittedStatusId
FROM Complaints c
LEFT JOIN ComplaintStatusMasters sm ON c.StatusMasterId = sm.Id
WHERE c.StatusMasterId IS NOT NULL AND sm.Id IS NULL;

PRINT 'Updated ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' records';

-- Step 11: Final verification
PRINT '';
PRINT '========================================';
PRINT 'FINAL VERIFICATION:';
PRINT '========================================';

SELECT
    'NULL PriorityMasterId' as CheckType,
    COUNT(*) as Count
FROM Complaints
WHERE PriorityMasterId IS NULL
UNION ALL
SELECT
    'NULL StatusMasterId' as CheckType,
    COUNT(*) as Count
FROM Complaints
WHERE StatusMasterId IS NULL
UNION ALL
SELECT
    'Orphaned PriorityMasterId' as CheckType,
    COUNT(*) as Count
FROM Complaints c
LEFT JOIN ComplaintPriorityMasters pm ON c.PriorityMasterId = pm.Id
WHERE c.PriorityMasterId IS NOT NULL AND pm.Id IS NULL
UNION ALL
SELECT
    'Orphaned StatusMasterId' as CheckType,
    COUNT(*) as Count
FROM Complaints c
LEFT JOIN ComplaintStatusMasters sm ON c.StatusMasterId = sm.Id
WHERE c.StatusMasterId IS NOT NULL AND sm.Id IS NULL;

PRINT '';
PRINT '========================================';
PRINT 'Fix complete! All counts should be 0.';
PRINT 'You can now run the EF Core migration.';
PRINT '========================================';
