-- Fix Priority Master Data - November 2, 2025
-- Issue: Wrong level values causing priority dropdown mapping bug
-- When user selects "Normal", system sends priority=3 (Critical) instead of priority=1

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Step 1: Delete test data entries
DELETE FROM ComplaintPriorityMasters
WHERE Id IN (
    'ae474cf9-8f4e-438e-9813-0a8d25b6c8f6',  -- Test Priority
    '0351fec1-f8af-4863-a049-c4c29636acc2',  -- Invalid Priority
    'c7814bf9-c5ee-41ce-9e32-a021423edf44'   -- Dynamic Test Priority
);

-- Step 2: Update system priorities to have correct level values matching backend enum
-- Backend enum: Low=0, Normal=1, High=2, Critical=3, Urgent=4

-- Update Low: Change level from 1 to 0, displayOrder to 0
UPDATE ComplaintPriorityMasters
SET Level = 0,
    DisplayOrder = 0,
    Name = 'Low',  -- Remove trailing space
    UpdatedAt = GETUTCDATE()
WHERE Id = '20000000-0000-0000-0000-000000000001';

-- Update Normal: Change level from 3 to 1, displayOrder to 1
UPDATE ComplaintPriorityMasters
SET Level = 1,
    DisplayOrder = 1,
    UpdatedAt = GETUTCDATE()
WHERE Id = '20000000-0000-0000-0000-000000000002';

-- Update High: Change level from 5 to 2, displayOrder to 2
UPDATE ComplaintPriorityMasters
SET Level = 2,
    DisplayOrder = 2,
    UpdatedAt = GETUTCDATE()
WHERE Id = '20000000-0000-0000-0000-000000000003';

-- Update Critical: Change level from 8 to 3, displayOrder to 3
UPDATE ComplaintPriorityMasters
SET Level = 3,
    DisplayOrder = 3,
    UpdatedAt = GETUTCDATE()
WHERE Id = '20000000-0000-0000-0000-000000000004';

-- Update Urgent: Change level from 10 to 4, displayOrder to 4
UPDATE ComplaintPriorityMasters
SET Level = 4,
    DisplayOrder = 4,
    UpdatedAt = GETUTCDATE()
WHERE Id = '20000000-0000-0000-0000-000000000005';

-- Step 3: Verify the fix
SELECT
    Id,
    Name,
    Code,
    Level,
    DisplayOrder,
    ColorCode,
    IsActive,
    IsSystem
FROM ComplaintPriorityMasters
WHERE IsDeleted = 0
ORDER BY DisplayOrder;

-- Expected Result:
-- displayOrder | level | name     | code
-- 0           | 0     | Low      | LOW
-- 1           | 1     | Normal   | NORMAL
-- 2           | 2     | High     | HIGH
-- 3           | 3     | Critical | CRITICAL
-- 4           | 4     | Urgent   | URGENT
