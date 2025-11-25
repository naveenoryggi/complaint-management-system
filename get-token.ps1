$authBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $authResponse = Invoke-RestMethod -Uri "http://localhost:5058/api/auth/login" -Method POST -Body $authBody -ContentType "application/json"
    $token = $authResponse.data.token
    Write-Host "Token obtained successfully"
    Write-Host $token
    $token | Out-File -FilePath ".test-token" -NoNewline
} catch {
    Write-Host "Failed to get token: $($_.Exception.Message)"
}
