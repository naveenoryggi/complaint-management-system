# Test the security fix with correct port
$token = Get-Content ".complainant-token" -Raw
$token = $token.Trim()

Write-Host "=== Testing Backend Security Fix ===" -ForegroundColor Cyan
Write-Host "Testing as: Nav Nainital (Complainant)" -ForegroundColor Yellow
Write-Host "Backend port: 5058" -ForegroundColor Yellow
Write-Host ""

# Test: Get complaints as complainant
$url = "http://localhost:5058/api/complaints?page=1" + "&" + "pageSize=20"
Write-Host "API URL: $url" -ForegroundColor White
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri $url -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

    Write-Host "Total complaints visible: $($response.data.totalCount)" -ForegroundColor $(if ($response.data.totalCount -le 50) { "Green" } else { "Red" })
    Write-Host ""

    if ($response.data.totalCount -le 50) {
        Write-Host "SUCCESS: Complainant sees limited complaints!" -ForegroundColor Green
        Write-Host "Security fix is working! Backend is enforcing role-based filtering." -ForegroundColor Green
    } else {
        Write-Host "FAILED: Complainant still sees too many complaints ($($response.data.totalCount))" -ForegroundColor Red
        Write-Host "Security vulnerability still exists!" -ForegroundColor Red
    }

    Write-Host ""
    if ($response.data.items.Count -gt 0) {
        Write-Host "First 5 complaints:" -ForegroundColor Yellow
        $response.data.items | Select-Object -First 5 | ForEach-Object {
            Write-Host "  - $($_.complaintNumber): $($_.title)" -ForegroundColor White
        }
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
