# Check for DEFAULT tenant
$connectionString = "Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected to database successfully" -ForegroundColor Green
    Write-Host ""

    # Check for DEFAULT tenant
    Write-Host "=== CHECKING FOR DEFAULT TENANT ===" -ForegroundColor Yellow
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT Id, Code, Name, IsActive, IsDeleted FROM Tenants WHERE Code = 'DEFAULT'"
    $reader = $cmd.ExecuteReader()

    $found = $false
    while ($reader.Read()) {
        $found = $true
        Write-Host "DEFAULT TENANT FOUND:" -ForegroundColor Green
        Write-Host "  ID: $($reader['Id'])" -ForegroundColor White
        Write-Host "  Code: $($reader['Code'])" -ForegroundColor White
        Write-Host "  Name: $($reader['Name'])" -ForegroundColor White
        Write-Host "  IsActive: $($reader['IsActive'])" -ForegroundColor White
        Write-Host "  IsDeleted: $($reader['IsDeleted'])" -ForegroundColor White
    }
    $reader.Close()

    if (-not $found) {
        Write-Host "DEFAULT TENANT NOT FOUND!" -ForegroundColor Red
        Write-Host ""

        # List all tenants
        Write-Host "=== ALL TENANTS IN DATABASE ===" -ForegroundColor Yellow
        $cmd2 = $connection.CreateCommand()
        $cmd2.CommandText = "SELECT Id, Code, Name, IsActive, IsDeleted FROM Tenants"
        $reader2 = $cmd2.ExecuteReader()

        $tenantCount = 0
        while ($reader2.Read()) {
            $tenantCount++
            Write-Host "Tenant #$tenantCount" -ForegroundColor Cyan
            Write-Host "  ID: $($reader2['Id'])" -ForegroundColor White
            Write-Host "  Code: $($reader2['Code'])" -ForegroundColor White
            Write-Host "  Name: $($reader2['Name'])" -ForegroundColor White
            Write-Host "  IsActive: $($reader2['IsActive'])" -ForegroundColor White
            Write-Host "  IsDeleted: $($reader2['IsDeleted'])" -ForegroundColor White
            Write-Host ""
        }
        $reader2.Close()

        if ($tenantCount -eq 0) {
            Write-Host "NO TENANTS FOUND IN DATABASE!" -ForegroundColor Red
        }
    }

    $connection.Close()
    Write-Host ""
    Write-Host "Check completed!" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}
