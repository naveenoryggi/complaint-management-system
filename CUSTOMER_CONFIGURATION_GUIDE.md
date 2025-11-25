# Customer Configuration Guide - Oryggi Database Integration

**Version**: 1.0
**Date**: 2025-10-12
**For**: Complaint Management System Customers

---

## Overview

This guide helps customers configure the Complaint Management System to connect with their existing Oryggi HRMS database. The system is **fully configurable** and supports different database names, server names, table names, and field names.

---

## 📋 Prerequisites

Before starting, you need:

1. ✅ SQL Server instance with Oryggi HRMS database
2. ✅ Database credentials (username/password)
3. ✅ Network access from Complaint System server to Oryggi database
4. ✅ Read access to Oryggi database tables

---

## 🔧 Configuration Steps

### Step 1: Locate Configuration File

Navigate to the API configuration file:

**File Location**:
```
complaint-system-dotnet/src/ComplaintManagement.API/appsettings.json
```

### Step 2: Update Connection Strings

Find the `ConnectionStrings` section and update with your details:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ComplaintManagementDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true",
    "OryggiConnection": "Server=YOUR_SERVER;Database=YOUR_ORYGGI_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Replace**:
- `YOUR_SERVER` → Your SQL Server name (e.g., `LAPTOP-ABC\\SQLEXPRESS` or `192.168.1.100`)
- `YOUR_ORYGGI_DB` → Your Oryggi database name (e.g., `Oryggi`, `OryggiHRMS`, `HRMS`)
- `YOUR_USER` → Database username (e.g., `sa` or specific user)
- `YOUR_PASSWORD` → Database password

**Examples**:

**Example 1: Local SQL Server Express**
```json
"OryggiConnection": "Server=LAPTOP-NF9BTG7Q\\SQLEXPRESS;Database=Oryggi;User Id=sa;Password=Admin@123;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Example 2: Remote SQL Server**
```json
"OryggiConnection": "Server=192.168.1.50;Database=OryggiHRMS;User Id=hrms_user;Password=SecurePass123;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Example 3: Windows Authentication**
```json
"OryggiConnection": "Server=SQL-SERVER-01;Database=Oryggi;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

---

### Step 3: Configure Oryggi Settings

Update the `OryggiSettings` section to match your Oryggi database schema:

```json
{
  "OryggiSettings": {
    "Enabled": true,
    "SyncOnStartup": false,
    "SyncIntervalHours": 6,
    "TableMappings": {
      "CompanyTable": "CompanyMaster",
      "BranchTable": "BranchMaster",
      "DepartmentTable": "DeptMaster",
      "SectionTable": "SectionMaster",
      "EmployeeTable": "EmployeeMaster"
    }
  }
}
```

**Configuration Options**:

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | boolean | `true` | Enable/disable Oryggi sync |
| `SyncOnStartup` | boolean | `false` | Run sync immediately on application startup |
| `SyncIntervalHours` | number | `6` | Auto-sync frequency (hours) |

**Table Mappings**:

If your Oryggi database uses **different table names**, update the `TableMappings` section:

```json
"TableMappings": {
  "CompanyTable": "YOUR_COMPANY_TABLE_NAME",
  "BranchTable": "YOUR_BRANCH_TABLE_NAME",
  "DepartmentTable": "YOUR_DEPARTMENT_TABLE_NAME",
  "SectionTable": "YOUR_SECTION_TABLE_NAME",
  "EmployeeTable": "YOUR_EMPLOYEE_TABLE_NAME"
}
```

**Common Variations**:

| Standard | Your Database Might Use |
|----------|------------------------|
| `CompanyMaster` | `Companies`, `tbl_Company`, `Company_Master` |
| `BranchMaster` | `Branches`, `tbl_Branch`, `Branch_Master` |
| `DeptMaster` | `Departments`, `tbl_Department`, `Dept_Master` |
| `SectionMaster` | `Sections`, `tbl_Section`, `Section_Master` |
| `EmployeeMaster` | `Employees`, `tbl_Employee`, `Employee_Master` |

---

### Step 4: Configure Field Mappings (Advanced)

If your Oryggi database uses **different column names**, update the `FieldMappings` section:

#### Company Field Mapping

```json
"Company": {
  "KeyField": "Ccode",           // Company ID field
  "NameField": "CName",           // Company name field
  "AddressField": "Address",      // Address field
  "EmailField": "Email",          // Email field
  "PhoneField": "TelephoneNo"     // Phone field
}
```

**Common Variations**:
- `Ccode` → `CompanyID`, `CompanyCode`, `Company_ID`
- `CName` → `CompanyName`, `Name`, `Company_Name`

#### Branch Field Mapping

```json
"Branch": {
  "KeyField": "BranchCode",       // Branch ID field
  "NameField": "BranchName",      // Branch name field
  "LocationField": "Location",    // Location field
  "CompanyKeyField": "Ccode"      // Foreign key to Company
}
```

#### Department Field Mapping

```json
"Department": {
  "KeyField": "Dcode",            // Department ID field
  "NameField": "Dname",           // Department name field
  "BranchKeyField": "BranchCode"  // Foreign key to Branch
}
```

#### Section Field Mapping

```json
"Section": {
  "KeyField": "SecCode",          // Section ID field
  "NameField": "SecName",         // Section name field
  "DepartmentKeyField": "Dcode"   // Foreign key to Department
}
```

#### Employee Field Mapping

```json
"Employee": {
  "KeyField": "Ecode",
  "EmployeeCodeField": "CorpEmpCode",
  "EmailField": "E_mail",
  "PhoneField": "Telephone1",
  "PhoneSecondaryField": "Telephone2",
  "FirstNameField": "FName",
  "LastNameField": "LName",
  "FullNameField": "EmpName",
  "ManagerKeyField": "ReportingHeadEcode",
  "SectionKeyField": "SecCode",
  "DateOfJoiningField": "DateofJoin",
  "DateOfBirthField": "DateofBirth",
  "IsActiveField": "Active"
}
```

---

## 🧪 Testing Configuration

### Step 1: Verify Database Connection

Run this SQL query on your SQL Server to verify Oryggi database exists:

```sql
-- Check database exists
SELECT name FROM sys.databases WHERE name LIKE '%Oryggi%'

-- Check tables exist
USE Oryggi  -- Replace with your database name
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN (
    'CompanyMaster',
    'BranchMaster',
    'DeptMaster',
    'SectionMaster',
    'EmployeeMaster'
)
```

### Step 2: Verify Data Exists

```sql
-- Count records in each table
SELECT 'Companies' as TableName, COUNT(*) as RecordCount FROM CompanyMaster
UNION ALL
SELECT 'Branches', COUNT(*) FROM BranchMaster
UNION ALL
SELECT 'Departments', COUNT(*) FROM DeptMaster
UNION ALL
SELECT 'Sections', COUNT(*) FROM SectionMaster
UNION ALL
SELECT 'Employees', COUNT(*) FROM EmployeeMaster
```

### Step 3: Test Application Startup

1. Save your `appsettings.json` file
2. Restart the Complaint Management API
3. Check logs for connection errors
4. Access the Oryggi Sync Dashboard in the admin panel

---

## 🚨 Troubleshooting

### Problem: Cannot connect to Oryggi database

**Solution 1**: Check network connectivity
```bash
# From Complaint System server, ping Oryggi SQL Server
ping YOUR_SQL_SERVER_IP

# Test SQL Server port (default 1433)
telnet YOUR_SQL_SERVER_IP 1433
```

**Solution 2**: Verify credentials
- Login to SQL Server Management Studio with the same credentials
- Ensure user has `SELECT` permissions on Oryggi tables

**Solution 3**: Enable TCP/IP in SQL Server
1. Open SQL Server Configuration Manager
2. Go to SQL Server Network Configuration → Protocols
3. Enable TCP/IP
4. Restart SQL Server service

---

### Problem: "Table does not exist" error

**Solution**: Check table names match your database

```sql
-- List all tables in Oryggi database
USE Oryggi
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME
```

Update `TableMappings` in `appsettings.json` with actual table names.

---

### Problem: "Column does not exist" error

**Solution**: Check column names match your schema

```sql
-- Check columns in CompanyMaster
USE Oryggi
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'CompanyMaster'
ORDER BY ORDINAL_POSITION
```

Update `FieldMappings` in `appsettings.json` with actual column names.

---

### Problem: Sync not running

**Solution 1**: Check if sync is enabled
- Verify `"Enabled": true` in `OryggiSettings`

**Solution 2**: Check sync interval
- Default is 6 hours, change `SyncIntervalHours` if needed
- Set `"SyncOnStartup": true` to run sync immediately

**Solution 3**: Trigger manual sync
- Login to admin panel
- Navigate to Oryggi Sync Dashboard
- Click "Run Manual Sync"

---

## 📊 Sync Dashboard

After configuration, access the Oryggi Sync Dashboard:

**URL**: `http://your-app-url/admin/oryggi-sync`

**Features**:
- ✅ View last sync timestamp
- ✅ View synced record counts
- ✅ Trigger manual sync
- ✅ View sync history logs
- ✅ Check sync status (success/failed)

---

## 🔐 Security Best Practices

### 1. Use Dedicated Read-Only User

**Recommended**: Create a dedicated database user for sync operations:

```sql
-- Create read-only user for Complaint System
CREATE LOGIN complaint_sync_user WITH PASSWORD = 'StrongPassword123!';
USE Oryggi;
CREATE USER complaint_sync_user FOR LOGIN complaint_sync_user;

-- Grant SELECT permissions only
GRANT SELECT ON CompanyMaster TO complaint_sync_user;
GRANT SELECT ON BranchMaster TO complaint_sync_user;
GRANT SELECT ON DeptMaster TO complaint_sync_user;
GRANT SELECT ON SectionMaster TO complaint_sync_user;
GRANT SELECT ON EmployeeMaster TO complaint_sync_user;
```

Then update connection string:
```json
"OryggiConnection": "Server=YOUR_SERVER;Database=Oryggi;User Id=complaint_sync_user;Password=StrongPassword123!;..."
```

### 2. Store Passwords Securely

**Option 1**: Use Environment Variables (Recommended for production)
```json
"OryggiConnection": "Server=YOUR_SERVER;Database=Oryggi;User Id=complaint_sync_user;Password=${ORYGGI_DB_PASSWORD};..."
```

**Option 2**: Use Azure Key Vault / AWS Secrets Manager

**Option 3**: Use User Secrets (Development only)
```bash
dotnet user-secrets set "ConnectionStrings:OryggiConnection" "Server=...;Password=YourPassword;..."
```

### 3. Enable SSL/TLS

For remote SQL Server, use encrypted connections:
```json
"OryggiConnection": "Server=YOUR_SERVER;Database=Oryggi;User Id=sa;Password=YourPassword;Encrypt=True;TrustServerCertificate=False;..."
```

---

## 📝 Configuration Template

Copy this template and customize for your environment:

```json
{
  "ConnectionStrings": {
    "OryggiConnection": "Server=_____________;Database=_____________;User Id=_____________;Password=_____________;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "OryggiSettings": {
    "Enabled": true,
    "SyncOnStartup": false,
    "SyncIntervalHours": 6,
    "TableMappings": {
      "CompanyTable": "_____________",
      "BranchTable": "_____________",
      "DepartmentTable": "_____________",
      "SectionTable": "_____________",
      "EmployeeTable": "_____________"
    },
    "FieldMappings": {
      "Company": {
        "KeyField": "_____________",
        "NameField": "_____________",
        "AddressField": "_____________",
        "EmailField": "_____________",
        "PhoneField": "_____________"
      }
      // ... (complete other field mappings)
    }
  }
}
```

---

## 📞 Support

If you encounter issues:

1. Check this guide's Troubleshooting section
2. Review application logs: `ComplaintManagement.API/Logs/`
3. Check SQL Server logs
4. Contact support with:
   - Error message from logs
   - Your configuration (hide passwords!)
   - Oryggi database schema (table/column names)

---

## ✅ Configuration Checklist

Before going live, verify:

- [ ] Connection string updated with correct server/database name
- [ ] Database credentials tested and working
- [ ] Network connectivity verified
- [ ] Table names match your Oryggi database
- [ ] Field names match your Oryggi schema
- [ ] Test sync completed successfully
- [ ] Synced data appears in Complaint System
- [ ] Foreign key relationships preserved
- [ ] Employee-manager relationships correct
- [ ] Read-only user created (recommended)
- [ ] Passwords secured (not in plain text)
- [ ] Sync interval configured appropriately
- [ ] Sync dashboard accessible

---

**Document Version**: 1.0
**Last Updated**: 2025-10-12
**Next Review**: After first customer deployment
