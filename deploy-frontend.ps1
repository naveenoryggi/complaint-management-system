# Deploy frontend to installed location
$source = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-angular\dist\complaint-system-angular\browser\*"
$dest = "C:\Program Files\ComplaintManagement\WWW\"

Write-Host "Deploying frontend to $dest"
Copy-Item -Path $source -Destination $dest -Recurse -Force
Write-Host "Frontend deployed successfully"

# Also update config.json with correct relative URL
$config = @{
    apiUrl = "/api"
    baseUrl = "http://localhost:5000"
    hostname = "localhost"
    webPort = "5000"
    apiPort = "5000"
    isIIS = $false
}
$config | ConvertTo-Json | Set-Content "$dest\assets\config.json" -Encoding UTF8
Write-Host "Config.json updated"
Get-Content "$dest\assets\config.json"
