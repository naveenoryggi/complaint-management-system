-- Find complaints with invalid PriorityMasterId
SELECT DISTINCT c.PriorityMasterId, COUNT(*) as Count
FROM Complaints c
LEFT JOIN ComplaintPriorityMasters pm ON c.PriorityMasterId = pm.Id
WHERE c.PriorityMasterId IS NOT NULL AND pm.Id IS NULL
GROUP BY c.PriorityMasterId;

-- Find complaints with invalid StatusMasterId
SELECT DISTINCT c.StatusMasterId, COUNT(*) as Count
FROM Complaints c
LEFT JOIN ComplaintStatusMasters sm ON c.StatusMasterId = sm.Id
WHERE c.StatusMasterId IS NOT NULL AND sm.Id IS NULL
GROUP BY c.StatusMasterId;

-- Check which priority masters exist
SELECT Id, Name, DisplayOrder FROM ComplaintPriorityMasters ORDER BY DisplayOrder;

-- Check which status masters exist
SELECT Id, Name, DisplayOrder FROM ComplaintStatusMasters ORDER BY DisplayOrder;
