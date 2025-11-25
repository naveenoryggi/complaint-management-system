# Fix User Login Issues
# This script checks and fixes login issues for two test users

$baseUrl = "http://localhost:5000/api"
$token = (Get-Content ".test-token" -Raw).Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "User Login Fix Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# User details
$users = @(
    @{
        Name = "Nav Nainital (Complainant)"
        Id = "fd0073b8-fc95-4a49-867c-6ffb38b7d177"
        Email = "nav_nainital@yahoo.com"
        Password = "Nav@12345"
        RoleCode = "COMPLAINANT"
        RoleType = "Complainant"
    },
    @{
        Name = "Naveen Chandra (Handler)"
        Id = "94c91ae3-72ef-4b53-8057-08de0e0582b5"
        Email = "naveen.chandra@oryggitech.com"
        Password = "Naveen@12345"
        RoleCode = "LEVEL1_HANDLER"
        RoleType = "Handler"
    }
)

$results = @()

foreach ($user in $users) {
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "Processing: $($user.Name)" -ForegroundColor Yellow
    Write-Host "Email: $($user.Email)" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host ""

    $userResult = @{
        Name = $user.Name
        Email = $user.Email
        Issues = @()
        Fixes = @()
        Status = "Unknown"
    }

    # Step 1: Get user details
    Write-Host "[1/6] Checking user exists..." -ForegroundColor Cyan
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/users/$($user.Id)" -Method Get -Headers $headers
        if ($response.isSuccess) {
            Write-Host "  ✓ User found: $($response.data.fullName)" -ForegroundColor Green
            $userData = $response.data

            # Check if user is active
            if (-not $userData.isActive) {
                Write-Host "  ✗ User is INACTIVE" -ForegroundColor Red
                $userResult.Issues += "User is inactive"
            } else {
                Write-Host "  ✓ User is ACTIVE" -ForegroundColor Green
            }

            # Check roles
            Write-Host "  Current roles: $($userData.roles.Count)" -ForegroundColor Cyan
            foreach ($role in $userData.roles) {
                Write-Host "    - $($role.roleName) (Code: $($role.roleCode), Type: $($role.roleType))" -ForegroundColor Gray
            }
        } else {
            Write-Host "  ✗ User not found in system" -ForegroundColor Red
            $userResult.Issues += "User not found"
            $userResult.Status = "Not Found"
            $results += $userResult
            continue
        }
    } catch {
        Write-Host "  ✗ Error checking user: $($_.Exception.Message)" -ForegroundColor Red
        $userResult.Issues += "Error checking user: $($_.Exception.Message)"
        $userResult.Status = "Error"
        $results += $userResult
        continue
    }

    # Step 2: Get all roles to find the correct role ID
    Write-Host ""
    Write-Host "[2/6] Finding role ID for $($user.RoleType)..." -ForegroundColor Cyan
    $targetRole = $null
    try {
        $rolesResponse = Invoke-RestMethod -Uri "$baseUrl/complaint-roles" -Method Get -Headers $headers
        $targetRole = $rolesResponse.data | Where-Object { $_.code -eq $user.RoleCode }

        if ($targetRole) {
            Write-Host "  ✓ Found role: $($targetRole.name) (ID: $($targetRole.id))" -ForegroundColor Green
        } else {
            Write-Host "  ✗ Role not found: $($user.RoleCode)" -ForegroundColor Red
            $userResult.Issues += "Target role not found: $($user.RoleCode)"
        }
    } catch {
        Write-Host "  ✗ Error fetching roles: $($_.Exception.Message)" -ForegroundColor Red
        $userResult.Issues += "Error fetching roles: $($_.Exception.Message)"
    }

    # Step 3: Check if user has the correct role
    Write-Host ""
    Write-Host "[3/6] Checking role assignment..." -ForegroundColor Cyan
    $hasCorrectRole = $userData.roles | Where-Object { $_.roleCode -eq $user.RoleCode }

    if ($hasCorrectRole) {
        Write-Host "  ✓ User has correct role: $($hasCorrectRole.roleName)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ User missing required role: $($user.RoleType)" -ForegroundColor Red
        $userResult.Issues += "Missing required role: $($user.RoleType)"

        # Try to assign the role
        if ($targetRole) {
            Write-Host "  → Attempting to assign role..." -ForegroundColor Yellow
            try {
                $assignRoleBody = @{
                    userId = $user.Id
                    complaintRoleId = $targetRole.id
                    isPrimary = $true
                    effectiveFrom = (Get-Date).ToString("o")
                    notes = "Auto-assigned for login fix"
                } | ConvertTo-Json

                $assignResponse = Invoke-RestMethod -Uri "$baseUrl/user-roles" -Method Post -Headers $headers -Body $assignRoleBody

                if ($assignResponse.isSuccess) {
                    Write-Host "  ✓ Role assigned successfully" -ForegroundColor Green
                    $userResult.Fixes += "Assigned role: $($targetRole.name)"
                } else {
                    Write-Host "  ✗ Failed to assign role: $($assignResponse.message)" -ForegroundColor Red
                    $userResult.Issues += "Failed to assign role: $($assignResponse.message)"
                }
            } catch {
                $errorMsg = $_.Exception.Message
                if ($_.Exception.Response) {
                    try {
                        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                        $errorMsg = $reader.ReadToEnd()
                    } catch {
                        # Ignore error reading response
                    }
                }
                Write-Host "  ✗ Error assigning role: $errorMsg" -ForegroundColor Red
                $userResult.Issues += "Error assigning role: $errorMsg"
            }
        }
    }

    # Step 4: Set user password using the password service
    Write-Host ""
    Write-Host "[4/6] Setting user password..." -ForegroundColor Cyan
    $passwordSet = $false
    try {
        # Get admin user ID from token
        $adminId = "f56d8d03-e382-454b-bf7d-fa8236c125c3" # Admin user ID from token

        $passwordBody = @{
            password = $user.Password
            setBy = $adminId
            mustChangeOnNextLogin = $false
            sendEmail = $false
        } | ConvertTo-Json

        # Try the password service endpoint
        $passwordUrl = "$baseUrl/password-management/users/$($user.Id)/set-password"

        try {
            $passwordResponse = Invoke-RestMethod -Uri $passwordUrl -Method Post -Headers $headers -Body $passwordBody

            if ($passwordResponse.isSuccess) {
                Write-Host "  ✓ Password set successfully via password service" -ForegroundColor Green
                $userResult.Fixes += "Password set to: $($user.Password)"
                $passwordSet = $true
            } else {
                Write-Host "  ✗ Failed to set password: $($passwordResponse.message)" -ForegroundColor Red
                $userResult.Issues += "Failed to set password: $($passwordResponse.message)"
            }
        } catch {
            Write-Host "  → Password service endpoint not available, trying alternative method..." -ForegroundColor Yellow

            # Try to use change-password endpoint
            try {
                $changePasswordUrl = "$baseUrl/users/$($user.Id)/change-password"
                $changePasswordBody = @{
                    oldPassword = ""
                    newPassword = $user.Password
                } | ConvertTo-Json

                $changeResponse = Invoke-RestMethod -Uri $changePasswordUrl -Method Post -Headers $headers -Body $changePasswordBody
                Write-Host "  ✓ Password set successfully via change-password endpoint" -ForegroundColor Green
                $userResult.Fixes += "Password set to: $($user.Password)"
                $passwordSet = $true
            } catch {
                Write-Host "  ! Password endpoints not working, will need manual SQL update" -ForegroundColor Yellow
                $userResult.Issues += "Password needs manual SQL update"
                $userResult.Fixes += "Need to run SQL: See output below"
            }
        }
    } catch {
        $errorMsg = $_.Exception.Message
        Write-Host "  ✗ Error setting password: $errorMsg" -ForegroundColor Red
        $userResult.Issues += "Error setting password: $errorMsg"
    }

    # Step 5: Ensure user is active
    Write-Host ""
    Write-Host "[5/6] Ensuring user is active..." -ForegroundColor Cyan
    if (-not $userData.isActive) {
        try {
            $updateBody = @{
                isActive = $true
            } | ConvertTo-Json

            $updateResponse = Invoke-RestMethod -Uri "$baseUrl/users/$($user.Id)" -Method Put -Headers $headers -Body $updateBody

            if ($updateResponse.isSuccess) {
                Write-Host "  ✓ User activated successfully" -ForegroundColor Green
                $userResult.Fixes += "User activated"
            } else {
                Write-Host "  ✗ Failed to activate user: $($updateResponse.message)" -ForegroundColor Red
                $userResult.Issues += "Failed to activate user"
            }
        } catch {
            Write-Host "  ✗ Error activating user: $($_.Exception.Message)" -ForegroundColor Red
            $userResult.Issues += "Error activating user"
        }
    } else {
        Write-Host "  ✓ User already active" -ForegroundColor Green
    }

    # Step 6: Test login
    Write-Host ""
    Write-Host "[6/6] Testing login..." -ForegroundColor Cyan
    try {
        $loginBody = @{
            email = $user.Email
            password = $user.Password
        } | ConvertTo-Json

        $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -ContentType "application/json" -Body $loginBody

        if ($loginResponse.isSuccess) {
            Write-Host "  ✓ LOGIN SUCCESSFUL!" -ForegroundColor Green
            Write-Host "    User: $($loginResponse.data.user.fullName)" -ForegroundColor Green
            Write-Host "    Roles: $($loginResponse.data.user.roles.Count) role(s)" -ForegroundColor Green
            foreach ($role in $loginResponse.data.user.roles) {
                Write-Host "      - $($role.roleName)" -ForegroundColor Gray
            }
            $userResult.Status = "Success"
        } else {
            Write-Host "  ✗ Login failed: $($loginResponse.message)" -ForegroundColor Red
            $userResult.Issues += "Login test failed: $($loginResponse.message)"
            $userResult.Status = "Failed"
        }
    } catch {
        $errorMsg = $_.Exception.Message
        if ($_.Exception.Response) {
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $errorDetail = $reader.ReadToEnd()
                if ($errorDetail) {
                    try {
                        $errorJson = $errorDetail | ConvertFrom-Json
                        $errorMsg = $errorJson.message
                    } catch {
                        $errorMsg = $errorDetail
                    }
                }
            } catch {
                # Ignore error reading response
            }
        }
        Write-Host "  ✗ Login test failed: $errorMsg" -ForegroundColor Red
        $userResult.Issues += "Login test failed: $errorMsg"
        $userResult.Status = "Failed"
    }

    $results += $userResult
    Write-Host ""
}

# Generate summary report
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SUMMARY REPORT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

foreach ($result in $results) {
    Write-Host "User: $($result.Name)" -ForegroundColor Yellow
    Write-Host "Email: $($result.Email)" -ForegroundColor Yellow

    if ($result.Status -eq "Success") {
        Write-Host "Status: ✓ SUCCESS" -ForegroundColor Green
    } else {
        Write-Host "Status: ✗ $($result.Status.ToUpper())" -ForegroundColor Red
    }

    if ($result.Issues.Count -gt 0) {
        Write-Host "Issues Found:" -ForegroundColor Red
        foreach ($issue in $result.Issues) {
            Write-Host "  - $issue" -ForegroundColor Red
        }
    }

    if ($result.Fixes.Count -gt 0) {
        Write-Host "Fixes Applied:" -ForegroundColor Green
        foreach ($fix in $result.Fixes) {
            Write-Host "  - $fix" -ForegroundColor Green
        }
    }

    Write-Host ""
}

# Generate SQL script for manual password setting if needed
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "MANUAL PASSWORD SQL (if needed)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "If passwords need to be set manually, run this SQL:" -ForegroundColor Yellow
Write-Host ""

foreach ($user in $users) {
    Write-Host "-- Set password for $($user.Name)" -ForegroundColor Gray
    Write-Host "-- Password: $($user.Password)" -ForegroundColor Gray
    Write-Host "-- Note: You will need to hash the password using the AES encryption service" -ForegroundColor Gray
    Write-Host "UPDATE Users SET" -ForegroundColor White
    Write-Host "  PasswordHash = '<HASHED_PASSWORD>'," -ForegroundColor White
    Write-Host "  MustChangePasswordOnNextLogin = 0," -ForegroundColor White
    Write-Host "  PasswordChangedAt = GETUTCDATE()," -ForegroundColor White
    Write-Host "  FailedLoginAttempts = 0," -ForegroundColor White
    Write-Host "  AccountLockedUntil = NULL," -ForegroundColor White
    Write-Host "  IsActive = 1" -ForegroundColor White
    Write-Host "WHERE Id = '$($user.Id)'`;" -ForegroundColor White
    Write-Host ""
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Script execution completed!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Save results to JSON file
$jsonResults = $results | ConvertTo-Json -Depth 5
$jsonResults | Out-File "user-login-fix-results.json" -Encoding UTF8
Write-Host ""
Write-Host "Results saved to: user-login-fix-results.json" -ForegroundColor Green
