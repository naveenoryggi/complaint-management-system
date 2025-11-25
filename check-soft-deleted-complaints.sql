-- Check for soft-deleted complaints
SELECT
    'Total Complaints' as Category,
    COUNT(*) as Count
FROM Complaints

UNION ALL

SELECT
    'Active (Not Deleted)' as Category,
    COUNT(*) as Count
FROM Complaints
WHERE IsDeleted = 0 OR IsDeleted IS NULL

UNION ALL

SELECT
    'Soft Deleted' as Category,
    COUNT(*) as Count
FROM Complaints
WHERE IsDeleted = 1;

-- Show details of all complaints
SELECT
    ComplaintNumber,
    Title,
    Status,
    ComplainantName,
    CASE WHEN IsDeleted = 1 THEN 'DELETED' ELSE 'ACTIVE' END as RecordStatus,
    CreatedAt
FROM Complaints
ORDER BY CreatedAt DESC;
