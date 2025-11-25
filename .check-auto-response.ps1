$complaintId = "a86408a1-3111-4fa7-bdd2-2aa3bf5cc5b0"
$token = (Get-Content .fresh-token -Raw).Trim()
$headers = @{
    'Authorization' = "Bearer $token"
}

try {
    Write-Host "Checking communication logs for complaint: $complaintId"
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$complaintId/communications" -Headers $headers -Method Get

    Write-Host "`nCommunication Logs Found: $($response.Count)"
    $response | ConvertTo-Json -Depth 10

} catch {
    Write-Host "Error checking communications: $_"
    Write-Host $_.Exception.Message
}
