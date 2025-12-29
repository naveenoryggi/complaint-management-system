# Check IIS Configuration
Import-Module WebAdministration -ErrorAction SilentlyContinue

Write-Host "=== IIS Sites ===" -ForegroundColor Yellow
Get-Website | Select-Object Name, State, PhysicalPath, Bindings | Format-Table -AutoSize

Write-Host "`n=== Application Pools ===" -ForegroundColor Yellow
Get-IISAppPool | Select-Object Name, State, ManagedPipelineMode | Format-Table -AutoSize

Write-Host "`n=== ComplaintManagement API Site Details ===" -ForegroundColor Yellow
$site = Get-Website -Name "ComplaintManagement*" | Select-Object -First 1
if ($site) {
    Write-Host "Site Name: $($site.Name)"
    Write-Host "State: $($site.State)"
    Write-Host "Physical Path: $($site.PhysicalPath)"
    Write-Host "Bindings: $($site.Bindings.Collection | ForEach-Object { $_.BindingInformation })"

    # Get application pool
    $appPool = Get-IISAppPool -Name $site.ApplicationPool
    if ($appPool) {
        Write-Host "`nApplication Pool: $($appPool.Name)"
        Write-Host "Pool State: $($appPool.State)"
    }
}

Write-Host "`n=== Logs Folder Permissions ===" -ForegroundColor Yellow
$logsPath = "C:\Program Files\ComplaintManagement\API\logs"
if (Test-Path $logsPath) {
    icacls $logsPath
} else {
    Write-Host "Logs folder does not exist!" -ForegroundColor Red
}

Write-Host "`n=== API Folder Permissions ===" -ForegroundColor Yellow
icacls "C:\Program Files\ComplaintManagement\API" | Select-Object -First 10
