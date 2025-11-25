Write-Host "Checking server status..." -ForegroundColor Cyan

Write-Host "`nProcesses:" -ForegroundColor Yellow
Get-Process dotnet,node -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime | Format-Table

Write-Host "Testing Backend (port 5000):" -ForegroundColor Yellow
$backend = Test-NetConnection localhost -Port 5000 -InformationLevel Quiet -WarningAction SilentlyContinue
if ($backend) { Write-Host "  Backend: RUNNING" -ForegroundColor Green } else { Write-Host "  Backend: NOT RUNNING" -ForegroundColor Red }

Write-Host "`nTesting Frontend (port 4200):" -ForegroundColor Yellow
$frontend = Test-NetConnection localhost -Port 4200 -InformationLevel Quiet -WarningAction SilentlyContinue
if ($frontend) { Write-Host "  Frontend: RUNNING" -ForegroundColor Green } else { Write-Host "  Frontend: NOT RUNNING" -ForegroundColor Red }

if ($backend -and $frontend) {
    Write-Host "`n SUCCESS: Both servers are running!" -ForegroundColor Green
} else {
    Write-Host "`n ERROR: One or both servers are not running" -ForegroundColor Red
}
