# Check startup logs
$logsPath = "C:\ProgramData\ComplaintManagement\Logs"
$latestLog = Get-ChildItem $logsPath | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($latestLog) {
    Write-Host "Latest log: $($latestLog.Name)" -ForegroundColor Yellow
    Get-Content $latestLog.FullName -Tail 30
} else {
    Write-Host "No log files found" -ForegroundColor Red
}
