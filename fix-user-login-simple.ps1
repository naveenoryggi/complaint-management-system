# Fix User Login Issues - Simplified Version
param()

$ErrorActionPreference = "Continue"

$baseUrl = "http://localhost:5000/api"
$token = (Get-Content ".test-token" -Raw).Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "User Login Fix Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# User 1: Nav Nainital (Complainant)
$user1Id = "fd0073b8-fc95-4a49-867c-6ffb38b7d177"
$user1Email = "nav_nainital@yahoo.com"
$user1Password = "Nav@12345"

# User 2: Naveen Chandra (Handler)
$user2Id = "94c91ae3-72ef-4b53-8057-08de0e0582b5"
$user2Email = "naveen.chandra@oryggitech.com"
$user2Password = "Naveen@12345"

$adminId = "f56d8d03-e382-454b-bf7d-fa8236c125c3"

function Test-UserLogin {
    param($email, $password)

    try {
        $loginBody = @{
            email = $email
            password = $password
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
        return $response.isSuccess
    } catch {
        return $false
    }
}

function Get-UserDetails {
    param($userId)

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/users/$userId" -Method Get -Headers $headers
        return $response.data
    } catch {
        return $null
    }
}

function Set-UserPassword {
    param($userId, $password)

    try {
        $body = @{
            password = $password
            setBy = $adminId
            mustChangeOnNextLogin = $false
            sendEmail = $false
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri "$baseUrl/password-management/users/$userId/set-password" -Method Post -Headers $headers -Body $body
        return $response.isSuccess
    } catch {
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Yellow
        return $false
    }
}

function Assign-UserRole {
    param($userId, $roleCode)

    try {
        # Get role ID
        $rolesResponse = Invoke-RestMethod -Uri "$baseUrl/complaint-roles" -Method Get -Headers $headers
        $role = $rolesResponse.data | Where-Object { $_.code -eq $roleCode }

        if (-not $role) {
            Write-Host "  Role not found: $roleCode" -ForegroundColor Red
            return $false
        }

        $body = @{
            userId = $userId
            complaintRoleId = $role.id
            isPrimary = $true
            effectiveFrom = (Get-Date).ToString("o")
            notes = "Auto-assigned for login fix"
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri "$baseUrl/user-roles" -Method Post -Headers $headers -Body $body
        return $response.isSuccess
    } catch {
        Write-Host "  Error assigning role: $($_.Exception.Message)" -ForegroundColor Yellow
        return $false
    }
}

# Process User 1
Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "USER 1: Nav Nainital (Complainant)" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow

Write-Host "1. Checking user..." -ForegroundColor Cyan
$user1 = Get-UserDetails -userId $user1Id
if ($user1) {
    Write-Host "   Found: $($user1.fullName)" -ForegroundColor Green
    Write-Host "   Active: $($user1.isActive)" -ForegroundColor Gray
    Write-Host "   Roles: $($user1.roles.Count)" -ForegroundColor Gray
} else {
    Write-Host "   NOT FOUND" -ForegroundColor Red
}

Write-Host "2. Setting password..." -ForegroundColor Cyan
$pwSet1 = Set-UserPassword -userId $user1Id -password $user1Password
if ($pwSet1) {
    Write-Host "   Password set successfully" -ForegroundColor Green
} else {
    Write-Host "   Failed to set password via API" -ForegroundColor Red
}

Write-Host "3. Checking role..." -ForegroundColor Cyan
if ($user1) {
    $hasRole = $user1.roles | Where-Object { $_.roleCode -eq "COMPLAINANT" }
    if ($hasRole) {
        Write-Host "   Has COMPLAINANT role" -ForegroundColor Green
    } else {
        Write-Host "   Missing COMPLAINANT role, assigning..." -ForegroundColor Yellow
        $roleAssigned = Assign-UserRole -userId $user1Id -roleCode "COMPLAINANT"
        if ($roleAssigned) {
            Write-Host "   Role assigned successfully" -ForegroundColor Green
        }
    }
}

Write-Host "4. Testing login..." -ForegroundColor Cyan
$login1 = Test-UserLogin -email $user1Email -password $user1Password
if ($login1) {
    Write-Host "   LOGIN SUCCESSFUL!" -ForegroundColor Green
} else {
    Write-Host "   LOGIN FAILED!" -ForegroundColor Red
}

# Process User 2
Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "USER 2: Naveen Chandra (Handler)" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow

Write-Host "1. Checking user..." -ForegroundColor Cyan
$user2 = Get-UserDetails -userId $user2Id
if ($user2) {
    Write-Host "   Found: $($user2.fullName)" -ForegroundColor Green
    Write-Host "   Active: $($user2.isActive)" -ForegroundColor Gray
    Write-Host "   Roles: $($user2.roles.Count)" -ForegroundColor Gray
} else {
    Write-Host "   NOT FOUND" -ForegroundColor Red
}

Write-Host "2. Setting password..." -ForegroundColor Cyan
$pwSet2 = Set-UserPassword -userId $user2Id -password $user2Password
if ($pwSet2) {
    Write-Host "   Password set successfully" -ForegroundColor Green
} else {
    Write-Host "   Failed to set password via API" -ForegroundColor Red
}

Write-Host "3. Checking role..." -ForegroundColor Cyan
if ($user2) {
    $hasHandlerRole = $user2.roles | Where-Object { $_.roleType -eq "Handler" }
    if ($hasHandlerRole) {
        Write-Host "   Has Handler role: $($hasHandlerRole.roleName)" -ForegroundColor Green
    } else {
        Write-Host "   Missing Handler role, assigning LEVEL1_HANDLER..." -ForegroundColor Yellow
        $roleAssigned = Assign-UserRole -userId $user2Id -roleCode "LEVEL1_HANDLER"
        if ($roleAssigned) {
            Write-Host "   Role assigned successfully" -ForegroundColor Green
        }
    }
}

Write-Host "4. Testing login..." -ForegroundColor Cyan
$login2 = Test-UserLogin -email $user2Email -password $user2Password
if ($login2) {
    Write-Host "   LOGIN SUCCESSFUL!" -ForegroundColor Green
} else {
    Write-Host "   LOGIN FAILED!" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "User 1 (nav_nainital@yahoo.com): " -NoNewline
if ($login1) {
    Write-Host "SUCCESS" -ForegroundColor Green
} else {
    Write-Host "FAILED" -ForegroundColor Red
}

Write-Host "User 2 (naveen.chandra@oryggitech.com): " -NoNewline
if ($login2) {
    Write-Host "SUCCESS" -ForegroundColor Green
} else {
    Write-Host "FAILED" -ForegroundColor Red
}

Write-Host ""
