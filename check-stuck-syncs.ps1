# Check for stuck syncs
$connectionString = "Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected to database successfully" -ForegroundColor Green
    Write-Host ""

    # Check for IN_PROGRESS syncs
    Write-Host "=== CHECKING FOR STUCK IN_PROGRESS SYNCS ===" -ForegroundColor Yellow
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
SELECT Id, TenantId, SyncType, Status, SyncStartedAt, SyncCompletedAt,
       DATEDIFF(MINUTE, SyncStartedAt, GETUTCDATE()) as MinutesSinceStart,
       ErrorMessage
FROM SyncLogs
WHERE Status = 'IN_PROGRESS'
ORDER BY SyncStartedAt DESC
"@
    $reader = $cmd.ExecuteReader()

    $found = $false
    while ($reader.Read()) {
        $found = $true
        Write-Host "STUCK SYNC FOUND!" -ForegroundColor Red
        Write-Host "  ID: $($reader['Id'])" -ForegroundColor White
        Write-Host "  Tenant: $($reader['TenantId'])" -ForegroundColor White
        Write-Host "  Type: $($reader['SyncType'])" -ForegroundColor White
        Write-Host "  Status: $($reader['Status'])" -ForegroundColor White
        Write-Host "  Started: $($reader['SyncStartedAt'])" -ForegroundColor White
        Write-Host "  Minutes Since Start: $($reader['MinutesSinceStart'])" -ForegroundColor Yellow
        if ($reader['ErrorMessage'] -ne [DBNull]::Value) {
            Write-Host "  Error: $($reader['ErrorMessage'])" -ForegroundColor Red
        }
        Write-Host ""
    }
    $reader.Close()

    if (-not $found) {
        Write-Host "No stuck IN_PROGRESS syncs found" -ForegroundColor Green
    }

    Write-Host ""
    # Check recent sync history
    Write-Host "=== RECENT SYNC HISTORY (Last 10) ===" -ForegroundColor Yellow
    $cmd2 = $connection.CreateCommand()
    $cmd2.CommandText = @"
SELECT TOP 10 Id, SyncType, Status, SyncStartedAt, SyncCompletedAt,
       ErrorMessage,
       CompaniesProcessed, BranchesProcessed, DepartmentsProcessed, EmployeesProcessed, UsersProcessed
FROM SyncLogs
ORDER BY SyncStartedAt DESC
"@
    $reader2 = $cmd2.ExecuteReader()

    $count = 0
    while ($reader2.Read()) {
        $count++
        Write-Host "Sync #$count" -ForegroundColor Cyan
        Write-Host "  ID: $($reader2['Id'])" -ForegroundColor White
        Write-Host "  Type: $($reader2['SyncType'])" -ForegroundColor White
        $statusColor = if ($reader2['Status'] -eq 'SUCCESS') { "Green" } elseif ($reader2['Status'] -eq 'FAILED') { "Red" } else { "Yellow" }
        Write-Host "  Status: $($reader2['Status'])" -ForegroundColor $statusColor
        Write-Host "  Started: $($reader2['SyncStartedAt'])" -ForegroundColor White
        if ($reader2['SyncCompletedAt'] -ne [DBNull]::Value) {
            Write-Host "  Completed: $($reader2['SyncCompletedAt'])" -ForegroundColor White
        }
        if ($reader2['ErrorMessage'] -ne [DBNull]::Value) {
            Write-Host "  Error: $($reader2['ErrorMessage'])" -ForegroundColor Red
        }
        Write-Host "  Processed - Companies: $($reader2['CompaniesProcessed']), Branches: $($reader2['BranchesProcessed']), Depts: $($reader2['DepartmentsProcessed']), Employees: $($reader2['EmployeesProcessed']), Users: $($reader2['UsersProcessed'])" -ForegroundColor White
        Write-Host ""
    }
    $reader2.Close()

    $connection.Close()
    Write-Host "Check completed!" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}
