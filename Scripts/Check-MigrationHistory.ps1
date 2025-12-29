# Check migration history in database
$server = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
$database = "ComplaintManagementDB"
# Use SQL Server authentication (same as installed API)
$connStr = "Server=$server;Database=$database;User Id=sa;Password=admin@123;TrustServerCertificate=True;"

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
    $conn.Open()
    Write-Host "Connected to $database" -ForegroundColor Green

    $cmd = $conn.CreateCommand()

    # Check if migration history table exists
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory'"
    $exists = $cmd.ExecuteScalar()

    if ($exists -gt 0) {
        Write-Host "`nMigration History:" -ForegroundColor Cyan
        $cmd.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId"
        $reader = $cmd.ExecuteReader()
        while ($reader.Read()) {
            Write-Host "  - $($reader['MigrationId'])" -ForegroundColor White
        }
        $reader.Close()
    } else {
        Write-Host "No __EFMigrationsHistory table found" -ForegroundColor Yellow
    }

    # List all tables
    Write-Host "`nTables in database:" -ForegroundColor Cyan
    $cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
    $reader = $cmd.ExecuteReader()
    $count = 0
    while ($reader.Read()) {
        Write-Host "  - $($reader['TABLE_NAME'])" -ForegroundColor White
        $count++
    }
    $reader.Close()
    Write-Host "Total: $count tables" -ForegroundColor Gray

    $conn.Close()
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
