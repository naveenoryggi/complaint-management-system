$headers = @{'Content-Type'='application/json'}
$loginBody = @{username='admin@test.com';password='Admin@123'} | ConvertTo-Json
$response = Invoke-RestMethod -Uri 'http://localhost:5058/api/auth/login' -Method Post -Headers $headers -Body $loginBody
$token = $response.data.token
$authHeaders = @{Authorization="Bearer $token";'Content-Type'='application/json'}

# Get a complaint to test with
$complaints = Invoke-RestMethod -Uri 'http://localhost:5058/api/complaints?pageSize=1' -Headers $authHeaders
$complaintId = $complaints.data.items[0].id

Write-Host "`n=== Testing Create Comment ===" -ForegroundColor Cyan
try {
    $commentBody = @{comment='Test comment';isInternal=$false} | ConvertTo-Json
    Write-Host "Request: $commentBody"
    $result = Invoke-RestMethod -Uri "http://localhost:5058/api/complaints/$complaintId/comments" -Method Post -Headers $authHeaders -Body $commentBody
    Write-Host "SUCCESS: Comment created" -ForegroundColor Green
} catch {
    Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    }
}

Write-Host "`n=== Testing Escalate Complaint ===" -ForegroundColor Cyan
try {
    # Get escalation matrix
    $matrices = Invoke-RestMethod -Uri 'http://localhost:5058/api/escalation/matrices' -Headers $authHeaders
    $matrixId = if ($matrices.data -and $matrices.data.Count -gt 0) { $matrices.data[0].id } else { $null }

    $escalateBody = @{reason='Test escalation';escalationMatrixId=$matrixId;targetLevel=1} | ConvertTo-Json
    Write-Host "Request: $escalateBody"
    $result = Invoke-RestMethod -Uri "http://localhost:5058/api/escalation/complaints/$complaintId/escalate" -Method Post -Headers $authHeaders -Body $escalateBody
    Write-Host "SUCCESS: Complaint escalated" -ForegroundColor Green
} catch {
    Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    }
}
