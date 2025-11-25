-- Fix User Login Issues
-- Generated: 2025-11-10 18:38:37

-- User 1: Nav Nainital (Complainant)
-- Email: nav_nainital@yahoo.com
-- Password: Nav@12345
UPDATE Users SET
    PasswordHash = 'U9PgR051Vnj0Q6DpvcP2+g==',
    MustChangePasswordOnNextLogin = 0,
    PasswordChangedAt = GETUTCDATE(),
    PasswordChangedBy = 'f56d8d03-e382-454b-bf7d-fa8236c125c3',
    FailedLoginAttempts = 0,
    AccountLockedUntil = NULL,
    PasswordNeverExpires = 0,
    IsActive = 1,
    UpdatedAt = GETUTCDATE()
WHERE Id = 'fd0073b8-fc95-4a49-867c-6ffb38b7d177';

-- User 2: Naveen Chandra (Handler)
-- Email: naveen.chandra@oryggitech.com
-- Password: Naveen@12345
UPDATE Users SET
    PasswordHash = 'qW03atWbDl3HauFlaYbyAQ==',
    MustChangePasswordOnNextLogin = 0,
    PasswordChangedAt = GETUTCDATE(),
    PasswordChangedBy = 'f56d8d03-e382-454b-bf7d-fa8236c125c3',
    FailedLoginAttempts = 0,
    AccountLockedUntil = NULL,
    PasswordNeverExpires = 0,
    IsActive = 1,
    UpdatedAt = GETUTCDATE()
WHERE Id = '94c91ae3-72ef-4b53-8057-08de0e0582b5';

-- Verify passwords were updated
SELECT
    Id,
    Email,
    FullName,
    EmployeeCode,
    IsActive,
    CASE WHEN PasswordHash IS NOT NULL THEN 'SET' ELSE 'NOT SET' END as PasswordStatus,
    MustChangePasswordOnNextLogin,
    FailedLoginAttempts,
    AccountLockedUntil,
    PasswordChangedAt
FROM Users
WHERE Id IN (
    'fd0073b8-fc95-4a49-867c-6ffb38b7d177',
    '94c91ae3-72ef-4b53-8057-08de0e0582b5'
);

-- Check role assignments
SELECT
    u.Id,
    u.Email,
    u.FullName,
    cr.Name as RoleName,
    cr.Code as RoleCode,
    cr.RoleType,
    ucr.IsPrimary,
    ucr.IsActive as RoleActive
FROM Users u
LEFT JOIN UserComplaintRoles ucr ON u.Id = ucr.UserId
LEFT JOIN ComplaintRoles cr ON ucr.ComplaintRoleId = cr.Id
WHERE u.Id IN (
    'fd0073b8-fc95-4a49-867c-6ffb38b7d177',
    '94c91ae3-72ef-4b53-8057-08de0e0582b5'
)
ORDER BY u.Email, cr.Name;

-- Assign COMPLAINANT role to Nav Nainital if not already assigned
IF NOT EXISTS (
    SELECT 1 FROM UserComplaintRoles ucr
    INNER JOIN ComplaintRoles cr ON ucr.ComplaintRoleId = cr.Id
    WHERE ucr.UserId = 'fd0073b8-fc95-4a49-867c-6ffb38b7d177'
    AND cr.Code = 'COMPLAINANT'
)
BEGIN
    DECLARE @ComplainantRoleId UNIQUEIDENTIFIER;
    SELECT @ComplainantRoleId = Id FROM ComplaintRoles WHERE Code = 'COMPLAINANT';

    INSERT INTO UserComplaintRoles (Id, UserId, ComplaintRoleId, IsPrimary, EffectiveFrom, IsActive, CreatedAt, Notes)
    VALUES (
        NEWID(),
        'fd0073b8-fc95-4a49-867c-6ffb38b7d177',
        @ComplainantRoleId,
        1,
        GETUTCDATE(),
        1,
        GETUTCDATE(),
        'Auto-assigned for login fix'
    );
    PRINT 'Complainant role assigned to Nav Nainital';
END
ELSE
BEGIN
    PRINT 'Nav Nainital already has Complainant role';
END

-- Assign LEVEL1_HANDLER role to Naveen Chandra if not already assigned
IF NOT EXISTS (
    SELECT 1 FROM UserComplaintRoles ucr
    INNER JOIN ComplaintRoles cr ON ucr.ComplaintRoleId = cr.Id
    WHERE ucr.UserId = '94c91ae3-72ef-4b53-8057-08de0e0582b5'
    AND cr.RoleType = 2 -- Handler role type
)
BEGIN
    DECLARE @HandlerRoleId UNIQUEIDENTIFIER;
    SELECT TOP 1 @HandlerRoleId = Id FROM ComplaintRoles
    WHERE Code LIKE '%HANDLER%' OR RoleType = 2
    ORDER BY EscalationLevel;

    INSERT INTO UserComplaintRoles (Id, UserId, ComplaintRoleId, IsPrimary, EffectiveFrom, IsActive, CreatedAt, Notes)
    VALUES (
        NEWID(),
        '94c91ae3-72ef-4b53-8057-08de0e0582b5',
        @HandlerRoleId,
        1,
        GETUTCDATE(),
        1,
        GETUTCDATE(),
        'Auto-assigned for login fix'
    );
    PRINT 'Handler role assigned to Naveen Chandra';
END
ELSE
BEGIN
    PRINT 'Naveen Chandra already has Handler role';
END

-- Final verification
SELECT
    u.Id,
    u.Email,
    u.FullName,
    u.IsActive,
    COUNT(ucr.Id) as RoleCount,
    STRING_AGG(cr.Name, ', ') as Roles
FROM Users u
LEFT JOIN UserComplaintRoles ucr ON u.Id = ucr.UserId AND ucr.IsActive = 1
LEFT JOIN ComplaintRoles cr ON ucr.ComplaintRoleId = cr.Id
WHERE u.Id IN (
    'fd0073b8-fc95-4a49-867c-6ffb38b7d177',
    '94c91ae3-72ef-4b53-8057-08de0e0582b5'
)
GROUP BY u.Id, u.Email, u.FullName, u.IsActive
ORDER BY u.Email;

PRINT 'User login fix SQL completed successfully';
