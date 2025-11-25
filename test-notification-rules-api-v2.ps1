# Test notification rules API endpoint with working token
$token = Get-Content "C:\Users\Navin Chandra\Pictures\Complaint management system\.working-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "Testing API endpoint: http://localhost:5000/api/event-communication-rules" -ForegroundColor Cyan
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/event-communication-rules" -Headers $headers -Method GET

    Write-Host "SUCCESS! API endpoint is working" -ForegroundColor Green
    Write-Host ""
    Write-Host "Number of rules returned: $($response.Count)" -ForegroundColor Yellow
    Write-Host ""

    if ($response.Count -gt 0) {
        Write-Host "Notification Rules:" -ForegroundColor Green
        foreach ($rule in $response) {
            Write-Host "  - ID: $($rule.id)" -ForegroundColor White
            Write-Host "    Name: $($rule.name)" -ForegroundColor White
            Write-Host "    Event Type: $($rule.eventType.name)" -ForegroundColor White
            Write-Host "    Channel: $($rule.channel)" -ForegroundColor White
            Write-Host "    Priority: $($rule.priority)" -ForegroundColor White
            Write-Host "    Active: $($rule.isActive)" -ForegroundColor White
            Write-Host ""
        }
    }

    # Save response to file
    $response | ConvertTo-Json -Depth 10 | Out-File "notification-rules-response.json"
    Write-Host "Full response saved to: notification-rules-response.json" -ForegroundColor Cyan

} catch {
    Write-Host "ERROR!" -ForegroundColor Red
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

# Also test if backend is running
Write-Host ""
Write-Host "Checking if backend is running..." -ForegroundColor Cyan
try {
    $healthCheck = Invoke-RestMethod -Uri "http://localhost:5000/api/health" -Method GET -ErrorAction SilentlyContinue
    Write-Host "Backend is running: $($healthCheck)" -ForegroundColor Green
} catch {
    Write-Host "Backend health check failed - backend may not be running" -ForegroundColor Yellow
}
