# Restart Angular Dev Server with Fresh Compilation
# This script stops all Node.js processes and starts Angular dev server fresh

Write-Host "=== Restarting Angular Dev Server ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Kill all node.exe processes
Write-Host "Step 1: Stopping all Node.js processes..." -ForegroundColor Yellow
Get-Process -Name node -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

# Step 2: Navigate to Angular directory
Write-Host "Step 2: Navigating to Angular directory..." -ForegroundColor Yellow
$angularPath = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-angular"
Set-Location $angularPath

# Step 3: Clear Angular cache
Write-Host "Step 3: Clearing Angular cache..." -ForegroundColor Yellow
if (Test-Path ".angular") {
    Remove-Item -Recurse -Force ".angular" -ErrorAction SilentlyContinue
    Write-Host "  - Cleared .angular cache" -ForegroundColor Green
}
if (Test-Path "node_modules\.cache") {
    Remove-Item -Recurse -Force "node_modules\.cache" -ErrorAction SilentlyContinue
    Write-Host "  - Cleared node_modules cache" -ForegroundColor Green
}

# Step 4: Start Angular dev server
Write-Host "Step 4: Starting Angular dev server..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Angular is compiling... Please wait for 'Compiled successfully' message" -ForegroundColor Cyan
Write-Host "Then open: http://localhost:4200/admin/email-ticketing-config" -ForegroundColor Green
Write-Host ""

npm start
