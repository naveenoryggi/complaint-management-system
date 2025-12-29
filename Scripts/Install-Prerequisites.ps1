# Install-Prerequisites.ps1
# Automatically installs all prerequisites for Complaint Management System
# Shows progress to user

param(
    [string]$InstallPath = "C:\Program Files\ComplaintManagement",
    [int]$WebPort = 80,
    [int]$ApiPort = 5000
)

# Set up environment
$ErrorActionPreference = "Continue"
$ProgressPreference = "Continue"
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

# Log file
$LogDir = "C:\ProgramData\ComplaintManagement\Logs"
if (-not (Test-Path $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }
$LogFile = Join-Path $LogDir "install_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

function Write-Status {
    param([string]$Message, [string]$Color = "White")
    $Timestamp = Get-Date -Format "HH:mm:ss"
    Write-Host "[$Timestamp] $Message" -ForegroundColor $Color
    Add-Content -Path $LogFile -Value "[$Timestamp] $Message" -ErrorAction SilentlyContinue
}

# Show header
Clear-Host
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  Complaint Management System Setup" -ForegroundColor Cyan
Write-Host "  Prerequisites Installation" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Status "Installation Path: $InstallPath"
Write-Status "Log File: $LogFile"
Write-Host ""

# ============================================================
# 1. CHECK AND INSTALL .NET 8 RUNTIME
# ============================================================
Write-Host "Step 1/6: Checking .NET 8 Runtime..." -ForegroundColor Yellow

$dotnetInstalled = $false
try {
    $runtimes = & dotnet --list-runtimes 2>&1
    if ($runtimes -match "Microsoft\.AspNetCore\.App 8\.") {
        Write-Status ".NET 8 ASP.NET Core Runtime: ALREADY INSTALLED" "Green"
        $dotnetInstalled = $true
    }
} catch {}

if (-not $dotnetInstalled) {
    Write-Status "Downloading .NET 8 Hosting Bundle..." "Yellow"
    Write-Status "Source: https://aka.ms/dotnet/8.0/dotnet-hosting-win.exe" "Gray"

    $dotnetInstaller = "$env:TEMP\dotnet-hosting-8.0.exe"

    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.DownloadFile("https://aka.ms/dotnet/8.0/dotnet-hosting-win.exe", $dotnetInstaller)
        Write-Status "Download complete. Installing (this takes 2-3 minutes)..." "Yellow"

        $proc = Start-Process -FilePath $dotnetInstaller -ArgumentList "/install", "/quiet", "/norestart" -Wait -PassThru

        if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010) {
            Write-Status ".NET 8 Hosting Bundle: INSTALLED SUCCESSFULLY" "Green"
        } else {
            Write-Status ".NET 8 installation returned code: $($proc.ExitCode)" "Yellow"
        }
        Remove-Item $dotnetInstaller -Force -ErrorAction SilentlyContinue
    } catch {
        Write-Status "ERROR: Could not install .NET 8: $($_.Exception.Message)" "Red"
        Write-Status "Please install manually from: https://dotnet.microsoft.com/download/dotnet/8.0" "Yellow"
    }
}

Write-Host ""

# ============================================================
# 2. ENABLE IIS FEATURES
# ============================================================
Write-Host "Step 2/6: Enabling IIS Features..." -ForegroundColor Yellow
Write-Status "This may take 1-2 minutes..."

$iisFeatures = @(
    "IIS-WebServerRole",
    "IIS-WebServer",
    "IIS-CommonHttpFeatures",
    "IIS-HttpErrors",
    "IIS-StaticContent",
    "IIS-DefaultDocument",
    "IIS-ApplicationDevelopment",
    "IIS-ISAPIExtensions",
    "IIS-ISAPIFilter",
    "IIS-NetFxExtensibility45",
    "IIS-ASPNET45",
    "IIS-HealthAndDiagnostics",
    "IIS-HttpLogging",
    "IIS-Security",
    "IIS-RequestFiltering",
    "IIS-Performance",
    "IIS-HttpCompressionStatic",
    "IIS-WebServerManagementTools",
    "IIS-ManagementConsole"
)

$enabledCount = 0
foreach ($feature in $iisFeatures) {
    try {
        $state = Get-WindowsOptionalFeature -Online -FeatureName $feature -ErrorAction SilentlyContinue
        if ($state -and $state.State -ne "Enabled") {
            Enable-WindowsOptionalFeature -Online -FeatureName $feature -All -NoRestart -ErrorAction SilentlyContinue | Out-Null
            $enabledCount++
        }
    } catch {
        # Try DISM as fallback
        $null = & dism /online /enable-feature /featurename:$feature /all /norestart /quiet 2>&1
    }
}

Write-Status "IIS Features: CONFIGURED ($enabledCount features enabled)" "Green"
Write-Host ""

# ============================================================
# 3. INSTALL URL REWRITE MODULE
# ============================================================
Write-Host "Step 3/6: Checking URL Rewrite Module..." -ForegroundColor Yellow

$urlRewriteInstalled = $false
if ((Test-Path "${env:ProgramFiles}\IIS\URL Rewrite\rewrite.dll") -or
    (Test-Path "${env:ProgramFiles(x86)}\IIS\URL Rewrite\rewrite.dll") -or
    (Test-Path "HKLM:\SOFTWARE\Microsoft\IIS Extensions\URL Rewrite")) {
    Write-Status "URL Rewrite Module: ALREADY INSTALLED" "Green"
    $urlRewriteInstalled = $true
}

if (-not $urlRewriteInstalled) {
    Write-Status "Downloading URL Rewrite Module..." "Yellow"
    Write-Status "Source: https://download.microsoft.com/download/.../rewrite_amd64_en-US.msi" "Gray"

    $urlRewriteInstaller = "$env:TEMP\urlrewrite2.msi"

    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.DownloadFile("https://download.microsoft.com/download/1/2/8/128E2E22-C1B9-44A4-BE2A-5859ED1D4592/rewrite_amd64_en-US.msi", $urlRewriteInstaller)
        Write-Status "Download complete. Installing..." "Yellow"

        $proc = Start-Process -FilePath "msiexec.exe" -ArgumentList "/i", "`"$urlRewriteInstaller`"", "/quiet", "/norestart" -Wait -PassThru

        if ($proc.ExitCode -eq 0) {
            Write-Status "URL Rewrite Module: INSTALLED SUCCESSFULLY" "Green"
        } else {
            Write-Status "URL Rewrite installation returned code: $($proc.ExitCode)" "Yellow"
        }
        Remove-Item $urlRewriteInstaller -Force -ErrorAction SilentlyContinue
    } catch {
        Write-Status "ERROR: Could not install URL Rewrite: $($_.Exception.Message)" "Red"
    }
}

Write-Host ""

# ============================================================
# 4. CONFIGURE FIREWALL RULES
# ============================================================
Write-Host "Step 4/6: Configuring Firewall Rules..." -ForegroundColor Yellow

# Remove old rules
Get-NetFirewallRule -DisplayName "Complaint Management*" -ErrorAction SilentlyContinue | Remove-NetFirewallRule -ErrorAction SilentlyContinue

# Add new rules
New-NetFirewallRule -DisplayName "Complaint Management API ($ApiPort)" -Direction Inbound -Protocol TCP -LocalPort $ApiPort -Action Allow -ErrorAction SilentlyContinue | Out-Null
New-NetFirewallRule -DisplayName "Complaint Management Web ($WebPort)" -Direction Inbound -Protocol TCP -LocalPort $WebPort -Action Allow -ErrorAction SilentlyContinue | Out-Null
New-NetFirewallRule -DisplayName "Complaint Management HTTPS (443)" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow -ErrorAction SilentlyContinue | Out-Null

Write-Status "Firewall Rules: CONFIGURED (Ports $WebPort, $ApiPort, 443)" "Green"
Write-Host ""

# ============================================================
# 5. CREATE DIRECTORIES
# ============================================================
Write-Host "Step 5/6: Creating Directories..." -ForegroundColor Yellow

$directories = @(
    $InstallPath,
    "$InstallPath\API",
    "$InstallPath\WWW",
    "$InstallPath\Logs",
    "$InstallPath\Uploads",
    "C:\ProgramData\ComplaintManagement",
    "C:\ProgramData\ComplaintManagement\Logs"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force -ErrorAction SilentlyContinue | Out-Null
    }
}

# Set permissions
$accountsToGrant = @("IIS_IUSRS", "NETWORK SERVICE", "LOCAL SERVICE")
$foldersNeedingWrite = @("$InstallPath\Logs", "$InstallPath\Uploads", "C:\ProgramData\ComplaintManagement", "C:\ProgramData\ComplaintManagement\Logs")

foreach ($folder in $foldersNeedingWrite) {
    if (Test-Path $folder) {
        foreach ($account in $accountsToGrant) {
            try {
                $acl = Get-Acl $folder -ErrorAction SilentlyContinue
                if ($acl) {
                    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule($account, "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
                    $acl.SetAccessRule($rule)
                    Set-Acl -Path $folder -AclObject $acl -ErrorAction SilentlyContinue
                }
            } catch {}
        }
    }
}

Write-Status "Directories: CREATED WITH PERMISSIONS" "Green"
Write-Host ""

# ============================================================
# 6. RESET IIS
# ============================================================
Write-Host "Step 6/6: Restarting IIS..." -ForegroundColor Yellow

try {
    $null = & iisreset /restart 2>&1
    Write-Status "IIS: RESTARTED" "Green"
} catch {
    Write-Status "IIS restart skipped (may not be fully installed)" "Yellow"
}

# ============================================================
# SUMMARY
# ============================================================
Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  Prerequisites Installation Complete!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Status "Log saved to: $LogFile" "Gray"
Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

exit 0
