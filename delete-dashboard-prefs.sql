USE [ComplaintManagementDb]
GO

DELETE FROM [DashboardPreferences]
WHERE UserId = 'f56d8d03-e382-454b-bf7d-fa8236c125c3';
GO

SELECT @@ROWCOUNT AS RowsDeleted;
GO
