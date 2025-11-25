# Check admin user roles
$connectionString = "Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected to database successfully" -ForegroundColor Green
    Write-Host ""

    # Check admin user's roles
    Write-Host "=== ADMIN USER ROLES ===" -ForegroundColor Yellow
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
SELECT u.Id, u.Email, u.FirstName, u.LastName,
       r.Name as RoleName, r.Code as RoleCode,
       ucr.IsPrimary, ucr.IsActive
FROM Users u
LEFT JOIN UserComplaintRoles ucr ON u.Id = ucr.UserId AND ucr.IsDeleted = 0
LEFT JOIN ComplaintRoles r ON ucr.ComplaintRoleId = r.Id
WHERE u.Email = 'admin@complaintmanagement.com'
"@
    $reader = $cmd.ExecuteReader()

    $hasRoles = $false
    while ($reader.Read()) {
        if ($reader['RoleName'] -ne [DBNull]::Value) {
            $hasRoles = $true
            Write-Host "Role: $($reader['RoleName']) ($($reader['RoleCode']))" -ForegroundColor Green
            Write-Host "  Primary: $($reader['IsPrimary'])" -ForegroundColor White
            Write-Host "  Active: $($reader['IsActive'])" -ForegroundColor White
        }
    }
    $reader.Close()

    if (-not $hasRoles) {
        Write-Host "NO ROLES ASSIGNED TO ADMIN USER!" -ForegroundColor Red
        Write-Host ""

        # Get the ADMIN role
        Write-Host "=== FINDING ADMIN ROLE ===" -ForegroundColor Yellow
        $cmd2 = $connection.CreateCommand()
        $cmd2.CommandText = "SELECT Id, Name, Code FROM ComplaintRoles WHERE Code = 'ADMIN' AND IsDeleted = 0"
        $reader2 = $cmd2.ExecuteReader()

        if ($reader2.Read()) {
            $adminRoleId = $reader2['Id']
            $adminRoleName = $reader2['Name']
            Write-Host "Found ADMIN role: $adminRoleName ($adminRoleId)" -ForegroundColor Green
            $reader2.Close()

            # Assign ADMIN role to admin user
            Write-Host ""
            Write-Host "=== ASSIGNING ADMIN ROLE ===" -ForegroundColor Yellow
            $cmd3 = $connection.CreateCommand()
            $cmd3.CommandText = @"
INSERT INTO UserComplaintRoles (Id, UserId, ComplaintRoleId, EffectiveFrom, IsPrimary, IsActive, Notes, CreatedAt, IsDeleted)
SELECT NEWID(),
       u.Id,
       @AdminRoleId,
       GETUTCDATE(),
       1,
       1,
       'Restored admin role',
       GETUTCDATE(),
       0
FROM Users u
WHERE u.Email = 'admin@complaintmanagement.com'
  AND NOT EXISTS (
      SELECT 1 FROM UserComplaintRoles ucr
      WHERE ucr.UserId = u.Id
        AND ucr.ComplaintRoleId = @AdminRoleId
        AND ucr.IsDeleted = 0
  )
"@
            $cmd3.Parameters.AddWithValue("@AdminRoleId", $adminRoleId) | Out-Null
            $rowsAffected = $cmd3.ExecuteNonQuery()

            if ($rowsAffected -gt 0) {
                Write-Host "ADMIN role assigned successfully!" -ForegroundColor Green
            } else {
                Write-Host "Role already assigned or error occurred" -ForegroundColor Yellow
            }
        } else {
            Write-Host "ADMIN role not found in database!" -ForegroundColor Red
            $reader2.Close()
        }
    } else {
        Write-Host ""
        Write-Host "Admin user has roles assigned" -ForegroundColor Green
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
