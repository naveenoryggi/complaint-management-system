# Run migration script directly
$server = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
$database = "ComplaintManagementDB"

# Create database first
$masterConn = "Server=$server;Database=master;Integrated Security=True;TrustServerCertificate=True;"
$dbConn = "Server=$server;Database=$database;Integrated Security=True;TrustServerCertificate=True;"

try {
    # Create database
    Write-Host "Creating database..." -ForegroundColor Yellow
    $conn = New-Object System.Data.SqlClient.SqlConnection($masterConn)
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '$database') CREATE DATABASE [$database]"
    $cmd.ExecuteNonQuery() | Out-Null
    $conn.Close()
    Write-Host "Database created!" -ForegroundColor Green

    # Run migration script
    Write-Host "Running migration script..." -ForegroundColor Yellow
    $scriptPath = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.API\migration_script.sql"

    $conn = New-Object System.Data.SqlClient.SqlConnection($dbConn)
    $conn.Open()

    # Read and execute the SQL script
    $script = Get-Content $scriptPath -Raw

    # Split by GO statements
    $batches = $script -split '\r?\nGO\r?\n'

    foreach ($batch in $batches) {
        $batch = $batch.Trim()
        if ($batch -ne '') {
            try {
                $cmd = $conn.CreateCommand()
                $cmd.CommandTimeout = 300
                $cmd.CommandText = $batch
                $cmd.ExecuteNonQuery() | Out-Null
            } catch {
                Write-Host "Error in batch: $($_.Exception.Message)" -ForegroundColor Red
                Write-Host "Batch (first 200 chars): $($batch.Substring(0, [Math]::Min(200, $batch.Length)))" -ForegroundColor Gray
            }
        }
    }

    $conn.Close()
    Write-Host "Migration script completed!" -ForegroundColor Green

    # Verify tables
    Write-Host "`nVerifying tables..." -ForegroundColor Yellow
    $conn = New-Object System.Data.SqlClient.SqlConnection($dbConn)
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
    $count = $cmd.ExecuteScalar()
    Write-Host "Total tables created: $count" -ForegroundColor Cyan
    $conn.Close()

} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
