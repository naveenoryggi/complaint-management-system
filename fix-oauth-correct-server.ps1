Write-Host "Fixing OAuth Authentication Type - Using Correct Server" -ForegroundColor Cyan

# Load SQL Server assembly
Add-Type -AssemblyName "System.Data"

$serverName = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
$databaseName = "ComplaintManagementDb"
$configId = "4a1b41ef-cbc5-4858-a6a5-02b1c147a80a"

$connectionString = "Server=$serverName;Database=$databaseName;Integrated Security=True;TrustServerCertificate=True;"

Write-Host "Connecting to SQL Server: $serverName" -ForegroundColor Yellow

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
        $authType = $reader['AuthenticationType']
        if ($authType -eq 1) {
            Write-Host "  Auth Type: 1 (Basic - WRONG)" -ForegroundColor Red
        } else {
            Write-Host "  Auth Type: 2 (OAuth2 - CORRECT)" -ForegroundColor Green
        }
        Write-Host "  OAuth Client ID: $($reader['OAuthClientId'])" -ForegroundColor White
        Write-Host "  Token Expires: $($reader['OAuthTokenExpiresAt'])" -ForegroundColor White
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
        $authType = $reader['AuthenticationType']
        if ($authType -eq 2) {
            Write-Host "  Auth Type: 2 (OAuth2 - CORRECT!)" -ForegroundColor Green
        } else {
            Write-Host "  Auth Type: $authType (WRONG)" -ForegroundColor Red
        }
        Write-Host "  Updated At: $($reader['UpdatedAt'])" -ForegroundColor White
    }
    $reader.Close()

    Write-Host "`n================================================" -ForegroundColor Yellow
    Write-Host "SUCCESS: Authentication type updated to OAuth2!" -ForegroundColor Green
    Write-Host "You can now proceed with OAuth authorization" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Yellow

} catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack Trace: $($_.Exception.StackTrace)" -ForegroundColor Yellow
} finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
        Write-Host "`nDatabase connection closed." -ForegroundColor Gray
    }
}
