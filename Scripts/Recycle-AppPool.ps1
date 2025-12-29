# Recycle the app pool
& "C:\Windows\System32\inetsrv\appcmd.exe" recycle apppool /apppool.name:"ComplaintManagementAPIPool"
Write-Host "App pool recycled!" -ForegroundColor Green
