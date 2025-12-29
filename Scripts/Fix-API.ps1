# Fix-API.ps1 - Fixes the API by copying all missing files
$sourcePath = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.API\bin\Release\net8.0\publish"
$destPath = "C:\Program Files\ComplaintManagement\API"

Write-Host "Copying all missing files from publish folder..." -ForegroundColor Yellow

# Copy all files (preserving existing appsettings with connection string)
$existingAppsettings = $null
if (Test-Path "$destPath\appsettings.json") {
    $existingAppsettings = Get-Content "$destPath\appsettings.json" -Raw
}

# Copy everything
xcopy "$sourcePath\*" "$destPath\" /E /Y /R

# Restore appsettings.json with connection string if it existed
if ($existingAppsettings) {
    Write-Host "Restoring appsettings.json with your connection string..." -ForegroundColor Yellow
    $existingAppsettings | Out-File -FilePath "$destPath\appsettings.json" -Encoding UTF8 -Force
}

Write-Host "Creating logs folder..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path "$destPath\logs" -Force -ErrorAction SilentlyContinue

Write-Host "All files copied!" -ForegroundColor Green
Write-Host ""
Write-Host "Files in API folder:" -ForegroundColor Cyan
Get-ChildItem $destPath -Filter "*.json" | ForEach-Object { Write-Host "  - $($_.Name)" }
Write-Host ""
pause
