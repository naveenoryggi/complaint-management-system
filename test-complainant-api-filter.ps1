# Test complainant API filtering directly
$token = Get-Content ".test-token" -Raw | ForEach-Object { $_.Trim() }

# Complainant ID for nav_nainital@yahoo.com
$complainantId = "fd0073b8-fc95-4a49-867c-6ffb38b7d177"

Write-Host "=== Testing Complainant API Filter ===" -ForegroundColor Cyan
Write-Host "Complainant ID: $complainantId" -ForegroundColor Yellow
Write-Host ""

try {
    # Test 1: Get complaints WITH complainantId filter
    Write-Host "Test 1: API call WITH complainantId filter" -ForegroundColor Green
    $url1 = "http://localhost:5000/api/complaints?page=1&pageSize=5&complainantId=$complainantId"
    Write-Host "URL: $url1"
    $response1 = Invoke-RestMethod -Uri $url1 -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

    Write-Host "  Total Count: $($response1.data.totalCount)" -ForegroundColor White
    Write-Host "  Items Returned: $($response1.data.items.Count)" -ForegroundColor White
    Write-Host ""

    if ($response1.data.items.Count -gt 0) {
        Write-Host "  First 3 complaints:" -ForegroundColor Yellow
        $response1.data.items | Select-Object -First 3 | ForEach-Object {
            $matchesFilter = if ($_.complainantId -eq $complainantId) { "✓ MATCH" } else { "✗ WRONG!" }
            Write-Host "    - $($_.complaintNumber): Complainant=$($_.complainantName) (ID: $($_.complainantId)) $matchesFilter" -ForegroundColor $(if ($_.complainantId -eq $complainantId) { "Green" } else { "Red" })
        }
    }

    Write-Host ""
    Write-Host ""

    # Test 2: Get complaints WITHOUT any filter (should return all)
    Write-Host "Test 2: API call WITHOUT any filter (for comparison)" -ForegroundColor Green
    $url2 = "http://localhost:5000/api/complaints?page=1&pageSize=5"
    Write-Host "URL: $url2"
    $response2 = Invoke-RestMethod -Uri $url2 -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

    Write-Host "  Total Count: $($response2.data.totalCount)" -ForegroundColor White
    Write-Host "  Items Returned: $($response2.data.items.Count)" -ForegroundColor White
    Write-Host ""

    Write-Host ""
    Write-Host "=== Analysis ===" -ForegroundColor Cyan
    if ($response1.data.totalCount -eq $response2.data.totalCount) {
        Write-Host "PROBLEM CONFIRMED: Filter is NOT working!" -ForegroundColor Red
        Write-Host "  Both queries return the same count: $($response1.data.totalCount)" -ForegroundColor Red
        Write-Host "  Expected: Filtered query should return fewer results" -ForegroundColor Yellow
    } else {
        Write-Host "Filter appears to be working!" -ForegroundColor Green
        Write-Host "  Filtered: $($response1.data.totalCount) complaints" -ForegroundColor Green
        Write-Host "  Unfiltered: $($response2.data.totalCount) complaints" -ForegroundColor Green
    }

    # Test 3: Check which complaints actually belong to this complainant
    Write-Host ""
    Write-Host "Test 3: Checking actual complaint ownership" -ForegroundColor Green
    $allComplaintsUrl = "http://localhost:5000/api/complaints?page=1&pageSize=1093"
    $allResponse = Invoke-RestMethod -Uri $allComplaintsUrl -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

    $ownedComplaints = $allResponse.data.items | Where-Object { $_.complainantId -eq $complainantId }
    Write-Host "  Actual complaints owned by complainant: $($ownedComplaints.Count)" -ForegroundColor White

    if ($ownedComplaints.Count -gt 0) {
        Write-Host "  Owned complaint numbers:" -ForegroundColor Yellow
        $ownedComplaints | ForEach-Object {
            Write-Host "    - $($_.complaintNumber)" -ForegroundColor Green
        }
    } else {
        Write-Host "  NO complaints owned by this complainant!" -ForegroundColor Red
        Write-Host "  This explains why filter returns all complaints - there are no matching records!" -ForegroundColor Yellow
    }

} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack Trace: $($_.Exception.StackTrace)" -ForegroundColor Red
}
