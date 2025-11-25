# Delete all old complaints except the 5 new test complaints
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

Write-Host "=== Deleting Old Complaints ===" -ForegroundColor Cyan
Write-Host "Keeping only the 5 new test complaints (CMP-2025-1143 to CMP-2025-1147)" -ForegroundColor Yellow
Write-Host ""

# Get all complaints
$url = "http://localhost:5000/api/complaints?page=1" + "&" + "pageSize=2000"
Write-Host "Fetching all complaints..." -ForegroundColor White
$response = Invoke-RestMethod -Uri $url -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

Write-Host "Total complaints in database: $($response.data.totalCount)" -ForegroundColor White
Write-Host ""

# Filter to keep only the 5 new test complaints
$keepComplaintNumbers = @("CMP-2025-1143", "CMP-2025-1144", "CMP-2025-1145", "CMP-2025-1146", "CMP-2025-1147")
$complaintsToDelete = $response.data.items | Where-Object { $_.complaintNumber -notin $keepComplaintNumbers }

Write-Host "Complaints to keep: $($keepComplaintNumbers.Count)" -ForegroundColor Green
Write-Host "Complaints to delete: $($complaintsToDelete.Count)" -ForegroundColor Yellow
Write-Host ""

if ($complaintsToDelete.Count -eq 0) {
    Write-Host "No complaints to delete!" -ForegroundColor Green
    exit 0
}

# Confirm deletion
Write-Host "This will DELETE $($complaintsToDelete.Count) complaints!" -ForegroundColor Red
Write-Host "Keeping only these 5 complaints:" -ForegroundColor Green
$keepComplaintNumbers | ForEach-Object { Write-Host "  - $_" -ForegroundColor Green }
Write-Host ""

$deletedCount = 0
$failedCount = 0
$batchSize = 50
$totalToDelete = $complaintsToDelete.Count

Write-Host "Starting deletion in batches of $batchSize..." -ForegroundColor Yellow
Write-Host ""

for ($i = 0; $i -lt $totalToDelete; $i++) {
    $complaint = $complaintsToDelete[$i]
    $progress = [math]::Round(($i / $totalToDelete) * 100, 1)

    if ($i % 10 -eq 0) {
        Write-Host "Progress: $progress% ($i/$totalToDelete) - Deleting $($complaint.complaintNumber)..." -ForegroundColor Cyan
    }

    try {
        $deleteUrl = "http://localhost:5000/api/complaints/$($complaint.id)"
        Invoke-RestMethod -Uri $deleteUrl -Method DELETE -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json" | Out-Null
        $deletedCount++
    } catch {
        Write-Host "  FAILED: $($complaint.complaintNumber) - $($_.Exception.Message)" -ForegroundColor Red
        $failedCount++
    }

    # Small delay to avoid overwhelming the API
    if ($i % $batchSize -eq 0 -and $i -gt 0) {
        Start-Sleep -Milliseconds 500
    }
}

Write-Host ""
Write-Host "=== Deletion Complete ===" -ForegroundColor Cyan
Write-Host "  Deleted: $deletedCount complaints" -ForegroundColor Green
Write-Host "  Failed: $failedCount complaints" -ForegroundColor $(if ($failedCount -gt 0) { "Red" } else { "White" })
Write-Host ""

# Verify final state
Write-Host "Verifying final state..." -ForegroundColor Cyan
$verifyResponse = Invoke-RestMethod -Uri $url -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"
Write-Host "Total complaints remaining: $($verifyResponse.data.totalCount)" -ForegroundColor $(if ($verifyResponse.data.totalCount -eq 5) { "Green" } else { "Yellow" })

if ($verifyResponse.data.totalCount -eq 5) {
    Write-Host "SUCCESS: Database cleaned! Only 5 test complaints remain." -ForegroundColor Green
} else {
    Write-Host "WARNING: Expected 5 complaints, but found $($verifyResponse.data.totalCount)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Remaining complaints:" -ForegroundColor White
$verifyResponse.data.items | ForEach-Object {
    Write-Host "  - $($_.complaintNumber): $($_.title) (Complainant: $($_.complainantName))" -ForegroundColor White
}
