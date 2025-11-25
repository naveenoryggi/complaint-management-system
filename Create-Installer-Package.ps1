#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Creates a distributable installation package for Complaint Management System
.DESCRIPTION
    This script creates a ZIP package containing:
    - Installation scripts
    - Source code
    - Documentation
    - Prerequisites guide
#>

$ErrorActionPreference = "Stop"

# Configuration
$PackageName = "ComplaintManagementSystem-Installer-v1.0.0"
$OutputPath = Join-Path $PSScriptRoot "$PackageName.zip"
$TempBuildPath = Join-Path $env:TEMP "ComplaintManagementBuild"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Complaint Management System - Package Builder           ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Clean temp directory
Write-Host "[INFO] Preparing build directory..." -ForegroundColor Yellow
if (Test-Path $TempBuildPath) {
    Remove-Item $TempBuildPath -Recurse -Force
}
New-Item -ItemType Directory -Path $TempBuildPath | Out-Null

# Copy source code
Write-Host "[INFO] Copying source code..." -ForegroundColor Yellow
$SourceFolders = @(
    "complaint-system-dotnet",
    "complaint-system-angular"
)

foreach ($folder in $SourceFolders) {
    $sourcePath = Join-Path $PSScriptRoot $folder
    $destPath = Join-Path $TempBuildPath $folder

    if (Test-Path $sourcePath) {
        Write-Host "  - Copying $folder..." -ForegroundColor Gray

        # Copy folder excluding node_modules, bin, obj, dist
        robocopy $sourcePath $destPath /E /XD node_modules bin obj dist .vs .angular /XF *.user *.suo /NFL /NDL /NJH /NJS /nc /ns /np | Out-Null

        if ($LASTEXITCODE -le 7) {
            Write-Host "    ✓ $folder copied" -ForegroundColor Green
        }
    }
}

# Copy installer scripts
Write-Host "[INFO] Copying installer scripts..." -ForegroundColor Yellow
$InstallerFiles = @(
    "Install-ComplaintManagementSystem.ps1",
    "Uninstall-ComplaintManagementSystem.ps1",
    "INSTALLATION_GUIDE.md"
)

foreach ($file in $InstallerFiles) {
    $sourcePath = Join-Path $PSScriptRoot $file
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath -Destination $TempBuildPath
        Write-Host "  ✓ $file" -ForegroundColor Green
    }
}

# Create README for the package
Write-Host "[INFO] Creating package README..." -ForegroundColor Yellow
$ReadmeContent = @"
# Complaint Management System - Installation Package v1.0.0

## Quick Start

1. **Extract this ZIP file** to a temporary location
2. **Run as Administrator**: Right-click `Install-ComplaintManagementSystem.ps1` → Run with PowerShell
3. **Follow the installation wizard**
4. **Access the application** at http://localhost

## What's Included

- ✅ Complete source code (.NET 8 + Angular 18)
- ✅ Automated installation script
- ✅ Database setup and migration
- ✅ Windows Service installer
- ✅ IIS configuration
- ✅ Comprehensive documentation

## Prerequisites

Before running the installer, ensure you have:

1. **Windows Server 2016+ or Windows 10/11 (64-bit)**
2. **.NET 8 Runtime** - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
3. **IIS** - Enable via Windows Features
4. **SQL Server 2016+** - Express, Standard, or Enterprise

## Installation Steps

### Step 1: Install Prerequisites

**Install .NET 8 Runtime:**
```powershell
# Download and install from:
https://dotnet.microsoft.com/download/dotnet/8.0
# Choose: "ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle"
```

**Enable IIS:**
```powershell
# Windows Server:
Install-WindowsFeature -Name Web-Server -IncludeManagementTools

# Windows 10/11:
# Control Panel → Programs → Turn Windows features on or off
# Enable "Internet Information Services"
```

**Install SQL Server Express (Free):**
```
Download from: https://www.microsoft.com/sql-server/sql-server-downloads
Choose "Express" edition for free installation
```

### Step 2: Run Installer

```powershell
# Right-click and select "Run with PowerShell"
.\Install-ComplaintManagementSystem.ps1
```

### Step 3: Configure Database

When prompted, enter your SQL Server details:

**Example 1: SQL Server Express with Windows Authentication**
```
SQL Server Name: localhost\SQLEXPRESS
Database Name: ComplaintManagementDB
Authentication: 1 (Windows Authentication)
```

**Example 2: SQL Server with SQL Authentication**
```
SQL Server Name: localhost
Database Name: ComplaintManagementDB
Authentication: 2 (SQL Server Authentication)
Login ID: sa
Password: YourPassword123!
```

### Step 4: Complete Installation

The installer will automatically:
- ✅ Build the application
- ✅ Create and configure database
- ✅ Install Windows Service for API
- ✅ Configure IIS website
- ✅ Start all services

## Post-Installation

### Access the Application

Open your browser and navigate to:
```
http://localhost
```

### Default Login Credentials

```
Username: admin@complaintmanagement.com
Password: Admin@123
```

**⚠️ IMPORTANT: Change the default password immediately after first login!**

## Documentation

- **Installation Guide**: `INSTALLATION_GUIDE.md` - Comprehensive installation documentation
- **User Manual**: Access from the application Help menu
- **API Documentation**: http://localhost:5000/swagger (after installation)

## Troubleshooting

### Installer Won't Run

**Error**: "Execution of scripts is disabled on this system"

**Solution**:
```powershell
# Run PowerShell as Administrator and execute:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Database Connection Failed

1. Verify SQL Server is running:
   ```powershell
   Get-Service MSSQL*
   ```

2. Test connection manually:
   ```powershell
   sqlcmd -S localhost\SQLEXPRESS -E
   ```

3. Check SQL Server Configuration Manager:
   - Enable TCP/IP protocol
   - Restart SQL Server service

### IIS Website Not Accessible

1. Check if IIS is installed:
   ```powershell
   Get-WindowsFeature -Name Web-Server
   ```

2. Verify website is running:
   ```powershell
   Get-Website -Name ComplaintManagement
   ```

3. Check Windows Firewall:
   - Allow port 80 (HTTP)
   - Allow port 443 (HTTPS)

### Service Won't Start

1. Check service status:
   ```powershell
   Get-Service ComplaintManagementAPI
   ```

2. View service logs:
   ```
   C:\Program Files\ComplaintManagement\API\Logs\
   ```

3. Verify database connection string in:
   ```
   C:\Program Files\ComplaintManagement\API\appsettings.json
   ```

## System Requirements

### Minimum Requirements
- **OS**: Windows Server 2016 / Windows 10 (64-bit)
- **CPU**: 2 GHz dual-core processor
- **RAM**: 4 GB
- **Disk**: 2 GB free space
- **Network**: Internet connection (for initial setup)

### Recommended Requirements
- **OS**: Windows Server 2019/2022
- **CPU**: 2.5 GHz quad-core processor
- **RAM**: 8 GB
- **Disk**: 10 GB free space (SSD recommended)
- **Network**: 100 Mbps+ connection

## Features

✅ **Complete Complaint Management System**
- Multi-tenant architecture
- Role-based access control (Admin, Technician, User)
- Real-time notifications
- Email integration (SMTP + IMAP)
- OAuth 2.0 support (Office 365, Gmail)
- Automatic ticket creation from emails
- Advanced reporting and analytics
- Document attachments
- Mobile-responsive design

✅ **Production-Ready**
- Windows Service deployment
- IIS hosting
- SQL Server database
- Automated backups
- Comprehensive logging
- Security hardening

## Uninstallation

To remove the application:

```powershell
# Run as Administrator
.\Uninstall-ComplaintManagementSystem.ps1
```

## Support

For technical support or questions:
- Email: support@yourcompany.com
- Documentation: See `INSTALLATION_GUIDE.md`
- Issue Tracker: [Your GitHub/Support URL]

## License

Commercial License - Copyright © 2025
All rights reserved.

---

**Version**: 1.0.0
**Release Date**: January 2025
**Build Date**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

*Thank you for choosing Complaint Management System!*
"@

$ReadmeContent | Out-File -FilePath (Join-Path $TempBuildPath "README.md") -Encoding UTF8
Write-Host "  ✓ README.md created" -ForegroundColor Green

# Create a quick start guide
Write-Host "[INFO] Creating Quick Start Guide..." -ForegroundColor Yellow
$QuickStartContent = @"
# QUICK START GUIDE

## 3-Step Installation

### 1. Install Prerequisites (5 minutes)

Run PowerShell as Administrator and execute:

``````powershell
# Install .NET 8 Runtime
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
# Choose: ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle

# Enable IIS
Install-WindowsFeature -Name Web-Server -IncludeManagementTools
``````

### 2. Run Installer (10 minutes)

Right-click `Install-ComplaintManagementSystem.ps1` and select "Run with PowerShell"

Follow the wizard:
- Enter SQL Server details
- Confirm installation
- Wait for completion

### 3. Access Application (1 minute)

Open browser: http://localhost

Login:
- Username: admin@complaintmanagement.com
- Password: Admin@123

**DONE! 🎉**

---

## Troubleshooting

**Can't run installer?**
``````powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
``````

**Database connection failed?**
- Use `localhost\SQLEXPRESS` for SQL Express
- Or install SQL Server Express from: https://go.microsoft.com/fwlink/?linkid=866662

**Need help?**
See `INSTALLATION_GUIDE.md` for detailed instructions

---

*Total Installation Time: ~15-20 minutes*
"@

$QuickStartContent | Out-File -FilePath (Join-Path $TempBuildPath "QUICK_START.md") -Encoding UTF8
Write-Host "  ✓ QUICK_START.md created" -ForegroundColor Green

# Create version info file
Write-Host "[INFO] Creating version info..." -ForegroundColor Yellow
$VersionInfo = @{
    Version = "1.0.0"
    BuildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    DotNetVersion = "8.0"
    AngularVersion = "18.0"
    Features = @(
        "Multi-tenant support",
        "Role-based access control",
        "Email integration (SMTP/IMAP)",
        "OAuth 2.0 authentication",
        "Automatic ticket creation",
        "Real-time notifications",
        "Advanced reporting",
        "Document management"
    )
}
$VersionInfo | ConvertTo-Json -Depth 5 | Out-File -FilePath (Join-Path $TempBuildPath "version.json") -Encoding UTF8
Write-Host "  ✓ version.json created" -ForegroundColor Green

# Create ZIP package
Write-Host ""
Write-Host "[INFO] Creating ZIP package..." -ForegroundColor Yellow

if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Force
}

Add-Type -Assembly "System.IO.Compression.FileSystem"
[System.IO.Compression.ZipFile]::CreateFromDirectory($TempBuildPath, $OutputPath)

Write-Host "[SUCCESS] Package created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Package Details:" -ForegroundColor Cyan
Write-Host "  - File: $OutputPath" -ForegroundColor White
Write-Host "  - Size: $([math]::Round((Get-Item $OutputPath).Length / 1MB, 2)) MB" -ForegroundColor White
Write-Host ""

# Cleanup
Write-Host "[INFO] Cleaning up temporary files..." -ForegroundColor Yellow
Remove-Item $TempBuildPath -Recurse -Force
Write-Host "[SUCCESS] Cleanup complete" -ForegroundColor Green

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              PACKAGE BUILD COMPLETE!                       ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "The installer package is ready for distribution:" -ForegroundColor Cyan
Write-Host "  $OutputPath" -ForegroundColor White
Write-Host ""
Write-Host "To distribute:" -ForegroundColor Yellow
Write-Host "  1. Send the ZIP file to end users" -ForegroundColor White
Write-Host "  2. Users extract the ZIP" -ForegroundColor White
Write-Host "  3. Users run Install-ComplaintManagementSystem.ps1 as Administrator" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
