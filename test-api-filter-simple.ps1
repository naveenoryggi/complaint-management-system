# Simple API filter test
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$complainantId = "fd0073b8-fc95-4a49-867c-6ffb38b7d177"

Write-Host "Testing complainant API filter..." -ForegroundColor Cyan
Write-Host "Complainant ID: $complainantId"
Write-Host ""

# Test with filter
$url1 = "http://localhost:5000/api/complaints?page=1" + "&" + "pageSize=5" + "&" + "complainantId=$complainantId"
Write-Host "URL with filter: $url1"
$response1 = Invoke-RestMethod -Uri $url1 -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"
Write-Host "Total with filter: $($response1.data.totalCount)"
Write-Host ""

# Test without filter
$url2 = "http://localhost:5000/api/complaints?page=1" + "&" + "pageSize=5"
Write-Host "URL without filter: $url2"
$response2 = Invoke-RestMethod -Uri $url2 -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"
Write-Host "Total without filter: $($response2.data.totalCount)"
Write-Host ""

if ($response1.data.totalCount -eq $response2.data.totalCount) {
    Write-Host "PROBLEM: Filter NOT working - both return same count!" -ForegroundColor Red
} else {
    Write-Host "SUCCESS: Filter working - different counts!" -ForegroundColor Green
}
Write-Host ""

# Check actual ownership
Write-Host "Checking actual complaint ownership..." -ForegroundColor Yellow
$allUrl = "http://localhost:5000/api/complaints?page=1" + "&" + "pageSize=1093"
$allResponse = Invoke-RestMethod -Uri $allUrl -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

$owned = $allResponse.data.items | Where-Object { $_.complainantId -eq $complainantId }
Write-Host "Complaints actually owned by this user: $($owned.Count)" -ForegroundColor White

if ($owned.Count -gt 0) {
    Write-Host "Owned complaint numbers:"
    $owned | ForEach-Object { Write-Host "  - $($_.complaintNumber)" }
} else {
    Write-Host "NO complaints owned by this user in database!" -ForegroundColor Red
    Write-Host "Root cause: Test data has wrong complainant IDs" -ForegroundColor Yellow
}
