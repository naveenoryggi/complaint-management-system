# Check Master Data and Roles
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

Write-Host "`n=== ROLES ===" -ForegroundColor Cyan
try {
    $roles = Invoke-RestMethod -Uri "$baseUrl/api/roles" -Headers $headers -Method Get
    Write-Host "Total Roles: $($roles.data.Count)" -ForegroundColor White
    $roles.data | ForEach-Object {
        Write-Host "  - $($_.name) [$($_.code)] (Type: $($_.roleType), Level: $($_.escalationLevel))" -ForegroundColor Gray
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host "`n=== PRIORITY MASTER ===" -ForegroundColor Cyan
try {
    $priorities = Invoke-RestMethod -Uri "$baseUrl/api/ComplaintPriorityMaster" -Headers $headers -Method Get
    Write-Host "Total Priorities: $($priorities.data.Count)" -ForegroundColor White
    $priorities.data | ForEach-Object {
        Write-Host "  - $($_.name) (Level: $($_.level), SLA: $($_.defaultSlaHours)h)" -ForegroundColor Gray
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host "`n=== STATUS MASTER ===" -ForegroundColor Cyan
try {
    $statuses = Invoke-RestMethod -Uri "$baseUrl/api/ComplaintStatusMaster" -Headers $headers -Method Get
    Write-Host "Total Statuses: $($statuses.data.Count)" -ForegroundColor White
    $statuses.data | ForEach-Object {
        Write-Host "  - $($_.name) [$($_.code)] (Type: $($_.statusType))" -ForegroundColor Gray
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host "`n=== BRANCHES ===" -ForegroundColor Cyan
try {
    $branches = Invoke-RestMethod -Uri "$baseUrl/api/branches" -Headers $headers -Method Get
    Write-Host "Total Branches: $($branches.data.Count)" -ForegroundColor White
    $branches.data | Select-Object -First 5 | ForEach-Object {
        Write-Host "  - $($_.name) [$($_.code)]" -ForegroundColor Gray
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host "`n=== DEPARTMENTS ===" -ForegroundColor Cyan
try {
    $departments = Invoke-RestMethod -Uri "$baseUrl/api/departments" -Headers $headers -Method Get
    Write-Host "Total Departments: $($departments.data.Count)" -ForegroundColor White
    $departments.data | Select-Object -First 5 | ForEach-Object {
        Write-Host "  - $($_.name) [$($_.code)]" -ForegroundColor Gray
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host "`n=== RESOURCE POOLS ===" -ForegroundColor Cyan
try {
    $resourcePools = Invoke-RestMethod -Uri "$baseUrl/api/resource-pools" -Headers $headers -Method Get
    Write-Host "Total Resource Pools: $($resourcePools.data.Count)" -ForegroundColor White
    $resourcePools.data | ForEach-Object {
        Write-Host "  - $($_.name) (Assignment Method: $($_.assignmentMethod))" -ForegroundColor Gray
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
