# Script to check complaint numbers

$apiUrl = "http://localhost:5058"

# Authenticate
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

Write-Host "Authenticating..." -ForegroundColor Cyan
$loginResponse = Invoke-RestMethod -Uri "$apiUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get all complaints
Write-Host "Fetching all complaints..." -ForegroundColor Cyan
$complaintsResponse = Invoke-RestMethod -Uri "$apiUrl/api/complaints?pageSize=2000" -Method GET -Headers $headers

# Filter for 2025 complaints
$complaints2025 = $complaintsResponse.data.items | Where-Object { $_.complaintNumber -like "CMP-2025-*" } |
    Select-Object id, complaintNumber, @{Name="Number";Expression={[int]($_.complaintNumber -replace "CMP-2025-","")}} |
    Sort-Object Number -Descending

Write-Host "`nTotal 2025 complaints: $($complaints2025.Count)" -ForegroundColor Yellow
Write-Host "`nTop 10 complaint numbers:" -ForegroundColor Cyan
$complaints2025 | Select-Object -First 10 | Format-Table -AutoSize

if ($complaints2025.Count -gt 0) {
    $maxNumber = $complaints2025[0].Number
    Write-Host "`nMaximum complaint number: CMP-2025-$maxNumber" -ForegroundColor Green
    Write-Host "Next number should be: CMP-2025-$($maxNumber + 1)" -ForegroundColor Green
}
