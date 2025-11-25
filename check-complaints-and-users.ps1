$token = (Get-Content ".working-token" -Raw).Trim()
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "=== Checking All Users ==="
$users = Invoke-RestMethod -Uri "http://localhost:5000/api/users" -Headers $headers -Method Get
$users | Select-Object -First 10 userName, email, roleName | Format-Table -AutoSize

Write-Host "`n=== Checking Complaints with Assigned Technicians ==="
$complaints = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" -Headers $headers -Method Get
$assignedComplaints = $complaints | Where-Object { $null -ne $_.assignedTechnicianId }

Write-Host "Found $($assignedComplaints.Count) complaints with assigned technicians"
$assignedComplaints | Select-Object -First 5 id, title, assignedTechnicianName, assignedTechnicianId | Format-Table -AutoSize

# Find users matching these technician IDs
if ($assignedComplaints.Count -gt 0) {
    Write-Host "`n=== Finding Matching Users ==="
    $technicianIds = $assignedComplaints | Select-Object -ExpandProperty assignedTechnicianId -Unique
    foreach ($techId in $technicianIds) {
        $user = $users | Where-Object { $_.id -eq $techId }
        if ($user) {
            Write-Host "User ID: $($user.id)"
            Write-Host "Username: $($user.userName)"
            Write-Host "Email: $($user.email)"
            Write-Host "Role: $($user.roleName)"
            Write-Host "---"
        }
    }
}
