-- ============================================
-- SQL SERVER DIAGNOSTICS AND CLEANUP SCRIPTS
-- For Complaint Management System
-- ============================================

USE ComplaintManagementDb;
GO

-- ============================================
-- 1. CHECK FOR BLOCKED PROCESSES
-- ============================================
-- This query shows all blocking chains
SELECT
    blocking.session_id AS BlockingSessionId,
    blocked.session_id AS BlockedSessionId,
    blocking_sql.text AS BlockingQuery,
    blocked_sql.text AS BlockedQuery,
    blocking.login_name AS BlockingUser,
    blocked.login_name AS BlockedUser,
    blocked.wait_type AS WaitType,
    blocked.wait_time / 1000 AS WaitTimeSeconds,
    blocked.status AS BlockedStatus,
    blocking.status AS BlockingStatus,
    blocked.program_name AS BlockedProgram,
    blocking.program_name AS BlockingProgram
FROM sys.dm_exec_requests blocked
INNER JOIN sys.dm_exec_sessions blocking
    ON blocked.blocking_session_id = blocking.session_id
CROSS APPLY sys.dm_exec_sql_text(blocked.sql_handle) blocked_sql
CROSS APPLY sys.dm_exec_sql_text(blocking.last_request_sql_handle) blocking_sql
WHERE blocked.blocking_session_id <> 0
ORDER BY blocked.wait_time DESC;

-- ============================================
-- 2. CHECK FOR LONG-RUNNING QUERIES
-- ============================================
-- Shows queries running longer than 30 seconds
SELECT
    r.session_id AS SessionId,
    s.login_name AS LoginName,
    s.host_name AS HostName,
    s.program_name AS ProgramName,
    r.status AS Status,
    r.command AS Command,
    r.cpu_time AS CpuTime,
    r.total_elapsed_time / 1000 AS ElapsedTimeSeconds,
    r.reads AS Reads,
    r.writes AS Writes,
    r.logical_reads AS LogicalReads,
    DB_NAME(r.database_id) AS DatabaseName,
    SUBSTRING(
        sql.text,
        (r.statement_start_offset / 2) + 1,
        ((CASE r.statement_end_offset
            WHEN -1 THEN DATALENGTH(sql.text)
            ELSE r.statement_end_offset
        END - r.statement_start_offset) / 2) + 1
    ) AS QueryText,
    sql.text AS FullQueryText,
    r.blocking_session_id AS BlockingSessionId,
    r.wait_type AS WaitType,
    r.wait_time / 1000 AS WaitTimeSeconds,
    r.last_wait_type AS LastWaitType
FROM sys.dm_exec_requests r
INNER JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) sql
WHERE r.total_elapsed_time > 30000  -- More than 30 seconds
    AND s.is_user_process = 1
ORDER BY r.total_elapsed_time DESC;

-- ============================================
-- 3. CHECK ALL ACTIVE SESSIONS
-- ============================================
SELECT
    s.session_id AS SessionId,
    s.login_name AS LoginName,
    s.host_name AS HostName,
    s.program_name AS ProgramName,
    s.status AS Status,
    s.cpu_time AS CpuTime,
    s.memory_usage AS MemoryUsage,
    s.total_elapsed_time / 1000 AS ElapsedTimeSeconds,
    s.reads AS Reads,
    s.writes AS Writes,
    s.logical_reads AS LogicalReads,
    s.last_request_start_time AS LastRequestStartTime,
    s.last_request_end_time AS LastRequestEndTime,
    c.net_transport AS NetTransport,
    c.client_net_address AS ClientIP
FROM sys.dm_exec_sessions s
LEFT JOIN sys.dm_exec_connections c ON s.session_id = c.session_id
WHERE s.is_user_process = 1
    AND s.session_id <> @@SPID  -- Exclude current session
ORDER BY s.total_elapsed_time DESC;

-- ============================================
-- 4. FIND DEADLOCKS (Recent)
-- ============================================
-- Check system health extended events for deadlocks
SELECT
    CAST(target_data AS XML) AS DeadlockGraph
FROM sys.dm_xe_session_targets st
INNER JOIN sys.dm_xe_sessions s ON s.address = st.event_session_address
WHERE s.name = 'system_health'
    AND st.target_name = 'ring_buffer';

-- ============================================
-- 5. CHECK OPEN TRANSACTIONS
-- ============================================
-- Shows all open transactions that might be holding locks
SELECT
    s.session_id AS SessionId,
    s.login_name AS LoginName,
    s.host_name AS HostName,
    s.program_name AS ProgramName,
    t.transaction_id AS TransactionId,
    t.name AS TransactionName,
    t.transaction_begin_time AS BeginTime,
    DATEDIFF(SECOND, t.transaction_begin_time, GETDATE()) AS DurationSeconds,
    t.transaction_type AS TransactionType,
    CASE t.transaction_state
        WHEN 0 THEN 'Initialized'
        WHEN 1 THEN 'Initialized but not started'
        WHEN 2 THEN 'Active'
        WHEN 3 THEN 'Ended (read-only)'
        WHEN 4 THEN 'Commit initiated'
        WHEN 5 THEN 'Prepared'
        WHEN 6 THEN 'Committed'
        WHEN 7 THEN 'Rolling back'
        WHEN 8 THEN 'Rolled back'
    END AS TransactionState,
    sql.text AS QueryText
FROM sys.dm_tran_active_transactions t
INNER JOIN sys.dm_tran_session_transactions st ON t.transaction_id = st.transaction_id
INNER JOIN sys.dm_exec_sessions s ON st.session_id = s.session_id
LEFT JOIN sys.dm_exec_connections c ON s.session_id = c.session_id
OUTER APPLY sys.dm_exec_sql_text(c.most_recent_sql_handle) sql
WHERE s.is_user_process = 1
ORDER BY t.transaction_begin_time;

-- ============================================
-- 6. KILL A SPECIFIC SESSION (USE WITH CAUTION!)
-- ============================================
-- Replace <session_id> with the actual session ID to kill
-- Example: KILL 54;
-- Uncomment and replace the session ID below:
-- KILL <session_id>;

-- ============================================
-- 7. KILL ALL ORYGGI SYNC PROCESSES
-- ============================================
-- WARNING: This will kill ALL processes related to OryggiSync
-- Only use this if syncs are completely stuck
/*
DECLARE @SessionId INT;
DECLARE @sql NVARCHAR(1000);

DECLARE session_cursor CURSOR FOR
SELECT s.session_id
FROM sys.dm_exec_sessions s
WHERE s.program_name LIKE '%ComplaintManagement%'
    OR s.login_name LIKE '%ComplaintManagement%'
    AND s.session_id <> @@SPID;  -- Don't kill current session

OPEN session_cursor;
FETCH NEXT FROM session_cursor INTO @SessionId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'KILL ' + CAST(@SessionId AS NVARCHAR(10));
    PRINT 'Killing session: ' + CAST(@SessionId AS NVARCHAR(10));
    EXEC sp_executesql @sql;
    FETCH NEXT FROM session_cursor INTO @SessionId;
END

CLOSE session_cursor;
DEALLOCATE session_cursor;
*/

-- ============================================
-- 8. CHECK DATABASE LOCKS
-- ============================================
SELECT
    l.request_session_id AS SessionId,
    s.login_name AS LoginName,
    s.program_name AS ProgramName,
    DB_NAME(l.resource_database_id) AS DatabaseName,
    OBJECT_NAME(l.resource_associated_entity_id, l.resource_database_id) AS ObjectName,
    l.resource_type AS ResourceType,
    l.request_mode AS RequestMode,
    l.request_type AS RequestType,
    l.request_status AS RequestStatus,
    sql.text AS QueryText
FROM sys.dm_tran_locks l
INNER JOIN sys.dm_exec_sessions s ON l.request_session_id = s.session_id
LEFT JOIN sys.dm_exec_connections c ON s.session_id = c.session_id
OUTER APPLY sys.dm_exec_sql_text(c.most_recent_sql_handle) sql
WHERE l.resource_database_id = DB_ID('ComplaintManagementDb')
    AND s.is_user_process = 1
ORDER BY l.request_session_id;

-- ============================================
-- 9. CHECK TRANSACTION LOG SIZE
-- ============================================
-- Large transaction logs can indicate stuck transactions
SELECT
    DB_NAME(database_id) AS DatabaseName,
    name AS LogicalName,
    physical_name AS PhysicalName,
    size * 8.0 / 1024 AS SizeMB,
    max_size * 8.0 / 1024 AS MaxSizeMB,
    growth AS Growth,
    type_desc AS FileType
FROM sys.master_files
WHERE database_id = DB_ID('ComplaintManagementDb')
ORDER BY type_desc;

-- ============================================
-- 10. FIND QUERIES WAITING ON LOCKS
-- ============================================
SELECT
    r.session_id AS SessionId,
    r.blocking_session_id AS BlockedBySessionId,
    s.login_name AS LoginName,
    s.program_name AS ProgramName,
    r.wait_type AS WaitType,
    r.wait_time / 1000 AS WaitTimeSeconds,
    r.wait_resource AS WaitResource,
    OBJECT_NAME(l.resource_associated_entity_id, l.resource_database_id) AS LockedObject,
    l.request_mode AS LockMode,
    sql.text AS QueryText
FROM sys.dm_exec_requests r
INNER JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
LEFT JOIN sys.dm_tran_locks l ON r.session_id = l.request_session_id
LEFT JOIN sys.dm_exec_connections c ON s.session_id = c.session_id
OUTER APPLY sys.dm_exec_sql_text(c.most_recent_sql_handle) sql
WHERE r.wait_type LIKE 'LCK%'  -- Lock waits
    AND s.is_user_process = 1
ORDER BY r.wait_time DESC;

-- ============================================
-- 11. CHECK SYNC HISTORY FOR STUCK SYNCS
-- ============================================
SELECT
    Id,
    TenantId,
    StartTime,
    EndTime,
    Status,
    Duration,
    TotalRecordsProcessed,
    ErrorMessage,
    DATEDIFF(MINUTE, StartTime, GETDATE()) AS MinutesSinceStart,
    CASE
        WHEN Status = 'IN_PROGRESS' AND DATEDIFF(MINUTE, StartTime, GETDATE()) > 30
        THEN 'POSSIBLY STUCK'
        ELSE 'OK'
    END AS HealthStatus
FROM SyncHistory
WHERE Status = 'IN_PROGRESS'
    OR (Status = 'IN_PROGRESS' AND DATEDIFF(MINUTE, StartTime, GETDATE()) > 15)
ORDER BY StartTime DESC;

-- ============================================
-- 12. MARK STUCK SYNCS AS FAILED
-- ============================================
-- Update any sync that has been running for more than 1 hour as failed
/*
UPDATE SyncHistory
SET
    Status = 'FAILED',
    EndTime = GETDATE(),
    Duration = DATEDIFF(SECOND, StartTime, GETDATE()),
    ErrorMessage = 'Sync marked as failed due to timeout (> 1 hour)'
WHERE Status = 'IN_PROGRESS'
    AND DATEDIFF(MINUTE, StartTime, GETDATE()) > 60
    AND IsDeleted = 0;
*/

-- ============================================
-- 13. CHECK FOR MISSING INDEXES
-- ============================================
-- Shows indexes that SQL Server thinks would improve performance
SELECT TOP 10
    OBJECT_NAME(s.object_id) AS TableName,
    s.avg_total_user_cost * s.avg_user_impact * (s.user_seeks + s.user_scans) AS ImprovementMeasure,
    s.avg_user_impact AS AvgUserImpact,
    s.avg_total_user_cost AS AvgTotalUserCost,
    s.user_seeks AS UserSeeks,
    s.user_scans AS UserScans,
    d.equality_columns AS EqualityColumns,
    d.inequality_columns AS InequalityColumns,
    d.included_columns AS IncludedColumns
FROM sys.dm_db_missing_index_groups g
INNER JOIN sys.dm_db_missing_index_group_stats s ON g.index_group_handle = s.group_handle
INNER JOIN sys.dm_db_missing_index_details d ON g.index_handle = d.index_handle
WHERE d.database_id = DB_ID('ComplaintManagementDb')
ORDER BY ImprovementMeasure DESC;

-- ============================================
-- 14. CHECK STATISTICS FRESHNESS
-- ============================================
-- Old statistics can cause poor query plans
SELECT
    OBJECT_NAME(s.object_id) AS TableName,
    s.name AS StatName,
    STATS_DATE(s.object_id, s.stats_id) AS LastUpdated,
    DATEDIFF(DAY, STATS_DATE(s.object_id, s.stats_id), GETDATE()) AS DaysSinceUpdate,
    sp.rows AS RowCount,
    sp.modification_counter AS ModificationsSinceUpdate
FROM sys.stats s
CROSS APPLY sys.dm_db_stats_properties(s.object_id, s.stats_id) sp
WHERE OBJECTPROPERTY(s.object_id, 'IsUserTable') = 1
    AND DATEDIFF(DAY, STATS_DATE(s.object_id, s.stats_id), GETDATE()) > 7
ORDER BY DaysSinceUpdate DESC;

-- ============================================
-- 15. UPDATE STATISTICS (Run if needed)
-- ============================================
-- Updates statistics for all tables in the database
-- WARNING: This can be resource-intensive on large databases
/*
EXEC sp_updatestats;
*/

-- ============================================
-- 16. REBUILD FRAGMENTED INDEXES
-- ============================================
-- Check for fragmented indexes
SELECT
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.index_type_desc AS IndexType,
    ips.avg_fragmentation_in_percent AS FragmentationPercent,
    ips.page_count AS PageCount,
    CASE
        WHEN ips.avg_fragmentation_in_percent > 30 THEN 'REBUILD'
        WHEN ips.avg_fragmentation_in_percent > 10 THEN 'REORGANIZE'
        ELSE 'OK'
    END AS RecommendedAction
FROM sys.dm_db_index_physical_stats(
    DB_ID('ComplaintManagementDb'),
    NULL, NULL, NULL, 'LIMITED'
) ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id
    AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
    AND ips.page_count > 1000  -- Only consider indexes with > 1000 pages
ORDER BY ips.avg_fragmentation_in_percent DESC;

-- ============================================
-- 17. QUICK HEALTH CHECK SUMMARY
-- ============================================
PRINT '=== SQL SERVER HEALTH CHECK SUMMARY ===';
PRINT '';
PRINT 'Blocked Sessions: ';
SELECT COUNT(*) AS BlockedCount
FROM sys.dm_exec_requests
WHERE blocking_session_id <> 0;

PRINT 'Long Running Queries (>30s): ';
SELECT COUNT(*) AS LongRunningCount
FROM sys.dm_exec_requests r
INNER JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
WHERE r.total_elapsed_time > 30000
    AND s.is_user_process = 1;

PRINT 'Open Transactions: ';
SELECT COUNT(*) AS OpenTransactionCount
FROM sys.dm_tran_active_transactions t
INNER JOIN sys.dm_tran_session_transactions st ON t.transaction_id = st.transaction_id
INNER JOIN sys.dm_exec_sessions s ON st.session_id = s.session_id
WHERE s.is_user_process = 1;

PRINT 'Stuck Syncs (>30 min): ';
SELECT COUNT(*) AS StuckSyncCount
FROM SyncHistory
WHERE Status = 'IN_PROGRESS'
    AND DATEDIFF(MINUTE, StartTime, GETDATE()) > 30
    AND IsDeleted = 0;

PRINT '';
PRINT '=== END SUMMARY ===';
