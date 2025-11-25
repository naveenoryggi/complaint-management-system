$token = (Get-Content .test-token -Raw).Trim()
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    title = "SLA Test Complaint"
    description = "Testing SLA calculation"
    categoryId = "ab5f70ee-5d58-4cec-b7ad-0fa732a41ec8"
    priority = 1
    isAnonymous = $false
} | ConvertTo-Json

Write-Host "Request Body:" -ForegroundColor Yellow
Write-Host $body

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5058/api/complaints" `
        -Method Post -Headers $headers -Body $body
    Write-Host "`nSUCCESS!" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5)
} catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details:" -ForegroundColor Yellow
        Write-Host $_.ErrorDetails.Message
    }
}
