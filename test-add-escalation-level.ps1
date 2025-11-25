# Test Add Escalation Level
$TOKEN = (Get-Content .test-token -Raw).Trim()
$matrixId = "27FB7807-6120-42A6-8801-39DF7A4557A0"

$headers = @{
    "Authorization" = "Bearer $TOKEN"
    "Content-Type" = "application/json"
}

$body = @{
    level = 1
    name = "Level 1 - Manager"
    triggerAfterValue = 24
    triggerTimeUnit = 1  # Hours
    assignmentStrategy = 0  # ReportingManager
    sendNotification = $true
    notifyPreviousHandler = $true
} | ConvertTo-Json

Write-Host "Testing Add Escalation Level..." -ForegroundColor Yellow
Write-Host "Matrix ID: $matrixId" -ForegroundColor Gray
Write-Host "Body: $body" -ForegroundColor Gray

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5058/api/escalation/matrices/$matrixId/levels" -Method Post -Headers $headers -Body $body
    Write-Host "SUCCESS: Escalation level added!" -ForegroundColor Green
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
