-- Add SLA Permissions to System Administrator Role
-- Run this script to enable SLA endpoints
-- Date: November 1, 2025

USE ComplaintManagementDB;
GO

-- Use the System Administrator role ID
DECLARE @SystemAdminRoleId UNIQUEIDENTIFIER = '728E95E4-AB64-446F-9FE3-7B356943ADAD';

PRINT 'System Administrator Role ID: ' + CAST(@SystemAdminRoleId AS NVARCHAR(50));

-- Delete existing SLA permissions to avoid duplicates
DELETE FROM ComplaintRolePermissions
WHERE ComplaintRoleId = @SystemAdminRoleId
AND PermissionType IN ('ViewSLA', 'ManageSLA', 'CreateSLA', 'UpdateSLA', 'DeleteSLA');

-- Add ViewSLA permission
INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, CreatedAt, IsDeleted)
VALUES (NEWID(), @SystemAdminRoleId, 'ViewSLA', 1, GETUTCDATE(), 0);
PRINT '✓ ViewSLA permission added';

-- Add ManageSLA permission
INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, CreatedAt, IsDeleted)
VALUES (NEWID(), @SystemAdminRoleId, 'ManageSLA', 1, GETUTCDATE(), 0);
PRINT '✓ ManageSLA permission added';

-- Add CreateSLA permission
INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, CreatedAt, IsDeleted)
VALUES (NEWID(), @SystemAdminRoleId, 'CreateSLA', 1, GETUTCDATE(), 0);
PRINT '✓ CreateSLA permission added';

-- Add UpdateSLA permission
INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, CreatedAt, IsDeleted)
VALUES (NEWID(), @SystemAdminRoleId, 'UpdateSLA', 1, GETUTCDATE(), 0);
PRINT '✓ UpdateSLA permission added';

-- Add DeleteSLA permission
INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, CreatedAt, IsDeleted)
VALUES (NEWID(), @SystemAdminRoleId, 'DeleteSLA', 1, GETUTCDATE(), 0);
PRINT '✓ DeleteSLA permission added';

PRINT '';
PRINT '========================================';
PRINT 'SLA Permissions Summary:';
PRINT '========================================';

-- Show all SLA permissions for System Administrator role
SELECT
    rp.PermissionType,
    rp.IsGranted,
    rp.CreatedAt
FROM ComplaintRolePermissions rp
WHERE rp.ComplaintRoleId = @SystemAdminRoleId
  AND rp.PermissionType LIKE '%SLA%'
  AND rp.IsDeleted = 0
ORDER BY rp.PermissionType;

PRINT '';
PRINT '✓ SUCCESS! SLA permissions are now active for System Administrator role.';
PRINT 'You can now test the SLA endpoints.';
GO
