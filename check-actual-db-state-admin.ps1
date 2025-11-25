# Check actual database state with admin token
Write-Host "=== Checking Actual Database State with Admin Token ===" -ForegroundColor Cyan
Write-Host ""

# Login as admin
$adminLogin = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$adminResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $adminLogin -ContentType "application/json"
$adminToken = $adminResponse.data.token

Write-Host "Logged in as Admin" -ForegroundColor Green
Write-Host ""

# Get ALL complaints with large page size
$url = "http://localhost:5000/api/complaints?page=1&pageSize=100"
$response = Invoke-RestMethod -Uri $url -Method GET -Headers @{ "Authorization" = "Bearer $adminToken" } -ContentType "application/json"

Write-Host "Total Complaints in Database: $($response.data.totalCount)" -ForegroundColor White
Write-Host "Items Returned: $($response.data.items.Count)" -ForegroundColor White
Write-Host ""

# Group by status
$statusCounts = $response.data.items | Group-Object -Property status | Select-Object Name, Count
Write-Host "Breakdown by Status:" -ForegroundColor Yellow
$statusCounts | ForEach-Object {
    Write-Host "  $($_.Name): $($_.Count) complaints" -ForegroundColor White
}
Write-Host ""

# Group by complainant
$complainantCounts = $response.data.items | Group-Object -Property complainantName | Select-Object Name, Count | Sort-Object Count -Descending
Write-Host "Breakdown by Complainant:" -ForegroundColor Yellow
$complainantCounts | ForEach-Object {
    Write-Host "  $($_.Name): $($_.Count) complaints" -ForegroundColor White
}
Write-Host ""

# Show complaint numbers for the 5 new test complaints
Write-Host "The 5 New Test Complaints (owned by Nav Nainital):" -ForegroundColor Cyan
$response.data.items | Where-Object { $_.complaintNumber -match "CMP-2025-114[3-7]" } | Sort-Object complaintNumber | ForEach-Object {
    Write-Host "  - $($_.complaintNumber): $($_.title) (Status: $($_.status))" -ForegroundColor Green
}
