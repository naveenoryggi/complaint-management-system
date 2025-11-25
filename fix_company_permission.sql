-- Fix Missing ManageCompany Permission for System Admin Role
-- This script adds the ManageCompany permission (value 20) to the SYSTEM_ADMIN role

-- Step 1: Delete any existing ManageCompany permissions (cleanup)
DELETE FROM ComplaintRolePermissions
WHERE ComplaintRoleId IN (SELECT Id FROM ComplaintRoles WHERE Code = 'SYSTEM_ADMIN')
AND PermissionType = 'ManageCompany';

-- Step 2: Insert ManageCompany permission for SYSTEM_ADMIN role
INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, CreatedAt, IsDeleted)
SELECT
    NEWID() AS Id,
    cr.Id AS ComplaintRoleId,
    'ManageCompany' AS PermissionType,
    1 AS IsGranted,
    GETUTCDATE() AS CreatedAt,
    0 AS IsDeleted
FROM ComplaintRoles cr
WHERE cr.Code = 'SYSTEM_ADMIN';

-- Step 3: Verify the fix
SELECT
    cr.Name AS RoleName,
    cr.Code AS RoleCode,
    crp.PermissionType,
    crp.IsGranted,
    crp.IsDeleted,
    crp.CreatedAt
FROM ComplaintRoles cr
INNER JOIN ComplaintRolePermissions crp ON cr.Id = crp.ComplaintRoleId
WHERE cr.Code = 'SYSTEM_ADMIN'
AND crp.PermissionType = 'ManageCompany'
ORDER BY crp.PermissionType;

PRINT 'ManageCompany permission has been successfully added to SYSTEM_ADMIN role';
