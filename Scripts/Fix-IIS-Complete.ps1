# Complete IIS Fix
$apiPath = "C:\Program Files\ComplaintManagement\API"
$outputFile = "C:\Users\Navin Chandra\Pictures\Complaint management system\iis-fix-result.txt"
$output = @()

# 1. Grant permissions to IIS_IUSRS
$output += "=== Granting permissions to IIS_IUSRS ==="
icacls $apiPath /grant "IIS_IUSRS:(OI)(CI)RX" /T
icacls $apiPath /grant "IUSR:(OI)(CI)RX" /T
icacls "$apiPath\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T
$output += "Permissions granted"

# 2. Check current site configuration
$output += ""
$output += "=== Current Site Configuration ==="
$siteConfig = & "C:\Windows\System32\inetsrv\appcmd.exe" list site "ComplaintManagement" /config
$output += $siteConfig

# 3. Check application under the site
$output += ""
$output += "=== Applications under ComplaintManagement site ==="
$apps = & "C:\Windows\System32\inetsrv\appcmd.exe" list app /site.name:"ComplaintManagement"
$output += $apps

# 4. Check virtual directories
$output += ""
$output += "=== Virtual Directories ==="
$vdirs = & "C:\Windows\System32\inetsrv\appcmd.exe" list vdir /app.name:"ComplaintManagement/"
$output += $vdirs

# 5. Check if the path is correct
$output += ""
$output += "=== Physical Path Check ==="
$output += "Checking: $apiPath"
$output += "Exists: $(Test-Path $apiPath)"
$output += "web.config exists: $(Test-Path '$apiPath\web.config')"
$output += "DLL exists: $(Test-Path '$apiPath\ComplaintManagement.API.dll')"

# 6. Recycle the app pool
$output += ""
$output += "=== Recycling App Pool ==="
& "C:\Windows\System32\inetsrv\appcmd.exe" recycle apppool /apppool.name:"ComplaintManagementAPIPool"
$output += "App pool recycled"

# 7. Check if /api is a separate application
$output += ""
$output += "=== Checking for /api application ==="
$apiApp = & "C:\Windows\System32\inetsrv\appcmd.exe" list app | Select-String "ComplaintManagement"
$output += $apiApp

# Save output
$output | Out-File -FilePath $outputFile -Force -Encoding UTF8
Write-Host "Results saved to: $outputFile" -ForegroundColor Green
