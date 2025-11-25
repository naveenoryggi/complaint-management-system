$connectionString = 'Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;'

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Database connected successfully" -ForegroundColor Green

    # Get total complaints count
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT COUNT(*) as Count FROM Complaints"
    $result = $command.ExecuteScalar()
    Write-Host "Total Complaints: $result" -ForegroundColor Cyan

    # Get status distribution
    $command.CommandText = "SELECT Status, COUNT(*) as Count FROM Complaints WHERE IsDeleted = 0 GROUP BY Status"
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null

    Write-Host "`nStatus Distribution:" -ForegroundColor Yellow
    foreach ($row in $dataset.Tables[0].Rows) {
        Write-Host "  $($row.Status): $($row.Count)"
    }

    $connection.Close()
    Write-Host "`nDatabase connection test completed successfully" -ForegroundColor Green
    exit 0
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
