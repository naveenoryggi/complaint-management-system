#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Builds the Complaint Management System installer with GUI
.DESCRIPTION
    This script:
    1. Builds the .NET API
    2. Builds the Angular frontend
    3. Creates the Inno Setup installer (if Inno Setup is installed)
    4. OR builds the WinForms installer
#>

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Complaint Management System - Installer Builder         ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Check for Inno Setup
$innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$hasInnoSetup = Test-Path $innoSetupPath

if ($hasInnoSetup) {
    Write-Host "[INFO] Inno Setup detected - Will create professional GUI installer" -ForegroundColor Green
} else {
    Write-Host "[INFO] Inno Setup not found - Will build WinForms installer" -ForegroundColor Yellow
    Write-Host "[TIP] For best results, download Inno Setup from: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Step 1: Building .NET API" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan

$apiPath = Join-Path $PSScriptRoot "complaint-system-dotnet\src\ComplaintManagement.API"
Push-Location $apiPath

Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path "bin") { Remove-Item "bin" -Recurse -Force }
if (Test-Path "obj") { Remove-Item "obj" -Recurse -Force }

Write-Host "Publishing API..." -ForegroundColor Yellow
dotnet publish -c Release -o "bin\Release\net8.0\publish" --self-contained false

if ($LASTEXITCODE -eq 0) {
    Write-Host "[SUCCESS] API built successfully" -ForegroundColor Green
} else {
    Write-Host "[ERROR] API build failed" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Step 2: Building Angular Frontend" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan

$angularPath = Join-Path $PSScriptRoot "complaint-system-angular"
Push-Location $angularPath

Write-Host "Installing dependencies (if needed)..." -ForegroundColor Yellow
if (-not (Test-Path "node_modules")) {
    npm install
}

Write-Host "Building frontend for production..." -ForegroundColor Yellow
npm run build --prod

if ($LASTEXITCODE -eq 0) {
    Write-Host "[SUCCESS] Frontend built successfully" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Frontend build failed" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Step 3: Creating Installer" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan

if ($hasInnoSetup) {
    Write-Host "Compiling Inno Setup installer..." -ForegroundColor Yellow

    $issFile = Join-Path $PSScriptRoot "ComplaintManagementSetup.iss"
    & $innoSetupPath $issFile

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[SUCCESS] Installer created successfully!" -ForegroundColor Green

        $installerPath = Join-Path $PSScriptRoot "installer-output\ComplaintManagementSetup-v1.0.0.exe"
        if (Test-Path $installerPath) {
            $size = [math]::Round((Get-Item $installerPath).Length / 1MB, 2)
            Write-Host ""
            Write-Host "═══════════════════════════════════════════" -ForegroundColor Green
            Write-Host "  INSTALLER READY FOR DISTRIBUTION" -ForegroundColor Green
            Write-Host "═══════════════════════════════════════════" -ForegroundColor Green
            Write-Host "  Location: $installerPath" -ForegroundColor White
            Write-Host "  Size: $size MB" -ForegroundColor White
            Write-Host ""
            Write-Host "  This is a professional GUI installer that:" -ForegroundColor Cyan
            Write-Host "  ✓ Has a beautiful wizard interface" -ForegroundColor White
            Write-Host "  ✓ Includes database configuration screen" -ForegroundColor White
            Write-Host "  ✓ Shows installation progress" -ForegroundColor White
            Write-Host "  ✓ Creates start menu shortcuts" -ForegroundColor White
            Write-Host "  ✓ Includes automatic uninstaller" -ForegroundColor White
            Write-Host "  ✓ Integrates with Windows properly" -ForegroundColor White
            Write-Host ""
            Write-Host "  Send this file to your customers!" -ForegroundColor Yellow
            Write-Host "═══════════════════════════════════════════" -ForegroundColor Green

            # Ask to open folder
            $openFolder = Read-Host "`nWould you like to open the installer folder? (Y/N)"
            if ($openFolder -eq "Y" -or $openFolder -eq "y") {
                explorer.exe (Join-Path $PSScriptRoot "installer-output")
            }
        }
    } else {
        Write-Host "[ERROR] Inno Setup compilation failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Building WinForms installer..." -ForegroundColor Yellow

    $installerProject = Join-Path $PSScriptRoot "installer"
    Push-Location $installerProject

    dotnet publish -c Release -r win-x64 --self-contained

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[SUCCESS] WinForms installer built successfully!" -ForegroundColor Green

        $installerExe = Join-Path $installerProject "bin\Release\net8.0-windows\win-x64\publish\ComplaintManagement.Installer.exe"
        if (Test-Path $installerExe) {
            Write-Host ""
            Write-Host "═══════════════════════════════════════════" -ForegroundColor Green
            Write-Host "  INSTALLER READY" -ForegroundColor Green
            Write-Host "═══════════════════════════════════════════" -ForegroundColor Green
            Write-Host "  Location: $installerExe" -ForegroundColor White
            Write-Host ""
            Write-Host "  NOTE: For a more professional installer," -ForegroundColor Yellow
            Write-Host "  install Inno Setup from:" -ForegroundColor Yellow
            Write-Host "  https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
            Write-Host "═══════════════════════════════════════════" -ForegroundColor Green
        }
    } else {
        Write-Host "[ERROR] WinForms installer build failed" -ForegroundColor Red
    }

    Pop-Location
}

Write-Host ""
Write-Host "Build process complete!" -ForegroundColor Green
Write-Host ""
