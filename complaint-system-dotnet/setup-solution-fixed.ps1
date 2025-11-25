# PowerShell script to create .NET 8 solution structure with detailed logging
# Run this with: powershell -ExecutionPolicy Bypass -File setup-solution-fixed.ps1

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
Write-Log "Step 1: Creating solution..." "Cyan"
dotnet new sln -n ComplaintManagementSystem 2>&1 | Out-File -Append $LogFile
if ($LASTEXITCODE -eq 0) {
    Write-Log "✓ Solution created successfully" "Green"
} else {
    Write-ErrorLog "Failed to create solution. Exit code: $LASTEXITCODE"
}

Write-Log ""

# Create src directory
if (-not (Test-Path "src")) {
    New-Item -ItemType Directory -Force -Path "src" | Out-Null
    Write-Log "✓ Created src directory" "Green"
}

# Step 2: Create Domain project
Write-Log "Step 2: Creating Domain project..." "Cyan"
dotnet new classlib -n ComplaintManagement.Domain -o src/ComplaintManagement.Domain -f net8.0 2>&1 | Out-File -Append $LogFile
if ($LASTEXITCODE -eq 0) {
    Write-Log "✓ Domain project created" "Green"
    dotnet sln add src/ComplaintManagement.Domain/ComplaintManagement.Domain.csproj 2>&1 | Out-Null
    Write-Log "✓ Domain project added to solution" "Green"
} else {
    Write-ErrorLog "Failed to create Domain project"
}

Write-Log ""

# Step 3: Create Application project
Write-Log "Step 3: Creating Application project..." "Cyan"
dotnet new classlib -n ComplaintManagement.Application -o src/ComplaintManagement.Application -f net8.0 2>&1 | Out-File -Append $LogFile
if ($LASTEXITCODE -eq 0) {
    Write-Log "✓ Application project created" "Green"
    dotnet sln add src/ComplaintManagement.Application/ComplaintManagement.Application.csproj 2>&1 | Out-Null
    dotnet add src/ComplaintManagement.Application reference src/ComplaintManagement.Domain 2>&1 | Out-Null
    Write-Log "✓ Application project configured" "Green"
} else {
    Write-ErrorLog "Failed to create Application project"
}

Write-Log ""

# Step 4: Create Infrastructure project
Write-Log "Step 4: Creating Infrastructure project..." "Cyan"
dotnet new classlib -n ComplaintManagement.Infrastructure -o src/ComplaintManagement.Infrastructure -f net8.0 2>&1 | Out-File -Append $LogFile
if ($LASTEXITCODE -eq 0) {
    Write-Log "✓ Infrastructure project created" "Green"
    dotnet sln add src/ComplaintManagement.Infrastructure/ComplaintManagement.Infrastructure.csproj 2>&1 | Out-Null
    dotnet add src/ComplaintManagement.Infrastructure reference src/ComplaintManagement.Domain 2>&1 | Out-Null
    dotnet add src/ComplaintManagement.Infrastructure reference src/ComplaintManagement.Application 2>&1 | Out-Null
    Write-Log "✓ Infrastructure project configured" "Green"
} else {
    Write-ErrorLog "Failed to create Infrastructure project"
}

Write-Log ""

# Step 5: Create Shared project
Write-Log "Step 5: Creating Shared project..." "Cyan"
dotnet new classlib -n ComplaintManagement.Shared -o src/ComplaintManagement.Shared -f net8.0 2>&1 | Out-File -Append $LogFile
if ($LASTEXITCODE -eq 0) {
    Write-Log "✓ Shared project created" "Green"
    dotnet sln add src/ComplaintManagement.Shared/ComplaintManagement.Shared.csproj 2>&1 | Out-Null
    Write-Log "✓ Shared project added to solution" "Green"
} else {
    Write-ErrorLog "Failed to create Shared project"
}

Write-Log ""

# Step 6: Create Web API project
Write-Log "Step 6: Creating Web API project..." "Cyan"
dotnet new webapi -n ComplaintManagement.API -o src/ComplaintManagement.API -f net8.0 2>&1 | Out-File -Append $LogFile
if ($LASTEXITCODE -eq 0) {
    Write-Log "✓ Web API project created" "Green"
    dotnet sln add src/ComplaintManagement.API/ComplaintManagement.API.csproj 2>&1 | Out-Null
    dotnet add src/ComplaintManagement.API reference src/ComplaintManagement.Application 2>&1 | Out-Null
    dotnet add src/ComplaintManagement.API reference src/ComplaintManagement.Infrastructure 2>&1 | Out-Null
    dotnet add src/ComplaintManagement.API reference src/ComplaintManagement.Shared 2>&1 | Out-Null
    Write-Log "✓ Web API project configured" "Green"
} else {
    Write-ErrorLog "Failed to create Web API project"
}

Write-Log ""

# Step 7: Create Worker Service project
Write-Log "Step 7: Creating Worker Service project..." "Cyan"
dotnet new worker -n ComplaintManagement.WorkerService -o src/ComplaintManagement.WorkerService -f net8.0 2>&1 | Out-File -Append $LogFile
if ($LASTEXITCODE -eq 0) {
    Write-Log "✓ Worker Service project created" "Green"
    dotnet sln add src/ComplaintManagement.WorkerService/ComplaintManagement.WorkerService.csproj 2>&1 | Out-Null
    dotnet add src/ComplaintManagement.WorkerService reference src/ComplaintManagement.Application 2>&1 | Out-Null
    dotnet add src/ComplaintManagement.WorkerService reference src/ComplaintManagement.Infrastructure 2>&1 | Out-Null
    dotnet add src/ComplaintManagement.WorkerService reference src/ComplaintManagement.Shared 2>&1 | Out-Null
    Write-Log "✓ Worker Service project configured" "Green"
} else {
    Write-ErrorLog "Failed to create Worker Service project"
}

Write-Log ""
Write-Log "Step 8: Installing NuGet packages..." "Cyan"
Write-Log "This may take a few minutes..." "Yellow"
Write-Log ""

# Application packages
Write-Log "Installing Application packages..." "Yellow"
dotnet add src/ComplaintManagement.Application package AutoMapper 2>&1 | Out-Null
dotnet add src/ComplaintManagement.Application package AutoMapper.Extensions.Microsoft.DependencyInjection 2>&1 | Out-Null
dotnet add src/ComplaintManagement.Application package FluentValidation 2>&1 | Out-Null
dotnet add src/ComplaintManagement.Application package FluentValidation.DependencyInjectionExtensions 2>&1 | Out-Null
dotnet add src/ComplaintManagement.Application package MediatR 2>&1 | Out-Null
Write-Log "✓ Application packages installed" "Green"

Write-Log ""

# Infrastructure packages
Write-Log "Installing Infrastructure packages..." "Yellow"
dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore 2>&1 | Out-Null
dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer 2>&1 | Out-Null
dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore.Tools 2>&1 | Out-Null
dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore.Design 2>&1 | Out-Null
dotnet add src/ComplaintManagement.Infrastructure package Dapper 2>&1 | Out-Null
Write-Log "✓ Infrastructure packages installed" "Green"

Write-Log ""

# API packages
Write-Log "Installing API packages..." "Yellow"
dotnet add src/ComplaintManagement.API package Microsoft.AspNetCore.Authentication.JwtBearer 2>&1 | Out-Null
dotnet add src/ComplaintManagement.API package Swashbuckle.AspNetCore 2>&1 | Out-Null
dotnet add src/ComplaintManagement.API package Serilog.AspNetCore 2>&1 | Out-Null
dotnet add src/ComplaintManagement.API package BCrypt.Net-Next 2>&1 | Out-Null
Write-Log "✓ API packages installed" "Green"

Write-Log ""

# Worker Service packages
Write-Log "Installing Worker Service packages..." "Yellow"
dotnet add src/ComplaintManagement.WorkerService package Serilog.Extensions.Hosting 2>&1 | Out-Null
Write-Log "✓ Worker Service packages installed" "Green"

Write-Log ""
Write-Log "Step 9: Building solution..." "Cyan"
dotnet build 2>&1 | Out-File -Append $LogFile
if ($LASTEXITCODE -eq 0) {
    Write-Log "✓ Solution built successfully" "Green"
} else {
    Write-ErrorLog "Build failed. Exit code: $LASTEXITCODE"
}

Write-Log ""
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

if ($solutionExists) { Write-Log "✓ Solution file created" "Green" } else { Write-ErrorLog "✗ Solution file NOT created" }
if ($domainExists) { Write-Log "✓ Domain project created" "Green" } else { Write-ErrorLog "✗ Domain project NOT created" }
if ($applicationExists) { Write-Log "✓ Application project created" "Green" } else { Write-ErrorLog "✗ Application project NOT created" }
if ($infrastructureExists) { Write-Log "✓ Infrastructure project created" "Green" } else { Write-ErrorLog "✗ Infrastructure project NOT created" }
if ($sharedExists) { Write-Log "✓ Shared project created" "Green" } else { Write-ErrorLog "✗ Shared project NOT created" }
if ($apiExists) { Write-Log "✓ API project created" "Green" } else { Write-ErrorLog "✗ API project NOT created" }
if ($workerExists) { Write-Log "✓ Worker Service project created" "Green" } else { Write-ErrorLog "✗ Worker Service project NOT created" }

Write-Log ""
Write-Log "============================================" "Cyan"
Write-Log "  LOG FILES CREATED" "Cyan"
Write-Log "============================================" "Cyan"
Write-Log ""
Write-Log "Full log: $LogFile" "Yellow"

if (Test-Path $ErrorLogFile) {
    $errorContent = Get-Content $ErrorLogFile
    if ($errorContent) {
        Write-Log "Errors only: $ErrorLogFile" "Red"
        Write-Log ""
        Write-Log "WARNING: THERE WERE ERRORS - Check $ErrorLogFile" "Red"
    }
} else {
    Write-Log "No errors occurred!" "Green"
}

Write-Log ""
Write-Log "Setup script completed at: $(Get-Date)" "Cyan"
Write-Log ""
