# Update-API.ps1 - Updates the installed API with the latest build
$ErrorActionPreference = "Stop"

$sourcePath = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.API\bin\Release\net8.0\publish"
$destPath = "C:\Program Files\ComplaintManagement\API"

Write-Host "Stopping IIS..." -ForegroundColor Yellow
iisreset /stop

Write-Host "Waiting for files to be released..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Don't delete everything - preserve appsettings.json with connection string
Write-Host "Backing up appsettings.json..." -ForegroundColor Yellow
$appsettingsBackup = $null
if (Test-Path "$destPath\appsettings.json") {
    $appsettingsBackup = Get-Content "$destPath\appsettings.json" -Raw
}

Write-Host "Copying new API files..." -ForegroundColor Yellow
Copy-Item -Path "$sourcePath\*" -Destination "$destPath\" -Recurse -Force

# Restore appsettings.json with connection string
if ($appsettingsBackup) {
    Write-Host "Restoring appsettings.json..." -ForegroundColor Yellow
    $appsettingsBackup | Out-File -FilePath "$destPath\appsettings.json" -Encoding UTF8
}

Write-Host "Starting IIS..." -ForegroundColor Yellow
iisreset /start

Write-Host "API updated successfully!" -ForegroundColor Green
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
