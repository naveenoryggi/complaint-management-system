# Get Event Types
$TOKEN = (Get-Content .test-token -Raw).Trim()

$headers = @{
    "Authorization" = "Bearer $TOKEN"
}

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5058/api/event-types" -Method Get -Headers $headers
    Write-Host "Event Types:" -ForegroundColor Green
    $response.data | Select-Object -First 5 | Format-Table id, name, code, isActive
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
