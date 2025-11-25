# Create Test Complaint for E2E Testing
$ErrorActionPreference = "Stop"

Write-Host "Creating test complaint..." -ForegroundColor Cyan

# Login as complainant
Write-Host "[1/2] Logging in as test complainant..."
$loginBody = @{
    email = "test.complainant@e2e.local"
    password = "Nav@123"
} | ConvertTo-Json

$authResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/public" -Method POST -Body $loginBody -ContentType "application/json" -ErrorAction SilentlyContinue

if (-not $authResponse) {
    # Try regular login endpoint
    $authResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $authResponse.data.token
    Write-Host "  Logged in with token"

    # Create complaint
    Write-Host "[2/2] Creating complaint..."
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    $complaintBody = @{
        title = "E2E Test Complaint - System Performance"
        description = "This is a test complaint for E2E testing. The system experiences slow response times during peak hours."
        email = "test.complainant@e2e.local"
        phone = "1234567890"
    } | ConvertTo-Json

    try {
        $complaintResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" -Method POST -Headers $headers -Body $complaintBody
        Write-Host "SUCCESS! Complaint created:" -ForegroundColor Green
        Write-Host "  ID: $($complaintResponse.data.id)"
        Write-Host "  Title: $($complaintResponse.data.title)"
    } catch {
        Write-Host "Failed to create complaint: $_" -ForegroundColor Red
        Write-Host "Error details: $($_.Exception.Message)"
    }
} else {
    Write-Host "Public complaint submission available"
}

Write-Host "`nDone! You can now run the E2E tests." -ForegroundColor Green
