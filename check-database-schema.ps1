$token = Get-Content '.test-token' -Raw
$headers = @{ 'Authorization' = "Bearer $token" }

try {
    # Get a sample complaint to see the actual format
    $response = Invoke-RestMethod -Uri 'http://localhost:5058/api/Complaints' -Headers $headers -Method Get

    if ($response.data) {
        $complaints = $response.data
    } else {
        $complaints = $response
    }

    if ($complaints -and $complaints.Count -gt 0) {
        Write-Host "Sample Complaint Numbers from database:"
        $complaints | Select-Object -First 10 | ForEach-Object {
            Write-Host "  - $($_.complaintNumber)"
        }
    } else {
        Write-Host "No complaints in database"
    }
} catch {
    Write-Host "Error: $_"
}
