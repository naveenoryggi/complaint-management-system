$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$complaintId = "dc5f95da-92d1-40f9-8ed3-1b91f0b70c34"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "=== TESTING ESCALATION WITH COMPLETE CONFIGURATION ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "1. Getting complaint BEFORE escalation..." -ForegroundColor Yellow
$beforeResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$complaintId" -Headers $headers
Write-Host "   Current Escalation Level: $($beforeResponse.data.currentEscalationLevel)" -ForegroundColor White
Write-Host "   Current Status: $($beforeResponse.data.status)" -ForegroundColor White
Write-Host ""

Write-Host "2. Escalating complaint..." -ForegroundColor Yellow
$body = @{
    reason = "Final test - all bugs fixed, Escalated status added"
} | ConvertTo-Json

try {
    $escalateResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$complaintId/escalate" `
        -Method POST `
        -Headers $headers `
        -Body $body

    Write-Host "   SUCCESS: Escalation completed!" -ForegroundColor Green
    Write-Host "   Response: $($escalateResponse | ConvertTo-Json -Depth 3)" -ForegroundColor White
} catch {
    Write-Host "   FAILED: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "3. Getting complaint AFTER escalation..." -ForegroundColor Yellow
$afterResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$complaintId" -Headers $headers
Write-Host "   New Escalation Level: $($afterResponse.data.currentEscalationLevel)" -ForegroundColor White
Write-Host "   New Status: $($afterResponse.data.status)" -ForegroundColor White
Write-Host ""

Write-Host "=== VERIFICATION ===" -ForegroundColor Cyan
if ($afterResponse.data.currentEscalationLevel -gt $beforeResponse.data.currentEscalationLevel) {
    Write-Host "✓ Escalation level increased from $($beforeResponse.data.currentEscalationLevel) to $($afterResponse.data.currentEscalationLevel)" -ForegroundColor Green
} else {
    Write-Host "✗ Escalation level did NOT increase!" -ForegroundColor Red
}

if ($afterResponse.data.status -eq "Escalated") {
    Write-Host "✓ Status changed to 'Escalated'" -ForegroundColor Green
} else {
    Write-Host "✗ Status is '$($afterResponse.data.status)' instead of 'Escalated'" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== TEST COMPLETE ===" -ForegroundColor Cyan
