-- Check for soft-deleted complaints in database
USE ComplaintDB;  -- Adjust database name if different
GO

SELECT
    'Total Records' as Category,
    COUNT(*) as Count
FROM Complaints

UNION ALL

SELECT
    'Active (IsDeleted=0 or NULL)' as Category,
    COUNT(*) as Count
FROM Complaints
WHERE ISNULL(IsDeleted, 0) = 0

UNION ALL

SELECT
    'Soft Deleted (IsDeleted=1)' as Category,
    COUNT(*) as Count
FROM Complaints
WHERE IsDeleted = 1;

-- Show details
SELECT
    ComplaintNumber,
    Title,
    Status,
    ComplainantName,
    CASE
        WHEN IsDeleted = 1 THEN 'SOFT DELETED'
        ELSE 'ACTIVE'
    END as RecordStatus,
    IsDeleted,
    CreatedAt
FROM Complaints
ORDER BY IsDeleted, CreatedAt DESC;
