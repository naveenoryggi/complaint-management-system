# Complete deployment script
$sourcePath = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.API\bin\Release\net8.0\publish"
$destPath = "C:\Program Files\ComplaintManagement\API"

Write-Host "=== Complete API Deployment ===" -ForegroundColor Cyan

# 1. Backup appsettings.json if it has connection string
$appsettingsPath = "$destPath\appsettings.json"
$appsettingsBackup = $null
if (Test-Path $appsettingsPath) {
    $appsettingsBackup = Get-Content $appsettingsPath -Raw
    Write-Host "Backed up appsettings.json" -ForegroundColor Green
}

# 2. Copy all files using robocopy
Write-Host "Copying files..." -ForegroundColor Yellow
robocopy $sourcePath $destPath /E /R:1 /W:1 /NFL /NDL /NJH /NJS

# 3. Restore appsettings.json
if ($appsettingsBackup) {
    Write-Host "Restoring appsettings.json..." -ForegroundColor Yellow
    $appsettingsBackup | Out-File -FilePath $appsettingsPath -Encoding UTF8 -Force
}

# 4. Create logs folder
New-Item -ItemType Directory -Path "$destPath\logs" -Force -ErrorAction SilentlyContinue | Out-Null

# 5. Update web.config with PathBase environment variable
Write-Host "Updating web.config for IIS..." -ForegroundColor Yellow
$webConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\ComplaintManagement.API.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="InProcess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="ASPNETCORE_PATHBASE" value="/api" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
"@
$webConfig | Out-File -FilePath "$destPath\web.config" -Encoding UTF8 -Force

# 6. Recycle the app pool
Write-Host "Recycling IIS App Pool..." -ForegroundColor Yellow
& "C:\Windows\System32\inetsrv\appcmd.exe" recycle apppool /apppool.name:"ComplaintManagementAPIPool"

Write-Host ""
Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host "Test: http://localhost:11020/api/auth/login" -ForegroundColor Cyan
