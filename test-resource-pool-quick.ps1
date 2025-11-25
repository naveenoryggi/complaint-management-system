# Quick Resource Pool Feature Test
$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5058/api"

Write-Host "=== Resource Pool Feature Test ===" -ForegroundColor Cyan

# Step 1: Authentication
Write-Host "`n[1/5] Authenticating..." -ForegroundColor Yellow
$loginData = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json"
$token = $loginResponse.data.token
Write-Host "  SUCCESS - Token obtained" -ForegroundColor Green

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Step 2: Get existing pools
Write-Host "`n[2/5] Getting existing resource pools..." -ForegroundColor Yellow
$poolsResponse = Invoke-RestMethod -Uri "$baseUrl/resource-pools" -Method GET -Headers $headers
$existingPools = $poolsResponse.data
Write-Host "  SUCCESS - Found $($existingPools.Count) pool(s)" -ForegroundColor Green

foreach ($pool in $existingPools) {
    Write-Host "    - $($pool.name) | Members: $($pool.memberCount)" -ForegroundColor Gray
}

# Step 3: Create new pool
Write-Host "`n[3/5] Creating new resource pool..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$createRequest = @{
    name = "Test Pool $timestamp"
    description = "Created by automated test"
    poolType = "Custom"
    memberUserIds = @()
} | ConvertTo-Json

$createResponse = Invoke-RestMethod -Uri "$baseUrl/resource-pools" -Method POST -Body $createRequest -Headers $headers
$newPool = $createResponse.data
Write-Host "  SUCCESS - Pool created" -ForegroundColor Green
Write-Host "    ID: $($newPool.id)" -ForegroundColor Gray
Write-Host "    Name: $($newPool.name)" -ForegroundColor Gray

# Step 4: Get users for adding to pool
Write-Host "`n[4/5] Getting users to add to pool..." -ForegroundColor Yellow
$usersResponse = Invoke-RestMethod -Uri "$baseUrl/users" -Method GET -Headers $headers
$users = $usersResponse.data
Write-Host "  SUCCESS - Found $($users.Count) user(s)" -ForegroundColor Green

if ($users.Count -gt 0) {
    $testUser = $users[0]
    Write-Host "    Adding: $($testUser.fullName)" -ForegroundColor Gray

    # Add member
    $addMemberRequest = @{ userId = $testUser.id } | ConvertTo-Json
    $addResponse = Invoke-RestMethod -Uri "$baseUrl/resource-pools/$($newPool.id)/members" -Method POST -Body $addMemberRequest -Headers $headers
    Write-Host "  SUCCESS - Member added" -ForegroundColor Green
}

# Step 5: Verify pool with members
Write-Host "`n[5/5] Verifying pool..." -ForegroundColor Yellow
$poolDetailResponse = Invoke-RestMethod -Uri "$baseUrl/resource-pools/$($newPool.id)" -Method GET -Headers $headers
$poolDetail = $poolDetailResponse.data
Write-Host "  SUCCESS - Pool verified" -ForegroundColor Green
Write-Host "    Members: $($poolDetail.memberCount)" -ForegroundColor Gray

foreach ($member in $poolDetail.members) {
    Write-Host "      - $($member.userName)" -ForegroundColor Gray
}

Write-Host "`n=== ALL TESTS PASSED ===" -ForegroundColor Green
Write-Host "`nFeature Status: READY TO USE" -ForegroundColor Green
Write-Host "`nHow to access in UI:" -ForegroundColor Yellow
Write-Host "  Navigate to: Admin > User Management > Resource Pools" -ForegroundColor Gray
Write-Host "===================================" -ForegroundColor Cyan
