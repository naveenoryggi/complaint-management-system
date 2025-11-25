# Check status masters via API
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
}

Write-Host "Getting all status masters..."
$statuses = Invoke-RestMethod -Uri "http://localhost:5000/api/complaintstatusmaster" -Headers $headers

Write-Host "Total statuses: $($statuses.data.Count)"
Write-Host ""
Write-Host "Looking for 'Escalated' status..."

$escalated = $statuses.data | Where-Object { $_.name -eq "Escalated" }

if ($escalated) {
    Write-Host "✓ Escalated status FOUND:" -ForegroundColor Green
    $escalated | Format-List id, name, code, colorCode, displayOrder, isActive, isSystem
} else {
    Write-Host "✗ Escalated status NOT FOUND!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Available statuses:"
    $statuses.data | Select-Object name, code, displayOrder | Format-Table -AutoSize
}
