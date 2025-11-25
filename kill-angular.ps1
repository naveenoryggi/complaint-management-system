$process = Get-NetTCPConnection -LocalPort 4200 -State Listen -ErrorAction SilentlyContinue
if ($process) {
    Stop-Process -Id $process.OwningProcess -Force
    Write-Host "Killed process on port 4200"
} else {
    Write-Host "No process found on port 4200"
}
