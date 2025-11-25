using System;
using System.Collections.Generic;

namespace ComplaintManagement.Application.DTOs.Sync
{
    public class SqlHealthCheckDto
    {
        public int BlockedProcessCount { get; set; }
        public int LongRunningQueryCount { get; set; }
        public int OpenTransactionCount { get; set; }
        public int StuckSyncCount { get; set; }
        public DateTime CheckTime { get; set; }
        public string OverallStatus { get; set; } = "Healthy"; // Healthy, Warning, Critical
    }

    public class BlockedProcessDto
    {
        public int BlockingSessionId { get; set; }
        public int BlockedSessionId { get; set; }
        public string BlockingQuery { get; set; } = string.Empty;
        public string BlockedQuery { get; set; } = string.Empty;
        public string BlockingUser { get; set; } = string.Empty;
        public string BlockedUser { get; set; } = string.Empty;
        public string WaitType { get; set; } = string.Empty;
        public int WaitTimeSeconds { get; set; }
        public string BlockedStatus { get; set; } = string.Empty;
        public string BlockingStatus { get; set; } = string.Empty;
    }

    public class LongRunningQueryDto
    {
        public int SessionId { get; set; }
        public string LoginName { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public int ElapsedTimeSeconds { get; set; }
        public string QueryText { get; set; } = string.Empty;
        public int? BlockingSessionId { get; set; }
        public string WaitType { get; set; } = string.Empty;
        public long Reads { get; set; }
        public long Writes { get; set; }
    }

    public class OpenTransactionDto
    {
        public int SessionId { get; set; }
        public string LoginName { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public long TransactionId { get; set; }
        public string TransactionName { get; set; } = string.Empty;
        public DateTime BeginTime { get; set; }
        public int DurationSeconds { get; set; }
        public string TransactionState { get; set; } = string.Empty;
        public string QueryText { get; set; } = string.Empty;
    }

    public class SqlDiagnosticsResponse
    {
        public SqlHealthCheckDto HealthCheck { get; set; } = new();
        public List<BlockedProcessDto> BlockedProcesses { get; set; } = new();
        public List<LongRunningQueryDto> LongRunningQueries { get; set; } = new();
        public List<OpenTransactionDto> OpenTransactions { get; set; } = new();
    }

    public class KillSessionRequest
    {
        public int SessionId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class KillSessionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int SessionId { get; set; }
    }
}
