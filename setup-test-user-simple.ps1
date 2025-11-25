# Simple Complainant Test User Setup
$ErrorActionPreference = "Stop"

Write-Host "Setting up complainant test user..." -ForegroundColor Cyan

# Step 1: Login as admin
Write-Host "[1/5] Admin login..."
$login = @{ email="admin@complaintmanagement.com"; password="Admin@123" } | ConvertTo-Json
$auth = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $login -ContentType "application/json"
$token = $auth.data.token
$companyId = $auth.data.user.companyId
Write-Host "  Token received, Company: $companyId"

$headers = @{ Authorization="Bearer $token"; "Content-Type"="application/json" }

# Step 2: Get Complainant role
Write-Host "[2/5] Getting Complainant role..."
$roles = Invoke-RestMethod -Uri "http://localhost:5000/api/roles" -Method GET -Headers $headers
$complainantRole = $roles.data | Where-Object { $_.name -like "*Complainant*" } | Select-Object -First 1
if (-not $complainantRole) {
    Write-Host "ERROR: Complainant role not found!" -ForegroundColor Red
    exit 1
}
$roleId = $complainantRole.id
Write-Host "  Role ID: $roleId ($($complainantRole.name))"

# Step 3: Try to create user
Write-Host "[3/5] Creating user..."
$newUser = @{
    companyId=$companyId
    employeeCode="NAV001"
    firstName="Naveen"
    lastName="Chandra"
    email="nav_nainital@yahoo.com"
    phone="1234567890"
    jobTitle="Test Complainant"
} | ConvertTo-Json

try {
    $userResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/users" -Method POST -Headers $headers -Body $newUser
    $userId = $userResponse.data.id
    Write-Host "  User created: $userId"
} catch {
    if ($_ -match "duplicate|already exists") {
        Write-Host "  User already exists, finding..."
        $users = Invoke-RestMethod -Uri "http://localhost:5000/api/users" -Method GET -Headers $headers
        $existing = $users.data | Where-Object { $_.email -eq "nav_nainital@yahoo.com" }
        if ($existing) {
            $userId = $existing.id
            Write-Host "  Found existing user: $userId"
        } else {
            Write-Host "  ERROR: User exists but not found in list (different company?)" -ForegroundColor Red
            Write-Host "  Creating with different email..." -ForegroundColor Yellow
            $newUser2 = @{
                companyId=$companyId
                employeeCode="NAV002"
                firstName="Naveen"
                lastName="Test"
                email="naveen.test@complaint.local"
                phone="1234567890"
                jobTitle="Test Complainant"
            } | ConvertTo-Json
            $userResponse2 = Invoke-RestMethod -Uri "http://localhost:5000/api/users" -Method POST -Headers $headers -Body $newUser2
            $userId = $userResponse2.data.id
            Write-Host "  Created user with alternative email: $userId"
            Write-Host "  NOTE: Update test config to use: naveen.test@complaint.local" -ForegroundColor Yellow
        }
    } else {
        throw
    }
}

# Step 4: Set password
Write-Host "[4/5] Setting password..."
$pwd = @{ userId=$userId; password="Nav@123"; mustChangeOnNextLogin=$false; sendEmail=$false } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/password/set" -Method POST -Headers $headers -Body $pwd | Out-Null
Write-Host "  Password set"

# Step 5: Assign role
Write-Host "[5/5] Assigning Complainant role..."
$roleAssign = @{ userId=$userId; roleId=$roleId; isPrimary=$true } | ConvertTo-Json
try {
    Invoke-RestMethod -Uri "http://localhost:5000/api/users/$userId/roles" -Method POST -Headers $headers -Body $roleAssign | Out-Null
    Write-Host "  Role assigned"
} catch {
    Write-Host "  Role assignment issue (may already have role): $_" -ForegroundColor Yellow
}

# Verify login
Write-Host "`nVerifying login..."
$testLogin = @{ email="nav_nainital@yahoo.com"; password="Nav@123" } | ConvertTo-Json
try {
    $testAuth = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $testLogin -ContentType "application/json"
    Write-Host "SUCCESS! Complainant can log in" -ForegroundColor Green
    Write-Host "  User: $($testAuth.data.user.fullName)"
    Write-Host "  Roles: $($testAuth.data.user.roles.Count)"
} catch {
    # Try alternative email
    Write-Host "Primary email failed, trying alternative..." -ForegroundColor Yellow
    $testLogin2 = @{ email="naveen.test@complaint.local"; password="Nav@123" } | ConvertTo-Json
    $testAuth2 = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $testLogin2 -ContentType "application/json"
    Write-Host "SUCCESS! Alternative email works" -ForegroundColor Green
    Write-Host "  Email: naveen.test@complaint.local"
    Write-Host "  Password: Nav@123"
    Write-Host "`nIMPORTANT: Update phase1-comprehensive-e2e-test-fixed.js" -ForegroundColor Yellow
    Write-Host "  Change complainant.email to: naveen.test@complaint.local" -ForegroundColor Yellow
}

Write-Host "`nSetup complete!" -ForegroundColor Green
