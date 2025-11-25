# Hash passwords using the same AES encryption logic as the backend

Add-Type -TypeDefinition @"
using System;
using System.Security.Cryptography;
using System.Text;

public class PasswordHasher
{
    private static byte[] _key;
    private static byte[] _iv;

    static PasswordHasher()
    {
        // Using the same default keys as AesEncryptionService.cs
        var defaultKey = "ComplaintManagement12345678"; // 28 chars + padding to 32
        var defaultIV = "ComplaintMgmt_IV"; // 16 chars exactly

        _key = new byte[32];
        _iv = new byte[16];

        var keyBytes = Encoding.UTF8.GetBytes(defaultKey);
        var ivBytes = Encoding.UTF8.GetBytes(defaultIV);

        Array.Copy(keyBytes, _key, Math.Min(keyBytes.Length, 32));
        Array.Copy(ivBytes, _iv, Math.Min(ivBytes.Length, 16));
    }

    public static string HashPassword(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            throw new ArgumentException("Password cannot be null or empty");
        }

        using (var aes = Aes.Create())
        {
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor())
            {
                var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                var ciphertextBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
                return Convert.ToBase64String(ciphertextBytes);
            }
        }
    }
}
"@

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Password Hash Generator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Hash passwords
$password1 = "Nav@12345"
$password2 = "Naveen@12345"

Write-Host "Hashing passwords..." -ForegroundColor Cyan
$hash1 = [PasswordHasher]::HashPassword($password1)
$hash2 = [PasswordHasher]::HashPassword($password2)

Write-Host "Password 1: $password1" -ForegroundColor Yellow
Write-Host "Hash 1: $hash1" -ForegroundColor Green
Write-Host ""
Write-Host "Password 2: $password2" -ForegroundColor Yellow
Write-Host "Hash 2: $hash2" -ForegroundColor Green
Write-Host ""

# Generate SQL script
$sqlScript = @"
-- Fix User Login Issues
-- Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

-- User 1: Nav Nainital (Complainant)
-- Email: nav_nainital@yahoo.com
-- Password: $password1
UPDATE Users SET
    PasswordHash = '$hash1',
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
-- Password: $password2
UPDATE Users SET
    PasswordHash = '$hash2',
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
"@

# Save SQL script
$sqlScript | Out-File "fix-user-login.sql" -Encoding UTF8

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SQL Script Generated" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "SQL script saved to: fix-user-login.sql" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Open SQL Server Management Studio" -ForegroundColor White
Write-Host "2. Connect to your database" -ForegroundColor White
Write-Host "3. Open and execute the file: fix-user-login.sql" -ForegroundColor White
Write-Host "4. Verify the users can now login" -ForegroundColor White
Write-Host ""
Write-Host "User credentials after fix:" -ForegroundColor Yellow
Write-Host "  User 1: nav_nainital@yahoo.com / $password1" -ForegroundColor White
Write-Host "  User 2: naveen.chandra@oryggitech.com / $password2" -ForegroundColor White
Write-Host ""
