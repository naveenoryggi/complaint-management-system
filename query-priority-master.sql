SELECT 
    Id,
    Name,
    Level,
    DisplayOrder,
    IsActive,
    IsSystem,
    ColorCode
FROM ComplaintPriorityMaster
WHERE IsDeleted = 0
ORDER BY DisplayOrder;
