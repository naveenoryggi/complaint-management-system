$BaseUrl = "http://localhost:5000"

# Login
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token
$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "Creating Event Type..." -ForegroundColor Yellow

$newEventType = @{
    name = "Test Notification Event"
    code = "TEST_NOTIF_DEBUG"
    description = "Test event type"
    entityType = "Complaint"
    category = "Notification"
    isActive = $true
    availableFields = @("ComplaintNumber", "Title", "Status")
    iconClass = "fa fa-bell"
}

Write-Host "Request body:" -ForegroundColor Cyan
$newEventType | ConvertTo-Json -Depth 5

try {
    $result = Invoke-WebRequest -Uri "$BaseUrl/api/event-types" -Method POST -Headers $headers -Body ($newEventType | ConvertTo-Json -Depth 5) -ContentType "application/json"
    Write-Host "Success!" -ForegroundColor Green
    $result.Content | ConvertFrom-Json | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Error:" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $responseStream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($responseStream)
    $responseBody = $reader.ReadToEnd()
    Write-Host "Response: $responseBody" -ForegroundColor Red
}
