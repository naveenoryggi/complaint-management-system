# Find Naveen Chandra user
$baseUrl = "http://localhost:5000"

$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$users = Invoke-RestMethod -Uri "$baseUrl/api/users" -Headers $headers -Method Get

Write-Host "Looking for Naveen Chandra and related users..." -ForegroundColor Cyan

# Search for employee code 10238 (from the earlier data)
$naveen = $users.data | Where-Object { $_.employeeCode -eq '10238' }
if ($naveen) {
    Write-Host "`nFound employee 10238:" -ForegroundColor Green
    Write-Host "  Email: $($naveen.email)" -ForegroundColor White
    Write-Host "  Full Name: $($naveen.fullName)" -ForegroundColor White
    Write-Host "  Employee Code: $($naveen.employeeCode)" -ForegroundColor White
    Write-Host "  Job Title: $($naveen.jobTitle)" -ForegroundColor White
    Write-Host "  Is Active: $($naveen.isActive)" -ForegroundColor White
}

# Also check for any users with "NAVEEN" in name
$naveenUsers = $users.data | Where-Object { $_.fullName -like '*NAVEEN*' }
if ($naveenUsers) {
    Write-Host "`nAll users with 'NAVEEN' in name:" -ForegroundColor Yellow
    $naveenUsers | ForEach-Object {
        Write-Host "  - $($_.email) | $($_.fullName) | Code: $($_.employeeCode)" -ForegroundColor Gray
    }
}

# Check categories and priorities
Write-Host "`n`nChecking Categories..." -ForegroundColor Cyan
$categories = Invoke-RestMethod -Uri "$baseUrl/api/categories" -Headers $headers -Method Get
Write-Host "Total Categories: $($categories.data.Count)" -ForegroundColor White
$categories.data | Select-Object -First 10 | ForEach-Object {
    Write-Host "  - $($_.name) [$($_.code)]" -ForegroundColor Gray
}

Write-Host "`n`nChecking Priority Masters..." -ForegroundColor Cyan
$priorities = Invoke-RestMethod -Uri "$baseUrl/api/complaint-priority-master" -Headers $headers -Method Get
Write-Host "Total Priorities: $($priorities.data.Count)" -ForegroundColor White
$priorities.data | ForEach-Object {
    Write-Host "  - $($_.name) (Level: $($_.level), SLA Hours: $($_.defaultSlaHours))" -ForegroundColor Gray
}

Write-Host "`n`nChecking Status Masters..." -ForegroundColor Cyan
$statuses = Invoke-RestMethod -Uri "$baseUrl/api/complaint-status-master" -Headers $headers -Method Get
Write-Host "Total Statuses: $($statuses.data.Count)" -ForegroundColor White
$statuses.data | ForEach-Object {
    Write-Host "  - $($_.name) [$($_.code)] (Type: $($_.statusType))" -ForegroundColor Gray
}
