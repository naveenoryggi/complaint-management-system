# Check EmailServerSettings in Database

Write-Host "Checking EmailServerSettings table..." -ForegroundColor Cyan

try {
    $query = "SELECT COUNT(*) as RecordCount FROM EmailServerSettings"
    $count = Invoke-Sqlcmd -ServerInstance "PRANA-ASUS\SQLEXPRESS" -Database "ComplaintManagementDb" -Query $query

    Write-Host "`nTotal records: $($count.RecordCount)" -ForegroundColor Green

    if ($count.RecordCount -gt 0) {
        Write-Host "`nEmailServerSettings records:" -ForegroundColor Yellow
        $query2 = "SELECT Id, Name, Host, Port, UseSsl, FromEmail, IsActive, IsDefault FROM EmailServerSettings"
        $records = Invoke-Sqlcmd -ServerInstance "PRANA-ASUS\SQLEXPRESS" -Database "ComplaintManagementDb" -Query $query2
        $records | Format-Table -AutoSize
    } else {
        Write-Host "`nNo EmailServerSettings configured - this is required for auto-response system!" -ForegroundColor Yellow
        Write-Host "The table exists but is empty." -ForegroundColor Yellow
    }
} catch {
    Write-Host "Error querying database: $_" -ForegroundColor Red
}
