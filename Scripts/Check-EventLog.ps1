# Check Windows Event Log for IIS/ASP.NET errors
$events = Get-WinEvent -LogName Application -MaxEvents 50 |
    Where-Object { $_.ProviderName -match 'IIS|ASP\.NET|\.NET Runtime' -or $_.Message -match 'ComplaintManagement' }

if ($events) {
    foreach ($event in $events | Select-Object -First 10) {
        Write-Host "==================" -ForegroundColor Yellow
        Write-Host "Time: $($event.TimeCreated)" -ForegroundColor Cyan
        Write-Host "Provider: $($event.ProviderName)" -ForegroundColor Cyan
        Write-Host "Message:" -ForegroundColor Green
        Write-Host $event.Message
    }
} else {
    Write-Host "No relevant events found"
}
