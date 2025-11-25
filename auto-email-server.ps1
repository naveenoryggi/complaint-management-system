# Login to API
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5058/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "Logged in successfully" -ForegroundColor Green
} catch {
    Write-Host "Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Setup headers
$headers = @{ "Authorization" = "Bearer $token" }

# Get all email servers
try {
    $emailServers = Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings?includeInactive=true" -Method Get -Headers $headers
    Write-Host "Found $($emailServers.data.Count) email servers" -ForegroundColor Yellow
} catch {
    Write-Host "Failed to get email servers: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Delete all non-default email servers
foreach ($server in $emailServers.data) {
    if ($server.isDefault -eq $false) {
        Write-Host "Deleting: $($server.name)..." -NoNewline
        try {
            Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings/$($server.id)" -Method Delete -Headers $headers | Out-Null
            Write-Host " Deleted" -ForegroundColor Green
        } catch {
            Write-Host " Failed" -ForegroundColor Red
        }
    }
}

# Also delete default servers
foreach ($server in $emailServers.data) {
    if ($server.isDefault -eq $true) {
        Write-Host "Deleting default server: $($server.name)..." -NoNewline
        try {
            # First deactivate it
            $deactivate = @{
                id = $server.id
                name = $server.name
                host = $server.host
                port = $server.port
                useSsl = $server.useSsl
                username = $server.username
                password = $server.password
                fromEmail = $server.fromEmail
                fromName = $server.fromName
                replyToEmail = $server.replyToEmail
                isDefault = $false
                isActive = $false
                timeoutSeconds = $server.timeoutSeconds
            } | ConvertTo-Json
            Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings/$($server.id)" -Method Put -Body $deactivate -ContentType "application/json" -Headers $headers | Out-Null
            # Then delete it
            Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings/$($server.id)" -Method Delete -Headers $headers | Out-Null
            Write-Host " Deleted" -ForegroundColor Green
        } catch {
            Write-Host " Failed" -ForegroundColor Red
        }
    }
}

Write-Host "Cleanup completed" -ForegroundColor Green
Write-Host ""
Write-Host "Creating new Gmail SMTP server..." -ForegroundColor Yellow

# Create new Gmail SMTP server
$newServer = @{
    name = "Production Gmail SMTP"
    host = "smtp.gmail.com"
    port = 587
    useSsl = $true
    username = "oryggiserver@gmail.com"
    password = "veaa mwlw hbbq nbzz"
    fromEmail = "oryggiserver@gmail.com"
    fromName = "Complaint Management System"
    replyToEmail = "oryggiserver@gmail.com"
    isDefault = $true
    isActive = $true
    timeoutSeconds = 30
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings" -Method Post -Body $newServer -ContentType "application/json" -Headers $headers
    $newServerId = $createResponse.data.id
    Write-Host "Created new email server with ID: $newServerId" -ForegroundColor Green
} catch {
    Write-Host "Failed to create email server: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test the email server
Write-Host ""
Write-Host "Testing email server..." -ForegroundColor Yellow

$testBody = @{
    testRecipient = "nav_nainital@yahoo.com"
} | ConvertTo-Json

try {
    $testResponse = Invoke-RestMethod -Uri "http://localhost:5058/api/email-settings/$newServerId/test" -Method Post -Body $testBody -ContentType "application/json" -Headers $headers
    Write-Host "Email test completed successfully!" -ForegroundColor Green
    Write-Host "Check nav_nainital@yahoo.com inbox for test email" -ForegroundColor Cyan
} catch {
    Write-Host "Email test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "All operations completed!" -ForegroundColor Green
