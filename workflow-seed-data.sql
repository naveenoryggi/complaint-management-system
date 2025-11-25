-- =====================================================================
-- Category-Specific Workflow Engine - Seed Data
-- =====================================================================
-- This script creates example workflows for common complaint categories
-- Date: November 2, 2025
-- =====================================================================

USE ComplaintManagementDB;
GO

-- =====================================================================
-- PART 1: Declare variables for IDs (will be looked up)
-- =====================================================================

DECLARE @CompanyId UNIQUEIDENTIFIER;
DECLARE @HRCategoryId UNIQUEIDENTIFIER;
DECLARE @ITCategoryId UNIQUEIDENTIFIER;
DECLARE @CustomerServiceCategoryId UNIQUEIDENTIFIER;

-- Status Master IDs
DECLARE @SubmittedStatusId UNIQUEIDENTIFIER;
DECLARE @UnderReviewStatusId UNIQUEIDENTIFIER;
DECLARE @InProgressStatusId UNIQUEIDENTIFIER;
DECLARE @EscalatedStatusId UNIQUEIDENTIFIER;
DECLARE @ResolvedStatusId UNIQUEIDENTIFIER;
DECLARE @ClosedStatusId UNIQUEIDENTIFIER;
DECLARE @RejectedStatusId UNIQUEIDENTIFIER;
DECLARE @ReopenedStatusId UNIQUEIDENTIFIER;
DECLARE @PendingInfoStatusId UNIQUEIDENTIFIER;

-- Workflow IDs (will be generated)
DECLARE @HRWorkflowId UNIQUEIDENTIFIER = NEWID();
DECLARE @ITWorkflowId UNIQUEIDENTIFIER = NEWID();
DECLARE @CustomerServiceWorkflowId UNIQUEIDENTIFIER = NEWID();

-- =====================================================================
-- PART 2: Lookup existing IDs
-- =====================================================================

-- Get first active company (for testing)
SELECT TOP 1 @CompanyId = Id FROM Companies WHERE IsDeleted = 0;

-- Get category IDs (create if not exist)
SELECT @HRCategoryId = Id FROM ComplaintCategories WHERE Name LIKE '%HR%' OR Name LIKE '%Human%Resource%' AND IsDeleted = 0;
SELECT @ITCategoryId = Id FROM ComplaintCategories WHERE Name LIKE '%IT%' OR Name LIKE '%Information%Technology%' AND IsDeleted = 0;
SELECT @CustomerServiceCategoryId = Id FROM ComplaintCategories WHERE Name LIKE '%Customer%' OR Name LIKE '%Service%' AND IsDeleted = 0;

-- Get status master IDs
SELECT @SubmittedStatusId = Id FROM ComplaintStatusMasters WHERE Code = 'SUBMITTED' AND IsDeleted = 0;
SELECT @UnderReviewStatusId = Id FROM ComplaintStatusMasters WHERE Code = 'UNDER_REVIEW' AND IsDeleted = 0;
SELECT @InProgressStatusId = Id FROM ComplaintStatusMasters WHERE Code = 'IN_PROGRESS' AND IsDeleted = 0;
SELECT @EscalatedStatusId = Id FROM ComplaintStatusMasters WHERE Code = 'ESCALATED' AND IsDeleted = 0;
SELECT @ResolvedStatusId = Id FROM ComplaintStatusMasters WHERE Code = 'RESOLVED' AND IsDeleted = 0;
SELECT @ClosedStatusId = Id FROM ComplaintStatusMasters WHERE Code = 'CLOSED' AND IsDeleted = 0;
SELECT @RejectedStatusId = Id FROM ComplaintStatusMasters WHERE Code = 'REJECTED' AND IsDeleted = 0;
SELECT @ReopenedStatusId = Id FROM ComplaintStatusMasters WHERE Code = 'REOPENED' AND IsDeleted = 0;
SELECT @PendingInfoStatusId = Id FROM ComplaintStatusMasters WHERE Code = 'PENDING_INFO' AND IsDeleted = 0;

-- =====================================================================
-- PART 3: HR Payroll Workflow
-- =====================================================================

IF @HRCategoryId IS NOT NULL
BEGIN
    PRINT 'Creating HR Payroll Workflow...';

    -- Create workflow
    INSERT INTO CategoryWorkflows (Id, CategoryId, Name, Description, IsActive, IsDefault, CompanyId, CreatedAt, IsDeleted)
    VALUES (
        @HRWorkflowId,
        @HRCategoryId,
        'HR Payroll Processing Workflow',
        'Workflow for HR and payroll-related complaints with finance approval steps',
        1, -- IsActive
        1, -- IsDefault
        NULL, -- CompanyId (global workflow)
        GETUTCDATE(),
        0 -- IsDeleted
    );

    -- Add statuses to workflow
    INSERT INTO CategoryWorkflowStatuses (Id, WorkflowId, StatusMasterId, DisplayOrder, IsInitialStatus, IsActive, DefaultSLAHours, EscalationHours, RequiresApproval, CreatedAt, IsDeleted)
    VALUES
        (NEWID(), @HRWorkflowId, @SubmittedStatusId, 1, 1, 1, 4, 2, 0, GETUTCDATE(), 0),      -- Submitted (Initial, 4h SLA)
        (NEWID(), @HRWorkflowId, @UnderReviewStatusId, 2, 0, 1, 8, 4, 0, GETUTCDATE(), 0),    -- Under Review (8h SLA)
        (NEWID(), @HRWorkflowId, @PendingInfoStatusId, 3, 0, 1, 24, 12, 0, GETUTCDATE(), 0),  -- Verification Required (24h SLA)
        (NEWID(), @HRWorkflowId, @InProgressStatusId, 4, 0, 1, 48, 24, 1, GETUTCDATE(), 0),   -- Approved for Payment (48h SLA, requires approval)
        (NEWID(), @HRWorkflowId, @ResolvedStatusId, 5, 0, 1, 24, 12, 0, GETUTCDATE(), 0),     -- Payment Processed (24h SLA)
        (NEWID(), @HRWorkflowId, @ClosedStatusId, 6, 0, 1, NULL, NULL, 0, GETUTCDATE(), 0);   -- Closed (final state)

    -- Add transition rules
    INSERT INTO CategoryWorkflowTransitions (Id, WorkflowId, FromStatusId, ToStatusId, TransitionName, Description, RequiresComment, RequiresApproval, AllowedRoles, DisplayOrder, IsActive, IsAutomatic, AutoTransitionAfterHours, ButtonColor, IconClass, CreatedAt, IsDeleted)
    VALUES
        -- Submitted → Under Review
        (NEWID(), @HRWorkflowId, @SubmittedStatusId, @UnderReviewStatusId, 'Start Review', 'Begin reviewing the HR complaint', 0, 0, NULL, 1, 1, 0, NULL, '#17a2b8', 'bi-play-circle', GETUTCDATE(), 0),

        -- Under Review → Verification Required
        (NEWID(), @HRWorkflowId, @UnderReviewStatusId, @PendingInfoStatusId, 'Request Verification', 'Request additional verification or documentation', 1, 0, NULL, 2, 1, 0, NULL, '#ffc107', 'bi-file-earmark-text', GETUTCDATE(), 0),

        -- Under Review → Rejected
        (NEWID(), @HRWorkflowId, @UnderReviewStatusId, @ClosedStatusId, 'Reject Complaint', 'Reject and close the complaint', 1, 0, NULL, 3, 1, 0, NULL, '#dc3545', 'bi-x-circle', GETUTCDATE(), 0),

        -- Verification Required → Approved for Payment
        (NEWID(), @HRWorkflowId, @PendingInfoStatusId, @InProgressStatusId, 'Approve for Payment', 'Approve for payment processing', 1, 1, NULL, 4, 1, 0, NULL, '#28a745', 'bi-check2-circle', GETUTCDATE(), 0),

        -- Approved → Payment Processed
        (NEWID(), @HRWorkflowId, @InProgressStatusId, @ResolvedStatusId, 'Process Payment', 'Mark payment as processed', 0, 0, NULL, 5, 1, 0, NULL, '#6610f2', 'bi-credit-card', GETUTCDATE(), 0),

        -- Payment Processed → Closed
        (NEWID(), @HRWorkflowId, @ResolvedStatusId, @ClosedStatusId, 'Close', 'Close the complaint after payment confirmation', 0, 0, NULL, 6, 1, 1, 24, '#6c757d', 'bi-check-circle-fill', GETUTCDATE(), 0),

        -- Closed → Reopened (Admin override)
        (NEWID(), @HRWorkflowId, @ClosedStatusId, @ReopenedStatusId, 'Reopen', 'Reopen a closed complaint', 1, 0, NULL, 7, 1, 0, NULL, '#fd7e14', 'bi-arrow-counterclockwise', GETUTCDATE(), 0);

    PRINT 'HR Payroll Workflow created successfully.';
END
ELSE
BEGIN
    PRINT 'WARNING: HR category not found. Skipping HR workflow creation.';
END

-- =====================================================================
-- PART 4: IT Support Workflow
-- =====================================================================

IF @ITCategoryId IS NOT NULL
BEGIN
    PRINT 'Creating IT Support Workflow...';

    -- Create workflow
    INSERT INTO CategoryWorkflows (Id, CategoryId, Name, Description, IsActive, IsDefault, CompanyId, CreatedAt, IsDeleted)
    VALUES (
        @ITWorkflowId,
        @ITCategoryId,
        'IT Ticket Resolution Workflow',
        'Standard IT support ticket workflow with testing phase',
        1, -- IsActive
        1, -- IsDefault
        NULL, -- CompanyId (global workflow)
        GETUTCDATE(),
        0 -- IsDeleted
    );

    -- Add statuses to workflow
    INSERT INTO CategoryWorkflowStatuses (Id, WorkflowId, StatusMasterId, DisplayOrder, IsInitialStatus, IsActive, DefaultSLAHours, EscalationHours, RequiresApproval, CreatedAt, IsDeleted)
    VALUES
        (NEWID(), @ITWorkflowId, @SubmittedStatusId, 1, 1, 1, 2, 1, 0, GETUTCDATE(), 0),      -- Submitted (Initial, 2h SLA)
        (NEWID(), @ITWorkflowId, @InProgressStatusId, 2, 0, 1, 8, 4, 0, GETUTCDATE(), 0),      -- Assigned/In Progress (8h SLA)
        (NEWID(), @ITWorkflowId, @PendingInfoStatusId, 3, 0, 1, 24, 12, 0, GETUTCDATE(), 0),   -- Testing (24h SLA)
        (NEWID(), @ITWorkflowId, @ResolvedStatusId, 4, 0, 1, 24, 12, 0, GETUTCDATE(), 0),      -- Resolved (24h SLA)
        (NEWID(), @ITWorkflowId, @ClosedStatusId, 5, 0, 1, NULL, NULL, 0, GETUTCDATE(), 0);    -- Closed (final)

    -- Add transition rules
    INSERT INTO CategoryWorkflowTransitions (Id, WorkflowId, FromStatusId, ToStatusId, TransitionName, Description, RequiresComment, RequiresApproval, AllowedRoles, DisplayOrder, IsActive, IsAutomatic, AutoTransitionAfterHours, ButtonColor, IconClass, CreatedAt, IsDeleted)
    VALUES
        -- Submitted → In Progress
        (NEWID(), @ITWorkflowId, @SubmittedStatusId, @InProgressStatusId, 'Assign & Start', 'Assign to technician and start work', 0, 0, NULL, 1, 1, 0, NULL, '#17a2b8', 'bi-play-fill', GETUTCDATE(), 0),

        -- In Progress → Testing
        (NEWID(), @ITWorkflowId, @InProgressStatusId, @PendingInfoStatusId, 'Submit for Testing', 'Submit solution for QA testing', 0, 0, NULL, 2, 1, 0, NULL, '#ffc107', 'bi-clipboard-check', GETUTCDATE(), 0),

        -- Testing → Resolved
        (NEWID(), @ITWorkflowId, @PendingInfoStatusId, @ResolvedStatusId, 'Test Passed', 'Mark as resolved after successful testing', 0, 0, NULL, 3, 1, 0, NULL, '#28a745', 'bi-check2-all', GETUTCDATE(), 0),

        -- Testing → In Progress (test failed)
        (NEWID(), @ITWorkflowId, @PendingInfoStatusId, @InProgressStatusId, 'Test Failed', 'Send back for additional work', 1, 0, NULL, 4, 1, 0, NULL, '#dc3545', 'bi-arrow-left-circle', GETUTCDATE(), 0),

        -- Resolved → Closed
        (NEWID(), @ITWorkflowId, @ResolvedStatusId, @ClosedStatusId, 'Close Ticket', 'Close the IT ticket', 0, 0, NULL, 5, 1, 1, 48, '#6c757d', 'bi-check-circle-fill', GETUTCDATE(), 0),

        -- Closed → Reopened
        (NEWID(), @ITWorkflowId, @ClosedStatusId, @ReopenedStatusId, 'Reopen Ticket', 'Reopen if issue persists', 1, 0, NULL, 6, 1, 0, NULL, '#fd7e14', 'bi-arrow-counterclockwise', GETUTCDATE(), 0);

    PRINT 'IT Support Workflow created successfully.';
END
ELSE
BEGIN
    PRINT 'WARNING: IT category not found. Skipping IT workflow creation.';
END

-- =====================================================================
-- PART 5: Customer Service Workflow
-- =====================================================================

IF @CustomerServiceCategoryId IS NOT NULL
BEGIN
    PRINT 'Creating Customer Service Workflow...';

    -- Create workflow
    INSERT INTO CategoryWorkflows (Id, CategoryId, Name, Description, IsActive, IsDefault, CompanyId, CreatedAt, IsDeleted)
    VALUES (
        @CustomerServiceWorkflowId,
        @CustomerServiceCategoryId,
        'Customer Service Resolution Workflow',
        'Standard customer service complaint workflow with escalation support',
        1, -- IsActive
        1, -- IsDefault
        NULL, -- CompanyId (global workflow)
        GETUTCDATE(),
        0 -- IsDeleted
    );

    -- Add statuses to workflow
    INSERT INTO CategoryWorkflowStatuses (Id, WorkflowId, StatusMasterId, DisplayOrder, IsInitialStatus, IsActive, DefaultSLAHours, EscalationHours, RequiresApproval, CreatedAt, IsDeleted)
    VALUES
        (NEWID(), @CustomerServiceWorkflowId, @SubmittedStatusId, 1, 1, 1, 4, 2, 0, GETUTCDATE(), 0),        -- Submitted (Initial, 4h SLA)
        (NEWID(), @CustomerServiceWorkflowId, @UnderReviewStatusId, 2, 0, 1, 12, 6, 0, GETUTCDATE(), 0),      -- Acknowledged (12h SLA)
        (NEWID(), @CustomerServiceWorkflowId, @InProgressStatusId, 3, 0, 1, 24, 12, 0, GETUTCDATE(), 0),      -- Investigating (24h SLA)
        (NEWID(), @CustomerServiceWorkflowId, @EscalatedStatusId, 4, 0, 1, 12, 6, 0, GETUTCDATE(), 0),        -- Escalated (12h SLA)
        (NEWID(), @CustomerServiceWorkflowId, @ResolvedStatusId, 5, 0, 1, 48, 24, 0, GETUTCDATE(), 0),        -- Resolved (48h SLA)
        (NEWID(), @CustomerServiceWorkflowId, @ClosedStatusId, 6, 0, 1, NULL, NULL, 0, GETUTCDATE(), 0);      -- Closed (final)

    -- Add transition rules
    INSERT INTO CategoryWorkflowTransitions (Id, WorkflowId, FromStatusId, ToStatusId, TransitionName, Description, RequiresComment, RequiresApproval, AllowedRoles, DisplayOrder, IsActive, IsAutomatic, AutoTransitionAfterHours, ButtonColor, IconClass, CreatedAt, IsDeleted)
    VALUES
        -- Submitted → Acknowledged
        (NEWID(), @CustomerServiceWorkflowId, @SubmittedStatusId, @UnderReviewStatusId, 'Acknowledge', 'Acknowledge receipt of complaint', 0, 0, NULL, 1, 1, 1, 2, '#17a2b8', 'bi-envelope-check', GETUTCDATE(), 0),

        -- Acknowledged → Investigating
        (NEWID(), @CustomerServiceWorkflowId, @UnderReviewStatusId, @InProgressStatusId, 'Start Investigation', 'Begin investigating the issue', 0, 0, NULL, 2, 1, 0, NULL, '#ffc107', 'bi-search', GETUTCDATE(), 0),

        -- Investigating → Escalated
        (NEWID(), @CustomerServiceWorkflowId, @InProgressStatusId, @EscalatedStatusId, 'Escalate', 'Escalate to supervisor/manager', 1, 0, NULL, 3, 1, 0, NULL, '#fd7e14', 'bi-arrow-up-circle', GETUTCDATE(), 0),

        -- Investigating → Resolved
        (NEWID(), @CustomerServiceWorkflowId, @InProgressStatusId, @ResolvedStatusId, 'Resolve', 'Mark as resolved', 1, 0, NULL, 4, 1, 0, NULL, '#28a745', 'bi-check2-circle', GETUTCDATE(), 0),

        -- Escalated → Resolved
        (NEWID(), @CustomerServiceWorkflowId, @EscalatedStatusId, @ResolvedStatusId, 'Resolve Escalation', 'Resolve the escalated issue', 1, 0, NULL, 5, 1, 0, NULL, '#28a745', 'bi-check2-all', GETUTCDATE(), 0),

        -- Resolved → Closed
        (NEWID(), @CustomerServiceWorkflowId, @ResolvedStatusId, @ClosedStatusId, 'Close', 'Close the complaint', 0, 0, NULL, 6, 1, 1, 72, '#6c757d', 'bi-x-circle-fill', GETUTCDATE(), 0),

        -- Closed → Reopened
        (NEWID(), @CustomerServiceWorkflowId, @ClosedStatusId, @ReopenedStatusId, 'Reopen', 'Reopen if customer not satisfied', 1, 0, NULL, 7, 1, 0, NULL, '#dc3545', 'bi-arrow-counterclockwise', GETUTCDATE(), 0);

    PRINT 'Customer Service Workflow created successfully.';
END
ELSE
BEGIN
    PRINT 'WARNING: Customer Service category not found. Skipping Customer Service workflow creation.';
END

-- =====================================================================
-- PART 6: Verification Query
-- =====================================================================

PRINT '';
PRINT '========================================';
PRINT 'Workflow Seed Data - Summary';
PRINT '========================================';

SELECT
    'CategoryWorkflows' AS TableName,
    COUNT(*) AS RecordCount
FROM CategoryWorkflows
WHERE IsDeleted = 0
UNION ALL
SELECT
    'CategoryWorkflowStatuses' AS TableName,
    COUNT(*) AS RecordCount
FROM CategoryWorkflowStatuses
WHERE IsDeleted = 0
UNION ALL
SELECT
    'CategoryWorkflowTransitions' AS TableName,
    COUNT(*) AS RecordCount
FROM CategoryWorkflowTransitions
WHERE IsDeleted = 0;

PRINT '';
PRINT 'Workflow details:';
SELECT
    cw.Name AS WorkflowName,
    c.Name AS CategoryName,
    (SELECT COUNT(*) FROM CategoryWorkflowStatuses WHERE WorkflowId = cw.Id AND IsDeleted = 0) AS StatusCount,
    (SELECT COUNT(*) FROM CategoryWorkflowTransitions WHERE WorkflowId = cw.Id AND IsDeleted = 0) AS TransitionCount
FROM CategoryWorkflows cw
LEFT JOIN ComplaintCategories c ON cw.CategoryId = c.Id
WHERE cw.IsDeleted = 0
ORDER BY cw.Name;

PRINT '';
PRINT '========================================';
PRINT 'Seed data creation complete!';
PRINT '========================================';

GO
