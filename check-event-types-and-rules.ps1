# Check Event Types and Notification Rules

$token = Get-Content ".working-token" -Raw

# Check Event Types
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Event Types" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

try {
    $eventTypesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/event-types" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($eventTypesResponse.isSuccess) {
        Write-Host "`nTotal Event Types: $($eventTypesResponse.data.Count)" -ForegroundColor Green

        $eventTypesResponse.data | Select-Object Id, Code, Name, IsActive | Format-Table -AutoSize

        $complaintCreated = $eventTypesResponse.data | Where-Object { $_.code -eq "COMPLAINT_CREATED" }
        if ($complaintCreated) {
            Write-Host "`nCOMPLAINT_CREATED Event:" -ForegroundColor Green
            Write-Host "  ID: $($complaintCreated.id)" -ForegroundColor White
            Write-Host "  Code: $($complaintCreated.code)" -ForegroundColor White
            Write-Host "  Name: $($complaintCreated.name)" -ForegroundColor White
            Write-Host "  Is Active: $($complaintCreated.isActive)" -ForegroundColor $(if ($complaintCreated.isActive) { "Green" } else { "Red" })
        } else {
            Write-Host "`nCOMPLAINT_CREATED event not found!" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Check Notification Rules
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Event Communication Rules" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

try {
    $rulesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/event-communication-rules" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($rulesResponse.isSuccess) {
        Write-Host "`nTotal Rules: $($rulesResponse.data.Count)" -ForegroundColor Green

        Write-Host "`nAll Rules:" -ForegroundColor Yellow
        $rulesResponse.data | Select-Object @{Name='EventCode';Expression={$_.eventType.code}}, RuleName, IsActive, Priority, @{Name='TemplateName';Expression={$_.template.name}} | Format-Table -AutoSize

        # Group by event type
        Write-Host "`nRules by Event Type:" -ForegroundColor Yellow
        $rulesResponse.data | Group-Object -Property {$_.eventType.code} | Select-Object Name, Count | Format-Table -AutoSize
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
