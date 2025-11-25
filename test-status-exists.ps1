$token = Get-Content .test-token -Raw
$token = $token.Trim()

$response = Invoke-RestMethod -Uri http://localhost:5000/api/complaintstatusmaster -Headers @{Authorization="Bearer $token"}

$escalated = $response.data | Where-Object { $_.name -eq "Escalated" }

if ($escalated) {
    Write-Host "FOUND: Escalated status exists"
    $escalated.name
} else {
    Write-Host "NOT FOUND: Escalated status missing"
    Write-Host "Available:"
    $response.data | ForEach-Object { $_.name }
}
