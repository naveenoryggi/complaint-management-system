# Delete all users from ComplaintManagementDb
$connectionString = "Server=DESKTOP-2SARLEL\SQLEXPRESS;Database=ComplaintManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected to database successfully" -ForegroundColor Green

    # Delete UserComplaintRoles first (foreign key dependency)
    $cmd1 = $connection.CreateCommand()
    $cmd1.CommandText = "DELETE FROM UserComplaintRoles"
    $rows1 = $cmd1.ExecuteNonQuery()
    Write-Host "Deleted $rows1 rows from UserComplaintRoles" -ForegroundColor Yellow

    # Delete Users
    $cmd2 = $connection.CreateCommand()
    $cmd2.CommandText = "DELETE FROM Users"
    $rows2 = $cmd2.ExecuteNonQuery()
    Write-Host "Deleted $rows2 rows from Users" -ForegroundColor Yellow

    # Reset identity
    $cmd3 = $connection.CreateCommand()
    $cmd3.CommandText = "DBCC CHECKIDENT ('Users', RESEED, 0)"
    $cmd3.ExecuteNonQuery() | Out-Null
    Write-Host "Reset identity seed for Users table" -ForegroundColor Yellow

    Write-Host "`nAll users deleted successfully!" -ForegroundColor Green

    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}
