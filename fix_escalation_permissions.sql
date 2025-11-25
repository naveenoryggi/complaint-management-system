-- Fix Missing Escalation Permissions for System Admin Role
-- This script adds ViewEscalation and ManageEscalation permissions to the SYSTEM_ADMIN role

-- First, delete any existing entries (in case they exist but are soft-deleted or have wrong IsGranted flag)
DELETE FROM ComplaintRolePermissions
WHERE ComplaintRoleId IN (SELECT Id FROM ComplaintRoles WHERE Code = 'SYSTEM_ADMIN')
AND PermissionType IN ('ViewEscalation', 'ManageEscalation');

-- Now insert the permissions fresh
INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, CreatedAt, IsDeleted)
SELECT
    NEWID() AS Id,
    cr.Id AS ComplaintRoleId,
    'ViewEscalation' AS PermissionType,
    1 AS IsGranted,
    GETUTCDATE() AS CreatedAt,
    0 AS IsDeleted
FROM ComplaintRoles cr
WHERE cr.Code = 'SYSTEM_ADMIN';

INSERT INTO ComplaintRolePermissions (Id, ComplaintRoleId, PermissionType, IsGranted, CreatedAt, IsDeleted)
SELECT
    NEWID() AS Id,
    cr.Id AS ComplaintRoleId,
    'ManageEscalation' AS PermissionType,
    1 AS IsGranted,
    GETUTCDATE() AS CreatedAt,
    0 AS IsDeleted
FROM ComplaintRoles cr
WHERE cr.Code = 'SYSTEM_ADMIN';

-- Verify the fix
SELECT
    cr.Name AS RoleName,
    cr.Code AS RoleCode,
    crp.PermissionType,
    crp.IsGranted,
    crp.IsDeleted
FROM ComplaintRoles cr
INNER JOIN ComplaintRolePermissions crp ON cr.Id = crp.ComplaintRoleId
WHERE cr.Code = 'SYSTEM_ADMIN'
AND crp.PermissionType IN ('ViewEscalation', 'ManageEscalation');
