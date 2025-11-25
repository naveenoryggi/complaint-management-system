$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "Testing Update Complaint Endpoint with correct field names..." -ForegroundColor Cyan
Write-Host ""

# Get priority master IDs
Write-Host "1. Fetching priority and status IDs..." -ForegroundColor Yellow
try {
    $prioritiesData = Invoke-RestMethod -Uri "http://localhost:5000/api/ComplaintPriorityMaster" -Headers $headers
    $HighPriorityId = ($prioritiesData.data | Where-Object { $_.name -eq "High" }).id
    Write-Host "   High Priority ID: $HighPriorityId" -ForegroundColor White

    $statusesData = Invoke-RestMethod -Uri "http://localhost:5000/api/ComplaintStatusMaster" -Headers $headers
    $InProgressStatusId = ($statusesData.data | Where-Object { $_.statusType -eq "InProgress" } | Select-Object -First 1).id
    Write-Host "   In Progress Status ID: $InProgressStatusId" -ForegroundColor White
} catch {
    Write-Host "   FAILED to fetch IDs: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Create test complaint
Write-Host "2. Creating test complaint..." -ForegroundColor Yellow
$createBody = @{
    title = "Update Test Complaint"
    description = "Testing update functionality"
    categoryId = "a4e6d993-ea9b-442f-a803-e61356c56760"
    isAnonymous = $false
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" `
        -Method POST `
        -Headers $headers `
        -Body $createBody

    $complaintId = $createResponse.data.id
    Write-Host "   Created complaint: $complaintId" -ForegroundColor Green
} catch {
    Write-Host "   FAILED to create complaint: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Now try to update it with CORRECT field names including priorityMasterId
Write-Host "3. Updating complaint with correct field names (priorityMasterId + statusMasterId)..." -ForegroundColor Yellow
$updateBody = @{
    title = "UPDATED - Update Test Complaint"
    description = "Updated description with correct fields"
    categoryId = "a4e6d993-ea9b-442f-a803-e61356c56760"
    priorityMasterId = $HighPriorityId
    statusMasterId = $InProgressStatusId
    tags = "test,updated"
} | ConvertTo-Json

try {
    $updateResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$complaintId" `
        -Method PUT `
        -Headers $headers `
        -Body $updateBody

    Write-Host "   SUCCESS: Complaint updated!" -ForegroundColor Green
    Write-Host "   New title: $($updateResponse.data.title)" -ForegroundColor White
    Write-Host "   New priority: $($updateResponse.data.priority)" -ForegroundColor White
    Write-Host "   New status: $($updateResponse.data.status)" -ForegroundColor White
} catch {
    Write-Host "   FAILED: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Error details: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Test Complete" -ForegroundColor Cyan
