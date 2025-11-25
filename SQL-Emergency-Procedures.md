# SQL Emergency Procedures - Complaint Management System

## Quick Response Guide for Stuck/Blocked SQL Processes

### 🚨 Emergency: System Completely Frozen

**Quick Fix (Kill all stuck sessions):**

```sql
-- 1. First, identify the problem
USE ComplaintManagementDb;

-- Check for blocked processes
SELECT blocking.session_id AS BlockingSessionId, blocked.session_id AS BlockedSessionId
FROM sys.dm_exec_requests blocked
INNER JOIN sys.dm_exec_sessions blocking ON blocked.blocking_session_id = blocking.session_id
WHERE blocked.blocking_session_id <> 0;

-- 2. Kill the blocking session (replace <session_id> with actual ID)
KILL <session_id>;
```

### 📊 Step-by-Step Diagnosis

#### Step 1: Run Quick Health Check
```sql
-- Run this first to get an overview
USE ComplaintManagementDb;

-- See blocked processes
SELECT COUNT(*) AS BlockedCount
FROM sys.dm_exec_requests
WHERE blocking_session_id <> 0;

-- See long-running queries
SELECT COUNT(*) AS LongRunningCount
FROM sys.dm_exec_requests r
INNER JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
WHERE r.total_elapsed_time > 30000 AND s.is_user_process = 1;
```

#### Step 2: Identify the Culprit
```sql
-- Find the specific query causing problems
SELECT TOP 5
    r.session_id AS SessionId,
    s.login_name AS User,
    r.total_elapsed_time / 1000 AS ElapsedSeconds,
    r.blocking_session_id AS BlockedBy,
    SUBSTRING(sql.text, (r.statement_start_offset / 2) + 1,
        ((CASE r.statement_end_offset WHEN -1 THEN DATALENGTH(sql.text)
        ELSE r.statement_end_offset END - r.statement_start_offset) / 2) + 1
    ) AS QueryText
FROM sys.dm_exec_requests r
INNER JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) sql
WHERE s.is_user_process = 1
ORDER BY r.total_elapsed_time DESC;
```

#### Step 3: Kill Problematic Sessions
```sql
-- Kill a specific session
KILL 54;  -- Replace 54 with actual session ID

-- Kill with rollback (if normal KILL doesn't work)
KILL 54 WITH STATUSONLY;  -- Check rollback progress
```

### 🔍 Common Scenarios

#### Scenario 1: Sync Process Stuck
**Symptoms:**
- Sync shows "IN_PROGRESS" for > 30 minutes
- High CPU usage
- Database locks visible

**Solution:**
```sql
-- 1. Check sync status
SELECT * FROM SyncHistory
WHERE Status = 'IN_PROGRESS'
    AND DATEDIFF(MINUTE, StartTime, GETDATE()) > 30
ORDER BY StartTime DESC;

-- 2. Find related SQL sessions
SELECT s.session_id, s.login_name, s.program_name, r.status, sql.text
FROM sys.dm_exec_sessions s
LEFT JOIN sys.dm_exec_requests r ON s.session_id = r.session_id
LEFT JOIN sys.dm_exec_connections c ON s.session_id = c.session_id
OUTER APPLY sys.dm_exec_sql_text(c.most_recent_sql_handle) sql
WHERE s.program_name LIKE '%ComplaintManagement%'
ORDER BY s.session_id;

-- 3. Kill the stuck session
-- KILL <session_id>;

-- 4. Mark sync as failed
UPDATE SyncHistory
SET Status = 'FAILED',
    EndTime = GETDATE(),
    ErrorMessage = 'Manually terminated due to timeout'
WHERE Status = 'IN_PROGRESS'
    AND DATEDIFF(MINUTE, StartTime, GETDATE()) > 30;
```

#### Scenario 2: Deadlock Detected
**Symptoms:**
- Error: "Transaction was deadlocked"
- Multiple processes waiting on each other

**Solution:**
```sql
-- 1. Check recent deadlocks
SELECT CAST(target_data AS XML) AS DeadlockGraph
FROM sys.dm_xe_session_targets st
INNER JOIN sys.dm_xe_sessions s ON s.address = st.event_session_address
WHERE s.name = 'system_health' AND st.target_name = 'ring_buffer';

-- 2. Set deadlock priority (in application code)
-- SET DEADLOCK_PRIORITY LOW;  -- Let this process be the victim

-- 3. Check locking tables
SELECT OBJECT_NAME(resource_associated_entity_id) AS LockedTable, *
FROM sys.dm_tran_locks
WHERE resource_database_id = DB_ID('ComplaintManagementDb')
    AND resource_type = 'OBJECT';
```

#### Scenario 3: Transaction Log Full
**Symptoms:**
- Error: "The transaction log is full"
- Cannot insert/update/delete

**Solution:**
```sql
-- 1. Check log size
SELECT name, size * 8.0 / 1024 AS SizeMB,
    max_size * 8.0 / 1024 AS MaxSizeMB
FROM sys.master_files
WHERE database_id = DB_ID('ComplaintManagementDb') AND type = 1;

-- 2. Backup transaction log (if in FULL recovery mode)
BACKUP LOG ComplaintManagementDb TO DISK = 'C:\Temp\ComplaintManagementDb_Log.bak';

-- 3. Shrink log (CAUTION: Only after backup)
-- USE ComplaintManagementDb;
-- DBCC SHRINKFILE (ComplaintManagementDb_log, 1);
```

### 🛡️ Prevention Strategies

#### 1. Application-Level Timeouts
```csharp
// In appsettings.json
"ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;Command Timeout=300;"
}

// In DbContext
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer(connectionString, options =>
    {
        options.CommandTimeout(300); // 5 minutes
        options.EnableRetryOnFailure(3);
    });
}
```

#### 2. Transaction Scope Timeout
```csharp
using (var scope = new TransactionScope(
    TransactionScopeOption.Required,
    new TransactionOptions
    {
        IsolationLevel = IsolationLevel.ReadCommitted,
        Timeout = TimeSpan.FromMinutes(10)
    }))
{
    // Your code here
    scope.Complete();
}
```

#### 3. Monitor and Auto-Kill
```sql
-- Create a SQL Agent Job to run every 5 minutes
-- Kill sessions running longer than 1 hour

DECLARE @SessionId INT;
DECLARE @sql NVARCHAR(1000);

SELECT TOP 1 @SessionId = r.session_id
FROM sys.dm_exec_requests r
INNER JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
WHERE r.total_elapsed_time > 3600000  -- 1 hour
    AND s.is_user_process = 1
    AND s.program_name LIKE '%ComplaintManagement%'
ORDER BY r.total_elapsed_time DESC;

IF @SessionId IS NOT NULL
BEGIN
    PRINT 'Killing long-running session: ' + CAST(@SessionId AS NVARCHAR(10));
    SET @sql = 'KILL ' + CAST(@SessionId AS NVARCHAR(10));
    EXEC sp_executesql @sql;
END
```

### 📞 When to Call for Help

**Call DBA immediately if:**
- ✅ Multiple KILLs don't work
- ✅ Transaction log at 90%+ capacity
- ✅ Database is in RECOVERY_PENDING state
- ✅ Server CPU consistently > 95%
- ✅ Deadlocks occurring every few minutes

### 📝 Maintenance Checklist

**Weekly:**
- [ ] Check for fragmented indexes (Query #16 in diagnostics)
- [ ] Update statistics if needed (Query #15)
- [ ] Review stuck syncs (Query #11)
- [ ] Check transaction log size (Query #9)

**Monthly:**
- [ ] Review missing indexes (Query #13)
- [ ] Check statistics freshness (Query #14)
- [ ] Rebuild heavily fragmented indexes
- [ ] Review and optimize slow queries

**After Each Major Sync:**
- [ ] Verify sync completed successfully
- [ ] Check for orphaned transactions
- [ ] Update statistics on affected tables
- [ ] Review query execution times

### 🔧 Useful Commands

```sql
-- See active transactions
DBCC OPENTRAN;

-- Check database status
SELECT name, state_desc, recovery_model_desc
FROM sys.databases
WHERE name = 'ComplaintManagementDb';

-- Get currently running queries
SELECT * FROM sys.dm_exec_requests WHERE session_id > 50;

-- See what's using tempdb
SELECT session_id, SUM(user_objects_alloc_page_count) AS user_objects_pages
FROM sys.dm_db_session_space_usage
GROUP BY session_id
ORDER BY user_objects_pages DESC;

-- Kill all sleeping sessions with open transactions
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'KILL ' + CAST(session_id AS VARCHAR(10)) + '; '
FROM sys.dm_exec_sessions
WHERE status = 'sleeping'
    AND open_transaction_count > 0
    AND session_id <> @@SPID;
EXEC sp_executesql @sql;
```

### 📚 Additional Resources

**Performance Monitoring:**
- sp_who2 - Quick session overview
- sp_WhoIsActive (if installed) - Detailed session info
- Activity Monitor in SSMS - GUI for monitoring

**Extended Events:**
```sql
-- Create event session to track long-running queries
CREATE EVENT SESSION [LongRunningQueries] ON SERVER
ADD EVENT sqlserver.sql_statement_completed(
    ACTION(sqlserver.sql_text, sqlserver.session_id)
    WHERE duration > 30000000  -- 30 seconds in microseconds
)
ADD TARGET package0.ring_buffer
WITH (MAX_MEMORY=4096 KB, EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,
      MAX_DISPATCH_LATENCY=30 SECONDS, STARTUP_STATE=ON);

-- Start the session
ALTER EVENT SESSION [LongRunningQueries] ON SERVER STATE = START;
```

---

**File Location:** `SQL-Diagnostics-and-Cleanup.sql` contains all queries mentioned above.

**Last Updated:** 2025-10-17
