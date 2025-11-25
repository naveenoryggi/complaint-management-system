# Backup & Restore Instructions

## Current Stable Version
**Date:** 2025-10-20
**Version:** Oryggi Sync Stable (v1.0)
**Status:** All features working, tested and stable

## What's Working:
✅ Oryggi employee sync (10,000+ employees)
✅ User creation and updates
✅ Complaint creation with file attachments
✅ Assignment and escalation
✅ Dashboard with modern UI
✅ All CRUD operations

## Manual Backup Created

### Backup Location:
A copy of this entire directory should be created at:
`C:\Users\Navin Chandra\Pictures\Complaint management system - BACKUP - 2025-10-20`

### To Create Backup Manually:
1. Copy the entire "Complaint management system" folder
2. Rename to "Complaint management system - BACKUP - [DATE]"
3. Store in a safe location

### To Restore from Backup:
1. Stop all running services (API and Angular)
2. Delete current "Complaint management system" folder
3. Copy backup folder back
4. Rename to "Complaint management system"
5. Restore database from backup (see below)
6. Restart services

## Database Backup

### Create Database Backup:
```powershell
sqlcmd -S "LAPTOP-NF9BTG7Q\SQLEXPRESS" -E -Q "BACKUP DATABASE ComplaintManagementDB TO DISK='C:\Backups\ComplaintManagementDB_2025-10-20.bak' WITH INIT"
```

### Restore Database:
```powershell
# Stop API first
sqlcmd -S "LAPTOP-NF9BTG7Q\SQLEXPRESS" -E -Q "RESTORE DATABASE ComplaintManagementDB FROM DISK='C:\Backups\ComplaintManagementDB_2025-10-20.bak' WITH REPLACE"
```

## Quick Rollback Checklist

If something goes wrong after new changes:

1. **Stop Services:**
   ```
   taskkill /F /IM dotnet.exe
   taskkill /F /IM node.exe
   ```

2. **Restore Code:**
   - Delete current folder
   - Copy backup folder back

3. **Restore Database:**
   - Run restore command above

4. **Restart Services:**
   ```
   cd complaint-system-dotnet\src\ComplaintManagement.API
   dotnet run

   cd complaint-system-angular
   ng serve
   ```

## Files Created Today (New Architecture):

These files are NEW and can be safely deleted if you want to revert:

### Domain Entities:
- ComplaintStatusMaster.cs
- ComplaintPriorityMaster.cs
- CustomFieldDefinition.cs
- CustomFieldValue.cs
- EventType.cs
- EventLog.cs
- EventCommunicationRule.cs
- CommunicationTemplate.cs
- CommunicationLog.cs
- EmailServerSettings.cs
- SmsGatewaySettings.cs
- WhatsAppSettings.cs

### Enums:
- CustomFieldType.cs
- CommunicationChannel.cs
- CommunicationStatus.cs
- RecipientType.cs

### Configurations:
- ComplaintStatusMasterConfiguration.cs
- ComplaintPriorityMasterConfiguration.cs

### Documentation:
- ARCHITECTURE.md
- SECURITY-COMPLIANCE.md
- BACKUP-RESTORE-INSTRUCTIONS.md (this file)

## Changes to Existing Files:

### Complaint.cs (SAFE - Only additions):
- Added `StatusMasterId` (nullable)
- Added `PriorityMasterId` (nullable)
- Added `CustomFieldValues` collection
- Kept existing `Status` and `Priority` enums

### ComplaintConfiguration.cs (SAFE - Only additions):
- Added indexes for new fields
- Added navigation properties

### ComplaintDbContext.cs (SAFE - Only additions):
- Added `ComplaintStatusMasters` DbSet
- Added `ComplaintPriorityMasters` DbSet

**All changes are ADDITIVE - no existing functionality removed!**

## Verification Steps

After any restore, verify:
1. ✅ Can login as admin
2. ✅ Can see dashboard
3. ✅ Can create new complaint
4. ✅ Can view existing complaints
5. ✅ Oryggi sync works
6. ✅ File attachments work

---

**IMPORTANT:** Always test in a dev environment first before applying to production!
