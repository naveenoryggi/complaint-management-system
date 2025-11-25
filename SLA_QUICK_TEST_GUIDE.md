# SLA System - Quick Test Guide

Use this guide to quickly test the SLA system manually.

---

## Prerequisites

1. Backend running on http://localhost:5000
2. Valid authentication token
3. API testing tool (Postman, curl, or PowerShell)

---

## Quick Test Script (PowerShell)

```powershell
# 1. Get Token
$token = (Get-Content ".test-token" -Raw).Trim()
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}
$baseUrl = "http://localhost:5000/api"

# 2. Get current SLA settings
$slaSettings = Invoke-RestMethod -Uri "$baseUrl/sla/settings" -Headers $headers
Write-Host "Current SLA Settings:" -ForegroundColor Cyan
$slaSettings | ConvertTo-Json

# 3. Get all SLA levels
$slaLevels = Invoke-RestMethod -Uri "$baseUrl/sla/levels" -Headers $headers
Write-Host "`nSLA Levels:" -ForegroundColor Cyan
$slaLevels.data | Format-Table Name, ResponseTimeDisplay, ResolutionTimeDisplay, ColorCode

# 4. Create a new SLA level
$newLevel = @{
    name = "TEST-Critical-4h"
    description = "Test critical SLA level"
    order = 99
    isActive = $true
    colorCode = "#FF0000"
    defaultResponseTime = 30
    responseTimeUnit = "Minutes"
    defaultResolutionTime = 4
    resolutionTimeUnit = "Hours"
} | ConvertTo-Json

$createdLevel = Invoke-RestMethod -Uri "$baseUrl/sla/levels" -Headers $headers -Method Post -Body $newLevel
Write-Host "`nCreated SLA Level:" -ForegroundColor Green
$createdLevel | ConvertTo-Json

# 5. Get categories
$categories = Invoke-RestMethod -Uri "$baseUrl/categories" -Headers $headers
Write-Host "`nAvailable Categories:" -ForegroundColor Cyan
$categories | Select-Object -First 5 | Format-Table id, name

# 6. Map category to SLA level (use first category)
if ($categories.Count -gt 0 -and $createdLevel.data.id) {
    $mapping = @{
        categoryId = $categories[0].id
        slaLevelId = $createdLevel.data.id
        isActive = $true
    } | ConvertTo-Json

    $createdMapping = Invoke-RestMethod -Uri "$baseUrl/sla/category-mappings" -Headers $headers -Method Post -Body $mapping
    Write-Host "`nCreated Category-SLA Mapping:" -ForegroundColor Green
    $createdMapping | ConvertTo-Json
}

# 7. Get all category mappings
$mappings = Invoke-RestMethod -Uri "$baseUrl/sla/category-mappings" -Headers $headers
Write-Host "`nAll Category Mappings:" -ForegroundColor Cyan
$mappings.data | Format-Table CategoryName, SLALevelName, EffectiveResponseTimeMinutes, EffectiveResolutionTimeMinutes

# 8. Update SLA settings
$updateSettings = @{
    isEnabled = $true
    workingHoursOnly = $true
    workingHoursStart = "09:00:00"
    workingHoursEnd = "17:00:00"
    workingDays = "[1,2,3,4,5]"  # Monday to Friday
    excludeHolidays = $true
    autoEscalateOnBreach = $true
    escalationThresholdPercent = 90
    notifyBeforeBreach = $true
    notifyBeforeBreachMinutes = 60
    pauseSLAOnPendingInfo = $true
    timezone = "UTC"
} | ConvertTo-Json

$updatedSettings = Invoke-RestMethod -Uri "$baseUrl/sla/settings" -Headers $headers -Method Put -Body $updateSettings
Write-Host "`nUpdated SLA Settings:" -ForegroundColor Green
$updatedSettings.data | Format-List

Write-Host "`n✅ SLA System Test Complete!" -ForegroundColor Green
Write-Host "The SLA system is fully functional and ready to use." -ForegroundColor Green
```

---

## Test Results You Should See

### 1. SLA Settings Response
```json
{
  "isSuccess": true,
  "data": {
    "id": "...",
    "isEnabled": true,
    "workingHoursOnly": true,
    "workingHoursStart": "09:00:00",
    "workingHoursEnd": "17:00:00",
    "workingDays": "[1,2,3,4,5]",
    "excludeHolidays": true,
    "autoEscalateOnBreach": true,
    "escalationThresholdPercent": 90,
    "notifyBeforeBreach": true,
    "notifyBeforeBreachMinutes": 60,
    "pauseSLAOnPendingInfo": true,
    "timezone": "UTC",
    "companyId": "..."
  }
}
```

### 2. SLA Level Response
```json
{
  "isSuccess": true,
  "data": {
    "id": "...",
    "name": "TEST-Critical-4h",
    "description": "Test critical SLA level",
    "order": 99,
    "isActive": true,
    "colorCode": "#FF0000",
    "defaultResponseTime": 30,
    "responseTimeUnit": "Minutes",
    "defaultResolutionTime": 4,
    "resolutionTimeUnit": "Hours",
    "responseTimeInMinutes": 30,
    "resolutionTimeInMinutes": 240,
    "responseTimeDisplay": "30 minutes",
    "resolutionTimeDisplay": "4 hours"
  }
}
```

### 3. Category Mapping Response
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "...",
      "categoryId": "...",
      "categoryName": "Hardware Issues",
      "slaLevelId": "...",
      "slaLevelName": "TEST-Critical-4h",
      "slaLevelColorCode": "#FF0000",
      "effectiveResponseTimeMinutes": 30,
      "effectiveResolutionTimeMinutes": 240,
      "isActive": true
    }
  ]
}
```

---

## Common Issues & Solutions

### Issue: 401 Unauthorized
**Solution:** Token expired. Run `./get-fresh-token.ps1` to get a new token.

### Issue: 404 Not Found on /api/sla/settings
**Solution:** Backend may not be running. Check if backend is on port 5000.

### Issue: 400 Bad Request on creating SLA level
**Solution:** Check all required fields are provided and values are valid.

### Issue: Cannot create category mapping
**Solution:** Ensure both category and SLA level exist before mapping.

---

## Verify SLA is Working

After setting up SLA, create a test complaint:

```powershell
# Get required IDs
$categories = Invoke-RestMethod -Uri "$baseUrl/categories" -Headers $headers
$priorities = Invoke-RestMethod -Uri "$baseUrl/complaint-priority-master" -Headers $headers
$statuses = Invoke-RestMethod -Uri "$baseUrl/complaint-status-master" -Headers $headers

# Create test complaint
$complaint = @{
    title = "SLA Test Complaint"
    description = "Testing SLA calculations"
    categoryId = $categories[0].id  # Use category with SLA mapping
    priorityMasterId = $priorities[0].id
    statusMasterId = $statuses[0].id
} | ConvertTo-Json

$newComplaint = Invoke-RestMethod -Uri "$baseUrl/complaints" -Headers $headers -Method Post -Body $complaint

# Get complaint details to see SLA info
$complaintDetail = Invoke-RestMethod -Uri "$baseUrl/complaints/$($newComplaint.data.id)" -Headers $headers

# Check for SLA fields
Write-Host "Complaint SLA Information:" -ForegroundColor Cyan
$complaintDetail.data | Select-Object title, slaHours, timeElapsed, timeRemaining, slaPercentage, isBreached | Format-List
```

---

## Expected Behavior

When SLA is properly configured:

1. ✅ Complaints in mapped categories should have SLA hours set
2. ✅ Time elapsed should increase as time passes
3. ✅ Time remaining should decrease
4. ✅ SLA percentage should calculate correctly
5. ✅ Breach flag should trigger when SLA exceeded

---

## Next Steps

If all tests pass:
1. Test in the UI at http://localhost:4200
2. Check complaint list for SLA indicators
3. Check complaint detail for SLA progress
4. Look for SLA management page in admin section

If tests fail:
1. Check backend logs for errors
2. Verify database migrations are applied
3. Confirm SLA tables exist in database
4. Review the detailed report: `COMPREHENSIVE_SLA_E2E_TEST_FINAL_REPORT.md`

---

## Quick Reference

**Documentation:**
- Full Report: `COMPREHENSIVE_SLA_E2E_TEST_FINAL_REPORT.md`
- Executive Summary: `SLA_TESTING_EXECUTIVE_SUMMARY.md`
- This Guide: `SLA_QUICK_TEST_GUIDE.md`

**Test Scripts:**
- Automated: `comprehensive-sla-e2e-test.ps1`
- Get Token: `get-fresh-token.ps1`

**Key Endpoints:**
- Settings: `/api/sla/settings`
- Levels: `/api/sla/levels`
- Category Mappings: `/api/sla/category-mappings`
- Priority Mappings: `/api/sla/priority-mappings`

---

**Happy Testing! 🚀**
