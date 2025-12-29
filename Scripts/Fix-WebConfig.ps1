# Fix web.config for IIS hosting
$webConfigPath = "C:\Program Files\ComplaintManagement\API\web.config"

# Create logs folder
New-Item -ItemType Directory -Path "C:\Program Files\ComplaintManagement\API\logs" -Force -ErrorAction SilentlyContinue

$webConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath=".\ComplaintManagement.API.exe" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="InProcess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
"@

$webConfig | Out-File -FilePath $webConfigPath -Encoding UTF8 -Force
Write-Host "web.config updated with InProcess model!" -ForegroundColor Green
pause
