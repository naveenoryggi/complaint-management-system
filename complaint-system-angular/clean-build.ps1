# ================================================
# Angular Clean Build Script
# ================================================
# Purpose: Completely clean all TypeScript caches and rebuild
# Use this when experiencing TypeScript compilation cache issues
# ================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Angular Clean Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Stop any running Angular processes
Write-Host "[1/6] Stopping Angular dev server..." -ForegroundColor Yellow
Get-Process -Name node -ErrorAction SilentlyContinue | Where-Object {
    $_.CommandLine -like '*ng serve*' -or $_.CommandLine -like '*angular*'
} | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "  Done!" -ForegroundColor Green

# Step 2: Delete Angular cache directory
Write-Host "[2/6] Deleting .angular cache..." -ForegroundColor Yellow
if (Test-Path ".angular") {
    Remove-Item -Path ".angular" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  Deleted .angular directory" -ForegroundColor Green
} else {
    Write-Host "  No .angular directory found" -ForegroundColor Gray
}

# Step 3: Delete node_modules cache
Write-Host "[3/6] Deleting node_modules cache..." -ForegroundColor Yellow
if (Test-Path "node_modules\.cache") {
    Remove-Item -Path "node_modules\.cache" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  Deleted node_modules\.cache directory" -ForegroundColor Green
} else {
    Write-Host "  No node_modules\.cache directory found" -ForegroundColor Gray
}

# Step 4: Delete all TypeScript build info files
Write-Host "[4/6] Deleting TypeScript build info files..." -ForegroundColor Yellow
$tsInfoFiles = Get-ChildItem -Path "." -Filter "*.tsbuildinfo" -Recurse -ErrorAction SilentlyContinue
if ($tsInfoFiles.Count -gt 0) {
    $tsInfoFiles | Remove-Item -Force -ErrorAction SilentlyContinue
    Write-Host "  Deleted $($tsInfoFiles.Count) .tsbuildinfo file(s)" -ForegroundColor Green
} else {
    Write-Host "  No .tsbuildinfo files found" -ForegroundColor Gray
}

# Step 5: Delete dist folder
Write-Host "[5/6] Deleting dist folder..." -ForegroundColor Yellow
if (Test-Path "dist") {
    Remove-Item -Path "dist" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  Deleted dist directory" -ForegroundColor Green
} else {
    Write-Host "  No dist directory found" -ForegroundColor Gray
}

# Step 6: Clear npm cache (optional but thorough)
Write-Host "[6/6] Clearing npm cache..." -ForegroundColor Yellow
npm cache clean --force 2>&1 | Out-Null
Write-Host "  npm cache cleared" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Clean completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Run: npm start" -ForegroundColor Yellow
Write-Host "  2. Wait for compilation to complete" -ForegroundColor Yellow
Write-Host "  3. Check for any remaining errors" -ForegroundColor Yellow
Write-Host ""
