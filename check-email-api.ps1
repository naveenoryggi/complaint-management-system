# Login and get email configuration
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    Write-Host "Login successful!" -ForegroundColor Green

    $headers = @{
        Authorization = "Bearer $($loginResponse.token)"
    }

    $emailConfig = Invoke-RestMethod -Uri "http://localhost:5000/api/email-configuration" -Headers $headers -Method Get

    Write-Host "`n=== EMAIL CONFIGURATION API RESPONSE ===" -ForegroundColor Cyan
    $emailConfig | ConvertTo-Json -Depth 5

    Write-Host "`n=== KEY FIELDS ===" -ForegroundColor Yellow
    $emailConfig | ForEach-Object {
        Write-Host "ID: $($_.id)"
        Write-Host "DisplayName: $($_.displayName)"
        Write-Host "ImapUsername: $($_.imapUsername)"
        Write-Host "AuthenticationType: $($_.authenticationType)"
        Write-Host "OAuthTokenExpiresAt: $($_.oAuthTokenExpiresAt)"
        Write-Host "OAuthAccessToken: $($_.oAuthAccessToken -ne $null -and $_.oAuthAccessToken -ne '')"
        Write-Host "OAuthRefreshToken: $($_.oAuthRefreshToken -ne $null -and $_.oAuthRefreshToken -ne '')"
        Write-Host "LastPolledAt: $($_.lastPolledAt)"
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
