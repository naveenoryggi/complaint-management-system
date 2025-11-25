Get-Process | Where-Object { $_.ProcessName -eq 'dotnet' } | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Host "Killed all dotnet processes"
Start-Sleep -Seconds 2
