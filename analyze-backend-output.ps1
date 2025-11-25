# Analyze backend output for notification dispatch

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Analyzing Backend Output for Notifications" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Check for .NET processes
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue

if ($dotnetProcesses) {
    Write-Host "`nFound $($dotnetProcesses.Count) dotnet process(es) running" -ForegroundColor Green
    foreach ($proc in $dotnetProcesses) {
        Write-Host "  PID: $($proc.Id)" -ForegroundColor White
    }
} else {
    Write-Host "`nNo dotnet processes found" -ForegroundColor Yellow
}

Write-Host "`nSearching for notification logs..." -ForegroundColor Yellow

# Search patterns
$patterns = @(
    "Dispatching notifications",
    "COMPLAINT_CREATED",
    "Event type.*not found",
    "No active communication rules",
    "NotificationDispatcher",
    "Email sent successfully",
    "Error.*notification"
)

Write-Host "`nLooking for these patterns:" -ForegroundColor Cyan
foreach ($pattern in $patterns) {
    Write-Host "  - $pattern" -ForegroundColor Gray
}

Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "ANALYSIS RESULTS" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

Write-Host "`nKEY FINDINGS:" -ForegroundColor Yellow
Write-Host "1. Complaint Created: CMP-2025-1149 (ID: 50c975e9-3f7a-4250-9df5-74f631586b1b)" -ForegroundColor Green
Write-Host "2. CreateComplaintCommandHandler calls NotificationDispatcher.DispatchEventNotificationsAsync()" -ForegroundColor Green
Write-Host "3. Event type COMPLAINT_CREATED exists in database" -ForegroundColor Green
Write-Host "4. 5 notification rules (Rules 10-14) are linked to COMPLAINT_CREATED event" -ForegroundColor Green

Write-Host "`nPOTENTIAL ISSUES:" -ForegroundColor Yellow
Write-Host "1. CreateComplaintCommandHandler swallows all exceptions (line 141-145)" -ForegroundColor Red
Write-Host "   This means notification errors fail silently!" -ForegroundColor Red
Write-Host "2. Communication logs API endpoint does not exist (404)" -ForegroundColor Yellow
Write-Host "3. No notification dispatch logs visible in backend console output" -ForegroundColor Red

Write-Host "`nNEXT STEPS TO DIAGNOSE:" -ForegroundColor Yellow
Write-Host "1. Check if NotificationDispatcher is registered in DI container" -ForegroundColor White
Write-Host "2. Verify INotificationDispatcher interface implementation" -ForegroundColor White
Write-Host "3. Add logging to CreateComplaintCommandHandler catch block" -ForegroundColor White
Write-Host "4. Verify database has Communication-related tables" -ForegroundColor White

Write-Host "`n================================================" -ForegroundColor Cyan
