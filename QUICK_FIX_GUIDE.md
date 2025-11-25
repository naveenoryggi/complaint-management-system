# Quick Fix Guide - User Login Issues

## Problem
Two test users cannot login:
- nav_nainital@yahoo.com (Complainant)
- naveen.chandra@oryggitech.com (Handler)

## Solution (3 Simple Steps)

### Step 1: Run SQL Script
```
1. Open SQL Server Management Studio
2. Connect to your database
3. Open file: fix-user-login.sql
4. Press F5 to execute
```

### Step 2: Verify Changes
```
1. Open file: verify-user-status.sql
2. Press F5 to execute
3. Check "LoginReadiness" column shows "Ready for Login"
```

### Step 3: Test Login
```
User 1:
  Email: nav_nainital@yahoo.com
  Password: Nav@12345

User 2:
  Email: naveen.chandra@oryggitech.com
  Password: Naveen@12345
```

## Files Overview

| File | Purpose |
|------|---------|
| `fix-user-login.sql` | Main SQL script to fix both users |
| `verify-user-status.sql` | Check user status before/after fix |
| `hash-passwords.ps1` | Regenerate password hashes if needed |
| `USER_LOGIN_FIX_REPORT.md` | Complete technical documentation |

## Expected Results

After running fix-user-login.sql:

**Nav Nainital:**
- ✓ Password: Nav@12345
- ✓ Active: Yes
- ✓ Role: Complainant
- ✓ Can Login: Yes

**Naveen Chandra:**
- ✓ Password: Naveen@12345
- ✓ Active: Yes
- ✓ Role: Handler (Level 1)
- ✓ Can Login: Yes

## Troubleshooting

**If SQL fails:**
- Check you're connected to correct database
- Verify you have db_owner permissions
- Ensure users exist in database

**If login still fails:**
- Clear browser cache
- Restart backend API
- Restart frontend app
- Check verify-user-status.sql output

## Support

See `USER_LOGIN_FIX_REPORT.md` for:
- Detailed technical explanation
- Password encryption details
- Database schema
- API endpoints
- Complete troubleshooting guide

---

**Quick Summary:**
1. Execute: `fix-user-login.sql`
2. Verify: `verify-user-status.sql`
3. Test: Login with credentials above
