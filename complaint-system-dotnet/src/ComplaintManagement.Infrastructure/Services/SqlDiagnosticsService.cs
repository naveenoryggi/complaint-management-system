using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ComplaintManagement.Application.DTOs.Sync;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services
{
    public interface ISqlDiagnosticsService
    {
        Task<SqlDiagnosticsResponse> GetDiagnosticsAsync();
        Task<SqlHealthCheckDto> GetHealthCheckAsync();
        Task<KillSessionResponse> KillSessionAsync(int sessionId, string reason);
        Task<bool> MarkStuckSyncsAsFailedAsync();
        Task<bool> MarkSingleSyncAsFailedAsync(Guid syncLogId);
    }

    public class SqlDiagnosticsService : ISqlDiagnosticsService
    {
        private readonly ComplaintDbContext _context;
        private readonly ILogger<SqlDiagnosticsService> _logger;

        public SqlDiagnosticsService(
            ComplaintDbContext context,
            ILogger<SqlDiagnosticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SqlDiagnosticsResponse> GetDiagnosticsAsync()
        {
            try
            {
                var response = new SqlDiagnosticsResponse
                {
                    HealthCheck = await GetHealthCheckAsync(),
                    BlockedProcesses = await GetBlockedProcessesAsync(),
                    LongRunningQueries = await GetLongRunningQueriesAsync(),
                    OpenTransactions = await GetOpenTransactionsAsync()
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SQL diagnostics. Ensure SQL user has VIEW SERVER STATE permission.");
                throw new InvalidOperationException(
                    "Failed to retrieve SQL diagnostics. The SQL user may lack VIEW SERVER STATE permission. " +
                    "Grant permission with: GRANT VIEW SERVER STATE TO [YourSqlUser]",
                    ex);
            }
        }

        public async Task<SqlHealthCheckDto> GetHealthCheckAsync()
        {
            try
            {
                var healthCheck = new SqlHealthCheckDto
                {
                    CheckTime = DateTime.UtcNow
                };

                // Count blocked processes
                var blockedSql = @"
                    SELECT COUNT(*)
                    FROM sys.dm_exec_requests
                    WHERE blocking_session_id <> 0";

                healthCheck.BlockedProcessCount = await ExecuteScalarQueryAsync<int>(blockedSql);

                // Count long-running queries (>30 seconds)
                var longRunningSql = @"
                    SELECT COUNT(*)
                    FROM sys.dm_exec_requests r
                    INNER JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
                    WHERE r.total_elapsed_time > 30000
                        AND s.is_user_process = 1";

                healthCheck.LongRunningQueryCount = await ExecuteScalarQueryAsync<int>(longRunningSql);

                // Count open transactions
                var openTransSql = @"
                    SELECT COUNT(*)
                    FROM sys.dm_tran_active_transactions t
                    INNER JOIN sys.dm_tran_session_transactions st ON t.transaction_id = st.transaction_id
                    INNER JOIN sys.dm_exec_sessions s ON st.session_id = s.session_id
                    WHERE s.is_user_process = 1";

                healthCheck.OpenTransactionCount = await ExecuteScalarQueryAsync<int>(openTransSql);

                // Count stuck syncs
                healthCheck.StuckSyncCount = await _context.SyncLogs
                    .Where(s => s.Status == "IN_PROGRESS"
                                && s.SyncStartedAt < DateTime.UtcNow.AddMinutes(-30)
                                && !s.IsDeleted)
                    .CountAsync();

                // Determine overall status
                if (healthCheck.BlockedProcessCount > 0 || healthCheck.StuckSyncCount > 0)
                    healthCheck.OverallStatus = "Critical";
                else if (healthCheck.LongRunningQueryCount > 2 || healthCheck.OpenTransactionCount > 5)
                    healthCheck.OverallStatus = "Warning";
                else
                    healthCheck.OverallStatus = "Healthy";

                return healthCheck;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health check");
                return new SqlHealthCheckDto
                {
                    CheckTime = DateTime.UtcNow,
                    OverallStatus = "Error"
                };
            }
        }

        private async Task<List<BlockedProcessDto>> GetBlockedProcessesAsync()
        {
            try
            {
                var sql = @"
                    SELECT
                        blocking.session_id AS BlockingSessionId,
                        blocked.session_id AS BlockedSessionId,
                        ISNULL(SUBSTRING(blocking_sql.text, 1, 200), '') AS BlockingQuery,
                        ISNULL(SUBSTRING(blocked_sql.text, 1, 200), '') AS BlockedQuery,
                        blocking.login_name AS BlockingUser,
                        blocked.login_name AS BlockedUser,
                        ISNULL(blocked.wait_type, '') AS WaitType,
                        blocked.wait_time / 1000 AS WaitTimeSeconds,
                        ISNULL(blocked.status, '') AS BlockedStatus,
                        ISNULL(blocking.status, '') AS BlockingStatus
                    FROM sys.dm_exec_requests blocked
                    INNER JOIN sys.dm_exec_sessions blocking ON blocked.blocking_session_id = blocking.session_id
                    OUTER APPLY sys.dm_exec_sql_text(blocked.sql_handle) blocked_sql
                    OUTER APPLY sys.dm_exec_sql_text(blocking.last_request_sql_handle) blocking_sql
                    WHERE blocked.blocking_session_id <> 0
                    ORDER BY blocked.wait_time DESC";

                return await ExecuteQueryAsync<BlockedProcessDto>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blocked processes");
                return new List<BlockedProcessDto>();
            }
        }

        private async Task<List<LongRunningQueryDto>> GetLongRunningQueriesAsync()
        {
            try
            {
                var sql = @"
                    SELECT TOP 10
                        r.session_id AS SessionId,
                        s.login_name AS LoginName,
                        ISNULL(s.program_name, '') AS ProgramName,
                        ISNULL(r.status, '') AS Status,
                        ISNULL(r.command, '') AS Command,
                        r.total_elapsed_time / 1000 AS ElapsedTimeSeconds,
                        ISNULL(SUBSTRING(sql.text, 1, 500), '') AS QueryText,
                        r.blocking_session_id AS BlockingSessionId,
                        ISNULL(r.wait_type, '') AS WaitType,
                        r.reads AS Reads,
                        r.writes AS Writes
                    FROM sys.dm_exec_requests r
                    INNER JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
                    OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) sql
                    WHERE r.total_elapsed_time > 30000
                        AND s.is_user_process = 1
                    ORDER BY r.total_elapsed_time DESC";

                return await ExecuteQueryAsync<LongRunningQueryDto>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving long-running queries");
                return new List<LongRunningQueryDto>();
            }
        }

        private async Task<List<OpenTransactionDto>> GetOpenTransactionsAsync()
        {
            try
            {
                var sql = @"
                    SELECT TOP 10
                        s.session_id AS SessionId,
                        s.login_name AS LoginName,
                        ISNULL(s.program_name, '') AS ProgramName,
                        t.transaction_id AS TransactionId,
                        ISNULL(t.name, '') AS TransactionName,
                        t.transaction_begin_time AS BeginTime,
                        DATEDIFF(SECOND, t.transaction_begin_time, GETUTCDATE()) AS DurationSeconds,
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
                            ELSE 'Unknown'
                        END AS TransactionState,
                        ISNULL(SUBSTRING(sql.text, 1, 500), '') AS QueryText
                    FROM sys.dm_tran_active_transactions t
                    INNER JOIN sys.dm_tran_session_transactions st ON t.transaction_id = st.transaction_id
                    INNER JOIN sys.dm_exec_sessions s ON st.session_id = s.session_id
                    LEFT JOIN sys.dm_exec_connections c ON s.session_id = c.session_id
                    OUTER APPLY sys.dm_exec_sql_text(c.most_recent_sql_handle) sql
                    WHERE s.is_user_process = 1
                    ORDER BY t.transaction_begin_time";

                return await ExecuteQueryAsync<OpenTransactionDto>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving open transactions");
                return new List<OpenTransactionDto>();
            }
        }

        public async Task<KillSessionResponse> KillSessionAsync(int sessionId, string reason)
        {
            try
            {
                // Verify session exists and is not system session
                var checkSql = @"
                    SELECT COUNT(*)
                    FROM sys.dm_exec_sessions
                    WHERE session_id = @SessionId
                        AND is_user_process = 1
                        AND session_id <> @@SPID";

                var parameters = new[] { new SqlParameter("@SessionId", sessionId) };
                var exists = await ExecuteScalarQueryAsync<int>(checkSql, parameters);

                if (exists == 0)
                {
                    return new KillSessionResponse
                    {
                        Success = false,
                        Message = "Session not found or cannot be killed (system session)",
                        SessionId = sessionId
                    };
                }

                // Execute KILL command
                var killSql = $"KILL {sessionId}";
                await ExecuteNonQueryAsync(killSql);

                _logger.LogWarning(
                    "Session {SessionId} killed by admin. Reason: {Reason}",
                    sessionId,
                    reason);

                return new KillSessionResponse
                {
                    Success = true,
                    Message = $"Session {sessionId} killed successfully",
                    SessionId = sessionId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error killing session {SessionId}", sessionId);
                return new KillSessionResponse
                {
                    Success = false,
                    Message = $"Error killing session: {ex.Message}",
                    SessionId = sessionId
                };
            }
        }

        public async Task<bool> MarkStuckSyncsAsFailedAsync()
        {
            try
            {
                var stuckSyncs = await _context.SyncLogs
                    .Where(s => s.Status == "IN_PROGRESS"
                                && s.SyncStartedAt < DateTime.UtcNow.AddHours(-1)
                                && !s.IsDeleted)
                    .ToListAsync();

                foreach (var sync in stuckSyncs)
                {
                    sync.Status = "FAILED";
                    sync.SyncCompletedAt = DateTime.UtcNow;

                    // Calculate duration - cap at 23:59:59 for SQL time type compatibility
                    var duration = sync.SyncCompletedAt.Value - sync.SyncStartedAt;
                    if (duration.TotalHours >= 24)
                    {
                        // For syncs longer than 24 hours, set to null (SQL time type limitation)
                        sync.Duration = null;
                    }
                    else
                    {
                        sync.Duration = duration;
                    }

                    sync.ErrorMessage = "Sync marked as failed due to timeout (> 1 hour)";
                }

                await _context.SaveChangesAsync();

                _logger.LogWarning("Marked {Count} stuck syncs as failed", stuckSyncs.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking stuck syncs as failed");
                return false;
            }
        }

        public async Task<bool> MarkSingleSyncAsFailedAsync(Guid syncLogId)
        {
            try
            {
                // Use Id (primary key) instead of SyncLogId
                var sync = await _context.SyncLogs
                    .Where(s => s.Id == syncLogId)
                    .IgnoreQueryFilters()  // Ignore soft delete filter to ensure we get the entity
                    .FirstOrDefaultAsync();

                if (sync == null)
                {
                    _logger.LogWarning("Sync log {SyncLogId} not found", syncLogId);
                    return false;
                }

                if (sync.IsDeleted)
                {
                    _logger.LogWarning("Sync log {SyncLogId} is deleted", syncLogId);
                    return false;
                }

                if (sync.Status != "IN_PROGRESS")
                {
                    _logger.LogWarning("Sync log {SyncLogId} is not in progress (current status: {Status})",
                        syncLogId, sync.Status);
                    return false;
                }

                // Mark as failed
                sync.Status = "FAILED";
                sync.SyncCompletedAt = DateTime.UtcNow;

                // Calculate duration - cap at 23:59:59 for SQL time type compatibility
                var duration = sync.SyncCompletedAt.Value - sync.SyncStartedAt;
                if (duration.TotalHours >= 24)
                {
                    // For syncs longer than 24 hours, set to null (SQL time type limitation)
                    sync.Duration = null;
                }
                else
                {
                    sync.Duration = duration;
                }

                sync.ErrorMessage = "Sync manually marked as failed by admin";
                sync.UpdatedAt = DateTime.UtcNow;  // Explicitly set UpdatedAt

                // Explicitly mark the entity as modified
                _context.Entry(sync).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                await _context.SaveChangesAsync();

                _logger.LogWarning("Sync log {SyncLogId} manually marked as failed by admin", syncLogId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking sync {SyncLogId} as failed. Inner exception: {InnerException}",
                    syncLogId, ex.InnerException?.Message);
                return false;
            }
        }

        // Helper methods for executing raw SQL
        private async Task<T> ExecuteScalarQueryAsync<T>(string sql, SqlParameter[]? parameters = null)
        {
            var connection = _context.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 30;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? default! : (T)Convert.ChangeType(result, typeof(T));
        }

        private async Task<List<T>> ExecuteQueryAsync<T>(string sql) where T : new()
        {
            var connection = _context.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 30;

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            var results = new List<T>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var obj = new T();
                var properties = typeof(T).GetProperties();

                foreach (var prop in properties)
                {
                    try
                    {
                        var ordinal = reader.GetOrdinal(prop.Name);
                        if (!reader.IsDBNull(ordinal))
                        {
                            var value = reader.GetValue(ordinal);
                            prop.SetValue(obj, value);
                        }
                    }
                    catch
                    {
                        // Skip if column not found or conversion fails
                    }
                }

                results.Add(obj);
            }

            return results;
        }

        private async Task ExecuteNonQueryAsync(string sql, SqlParameter[]? parameters = null)
        {
            var connection = _context.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 30;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }
    }
}
