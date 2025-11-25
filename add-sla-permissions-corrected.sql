-- Add SLA Permissions to Admin Role (CORRECTED)
-- Uses correct column names: ComplaintRoleId, PermissionType, IsGranted

USE ComplaintManagementDB;
GO

-- Get Admin Role ID
DECLARE @AdminRoleId UNIQUEIDENTIFIER;
SELECT @AdminRoleId = Id FROM ComplaintRoles WHERE Code = 'ADMIN' AND IsDeleted = 0;

-- Check if admin role exists
IF @AdminRoleId IS NULL
BEGIN
    PRINT 'ERROR: Admin role not found!';
    SELECT * FROM ComplaintRoles WHERE IsDeleted = 0;
END
ELSE
BEGIN
    PRINT 'Admin Role ID: ' + CAST(@AdminRoleId AS NVARCHAR(50));

    -- Add ViewSLA permission
    IF NOT EXISTS (SELECT 1 FROM ComplaintRolePermissions WHERE ComplaintRoleId = @AdminRoleId AND PermissionType = 'ViewSLA' AND IsDeleted = 0)
    BEGIN
        INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, IsDeleted, CreatedAt)
        VALUES (NEWID(), @AdminRoleId, 'ViewSLA', 1, 0, GETUTCDATE());
        PRINT 'ViewSLA permission added';
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
        PRINT 'ManageSLA permission added';
    END
    ELSE
    BEGIN
        PRINT 'ManageSLA permission already exists';
    END

    PRINT '';
    PRINT '========================================';
    PRINT 'SLA Permissions Summary:';
    PRINT '========================================';

    -- Show all SLA permissions
    SELECT PermissionType, IsGranted, CreatedAt
    FROM ComplaintRolePermissions
    WHERE ComplaintRoleId = @AdminRoleId
      AND PermissionType LIKE '%SLA%'
      AND IsDeleted = 0;

    PRINT '';
    PRINT 'SUCCESS! SLA permissions are now active.';
END
GO
