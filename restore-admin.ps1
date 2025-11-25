# Restore admin user
$connectionString = "Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected to database successfully" -ForegroundColor Green

    # Restore admin user
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
UPDATE Users
SET IsDeleted = 0,
    DeletedAt = NULL,
    IsActive = 1,
    UpdatedAt = GETUTCDATE()
WHERE Email = 'admin@complaintmanagement.com'
"@
    $rowsAffected = $cmd.ExecuteNonQuery()

    if ($rowsAffected -gt 0) {
        Write-Host "Admin user restored successfully! ($rowsAffected row(s) updated)" -ForegroundColor Green
    } else {
        Write-Host "No admin user found to restore" -ForegroundColor Yellow
    }

    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}
