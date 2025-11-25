$response = Invoke-RestMethod -Uri 'http://localhost:5058/api/auth/login' -Method Post -Body (@{username='admin@test.com';password='Admin@123'} | ConvertTo-Json) -ContentType 'application/json'
$token = $response.data.token
$complaints = Invoke-RestMethod -Uri 'http://localhost:5058/api/complaints?pageSize=1' -Headers @{Authorization="Bearer $token"}
$complaintId = $complaints.data.items[0].id
Write-Host "Testing comment creation for complaint: $complaintId"
try {
    $result = Invoke-RestMethod -Uri "http://localhost:5058/api/complaints/$complaintId/comments" -Method Post -Body (@{comment='Test comment';isInternal=$false} | ConvertTo-Json) -Headers @{Authorization="Bearer $token";'Content-Type'='application/json'}
    Write-Host "SUCCESS" -ForegroundColor Green
    $result | ConvertTo-Json -Depth 3
} catch {
    Write-Host "FAILED" -ForegroundColor Red
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "Error: $($_.Exception.Message)"
    Write-Host "Details: $($_.ErrorDetails.Message)"
}
