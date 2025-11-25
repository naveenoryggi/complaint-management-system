#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Complaint Management System - Production Installer
.DESCRIPTION
    Interactive installer for the Complaint Management System
    - Configures SQL Server database connection
    - Runs EF Core migrations
    - Installs .NET API as Windows Service
    - Deploys Angular frontend to IIS
.NOTES
    Requires Administrator privileges
#>

# ============================================
# CONFIGURATION
# ============================================
$Script:Config = @{
    AppName = "Complaint Management System"
    Version = "1.0.0"
    ServiceName = "ComplaintManagementAPI"
    ServiceDisplayName = "Complaint Management API Service"
    ServiceDescription = "Backend API service for Complaint Management System"
    IISSiteName = "ComplaintManagement"
    IISPort = 80
    APIPort = 5000
    InstallPath = "C:\Program Files\ComplaintManagement"
}

# ============================================
# HELPER FUNCTIONS
# ============================================

function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host " $Text" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Yellow
}

function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Test-Prerequisites {
    Write-Header "Checking Prerequisites"

    $prerequisites = @()

    # Check .NET 8 SDK/Runtime
    Write-Info "Checking .NET 8 Runtime..."
    try {
        $dotnetVersion = & dotnet --list-runtimes | Select-String "Microsoft.AspNetCore.App 8"
        if ($dotnetVersion) {
            Write-Success ".NET 8 Runtime found"
        } else {
            $prerequisites += "Please install .NET 8 Runtime from https://dotnet.microsoft.com/download/dotnet/8.0"
        }
    } catch {
        $prerequisites += "Please install .NET 8 Runtime from https://dotnet.microsoft.com/download/dotnet/8.0"
    }

    # Check IIS
    Write-Info "Checking IIS..."
    $iis = Get-WindowsFeature -Name Web-Server -ErrorAction SilentlyContinue
    if ($iis -and $iis.Installed) {
        Write-Success "IIS is installed"
    } else {
        $prerequisites += "IIS is not installed. Run: Install-WindowsFeature -Name Web-Server -IncludeManagementTools"
    }

    # Check SQL Server availability
    Write-Info "Checking SQL Server client tools..."
    try {
        $sqlcmd = Get-Command sqlcmd -ErrorAction Stop
        Write-Success "SQL Server client tools found"
    } catch {
        Write-Info "SQL Server command-line tools not found (optional)"
    }

    if ($prerequisites.Count -gt 0) {
        Write-Error "Missing prerequisites:"
        $prerequisites | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
        return $false
    }

    Write-Success "All prerequisites met"
    return $true
}

function Get-DatabaseConfiguration {
    Write-Header "Database Configuration"

    $dbConfig = @{}

    Write-Host "Enter SQL Server connection details:" -ForegroundColor Cyan
    Write-Host ""

    # Server Name
    $defaultServer = "localhost"
    $serverInput = Read-Host "SQL Server Name/IP [$defaultServer]"
    $dbConfig.Server = if ([string]::IsNullOrWhiteSpace($serverInput)) { $defaultServer } else { $serverInput }

    # Database Name
    $defaultDatabase = "ComplaintManagementDB"
    $databaseInput = Read-Host "Database Name [$defaultDatabase]"
    $dbConfig.Database = if ([string]::IsNullOrWhiteSpace($databaseInput)) { $defaultDatabase } else { $databaseInput }

    # Authentication Type
    Write-Host ""
    Write-Host "Authentication Type:" -ForegroundColor Cyan
    Write-Host "1. Windows Authentication (Integrated Security)"
    Write-Host "2. SQL Server Authentication (Username/Password)"
    $authChoice = Read-Host "Select authentication type [1/2]"

    if ($authChoice -eq "2") {
        $dbConfig.UseWindowsAuth = $false
        $dbConfig.UserId = Read-Host "SQL Server Login ID"
        $dbConfig.Password = Read-Host "SQL Server Password" -AsSecureString
        $dbConfig.PasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
            [Runtime.InteropServices.Marshal]::SecureStringToBSTR($dbConfig.Password)
        )
    } else {
        $dbConfig.UseWindowsAuth = $true
    }

    # Build Connection String
    if ($dbConfig.UseWindowsAuth) {
        $dbConfig.ConnectionString = "Server=$($dbConfig.Server);Database=$($dbConfig.Database);Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
    } else {
        $dbConfig.ConnectionString = "Server=$($dbConfig.Server);Database=$($dbConfig.Database);User Id=$($dbConfig.UserId);Password=$($dbConfig.PasswordPlain);TrustServerCertificate=True;MultipleActiveResultSets=True"
    }

    Write-Host ""
    Write-Info "Testing database connection..."

    if (Test-DatabaseConnection -ConnectionString $dbConfig.ConnectionString) {
        Write-Success "Database connection successful"
        return $dbConfig
    } else {
        Write-Error "Failed to connect to database"
        $retry = Read-Host "Would you like to retry? (Y/N)"
        if ($retry -eq "Y" -or $retry -eq "y") {
            return Get-DatabaseConfiguration
        } else {
            return $null
        }
    }
}

function Test-DatabaseConnection {
    param([string]$ConnectionString)

    try {
        $apiPath = Join-Path $PSScriptRoot "complaint-system-dotnet\src\ComplaintManagement.API"
        $testCode = @"
using System;
using Microsoft.Data.SqlClient;

try {
    using var connection = new SqlConnection(@"$ConnectionString");
    connection.Open();
    Console.WriteLine("SUCCESS");
    return 0;
} catch (Exception ex) {
    Console.WriteLine("ERROR: " + ex.Message);
    return 1;
}
"@
        $testFile = [System.IO.Path]::GetTempFileName() + ".cs"
        $testCode | Out-File -FilePath $testFile -Encoding UTF8

        $result = & dotnet script $testFile 2>&1
        Remove-Item $testFile -Force

        return $result -match "SUCCESS"
    } catch {
        Write-Error "Connection test failed: $($_.Exception.Message)"
        return $false
    }
}

function Install-Database {
    param([hashtable]$DbConfig)

    Write-Header "Database Installation"

    $apiPath = Join-Path $PSScriptRoot "complaint-system-dotnet\src\ComplaintManagement.API"

    if (-not (Test-Path $apiPath)) {
        Write-Error "API project not found at: $apiPath"
        return $false
    }

    # Update connection string in appsettings.json
    Write-Info "Updating appsettings.json..."
    $appsettingsPath = Join-Path $apiPath "appsettings.json"
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $appsettings.ConnectionStrings.DefaultConnection = $DbConfig.ConnectionString
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Success "Connection string updated"

    # Run EF Core migrations
    Write-Info "Running database migrations..."
    Push-Location $apiPath
    try {
        $migrationResult = & dotnet ef database update --verbose 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Database migrations completed successfully"
            return $true
        } else {
            Write-Error "Migration failed: $migrationResult"
            return $false
        }
    } catch {
        Write-Error "Migration error: $($_.Exception.Message)"
        return $false
    } finally {
        Pop-Location
    }
}

function Build-Application {
    Write-Header "Building Application"

    # Build .NET API
    Write-Info "Building .NET API..."
    $apiPath = Join-Path $PSScriptRoot "complaint-system-dotnet\src\ComplaintManagement.API"
    Push-Location $apiPath
    try {
        & dotnet publish -c Release -o "$($Script:Config.InstallPath)\API" --self-contained false
        if ($LASTEXITCODE -eq 0) {
            Write-Success ".NET API built successfully"
        } else {
            Write-Error "Failed to build .NET API"
            return $false
        }
    } finally {
        Pop-Location
    }

    # Build Angular frontend
    Write-Info "Building Angular frontend..."
    $angularPath = Join-Path $PSScriptRoot "complaint-system-angular"
    Push-Location $angularPath
    try {
        & npm run build --prod
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Angular frontend built successfully"

            # Copy to install location
            $distPath = Join-Path $angularPath "dist\complaint-system-angular"
            $wwwPath = "$($Script:Config.InstallPath)\WWW"
            Copy-Item -Path $distPath -Destination $wwwPath -Recurse -Force
            Write-Success "Frontend files copied to installation directory"
        } else {
            Write-Error "Failed to build Angular frontend"
            return $false
        }
    } finally {
        Pop-Location
    }

    return $true
}

function Install-WindowsService {
    Write-Header "Installing Windows Service"

    $servicePath = "$($Script:Config.InstallPath)\API\ComplaintManagement.API.exe"

    # Check if service already exists
    $existingService = Get-Service -Name $Script:Config.ServiceName -ErrorAction SilentlyContinue
    if ($existingService) {
        Write-Info "Service already exists. Stopping and removing..."
        Stop-Service -Name $Script:Config.ServiceName -Force
        & sc.exe delete $Script:Config.ServiceName
        Start-Sleep -Seconds 2
    }

    # Create Windows Service using sc.exe
    Write-Info "Creating Windows Service..."
    $scCommand = "create `"$($Script:Config.ServiceName)`" binPath= `"$servicePath`" DisplayName= `"$($Script:Config.ServiceDisplayName)`" start= auto"
    & sc.exe $scCommand

    if ($LASTEXITCODE -eq 0) {
        & sc.exe description $Script:Config.ServiceName $Script:Config.ServiceDescription
        Write-Success "Windows Service created successfully"

        Write-Info "Starting service..."
        Start-Service -Name $Script:Config.ServiceName

        $service = Get-Service -Name $Script:Config.ServiceName
        if ($service.Status -eq 'Running') {
            Write-Success "Service is running"
            return $true
        } else {
            Write-Error "Service failed to start"
            return $false
        }
    } else {
        Write-Error "Failed to create Windows Service"
        return $false
    }
}

function Install-IISSite {
    Write-Header "Configuring IIS"

    Import-Module WebAdministration -ErrorAction Stop

    $sitePath = "$($Script:Config.InstallPath)\WWW"
    $appPoolName = $Script:Config.IISSiteName

    # Create Application Pool
    Write-Info "Creating IIS Application Pool..."
    if (Test-Path "IIS:\AppPools\$appPoolName") {
        Remove-WebAppPool -Name $appPoolName
    }
    New-WebAppPool -Name $appPoolName
    Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name managedRuntimeVersion -Value ""
    Write-Success "Application Pool created"

    # Create Website
    Write-Info "Creating IIS Website..."
    if (Test-Path "IIS:\Sites\$($Script:Config.IISSiteName)") {
        Remove-Website -Name $Script:Config.IISSiteName
    }
    New-Website -Name $Script:Config.IISSiteName -Port $Script:Config.IISPort -PhysicalPath $sitePath -ApplicationPool $appPoolName
    Write-Success "IIS Website created"

    # Start website
    Start-Website -Name $Script:Config.IISSiteName
    Write-Success "Website started on port $($Script:Config.IISPort)"

    return $true
}

function Show-CompletionMessage {
    Write-Header "Installation Complete!"

    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║  COMPLAINT MANAGEMENT SYSTEM - INSTALLATION SUCCESSFUL     ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "Application Details:" -ForegroundColor Cyan
    Write-Host "  - Frontend URL:  http://localhost:$($Script:Config.IISPort)" -ForegroundColor White
    Write-Host "  - API URL:       http://localhost:$($Script:Config.APIPort)" -ForegroundColor White
    Write-Host "  - Service Name:  $($Script:Config.ServiceName)" -ForegroundColor White
    Write-Host "  - Install Path:  $($Script:Config.InstallPath)" -ForegroundColor White
    Write-Host ""
    Write-Host "Default Admin Credentials:" -ForegroundColor Cyan
    Write-Host "  - Username: admin@complaintmanagement.com" -ForegroundColor White
    Write-Host "  - Password: Admin@123" -ForegroundColor White
    Write-Host ""
    Write-Host "Service Management:" -ForegroundColor Cyan
    Write-Host "  - Start:   net start $($Script:Config.ServiceName)" -ForegroundColor White
    Write-Host "  - Stop:    net stop $($Script:Config.ServiceName)" -ForegroundColor White
    Write-Host "  - Status:  sc query $($Script:Config.ServiceName)" -ForegroundColor White
    Write-Host ""
    Write-Host "Press any key to open the application..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    Start-Process "http://localhost:$($Script:Config.IISPort)"
}

# ============================================
# MAIN INSTALLATION FLOW
# ============================================

Clear-Host

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                                                            ║" -ForegroundColor Cyan
Write-Host "║     COMPLAINT MANAGEMENT SYSTEM - INSTALLER v1.0.0         ║" -ForegroundColor Cyan
Write-Host "║                                                            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Check administrator privileges
if (-not (Test-Administrator)) {
    Write-Error "This installer must be run as Administrator"
    Write-Host "Please right-click and select 'Run as Administrator'"
    Pause
    exit 1
}

# Check prerequisites
if (-not (Test-Prerequisites)) {
    Write-Host ""
    Write-Host "Please install missing prerequisites and run the installer again" -ForegroundColor Yellow
    Pause
    exit 1
}

# Get database configuration
$dbConfig = Get-DatabaseConfiguration
if (-not $dbConfig) {
    Write-Error "Database configuration failed. Installation aborted."
    Pause
    exit 1
}

# Confirm installation
Write-Host ""
Write-Host "Ready to install Complaint Management System" -ForegroundColor Cyan
Write-Host "  - Install Location: $($Script:Config.InstallPath)" -ForegroundColor White
Write-Host "  - Database: $($dbConfig.Server)\$($dbConfig.Database)" -ForegroundColor White
Write-Host ""
$confirm = Read-Host "Continue with installation? (Y/N)"
if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Info "Installation cancelled by user"
    exit 0
}

# Create install directory
Write-Info "Creating installation directory..."
New-Item -ItemType Directory -Path $Script:Config.InstallPath -Force | Out-Null

# Build application
if (-not (Build-Application)) {
    Write-Error "Build failed. Installation aborted."
    Pause
    exit 1
}

# Install database
if (-not (Install-Database -DbConfig $dbConfig)) {
    Write-Error "Database installation failed. Installation aborted."
    Pause
    exit 1
}

# Install Windows Service
if (-not (Install-WindowsService)) {
    Write-Error "Windows Service installation failed. Installation aborted."
    Pause
    exit 1
}

# Install IIS Site
if (-not (Install-IISSite)) {
    Write-Error "IIS configuration failed. Installation aborted."
    Pause
    exit 1
}

# Show completion message
Show-CompletionMessage
