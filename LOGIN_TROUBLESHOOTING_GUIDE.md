# Login Troubleshooting Guide

**Issue**: Unable to login to the application
**Date**: November 1, 2025

---

## Quick Diagnostics Checklist

### 1. Check Both Servers Are Running

**Backend (API)**:
- URL: http://localhost:5058
- Check console/terminal for errors
- Should see: "Now listening on: http://localhost:5058"

**Frontend (Angular)**:
- URL: http://localhost:4200
- Check console/terminal for errors
- Should see: "Application bundle generation complete"

### 2. Verify Login Credentials

**Default Admin Credentials**:
- **Email**: admin@complaintmanagement.com
- **Password**: Admin@123

**Important**:
- Email is case-sensitive
- Password is case-sensitive
- No spaces before/after

---

## Step-by-Step Login Process

### Method 1: Manual Browser Login

**Step 1: Open Application**
1. Open browser (Chrome/Edge recommended)
2. Navigate to: http://localhost:4200
3. Wait for page to load completely

**Step 2: Check Page Loaded**
Look for:
- ✅ Login form visible
- ✅ Email and Password fields present
- ✅ Login button visible
- ✅ No console errors (F12 → Console tab)

**Step 3: Enter Credentials**
1. Click in Email field
2. Type: admin@complaintmanagement.com
3. Click in Password field
4. Type: Admin@123

**Step 4: Submit**
1. Click "Login" button
2. Watch for loading indicator
3. Wait for redirect

**Expected Result**: Redirected to dashboard/home page

---

## Common Login Issues & Solutions

### Issue 1: "Invalid credentials" or 401 Error

**Possible Causes**:
- Wrong email/password
- Database not seeded
- User not active

**Solution**:
```powershell
# Reset admin password
powershell.exe -ExecutionPolicy Bypass -File reset-admin-password.ps1
```

Or manually reset via SQL:
```sql
UPDATE Users
SET PasswordHash = '$2a$11$YourHashedPasswordHere', IsActive = 1
WHERE Email = 'admin@complaintmanagement.com'
```

### Issue 2: 403 Forbidden Error

**Possible Causes**:
- User doesn't have required role
- Role permissions missing
- User is inactive

**Solution**:
```sql
-- Check user status
SELECT Email, IsActive, IsDeleted FROM Users
WHERE Email = 'admin@complaintmanagement.com'

-- Activate user if needed
UPDATE Users
SET IsActive = 1, IsDeleted = 0
WHERE Email = 'admin@complaintmanagement.com'
```

### Issue 3: 500 Internal Server Error

**Possible Causes**:
- Database connection issue
- Backend service crashed
- Missing migrations

**Solution**:
1. Check backend console for error details
2. Restart backend server
3. Check database connection string
4. Apply migrations:
```bash
cd complaint-system-dotnet/src/ComplaintManagement.API
dotnet ef database update
```

### Issue 4: Network Error / Cannot Connect

**Possible Causes**:
- Backend not running
- Wrong URL
- Port conflict

**Solution**:
1. Verify backend running: http://localhost:5058/health
2. Check port 5058 not blocked:
```powershell
netstat -ano | findstr :5058
```
3. Restart backend if needed

### Issue 5: Login Button Not Working

**Possible Causes**:
- JavaScript error
- Form validation failed
- CORS issue

**Solution**:
1. Open browser console (F12)
2. Look for red error messages
3. Check Network tab for failed requests
4. Clear browser cache: Ctrl+Shift+Delete

### Issue 6: Infinite Loading / No Response

**Possible Causes**:
- API timeout
- Database query hanging
- Network issue

**Solution**:
1. Check Network tab in browser (F12)
2. Look for request to /api/auth/login
3. Check response status and time
4. Restart backend if timeout

---

## Detailed Troubleshooting Steps

### Step 1: Check Backend API Health

**Test 1: API Reachable**
```powershell
# PowerShell command
Invoke-WebRequest -Uri "http://localhost:5058" -Method GET
```

**Expected**: Should return 200 OK or redirect

**Test 2: Login Endpoint Exists**
```powershell
# Try login via PowerShell
$body = @{
    identifier = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5058/api/auth/login" -Method Post -Body $body -ContentType "application/json"
```

**Expected**: Should return token or error message

### Step 2: Check Database Connection

**Verify User Exists**:
```sql
SELECT Id, Email, IsActive, IsDeleted, LastLoginAt
FROM Users
WHERE Email = 'admin@complaintmanagement.com'
```

**Expected Result**:
- User record found ✅
- IsActive = 1 ✅
- IsDeleted = 0 ✅

**Check User Role**:
```sql
SELECT u.Email, r.Name as RoleName, r.RoleType
FROM Users u
JOIN UserComplaintRoles ucr ON u.Id = ucr.UserId
JOIN ComplaintRoles r ON ucr.ComplaintRoleId = r.Id
WHERE u.Email = 'admin@complaintmanagement.com'
AND ucr.IsActive = 1
AND ucr.IsDeleted = 0
```

**Expected Result**: Should have SystemAdmin or Administrator role

### Step 3: Check Frontend Console

**Open Browser Developer Tools**:
1. Press F12
2. Go to Console tab
3. Look for errors

**Common Console Errors**:

**Error**: `CORS policy` error
**Solution**: Check backend CORS configuration allows localhost:4200

**Error**: `404 Not Found` on login endpoint
**Solution**: Verify backend URL in environment.ts

**Error**: `Cannot read property of undefined`
**Solution**: Check Angular routing configuration

### Step 4: Check Network Tab

**In Browser Developer Tools**:
1. Press F12
2. Go to Network tab
3. Try login
4. Look for /api/auth/login request

**Check Request**:
- Method: POST ✅
- URL: http://localhost:5058/api/auth/login ✅
- Status: Should be 200 ✅
- Response: Should contain token ✅

**If Status is 400**:
- Check request payload format
- Verify credentials spelling

**If Status is 401**:
- Wrong credentials
- User not found

**If Status is 500**:
- Check backend logs
- Database error likely

---

## Manual Login via API (Bypass UI)

If UI login not working, test via API directly:

```powershell
# Get token via PowerShell
$body = @{
    identifier = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5058/api/auth/login" -Method Post -Body $body -ContentType "application/json"

# Save token
$response.token | Out-File -FilePath ".test-token" -NoNewline

Write-Host "Token saved to .test-token"
Write-Host "Token: $($response.token.Substring(0, 50))..."

# Verify token works
$token = (Get-Content .test-token -Raw).Trim()
$headers = @{"Authorization" = "Bearer $token"}

Invoke-RestMethod -Uri "http://localhost:5058/api/auth/me" -Headers $headers -Method Get
```

**Expected**: Should return user details

---

## Browser-Specific Issues

### Chrome
- Clear cache: Ctrl+Shift+Delete
- Disable extensions temporarily
- Try incognito mode

### Edge
- Clear cache: Ctrl+Shift+Delete
- Reset settings if needed
- Try InPrivate window

### Firefox
- Clear cache: Ctrl+Shift+Delete
- Check security settings
- Try private window

---

## Environment Configuration Check

**Check Frontend Environment File**:
```typescript
// complaint-system-angular/src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5058/api'  // ← Verify this URL
};
```

**Should Match Backend URL**: http://localhost:5058

---

## Advanced Troubleshooting

### Check JWT Secret Configuration

**Backend**: Verify appsettings.json or appsettings.Development.json

```json
{
  "Jwt": {
    "Key": "YourSecretKeyHere",  // Must be set
    "Issuer": "ComplaintManagement",
    "Audience": "ComplaintManagementUsers",
    "ExpiryInMinutes": 60
  }
}
```

### Check Database Connection String

**Backend**: Verify connection string in appsettings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LAPTOP-NF9BTG7Q\\SQLEXPRESS;Database=ComplaintManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Test Connection**:
```powershell
sqlcmd -S "LAPTOP-NF9BTG7Q\SQLEXPRESS" -d ComplaintManagementDB -E -Q "SELECT COUNT(*) FROM Users"
```

---

## Quick Reset Procedure

If all else fails, try this complete reset:

### Step 1: Stop All Services
```powershell
# Stop backend (Ctrl+C in terminal)
# Stop frontend (Ctrl+C in terminal)
```

### Step 2: Clear Browser Data
- Close all browser tabs
- Clear cache and cookies
- Restart browser

### Step 3: Verify Database
```sql
-- Verify admin user
SELECT * FROM Users WHERE Email = 'admin@complaintmanagement.com'

-- Reset password if needed
UPDATE Users
SET PasswordHash = '$2a$11$SomeHashHere',
    IsActive = 1,
    IsDeleted = 0
WHERE Email = 'admin@complaintmanagement.com'
```

### Step 4: Restart Services
```bash
# Terminal 1: Backend
cd complaint-system-dotnet/src/ComplaintManagement.API
dotnet run

# Terminal 2: Frontend
cd complaint-system-angular
npm start
```

### Step 5: Wait for Complete Startup
- Backend: Wait for "Now listening on: http://localhost:5058"
- Frontend: Wait for "Application bundle generation complete"

### Step 6: Try Login Again
- Open: http://localhost:4200
- Login: admin@complaintmanagement.com / Admin@123

---

## Success Indicators

**Login Successful When**:
- ✅ Redirected to dashboard
- ✅ User menu/name visible in header
- ✅ Navigation menu available
- ✅ No error messages
- ✅ Console shows no errors

---

## Get Additional Help

**Check These Files**:
1. Backend logs (terminal output)
2. Browser console (F12 → Console)
3. Network tab (F12 → Network)
4. Backend appsettings.json
5. Frontend environment.ts

**Collect This Information**:
- Error messages (exact text)
- Browser console errors
- Network request details
- Backend log errors
- Database query results

---

## Contact Information

If issue persists, provide:
1. Error message screenshot
2. Browser console output
3. Network tab screenshot
4. Backend terminal output
5. Steps you tried

---

**Last Updated**: November 1, 2025
**Status**: Active troubleshooting guide
