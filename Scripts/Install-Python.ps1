# Install Python 3.12 for Tender Automation Backend
# This script downloads and installs Python embeddable package

param(
    [string]$InstallPath = "$env:ProgramFiles\ComplaintManagement\Python"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Installing Python 3.12 for Tender AI Backend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Python version
$pythonVersion = "3.12.0"
$pythonUrl = "https://www.python.org/ftp/python/3.12.0/python-3.12.0-embed-amd64.zip"
$pythonZip = "$env:TEMP\python-embed.zip"

try {
    # Create installation directory
    if (-not (Test-Path $InstallPath)) {
        New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
        Write-Host "✓ Created Python directory: $InstallPath" -ForegroundColor Green
    }

    # Download Python embeddable package
    Write-Host "`nDownloading Python 3.12 embeddable package..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri $pythonUrl -OutFile $pythonZip -UseBasicParsing
    Write-Host "✓ Downloaded Python package" -ForegroundColor Green

    # Extract Python
    Write-Host "`nExtracting Python..." -ForegroundColor Yellow
    Expand-Archive -Path $pythonZip -DestinationPath $InstallPath -Force
    Write-Host "✓ Extracted Python to $InstallPath" -ForegroundColor Green

    # Download get-pip.py
    Write-Host "`nDownloading pip installer..." -ForegroundColor Yellow
    $getPipUrl = "https://bootstrap.pypa.io/get-pip.py"
    $getPipPath = "$InstallPath\get-pip.py"
    Invoke-WebRequest -Uri $getPipUrl -OutFile $getPipPath -UseBasicParsing
    Write-Host "✓ Downloaded pip installer" -ForegroundColor Green

    # Enable site-packages in python312._pth
    Write-Host "`nConfiguring Python paths..." -ForegroundColor Yellow
    $pthFile = Get-ChildItem -Path $InstallPath -Filter "python*._pth" | Select-Object -First 1
    if ($pthFile) {
        $content = Get-Content $pthFile.FullName
        $content = $content -replace '^#import site', 'import site'
        $content | Set-Content $pthFile.FullName
        Write-Host "✓ Configured Python paths" -ForegroundColor Green
    }

    # Install pip
    Write-Host "`nInstalling pip..." -ForegroundColor Yellow
    $pythonExe = "$InstallPath\python.exe"
    & $pythonExe $getPipPath 2>&1 | Out-Null
    Write-Host "✓ Installed pip" -ForegroundColor Green

    # Add to PATH
    Write-Host "`nAdding Python to system PATH..." -ForegroundColor Yellow
    $currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
    if ($currentPath -notlike "*$InstallPath*") {
        [Environment]::SetEnvironmentVariable("Path", "$currentPath;$InstallPath", "Machine")
        $env:Path += ";$InstallPath"
        Write-Host "✓ Added to PATH" -ForegroundColor Green
    } else {
        Write-Host "✓ Already in PATH" -ForegroundColor Green
    }

    # Cleanup
    Remove-Item $pythonZip -Force -ErrorAction SilentlyContinue

    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Python Installation Complete!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Python installed at: $InstallPath" -ForegroundColor Cyan

    return $true

} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "Python Installation Failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    return $false
}
