-- Create Event Types for Week 2 Auto-Response System
-- This creates all necessary event types that the NotificationDispatcher expects

USE ComplaintManagementDb;
GO

-- Insert Event Types
INSERT INTO EventTypes (Id, Code, Name, EntityType, Description, Category, AvailableFields, IsActive, IsSystem, IsDeleted, CreatedAt)
VALUES
-- 1. COMPLAINT_CREATED
(NEWID(), 'COMPLAINT_CREATED', 'Complaint Created', 'Complaint',
 'Triggered when a new complaint is created', 'Complaint Lifecycle',
 '["complaintId","complaintNumber","title","description","categoryName","priorityName","statusName","complainantName","complainantEmail","complainantEmployeeCode","createdDate","dueDate","companyName"]',
 1, 0, 0, GETUTCDATE()),

-- 2. COMPLAINT_ASSIGNED
(NEWID(), 'COMPLAINT_ASSIGNED', 'Complaint Assigned', 'Complaint',
 'Triggered when a complaint is assigned to a handler', 'Complaint Lifecycle',
 '["complaintId","complaintNumber","title","assignedToName","assignedToEmail","assignedByName","assignedByEmail","assignedDate"]',
 1, 0, 0, GETUTCDATE()),

-- 3. COMPLAINT_STATUS_CHANGED
(NEWID(), 'COMPLAINT_STATUS_CHANGED', 'Complaint Status Changed', 'Complaint',
 'Triggered when complaint status is changed', 'Complaint Lifecycle',
 '["complaintId","complaintNumber","title","oldStatus","newStatus","changedByName","changedByEmail","changedDate","reason"]',
 1, 0, 0, GETUTCDATE()),

-- 4. COMPLAINT_ESCALATED
(NEWID(), 'COMPLAINT_ESCALATED', 'Complaint Escalated', 'Complaint',
 'Triggered when a complaint is escalated', 'Complaint Lifecycle',
 '["complaintId","complaintNumber","title","escalationLevel","escalatedToName","escalatedToEmail","escalatedByName","escalatedByEmail","escalatedDate","reason"]',
 1, 0, 0, GETUTCDATE()),

-- 5. COMPLAINT_RESOLVED
(NEWID(), 'COMPLAINT_RESOLVED', 'Complaint Resolved', 'Complaint',
 'Triggered when a complaint is resolved', 'Complaint Lifecycle',
 '["complaintId","complaintNumber","title","resolution","resolvedByName","resolvedByEmail","resolvedDate","resolutionNotes"]',
 1, 0, 0, GETUTCDATE()),

-- 6. COMPLAINT_CLOSED
(NEWID(), 'COMPLAINT_CLOSED', 'Complaint Closed', 'Complaint',
 'Triggered when a complaint is closed', 'Complaint Lifecycle',
 '["complaintId","complaintNumber","title","closedByName","closedByEmail","closedDate","closureNotes"]',
 1, 0, 0, GETUTCDATE()),

-- 7. COMPLAINT_REOPENED
(NEWID(), 'COMPLAINT_REOPENED', 'Complaint Reopened', 'Complaint',
 'Triggered when a closed complaint is reopened', 'Complaint Lifecycle',
 '["complaintId","complaintNumber","title","reopenedByName","reopenedByEmail","reopenedDate","reopenReason"]',
 1, 0, 0, GETUTCDATE()),

-- 8. COMMENT_ADDED
(NEWID(), 'COMMENT_ADDED', 'Comment Added', 'Comment',
 'Triggered when a comment is added to a complaint', 'Communication',
 '["complaintId","complaintNumber","commentText","commentByName","commentByEmail","commentDate","isInternal"]',
 1, 0, 0, GETUTCDATE()),

-- 9. SLA_WARNING
(NEWID(), 'SLA_WARNING', 'SLA Warning', 'Complaint',
 'Triggered when complaint is approaching SLA deadline', 'SLA Management',
 '["complaintId","complaintNumber","title","slaLevel","timeRemaining","dueDate","assignedToName","assignedToEmail"]',
 1, 0, 0, GETUTCDATE()),

-- 10. SLA_BREACHED
(NEWID(), 'SLA_BREACHED', 'SLA Breached', 'Complaint',
 'Triggered when complaint has breached SLA deadline', 'SLA Management',
 '["complaintId","complaintNumber","title","slaLevel","breachTime","dueDate","assignedToName","assignedToEmail"]',
 1, 0, 0, GETUTCDATE());

GO

-- Verify the inserts
SELECT Code, Name, EntityType, Category, IsActive
FROM EventTypes
WHERE IsDeleted = 0
ORDER BY Category, Name;

PRINT 'Event types created successfully!';
PRINT 'Total Event Types: ' + CAST((SELECT COUNT(*) FROM EventTypes WHERE IsDeleted = 0) AS VARCHAR);
