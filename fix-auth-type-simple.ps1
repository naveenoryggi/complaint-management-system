# Fix OAuth authenticationType in database

$query = "UPDATE EmailConfigurations SET AuthenticationType = 1 WHERE FromEmail = 'marketing@oryggitech.com'; SELECT Id, FromEmail, AuthenticationType, CASE WHEN OAuthAccessToken IS NULL THEN 'NULL' ELSE 'HAS_TOKEN' END as HasToken, OAuthTokenExpiresAt FROM EmailConfigurations WHERE FromEmail = 'marketing@oryggitech.com';"

Write-Host "Updating authenticationType to OAuth 2.0..." -ForegroundColor Yellow

try {
    $result = Invoke-Sqlcmd -ServerInstance "PRANA-ASUS\SQLEXPRESS" -Database "ComplaintManagementDb" -Query $query
    Write-Host "Update successful!" -ForegroundColor Green
    $result | Format-Table -AutoSize
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
