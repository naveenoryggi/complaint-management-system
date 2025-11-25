# Execute SQL to create event types

Write-Host "Creating Event Types via SQL..." -ForegroundColor Cyan

try {
    sqlcmd -S "PRANA-ASUS\SQLEXPRESS" -d "ComplaintManagementDb" -i "create-event-types.sql" -o "event-types-creation-output.txt"

    Write-Host "`nSQL execution complete. Output:" -ForegroundColor Green
    Get-Content "event-types-creation-output.txt"

    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Verifying Event Types Created" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    $query = "SELECT Code, Name, EntityType, Category FROM EventTypes WHERE IsDeleted = 0 ORDER BY Category, Code"
    $eventTypes = Invoke-Sqlcmd -ServerInstance "PRANA-ASUS\SQLEXPRESS" -Database "ComplaintManagementDb" -Query $query

    Write-Host "`nTotal Event Types: $($eventTypes.Count)" -ForegroundColor Green
    $eventTypes | Format-Table -AutoSize

    Write-Host "`nSUCCESS: Event types created!" -ForegroundColor Green

} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
