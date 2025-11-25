-- Verify User Status Script
-- Run this BEFORE and AFTER applying the fix to see the changes

PRINT '========================================';
PRINT 'USER LOGIN STATUS VERIFICATION';
PRINT '========================================';
PRINT '';

-- Check if users exist
PRINT 'Checking if users exist...';
PRINT '';

SELECT
    Id,
    Email,
    FullName,
    EmployeeCode,
    CompanyId,
    IsActive,
    IsDeleted,
    CreatedAt
FROM Users
WHERE Id IN (
    'fd0073b8-fc95-4a49-867c-6ffb38b7d177',  -- Nav Nainital
    '94c91ae3-72ef-4b53-8057-08de0e0582b5'   -- Naveen Chandra
);

PRINT '';
PRINT '========================================';
PRINT 'PASSWORD & ACCOUNT STATUS';
PRINT '========================================';
PRINT '';

SELECT
    Id,
    Email,
    FullName,
    CASE
        WHEN PasswordHash IS NULL THEN 'NOT SET'
        WHEN PasswordHash = '' THEN 'EMPTY'
        ELSE 'SET'
    END as PasswordStatus,
    LEN(PasswordHash) as PasswordHashLength,
    IsActive,
    MustChangePasswordOnNextLogin,
    FailedLoginAttempts,
    CASE
        WHEN AccountLockedUntil IS NULL THEN 'Not Locked'
        WHEN AccountLockedUntil > GETUTCDATE() THEN 'LOCKED (Expires: ' + CONVERT(VARCHAR, AccountLockedUntil, 120) + ')'
        ELSE 'Lock Expired'
    END as LockoutStatus,
    PasswordChangedAt,
    PasswordExpiresAt,
    PasswordNeverExpires
FROM Users
WHERE Id IN (
    'fd0073b8-fc95-4a49-867c-6ffb38b7d177',
    '94c91ae3-72ef-4b53-8057-08de0e0582b5'
);

PRINT '';
PRINT '========================================';
PRINT 'ROLE ASSIGNMENTS';
PRINT '========================================';
PRINT '';

SELECT
    u.Id,
    u.Email,
    u.FullName,
    CASE
        WHEN cr.Id IS NULL THEN 'NO ROLES ASSIGNED'
        ELSE cr.Name
    END as RoleName,
    cr.Code as RoleCode,
    CASE cr.RoleType
        WHEN 0 THEN 'System'
        WHEN 1 THEN 'Complainant'
        WHEN 2 THEN 'Handler'
        WHEN 3 THEN 'Approver'
        WHEN 4 THEN 'Manager'
        ELSE 'Unknown'
    END as RoleType,
    cr.EscalationLevel,
    ucr.IsPrimary,
    ucr.IsActive as RoleActive,
    ucr.EffectiveFrom,
    ucr.EffectiveTo,
    ucr.Notes
FROM Users u
LEFT JOIN UserComplaintRoles ucr ON u.Id = ucr.UserId
LEFT JOIN ComplaintRoles cr ON ucr.ComplaintRoleId = cr.Id
WHERE u.Id IN (
    'fd0073b8-fc95-4a49-867c-6ffb38b7d177',
    '94c91ae3-72ef-4b53-8057-08de0e0582b5'
)
ORDER BY u.Email, cr.Name;

PRINT '';
PRINT '========================================';
PRINT 'SUMMARY';
PRINT '========================================';
PRINT '';

SELECT
    u.Email,
    u.FullName,
    u.IsActive,
    CASE
        WHEN u.PasswordHash IS NULL OR u.PasswordHash = '' THEN 'NO PASSWORD'
        ELSE 'PASSWORD SET'
    END as PasswordStatus,
    u.FailedLoginAttempts,
    CASE
        WHEN u.AccountLockedUntil IS NOT NULL AND u.AccountLockedUntil > GETUTCDATE() THEN 'LOCKED'
        ELSE 'UNLOCKED'
    END as AccountStatus,
    COUNT(DISTINCT ucr.Id) as ActiveRoleCount,
    STRING_AGG(cr.Name, ', ') WITHIN GROUP (ORDER BY cr.Name) as Roles,
    CASE
        WHEN u.IsActive = 0 THEN 'User is INACTIVE'
        WHEN u.PasswordHash IS NULL OR u.PasswordHash = '' THEN 'Password NOT SET'
        WHEN u.AccountLockedUntil IS NOT NULL AND u.AccountLockedUntil > GETUTCDATE() THEN 'Account LOCKED'
        WHEN COUNT(DISTINCT ucr.Id) = 0 THEN 'No roles assigned'
        ELSE 'Ready for Login'
    END as LoginReadiness
FROM Users u
LEFT JOIN UserComplaintRoles ucr ON u.Id = ucr.UserId AND ucr.IsActive = 1
LEFT JOIN ComplaintRoles cr ON ucr.ComplaintRoleId = cr.Id
WHERE u.Id IN (
    'fd0073b8-fc95-4a49-867c-6ffb38b7d177',
    '94c91ae3-72ef-4b53-8057-08de0e0582b5'
)
GROUP BY
    u.Email,
    u.FullName,
    u.IsActive,
    u.PasswordHash,
    u.FailedLoginAttempts,
    u.AccountLockedUntil
ORDER BY u.Email;

PRINT '';
PRINT '========================================';
PRINT 'AVAILABLE ROLES IN SYSTEM';
PRINT '========================================';
PRINT '';

SELECT
    Id,
    Code,
    Name,
    CASE RoleType
        WHEN 0 THEN 'System'
        WHEN 1 THEN 'Complainant'
        WHEN 2 THEN 'Handler'
        WHEN 3 THEN 'Approver'
        WHEN 4 THEN 'Manager'
        ELSE 'Unknown'
    END as RoleType,
    EscalationLevel,
    IsActive,
    RequiresResourcePool
FROM ComplaintRoles
WHERE IsDeleted = 0
ORDER BY RoleType, EscalationLevel;

PRINT '';
PRINT 'Verification complete.';
PRINT '';
