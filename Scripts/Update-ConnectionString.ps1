# Update connection string to use SQL authentication
$appsettingsPath = "C:\Program Files\ComplaintManagement\API\appsettings.json"

Write-Host "Reading appsettings.json..." -ForegroundColor Yellow
$json = Get-Content $appsettingsPath -Raw | ConvertFrom-Json

# Update the connection string to use SQL authentication
$newConnectionString = "Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;User Id=sa;Password=admin@123;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60;Command Timeout=600"

Write-Host "Old connection string:" -ForegroundColor Cyan
Write-Host $json.ConnectionStrings.DefaultConnection

$json.ConnectionStrings.DefaultConnection = $newConnectionString

Write-Host ""
Write-Host "New connection string:" -ForegroundColor Green
Write-Host $json.ConnectionStrings.DefaultConnection

# Save the updated JSON
$json | ConvertTo-Json -Depth 100 | Out-File -FilePath $appsettingsPath -Encoding UTF8 -Force

Write-Host ""
Write-Host "Connection string updated!" -ForegroundColor Green

# Recycle the app pool
Write-Host ""
Write-Host "Recycling IIS App Pool..." -ForegroundColor Yellow
& "C:\Windows\System32\inetsrv\appcmd.exe" recycle apppool /apppool.name:"ComplaintManagementAPIPool"

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
