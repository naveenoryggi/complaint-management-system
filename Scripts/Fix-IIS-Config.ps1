# Fix IIS Configuration and capture logs
$apiPath = "C:\Program Files\ComplaintManagement\API"
$webConfigPath = "$apiPath\web.config"

# Create logs folder
New-Item -ItemType Directory -Path "$apiPath\logs" -Force -ErrorAction SilentlyContinue | Out-Null

# Create web.config with stdout logging enabled
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
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
"@

Write-Host "Updating web.config with stdout logging enabled..." -ForegroundColor Yellow
$webConfig | Out-File -FilePath $webConfigPath -Encoding UTF8 -Force

# Check if appsettings.json exists and has connection string
$appsettingsPath = "$apiPath\appsettings.json"
if (Test-Path $appsettingsPath) {
    $content = Get-Content $appsettingsPath -Raw
    if ($content -match "LAPTOP-NF9BTG7Q") {
        Write-Host "Connection string in appsettings.json: OK" -ForegroundColor Green
    } else {
        Write-Host "WARNING: Connection string may be missing or incorrect!" -ForegroundColor Red
    }
} else {
    Write-Host "ERROR: appsettings.json not found!" -ForegroundColor Red
}

Write-Host ""
Write-Host "Restarting IIS..." -ForegroundColor Yellow
iisreset /restart

Write-Host ""
Write-Host "Done! Check the logs folder for stdout_*.log files after making a request." -ForegroundColor Cyan
