# Fix-API-Robocopy.ps1 - Uses robocopy to properly copy all files
$sourcePath = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.API\bin\Release\net8.0\publish"
$destPath = "C:\Program Files\ComplaintManagement\API"

Write-Host "Copying ALL files from publish folder using robocopy..." -ForegroundColor Yellow

# Backup appsettings.json if it has a connection string
$appsettingsPath = "$destPath\appsettings.json"
$appsettingsBackup = $null
if (Test-Path $appsettingsPath) {
    $content = Get-Content $appsettingsPath -Raw
    if ($content -match "LAPTOP-NF9BTG7Q") {
        $appsettingsBackup = $content
        Write-Host "Backed up appsettings.json with connection string" -ForegroundColor Cyan
    }
}

# Use robocopy to mirror the folder
Write-Host "Running robocopy..." -ForegroundColor Yellow
robocopy $sourcePath $destPath /E /R:1 /W:1 /NFL /NDL /NJH /NJS

# Restore appsettings.json
if ($appsettingsBackup) {
    Write-Host "Restoring appsettings.json..." -ForegroundColor Yellow
    $appsettingsBackup | Out-File -FilePath $appsettingsPath -Encoding UTF8 -Force
}

# Create logs folder
New-Item -ItemType Directory -Path "$destPath\logs" -Force -ErrorAction SilentlyContinue | Out-Null

Write-Host ""
Write-Host "Verifying critical files..." -ForegroundColor Cyan
$criticalFiles = @(
    "ComplaintManagement.API.exe",
    "ComplaintManagement.API.dll",
    "ComplaintManagement.API.runtimeconfig.json",
    "ComplaintManagement.API.deps.json",
    "web.config",
    "appsettings.json"
)

$allFound = $true
foreach ($file in $criticalFiles) {
    $path = Join-Path $destPath $file
    if (Test-Path $path) {
        Write-Host "  [OK] $file" -ForegroundColor Green
    } else {
        Write-Host "  [MISSING] $file" -ForegroundColor Red
        $allFound = $false
    }
}

if ($allFound) {
    Write-Host ""
    Write-Host "All critical files present!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Some files are missing!" -ForegroundColor Red
}

Write-Host ""
pause
