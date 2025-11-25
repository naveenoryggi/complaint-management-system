$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "Testing Update Complaint Endpoint..." -ForegroundColor Cyan
Write-Host ""

# First, create a test complaint
Write-Host "1. Creating test complaint..." -ForegroundColor Yellow
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

# Get priority master IDs
$priorities = Invoke-RestMethod -Uri "http://localhost:5000/api/complaintprioritymasters" -Headers $headers
$highPriorityId = ($priorities.data | Where-Object { $_.name -eq "High" }).id

# Now try to update it with CORRECT field names
Write-Host "2. Updating complaint with correct field names..." -ForegroundColor Yellow
$updateBody = @{
    title = "UPDATED - Update Test Complaint"
    description = "Updated description with correct fields"
    categoryId = "a4e6d993-ea9b-442f-a803-e61356c56760"
    priorityMasterId = $highPriorityId
    tags = "test,updated"
} | ConvertTo-Json

try {
    $updateResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$complaintId" `
        -Method PUT `
        -Headers $headers `
        -Body $updateBody

    Write-Host "   SUCCESS: Complaint updated!" -ForegroundColor Green
    Write-Host "   New title: $($updateResponse.data.title)" -ForegroundColor White
} catch {
    Write-Host "   FAILED: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Error details: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Test Complete" -ForegroundColor Cyan
