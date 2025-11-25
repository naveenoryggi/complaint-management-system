# Get error details for specific request IDs
$requestIds = @(
    "ce9f0dce-70e2-4a8a-a3eb-b37fa50c2f86",  # 18:00:03
    "9a521f7e-9b02-4040-9ad7-f6b506108541"   # 18:00:19
)

$logFile = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.API\Logs\requests_20251019.log"

try {
    Write-Host "Reading request log file..." -ForegroundColor Yellow
    $content = Get-Content $logFile -Raw

    # Split by separator
    $entries = $content -split '===================================================================================================='

    Write-Host "Total log entries: $($entries.Count)" -ForegroundColor Cyan
    Write-Host ""

    foreach ($requestId in $requestIds) {
        Write-Host "=== SEARCHING FOR REQUEST $requestId ===" -ForegroundColor Yellow
        $found = $entries | Where-Object { $_ -like "*$requestId*" }

        if ($found) {
            Write-Host "FOUND!" -ForegroundColor Green
            Write-Host $found -ForegroundColor White
            Write-Host "====================================================================================================" -ForegroundColor Gray
            Write-Host ""
        } else {
            Write-Host "Not found in requests log" -ForegroundColor Red
            Write-Host ""
        }
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
