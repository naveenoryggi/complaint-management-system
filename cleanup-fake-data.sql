-- ============================================================================
-- CLEANUP FAKE/TEST DATA FROM DATABASE
-- This script removes all fake GUIDs (pattern: 10000000-0000-0000-0000-00000000000X)
-- ============================================================================

BEGIN TRANSACTION;

PRINT '=== Starting cleanup of fake test data ===';

-- ============================================================================
-- 1. DELETE DASHBOARD PREFERENCES WITH FAKE STATUS IDS
-- ============================================================================
PRINT 'Cleaning DashboardPreferences table...';

-- Delete all dashboard preferences (they will be regenerated with real IDs)
DELETE FROM [DashboardPreferences]
WHERE [UserId] IS NOT NULL;

PRINT 'DashboardPreferences cleaned: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows deleted';

-- ============================================================================
-- 2. VERIFY NO FAKE STATUS MASTERS EXIST
-- ============================================================================
PRINT 'Checking for fake ComplaintStatusMasters...';

SELECT
    COUNT(*) as FakeStatusCount,
    STRING_AGG(CAST(Id AS VARCHAR(50)), ', ') as FakeIds
FROM [ComplaintStatusMasters]
WHERE CAST(Id AS VARCHAR(50)) LIKE '10000000-0000-0000-0000-00000000000%'
AND [IsDeleted] = 0;

-- If any fake status masters exist, delete them (should not happen in production)
DELETE FROM [ComplaintStatusMasters]
WHERE CAST(Id AS VARCHAR(50)) LIKE '10000000-0000-0000-0000-00000000000%';

PRINT 'Fake ComplaintStatusMasters deleted: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';

-- ============================================================================
-- 3. VERIFY NO FAKE COMPANIES EXIST
-- ============================================================================
PRINT 'Checking for fake Companies...';

SELECT
    COUNT(*) as FakeCompanyCount
FROM [Tenants]
WHERE CAST(Id AS VARCHAR(50)) LIKE '10000000-0000-0000-0000-00000000000%'
AND [IsDeleted] = 0;

-- Delete fake companies if any exist
DELETE FROM [Tenants]
WHERE CAST(Id AS VARCHAR(50)) LIKE '10000000-0000-0000-0000-00000000000%';

PRINT 'Fake Companies deleted: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';

-- ============================================================================
-- 4. SHOW REAL STATUS MASTERS THAT WILL BE USED
-- ============================================================================
PRINT '';
PRINT '=== Real Status Masters in Database ===';

SELECT
    [Name],
    [Code],
    [CompanyId],
    CASE WHEN [CompanyId] IS NULL THEN 'SYSTEM' ELSE 'COMPANY-SPECIFIC' END as [Type],
    [IsActive],
    [DisplayOrder]
FROM [ComplaintStatusMasters]
WHERE [IsDeleted] = 0
ORDER BY [DisplayOrder];

-- ============================================================================
-- 5. COMMIT TRANSACTION
-- ============================================================================
PRINT '';
PRINT '=== Cleanup completed successfully ===';
PRINT 'All fake test data has been removed.';
PRINT 'DashboardPreferences will be regenerated with real status IDs on next login.';

COMMIT TRANSACTION;
