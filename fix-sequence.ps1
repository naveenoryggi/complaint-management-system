$headers = @{'Content-Type'='application/json'}
$loginBody = @{username='admin@test.com';password='Admin@123'} | ConvertTo-Json
$response = Invoke-RestMethod -Uri 'http://localhost:5058/api/auth/login' -Method Post -Headers $headers -Body $loginBody
$token = $response.data.token
$authHeaders = @{Authorization="Bearer $token"}

# Get all complaints
$complaints = Invoke-RestMethod -Uri 'http://localhost:5058/api/complaints?pageSize=10000' -Headers $authHeaders
$complaint1060 = $complaints.data.items | Where-Object { $_.complaintNumber -eq 'CMP-2025-1060' }

if ($complaint1060) {
    Write-Host "Found CMP-2025-1060 (ID: $($complaint1060.id))" -ForegroundColor Yellow
    $deleteResponse = Invoke-RestMethod -Uri "http://localhost:5058/api/complaints/$($complaint1060.id)" -Method Delete -Headers $authHeaders
    Write-Host "Deleted successfully" -ForegroundColor Green
} else {
    Write-Host "CMP-2025-1060 not found in database" -ForegroundColor Cyan
}

# Show current max complaint number
$maxComplaint = $complaints.data.items | Where-Object { $_.complaintNumber -like 'CMP-2025-*' } | Sort-Object complaintNumber -Descending | Select-Object -First 1
Write-Host "Current max complaint: $($maxComplaint.complaintNumber)" -ForegroundColor Cyan
