#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Complaint Management System - Uninstaller
.DESCRIPTION
    Removes the Complaint Management System installation
    - Stops and removes Windows Service
    - Removes IIS website and application pool
    - Optionally removes database
    - Removes installation files
#>

$Script:Config = @{
    ServiceName = "ComplaintManagementAPI"
    IISSiteName = "ComplaintManagement"
    InstallPath = "C:\Program Files\ComplaintManagement"
}

function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host " $Text" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host ""
}

Write-Header "Complaint Management System - Uninstaller"

# Confirm uninstallation
Write-Host "This will remove the Complaint Management System from your computer" -ForegroundColor Yellow
Write-Host ""
$confirm = Read-Host "Are you sure you want to continue? (Y/N)"
if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Host "Uninstallation cancelled" -ForegroundColor Green
    exit 0
}

# Stop and remove Windows Service
Write-Host ""
Write-Host "Removing Windows Service..." -ForegroundColor Cyan
$service = Get-Service -Name $Script:Config.ServiceName -ErrorAction SilentlyContinue
if ($service) {
    Write-Host "Stopping service..." -ForegroundColor Yellow
    Stop-Service -Name $Script:Config.ServiceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2

    Write-Host "Removing service..." -ForegroundColor Yellow
    & sc.exe delete $Script:Config.ServiceName
    Write-Host "[SUCCESS] Windows Service removed" -ForegroundColor Green
} else {
    Write-Host "[INFO] Service not found" -ForegroundColor Yellow
}

# Remove IIS Website
Write-Host ""
Write-Host "Removing IIS Website..." -ForegroundColor Cyan
try {
    Import-Module WebAdministration -ErrorAction Stop

    if (Test-Path "IIS:\Sites\$($Script:Config.IISSiteName)") {
        Remove-Website -Name $Script:Config.IISSiteName
        Write-Host "[SUCCESS] IIS Website removed" -ForegroundColor Green
    }

    if (Test-Path "IIS:\AppPools\$($Script:Config.IISSiteName)") {
        Remove-WebAppPool -Name $Script:Config.IISSiteName
        Write-Host "[SUCCESS] Application Pool removed" -ForegroundColor Green
    }
} catch {
    Write-Host "[ERROR] Failed to remove IIS components: $($_.Exception.Message)" -ForegroundColor Red
}

# Remove installation files
Write-Host ""
Write-Host "Removing installation files..." -ForegroundColor Cyan
if (Test-Path $Script:Config.InstallPath) {
    try {
        Remove-Item -Path $Script:Config.InstallPath -Recurse -Force
        Write-Host "[SUCCESS] Installation files removed" -ForegroundColor Green
    } catch {
        Write-Host "[ERROR] Failed to remove files: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "You may need to manually delete: $($Script:Config.InstallPath)" -ForegroundColor Yellow
    }
} else {
    Write-Host "[INFO] Installation directory not found" -ForegroundColor Yellow
}

# Ask about database
Write-Host ""
$removeDb = Read-Host "Do you want to remove the database? (Y/N)"
if ($removeDb -eq "Y" -or $removeDb -eq "y") {
    Write-Host "[WARNING] Database removal must be done manually using SQL Server Management Studio" -ForegroundColor Yellow
    Write-Host "Database name: ComplaintManagementDB" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║         UNINSTALLATION COMPLETED SUCCESSFULLY              ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
