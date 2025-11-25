# Creating the Professional GUI Installer

I've created TWO options for you to create a professional installer with a beautiful graphical user interface:

## Option 1: Inno Setup (RECOMMENDED) ⭐

This is the industry-standard tool used by major software companies. It creates a professional wizard-style installer like you see with Adobe, Microsoft Office, etc.

### Step 1: Download Inno Setup
1. Go to: https://jrsoftware.org/isdl.php
2. Download "Inno Setup 6.x" (free and open-source)
3. Install it on your computer

### Step 2: Build Your Application
```powershell
# Build the .NET API
cd "complaint-system-dotnet\src\ComplaintManagement.API"
dotnet publish -c Release -o bin\Release\net8.0\publish

# Build the Angular Frontend
cd "..\..\..\"
cd "complaint-system-angular"
npm run build --prod
```

### Step 3: Open the Inno Setup Script
1. Open Inno Setup Compiler
2. File → Open → Select `ComplaintManagementSetup.iss`
3. Build → Compile

### Step 4: Get Your Installer!
The installer will be created in the `installer-output` folder:
- **File**: `ComplaintManagementSetup-v1.0.0.exe`
- **Size**: ~50-100 MB (depending on your app)

### Features You Get:

✅ **Beautiful Modern Wizard UI**
   - Welcome screen
   - License agreement
   - Component selection
   - Installation location browser
   - Database configuration with GUI
   - Progress bar with status
   - Completion screen

✅ **Database Configuration Screen**
   - SQL Server name input field
   - Database name input field
   - Windows Authentication checkbox
   - SQL Authentication username/password fields
   - "Test Connection" button

✅ **Professional Features**
   - Start menu shortcuts
   - Desktop icon (optional)
   - Automatic uninstaller creation
   - Windows registry integration
   - Service installation
   - IIS configuration
   - Add/Remove Programs integration

✅ **Installation Process**
   1. User runs `ComplaintManagementSetup-v1.0.0.exe`
   2. Modern wizard appears with your branding
   3. User clicks through welcome screen
   4. User selects installation location (or uses default)
   5. User enters database details in GUI form
   6. User clicks "Test Connection" button
   7. User clicks "Install"
   8. Progress bar shows installation steps
   9. Browser opens automatically when done

---

## Option 2: Custom WinForms Installer

I've also created a fully custom Windows Forms application with a beautiful modern UI.

### Features:
- 7-step wizard interface
- Modern flat design with blue gradient header
- Database configuration with visual feedback
- Prerequisites checker
- Real-time installation progress
- Log viewer

### To Use This:
```powershell
cd installer
dotnet build -c Release
dotnet publish -c Release --self-contained -r win-x64
```

The executable will be in: `installer\bin\Release\net8.0-windows\win-x64\publish\`

---

## Comparison

| Feature | Inno Setup | Custom WinForms |
|---------|------------|-----------------|
| **Professional Look** | ⭐⭐⭐⭐⭐ Industry standard | ⭐⭐⭐⭐ Modern & clean |
| **Ease of Use** | ⭐⭐⭐⭐⭐ Just compile | ⭐⭐⭐ Needs .NET |
| **Customization** | ⭐⭐⭐⭐ Very flexible | ⭐⭐⭐⭐⭐ Fully customizable |
| **File Size** | ⭐⭐⭐⭐ Compressed | ⭐⭐⭐ Larger (.NET included) |
| **Uninstaller** | ⭐⭐⭐⭐⭐ Automatic | ⭐⭐⭐ Manual creation |
| **Branding** | ⭐⭐⭐⭐⭐ Full branding | ⭐⭐⭐⭐ Code-based |

### My Recommendation: Use Inno Setup

**Why?**
1. ✅ More professional appearance
2. ✅ Smaller file size
3. ✅ Automatic uninstaller creation
4. ✅ Better Windows integration
5. ✅ Industry standard (trusted by users)
6. ✅ Easier to maintain and update
7. ✅ No .NET runtime needed for installer itself

---

## Screenshot of Inno Setup Installer

The installer will look like this:

### 📱 Welcome Screen
```
┌────────────────────────────────────────────┐
│  Setup - Complaint Management System      │
├────────────────────────────────────────────┤
│                                            │
│  Welcome to the Complaint Management      │
│  System Setup Wizard                      │
│                                            │
│  This will install Complaint Management   │
│  System on your computer.                 │
│                                            │
│  It is recommended that you close all     │
│  other applications before continuing.    │
│                                            │
│  Click Next to continue.                  │
│                                            │
└────────────────────────────────────────────┘
   [Cancel]              [< Back]  [Next >]
```

### 📱 Database Configuration Screen
```
┌────────────────────────────────────────────┐
│  Setup - Database Configuration           │
├────────────────────────────────────────────┤
│                                            │
│  SQL Server Name / IP Address:            │
│  ┌──────────────────────────────────────┐ │
│  │ localhost                            │ │
│  └──────────────────────────────────────┘ │
│                                            │
│  Database Name:                           │
│  ┌──────────────────────────────────────┐ │
│  │ ComplaintManagementDB                 │ │
│  └──────────────────────────────────────┘ │
│                                            │
│  ☑ Use Windows Authentication            │
│                                            │
│  SQL Username (if not using Windows Auth):│
│  ┌──────────────────────────────────────┐ │
│  │                                       │ │
│  └──────────────────────────────────────┘ │
│                                            │
│  SQL Password:                            │
│  ┌──────────────────────────────────────┐ │
│  │ ●●●●●●●●                             │ │
│  └──────────────────────────────────────┘ │
│                                            │
│         [Test Connection]                 │
│                                            │
└────────────────────────────────────────────┘
   [Cancel]              [< Back]  [Next >]
```

### 📱 Installing Screen
```
┌────────────────────────────────────────────┐
│  Setup - Installing                        │
├────────────────────────────────────────────┤
│                                            │
│  Please wait while Setup installs         │
│  Complaint Management System on your      │
│  computer.                                │
│                                            │
│  Status: Creating database...             │
│                                            │
│  ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░░  60%         │
│                                            │
│  Steps completed:                         │
│  ✓ Created installation directory         │
│  ✓ Installed API files                    │
│  ✓ Installed web files                    │
│  ⏳ Creating database...                   │
│  ○ Installing Windows Service             │
│  ○ Configuring IIS                        │
│                                            │
└────────────────────────────────────────────┘
   [Cancel]
```

### 📱 Completion Screen
```
┌────────────────────────────────────────────┐
│  Setup - Completing Installation          │
├────────────────────────────────────────────┤
│                                            │
│  ✓ Complaint Management System has been   │
│    successfully installed!                │
│                                            │
│  You can now access the application at:   │
│  http://localhost                         │
│                                            │
│  Default Login:                           │
│  Username: admin@complaintmanagement.com  │
│  Password: Admin@123                      │
│                                            │
│  ⚠️ Please change the default password!    │
│                                            │
│  ☑ Launch Complaint Management System now│
│                                            │
└────────────────────────────────────────────┘
                              [Finish]
```

---

## Quick Start Guide for End Users

Once you create the installer using Inno Setup, here's what your customers do:

### For End Users:

1. **Double-click `ComplaintManagementSetup-v1.0.0.exe`**
2. **Click "Next" through the wizard**
3. **Enter database details** (or use defaults)
4. **Click "Test Connection"** to verify
5. **Click "Install"**
6. **Done!** - Application opens automatically

**Total time**: 5-10 minutes
**User technical knowledge required**: Minimal (just need SQL Server details)

---

## Customization

### Add Your Company Logo
1. Replace `app-icon.ico` with your company icon
2. The icon appears in:
   - Installer window
   - Desktop shortcut
   - Start menu
   - Add/Remove Programs

### Change Colors/Branding
Edit the Inno Setup script:
```pascal
WizardImageFile=MyWizardImage.bmp
WizardSmallImageFile=MySmallImage.bmp
```

### Add License Agreement
Create `LICENSE.txt` and it will appear in the installer

---

## Building for Distribution

### Final Steps:

1. **Build your app**:
   ```powershell
   .\Build-For-Release.ps1
   ```

2. **Compile installer** (using Inno Setup):
   - Open `ComplaintManagementSetup.iss`
   - Click "Compile"

3. **Test the installer**:
   - Run `installer-output\ComplaintManagementSetup-v1.0.0.exe`
   - Test installation on a clean machine

4. **Distribute**:
   - Upload to your website
   - Send to customers via email
   - Put on USB drive
   - Host on download portal

---

## What Happens During Installation

The installer will:

1. ✅ Check for administrator privileges
2. ✅ Show welcome screen
3. ✅ Let user choose installation location
4. ✅ Show database configuration screen
5. ✅ Test database connection
6. ✅ Extract all files to install location
7. ✅ Update appsettings.json with database connection
8. ✅ Create database and run EF migrations
9. ✅ Install Windows Service (optional)
10. ✅ Configure IIS website (optional)
11. ✅ Create start menu shortcuts
12. ✅ Create desktop icon (optional)
13. ✅ Register in Add/Remove Programs
14. ✅ Create uninstaller
15. ✅ Open browser to application

**All with beautiful GUI progress bars and status messages!**

---

## Uninstallation

Users can uninstall via:
1. **Add/Remove Programs** (Control Panel)
2. **Start Menu** → Uninstall Complaint Management System
3. **Run** `unins000.exe` in install folder

The uninstaller will:
- Stop Windows Service
- Remove IIS configuration
- Delete all files
- Remove shortcuts
- Remove registry entries
- Clean and professional!

---

## Support

If you need help:
1. Inno Setup documentation: https://jrsoftware.org/ishelp/
2. Inno Setup community: Very active, helpful forum
3. The script I provided is fully commented and ready to use

---

**You now have everything you need to create a professional, enterprise-grade installer with a beautiful GUI!** 🎉

No more PowerShell scripts - just a clean, modern installer that your customers will love!
