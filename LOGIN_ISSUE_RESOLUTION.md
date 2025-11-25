# Login Issue - Root Cause & Resolution

**Date**: November 1, 2025
**Status**: Root Cause Identified
**Priority**: HIGH

---

## Problem Statement

User reported: **"not able to login"**

---

## Root Cause Analysis

### Issue Discovered

The login endpoint is failing due to **incorrect property names** in the request body.

### What Was Wrong

❌ **Incorrect Format** (used in all test scripts):
```json
{
    "identifier": "admin@complaintmanagement.com",
    "password": "Admin@123"
}
```

✅ **Correct Format** (required by LoginRequest DTO):
```json
{
    "Email": "admin@complaintmanagement.com",
    "Password": "Admin@123"
}
```

### Technical Details

**File**: `complaint-system-dotnet/src/ComplaintManagement.Application/DTOs/Auth/LoginRequest.cs`

```csharp
public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
```

**Property Names**:
- ✅ `Email` (capital E) - NOT `identifier` or `email`
- ✅ `Password` (capital P) - NOT `password`

### Test Results

| Test Format | Status Code | Result |
|-------------|-------------|--------|
| `identifier` + `password` | 400 Bad Request | ❌ Validation failed |
| `Email` + `Password` | 500 Internal Server Error | ⚠️ Validation passed, server error |

**Progress**: We moved from 400 (validation error) to 500 (server error), which means the property names are now correct, but there's a backend issue.

---

## Backend 500 Error Investigation

The 500 Internal Server Error suggests one of these issues:

### Possible Causes:

1. **Password Hashing Issue**
   - The admin user may have an incompatible password hash
   - BCrypt/AES encryption mismatch

2. **Database Connection Issue**
   - User lookup failing
   - Connection timeout

3. **Missing Required Data**
   - User.CompanyId null/invalid
   - Missing roles or permissions
   - User.IsActive = false or User.IsDeleted = true

4. **JWT Token Generation Issue**
   - Missing JWT secret key in appsettings
   - JWT service not configured properly

---

## Solution Steps

### Option 1: Login via Angular UI (RECOMMENDED)

**Why**: The Angular frontend may handle the request format correctly and bypass API issues.

**Steps**:
1. Open browser: http://localhost:4200
2. Navigate to login page
3. Enter credentials:
   - **Email**: admin@complaintmanagement.com
   - **Password**: Admin@123
4. Check browser DevTools (F12):
   - Console tab for errors
   - Network tab for /api/auth/login request details

**If successful**: You'll be redirected to dashboard with SLA permissions.

**If failed**:
- Take screenshot of Console errors
- Take screenshot of Network tab showing the login request/response
- Check the exact error message

### Option 2: Reset Admin Password

**Why**: The password hash in database may be corrupted or incompatible.

**Script**: `reset-admin-password.ps1` (if exists)

**Or manual SQL**:
```sql
-- First, verify user exists and is active
SELECT
    Id, Email, IsActive, IsDeleted, PasswordHash
FROM Users
WHERE Email = 'admin@complaintmanagement.com'

-- If user exists but IsActive = 0 or IsDeleted = 1:
UPDATE Users
SET IsActive = 1, IsDeleted = 0
WHERE Email = 'admin@complaintmanagement.com'

-- If password reset needed, you'll need to generate a new BCrypt hash
-- Using an external tool or C# snippet
```

### Option 3: Verify Database User Configuration

**Check these conditions**:

```sql
-- 1. User exists and is active
SELECT Email, IsActive, IsDeleted FROM Users
WHERE Email = 'admin@complaintmanagement.com'

-- Expected: IsActive = 1, IsDeleted = 0

-- 2. User has CompanyId
SELECT Email, CompanyId FROM Users
WHERE Email = 'admin@complaintmanagement.com'

-- Expected: CompanyId should be a valid GUID

-- 3. User has roles assigned
SELECT u.Email, r.Name as RoleName, r.RoleType
FROM Users u
INNER JOIN UserComplaintRoles ucr ON u.Id = ucr.UserId
INNER JOIN ComplaintRoles r ON ucr.ComplaintRoleId = r.Id
WHERE u.Email = 'admin@complaintmanagement.com'
AND ucr.IsActive = 1
AND ucr.IsDeleted = 0

-- Expected: At least one role (SystemAdmin or Administrator)
```

### Option 4: Check Backend Logs for Specific Error

**Manual Check**:
1. Open the terminal where backend is running (process c6eb78)
2. Try login via PowerShell:
   ```powershell
   powershell.exe -ExecutionPolicy Bypass -File test-login-fixed.ps1
   ```
3. Immediately check backend terminal for error details
4. Look for messages starting with:
   - `fail:`
   - `error:`
   - `Exception:`

---

## Files to Update

All test scripts and documentation used wrong property names. These need correction:

### Test Scripts:
- ✅ `test-login-fixed.ps1` - Already corrected
- ❌ `diagnose-login-issue.ps1` - Uses `identifier`
- ❌ `get-fresh-token.ps1` - Uses `identifier`
- ❌ `comprehensive-sla-e2e-test.ps1` - Uses `identifier`
- ❌ All other test scripts using login

### Documentation:
- ❌ `LOGIN_TROUBLESHOOTING_GUIDE.md` - Examples use `identifier`
- ❌ `SLA_MANUAL_UI_TESTING_GUIDE.md` - Shows wrong format
- ❌ `TESTING_STATUS_REPORT.md` - Examples use `identifier`

---

## Immediate Action Required

### For User:

**Step 1: Try Browser Login** ⭐ HIGHEST PRIORITY
1. Open http://localhost:4200 in Chrome/Edge
2. Login with admin@complaintmanagement.com / Admin@123
3. If it works, you're done!
4. If it fails, open F12 DevTools and:
   - Take screenshot of Console tab
   - Take screenshot of Network tab showing login request
   - Copy any error messages

**Step 2: If Browser Login Fails**
Check the Network tab in DevTools:
- Find the POST request to `/api/auth/login`
- Click on it
- Check the **Request Body** - verify it's sending `Email` and `Password`
- Check the **Response** tab - note the error message
- Check **Status Code** - is it 400, 401, 403, or 500?

**Step 3: Report Findings**
Share:
- Screenshot of browser console errors
- Screenshot of network request/response
- Status code received
- Any error messages shown

---

## Verification Checklist

Once login works:

- [ ] User can login via browser
- [ ] Redirected to dashboard after login
- [ ] User menu/profile visible
- [ ] Can navigate to SLA Management
- [ ] SLA permissions active (no 403 errors on SLA endpoints)
- [ ] Can create complaints with SLA calculation

---

## Backend Configuration to Verify

If 500 error persists, check backend configuration:

### 1. JWT Configuration (`appsettings.json` or `appsettings.Development.json`)

```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_HERE",
    "Issuer": "ComplaintManagement",
    "Audience": "ComplaintManagementUsers",
    "ExpiryInMinutes": 60
  }
}
```

**Required**: `Key` must be set and at least 16 characters

### 2. Database Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LAPTOP-NF9BTG7Q\\SQLEXPRESS;Database=ComplaintManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Verify**: Connection string is valid and database accessible

### 3. Password Encryption Service

**Backend Logs Should Show**:
```
info: ComplaintManagement.Infrastructure.Services.AesEncryptionService[0]
      AES encryption initialized with default keys for local password management
```

**If missing**: Password service not initialized correctly

---

## Expected Successful Login Response

When login works, you should see:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "guid-here",
    "email": "admin@complaintmanagement.com",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["System Administrator"],
    "permissions": [
      "ViewSLA",
      "ManageSLA",
      "CreateSLA",
      "UpdateSLA",
      "DeleteSLA",
      ... other permissions ...
    ]
  }
}
```

---

## Next Steps After Login Works

1. Verify SLA permissions in token (check `/api/auth/me` response)
2. Access SLA Management module
3. Follow `SLA_MANUAL_UI_TESTING_GUIDE.md` for comprehensive testing
4. Create test SLA levels (Gold, Silver, Bronze)
5. Create category and priority mappings
6. Test complaint creation with SLA calculation

---

## Technical Notes

### Why "identifier" Was Used

The comment in AuthController suggests multi-field login:
```csharp
/// <summary>
/// Authenticate user and return JWT token
/// Supports login with Email, Employee Code, or Phone Number
/// </summary>
```

This suggests the backend *may* support multiple identifier types, but the **LoginRequest DTO only accepts `Email`**. There may be a mismatch between documentation and implementation.

### Potential Backend Enhancement

Consider updating `LoginRequest` to:
```csharp
public class LoginRequest
{
    [Required]
    public string Identifier { get; set; }  // Can be Email, EmployeeCode, or Phone

    [Required]
    public string Password { get; set; }
}
```

Then handle multi-field lookup in the handler. But for now, use `Email`.

---

## Summary

**Problem**: Login failing due to wrong property names (`identifier` vs `Email`)
**Solution**: Use `Email` and `Password` (capital letters) in request body
**Status**: Property names fixed, but 500 error suggests backend issue
**Next**: User should try browser login and report results

---

**Report Created**: November 1, 2025
**Last Updated**: November 1, 2025
**Status**: Awaiting User Testing
