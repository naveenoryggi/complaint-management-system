# Check admin user in database
$connectionString = "Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected to database successfully" -ForegroundColor Green
    Write-Host ""

    # Check for admin user
    Write-Host "=== CHECKING FOR ADMIN USER ===" -ForegroundColor Yellow
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT TOP 5 Id, EmployeeCode, FirstName, LastName, Email, IsActive, IsDeleted FROM Users ORDER BY CreatedAt"
    $reader = $cmd.ExecuteReader()

    $found = $false
    while ($reader.Read()) {
        $found = $true
        Write-Host "ID: $($reader['Id'])" -ForegroundColor Cyan
        Write-Host "  Employee Code: $($reader['EmployeeCode'])" -ForegroundColor White
        Write-Host "  Name: $($reader['FirstName']) $($reader['LastName'])" -ForegroundColor White
        Write-Host "  Email: $($reader['Email'])" -ForegroundColor White
        Write-Host "  Active: $($reader['IsActive'])" -ForegroundColor White
        Write-Host "  Deleted: $($reader['IsDeleted'])" -ForegroundColor White
        Write-Host ""
    }
    $reader.Close()

    if (-not $found) {
        Write-Host "NO USERS FOUND IN DATABASE!" -ForegroundColor Red
    }

    # Count total users
    $cmd2 = $connection.CreateCommand()
    $cmd2.CommandText = "SELECT COUNT(*) as TotalUsers FROM Users WHERE IsDeleted = 0"
    $reader2 = $cmd2.ExecuteReader()
    if ($reader2.Read()) {
        Write-Host "Total Active Users: $($reader2['TotalUsers'])" -ForegroundColor Yellow
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
