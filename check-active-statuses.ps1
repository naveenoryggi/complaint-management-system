$response = Invoke-RestMethod -Uri "http://localhost:5000/api/complaintstatusmaster?isActive=true" -Method GET
Write-Host "Total active statuses: $($response.data.Count)"
Write-Host ""
$response.data | Select-Object -First 15 | ForEach-Object {
    Write-Host "- $($_.statusName) (ID: $($_.id))"
}
