# Kill all Chrome processes that have mcp-chrome in their command line
$chromeProcesses = Get-WmiObject Win32_Process -Filter "name = 'chrome.exe'" | Where-Object {
    $_.CommandLine -like '*mcp-chrome*'
}

if ($chromeProcesses) {
    Write-Host "Found $($chromeProcesses.Count) Playwright Chrome processes"
    foreach ($proc in $chromeProcesses) {
        Write-Host "Killing process $($proc.ProcessId)"
        Stop-Process -Id $proc.ProcessId -Force
    }
    Write-Host "All Playwright Chrome processes killed"
    Start-Sleep -Seconds 2
} else {
    Write-Host "No Playwright Chrome processes found"
}
