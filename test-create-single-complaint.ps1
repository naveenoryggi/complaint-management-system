$token = Get-Content '.test-token' -Raw
$headers = @{
    'Authorization' = "Bearer $token"
    'Content-Type' = 'application/json'
}

# Get category ID
$categories = Invoke-RestMethod -Uri 'http://localhost:5058/api/categories' -Headers $headers -Method Get
$categoryId = if ($categories.data) { $categories.data[0].id } else { $categories[0].id }

# Get complainant ID (admin user)
$users = Invoke-RestMethod -Uri 'http://localhost:5058/api/users' -Headers $headers -Method Get
$userId = if ($users.data) { $users.data[0].id } else { $users[0].id }

# Get company ID
$companies = Invoke-RestMethod -Uri 'http://localhost:5058/api/company' -Headers $headers -Method Get
$companyId = if ($companies.data) { $companies.data[0].id } else { $companies[0].id }

Write-Host "Creating test complaint..."
Write-Host "CategoryId: $categoryId"
Write-Host "UserId: $userId"
Write-Host "CompanyId: $companyId"

$complaintBody = @{
    title = "Test Complaint for Number Generation"
    description = "Testing the complaint number generation with database locking"
    categoryId = $categoryId
    complainantId = $userId
    companyId = $companyId
    priority = 1
    contactEmail = "test@test.com"
    contactPhone = "1234567890"
    preferredContactMethod = 0
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5058/api/complaints' -Headers $headers -Method Post -Body $complaintBody

    if ($response.data) {
        Write-Host "SUCCESS! Created complaint: $($response.data.complaintNumber)" -ForegroundColor Green
        Write-Host "Complaint ID: $($response.data.id)"
    } elseif ($response.complaintNumber) {
        Write-Host "SUCCESS! Created complaint: $($response.complaintNumber)" -ForegroundColor Green
        Write-Host "Complaint ID: $($response.id)"
    } else {
        Write-Host "Response: $($response | ConvertTo-Json -Depth 3)"
    }
} catch {
    Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails) {
        Write-Host "Details: $($_.ErrorDetails.Message)"
    }
}
