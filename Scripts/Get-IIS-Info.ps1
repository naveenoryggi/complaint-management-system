# Get IIS Info and save to file
$outputFile = "C:\Users\Navin Chandra\Pictures\Complaint management system\iis-info.txt"

$output = @()
$output += "=== IIS Sites ==="
$output += (& "C:\Windows\System32\inetsrv\appcmd.exe" list site)
$output += ""
$output += "=== Application Pools ==="
$output += (& "C:\Windows\System32\inetsrv\appcmd.exe" list apppool)
$output += ""
$output += "=== Site Config for ComplaintManagementAPI ==="
$output += (& "C:\Windows\System32\inetsrv\appcmd.exe" list app /site.name:"ComplaintManagementAPI")
$output += ""
$output += "=== Logs Folder Contents ==="
$logsPath = "C:\Program Files\ComplaintManagement\API\logs"
if (Test-Path $logsPath) {
    $output += Get-ChildItem $logsPath -Force | ForEach-Object { $_.Name }
} else {
    $output += "Logs folder does not exist"
}

$output | Out-File -FilePath $outputFile -Force -Encoding UTF8
Write-Host "Output saved to: $outputFile"
