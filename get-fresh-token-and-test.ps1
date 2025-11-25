# Login and get fresh token, then test notification rules API
$loginUrl = "http://localhost:5000/api/auth/login"
$apiUrl = "http://localhost:5000/api/event-communication-rules"

# Login credentials
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

Write-Host "Step 1: Logging in to get fresh token..." -ForegroundColor Cyan
try {
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method POST -Body $loginBody -ContentType "application/json"

    $token = $loginResponse.data.token
    Write-Host "Login successful! Token obtained." -ForegroundColor Green
    Write-Host ""

    # Save token
    $token | Out-File ".fresh-token-new" -NoNewline

} catch {
    Write-Host "Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test notification rules API
Write-Host "Step 2: Testing notification rules API endpoint..." -ForegroundColor Cyan
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri $apiUrl -Headers $headers -Method GET

    Write-Host "SUCCESS! API endpoint is working" -ForegroundColor Green
    Write-Host ""
    Write-Host "Number of rules returned: $($response.Count)" -ForegroundColor Yellow
    Write-Host ""

    if ($response.Count -gt 0) {
        Write-Host "Notification Rules:" -ForegroundColor Green
        $ruleIds = @()
        foreach ($rule in $response) {
            Write-Host "  - ID: $($rule.id)" -ForegroundColor White
            Write-Host "    Name: $($rule.name)" -ForegroundColor White
            Write-Host "    Event Type: $($rule.eventType.name)" -ForegroundColor White
            Write-Host "    Channel: $($rule.channel)" -ForegroundColor White
            Write-Host "    Priority: $($rule.priority)" -ForegroundColor White
            Write-Host "    Active: $($rule.isActive)" -ForegroundColor White
            Write-Host ""
            $ruleIds += $rule.id
        }

        Write-Host "Rule IDs:" -ForegroundColor Cyan
        $ruleIds | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
    }

    # Save response to file
    $response | ConvertTo-Json -Depth 10 | Out-File "notification-rules-response.json"
    Write-Host ""
    Write-Host "Full response saved to: notification-rules-response.json" -ForegroundColor Cyan

} catch {
    Write-Host "ERROR testing API!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $reader.DiscardBufferedData()
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}
