# Test Statistics API After Fix
Write-Host "=== Testing Statistics API ===" -ForegroundColor Cyan
Write-Host ""

# Login to get fresh token
$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/login' -Method POST -Headers @{'Content-Type'='application/json'} -Body '{"email":"admin@complaint-system.com","password":"Admin@123"}'
$token = $loginResponse.data.accessToken

Write-Host "Logged in successfully" -ForegroundColor Green
Write-Host ""

# Get statistics
$statsResponse = Invoke-RestMethod -Uri 'http://localhost:5000/api/dashboard/statistics?dateRangeDays=30' -Method GET -Headers @{'Authorization'="Bearer $token"}

Write-Host "===== WIDGET COUNT =====" -ForegroundColor Yellow
Write-Host "Total Widgets: $($statsResponse.data.statusWidgets.Count)" -ForegroundColor White
Write-Host ""

Write-Host "===== WIDGET DETAILS =====" -ForegroundColor Yellow
$statsResponse.data.statusWidgets | ForEach-Object {
    Write-Host "- [$($_.code)] $($_.name)" -ForegroundColor White
}

Write-Host ""
if ($statsResponse.data.statusWidgets.Count -eq 11) {
    Write-Host "✅ SUCCESS! All 11 widgets are being returned!" -ForegroundColor Green
} else {
    Write-Host "❌ ISSUE: Expected 11 widgets, got $($statsResponse.data.statusWidgets.Count)" -ForegroundColor Red
}
