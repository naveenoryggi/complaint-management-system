# CI/CD Setup Guide

This guide explains how to set up automatic builds and deployments for the Complaint Management System.

## Overview

The CI/CD pipeline:
1. **Triggers** on push to `main` or `clean-main` branches
2. **Builds** the .NET API and Angular frontend
3. **Creates** a Windows installer using Inno Setup
4. **Deploys** the installer to your server via network share
5. **(Optional)** Auto-installs the new version on the server

## Prerequisites

- GitHub repository with Actions enabled
- Windows Server (192.168.1.185) with:
  - IIS configured
  - Network share accessible
  - PowerShell remoting enabled (optional, for auto-install)

## Setup Steps

### Step 1: Configure GitHub Secrets

Go to your GitHub repository → Settings → Secrets and variables → Actions

Add these secrets:

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `SERVER_HOST` | Server hostname or IP | `192.168.1.185` |
| `SERVER_USER` | Windows username for share access | `Administrator` or `DOMAIN\Username` |
| `SERVER_PASSWORD` | Password for the user | `YourPassword` |
| `DEPLOY_SHARE` | UNC path to deploy share | `\\192.168.1.185\CMSDeploy` |

### Step 2: Set Up Deployment Share on Server

Run this on the **server** (192.168.1.185) as Administrator:

```powershell
# Copy the script to the server first, then run:
.\Setup-DeploymentShare.ps1

# Or with custom parameters:
.\Setup-DeploymentShare.ps1 -SharePath "D:\Deploy" -ShareName "CMSDeploy"
```

This creates:
- A folder for deployment files
- An SMB share accessible from GitHub Actions
- Proper permissions

### Step 3: (Optional) Enable Auto-Install

To automatically install new builds when they're deployed:

Run on the **server** as Administrator:

```powershell
.\Setup-AutoInstall.ps1

# Or with custom check interval (default: 5 minutes):
.\Setup-AutoInstall.ps1 -CheckIntervalMinutes 10
```

This creates a scheduled task that:
- Monitors the deploy folder every 5 minutes
- Detects new builds via `latest-build.json`
- Stops IIS, runs installer silently, starts IIS
- Logs all actions to `auto-install.log`

### Step 4: Test the Pipeline

1. Make a small change to the code
2. Commit and push to `main` branch
3. Go to GitHub → Actions to watch the build
4. Check the server's deploy folder for the installer

## How It Works

### Build Process (GitHub Actions)

```
Push to main
    ↓
Build .NET API (dotnet publish)
    ↓
Build Angular (ng build --production)
    ↓
Create Installer (Inno Setup)
    ↓
Upload Artifact
    ↓
Deploy to Server Share
```

### Deployment Process

```
Installer copied to \\server\CMSDeploy
    ↓
latest-build.json updated with version info
    ↓
(If auto-install enabled)
    ↓
Scheduled task detects new build
    ↓
Stops IIS → Runs installer → Starts IIS
```

## Manual Deployment

If you prefer manual installation:

1. Go to GitHub → Actions → Latest successful run
2. Download the `installer` artifact
3. Copy to server and run

Or from the deploy share:

```powershell
# On the server
cd C:\ComplaintManagement-Deploy
.\ComplaintManagementSetup-v1.4.XX.exe
```

## Troubleshooting

### Build Fails

- Check GitHub Actions logs for errors
- Ensure all dependencies are committed (package-lock.json)
- Verify .NET and Node.js versions

### Deploy Fails

- Verify GitHub secrets are correct
- Test network share access manually:
  ```powershell
  net use \\192.168.1.185\CMSDeploy /user:USERNAME PASSWORD
  ```
- Check Windows Firewall allows SMB (port 445)

### Auto-Install Not Working

- Check scheduled task is running: `Get-ScheduledTask -TaskName "ComplaintManagement-AutoInstall"`
- Review logs: `C:\ComplaintManagement-Deploy\auto-install.log`
- Ensure IIS site names match: `ComplaintManagementAPI`, `ComplaintManagementWeb`

## Version Numbering

The CI/CD automatically versions builds as: `1.4.{build_number}`

Where `{build_number}` is the GitHub Actions run number.

## Files Created

| File | Location | Purpose |
|------|----------|---------|
| `build-and-deploy.yml` | `.github/workflows/` | GitHub Actions workflow |
| `Setup-DeploymentShare.ps1` | `Scripts/` | Creates network share on server |
| `Setup-AutoInstall.ps1` | `Scripts/` | Sets up automatic installation |

## Security Notes

- Store credentials only in GitHub Secrets (encrypted)
- Use a dedicated service account for deployments
- Consider IP restrictions on the network share
- Review Windows Firewall rules
