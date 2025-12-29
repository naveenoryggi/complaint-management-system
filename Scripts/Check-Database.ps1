# Check and optionally drop database
param([switch]$Drop)

$server = ".\SQLEXPRESS"
$connStr = "Server=$server;Database=master;Integrated Security=True;TrustServerCertificate=True;"

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
    $conn.Open()
    Write-Host "Connected to SQL Server" -ForegroundColor Green

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT name FROM sys.databases WHERE name LIKE 'ComplaintManagement%'"
    $reader = $cmd.ExecuteReader()

    $databases = @()
    while ($reader.Read()) {
        $databases += $reader["name"]
    }
    $reader.Close()

    if ($databases.Count -eq 0) {
        Write-Host "No ComplaintManagement databases found" -ForegroundColor Yellow
    } else {
        Write-Host "Found databases:" -ForegroundColor Cyan
        foreach ($db in $databases) {
            Write-Host "  - $db" -ForegroundColor White

            if ($Drop) {
                Write-Host "    Dropping $db..." -ForegroundColor Yellow
                $cmd.CommandText = "ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$db]"
                $cmd.ExecuteNonQuery() | Out-Null
                Write-Host "    Dropped!" -ForegroundColor Green
            }
        }
    }

    $conn.Close()
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
