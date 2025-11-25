$serverName = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
$databaseName = "ComplaintManagementDb"
$configId = "4a1b41ef-cbc5-4858-a6a5-02b1c147a80a"

$connectionString = "Server=$serverName;Database=$databaseName;Integrated Security=True;TrustServerCertificate=True;"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()

$query = @"
SELECT
    OAuthTokenExpiresAt,
    DATEDIFF(MINUTE, GETUTCDATE(), OAuthTokenExpiresAt) AS MinutesUntilExpiry,
    CASE
        WHEN GETUTCDATE() > OAuthTokenExpiresAt THEN 'EXPIRED'
        ELSE 'VALID'
    END AS TokenStatus
FROM EmailConfigurations
WHERE Id = '$configId'
"@

$command = $connection.CreateCommand()
$command.CommandText = $query
$reader = $command.ExecuteReader()

if ($reader.Read()) {
    Write-Host "Current Token Status:" -ForegroundColor Cyan
    Write-Host "Expires At (UTC): $($reader['OAuthTokenExpiresAt'])"
    Write-Host "Minutes Until Expiry: $($reader['MinutesUntilExpiry'])"
    Write-Host "Status: $($reader['TokenStatus'])" -ForegroundColor $(if ($reader['TokenStatus'] -eq 'VALID') { 'Green' } else { 'Red' })
}

$reader.Close()
$connection.Close()
