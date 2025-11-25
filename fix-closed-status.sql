-- Fix StatusMasterId for closed complaints
UPDATE Complaints
SET StatusMasterId = '10000000-0000-0000-0000-000000000007'
WHERE IsDeleted = 0
  AND Status = 'Closed'
  AND StatusMasterId != '10000000-0000-0000-0000-000000000007';

-- Verify the fix
SELECT ComplaintNumber, Status, StatusMasterId, ClosedAt
FROM Complaints
WHERE IsDeleted = 0 AND Status = 'Closed';
