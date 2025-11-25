# Real-time log monitor - shows last 30 lines every 2 seconds
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "ADVANCED LOGGING ACTIVE - Monitoring Backend Logs" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Log file: backend-live-logs.log"
Write-Host "Watching for: Errors, Warnings, OAuth, Email, User operations"
Write-Host "Press Ctrl+C to stop monitoring"
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

$lastSize = 0
while ($true) {
    Start-Sleep -Seconds 2

    if (Test-Path "backend-live-logs.log") {
        $currentSize = (Get-Item "backend-live-logs.log").Length

        if ($currentSize -ne $lastSize) {
            Clear-Host
            Write-Host "=== LATEST LOGS (Last 40 lines) ===" -ForegroundColor Yellow
            Get-Content "backend-live-logs.log" -Tail 40 | ForEach-Object {
                if ($_ -match "error|Error|ERROR|Exception|exception") {
                    Write-Host $_ -ForegroundColor Red
                } elseif ($_ -match "warn|Warn|WARN") {
                    Write-Host $_ -ForegroundColor Yellow
                } elseif ($_ -match "oauth|OAuth|token|Token") {
                    Write-Host $_ -ForegroundColor Cyan
                } elseif ($_ -match "email|Email|IMAP|SMTP") {
                    Write-Host $_ -ForegroundColor Green
                } else {
                    Write-Host $_
                }
            }
            $lastSize = $currentSize
            Write-Host ""
            Write-Host "=== Watching for changes... ===" -ForegroundColor Gray
        }
    }
}
