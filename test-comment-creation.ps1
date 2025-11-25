# Test Comment Creation to see exact error
$BaseUrl = "http://localhost:5058"

# Login
$loginBody = @{
    username = "admin@test.com"
    password = "Admin@123"
} | ConvertTo-Json

Write-Host "Logging in..." -ForegroundColor Cyan
$loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token
Write-Host "✓ Login successful" -ForegroundColor Green

$authHeaders = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get first complaint
Write-Host "`nGetting complaints..." -ForegroundColor Cyan
$complaints = Invoke-RestMethod -Uri "$BaseUrl/api/complaints?pageSize=1" -Headers $authHeaders
$complaintId = $complaints.data.items[0].id
Write-Host "✓ Got complaint: $complaintId" -ForegroundColor Green

# Try to create comment
Write-Host "`nTrying to create comment..." -ForegroundColor Cyan
$commentBody = @{
    comment = "Test comment for debugging"
    isInternal = $false
} | ConvertTo-Json

Write-Host "Request Body: $commentBody" -ForegroundColor Yellow
Write-Host "URL: $BaseUrl/api/complaints/$complaintId/comments" -ForegroundColor Yellow

try {
    $result = Invoke-RestMethod -Uri "$BaseUrl/api/complaints/$complaintId/comments" -Method Post -Headers $authHeaders -Body $commentBody
    Write-Host "✓ SUCCESS: Comment created" -ForegroundColor Green
    Write-Host "Response: $($result | ConvertTo-Json -Depth 5)" -ForegroundColor Green
} catch {
    Write-Host "FAILED" -ForegroundColor Red
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorMsg = $_.Exception.Message
    $errorDetails = $_.ErrorDetails.Message
    Write-Host "Status Code: $statusCode" -ForegroundColor Red
    Write-Host "Error: $errorMsg" -ForegroundColor Red
    if ($errorDetails) {
        Write-Host "Details: $errorDetails" -ForegroundColor Yellow
    }
}
