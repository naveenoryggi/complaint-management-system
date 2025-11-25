# Check actual complaint count in database
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

Write-Host "=== Checking Actual Database State ===" -ForegroundColor Cyan
Write-Host ""

# Get all complaints with larger page size
$url = "http://localhost:5000/api/complaints?page=1" + "&" + "pageSize=100"
Write-Host "Fetching all complaints from API..." -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri $url -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

Write-Host "Total Complaints: $($response.data.totalCount)" -ForegroundColor $(if ($response.data.totalCount -eq 5) { "Green" } else { "Red" })
Write-Host "Items Returned: $($response.data.items.Count)" -ForegroundColor White
Write-Host ""

# Count by status
$statusCounts = $response.data.items | Group-Object -Property status | Select-Object Name, Count
Write-Host "Breakdown by Status:" -ForegroundColor Yellow
$statusCounts | ForEach-Object {
    Write-Host "  $($_.Name): $($_.Count) complaints" -ForegroundColor White
}
Write-Host ""

# List all complaint numbers
Write-Host "All Complaint Numbers in Database:" -ForegroundColor Yellow
$response.data.items | Sort-Object complaintNumber | ForEach-Object {
    $color = if ($_.complaintNumber -match "CMP-2025-114[3-7]") { "Green" } else { "Red" }
    Write-Host "  - $($_.complaintNumber): $($_.title) (Status: $($_.status), Complainant: $($_.complainantName))" -ForegroundColor $color
}

Write-Host ""
if ($response.data.totalCount -ne 5) {
    Write-Host "PROBLEM: Expected 5 complaints, but found $($response.data.totalCount)!" -ForegroundColor Red
    Write-Host "Some complaints were not deleted properly." -ForegroundColor Red
} else {
    Write-Host "SUCCESS: Database has exactly 5 complaints as expected." -ForegroundColor Green
}
