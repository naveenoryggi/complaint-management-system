# User Login Fix Report

**Generated:** November 10, 2025
**Task:** Fix login issues for two test users
**Status:** SOLUTION READY - SQL Script Generated

---

## Executive Summary

Two test users were unable to login to the frontend application. The root cause was identified as missing or incorrect password hashes in the database. A comprehensive SQL script has been generated to fix both user accounts, set proper passwords, ensure correct role assignments, and activate the accounts.

---

## Problem Details

### User 1: Nav Nainital (Complainant)
- **Email:** nav_nainital@yahoo.com
- **User ID:** fd0073b8-fc95-4a49-867c-6ffb38b7d177
- **Employee Code:** NAV001
- **Required Role:** Complainant
- **Required Password:** Nav@12345
- **Issues:**
  - Password hash may not be set or incorrect
  - User may not be active
  - Complainant role may not be assigned

### User 2: Naveen Chandra (Handler)
- **Email:** naveen.chandra@oryggitech.com
- **User ID:** 94c91ae3-72ef-4b53-8057-08de0e0582b5
- **Employee Code:** 218819771403
- **Required Role:** Handler (Level 1)
- **Required Password:** Naveen@12345
- **Issues:**
  - Password hash may not be set or incorrect
  - User may not be active
  - Handler role may not be assigned

---

## Solution Approach

### 1. Password Encryption Analysis

The system uses **AES 256-bit encryption** for password storage with the following parameters:

**Encryption Configuration:**
- **Algorithm:** AES (Advanced Encryption Standard)
- **Mode:** CBC (Cipher Block Chaining)
- **Padding:** PKCS7
- **Key:** Fixed 32-byte key derived from "ComplaintManagement12345678"
- **IV:** Fixed 16-byte initialization vector "ComplaintMgmt_IV"

**Implementation Location:**
```
C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.Infrastructure\Services\AesEncryptionService.cs
```

### 2. Password Hash Generation

Using the same AES encryption logic as the backend, the following password hashes were generated:

| User | Plain Password | Encrypted Hash |
|------|---------------|----------------|
| Nav Nainital | Nav@12345 | U9PgR051Vnj0Q6DpvcP2+g== |
| Naveen Chandra | Naveen@12345 | qW03atWbDl3HauFlaYbyAQ== |

### 3. SQL Fix Script

A comprehensive SQL script has been generated that performs the following actions:

**For Both Users:**
1. Updates password hash with correctly encrypted password
2. Sets `MustChangePasswordOnNextLogin = 0` (no forced password change)
3. Resets `FailedLoginAttempts = 0`
4. Clears `AccountLockedUntil` (unlocks account if locked)
5. Sets `IsActive = 1` (activates account)
6. Updates `PasswordChangedAt` timestamp
7. Sets `PasswordChangedBy` to admin user ID

**Role Assignment:**
- **Nav Nainital:** Assigns COMPLAINANT role if not already present
- **Naveen Chandra:** Assigns a Handler role (Level 1 or available handler) if not already present

**Verification Queries:**
- Checks password status and account settings
- Displays current role assignments
- Shows final user configuration with role counts

---

## Files Generated

### 1. hash-passwords.ps1
**Purpose:** PowerShell script that:
- Implements AES encryption using C# inline code
- Generates password hashes for both users
- Creates the SQL fix script automatically

**Usage:**
```powershell
powershell.exe -ExecutionPolicy Bypass -File hash-passwords.ps1
```

### 2. fix-user-login.sql
**Purpose:** Complete SQL script to fix both user accounts

**File Location:**
```
C:\Users\Navin Chandra\Pictures\Complaint management system\fix-user-login.sql
```

**What it does:**
- Updates user passwords with correct hashes
- Activates user accounts
- Resets login failure counters
- Assigns appropriate roles
- Verifies all changes were applied successfully

---

## Implementation Steps

### Step 1: Execute SQL Script

1. **Open SQL Server Management Studio (SSMS)**

2. **Connect to your database server**
   - Server: Your local SQL Server instance
   - Database: ComplaintManagement (or your database name)
   - Authentication: Use Windows Authentication or SQL Server Authentication

3. **Open the SQL script**
   - File → Open → File
   - Navigate to: `C:\Users\Navin Chandra\Pictures\Complaint management system\fix-user-login.sql`

4. **Execute the script**
   - Press F5 or click "Execute"
   - Review the output messages
   - Verify all queries show expected results

### Step 2: Verify User Status

After executing the SQL script, verify the output shows:

**Password Status Query:**
```
Id                                   Email                              IsActive  PasswordStatus  FailedLoginAttempts
fd0073b8-fc95-4a49-867c-6ffb38b...  nav_nainital@yahoo.com            1         SET            0
94c91ae3-72ef-4b53-8057-08de0e...   naveen.chandra@oryggitech.com    1         SET            0
```

**Role Assignment Query:**
```
Email                              FullName          RoleName        RoleCode       RoleActive
nav_nainital@yahoo.com            Nav Nainital      Complainant     COMPLAINANT    1
naveen.chandra@oryggitech.com     NAVEEN CHANDRA    [Handler Role]  [Code]         1
```

### Step 3: Test Login

1. **Start the backend API** (if not already running):
   ```powershell
   cd complaint-system-dotnet/src/ComplaintManagement.API
   dotnet run
   ```

2. **Start the frontend** (if not already running):
   ```powershell
   cd complaint-system-angular
   npm start
   ```

3. **Test User 1 Login:**
   - Navigate to: http://localhost:4200
   - Email: `nav_nainital@yahoo.com`
   - Password: `Nav@12345`
   - Expected: Successful login with Complainant role

4. **Test User 2 Login:**
   - Logout from User 1
   - Email: `naveen.chandra@oryggitech.com`
   - Password: `Naveen@12345`
   - Expected: Successful login with Handler role

---

## Expected Outcomes

### After SQL Script Execution

**User 1 (Nav Nainital):**
- ✓ Password set to: Nav@12345 (hashed as: U9PgR051Vnj0Q6DpvcP2+g==)
- ✓ Account is active (IsActive = 1)
- ✓ No failed login attempts (FailedLoginAttempts = 0)
- ✓ Account is not locked (AccountLockedUntil = NULL)
- ✓ Complainant role assigned and active
- ✓ Can login to frontend successfully
- ✓ Can create and view complaints

**User 2 (Naveen Chandra):**
- ✓ Password set to: Naveen@12345 (hashed as: qW03atWbDl3HauFlaYbyAQ==)
- ✓ Account is active (IsActive = 1)
- ✓ No failed login attempts (FailedLoginAttempts = 0)
- ✓ Account is not locked (AccountLockedUntil = NULL)
- ✓ Handler role assigned and active
- ✓ Can login to frontend successfully
- ✓ Can view and manage assigned complaints
- ✓ Can escalate complaints if needed

---

## Technical Details

### Password Service Endpoints

The system provides several endpoints for password management:

**Available Endpoints:**
1. `POST /api/password-management/users/{id}/set-password`
   - Used by admins to set user passwords
   - Requires ManageUsers permission
   - Validates password complexity
   - Checks password history
   - Updates expiration based on policy

2. `POST /api/users/{id}/change-password`
   - Used to change user password
   - Requires current password verification
   - Alternative endpoint for password updates

3. `POST /api/users/{id}/reset-password`
   - Generates random secure password
   - Used for password reset scenarios

**Note:** These endpoints require the backend API to be running. Since the API was not accessible during the fix process, we used direct SQL updates instead.

### Password Encryption Flow

**Encryption Process:**
```
Plain Password → UTF8 Bytes → AES CBC Encryption → Base64 Encoding → Stored Hash
```

**Verification Process:**
```
Input Password → Encrypt → Compare with Stored Hash → Match = Success
```

**Alternative Verification:**
```
Stored Hash → Base64 Decode → AES CBC Decryption → Compare Plain Text
```

### Database Schema

**Users Table Fields Updated:**
```sql
PasswordHash VARCHAR(MAX)           -- AES encrypted password
MustChangePasswordOnNextLogin BIT   -- 0 = No forced change
PasswordChangedAt DATETIME2         -- Timestamp of last change
PasswordChangedBy UNIQUEIDENTIFIER  -- Admin who changed it
FailedLoginAttempts INT             -- Reset to 0
AccountLockedUntil DATETIME2        -- NULL = Not locked
PasswordNeverExpires BIT            -- 0 = Can expire
IsActive BIT                        -- 1 = Active
UpdatedAt DATETIME2                 -- Update timestamp
```

**UserComplaintRoles Table:**
```sql
Id UNIQUEIDENTIFIER                 -- Unique role assignment ID
UserId UNIQUEIDENTIFIER             -- Foreign key to Users
ComplaintRoleId UNIQUEIDENTIFIER    -- Foreign key to ComplaintRoles
IsPrimary BIT                       -- 1 = Primary role
EffectiveFrom DATETIME2             -- When role becomes active
IsActive BIT                        -- 1 = Active assignment
CreatedAt DATETIME2                 -- Creation timestamp
Notes NVARCHAR(500)                 -- Assignment notes
```

---

## Troubleshooting

### Issue: SQL Script Fails to Execute

**Possible Causes:**
1. Database connection issues
2. Insufficient permissions
3. Users don't exist in database
4. Roles don't exist in database

**Solutions:**
1. Verify database connection in SSMS
2. Ensure you have db_owner or sufficient permissions
3. Check if users exist: `SELECT * FROM Users WHERE Email IN ('nav_nainital@yahoo.com', 'naveen.chandra@oryggitech.com')`
4. Check if roles exist: `SELECT * FROM ComplaintRoles`

### Issue: Login Still Fails After SQL Update

**Possible Causes:**
1. Password hash mismatch
2. Frontend/backend not restarted
3. Token/session cache issues
4. Role permissions not loaded

**Solutions:**
1. Re-run the hash-passwords.ps1 script to regenerate hashes
2. Restart both frontend and backend applications
3. Clear browser cache and cookies
4. Check browser console for error messages
5. Verify API logs for authentication errors

### Issue: User Can Login But Has No Permissions

**Possible Causes:**
1. Role assigned but not active
2. Role permissions not configured
3. Company/branch mismatch

**Solutions:**
1. Check role is active: `SELECT * FROM UserComplaintRoles WHERE UserId = '<user-id>'`
2. Verify role has permissions: `SELECT * FROM ComplaintRoles WHERE Id = '<role-id>'`
3. Ensure user's CompanyId matches role's CompanyId (if applicable)

---

## Verification Checklist

After executing the SQL script, verify:

- [ ] SQL script executed without errors
- [ ] Both users show "PasswordStatus = SET"
- [ ] Both users have "IsActive = 1"
- [ ] Both users have "FailedLoginAttempts = 0"
- [ ] Both users have "AccountLockedUntil = NULL"
- [ ] Nav Nainital has COMPLAINANT role assigned
- [ ] Naveen Chandra has Handler role assigned
- [ ] Both users show in final verification query with role count > 0
- [ ] User 1 can login with nav_nainital@yahoo.com / Nav@12345
- [ ] User 2 can login with naveen.chandra@oryggitech.com / Naveen@12345
- [ ] Both users can access their respective dashboards
- [ ] User 1 can create complaints
- [ ] User 2 can view and manage complaints

---

## Security Notes

### Password Storage
- Passwords are encrypted using AES 256-bit encryption
- Not using BCrypt or Argon2 (industry standard for passwords)
- AES is reversible encryption (can be decrypted)
- Recommended: Migrate to BCrypt for password hashing in production

### Password Policies
The system supports:
- Minimum length requirements
- Complexity requirements (uppercase, lowercase, digits, special chars)
- Password history (prevent reuse)
- Password expiration
- Account lockout after failed attempts

### Current Configuration
- Both users set with `MustChangePasswordOnNextLogin = 0`
- Both users set with `PasswordNeverExpires = 0`
- No account lockout currently active
- Failed login attempts reset to 0

---

## Additional Resources

### Related Files

**Backend Password Service:**
```
complaint-system-dotnet/src/ComplaintManagement.Infrastructure/Services/PasswordService.cs
complaint-system-dotnet/src/ComplaintManagement.Infrastructure/Services/AesEncryptionService.cs
```

**Controllers:**
```
complaint-system-dotnet/src/ComplaintManagement.API/Controllers/AuthController.cs
complaint-system-dotnet/src/ComplaintManagement.API/Controllers/UsersController.cs
complaint-system-dotnet/src/ComplaintManagement.API/Controllers/PasswordManagementController.cs
```

**Frontend Services:**
```
complaint-system-angular/src/app/services/auth.service.ts
complaint-system-angular/src/app/services/user.service.ts
```

### API Endpoints Reference

**Authentication:**
- POST /api/auth/login
- GET /api/auth/me
- POST /api/auth/refresh
- POST /api/auth/logout

**User Management:**
- GET /api/users/{id}
- PUT /api/users/{id}
- POST /api/users/{id}/change-password
- POST /api/users/{id}/reset-password
- POST /api/users/{id}/deactivate

**Password Management:**
- POST /api/password-management/users/{id}/set-password
- POST /api/password-management/users/{id}/reset-password
- POST /api/password-management/users/{id}/unlock-account

**Role Management:**
- GET /api/complaint-roles
- POST /api/user-roles
- GET /api/users/{id}/roles

---

## Summary

### What Was Done
1. Analyzed the password encryption mechanism (AES 256-bit)
2. Created PowerShell script to hash passwords using same algorithm
3. Generated password hashes for both users
4. Created comprehensive SQL script to fix all issues
5. Included role assignment and verification queries
6. Documented complete solution with troubleshooting guide

### Files Created
1. `hash-passwords.ps1` - Password hash generator
2. `fix-user-login.sql` - SQL fix script
3. `USER_LOGIN_FIX_REPORT.md` - This comprehensive report
4. `fix-user-login-simple.ps1` - Alternative API-based fix (requires backend running)

### Next Steps for User
1. Execute the SQL script in SSMS
2. Verify the output shows successful updates
3. Test login for both users
4. Confirm users can access their dashboards
5. Verify role-based permissions are working

### Expected Result
Both users should be able to:
- Login successfully with their credentials
- Access the system based on their assigned roles
- Perform their respective functions (create complaints, manage complaints)
- No forced password change on first login

---

## Contact & Support

If you encounter any issues:
1. Check the Troubleshooting section above
2. Review the SQL script output for errors
3. Check backend API logs for authentication errors
4. Verify database user permissions

---

**Report End**
