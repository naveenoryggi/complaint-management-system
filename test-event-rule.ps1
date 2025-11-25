# Test Event Communication Rule Creation
$TOKEN = (Get-Content .test-token -Raw).Trim()

$headers = @{
    "Authorization" = "Bearer $TOKEN"
    "Content-Type" = "application/json"
}

$body = @{
    name = "Test Rule"
    eventTypeId = "C63369B9-75FC-43F4-8402-20127EADD5B7"  # Complaint Status Changed
    channel = 0  # Email
    recipientType = 0  # Complainant
    isActive = $true
} | ConvertTo-Json

Write-Host "Testing Event Communication Rule Creation..." -ForegroundColor Yellow
Write-Host "Body: $body" -ForegroundColor Gray

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5058/api/event-communication-rules" -Method Post -Headers $headers -Body $body
    Write-Host "SUCCESS: Event rule created!" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 10
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $reader.DiscardBufferedData()
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Yellow
    }
}
