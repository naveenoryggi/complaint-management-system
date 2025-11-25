# Test Handler Role Statistics
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "INVESTIGATION 1: Handler Statistics" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Login as handler
Write-Host "Step 1: Logging in as HANDLER (naveen.chandra@oryggitech.com)..." -ForegroundColor Yellow
$handlerLogin = @{
    email = "naveen.chandra@oryggitech.com"
    password = "Naveen@12345"
} | ConvertTo-Json

try {
    $handlerResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $handlerLogin -ContentType "application/json"
    $handlerToken = $handlerResponse.data.token
    $handlerId = $handlerResponse.data.userId

    Write-Host "  ✓ Handler logged in successfully" -ForegroundColor Green
    Write-Host "  User: $($handlerResponse.data.fullName)" -ForegroundColor White
    Write-Host "  User ID: $handlerId" -ForegroundColor White
    Write-Host "  Roles: $($handlerResponse.data.roles -join ', ')" -ForegroundColor White
    Write-Host ""

    # Check complaints API to see how many are assigned to handler
    Write-Host "Step 2: Checking complaints assigned to handler via API..." -ForegroundColor Yellow
    $complaintsUrl = 'http://localhost:5000/api/complaints?page=1&pageSize=100'
    $complaintsResponse = Invoke-RestMethod -Uri $complaintsUrl -Method GET -Headers @{ "Authorization" = "Bearer $handlerToken" } -ContentType "application/json"

    Write-Host "  Total complaints handler can see: $($complaintsResponse.data.totalCount)" -ForegroundColor White
    Write-Host "  Items returned: $($complaintsResponse.data.items.Count)" -ForegroundColor White

    if ($complaintsResponse.data.items.Count -gt 0) {
        Write-Host "  Complaint Details:" -ForegroundColor Cyan
        $complaintsResponse.data.items | Select-Object -First 10 | ForEach-Object {
            Write-Host "    - $($_.complaintNumber): $($_.title) (Assigned: $($_.assignedToName))" -ForegroundColor White
        }
    } else {
        Write-Host "  ⚠ WARNING: Handler has NO complaints assigned!" -ForegroundColor Yellow
    }
    Write-Host ""

    # Get statistics for handler
    Write-Host "Step 3: Getting HANDLER statistics..." -ForegroundColor Yellow
    $handlerStats = Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/statistics" -Method GET -Headers @{ "Authorization" = "Bearer $handlerToken" } -ContentType "application/json"

    Write-Host "  Total Complaints: $($handlerStats.data.totalComplaints)" -ForegroundColor White
    Write-Host "  Active: $($handlerStats.data.activeComplaints)" -ForegroundColor White
    Write-Host "  Completed: $($handlerStats.data.completedComplaints)" -ForegroundColor White
    Write-Host "  Week: $($handlerStats.data.weekComplaints)" -ForegroundColor White
    Write-Host "  Month: $($handlerStats.data.monthComplaints)" -ForegroundColor White
    Write-Host ""

    Write-Host "  Status Breakdown:" -ForegroundColor Cyan
    $handlerStats.data.statusWidgets | Where-Object { $_.currentCount -gt 0 } | ForEach-Object {
        Write-Host "    - $($_.name): $($_.currentCount)" -ForegroundColor White
    }
    Write-Host ""

    # Validation
    Write-Host "Step 4: VALIDATION..." -ForegroundColor Yellow

    $complaintsCount = $complaintsResponse.data.totalCount
    $statsCount = $handlerStats.data.totalComplaints

    if ($complaintsCount -eq $statsCount) {
        Write-Host "  ✓ PASS: Complaints API ($complaintsCount) matches Statistics API ($statsCount)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ FAIL: Mismatch! Complaints API shows $complaintsCount but Statistics shows $statsCount" -ForegroundColor Red
    }

    if ($complaintsCount -eq 0) {
        Write-Host "  ⚠ WARNING: Handler has ZERO complaints assigned - cannot fully test filtering!" -ForegroundColor Yellow
        Write-Host "  Recommendation: Assign some complaints to this handler first." -ForegroundColor Yellow
    } else {
        Write-Host "  ✓ Handler has $complaintsCount complaints to test with" -ForegroundColor Green
    }

} catch {
    Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Details: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "INVESTIGATION 1 COMPLETE" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
