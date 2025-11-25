-- Clean Category Test Data - November 2, 2025
-- Remove test/invalid categories to achieve 100% system health

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Step 1: Clean categories test data
-- Removing 5 test entries:
-- 1. "A" - Meaningless test entry
-- 2. "Duplicate Test" - Duplicate of legitimate category
-- 3. "Test Cat" - Generic test entry
-- 4. "Test<script>alert('xss')</script>" - XSS payload (SECURITY RISK)
-- 5. "Workflow Test Category" - Test workflow entry

-- Use soft delete instead of hard delete due to foreign key constraints
-- Mark test categories as deleted and inactive
UPDATE ComplaintCategories
SET IsDeleted = 1,
    IsActive = 0,
    UpdatedAt = GETUTCDATE()
WHERE Id IN (
    'ec910134-8c32-4837-696c-08de13a4b223',  -- A
    'fd7f5d67-1ccf-46d4-60fb-08de132d67b5',  -- Duplicate Test
    'e07b9bdd-bdf3-41fd-f6ea-08de13ab22a9',  -- Test Cat
    '178c90f5-55b9-44d7-696d-08de13a4b223',  -- XSS Test (SECURITY RISK)
    'aab6327c-8c79-4c65-60f9-08de132d67b5'   -- Workflow Test
);

-- Step 2: Verify cleanup - should show 19 legitimate categories
SELECT
    Id,
    Name,
    Code,
    DefaultPriority,
    IsActive,
    DisplayOrder
FROM ComplaintCategories
WHERE IsDeleted = 0
ORDER BY DisplayOrder, Name;

-- Expected: 19 legitimate categories
-- - Attendance Issues
-- - Benefits & Insurance
-- - Billing Problems
-- - Bug Reports
-- - Customer Service Issues
-- - Delivery Problems
-- - Facilities & Infrastructure
-- - Feature Requests
-- - General Inquiries
-- - HRMS System
-- - IT & Technical Support
-- - Leave Management
-- - Performance Management
-- - Policy Questions
-- - Product Quality Issues
-- - Salary & Payroll
-- - Service Delays
-- - Technical Issues
-- - Workplace Harassment
