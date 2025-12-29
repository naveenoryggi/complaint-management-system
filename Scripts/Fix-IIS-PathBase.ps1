# Fix IIS path mapping - add PathBase configuration to web.config
$webConfigPath = "C:\Program Files\ComplaintManagement\API\web.config"

Write-Host "Updating web.config to add PathBase environment variable..." -ForegroundColor Yellow

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

$webConfig | Out-File -FilePath $webConfigPath -Encoding UTF8 -Force

Write-Host "web.config updated!" -ForegroundColor Green

# Recycle the app pool
Write-Host ""
Write-Host "Recycling IIS App Pool..." -ForegroundColor Yellow
& "C:\Windows\System32\inetsrv\appcmd.exe" recycle apppool /apppool.name:"ComplaintManagementAPIPool"

Write-Host ""
Write-Host "Done! Test the API at http://localhost:11020/api/auth/login" -ForegroundColor Green
