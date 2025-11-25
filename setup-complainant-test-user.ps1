# Setup Complainant Test User for E2E Tests
# This script creates/configures the complainant test user: nav_nainital@yahoo.com

$ErrorActionPreference = "Continue"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "COMPLAINANT TEST USER SETUP" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Configuration
$baseUrl = "http://localhost:5000/api"
$adminEmail = "admin@complaintmanagement.com"
$adminPassword = "Admin@123"
$complainantEmail = "nav_nainital@yahoo.com"
$complainantPassword = "Nav@123"

# Step 1: Admin Login
Write-Host "[1/6] Logging in as Admin..." -ForegroundColor Yellow
$loginBody = @{
    email = $adminEmail
    password = $adminPassword
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    $companyId = $loginResponse.data.user.companyId
    Write-Host "      ✓ Admin logged in successfully" -ForegroundColor Green
    Write-Host "      Company ID: $companyId" -ForegroundColor Gray
} catch {
    Write-Host "      ✗ Admin login failed: $_" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Step 2: Get Complainant Role ID
Write-Host "`n[2/6] Getting Complainant role ID..." -ForegroundColor Yellow
try {
    $rolesResponse = Invoke-RestMethod -Uri "$baseUrl/roles" -Method GET -Headers $headers
    $complainantRole = $rolesResponse.data | Where-Object { $_.code -eq "COMPLAINANT" -or $_.roleType -eq "Complainant" -or $_.name -like "*Complainant*" }

    if ($complainantRole) {
        $complainantRoleId = $complainantRole[0].id
        Write-Host "      ✓ Complainant role found" -ForegroundColor Green
        Write-Host "      Role ID: $complainantRoleId" -ForegroundColor Gray
        Write-Host "      Role Name: $($complainantRole[0].name)" -ForegroundColor Gray
    } else {
        Write-Host "      ✗ Complainant role not found" -ForegroundColor Red
        Write-Host "      Available roles:" -ForegroundColor Gray
        $rolesResponse.data | ForEach-Object { Write-Host "        - $($_.name) ($($_.code))" -ForegroundColor Gray }
        exit 1
    }
} catch {
    Write-Host "      ✗ Failed to get roles: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Check if user exists
Write-Host "`n[3/6] Checking if user exists..." -ForegroundColor Yellow
try {
    $usersResponse = Invoke-RestMethod -Uri "$baseUrl/users" -Method GET -Headers $headers
    $existingUser = $usersResponse.data | Where-Object { $_.email -eq $complainantEmail }

    if ($existingUser) {
        $userId = $existingUser.id
        Write-Host "      ℹ User already exists (ID: $userId)" -ForegroundColor Cyan
        Write-Host "      Current roles: $($existingUser.roles.Count)" -ForegroundColor Gray
    } else {
        Write-Host "      ℹ User does not exist, will create new" -ForegroundColor Cyan
        $userId = $null
    }
} catch {
    Write-Host "      ⚠ Could not check existing users: $_" -ForegroundColor Yellow
    $userId = $null
}

# Step 4: Create user if doesn't exist
if (-not $userId) {
    Write-Host "`n[4/6] Creating new user..." -ForegroundColor Yellow
    $createUserBody = @{
        companyId = $companyId
        employeeCode = "NAV001"
        firstName = "Naveen"
        lastName = "Chandra"
        email = $complainantEmail
        phone = "1234567890"
        jobTitle = "Test Complainant"
    } | ConvertTo-Json

    try {
        $createResponse = Invoke-RestMethod -Uri "$baseUrl/users" -Method POST -Headers $headers -Body $createUserBody
        $userId = $createResponse.data.id
        Write-Host "      ✓ User created successfully" -ForegroundColor Green
        Write-Host "      User ID: $userId" -ForegroundColor Gray
    } catch {
        Write-Host "      ✗ Failed to create user: $_" -ForegroundColor Red
        Write-Host "      Error details: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "`n[4/6] Skipping user creation (already exists)" -ForegroundColor Gray
}

# Step 5: Set password
Write-Host "`n[5/6] Setting user password..." -ForegroundColor Yellow
$setPasswordBody = @{
    userId = $userId
    password = $complainantPassword
    mustChangeOnNextLogin = $false
    sendEmail = $false
} | ConvertTo-Json

try {
    $passwordResponse = Invoke-RestMethod -Uri "$baseUrl/password/set" -Method POST -Headers $headers -Body $setPasswordBody
    Write-Host "      ✓ Password set successfully" -ForegroundColor Green
} catch {
    Write-Host "      ✗ Failed to set password: $_" -ForegroundColor Red
    Write-Host "      Error details: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 6: Assign Complainant role
Write-Host "`n[6/6] Assigning Complainant role..." -ForegroundColor Yellow
$assignRoleBody = @{
    userId = $userId
    roleId = $complainantRoleId
    isPrimary = $true
} | ConvertTo-Json

try {
    $roleResponse = Invoke-RestMethod -Uri "$baseUrl/users/$userId/roles" -Method POST -Headers $headers -Body $assignRoleBody
    Write-Host "      ✓ Complainant role assigned successfully" -ForegroundColor Green
} catch {
    Write-Host "      ⚠ Failed to assign role: $_" -ForegroundColor Yellow
    Write-Host "      Error details: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "      Note: User may already have this role" -ForegroundColor Gray
}

# Verification: Test login
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "VERIFICATION" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Testing complainant login..." -ForegroundColor Yellow
$testLoginBody = @{
    email = $complainantEmail
    password = $complainantPassword
} | ConvertTo-Json

try {
    $testLoginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $testLoginBody -ContentType "application/json"
    Write-Host "✓ Complainant login successful!" -ForegroundColor Green
    Write-Host "  User: $($testLoginResponse.data.user.fullName)" -ForegroundColor Gray
    Write-Host "  Email: $($testLoginResponse.data.user.email)" -ForegroundColor Gray
    Write-Host "  Roles: $($testLoginResponse.data.user.roles.Count)" -ForegroundColor Gray
    $testLoginResponse.data.user.roles | ForEach-Object {
        Write-Host "    - $($_.roleName) ($($_.roleCode))" -ForegroundColor Gray
    }
    Write-Host "  Permissions: $($testLoginResponse.data.user.permissions.Count)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Complainant login failed!" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nThis means the user configuration is incomplete." -ForegroundColor Yellow
    Write-Host "Check the error above for details." -ForegroundColor Yellow
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SETUP COMPLETE!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Test User Credentials:" -ForegroundColor Cyan
Write-Host "  Email: $complainantEmail" -ForegroundColor White
Write-Host "  Password: $complainantPassword" -ForegroundColor White
Write-Host "`nYou can now run E2E tests:" -ForegroundColor Cyan
Write-Host "  node phase1-comprehensive-e2e-test-fixed.js" -ForegroundColor White
Write-Host ""
