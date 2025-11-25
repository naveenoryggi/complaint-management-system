# PowerShell script to create .NET 8 solution structure with detailed logging
# Run this with: powershell -ExecutionPolicy Bypass -File setup-solution-with-logging.ps1

$ErrorActionPreference = "Continue"
$LogFile = "setup-log.txt"
$ErrorLogFile = "setup-errors.txt"

# Function to log both to console and file
function Write-Log {
    param($Message, $Color = "White")
    Write-Host $Message -ForegroundColor $Color
    Add-Content -Path $LogFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $Message"
}

function Write-ErrorLog {
    param($Message)
    Write-Host $Message -ForegroundColor Red
    Add-Content -Path $LogFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - ERROR: $Message"
    Add-Content -Path $ErrorLogFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $Message"
}

# Clear old logs
if (Test-Path $LogFile) { Remove-Item $LogFile }
if (Test-Path $ErrorLogFile) { Remove-Item $ErrorLogFile }

Write-Log "============================================" "Cyan"
Write-Log "  .NET 8 Solution Setup - Started" "Cyan"
Write-Log "============================================" "Cyan"
Write-Log ""

# Check prerequisites
Write-Log "Checking prerequisites..." "Yellow"

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Log "✓ .NET SDK version: $dotnetVersion" "Green"
    } else {
        Write-ErrorLog ".NET SDK not found. Please install from: https://dotnet.microsoft.com/download/dotnet/8.0"
        exit 1
    }
} catch {
    Write-ErrorLog ".NET SDK not found. Error: $_"
    exit 1
}

Write-Log ""

# Step 1: Create solution
Write-Log "============================================" "Cyan"
Write-Log "Step 1: Creating solution..." "Cyan"
Write-Log "============================================" "Cyan"

try {
    dotnet new sln -n ComplaintManagementSystem 2>&1 | Tee-Object -Append -FilePath $LogFile
    if ($LASTEXITCODE -eq 0) {
        Write-Log "✓ Solution created successfully" "Green"
    } else {
        Write-ErrorLog "Failed to create solution. Exit code: $LASTEXITCODE"
    }
} catch {
    Write-ErrorLog "Exception creating solution: $_"
}

Write-Log ""

# Create src directory
if (-not (Test-Path "src")) {
    New-Item -ItemType Directory -Force -Path "src" | Out-Null
    Write-Log "✓ Created src directory" "Green"
}

Write-Log ""

# Step 2: Create Domain project
Write-Log "============================================" "Cyan"
Write-Log "Step 2: Creating Domain project..." "Cyan"
Write-Log "============================================" "Cyan"

try {
    dotnet new classlib -n ComplaintManagement.Domain -o src/ComplaintManagement.Domain -f net8.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    if ($LASTEXITCODE -eq 0) {
        Write-Log "✓ Domain project created" "Green"

        dotnet sln add src/ComplaintManagement.Domain/ComplaintManagement.Domain.csproj 2>&1 | Tee-Object -Append -FilePath $LogFile
        if ($LASTEXITCODE -eq 0) {
            Write-Log "✓ Domain project added to solution" "Green"
        } else {
            Write-ErrorLog "Failed to add Domain project to solution"
        }
    } else {
        Write-ErrorLog "Failed to create Domain project"
    }
} catch {
    Write-ErrorLog "Exception creating Domain project: $_"
}

Write-Log ""

# Step 3: Create Application project
Write-Log "============================================" "Cyan"
Write-Log "Step 3: Creating Application project..." "Cyan"
Write-Log "============================================" "Cyan"

try {
    dotnet new classlib -n ComplaintManagement.Application -o src/ComplaintManagement.Application -f net8.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    if ($LASTEXITCODE -eq 0) {
        Write-Log "✓ Application project created" "Green"

        dotnet sln add src/ComplaintManagement.Application/ComplaintManagement.Application.csproj 2>&1 | Tee-Object -Append -FilePath $LogFile
        dotnet add src/ComplaintManagement.Application reference src/ComplaintManagement.Domain 2>&1 | Tee-Object -Append -FilePath $LogFile
        Write-Log "✓ Application project configured" "Green"
    } else {
        Write-ErrorLog "Failed to create Application project"
    }
} catch {
    Write-ErrorLog "Exception creating Application project: $_"
}

Write-Log ""

# Step 4: Create Infrastructure project
Write-Log "============================================" "Cyan"
Write-Log "Step 4: Creating Infrastructure project..." "Cyan"
Write-Log "============================================" "Cyan"

try {
    dotnet new classlib -n ComplaintManagement.Infrastructure -o src/ComplaintManagement.Infrastructure -f net8.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    if ($LASTEXITCODE -eq 0) {
        Write-Log "✓ Infrastructure project created" "Green"

        dotnet sln add src/ComplaintManagement.Infrastructure/ComplaintManagement.Infrastructure.csproj 2>&1 | Tee-Object -Append -FilePath $LogFile
        dotnet add src/ComplaintManagement.Infrastructure reference src/ComplaintManagement.Domain 2>&1 | Tee-Object -Append -FilePath $LogFile
        dotnet add src/ComplaintManagement.Infrastructure reference src/ComplaintManagement.Application 2>&1 | Tee-Object -Append -FilePath $LogFile
        Write-Log "✓ Infrastructure project configured" "Green"
    } else {
        Write-ErrorLog "Failed to create Infrastructure project"
    }
} catch {
    Write-ErrorLog "Exception creating Infrastructure project: $_"
}

Write-Log ""

# Step 5: Create Shared project
Write-Log "============================================" "Cyan"
Write-Log "Step 5: Creating Shared project..." "Cyan"
Write-Log "============================================" "Cyan"

try {
    dotnet new classlib -n ComplaintManagement.Shared -o src/ComplaintManagement.Shared -f net8.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    if ($LASTEXITCODE -eq 0) {
        Write-Log "✓ Shared project created" "Green"

        dotnet sln add src/ComplaintManagement.Shared/ComplaintManagement.Shared.csproj 2>&1 | Tee-Object -Append -FilePath $LogFile
        Write-Log "✓ Shared project added to solution" "Green"
    } else {
        Write-ErrorLog "Failed to create Shared project"
    }
} catch {
    Write-ErrorLog "Exception creating Shared project: $_"
}

Write-Log ""

# Step 6: Create Web API project
Write-Log "============================================" "Cyan"
Write-Log "Step 6: Creating Web API project..." "Cyan"
Write-Log "============================================" "Cyan"

try {
    dotnet new webapi -n ComplaintManagement.API -o src/ComplaintManagement.API -f net8.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    if ($LASTEXITCODE -eq 0) {
        Write-Log "✓ Web API project created" "Green"

        dotnet sln add src/ComplaintManagement.API/ComplaintManagement.API.csproj 2>&1 | Tee-Object -Append -FilePath $LogFile
        dotnet add src/ComplaintManagement.API reference src/ComplaintManagement.Application 2>&1 | Tee-Object -Append -FilePath $LogFile
        dotnet add src/ComplaintManagement.API reference src/ComplaintManagement.Infrastructure 2>&1 | Tee-Object -Append -FilePath $LogFile
        dotnet add src/ComplaintManagement.API reference src/ComplaintManagement.Shared 2>&1 | Tee-Object -Append -FilePath $LogFile
        Write-Log "✓ Web API project configured" "Green"
    } else {
        Write-ErrorLog "Failed to create Web API project"
    }
} catch {
    Write-ErrorLog "Exception creating Web API project: $_"
}

Write-Log ""

# Step 7: Create Worker Service project
Write-Log "============================================" "Cyan"
Write-Log "Step 7: Creating Worker Service project..." "Cyan"
Write-Log "============================================" "Cyan"

try {
    dotnet new worker -n ComplaintManagement.WorkerService -o src/ComplaintManagement.WorkerService -f net8.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    if ($LASTEXITCODE -eq 0) {
        Write-Log "✓ Worker Service project created" "Green"

        dotnet sln add src/ComplaintManagement.WorkerService/ComplaintManagement.WorkerService.csproj 2>&1 | Tee-Object -Append -FilePath $LogFile
        dotnet add src/ComplaintManagement.WorkerService reference src/ComplaintManagement.Application 2>&1 | Tee-Object -Append -FilePath $LogFile
        dotnet add src/ComplaintManagement.WorkerService reference src/ComplaintManagement.Infrastructure 2>&1 | Tee-Object -Append -FilePath $LogFile
        dotnet add src/ComplaintManagement.WorkerService reference src/ComplaintManagement.Shared 2>&1 | Tee-Object -Append -FilePath $LogFile
        Write-Log "✓ Worker Service project configured" "Green"
    } else {
        Write-ErrorLog "Failed to create Worker Service project"
    }
} catch {
    Write-ErrorLog "Exception creating Worker Service project: $_"
}

Write-Log ""
Write-Log ""

# Step 8: Install NuGet packages
Write-Log "============================================" "Cyan"
Write-Log "Step 8: Installing NuGet packages..." "Cyan"
Write-Log "============================================" "Cyan"
Write-Log "This may take a few minutes..." "Yellow"
Write-Log ""

# Application packages
Write-Log "Installing Application packages..." "Yellow"
try {
    dotnet add src/ComplaintManagement.Application package AutoMapper --version 12.0.1 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Application package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Application package FluentValidation --version 11.9.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Application package FluentValidation.DependencyInjectionExtensions --version 11.9.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Application package MediatR --version 12.2.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    Write-Log "✓ Application packages installed" "Green"
} catch {
    Write-ErrorLog "Exception installing Application packages: $_"
}

Write-Log ""

# Infrastructure packages
Write-Log "Installing Infrastructure packages..." "Yellow"
try {
    dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore --version 8.0.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 8.0.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 8.0.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Infrastructure package Dapper --version 2.1.28 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Infrastructure package StackExchange.Redis --version 2.7.10 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Infrastructure package Hangfire.AspNetCore --version 1.8.9 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.Infrastructure package Hangfire.SqlServer --version 1.8.9 2>&1 | Tee-Object -Append -FilePath $LogFile
    Write-Log "✓ Infrastructure packages installed" "Green"
} catch {
    Write-ErrorLog "Exception installing Infrastructure packages: $_"
}

Write-Log ""

# API packages
Write-Log "Installing API packages..." "Yellow"
try {
    dotnet add src/ComplaintManagement.API package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.API package Swashbuckle.AspNetCore --version 6.5.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.API package Serilog.AspNetCore --version 8.0.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.API package Serilog.Sinks.Console --version 5.0.1 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.API package Serilog.Sinks.File --version 5.0.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.API package BCrypt.Net-Next --version 4.0.3 2>&1 | Tee-Object -Append -FilePath $LogFile
    Write-Log "✓ API packages installed" "Green"
} catch {
    Write-ErrorLog "Exception installing API packages: $_"
}

Write-Log ""

# Worker Service packages
Write-Log "Installing Worker Service packages..." "Yellow"
try {
    dotnet add src/ComplaintManagement.WorkerService package Hangfire.AspNetCore --version 1.8.9 2>&1 | Tee-Object -Append -FilePath $LogFile
    dotnet add src/ComplaintManagement.WorkerService package Serilog.Extensions.Hosting --version 8.0.0 2>&1 | Tee-Object -Append -FilePath $LogFile
    Write-Log "✓ Worker Service packages installed" "Green"
} catch {
    Write-ErrorLog "Exception installing Worker Service packages: $_"
}

Write-Log ""
Write-Log ""

# Step 9: Build solution
Write-Log "============================================" "Cyan"
Write-Log "Step 9: Building solution..." "Cyan"
Write-Log "============================================" "Cyan"

try {
    dotnet build 2>&1 | Tee-Object -Append -FilePath $LogFile
    if ($LASTEXITCODE -eq 0) {
        Write-Log "✓ Solution built successfully" "Green"
    } else {
        Write-ErrorLog "Build failed. Exit code: $LASTEXITCODE"
    }
} catch {
    Write-ErrorLog "Exception during build: $_"
}

Write-Log ""
Write-Log ""

# Summary
Write-Log "============================================" "Cyan"
Write-Log "  SETUP SUMMARY" "Cyan"
Write-Log "============================================" "Cyan"
Write-Log ""

# Check what was created
$solutionExists = Test-Path "ComplaintManagementSystem.sln"
$domainExists = Test-Path "src/ComplaintManagement.Domain"
$applicationExists = Test-Path "src/ComplaintManagement.Application"
$infrastructureExists = Test-Path "src/ComplaintManagement.Infrastructure"
$sharedExists = Test-Path "src/ComplaintManagement.Shared"
$apiExists = Test-Path "src/ComplaintManagement.API"
$workerExists = Test-Path "src/ComplaintManagement.WorkerService"

if ($solutionExists) {
    Write-Log "✓ Solution file created" "Green"
} else {
    Write-ErrorLog "✗ Solution file NOT created"
}

if ($domainExists) {
    Write-Log "✓ Domain project created" "Green"
} else {
    Write-ErrorLog "✗ Domain project NOT created"
}

if ($applicationExists) {
    Write-Log "✓ Application project created" "Green"
} else {
    Write-ErrorLog "✗ Application project NOT created"
}

if ($infrastructureExists) {
    Write-Log "✓ Infrastructure project created" "Green"
} else {
    Write-ErrorLog "✗ Infrastructure project NOT created"
}

if ($sharedExists) {
    Write-Log "✓ Shared project created" "Green"
} else {
    Write-ErrorLog "✗ Shared project NOT created"
}

if ($apiExists) {
    Write-Log "✓ API project created" "Green"
} else {
    Write-ErrorLog "✗ API project NOT created"
}

if ($workerExists) {
    Write-Log "✓ Worker Service project created" "Green"
} else {
    Write-ErrorLog "✗ Worker Service project NOT created"
}

Write-Log ""
Write-Log "============================================" "Cyan"
Write-Log "  LOG FILES CREATED" "Cyan"
Write-Log "============================================" "Cyan"
Write-Log ""
Write-Log "Full log: $LogFile" "Yellow"
if (Test-Path $ErrorLogFile) {
    Write-Log "Errors only: $ErrorLogFile" "Red"
    Write-Log ""
    Write-Log "⚠️  THERE WERE ERRORS - Check $ErrorLogFile" "Red"
} else {
    Write-Log "No error log created (no errors occurred)" "Green"
}

Write-Log ""
Write-Log "============================================" "Cyan"
Write-Log "  NEXT STEPS" "Cyan"
Write-Log "============================================" "Cyan"
Write-Log ""

if (Test-Path $ErrorLogFile) {
    Write-Log "1. Share the file: $ErrorLogFile" "Yellow"
    Write-Log "2. Or run: type $ErrorLogFile" "Yellow"
} else {
    Write-Log "1. Run: dotnet build" "Yellow"
    Write-Log "2. If successful, notify me to continue!" "Yellow"
}

Write-Log ""
Write-Log "Setup script completed at: $(Get-Date)" "Cyan"
Write-Log ""
