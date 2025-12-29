# Show latest stdout log
$logsPath = "C:\Program Files\ComplaintManagement\API\logs"
$files = Get-ChildItem $logsPath -Filter "stdout*" | Sort-Object LastWriteTime -Descending
if ($files.Count -gt 0) {
    $latestLog = $files[0]
    Write-Host "Latest log: $($latestLog.Name)" -ForegroundColor Yellow
    Write-Host "Size: $($latestLog.Length) bytes" -ForegroundColor Cyan
    if ($latestLog.Length -gt 0) {
        Get-Content $latestLog.FullName -Tail 50
    } else {
        Write-Host "Log file is empty" -ForegroundColor Red
    }
} else {
    Write-Host "No stdout log files found" -ForegroundColor Red
}
