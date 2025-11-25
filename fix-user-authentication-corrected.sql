-- ========================================
-- FIX USER AUTHENTICATION (CORRECTED)
-- Script to enable login for both users
-- Date: 2025-11-10
-- Fixed: ComplaintRoleId column name
-- ========================================

USE ComplaintManagementDB;
GO

PRINT '========================================'
PRINT 'STARTING USER AUTHENTICATION FIX'
PRINT '========================================'
PRINT ''

-- ========================================
-- STEP 1: Fix nav_nainital@yahoo.com (Complainant)
-- Password: Nav@12345
-- Hash: U9PgR051Vnj0Q6DpvcP2+g== (AES encrypted)
-- ========================================

PRINT 'STEP 1: Fixing nav_nainital@yahoo.com (Complainant)'
PRINT '----------------------------------------------------'

UPDATE Users
SET
    PasswordHash = 'U9PgR051Vnj0Q6DpvcP2+g==',
    IsActive = 1,
    FailedLoginAttempts = 0,
    AccountLockedUntil = NULL,
    MustChangePasswordOnNextLogin = 0,
    PasswordChangedAt = GETUTCDATE(),
    UpdatedAt = GETUTCDATE()
WHERE Email = 'nav_nainital@yahoo.com';

IF @@ROWCOUNT > 0
    PRINT 'SUCCESS: Updated nav_nainital@yahoo.com user record'
ELSE
    PRINT 'WARNING: No user found with email nav_nainital@yahoo.com'

-- Ensure Complainant role is assigned
DECLARE @userId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'nav_nainital@yahoo.com');
DECLARE @roleId UNIQUEIDENTIFIER = (SELECT Id FROM ComplaintRoles WHERE Name = 'Complainant' AND IsDeleted = 0);

IF @userId IS NOT NULL AND @roleId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UserComplaintRoles WHERE UserId = @userId AND ComplaintRoleId = @roleId AND IsDeleted = 0)
    BEGIN
        INSERT INTO UserComplaintRoles (UserId, ComplaintRoleId, IsPrimary, IsActive, EffectiveFrom)
        VALUES (@userId, @roleId, 1, 1, GETUTCDATE());
        PRINT 'SUCCESS: Assigned Complainant role to nav_nainital@yahoo.com'
    END
    ELSE
    BEGIN
        UPDATE UserComplaintRoles
        SET IsActive = 1, IsPrimary = 1, EffectiveFrom = GETUTCDATE(), IsDeleted = 0, DeletedAt = NULL, DeletedBy = NULL
        WHERE UserId = @userId AND ComplaintRoleId = @roleId;
        PRINT 'SUCCESS: Updated Complainant role for nav_nainital@yahoo.com'
    END
END
ELSE
BEGIN
    IF @userId IS NULL
        PRINT 'ERROR: Could not find user nav_nainital@yahoo.com'
    IF @roleId IS NULL
        PRINT 'ERROR: Could not find Complainant role'
END

PRINT ''

-- ========================================
-- STEP 2: Fix naveen.chandra@oryggitech.com (Handler)
-- Password: Naveen@12345
-- Hash: qW03atWbDl3HauFlaYbyAQ== (AES encrypted)
-- ========================================

PRINT 'STEP 2: Fixing naveen.chandra@oryggitech.com (Handler)'
PRINT '----------------------------------------------------'

UPDATE Users
SET
    PasswordHash = 'qW03atWbDl3HauFlaYbyAQ==',
    IsActive = 1,
    FailedLoginAttempts = 0,
    AccountLockedUntil = NULL,
    MustChangePasswordOnNextLogin = 0,
    PasswordChangedAt = GETUTCDATE(),
    UpdatedAt = GETUTCDATE()
WHERE Email = 'naveen.chandra@oryggitech.com';

IF @@ROWCOUNT > 0
    PRINT 'SUCCESS: Updated naveen.chandra@oryggitech.com user record'
ELSE
    PRINT 'WARNING: No user found with email naveen.chandra@oryggitech.com'

-- Ensure Handler role is assigned
DECLARE @handlerUserId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'naveen.chandra@oryggitech.com');
DECLARE @handlerRoleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ComplaintRoles WHERE Name LIKE '%Handler%' AND IsDeleted = 0 ORDER BY Name);

IF @handlerUserId IS NOT NULL AND @handlerRoleId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UserComplaintRoles WHERE UserId = @handlerUserId AND ComplaintRoleId = @handlerRoleId AND IsDeleted = 0)
    BEGIN
        INSERT INTO UserComplaintRoles (UserId, ComplaintRoleId, IsPrimary, IsActive, EffectiveFrom)
        VALUES (@handlerUserId, @handlerRoleId, 1, 1, GETUTCDATE());
        PRINT 'SUCCESS: Assigned Handler role to naveen.chandra@oryggitech.com'
    END
    ELSE
    BEGIN
        UPDATE UserComplaintRoles
        SET IsActive = 1, IsPrimary = 1, EffectiveFrom = GETUTCDATE(), IsDeleted = 0, DeletedAt = NULL, DeletedBy = NULL
        WHERE UserId = @handlerUserId AND ComplaintRoleId = @handlerRoleId;
        PRINT 'SUCCESS: Updated Handler role for naveen.chandra@oryggitech.com'
    END
END
ELSE
BEGIN
    IF @handlerUserId IS NULL
        PRINT 'ERROR: Could not find user naveen.chandra@oryggitech.com'
    IF @handlerRoleId IS NULL
        PRINT 'ERROR: Could not find Handler role'
END

PRINT ''

-- ========================================
-- STEP 3: VERIFICATION - User Status
-- ========================================

PRINT 'STEP 3: VERIFICATION - User Status'
PRINT '----------------------------------------------------'

SELECT
    Email,
    FullName,
    PasswordHash,
    IsActive,
    FailedLoginAttempts,
    AccountLockedUntil,
    MustChangePasswordOnNextLogin,
    CASE
        WHEN IsActive = 1 AND PasswordHash IS NOT NULL AND AccountLockedUntil IS NULL
        THEN 'Ready for Login'
        ELSE 'Not Ready'
    END AS LoginStatus
FROM Users
WHERE Email IN ('nav_nainital@yahoo.com', 'naveen.chandra@oryggitech.com')
ORDER BY Email;

PRINT ''

-- ========================================
-- STEP 4: VERIFICATION - Role Assignments
-- ========================================

PRINT 'STEP 4: VERIFICATION - Role Assignments'
PRINT '----------------------------------------------------'

SELECT
    u.Email,
    u.FullName,
    r.Name AS RoleName,
    ucr.IsPrimary,
    ucr.IsActive,
    ucr.EffectiveFrom
FROM Users u
JOIN UserComplaintRoles ucr ON u.Id = ucr.UserId
JOIN ComplaintRoles r ON ucr.ComplaintRoleId = r.Id
WHERE u.Email IN ('nav_nainital@yahoo.com', 'naveen.chandra@oryggitech.com')
AND ucr.IsActive = 1
AND ucr.IsDeleted = 0
ORDER BY u.Email, r.Name;

PRINT ''

-- ========================================
-- STEP 5: Additional Diagnostic Information
-- ========================================

PRINT 'STEP 5: Additional Diagnostic Information'
PRINT '----------------------------------------------------'

-- Check if users exist
DECLARE @complainantExists BIT = (SELECT CASE WHEN EXISTS(SELECT 1 FROM Users WHERE Email = 'nav_nainital@yahoo.com') THEN 1 ELSE 0 END);
DECLARE @handlerExists BIT = (SELECT CASE WHEN EXISTS(SELECT 1 FROM Users WHERE Email = 'naveen.chandra@oryggitech.com') THEN 1 ELSE 0 END);

PRINT 'User Existence Check:'
PRINT '  nav_nainital@yahoo.com: ' + CASE WHEN @complainantExists = 1 THEN 'EXISTS' ELSE 'NOT FOUND' END
PRINT '  naveen.chandra@oryggitech.com: ' + CASE WHEN @handlerExists = 1 THEN 'EXISTS' ELSE 'NOT FOUND' END

-- Check available roles
PRINT ''
PRINT 'Available Roles in System:'
SELECT Name, Description, IsDeleted
FROM ComplaintRoles
WHERE IsDeleted = 0
ORDER BY Name;

PRINT ''
PRINT '========================================'
PRINT 'USER AUTHENTICATION FIX COMPLETED'
PRINT '========================================'
PRINT ''
PRINT 'NEXT STEPS:'
PRINT '1. Test login for nav_nainital@yahoo.com with password: Nav@12345'
PRINT '2. Test login for naveen.chandra@oryggitech.com with password: Naveen@12345'
PRINT '3. Verify JWT tokens are returned'
PRINT '4. Check role claims in tokens'
PRINT ''

GO
