-- Add Submitted Status - November 2, 2025
-- Add initial "Submitted" status to complete complaint workflow

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Step 1: Shift existing statuses displayOrder up by 1 to make room for "Submitted" at position 1
UPDATE ComplaintStatusMasters
SET DisplayOrder = DisplayOrder + 1,
    UpdatedAt = GETUTCDATE()
WHERE IsSystem = 1
  AND IsDeleted = 0
  AND DisplayOrder >= 1;

-- Step 2: Restore "Submitted" status (it already exists but was soft deleted)
UPDATE ComplaintStatusMasters
SET IsDeleted = 0,
    IsActive = 1,
    DisplayOrder = 1,
    Description = 'Complaint has been submitted and awaiting review',
    ColorCode = '#9C27B0',
    IconClass = 'bi-send',
    UpdatedAt = GETUTCDATE()
WHERE Id = '10000000-0000-0000-0000-000000000001';

-- Step 3: Verify final state - should show 9 statuses in correct order
SELECT
    Name,
    Code,
    DisplayOrder,
    ColorCode,
    IconClass,
    IsSystem,
    IsFinal
FROM ComplaintStatusMasters
WHERE IsDeleted = 0
  AND IsSystem = 1
ORDER BY DisplayOrder;

-- Expected result (9 statuses):
-- DisplayOrder | Name          | Code
-- 1           | Submitted     | SUBMITTED
-- 2           | Under Review  | UNDER_REVIEW  (was 2, now 3)
-- 3           | In Progress   | IN_PROGRESS   (was 3, now 4)
-- 4           | Escalated     | ESCALATED     (was 4, now 5)
-- 5           | Pending Info  | PENDING_INFO  (was 5, now 6)
-- 6           | Resolved      | RESOLVED      (was 6, now 7)
-- 7           | Closed        | CLOSED        (was 7, now 8)
-- 8           | Rejected      | REJECTED      (was 8, now 9)
-- 9           | Reopened      | REOPENED      (was 9, now 10)
