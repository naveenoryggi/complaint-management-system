-- Check all status counts
SELECT Status, COUNT(*) as Count
FROM Complaints
WHERE IsDeleted = 0
GROUP BY Status
ORDER BY Status;

-- Check for any mismatched StatusMasterIds
SELECT
    c.Status,
    c.StatusMasterId,
    COUNT(*) as Count
FROM Complaints c
WHERE c.IsDeleted = 0
GROUP BY c.Status, c.StatusMasterId
ORDER BY c.Status, c.StatusMasterId;

-- Show specific mismatches
SELECT TOP 5
    ComplaintNumber,
    Status,
    StatusMasterId,
    CASE Status
        WHEN 'Submitted' THEN '10000000-0000-0000-0000-000000000001'
        WHEN 'UnderReview' THEN '10000000-0000-0000-0000-000000000002'
        WHEN 'InProgress' THEN '10000000-0000-0000-0000-000000000003'
        WHEN 'Escalated' THEN '10000000-0000-0000-0000-000000000004'
        WHEN 'PendingInfo' THEN '10000000-0000-0000-0000-000000000005'
        WHEN 'Resolved' THEN '10000000-0000-0000-0000-000000000006'
        WHEN 'Closed' THEN '10000000-0000-0000-0000-000000000007'
        WHEN 'Rejected' THEN '10000000-0000-0000-0000-000000000008'
        WHEN 'Reopened' THEN '10000000-0000-0000-0000-000000000009'
    END as ExpectedStatusMasterId
FROM Complaints
WHERE IsDeleted = 0
  AND StatusMasterId !=
    CASE Status
        WHEN 'Submitted' THEN '10000000-0000-0000-0000-000000000001'
        WHEN 'UnderReview' THEN '10000000-0000-0000-0000-000000000002'
        WHEN 'InProgress' THEN '10000000-0000-0000-0000-000000000003'
        WHEN 'Escalated' THEN '10000000-0000-0000-0000-000000000004'
        WHEN 'PendingInfo' THEN '10000000-0000-0000-0000-000000000005'
        WHEN 'Resolved' THEN '10000000-0000-0000-0000-000000000006'
        WHEN 'Closed' THEN '10000000-0000-0000-0000-000000000007'
        WHEN 'Rejected' THEN '10000000-0000-0000-0000-000000000008'
        WHEN 'Reopened' THEN '10000000-0000-0000-0000-000000000009'
    END;
