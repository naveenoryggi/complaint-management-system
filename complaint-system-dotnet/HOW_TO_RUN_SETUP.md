# 🚀 HOW TO RUN SETUP - Step by Step

**UPDATED**: Now with error logging and easy troubleshooting!

---

## 📁 Files You Have Now

```
complaint-system-dotnet/
├── run-setup.bat                          ⭐ RUN THIS (easiest)
├── setup-solution-with-logging.ps1        📝 PowerShell script with logging
├── view-errors.bat                        👀 View errors after setup
├── setup-solution.ps1                     📝 Old script (use new one above)
└── HOW_TO_RUN_SETUP.md                   📖 This file
```

---

## ⚡ QUICK START (3 Easy Steps)

### Step 1: Open Command Prompt

```bash
# Navigate to the folder
cd "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet"
```

### Step 2: Run Setup

**Option A: Double-click** `run-setup.bat` in File Explorer

**Option B: Run in command prompt**
```bash
run-setup.bat
```

### Step 3: Check Results

**If successful:**
```bash
# You'll see:
✓ Solution file created
✓ Domain project created
✓ Application project created
... etc
```

**If there were errors:**
```bash
# Run this to view errors:
view-errors.bat

# Or manually:
type setup-errors.txt
```

---

## 📋 What The Script Does

The script will:
1. ✅ Check if .NET SDK is installed
2. ✅ Create .NET solution file
3. ✅ Create 6 projects (Domain, Application, Infrastructure, Shared, API, WorkerService)
4. ✅ Add project references
5. ✅ Install NuGet packages (EF Core, AutoMapper, FluentValidation, etc.)
6. ✅ Build the solution
7. ✅ **Log everything to files**

---

## 📊 Log Files Created

After running, you'll have:

### 1. `setup-log.txt` - Full Log
Contains **everything** (success + errors + all output)

```bash
# View full log:
type setup-log.txt
```

### 2. `setup-errors.txt` - Errors Only
Contains **only errors** (if any occurred)

```bash
# View errors only:
type setup-errors.txt

# Or use the batch file:
view-errors.bat
```

---

## ❓ Common Scenarios

### Scenario 1: Setup Succeeds ✅

**You'll see:**
```
✓ Solution file created
✓ Domain project created
✓ Application project created
✓ Infrastructure project created
✓ Shared project created
✓ API project created
✓ Worker Service project created
✓ Application packages installed
✓ Infrastructure packages installed
✓ API packages installed
✓ Worker Service packages installed
✓ Solution built successfully
```

**What to do:**
1. Verify: `dotnet build`
2. **Tell me "Setup successful"**
3. I'll create all the entities and code!

---

### Scenario 2: Setup Has Errors ❌

**You'll see:**
```
⚠️  THERE WERE ERRORS - Check setup-errors.txt
```

**What to do:**
1. Run: `view-errors.bat`
2. Copy the error text
3. **Share the errors with me**
4. I'll provide exact fixes

---

## 🔧 Viewing Errors

### Method 1: Batch File (Easiest)
```bash
# Just double-click or run:
view-errors.bat
```

### Method 2: Command Line
```bash
# View errors only:
type setup-errors.txt

# View full log:
type setup-log.txt
```

### Method 3: Notepad
```bash
# Open in Notepad:
notepad setup-errors.txt
```

---

## 🛠️ If .NET Is Not Installed

**Error you'll see:**
```
.NET SDK not found. Please install from: https://dotnet.microsoft.com/download/dotnet/8.0
```

**Fix:**
1. Go to: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download ".NET 8.0 SDK" (not Runtime)
3. Install it
4. **Restart Command Prompt**
5. Verify: `dotnet --version`
6. Re-run setup: `run-setup.bat`

---

## 📤 Sharing Errors With Me

### Quick Method:
```bash
# Run this and copy all the output:
view-errors.bat
```

### Detailed Method:
```bash
# Copy the error file content:
type setup-errors.txt

# Paste here in chat
```

### Full Debug Method:
```bash
# Copy both files:
type setup-log.txt > combined-log.txt
type setup-errors.txt >> combined-log.txt

# Share combined-log.txt
```

---

## ✅ Verify Setup Succeeded

### Check 1: Solution File Exists
```bash
dir *.sln

# Should show: ComplaintManagementSystem.sln
```

### Check 2: Projects Created
```bash
dir src

# Should show 6 folders:
# ComplaintManagement.API
# ComplaintManagement.Application
# ComplaintManagement.Domain
# ComplaintManagement.Infrastructure
# ComplaintManagement.Shared
# ComplaintManagement.WorkerService
```

### Check 3: Solution Builds
```bash
dotnet build

# Should show: Build succeeded. 0 Error(s)
```

---

## 🎯 Next Steps After Successful Setup

Once setup succeeds, **tell me** and I will:

1. ✅ Create all 13 Entity classes (C#)
2. ✅ Create DbContext with EF Core configurations
3. ✅ Create database migrations
4. ✅ Create seed data
5. ✅ Create API controllers
6. ✅ Create Worker Service background jobs
7. ✅ Create Angular frontend

---

## 🔄 Re-run Setup

If you need to start over:

```bash
# Delete everything except the script files
rmdir /s /q src
del ComplaintManagementSystem.sln
del setup-log.txt
del setup-errors.txt

# Re-run setup
run-setup.bat
```

---

## 📞 Still Having Issues?

**Share these 3 things with me:**

1. **Error log:**
   ```bash
   type setup-errors.txt
   ```

2. **.NET version:**
   ```bash
   dotnet --version
   ```

3. **What you see:**
   ```bash
   dir
   ```

---

## 🎉 Success Checklist

- [ ] .NET 8 SDK installed (`dotnet --version` works)
- [ ] Ran `run-setup.bat`
- [ ] No errors in `setup-errors.txt` (or file doesn't exist)
- [ ] `ComplaintManagementSystem.sln` exists
- [ ] 6 projects created in `src/` folder
- [ ] `dotnet build` succeeds
- [ ] **Ready to tell me it's done!**

---

**Status**: ⏳ Waiting for you to run setup
**Command**: `run-setup.bat`
**Next**: Share results (success or errors)

🚀 Let's get your .NET solution set up!
