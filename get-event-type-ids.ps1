# Get Event Type IDs for linking notification rules

$token = Get-Content ".working-token" -Raw

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Getting Event Type IDs" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/event-types?includeInactive=false" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    Write-Host "`nTotal Active Event Types: $($response.Count)" -ForegroundColor Green

    # Create a hashtable for easy lookup
    $eventTypeMap = @{}
    foreach ($eventType in $response) {
        $eventTypeMap[$eventType.code] = $eventType.id
        Write-Host "`n$($eventType.code)" -ForegroundColor Yellow
        Write-Host "  ID: $($eventType.id)" -ForegroundColor White
        Write-Host "  Name: $($eventType.name)" -ForegroundColor Gray
    }

    # Save to file
    $eventTypeMap | ConvertTo-Json | Out-File "event-type-ids.json"
    Write-Host "`n================================================" -ForegroundColor Cyan
    Write-Host "Event Type IDs saved to: event-type-ids.json" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Cyan

    # Show key event types for auto-response
    Write-Host "`nKey Event Types for Auto-Response:" -ForegroundColor Yellow
    $keyEvents = @("COMPLAINT_CREATED", "COMPLAINT_ASSIGNED", "COMPLAINT_STATUS_CHANGED",
                   "COMPLAINT_ESCALATED", "COMPLAINT_RESOLVED", "COMPLAINT_CLOSED",
                   "COMMENT_ADDED", "SLA_WARNING", "SLA_BREACHED")

    foreach ($eventCode in $keyEvents) {
        if ($eventTypeMap.ContainsKey($eventCode)) {
            Write-Host "  $eventCode : $($eventTypeMap[$eventCode])" -ForegroundColor Green
        } else {
            Write-Host "  $eventCode : NOT FOUND" -ForegroundColor Red
        }
    }

} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
