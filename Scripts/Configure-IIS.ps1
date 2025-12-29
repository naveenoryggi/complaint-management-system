# Configure-IIS.ps1
# IIS Configuration Script for Complaint Management System
# This script configures IIS to host the frontend and API

param(
    [string]$InstallPath = "C:\Program Files\ComplaintManagement",
    [int]$WebPort = 80,
    [int]$ApiPort = 5000,
    [string]$SiteName = "ComplaintManagement"
)

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Complaint Management System - IIS Setup  " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

# Import IIS module
try {
    Import-Module WebAdministration -ErrorAction Stop
    Write-Host "[OK] IIS WebAdministration module loaded" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] IIS is not installed or WebAdministration module not available" -ForegroundColor Red
    Write-Host "Please install IIS with the following features:" -ForegroundColor Yellow
    Write-Host "  - Web Server (IIS)" -ForegroundColor Yellow
    Write-Host "  - IIS Management Console" -ForegroundColor Yellow
    Write-Host "  - URL Rewrite Module (download from Microsoft)" -ForegroundColor Yellow
    exit 1
}

$wwwPath = Join-Path $InstallPath "WWW"
$apiPath = Join-Path $InstallPath "API"

# Verify paths exist
if (-not (Test-Path $wwwPath)) {
    Write-Host "[ERROR] Frontend path not found: $wwwPath" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $apiPath)) {
    Write-Host "[ERROR] API path not found: $apiPath" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Install Path: $InstallPath"
Write-Host "  Frontend Path: $wwwPath"
Write-Host "  API Path: $apiPath"
Write-Host "  Web Port: $WebPort"
Write-Host "  API Port: $ApiPort"
Write-Host "  Site Name: $SiteName"
Write-Host ""

# Step 1: Create Application Pool for Frontend
Write-Host "Step 1: Creating Application Pool for Frontend..." -ForegroundColor Yellow
$appPoolName = "${SiteName}AppPool"
if (Test-Path "IIS:\AppPools\$appPoolName") {
    Write-Host "  Application Pool '$appPoolName' already exists, removing..." -ForegroundColor Yellow
    Remove-WebAppPool -Name $appPoolName -ErrorAction SilentlyContinue
}
New-WebAppPool -Name $appPoolName
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Write-Host "  [OK] Application Pool '$appPoolName' created" -ForegroundColor Green

# Step 2: Create Application Pool for API
Write-Host "Step 2: Creating Application Pool for API..." -ForegroundColor Yellow
$apiAppPoolName = "${SiteName}APIPool"
if (Test-Path "IIS:\AppPools\$apiAppPoolName") {
    Write-Host "  Application Pool '$apiAppPoolName' already exists, removing..." -ForegroundColor Yellow
    Remove-WebAppPool -Name $apiAppPoolName -ErrorAction SilentlyContinue
}
New-WebAppPool -Name $apiAppPoolName
Set-ItemProperty "IIS:\AppPools\$apiAppPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$apiAppPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Write-Host "  [OK] Application Pool '$apiAppPoolName' created" -ForegroundColor Green

# Step 3: Remove existing site if exists
Write-Host "Step 3: Removing existing site if exists..." -ForegroundColor Yellow
if (Test-Path "IIS:\Sites\$SiteName") {
    Remove-Website -Name $SiteName -ErrorAction SilentlyContinue
    Write-Host "  [OK] Existing site removed" -ForegroundColor Green
} else {
    Write-Host "  [SKIP] No existing site found" -ForegroundColor Gray
}

# Step 4: Create Website for Frontend
Write-Host "Step 4: Creating Website for Frontend..." -ForegroundColor Yellow
New-Website -Name $SiteName -Port $WebPort -PhysicalPath $wwwPath -ApplicationPool $appPoolName
Write-Host "  [OK] Website '$SiteName' created on port $WebPort" -ForegroundColor Green

# Step 5: Create API Application under the website
Write-Host "Step 5: Creating API Application..." -ForegroundColor Yellow
New-WebApplication -Name "api" -Site $SiteName -PhysicalPath $apiPath -ApplicationPool $apiAppPoolName
Write-Host "  [OK] API Application created at /$SiteName/api" -ForegroundColor Green

# Step 6: Update frontend config.json with correct IIS API URL
Write-Host "Step 6: Updating frontend config.json for IIS..." -ForegroundColor Yellow
$frontendConfigPath = Join-Path $wwwPath "assets\config.json"
$frontendConfigDir = Join-Path $wwwPath "assets"

if (-not (Test-Path $frontendConfigDir)) {
    New-Item -ItemType Directory -Path $frontendConfigDir -Force | Out-Null
}

# For IIS: API is at /api virtual path, routes also have /api prefix = /api/api
if ($WebPort -eq 80) {
    $webUrl = "http://localhost"
} else {
    $webUrl = "http://localhost:$WebPort"
}

$frontendConfig = @{
    apiUrl = "/api/api"
    baseUrl = $webUrl
    hostname = "localhost"
    webPort = $WebPort
    apiPort = $ApiPort
    isIIS = $true
}
$frontendConfig | ConvertTo-Json | Set-Content $frontendConfigPath -Encoding UTF8
Write-Host "  [OK] Frontend config updated for IIS mode (apiUrl: /api/api)" -ForegroundColor Green

# Step 7: Create web.config for Angular SPA routing in WWW folder
Write-Host "Step 7: Creating web.config for Angular SPA routing..." -ForegroundColor Yellow
$webConfigPath = Join-Path $wwwPath "web.config"
$webConfigContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <defaultDocument>
      <files>
        <clear />
        <add value="index.html" />
      </files>
    </defaultDocument>
    <rewrite>
      <rules>
        <rule name="Angular Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
            <add input="{REQUEST_URI}" pattern="^/api" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <remove fileExtension=".woff" />
      <remove fileExtension=".woff2" />
      <remove fileExtension=".json" />
      <mimeMap fileExtension=".woff" mimeType="application/font-woff" />
      <mimeMap fileExtension=".woff2" mimeType="application/font-woff2" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
    </staticContent>
    <httpProtocol>
      <customHeaders>
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-Frame-Options" value="SAMEORIGIN" />
        <add name="X-XSS-Protection" value="1; mode=block" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
"@
$webConfigContent | Out-File -FilePath $webConfigPath -Encoding UTF8
Write-Host "  [OK] web.config created with URL rewrite rules" -ForegroundColor Green

# Step 8: Set folder permissions
Write-Host "Step 8: Setting folder permissions..." -ForegroundColor Yellow
$acl = Get-Acl $InstallPath
$identity = "IIS AppPool\$appPoolName"
$permission = "ReadAndExecute", "Synchronize"
$inheritance = "ContainerInherit, ObjectInherit"
$propagation = "None"
$type = "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($identity, $permission, $inheritance, $propagation, $type)
$acl.SetAccessRule($accessRule)
Set-Acl -Path $InstallPath -AclObject $acl

# Also set permissions for API app pool
$identity2 = "IIS AppPool\$apiAppPoolName"
$accessRule2 = New-Object System.Security.AccessControl.FileSystemAccessRule($identity2, $permission, $inheritance, $propagation, $type)
$acl.SetAccessRule($accessRule2)
Set-Acl -Path $InstallPath -AclObject $acl
Write-Host "  [OK] Folder permissions set" -ForegroundColor Green

# Step 9: Start the website
Write-Host "Step 9: Starting website..." -ForegroundColor Yellow
Start-Website -Name $SiteName
Write-Host "  [OK] Website started" -ForegroundColor Green

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  IIS Configuration Complete!              " -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access the application at:" -ForegroundColor Cyan
Write-Host "  Frontend: http://localhost:$WebPort" -ForegroundColor White
Write-Host "  API:      http://localhost:$WebPort/api" -ForegroundColor White
Write-Host ""
Write-Host "NOTE: Make sure URL Rewrite module is installed for Angular routing to work." -ForegroundColor Yellow
Write-Host "Download from: https://www.iis.net/downloads/microsoft/url-rewrite" -ForegroundColor Yellow
Write-Host ""
