# OAuth System Configuration - Quick Test Guide

## 🎯 What You're Testing

**Feature:** System Settings for OAuth Token Refresh
**Critical Setting:** Token Refresh Interval = 30 minutes (perfect for 1-hour OAuth tokens)

---

## 🚀 Quick Start (5 Minutes)

### 1. Login
```
URL: http://localhost:4200
User: admin@complaintmanagement.com
Pass: Admin@123
```

### 2. Navigate
```
Click: Admin → Email Ticketing Config
```

### 3. Open Settings
```
Look for: Button with gear icon ⚙️
Text: "System Settings"
Click it!
```

### 4. Configure OAuth
```
Find: "Token Refresh Interval"
Enter: 30
Expected: Green badge "✓ Perfect for 1-hour tokens!"
```

### 5. Save
```
Click: "Save Settings"
Expected: Success message with OAuth-specific confirmation
```

### 6. Verify
```
Close panel → Reopen panel
Verify: Value still shows 30 minutes
```

---

## ✅ Success Criteria

| Check | Expected Result |
|-------|----------------|
| System Settings button visible | ✅ Gear icon button present |
| Panel opens smoothly | ✅ Slides in from right |
| OAuth section visible | ✅ "OAuth Token Management" heading |
| Input accepts 30 | ✅ Value entered successfully |
| Green badge appears | ✅ "Perfect for 1-hour tokens!" shown |
| Save succeeds | ✅ Success message displayed |
| Settings persist | ✅ Value remains after close/reopen |

---

## 📸 Screenshots Needed

1. **01-email-ticketing-config-page.png** - Initial page with System Settings button
2. **02-system-settings-panel-opened.png** - Panel showing all settings
3. **03-oauth-refresh-interval-30.png** - Input set to 30 with green badge
4. **04-settings-saved-success.png** - Success message
5. **05-settings-persisted.png** - Reopened panel showing 30 minutes

---

## 🔍 What to Look For

### Visual Elements

**System Settings Button:**
```
[⚙️ System Settings]
```

**OAuth Token Management Section:**
```
┌─────────────────────────────────────┐
│ OAuth Token Management              │
├─────────────────────────────────────┤
│ Token Refresh Interval: [30] minutes│
│ Current: 30 minutes                 │
│ ✓ Perfect for 1-hour tokens!        │
├─────────────────────────────────────┤
│ Token Expiry Warning: [7] days      │
└─────────────────────────────────────┘
```

**Save Button:**
```
[💾 Save Settings]
```

---

## ⚠️ Common Issues

### Issue 1: System Settings button not visible
**Solution:** Scroll down on the Email Ticketing Config page

### Issue 2: Panel doesn't open
**Solution:** Check browser console for errors, refresh page

### Issue 3: Can't enter 30
**Solution:** Ensure value is between 5-120 (validated range)

### Issue 4: Green badge doesn't appear
**Solution:** Badge only shows when value is exactly 30

### Issue 5: Settings don't persist
**Solution:** Backend API may be down, check console logs

---

## 🎨 Expected UI Design

The System Settings panel should have:

- **Purple gradient header** (matching app theme)
- **Glassmorphism effect** (frosted glass look)
- **Smooth slide-in animation** (from right side)
- **Organized sections** (OAuth, Email, Auto-Response, etc.)
- **Visual feedback** (green badges for recommendations)
- **Clear labels** (with min/max constraints shown)

---

## 🔧 Technical Details

### Valid Values

| Setting | Min | Max | Recommended |
|---------|-----|-----|-------------|
| Token Refresh Interval | 5 | 120 | **30** |
| Token Expiry Warning | 1 | 30 | 7 |
| Email Polling Interval | 60 | 3600 | 120 |

### Why 30 Minutes?

1. ✅ Refreshes halfway through 1-hour token lifetime
2. ✅ Provides safety margin for network delays
3. ✅ Prevents token expiration during active requests
4. ✅ Aligns with Microsoft/Google best practices

---

## 📊 Test Results Template

```
Test Date: _______________
Tester: _________________

Login: [ ] PASS [ ] FAIL
Navigate to Page: [ ] PASS [ ] FAIL
Open Settings Panel: [ ] PASS [ ] FAIL
Set to 30 Minutes: [ ] PASS [ ] FAIL
Green Badge Visible: [ ] PASS [ ] FAIL
Save Settings: [ ] PASS [ ] FAIL
Settings Persist: [ ] PASS [ ] FAIL

Screenshots Captured: ___/5

Overall Result: [ ] PASS [ ] FAIL

Notes:
_________________________________
_________________________________
_________________________________
```

---

## 🚨 Escalation

If any test fails:

1. **Capture screenshot** of error state
2. **Check browser console** for JavaScript errors
3. **Check backend logs** for API errors
4. **Document exact steps** to reproduce
5. **Report to development team** with all evidence

---

## ✨ Feature Benefits

Once configured to 30 minutes:

- **No More Token Expiration Errors** - Automatic refresh prevents downtime
- **Seamless Email Integration** - OAuth tokens stay fresh
- **Better User Experience** - No manual re-authorization needed
- **Production Ready** - Robust token management

---

**Happy Testing!** 🎉
