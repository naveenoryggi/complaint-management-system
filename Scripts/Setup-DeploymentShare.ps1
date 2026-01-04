# Setup-DeploymentShare.ps1
# Run this script ON THE SERVER (192.168.1.185) to create the deployment share
# Run as Administrator

param(
    [string]$SharePath = "C:\ComplaintManagement-Deploy",
    [string]$ShareName = "CMSDeploy",
    [string]$AllowedUser = "DOMAIN\DeployUser"  # Change this to your deployment user
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setting up Deployment Share for CI/CD" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Create the folder
if (-not (Test-Path $SharePath)) {
    Write-Host "Creating folder: $SharePath"
    New-Item -ItemType Directory -Path $SharePath -Force | Out-Null
}

# Check if share already exists
$existingShare = Get-SmbShare -Name $ShareName -ErrorAction SilentlyContinue
if ($existingShare) {
    Write-Host "Share '$ShareName' already exists. Removing and recreating..."
    Remove-SmbShare -Name $ShareName -Force
}

# Create the share
Write-Host "Creating SMB share: \\$env:COMPUTERNAME\$ShareName"
New-SmbShare -Name $ShareName -Path $SharePath -FullAccess "Administrators" -ChangeAccess "Everyone"

# Set NTFS permissions
Write-Host "Setting NTFS permissions..."
$acl = Get-Acl $SharePath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("Everyone", "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl -Path $SharePath -AclObject $acl

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Deployment Share Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Share Path: \\$env:COMPUTERNAME\$ShareName" -ForegroundColor Yellow
Write-Host "Local Path: $SharePath" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Add these secrets to your GitHub repository:"
Write-Host "   - SERVER_HOST: $env:COMPUTERNAME (or IP address)"
Write-Host "   - SERVER_USER: Your Windows username"
Write-Host "   - SERVER_PASSWORD: Your Windows password"
Write-Host "   - DEPLOY_SHARE: \\$env:COMPUTERNAME\$ShareName"
Write-Host ""
Write-Host "2. (Optional) Run Setup-AutoInstall.ps1 to enable automatic installation"
