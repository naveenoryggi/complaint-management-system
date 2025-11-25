# Check statistics API directly
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

Write-Host "=== Checking Statistics API ===" -ForegroundColor Cyan
Write-Host ""

# Check if there's a statistics endpoint
$urls = @(
    "http://localhost:5000/api/dashboard/statistics",
    "http://localhost:5000/api/statistics",
    "http://localhost:5000/api/complaints/statistics"
)

foreach ($url in $urls) {
    Write-Host "Testing: $url" -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri $url -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json" -ErrorAction Stop
        Write-Host "  SUCCESS: Endpoint found!" -ForegroundColor Green
        Write-Host "  Response: $($response | ConvertTo-Json -Depth 5)" -ForegroundColor White
        Write-Host ""
    } catch {
        if ($_.Exception.Response.StatusCode -eq 404) {
            Write-Host "  Not Found (404)" -ForegroundColor Gray
        } else {
            Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    Write-Host ""
}

# Also check actual complaint counts by status
Write-Host "Actual Complaint Counts from API:" -ForegroundColor Cyan
$allUrl = "http://localhost:5000/api/complaints?page=1" + "&" + "pageSize=100"
$allResponse = Invoke-RestMethod -Uri $allUrl -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

$statusCounts = $allResponse.data.items | Group-Object -Property status
Write-Host "Total Complaints: $($allResponse.data.totalCount)" -ForegroundColor White
foreach ($statusGroup in $statusCounts) {
    Write-Host "  $($statusGroup.Name): $($statusGroup.Count)" -ForegroundColor White
}
