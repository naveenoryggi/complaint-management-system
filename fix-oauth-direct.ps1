Write-Host "Fixing OAuth Authentication Type - Direct Database Update" -ForegroundColor Cyan

# Load SQL Server assembly
Add-Type -AssemblyName "System.Data"

$serverName = "PRANA-ASUS\SQLEXPRESS"
$databaseName = "ComplaintManagementDb"
$configId = "4a1b41ef-cbc5-4858-a6a5-02b1c147a80a"

$connectionString = "Server=$serverName;Database=$databaseName;Integrated Security=True;TrustServerCertificate=True;"

Write-Host "Connecting to SQL Server..." -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()

    Write-Host "Connected successfully!" -ForegroundColor Green

    # Check current state
    Write-Host "`nChecking current configuration..." -ForegroundColor Yellow
    $query = "SELECT Id, FromEmail, AuthenticationType, OAuthClientId, OAuthTokenExpiresAt, IsEnabled FROM EmailConfigurations WHERE Id = '$configId'"

    $command = New-Object System.Data.SqlClient.SqlCommand
    $command.Connection = $connection
    $command.CommandText = $query

    $reader = $command.ExecuteReader()

    if ($reader.Read()) {
        Write-Host "`nCurrent State:" -ForegroundColor Cyan
        Write-Host "  Email: $($reader['FromEmail'])" -ForegroundColor White
        Write-Host "  Auth Type: $($reader['AuthenticationType']) $(if ($reader['AuthenticationType'] -eq 1) { '(Basic - WRONG)' } else { '(OAuth2 - CORRECT)' })" -ForegroundColor $(if ($reader['AuthenticationType'] -eq 1) { 'Red' } else { 'Green' })
        Write-Host "  OAuth Client ID: $($reader['OAuthClientId'])" -ForegroundColor White
        Write-Host "  Token Expires: $($reader['OAuthTokenExpiresAt'])" -ForegroundColor $(if ($reader['OAuthTokenExpiresAt'] -lt (Get-Date)) { 'Red' } else { 'Green' })
        Write-Host "  Enabled: $($reader['IsEnabled'])" -ForegroundColor White
    }
    $reader.Close()

    # Update to OAuth2
    Write-Host "`nUpdating AuthenticationType to OAuth2 (2)..." -ForegroundColor Yellow

    $updateQuery = "UPDATE EmailConfigurations SET AuthenticationType = 2, UpdatedAt = GETUTCDATE() WHERE Id = '$configId'"

    $command.CommandText = $updateQuery
    $rowsAffected = $command.ExecuteNonQuery()

    Write-Host "Update completed! Rows affected: $rowsAffected" -ForegroundColor Green

    # Verify the update
    Write-Host "`nVerifying update..." -ForegroundColor Yellow
    $verifyQuery = "SELECT AuthenticationType, UpdatedAt FROM EmailConfigurations WHERE Id = '$configId'"

    $command.CommandText = $verifyQuery
    $reader = $command.ExecuteReader()

    if ($reader.Read()) {
        Write-Host "`nUpdated State:" -ForegroundColor Green
        Write-Host "  Auth Type: $($reader['AuthenticationType']) $(if ($reader['AuthenticationType'] -eq 2) { '(OAuth2 - CORRECT!)' } else { '(WRONG)' })" -ForegroundColor $(if ($reader['AuthenticationType'] -eq 2) { 'Green' } else { 'Red' })
        Write-Host "  Updated At: $($reader['UpdatedAt'])" -ForegroundColor White
    }
    $reader.Close()

    Write-Host "`n================================================" -ForegroundColor Yellow
    Write-Host "SUCCESS: Authentication type updated to OAuth2!" -ForegroundColor Green
    Write-Host "You can now proceed with OAuth authorization" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Yellow

} catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please run this SQL manually in SQL Server Management Studio:" -ForegroundColor Yellow
    Write-Host "UPDATE EmailConfigurations SET AuthenticationType = 2 WHERE Id = '$configId'" -ForegroundColor Cyan
} finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
        Write-Host "`nDatabase connection closed." -ForegroundColor Gray
    }
}
