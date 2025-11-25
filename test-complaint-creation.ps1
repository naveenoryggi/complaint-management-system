# Test complaint creation endpoint
$ErrorActionPreference = "Stop"

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Testing Complaint Creation" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# First, login to get token
Write-Host "1. Logging in..." -ForegroundColor Yellow
$loginUrl = "http://localhost:5058/api/auth/login"
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "   Login successful! Token obtained." -ForegroundColor Green
} catch {
    Write-Host "   Login failed: $_" -ForegroundColor Red
    exit 1
}

# Get categories
Write-Host ""
Write-Host "2. Fetching categories..." -ForegroundColor Yellow
$categoriesUrl = "http://localhost:5058/api/categories"
$headers = @{
    "Authorization" = "Bearer $token"
}

try {
    $categoriesResponse = Invoke-RestMethod -Uri $categoriesUrl -Method Get -Headers $headers
    if ($categoriesResponse.data.Count -gt 0) {
        $categoryId = $categoriesResponse.data[0].id
        Write-Host "   Found category: $($categoriesResponse.data[0].name) (ID: $categoryId)" -ForegroundColor Green
    } else {
        Write-Host "   No categories found!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   Failed to fetch categories: $_" -ForegroundColor Red
    exit 1
}

# Create complaint
Write-Host ""
Write-Host "3. Creating complaint..." -ForegroundColor Yellow
$complaintUrl = "http://localhost:5058/api/complaints"
$complaintBody = @{
    title = "Test Complaint - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    description = "This is a test complaint to verify the POST endpoint is working correctly."
    categoryId = $categoryId
    priority = 1
    isAnonymous = $false
    tags = "test,automated"
    employeeCode = "EMP001"
    contactEmail = "test@example.com"
    contactPhone = "1234567890"
    preferredContactMethod = 0
} | ConvertTo-Json

Write-Host "Request URL: $complaintUrl" -ForegroundColor Gray
Write-Host "Request Body:" -ForegroundColor Gray
Write-Host $complaintBody -ForegroundColor Gray
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri $complaintUrl -Method Post -Body $complaintBody -ContentType "application/json" -Headers $headers

    if ($response.isSuccess) {
        Write-Host "   SUCCESS! Complaint created:" -ForegroundColor Green
        Write-Host "   - Complaint Number: $($response.data.complaintNumber)" -ForegroundColor Green
        Write-Host "   - ID: $($response.data.id)" -ForegroundColor Green
        Write-Host "   - Title: $($response.data.title)" -ForegroundColor Green
        Write-Host "   - Status: $($response.data.status)" -ForegroundColor Green
        Write-Host ""
        Write-Host "Full Response:" -ForegroundColor Gray
        Write-Host ($response | ConvertTo-Json -Depth 3) -ForegroundColor Gray
    } else {
        Write-Host "   FAILED: $($response.message)" -ForegroundColor Red
        Write-Host ($response | ConvertTo-Json -Depth 3) -ForegroundColor Red
    }
} catch {
    Write-Host "   ERROR creating complaint:" -ForegroundColor Red
    Write-Host "   Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "   Status Description: $($_.Exception.Response.StatusDescription)" -ForegroundColor Red
    Write-Host "   Error Message: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.ErrorDetails) {
        Write-Host "   Error Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "Full Error:" -ForegroundColor Gray
    Write-Host $_ -ForegroundColor Gray
    exit 1
}

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Test completed successfully!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Cyan
