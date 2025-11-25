# Script to delete duplicate complaint CMP-2025-1060

$apiUrl = "http://localhost:5058"

# Authenticate
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

Write-Host "Authenticating..." -ForegroundColor Cyan
$loginResponse = Invoke-RestMethod -Uri "$apiUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token

Write-Host "Token obtained successfully" -ForegroundColor Green

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get all complaints to find CMP-2025-1060
Write-Host "`nSearching for complaint CMP-2025-1060..." -ForegroundColor Cyan
$complaintsResponse = Invoke-RestMethod -Uri "$apiUrl/api/complaints?pageSize=2000" -Method GET -Headers $headers

$duplicateComplaint = $complaintsResponse.data.items | Where-Object { $_.complaintNumber -eq "CMP-2025-1060" }

if ($duplicateComplaint) {
    Write-Host "Found complaint CMP-2025-1060 with ID: $($duplicateComplaint.id)" -ForegroundColor Yellow

    # Delete the complaint
    Write-Host "Deleting complaint..." -ForegroundColor Cyan
    try {
        $deleteResponse = Invoke-RestMethod -Uri "$apiUrl/api/complaints/$($duplicateComplaint.id)" -Method DELETE -Headers $headers
        Write-Host "SUCCESS: Complaint CMP-2025-1060 deleted successfully!" -ForegroundColor Green
        Write-Host "Response: $($deleteResponse.message)" -ForegroundColor Green
    }
    catch {
        Write-Host "ERROR: Failed to delete complaint: $($_.Exception.Message)" -ForegroundColor Red
    }
}
else {
    Write-Host "Complaint CMP-2025-1060 not found in database" -ForegroundColor Yellow
}
