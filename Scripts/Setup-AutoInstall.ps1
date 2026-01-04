# Setup-AutoInstall.ps1
# Run this script ON THE SERVER to set up automatic installation of new builds
# This creates a scheduled task that monitors the deploy folder and installs new builds

param(
    [string]$DeployFolder = "C:\ComplaintManagement-Deploy",
    [string]$InstallPath = "C:\Program Files\ComplaintManagement",
    [int]$CheckIntervalMinutes = 5
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setting up Auto-Install for CI/CD" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Create the auto-install script
$autoInstallScript = @'
# Auto-Install Script - Monitors deploy folder and installs new builds
param(
    [string]$DeployFolder = "C:\ComplaintManagement-Deploy",
    [string]$LogFile = "C:\ComplaintManagement-Deploy\auto-install.log"
)

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] $Message"
    Add-Content -Path $LogFile -Value $logMessage
    Write-Host $logMessage
}

$latestBuildFile = Join-Path $DeployFolder "latest-build.json"
$lastInstalledFile = Join-Path $DeployFolder "last-installed.json"

# Check if there's a new build
if (-not (Test-Path $latestBuildFile)) {
    Write-Log "No new build found. Exiting."
    exit 0
}

$latestBuild = Get-Content $latestBuildFile | ConvertFrom-Json

# Check if we've already installed this build
if (Test-Path $lastInstalledFile) {
    $lastInstalled = Get-Content $lastInstalledFile | ConvertFrom-Json
    if ($lastInstalled.GitSha -eq $latestBuild.GitSha) {
        Write-Log "Build $($latestBuild.GitSha) already installed. Skipping."
        exit 0
    }
}

Write-Log "New build detected: $($latestBuild.FileName)"
Write-Log "Git SHA: $($latestBuild.GitSha)"

# Find the installer
$installerPath = Join-Path $DeployFolder $latestBuild.FileName
if (-not (Test-Path $installerPath)) {
    Write-Log "ERROR: Installer not found at $installerPath"
    exit 1
}

# Stop IIS sites before installation
Write-Log "Stopping IIS sites..."
Stop-WebSite -Name "ComplaintManagementAPI" -ErrorAction SilentlyContinue
Stop-WebSite -Name "ComplaintManagementWeb" -ErrorAction SilentlyContinue
Start-Sleep -Seconds 5

# Run the installer silently
Write-Log "Running installer: $installerPath"
$installArgs = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /LOG=`"$DeployFolder\install-$($latestBuild.GitSha.Substring(0,7)).log`""
$process = Start-Process -FilePath $installerPath -ArgumentList $installArgs -Wait -PassThru

if ($process.ExitCode -eq 0) {
    Write-Log "Installation completed successfully!"

    # Update last installed record
    $latestBuild | ConvertTo-Json | Set-Content $lastInstalledFile

    # Start IIS sites
    Write-Log "Starting IIS sites..."
    Start-WebSite -Name "ComplaintManagementAPI" -ErrorAction SilentlyContinue
    Start-WebSite -Name "ComplaintManagementWeb" -ErrorAction SilentlyContinue

    Write-Log "Deployment complete!"
} else {
    Write-Log "ERROR: Installation failed with exit code $($process.ExitCode)"

    # Restart IIS sites anyway
    Start-WebSite -Name "ComplaintManagementAPI" -ErrorAction SilentlyContinue
    Start-WebSite -Name "ComplaintManagementWeb" -ErrorAction SilentlyContinue

    exit 1
}
'@

$scriptPath = Join-Path $DeployFolder "Run-AutoInstall.ps1"
Set-Content -Path $scriptPath -Value $autoInstallScript
Write-Host "Created auto-install script: $scriptPath" -ForegroundColor Green

# Create the scheduled task
$taskName = "ComplaintManagement-AutoInstall"
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

if ($existingTask) {
    Write-Host "Removing existing scheduled task..."
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
}

$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-ExecutionPolicy Bypass -File `"$scriptPath`""
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes $CheckIntervalMinutes) -RepetitionDuration (New-TimeSpan -Days 9999)
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Description "Monitors deploy folder and auto-installs new Complaint Management builds"

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Auto-Install Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Scheduled Task: $taskName" -ForegroundColor Yellow
Write-Host "Check Interval: Every $CheckIntervalMinutes minutes" -ForegroundColor Yellow
Write-Host "Deploy Folder: $DeployFolder" -ForegroundColor Yellow
Write-Host ""
Write-Host "The system will automatically install new builds when they're deployed via CI/CD."
