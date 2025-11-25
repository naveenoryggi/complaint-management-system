SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

UPDATE ComplaintStatusMasters SET DisplayOrder = 2 WHERE Code = 'UNDER_REVIEW';
UPDATE ComplaintStatusMasters SET DisplayOrder = 3 WHERE Code = 'IN_PROGRESS';
UPDATE ComplaintStatusMasters SET DisplayOrder = 4 WHERE Code = 'ESCALATED';
UPDATE ComplaintStatusMasters SET DisplayOrder = 5 WHERE Code = 'PENDING_INFO';
UPDATE ComplaintStatusMasters SET DisplayOrder = 6 WHERE Code = 'RESOLVED';
UPDATE ComplaintStatusMasters SET DisplayOrder = 7 WHERE Code = 'CLOSED';
UPDATE ComplaintStatusMasters SET DisplayOrder = 8 WHERE Code = 'REJECTED';
UPDATE ComplaintStatusMasters SET DisplayOrder = 9 WHERE Code = 'REOPENED';

SELECT Name, Code, DisplayOrder, ColorCode FROM ComplaintStatusMasters
WHERE IsDeleted = 0 AND IsSystem = 1
ORDER BY DisplayOrder;
