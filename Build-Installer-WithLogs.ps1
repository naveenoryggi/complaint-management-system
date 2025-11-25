#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Builds the Complaint Management System installer with detailed logging
#>

$ErrorActionPreference = "Continue"
$LogFile = Join-Path $PSScriptRoot "installer-build-log.txt"

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    Write-Host $logMessage
    Add-Content -Path $LogFile -Value $logMessage
}

# Clear previous log
if (Test-Path $LogFile) { Remove-Item $LogFile }

Write-Log "==================================================" "INFO"
Write-Log "Complaint Management System - Installer Builder" "INFO"
Write-Log "==================================================" "INFO"

# Check for Inno Setup
$innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$hasInnoSetup = Test-Path $innoSetupPath

Write-Log "Checking for Inno Setup at: $innoSetupPath" "INFO"
if ($hasInnoSetup) {
    Write-Log "Inno Setup found!" "SUCCESS"
} else {
    Write-Log "Inno Setup NOT found" "WARNING"
    Write-Log "You can download it from: https://jrsoftware.org/isdl.php" "INFO"
    Write-Log "Will attempt to build WinForms installer instead" "INFO"
}

Write-Log "" "INFO"
Write-Log "Step 1: Building .NET API" "INFO"
Write-Log "=========================" "INFO"

$apiPath = Join-Path $PSScriptRoot "complaint-system-dotnet\src\ComplaintManagement.API"
Write-Log "API Path: $apiPath" "INFO"

if (-not (Test-Path $apiPath)) {
    Write-Log "ERROR: API path not found!" "ERROR"
    Write-Log "Expected: $apiPath" "ERROR"
    exit 1
}

Push-Location $apiPath
try {
    Write-Log "Cleaning previous builds..." "INFO"
    if (Test-Path "bin") {
        Remove-Item "bin" -Recurse -Force -ErrorAction SilentlyContinue
        Write-Log "Removed bin folder" "INFO"
    }
    if (Test-Path "obj") {
        Remove-Item "obj" -Recurse -Force -ErrorAction SilentlyContinue
        Write-Log "Removed obj folder" "INFO"
    }

    Write-Log "Running: dotnet publish -c Release" "INFO"
    $output = dotnet publish -c Release -o "bin\Release\net8.0\publish" --self-contained false 2>&1

    Write-Log "Publish output:" "INFO"
    $output | ForEach-Object { Write-Log $_ "INFO" }

    if ($LASTEXITCODE -eq 0) {
        Write-Log "API published successfully!" "SUCCESS"

        # Check if files exist
        $publishPath = Join-Path $apiPath "bin\Release\net8.0\publish"
        if (Test-Path $publishPath) {
            $fileCount = (Get-ChildItem $publishPath -Recurse).Count
            Write-Log "Published $fileCount files to: $publishPath" "SUCCESS"
        }
    } else {
        Write-Log "API publish FAILED with exit code: $LASTEXITCODE" "ERROR"
        Pop-Location
        exit 1
    }
} catch {
    Write-Log "Exception during API build: $($_.Exception.Message)" "ERROR"
    Write-Log "Stack trace: $($_.ScriptStackTrace)" "ERROR"
    Pop-Location
    exit 1
}
Pop-Location

Write-Log "" "INFO"
Write-Log "Step 2: Building Angular Frontend" "INFO"
Write-Log "==================================" "INFO"

$angularPath = Join-Path $PSScriptRoot "complaint-system-angular"
Write-Log "Angular Path: $angularPath" "INFO"

if (-not (Test-Path $angularPath)) {
    Write-Log "ERROR: Angular path not found!" "ERROR"
    Write-Log "Expected: $angularPath" "ERROR"
    exit 1
}

Push-Location $angularPath
try {
    # Check if node_modules exists
    if (-not (Test-Path "node_modules")) {
        Write-Log "node_modules not found, running npm install..." "INFO"
        $output = npm install 2>&1
        $output | ForEach-Object { Write-Log $_ "INFO" }

        if ($LASTEXITCODE -ne 0) {
            Write-Log "npm install FAILED with exit code: $LASTEXITCODE" "ERROR"
            Pop-Location
            exit 1
        }
    } else {
        Write-Log "node_modules folder exists" "INFO"
    }

    Write-Log "Running: npm run build" "INFO"
    $output = npm run build 2>&1
    $output | ForEach-Object { Write-Log $_ "INFO" }

    if ($LASTEXITCODE -eq 0) {
        Write-Log "Angular build successful!" "SUCCESS"

        # Check dist folder
        $distPath = Join-Path $angularPath "dist\complaint-system-angular\browser"
        if (Test-Path $distPath) {
            $fileCount = (Get-ChildItem $distPath -Recurse).Count
            Write-Log "Built $fileCount files to: $distPath" "SUCCESS"
        } else {
            # Try alternate path
            $distPath = Join-Path $angularPath "dist\complaint-system-angular"
            if (Test-Path $distPath) {
                $fileCount = (Get-ChildItem $distPath -Recurse).Count
                Write-Log "Built $fileCount files to: $distPath" "SUCCESS"
            }
        }
    } else {
        Write-Log "Angular build FAILED with exit code: $LASTEXITCODE" "ERROR"
        Pop-Location
        exit 1
    }
} catch {
    Write-Log "Exception during Angular build: $($_.Exception.Message)" "ERROR"
    Write-Log "Stack trace: $($_.ScriptStackTrace)" "ERROR"
    Pop-Location
    exit 1
}
Pop-Location

Write-Log "" "INFO"
Write-Log "Step 3: Creating Installer" "INFO"
Write-Log "==========================" "INFO"

if ($hasInnoSetup) {
    Write-Log "Using Inno Setup to create installer..." "INFO"

    $issFile = Join-Path $PSScriptRoot "ComplaintManagementSetup.iss"
    Write-Log "ISS File: $issFile" "INFO"

    if (-not (Test-Path $issFile)) {
        Write-Log "ERROR: Inno Setup script not found!" "ERROR"
        Write-Log "Expected: $issFile" "ERROR"
        exit 1
    }

    try {
        Write-Log "Running: $innoSetupPath $issFile" "INFO"
        $output = & $innoSetupPath $issFile 2>&1
        $output | ForEach-Object { Write-Log $_ "INFO" }

        if ($LASTEXITCODE -eq 0) {
            Write-Log "Installer compilation successful!" "SUCCESS"

            $installerPath = Join-Path $PSScriptRoot "installer-output\ComplaintManagementSetup-v1.0.0.exe"
            if (Test-Path $installerPath) {
                $size = [math]::Round((Get-Item $installerPath).Length / 1MB, 2)
                Write-Log "" "INFO"
                Write-Log "========================================" "SUCCESS"
                Write-Log "INSTALLER CREATED SUCCESSFULLY!" "SUCCESS"
                Write-Log "========================================" "SUCCESS"
                Write-Log "Location: $installerPath" "SUCCESS"
                Write-Log "Size: $size MB" "SUCCESS"
                Write-Log "" "INFO"

                # Open folder
                explorer.exe (Join-Path $PSScriptRoot "installer-output")
            } else {
                Write-Log "WARNING: Installer exe not found at expected location" "WARNING"
                Write-Log "Expected: $installerPath" "WARNING"
            }
        } else {
            Write-Log "Installer compilation FAILED with exit code: $LASTEXITCODE" "ERROR"
            exit 1
        }
    } catch {
        Write-Log "Exception during Inno Setup compilation: $($_.Exception.Message)" "ERROR"
        Write-Log "Stack trace: $($_.ScriptStackTrace)" "ERROR"
        exit 1
    }
} else {
    Write-Log "Building WinForms installer..." "INFO"

    $installerProject = Join-Path $PSScriptRoot "installer"
    Write-Log "Installer project path: $installerProject" "INFO"

    if (-not (Test-Path $installerProject)) {
        Write-Log "ERROR: Installer project not found!" "ERROR"
        Write-Log "Expected: $installerProject" "ERROR"
        Write-Log "Please download Inno Setup instead: https://jrsoftware.org/isdl.php" "INFO"
        exit 1
    }

    Push-Location $installerProject
    try {
        Write-Log "Running: dotnet publish -c Release -r win-x64 --self-contained" "INFO"
        $output = dotnet publish -c Release -r win-x64 --self-contained 2>&1
        $output | ForEach-Object { Write-Log $_ "INFO" }

        if ($LASTEXITCODE -eq 0) {
            Write-Log "WinForms installer built successfully!" "SUCCESS"

            $installerExe = Join-Path $installerProject "bin\Release\net8.0-windows\win-x64\publish\ComplaintManagement.Installer.exe"
            if (Test-Path $installerExe) {
                $size = [math]::Round((Get-Item $installerExe).Length / 1MB, 2)
                Write-Log "" "INFO"
                Write-Log "========================================" "SUCCESS"
                Write-Log "INSTALLER CREATED!" "SUCCESS"
                Write-Log "========================================" "SUCCESS"
                Write-Log "Location: $installerExe" "SUCCESS"
                Write-Log "Size: $size MB" "SUCCESS"
                Write-Log "" "INFO"
                Write-Log "NOTE: For a more professional installer, install Inno Setup" "INFO"
                Write-Log "Download from: https://jrsoftware.org/isdl.php" "INFO"

                # Open folder
                explorer.exe (Split-Path $installerExe)
            }
        } else {
            Write-Log "WinForms installer build FAILED with exit code: $LASTEXITCODE" "ERROR"
        }
    } catch {
        Write-Log "Exception during WinForms build: $($_.Exception.Message)" "ERROR"
        Write-Log "Stack trace: $($_.ScriptStackTrace)" "ERROR"
    }
    Pop-Location
}

Write-Log "" "INFO"
Write-Log "Build process complete!" "SUCCESS"
Write-Log "Log file saved to: $LogFile" "INFO"

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
