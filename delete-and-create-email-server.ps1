# Login and get token
$loginBody = @{
    emailOrPhone = 'admin@complaintmanagement.com'
    password = 'Admin@123'
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5058/api/auth/login' -Method Post -Body $loginBody -ContentType 'application/json'
$token = $loginResponse.data.token

Write-Host '✓ Logged in successfully' -ForegroundColor Green

# Get all email servers
$headers = @{ 'Authorization' = "Bearer $token" }
$emailServers = Invoke-RestMethod -Uri 'http://localhost:5058/api/email-settings?includeInactive=true' -Method Get -Headers $headers

Write-Host "Found $($emailServers.data.Count) email servers to delete" -ForegroundColor Yellow

# Delete all email servers (skip default ones to avoid error)
foreach ($server in $emailServers.data) {
    if (-not $server.isDefault) {
        Write-Host "Deleting: $($server.name)..." -NoNewline
        try {
            Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings/$($server.id)" -Method Delete -Headers $headers | Out-Null
            Write-Host " ✓ Deleted" -ForegroundColor Green
        } catch {
            Write-Host " ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "Skipping default server: $($server.name)" -ForegroundColor Cyan
    }
}

Write-Host "`n✓ Cleanup completed" -ForegroundColor Green
Write-Host "`nCreating new Gmail SMTP server..." -ForegroundColor Yellow

# Create new Gmail SMTP server
$newServer = @{
    name = 'Production Gmail SMTP'
    host = 'smtp.gmail.com'
    port = 587
    useSsl = $true
    username = 'oryggiserver@gmail.com'
    password = 'veaa mwlw hbbq nbzz'
    fromEmail = 'oryggiserver@gmail.com'
    fromName = 'Complaint Management System'
    replyToEmail = 'oryggiserver@gmail.com'
    isDefault = $true
    isActive = $true
    timeoutSeconds = 30
} | ConvertTo-Json

$createResponse = Invoke-RestMethod -Uri 'http://localhost:5058/api/email-settings' -Method Post -Body $newServer -ContentType 'application/json' -Headers $headers
$newServerId = $createResponse.data.id

Write-Host "✓ Created new email server with ID: $newServerId" -ForegroundColor Green

# Test the email server
Write-Host "`nTesting email server..." -ForegroundColor Yellow

$testBody = @{
    testRecipient = 'nav_nainital@yahoo.com'
} | ConvertTo-Json

try {
    $testResponse = Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings/$newServerId/test" -Method Post -Body $testBody -ContentType 'application/json' -Headers $headers
    Write-Host "✓ Email test completed successfully!" -ForegroundColor Green
    Write-Host "  Check nav_nainital@yahoo.com inbox for test email" -ForegroundColor Cyan
    Write-Host "  Test response: $($testResponse | ConvertTo-Json -Depth 3)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Email test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n✅ All operations completed!" -ForegroundColor Green
