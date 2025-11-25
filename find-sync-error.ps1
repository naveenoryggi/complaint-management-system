# Find OryggiSync trigger errors
$logFile = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.API\Logs\errors_20251019.log"

try {
    Write-Host "Reading error log file..." -ForegroundColor Yellow
    $content = Get-Content $logFile -Raw

    # Split by the separator
    $entries = $content -split '===================================================================================================='

    Write-Host "Total log entries: $($entries.Count)" -ForegroundColor Cyan
    Write-Host ""

    # Find OryggiSync trigger entries
    Write-Host "=== ORYGGI SYNC TRIGGER ERRORS ===" -ForegroundColor Yellow
    $syncErrors = $entries | Where-Object { $_ -like '*OryggiSync/trigger*' }

    if ($syncErrors.Count -gt 0) {
        Write-Host "Found $($syncErrors.Count) OryggiSync/trigger error(s)" -ForegroundColor Green
        Write-Host ""

        # Show the last 3
        $syncErrors | Select-Object -Last 3 | ForEach-Object {
            Write-Host $_ -ForegroundColor White
            Write-Host "====================================================================================================" -ForegroundColor Gray
        }
    } else {
        Write-Host "No OryggiSync/trigger errors found" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
