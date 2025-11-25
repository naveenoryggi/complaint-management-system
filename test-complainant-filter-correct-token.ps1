# Test API with correct complainant token
$token = Get-Content ".complainant-token" -Raw
$token = $token.Trim()

Write-Host "=== Testing API with Complainant Token ===" -ForegroundColor Cyan
Write-Host ""

# Test: Get complaints as complainant
$url = "http://localhost:5000/api/complaints?page=1" + "&" + "pageSize=20"
Write-Host "Fetching complaints as complainant user..." -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri $url -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

Write-Host "Total complaints visible to complainant: $($response.data.totalCount)" -ForegroundColor $(if ($response.data.totalCount -le 50) { "Green" } else { "Red" })
Write-Host "Items returned: $($response.data.items.Count)" -ForegroundColor White
Write-Host ""

if ($response.data.totalCount -gt 0) {
    Write-Host "First 10 complaints:" -ForegroundColor Yellow
    $response.data.items | Select-Object -First 10 | ForEach-Object {
        Write-Host "  - $($_.complaintNumber): $($_.title) (Complainant: $($_.complainantName))"
    }
} else {
    Write-Host "No complaints visible to this user." -ForegroundColor Red
}

Write-Host ""
if ($response.data.totalCount -le 20 -and $response.data.totalCount -gt 0) {
    Write-Host "SUCCESS: Complainant sees only their own complaints!" -ForegroundColor Green
    Write-Host "Expected: ~10-20 complaints" -ForegroundColor Green
    Write-Host "Actual: $($response.data.totalCount) complaints" -ForegroundColor Green
} elseif ($response.data.totalCount -gt 100) {
    Write-Host "FAILED: Complainant sees ALL complaints!" -ForegroundColor Red
    Write-Host "Expected: ~10-20 complaints" -ForegroundColor Red
    Write-Host "Actual: $($response.data.totalCount) complaints" -ForegroundColor Red
} else {
    Write-Host "PARTIAL: More complaints than expected but not all" -ForegroundColor Yellow
    Write-Host "Expected: ~10-20 complaints" -ForegroundColor Yellow
    Write-Host "Actual: $($response.data.totalCount) complaints" -ForegroundColor Yellow
}
