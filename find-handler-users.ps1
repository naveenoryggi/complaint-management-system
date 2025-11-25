$token = (Get-Content ".working-token" -Raw).Trim()
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $users = Invoke-RestMethod -Uri "http://localhost:5000/api/users" -Headers $headers -Method Get
    $handlers = $users | Where-Object { $_.roleName -eq "Handler" -or $_.roleName -eq "Technician" }

    Write-Host "Found $($handlers.Count) handler/technician users:"
    $handlers | Select-Object -First 5 userName, email, roleName | Format-Table -AutoSize

    # Get complaints assigned to first handler
    if ($handlers.Count -gt 0) {
        $firstHandler = $handlers[0]
        Write-Host "`nChecking complaints assigned to: $($firstHandler.userName)"

        $complaints = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" -Headers $headers -Method Get
        $assignedComplaints = $complaints | Where-Object { $_.assignedTechnicianId -eq $firstHandler.id }

        Write-Host "Found $($assignedComplaints.Count) assigned complaints"
        $assignedComplaints | Select-Object -First 3 id, title, status | Format-Table -AutoSize
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
