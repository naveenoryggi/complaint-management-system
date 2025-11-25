-- Clean Status Test Data - November 2, 2025
-- Remove test/invalid status entries to achieve 100% system health

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Step 1: Clean status test data
-- Removing 3 test entries:
-- 1. Empty name status ("") - Breaks UI
-- 2. "Test Status" - Generic test entry
-- 3. "Duplicate Status" - Duplicate/invalid status

-- Use soft delete for statuses that might be referenced
UPDATE ComplaintStatusMasters
SET IsDeleted = 1,
    IsActive = 0,
    UpdatedAt = GETUTCDATE()
WHERE Id IN (
    '2466c3e0-dbe3-43ae-99a5-ac1f2188e4f1',  -- Empty name
    '16b51e48-a383-46a8-8255-fb27b352b6d2',  -- Test Status
    'e1b72e83-e4da-4c20-972a-e2a1223a1a59'   -- Duplicate Status
);

-- Step 2: Verify cleanup - should show 9 system statuses
SELECT
    Id,
    Name,
    Code,
    DisplayOrder,
    ColorCode,
    IsSystem,
    IsFinal
FROM ComplaintStatusMasters
WHERE IsDeleted = 0
ORDER BY DisplayOrder;

-- Expected: 9 system statuses
-- Note: "Submitted" status will be added in next step
-- Current statuses:
-- 1. Under Review
-- 2. In Progress
-- 3. Escalated
-- 4. Pending Info
-- 5. Resolved
-- 6. Closed
-- 7. Rejected
-- 8. Reopened
