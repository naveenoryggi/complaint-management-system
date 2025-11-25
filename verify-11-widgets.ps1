# Verify All 11 Widgets Display on Dashboard
Write-Host "=== Dashboard Widget Verification ===" -ForegroundColor Cyan
Write-Host ""

# Navigate and take screenshot
npx playwright codegen --target=csharp http://localhost:4200/dashboard

# Alternative: Use existing auth and verify
Write-Host "Opening dashboard to verify 11 widgets display..." -ForegroundColor Yellow
Start-Process "http://localhost:4200/dashboard"
