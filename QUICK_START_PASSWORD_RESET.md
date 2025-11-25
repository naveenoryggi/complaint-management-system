# Quick Start Guide - Password Reset Flow

## 1-Minute Setup

### Step 1: Apply Database Migration
```powershell
# Stop the API first, then run:
sqlcmd -S LAPTOP-NF9BTG7Q\SQLEXPRESS -d ComplaintManagementDB -i add-password-reset-token-migration.sql
```

### Step 2: Start Servers
```powershell
# Backend (Terminal 1)
cd complaint-system-dotnet/src/ComplaintManagement.API
dotnet run

# Frontend (Terminal 2)
cd complaint-system-angular
npm start
```

### Step 3: Test It!
1. Open http://localhost:4200/login
2. Click "Forgot Password?"
3. Enter: admin@company.com
4. Check email for reset link
5. Click link and reset password

## Quick Test Commands

### Test API Endpoints
```powershell
# Request password reset
Invoke-RestMethod -Uri "http://localhost:5000/api/password-reset/request" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"email":"admin@company.com"}'

# Validate token (replace xxx with actual token)
Invoke-RestMethod -Uri "http://localhost:5000/api/password-reset/validate" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"token":"xxx"}'

# Reset password
Invoke-RestMethod -Uri "http://localhost:5000/api/password-reset/reset" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"token":"xxx","newPassword":"NewP@ssw0rd123"}'
```

### Run Comprehensive Tests
```powershell
.\test-password-reset-flow.ps1
```

## URLs

- **Login:** http://localhost:4200/login
- **Forgot Password:** http://localhost:4200/forgot-password
- **Reset Password:** http://localhost:4200/reset-password?token=xxx
- **API Docs:** http://localhost:5000/swagger (if enabled)

## Configuration

Edit `appsettings.json`:
```json
{
  "PasswordReset": {
    "TokenExpirationHours": 24,    // Token validity
    "MaxRequestsPerHour": 3,       // Rate limit
    "ResetLinkBaseUrl": "http://localhost:4200/reset-password"
  }
}
```

## Files to Review

### Backend
- `PasswordResetController.cs` - API endpoints
- `PasswordResetService.cs` - Business logic
- `PasswordResetToken.cs` - Entity model

### Frontend
- `forgot-password.component.ts` - Request reset
- `reset-password.component.ts` - Reset password
- `login.ts` - Updated with link

## Troubleshooting

**Emails not sending?**
- Check SMTP settings in appsettings.json
- Verify EmailService is configured

**Token invalid?**
- Check if it expired (24 hours)
- Verify it wasn't used already
- Check database PasswordResetTokens table

**Rate limited?**
- Wait 1 hour or adjust MaxRequestsPerHour
- Clear old tokens from database

## Security Checklist

- ✅ Tokens expire after 24 hours
- ✅ Tokens are single-use only
- ✅ Rate limiting (3 requests/hour)
- ✅ IP tracking for audit
- ✅ Password history check
- ✅ Password complexity validation
- ✅ Email enumeration protection

## What's Next?

1. Apply database migration
2. Test the flow end-to-end
3. Configure production email settings
4. Update ResetLinkBaseUrl for production
5. Deploy!

---

**Need Help?** Check `PASSWORD_RESET_IMPLEMENTATION_COMPLETE.md` for full documentation.
