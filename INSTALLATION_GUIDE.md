# Complaint Management System - Installation Guide

## Table of Contents
- [System Requirements](#system-requirements)
- [Pre-Installation](#pre-installation)
- [Installation Steps](#installation-steps)
- [Post-Installation](#post-installation)
- [Troubleshooting](#troubleshooting)
- [Uninstallation](#uninstallation)

---

## System Requirements

### Hardware Requirements
- **Processor**: 2 GHz or faster (4 cores recommended)
- **RAM**: Minimum 4 GB (8 GB recommended)
- **Disk Space**: Minimum 2 GB free space
- **Network**: Internet connection for initial setup

### Software Requirements
- **Operating System**: Windows Server 2016/2019/2022 or Windows 10/11 (64-bit)
- **.NET Runtime**: .NET 8.0 Runtime (ASP.NET Core)
- **IIS**: Internet Information Services 10.0 or later
- **SQL Server**: SQL Server 2016 or later (Express, Standard, or Enterprise)
- **Browser**: Modern web browser (Chrome, Edge, Firefox)

---

## Pre-Installation

### 1. Install .NET 8 Runtime

Download and install the .NET 8 ASP.NET Core Runtime:
1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download "ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle"
3. Run the installer
4. Restart your computer if prompted

### 2. Enable IIS

**Windows Server:**
```powershell
Install-WindowsFeature -Name Web-Server -IncludeManagementTools
```

**Windows 10/11:**
1. Open "Turn Windows features on or off"
2. Enable "Internet Information Services"
3. Enable "Internet Information Services → World Wide Web Services"
4. Enable "Internet Information Services → Web Management Tools → IIS Management Console"
5. Click OK and wait for installation

### 3. Install/Configure SQL Server

**Option A: SQL Server Express (Free)**
1. Download SQL Server Express from: https://www.microsoft.com/sql-server/sql-server-downloads
2. Run installer and choose "Basic" installation
3. Note the server name (usually `localhost\SQLEXPRESS`)

**Option B: Existing SQL Server**
- Ensure SQL Server is running
- Note your server name/IP
- Prepare login credentials (Windows Auth or SQL Auth)

### 4. Configure SQL Server for Remote Connections (if needed)

1. Open SQL Server Configuration Manager
2. Navigate to "SQL Server Network Configuration → Protocols for [Instance]"
3. Enable "TCP/IP"
4. Restart SQL Server service

---

## Installation Steps

### Quick Start

1. **Extract Installation Package**
   ```
   Extract the Complaint Management System ZIP file to a temporary location
   ```

2. **Run Installer as Administrator**
   ```powershell
   Right-click "Install-ComplaintManagementSystem.ps1" → "Run with PowerShell"
   ```

3. **Follow Installation Wizard**

### Detailed Installation Process

#### Step 1: Launch Installer

```powershell
# Navigate to extracted folder
cd "C:\Temp\ComplaintManagementSystem"

# Run as Administrator
PowerShell.exe -ExecutionPolicy Bypass -File .\Install-ComplaintManagementSystem.ps1
```

#### Step 2: Prerequisites Check

The installer will automatically check for:
- .NET 8 Runtime
- IIS Installation
- SQL Server Tools

If any prerequisites are missing, install them and restart the installer.

#### Step 3: Database Configuration

When prompted, enter your SQL Server details:

**Example 1: SQL Server with Windows Authentication**
```
SQL Server Name/IP: localhost
Database Name: ComplaintManagementDB
Authentication Type: 1 (Windows Authentication)
```

**Example 2: SQL Server with SQL Authentication**
```
SQL Server Name/IP: 192.168.1.100
Database Name: ComplaintManagementDB
Authentication Type: 2 (SQL Server Authentication)
SQL Server Login ID: sa
SQL Server Password: YourPassword123!
```

**Example 3: SQL Server Express**
```
SQL Server Name/IP: localhost\SQLEXPRESS
Database Name: ComplaintManagementDB
Authentication Type: 1 (Windows Authentication)
```

#### Step 4: Connection Test

The installer will test the database connection. If successful, it will proceed.

If connection fails:
- Verify SQL Server is running
- Check firewall settings
- Verify credentials
- Ensure TCP/IP is enabled in SQL Server Configuration Manager

#### Step 5: Confirm Installation

Review the installation summary and confirm to proceed.

#### Step 6: Automated Installation

The installer will:
1. ✅ Build the .NET API
2. ✅ Build the Angular frontend
3. ✅ Create database and run migrations
4. ✅ Install Windows Service for API
5. ✅ Configure IIS website
6. ✅ Start all services

#### Step 7: Complete

Upon successful installation, you'll see:
- Frontend URL: http://localhost
- API URL: http://localhost:5000
- Default admin credentials

Press any key to automatically open the application in your browser.

---

## Post-Installation

### Access the Application

1. **Open Web Browser**
   ```
   Navigate to: http://localhost
   ```

2. **Login with Default Credentials**
   ```
   Username: admin@complaintmanagement.com
   Password: Admin@123
   ```

3. **Change Default Password**
   - Go to Profile → Change Password
   - Set a strong password

### Configure Email Settings

1. Navigate to **Admin Panel → Communication Settings → Email Settings**
2. Click "Add Email Server"
3. Configure your SMTP server (Gmail, Office 365, or custom)
4. Test the connection

### Configure Email Ticketing (Optional)

1. Navigate to **Admin Panel → Communication Settings → Email Ticketing**
2. Click "Add Email Configuration"
3. Configure IMAP and SMTP settings
4. Authorize OAuth (if using Office 365 or Gmail)

### Create Additional Users

1. Navigate to **Admin Panel → User Management**
2. Click "Add User"
3. Assign appropriate roles (Admin, Technician, User)

### Customize Company Settings

1. Navigate to **Admin Panel → System Settings**
2. Update company information
3. Configure defaults for departments, priorities, etc.

---

## Service Management

### Windows Service Control

**Check Service Status:**
```powershell
sc query ComplaintManagementAPI
```

**Start Service:**
```powershell
net start ComplaintManagementAPI
```

**Stop Service:**
```powershell
net stop ComplaintManagementAPI
```

**Restart Service:**
```powershell
net stop ComplaintManagementAPI
net start ComplaintManagementAPI
```

### View Service Logs

Logs are stored in:
```
C:\Program Files\ComplaintManagement\API\Logs\
```

### IIS Management

**Open IIS Manager:**
```
Start → Run → inetmgr
```

**Restart Website:**
1. Open IIS Manager
2. Navigate to "Sites → ComplaintManagement"
3. Right-click → Manage Website → Restart

---

## Troubleshooting

### Issue: Cannot Access Website

**Check IIS Status:**
```powershell
Get-Website -Name ComplaintManagement
```

**Check if port 80 is in use:**
```powershell
netstat -ano | findstr :80
```

**Solution:**
- Ensure IIS website is started
- Check Windows Firewall
- Verify no other application is using port 80

### Issue: API Service Won't Start

**Check Service Status:**
```powershell
Get-Service ComplaintManagementAPI
```

**View Event Logs:**
```powershell
Get-EventLog -LogName Application -Source ComplaintManagementAPI -Newest 10
```

**Solutions:**
- Check database connection string in `appsettings.json`
- Verify SQL Server is running
- Review log files in `C:\Program Files\ComplaintManagement\API\Logs\`

### Issue: Database Connection Failed

**Test Connection Manually:**
```powershell
sqlcmd -S localhost -d ComplaintManagementDB -E
```

**Solutions:**
- Verify SQL Server service is running
- Check SQL Server Configuration Manager → TCP/IP enabled
- Test credentials
- Verify firewall allows SQL Server port (default 1433)

### Issue: 500 Internal Server Error

**Check API Logs:**
```
C:\Program Files\ComplaintManagement\API\Logs\complaint-api-YYYYMMDD.txt
```

**Common Causes:**
- Database connection issues
- Missing migrations
- Invalid configuration in appsettings.json

### Issue: Email Not Sending

**Check Email Server Settings:**
1. Navigate to Email Settings
2. Click "Test Connection" on configured server
3. Review error message

**Common Solutions:**
- Verify SMTP credentials
- Check if using App-Specific Password (Gmail)
- Verify OAuth authorization (Office 365/Gmail)
- Check firewall allows SMTP port (587/465)

---

## Updating the Application

### Manual Update Process

1. **Stop Services:**
   ```powershell
   net stop ComplaintManagementAPI
   Stop-Website -Name ComplaintManagement
   ```

2. **Backup Database:**
   ```sql
   BACKUP DATABASE ComplaintManagementDB
   TO DISK = 'C:\Backups\ComplaintDB_Backup.bak'
   ```

3. **Replace Files:**
   - Backup `C:\Program Files\ComplaintManagement\`
   - Copy new files to installation directory

4. **Run Migrations:**
   ```powershell
   cd "C:\Program Files\ComplaintManagement\API"
   dotnet ef database update
   ```

5. **Start Services:**
   ```powershell
   net start ComplaintManagementAPI
   Start-Website -Name ComplaintManagement
   ```

---

## Uninstallation

### Using Uninstaller Script

```powershell
# Run as Administrator
PowerShell.exe -ExecutionPolicy Bypass -File .\Uninstall-ComplaintManagementSystem.ps1
```

The uninstaller will:
- Stop and remove Windows Service
- Remove IIS website and application pool
- Delete installation files
- Optionally remove database

### Manual Uninstallation

1. **Stop and Remove Service:**
   ```powershell
   net stop ComplaintManagementAPI
   sc delete ComplaintManagementAPI
   ```

2. **Remove IIS Site:**
   ```powershell
   Remove-Website -Name ComplaintManagement
   Remove-WebAppPool -Name ComplaintManagement
   ```

3. **Delete Files:**
   ```powershell
   Remove-Item "C:\Program Files\ComplaintManagement" -Recurse -Force
   ```

4. **Drop Database (Optional):**
   ```sql
   USE master;
   DROP DATABASE ComplaintManagementDB;
   ```

---

## Security Best Practices

### 1. Change Default Passwords
- Change admin password immediately after installation
- Use strong passwords (minimum 12 characters, mixed case, numbers, symbols)

### 2. Configure HTTPS
```powershell
# Bind SSL certificate to IIS site
New-WebBinding -Name "ComplaintManagement" -Protocol https -Port 443
```

### 3. Database Security
- Use Windows Authentication when possible
- Restrict database user permissions
- Enable SQL Server encryption
- Regular backups

### 4. Network Security
- Configure firewall rules
- Use VPN for remote access
- Implement IP whitelisting if needed

### 5. Regular Updates
- Keep Windows Server updated
- Update .NET Runtime when new versions released
- Apply SQL Server security patches

---

## Support and Contact

### Documentation
- User Manual: `USER_MANUAL.md`
- API Documentation: `http://localhost:5000/swagger`
- Developer Guide: `DEVELOPER_GUIDE.md`

### Technical Support
- Email: support@yourcompany.com
- Phone: +1-XXX-XXX-XXXX
- Portal: https://support.yourcompany.com

### System Information
- Version: 1.0.0
- Release Date: 2025-01-18
- License: Commercial

---

## Appendix

### Default Port Configuration

| Service | Port | Protocol |
|---------|------|----------|
| Frontend (IIS) | 80 | HTTP |
| Frontend (IIS HTTPS) | 443 | HTTPS |
| API Service | 5000 | HTTP |
| SQL Server | 1433 | TCP |

### File Locations

| Component | Path |
|-----------|------|
| Application Files | `C:\Program Files\ComplaintManagement\` |
| API Executable | `C:\Program Files\ComplaintManagement\API\` |
| Frontend Files | `C:\Program Files\ComplaintManagement\WWW\` |
| Log Files | `C:\Program Files\ComplaintManagement\API\Logs\` |
| Configuration | `C:\Program Files\ComplaintManagement\API\appsettings.json` |

### Database Schema

The installer automatically creates:
- **Tables**: 20+ tables for complaints, users, departments, etc.
- **Views**: Pre-configured views for reporting
- **Stored Procedures**: Performance-optimized queries
- **Indexes**: Optimized for search and filtering
- **Constraints**: Foreign keys and data integrity rules

### Default Admin Account

```
Email: admin@complaintmanagement.com
Password: Admin@123
Role: Administrator
Company: Default Company
```

**⚠️ IMPORTANT: Change this password immediately after installation!**

---

*End of Installation Guide*
