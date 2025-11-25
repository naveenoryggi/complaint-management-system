# Assign admin role to admin user
$connectionString = "Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected to database successfully" -ForegroundColor Green
    Write-Host ""

    # List all available roles
    Write-Host "=== AVAILABLE COMPLAINT ROLES ===" -ForegroundColor Yellow
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT Id, Name, Code, RoleType, EscalationLevel FROM ComplaintRoles WHERE IsDeleted = 0 ORDER BY EscalationLevel DESC"
    $reader = $cmd.ExecuteReader()

    $roles = @()
    while ($reader.Read()) {
        $role = @{
            Id = $reader['Id']
            Name = $reader['Name']
            Code = $reader['Code']
            RoleType = $reader['RoleType']
            Level = $reader['EscalationLevel']
        }
        $roles += $role
        Write-Host "$($role.Name) ($($role.Code)) - Level $($role.Level)" -ForegroundColor Cyan
    }
    $reader.Close()

    if ($roles.Count -eq 0) {
        Write-Host "NO ROLES FOUND!" -ForegroundColor Red
        $connection.Close()
        return
    }

    # Find highest level role (should be admin/super admin)
    $highestRole = $roles | Sort-Object -Property Level -Descending | Select-Object -First 1

    Write-Host ""
    Write-Host "=== ASSIGNING HIGHEST ROLE TO ADMIN USER ===" -ForegroundColor Yellow
    Write-Host "Role: $($highestRole.Name) ($($highestRole.Code))" -ForegroundColor Green

    # Get admin user ID
    $cmd2 = $connection.CreateCommand()
    $cmd2.CommandText = "SELECT Id FROM Users WHERE Email = 'admin@complaintmanagement.com'"
    $adminUserId = $cmd2.ExecuteScalar()

    if ($adminUserId -ne $null) {
        # Assign role
        $cmd3 = $connection.CreateCommand()
        $cmd3.CommandText = @"
INSERT INTO UserComplaintRoles (Id, UserId, ComplaintRoleId, EffectiveFrom, IsPrimary, IsActive, Notes, CreatedAt, IsDeleted)
VALUES (NEWID(), @UserId, @RoleId, GETUTCDATE(), 1, 1, 'Restored admin role', GETUTCDATE(), 0)
"@
        $cmd3.Parameters.AddWithValue("@UserId", $adminUserId) | Out-Null
        $cmd3.Parameters.AddWithValue("@RoleId", $highestRole.Id) | Out-Null
        $rowsAffected = $cmd3.ExecuteNonQuery()

        if ($rowsAffected -gt 0) {
            Write-Host ""
            Write-Host "ROLE ASSIGNED SUCCESSFULLY!" -ForegroundColor Green
            Write-Host "Admin user now has the $($highestRole.Name) role" -ForegroundColor Green
        } else {
            Write-Host "Failed to assign role" -ForegroundColor Red
        }
    } else {
        Write-Host "Admin user not found!" -ForegroundColor Red
    }

    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}
