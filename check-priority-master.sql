-- Check ComplaintPriorityMaster table data
SELECT
    Id,
    Name,
    Level,
    ColorCode,
    IconClass,
    DisplayOrder,
    IsActive,
    CreatedAt
FROM ComplaintPriorityMaster
ORDER BY DisplayOrder, Level;

-- Check Complaints table priority distribution
SELECT
    Priority,
    COUNT(*) as Count
FROM Complaints
WHERE IsDeleted = 0
GROUP BY Priority
ORDER BY Priority;

-- Check for any PriorityMasterId usage in complaints
SELECT
    c.Priority,
    c.PriorityMasterId,
    COUNT(*) as Count
FROM Complaints c
WHERE c.IsDeleted = 0
GROUP BY c.Priority, c.PriorityMasterId
ORDER BY c.Priority;

-- Show what priority values we have
SELECT DISTINCT Priority
FROM Complaints
WHERE IsDeleted = 0
ORDER BY Priority;