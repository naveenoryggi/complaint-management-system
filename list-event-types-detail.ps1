$token = Get-Content ".working-token" -Raw

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/event-types?includeInactive=true" `
    -Method GET `
    -Headers @{Authorization="Bearer $token"} `
    -ContentType "application/json"

Write-Host "All Event Types in Database:" -ForegroundColor Cyan
$response | Select-Object Code, Name, EntityType, IsActive | Format-Table -AutoSize

Write-Host "`nEvent Codes:" -ForegroundColor Yellow
$response | ForEach-Object { Write-Host "  - $($_.code)" -ForegroundColor White }
