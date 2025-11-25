-- Delete all users from ComplaintManagement database
USE ComplaintManagementDb;
GO

-- First, delete from dependent tables that reference Users
DELETE FROM UserComplaintRoles;
DELETE FROM Users;

-- Reset identity seed
DBCC CHECKIDENT ('Users', RESEED, 0);
GO

SELECT 'All users deleted successfully' AS Result;
