# Configure-Runner.ps1
# Run this script ON THE SERVER (192.168.1.185) as Administrator
# It will download, configure, and start the GitHub Actions runner

param(
    [Parameter(Mandatory=$true)]
    [string]$Token  # Get this from: https://github.com/naveenoryggi/complaint-management-system/settings/actions/runners/new
)

$ErrorActionPreference = "Stop"
$runnerPath = "C:\actions-runner"
$repoUrl = "https://github.com/naveenoryggi/complaint-management-system"
$runnerVersion = "2.311.0"
$runnerZip = "actions-runner-win-x64-$runnerVersion.zip"
$downloadUrl = "https://github.com/actions/runner/releases/download/v$runnerVersion/$runnerZip"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "GitHub Actions Runner Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Create runner directory
if (-not (Test-Path $runnerPath)) {
    Write-Host "Creating runner directory: $runnerPath"
    New-Item -ItemType Directory -Path $runnerPath -Force | Out-Null
}

Set-Location $runnerPath

# Download runner if not present
if (-not (Test-Path ".\config.cmd")) {
    Write-Host "Downloading GitHub Actions Runner v$runnerVersion..."

    if (Test-Path ".\$runnerZip") {
        Remove-Item ".\$runnerZip" -Force
    }

    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest -Uri $downloadUrl -OutFile $runnerZip -UseBasicParsing

    Write-Host "Extracting runner..."
    Expand-Archive -Path $runnerZip -DestinationPath . -Force
    Remove-Item $runnerZip -Force
}

# Remove old config if exists
if (Test-Path ".\.runner") {
    Write-Host "Removing old runner configuration..."
    .\config.cmd remove --token $Token 2>$null
}

# Configure runner
Write-Host "Configuring runner..."
.\config.cmd --url $repoUrl --token $Token --name "deploy-server" --labels "self-hosted,Windows,X64" --unattended --replace

# Install and start as service
Write-Host "Installing runner as Windows service..."
.\svc.cmd install

Write-Host "Starting runner service..."
.\svc.cmd start

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Runner Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Runner Name: deploy-server"
Write-Host "Runner Path: $runnerPath"
Write-Host ""
Write-Host "Check status at:"
Write-Host "https://github.com/naveenoryggi/complaint-management-system/settings/actions/runners"
