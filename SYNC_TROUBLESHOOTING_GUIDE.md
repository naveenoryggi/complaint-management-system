# Oryggi Sync Troubleshooting Guide

## Issue: Users Not Showing After Sync

### 🔍 Step 1: Check if Sync Actually Ran

**In the Oryggi Sync page:**
1. Look at the **Latest Sync Status** card
2. Check the **Sync History** table
3. Look for the number of employees processed/created

**Expected**: You should see numbers like:
- `Employees Processed: XX`
- `Employees Created: XX`
- `Users Created: XX`

If these are all 0, the sync didn't find any employees.

---

### 🗄️ Step 2: Run Database Checks

Open SQL Server Management Studio and run the `DEBUG_USERS.sql` script I created.

**Key things to check:**

#### Check 1: Are users in the database?
```sql
SELECT COUNT(*) FROM Users WHERE IsDeleted = 0 AND IsActive = 1;
```

**Expected**: Should be > 1 (at least the admin user)

#### Check 2: Did the sync create users?
```sql
SELECT TOP 1
    EmployeesCreated,
    UsersCreated,
    Status,
    ErrorMessage
FROM SyncLogs
ORDER BY StartedAt DESC;
```

**Expected**: UsersCreated > 0 and Status = 'SUCCESS'

#### Check 3: Do companies exist?
```sql
SELECT * FROM Companies WHERE IsDeleted = 0;
```

**Expected**: At least one company must exist. If no companies, users can't be created.

---

### 🐛 Common Issues and Solutions

#### Issue 1: No Companies Synced
**Symptom**: `CompaniesCreated = 0` in sync logs

**Cause**: Oryggi database might not have any companies, or connection is wrong

**Solution**:
1. Check Oryggi database connection string in `appsettings.json`
2. Verify you can query the Oryggi database:
   ```sql
   SELECT * FROM OryggiHRMS.dbo.CompanyMaster;
   ```

#### Issue 2: Companies Synced but No Employees
**Symptom**: `CompaniesCreated > 0` but `EmployeesCreated = 0`

**Cause**: Employees are being filtered out

**Check** what's in Oryggi:
```sql
-- Run this against ORYGGI database
SELECT COUNT(*) AS TotalEmployees
FROM EmployeeMaster
WHERE (Active = 1 OR Active IS NULL)
  AND Ecode != 1  -- Not admin
  AND (CorpEmpCode IS NULL OR CorpEmpCode NOT LIKE '%_%');  -- Not system accounts
```

If this returns 0, all employees are being filtered.

**Solution**: Either:
- Add employees to Oryggi that don't have underscore in CorpEmpCode
- Modify the filter in `OryggiSyncService.cs` line 355-357

#### Issue 3: Users Created but Not Showing in UI
**Symptom**: Database has users, but UI shows empty

**Cause**: Users might be inactive, or API is not returning them

**Check**:
```sql
SELECT
    COUNT(*) AS Total,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS Active,
    SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) AS Inactive
FROM Users
WHERE IsDeleted = 0;
```

**Solution**: If users are inactive, update them:
```sql
UPDATE Users
SET IsActive = 1
WHERE IsDeleted = 0 AND IsActive = 0;
```

#### Issue 4: CompanyId is Empty GUID
**Symptom**: Users have `CompanyId = 00000000-0000-0000-0000-000000000000`

**Cause**: Employees in Oryggi don't have a section/department/branch that links to a company

**Check**:
```sql
SELECT COUNT(*)
FROM Users
WHERE CompanyId = '00000000-0000-0000-0000-000000000000';
```

**Solution**: Check the organizational hierarchy:
1. Ensure sections link to departments
2. Ensure departments link to branches
3. Ensure branches link to companies

#### Issue 5: API Endpoint Not Working
**Test the API directly in browser console:**
```javascript
fetch('http://localhost:5058/api/users', {
  headers: {
    'Authorization': 'Bearer ' + localStorage.getItem('complaint_system_token')
  }
})
.then(r => r.json())
.then(console.log)
.catch(console.error);
```

**Expected**: Should return `{ isSuccess: true, data: [...] }`

If it fails, check:
- Backend is running (http://localhost:5058)
- You're logged in (token exists)
- CORS is configured correctly

---

### 📊 Step 3: Check Browser Console

1. Open browser Dev Tools (F12)
2. Go to User Management page
3. Check Console tab for errors
4. Check Network tab:
   - Look for request to `/api/users`
   - Check if it's returning 200 OK
   - Check response body

**Common errors:**
- **401 Unauthorized**: Token expired, log in again
- **404 Not Found**: Backend API not running
- **500 Internal Server Error**: Database or backend issue

---

### 🔧 Step 4: Manual Fix if Needed

If sync isn't working, you can manually create a test user:

```sql
-- Create a test user manually
DECLARE @UserId UNIQUEIDENTIFIER = NEWID();
DECLARE @CompanyId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Companies WHERE IsDeleted = 0);

INSERT INTO Users (
    Id,
    CompanyId,
    EmployeeCode,
    FirstName,
    LastName,
    Email,
    Phone,
    JobTitle,
    IsActive,
    IsDeleted,
    CreatedAt,
    PasswordHash
)
VALUES (
    @UserId,
    @CompanyId,
    'TEST001',
    'Test',
    'User',
    'test@test.com',
    '1234567890',
    'Test Position',
    1,  -- IsActive = true
    0,  -- IsDeleted = false
    GETDATE(),
    'dummy_hash'  -- Won't be able to login, but will show in list
);

SELECT * FROM Users WHERE Id = @UserId;
```

This will create a user that should appear in the UI.

---

### ✅ Success Checklist

- [ ] Sync logs show Status = 'SUCCESS'
- [ ] EmployeesCreated > 0 in sync logs
- [ ] UsersCreated > 0 in sync logs
- [ ] Database query returns users: `SELECT COUNT(*) FROM Users WHERE IsActive = 1`
- [ ] User Management page loads without errors
- [ ] `/api/users` endpoint returns data
- [ ] Users appear in the UI table

---

### 🆘 Still Not Working?

1. **Check backend logs** in the console where you ran `dotnet run`
2. **Try refreshing Angular** (Ctrl + Shift + R in browser)
3. **Check if you have permission** to view users (ManageUsers permission)
4. **Verify Oryggi database connection** in appsettings.json
5. **Run sync again** and watch the progress bar to see what stage fails

---

### 📞 Next Steps

If none of the above works:
1. Run `DEBUG_USERS.sql` and share the results
2. Check the latest sync log error message
3. Check if there are employees in the Oryggi database
4. Verify the connection string is correct
