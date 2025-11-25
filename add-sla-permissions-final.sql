-- Add SLA Permissions to System Admin Role
-- Corrected to use SYSTEM_ADMIN code and proper column names

USE ComplaintManagementDB;
GO

-- Get System Admin Role ID
DECLARE @AdminRoleId UNIQUEIDENTIFIER;
SELECT @AdminRoleId = Id FROM ComplaintRoles WHERE Code = 'SYSTEM_ADMIN' AND IsDeleted = 0;

-- Check if admin role exists
IF @AdminRoleId IS NULL
BEGIN
    PRINT 'ERROR: System Admin role not found!';
    SELECT Id, Name, Code, RoleType FROM ComplaintRoles WHERE IsDeleted = 0 AND RoleType LIKE '%Admin%';
END
ELSE
BEGIN
    PRINT 'System Admin Role ID: ' + CAST(@AdminRoleId AS NVARCHAR(50));

    -- Add ViewSLA permission
    IF NOT EXISTS (SELECT 1 FROM ComplaintRolePermissions WHERE ComplaintRoleId = @AdminRoleId AND PermissionType = 'ViewSLA' AND IsDeleted = 0)
    BEGIN
        INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, IsDeleted, CreatedAt)
        VALUES (NEWID(), @AdminRoleId, 'ViewSLA', 1, 0, GETUTCDATE());
        PRINT 'ViewSLA permission added successfully';
    END
    ELSE
    BEGIN
        PRINT 'ViewSLA permission already exists';
    END

    -- Add ManageSLA permission
    IF NOT EXISTS (SELECT 1 FROM ComplaintRolePermissions WHERE ComplaintRoleId = @AdminRoleId AND PermissionType = 'ManageSLA' AND IsDeleted = 0)
    BEGIN
        INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, IsDeleted, CreatedAt)
        VALUES (NEWID(), @AdminRoleId, 'ManageSLA', 1, 0, GETUTCDATE());
        PRINT 'ManageSLA permission added successfully';
    END
    ELSE
    BEGIN
        PRINT 'ManageSLA permission already exists';
    END

    PRINT '';
    PRINT '========================================';
    PRINT 'SLA Permissions Summary:';
    PRINT '========================================';

    -- Show all SLA permissions for System Admin
    SELECT PermissionType, IsGranted, CreatedAt
    FROM ComplaintRolePermissions
    WHERE ComplaintRoleId = @AdminRoleId
      AND PermissionType LIKE '%SLA%'
      AND IsDeleted = 0;

    PRINT '';
    PRINT 'SUCCESS! SLA permissions are now active for System Admin role.';
    PRINT 'You can now test the SLA endpoints.';
END
GO
