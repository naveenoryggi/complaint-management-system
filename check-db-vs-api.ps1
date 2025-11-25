$serverName = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
$databaseName = "ComplaintManagementDb"
$configId = "4a1b41ef-cbc5-4858-a6a5-02b1c147a80a"

Write-Host "=== DATABASE VALUE ===" -ForegroundColor Cyan
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
    END AS TokenStatus,
    LEN(OAuthAccessToken) AS TokenLength,
    LEN(OAuthRefreshToken) AS RefreshTokenLength
FROM EmailConfigurations
WHERE Id = '$configId'
"@

$command = $connection.CreateCommand()
$command.CommandText = $query
$reader = $command.ExecuteReader()

if ($reader.Read()) {
    Write-Host "Token Expires At (UTC): $($reader['OAuthTokenExpiresAt'])"
    Write-Host "Minutes Until Expiry: $($reader['MinutesUntilExpiry'])"
    Write-Host "Status: $($reader['TokenStatus'])" -ForegroundColor $(if ($reader['TokenStatus'] -eq 'VALID') { 'Green' } else { 'Red' })
    Write-Host "Access Token Length: $($reader['TokenLength'])"
    Write-Host "Refresh Token Length: $($reader['RefreshTokenLength'])"
}

$reader.Close()
$connection.Close()

Write-Host "`nCurrent UTC Time: $([DateTime]::UtcNow.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Yellow
Write-Host "Current IST Time: $([DateTime]::UtcNow.AddHours(5).AddMinutes(30).ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Yellow
