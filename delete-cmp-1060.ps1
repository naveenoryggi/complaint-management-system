$token = Get-Content '.test-token' -Raw
$headers = @{ 'Authorization' = "Bearer $token" }

try {
    $complaints = Invoke-RestMethod -Uri 'http://localhost:5058/api/Complaints' -Headers $headers -Method Get
    $complaint = $complaints | Where-Object { $_.complaintNumber -eq 'CMP-2025-1060' }

    if ($complaint) {
        Write-Host "Found CMP-2025-1060 with ID: $($complaint.id)"
        Invoke-RestMethod -Uri "http://localhost:5058/api/Complaints/$($complaint.id)" -Headers $headers -Method Delete
        Write-Host 'Deleted CMP-2025-1060 successfully'
    } else {
        Write-Host 'CMP-2025-1060 not found in database'
    }
} catch {
    Write-Host "Error: $_"
}
