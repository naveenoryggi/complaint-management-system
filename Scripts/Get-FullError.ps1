# Get the full error details
$events = Get-WinEvent -LogName Application -MaxEvents 100 |
    Where-Object { $_.Message -match 'ComplaintManagement|fail|error|exception' -and $_.TimeCreated -gt (Get-Date).AddMinutes(-30) }

foreach ($event in $events | Select-Object -First 5) {
    Write-Host "`n==================" -ForegroundColor Yellow
    Write-Host "Time: $($event.TimeCreated)" -ForegroundColor Cyan
    Write-Host "Message (full):" -ForegroundColor Green
    Write-Host $event.Message
    Write-Host "==================" -ForegroundColor Yellow
}
