$body = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Output $response.token
} catch {
    Write-Host "ERROR: $($_.Exception.Message)"
    exit 1
}
