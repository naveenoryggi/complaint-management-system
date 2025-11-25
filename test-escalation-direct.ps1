# Test escalation endpoint directly
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$complaintId = "dc5f95da-92d1-40f9-8ed3-1b91f0b70c34"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    reason = "Direct test - investigating escalation issue"
} | ConvertTo-Json

Write-Host "Testing POST /api/complaints/$complaintId/escalate"
Write-Host "Body: $body"
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$complaintId/escalate" `
        -Method POST `
        -Headers $headers `
        -Body $body `
        -ErrorAction Stop

    Write-Host "SUCCESS - Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "ERROR - Response:" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "Status Description: $($_.Exception.Response.StatusDescription)"

    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    $responseBody = $reader.ReadToEnd()
    Write-Host "Response Body:" -ForegroundColor Yellow
    Write-Host $responseBody
}
