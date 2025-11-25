-- Seed Default Password Policies for All Companies
-- Run this script to create password policies with Microsoft Teams-style settings

-- Get all company IDs
DECLARE @Companies TABLE (CompanyId UNIQUEIDENTIFIER);
INSERT INTO @Companies (CompanyId)
SELECT Id FROM Companies WHERE IsDeleted = 0;

-- Create password policy for each company (if not exists)
INSERT INTO PasswordPolicies (
    Id,
    CompanyId,
    MinPasswordLength,
    RequireUppercase,
    RequireLowercase,
    RequireDigit,
    RequireSpecialChar,
    PasswordExpirationDays,
    PasswordHistoryCount,
    MaxFailedLoginAttempts,
    AccountLockoutDurationMinutes,
    EnforcePasswordExpiration,
    EnforcePasswordHistory,
    AllowPasswordReset,
    RequirePasswordChangeOnFirstLogin,
    MinPasswordAge,
    IsActive,
    CreatedAt,
    CreatedBy,
    IsDeleted
)
SELECT
    NEWID(),                          -- Id
    c.CompanyId,                      -- CompanyId
    8,                                -- MinPasswordLength (Microsoft Teams standard)
    1,                                -- RequireUppercase
    1,                                -- RequireLowercase
    1,                                -- RequireDigit
    1,                                -- RequireSpecialChar
    90,                               -- PasswordExpirationDays (3 months)
    5,                                -- PasswordHistoryCount (last 5 passwords)
    5,                                -- MaxFailedLoginAttempts
    15,                               -- AccountLockoutDurationMinutes
    1,                                -- EnforcePasswordExpiration
    1,                                -- EnforcePasswordHistory
    1,                                -- AllowPasswordReset
    0,                                -- RequirePasswordChangeOnFirstLogin (optional)
    1,                                -- MinPasswordAge (1 day)
    1,                                -- IsActive
    GETUTCDATE(),                     -- CreatedAt
    NULL,                             -- CreatedBy (system)
    0                                 -- IsDeleted
FROM @Companies c
WHERE NOT EXISTS (
    SELECT 1
    FROM PasswordPolicies pp
    WHERE pp.CompanyId = c.CompanyId
      AND pp.IsDeleted = 0
);

-- Display created policies
SELECT
    c.CompanyName,
    pp.MinPasswordLength,
    pp.RequireUppercase,
    pp.RequireLowercase,
    pp.RequireDigit,
    pp.RequireSpecialChar,
    pp.PasswordExpirationDays,
    pp.PasswordHistoryCount,
    pp.MaxFailedLoginAttempts,
    pp.AccountLockoutDurationMinutes,
    pp.CreatedAt
FROM PasswordPolicies pp
INNER JOIN Companies c ON pp.CompanyId = c.Id
WHERE pp.IsDeleted = 0
ORDER BY c.CompanyName;

PRINT 'Password policies created successfully!';
