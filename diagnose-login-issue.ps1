# Login Diagnostic Script
# Purpose: Diagnose why login is failing

Write-Host "=== LOGIN DIAGNOSTIC SCRIPT ===" -ForegroundColor Cyan
Write-Host ""

$backendUrl = "http://localhost:5058"
$frontendUrl = "http://localhost:4200"
$apiUrl = "$backendUrl/api"

# Test 1: Check if backend is reachable
Write-Host "Test 1: Checking if backend API is reachable..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $backendUrl -Method GET -TimeoutSec 5
    Write-Host "✅ Backend is reachable (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "❌ Backend is NOT reachable: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Please ensure backend is running: cd complaint-system-dotnet/src/ComplaintManagement.API && dotnet run" -ForegroundColor Yellow
}

Write-Host ""

# Test 2: Check if frontend is reachable
Write-Host "Test 2: Checking if frontend is reachable..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $frontendUrl -Method GET -TimeoutSec 5
    Write-Host "✅ Frontend is reachable (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "❌ Frontend is NOT reachable: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Please ensure frontend is running: cd complaint-system-angular && npm start" -ForegroundColor Yellow
}

Write-Host ""

# Test 3: Test login endpoint with correct format
Write-Host "Test 3: Testing login endpoint..." -ForegroundColor Yellow

$loginBody = @{
    identifier = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

Write-Host "Request Body:" -ForegroundColor Gray
Write-Host $loginBody -ForegroundColor Gray
Write-Host ""

try {
    $loginResponse = Invoke-RestMethod -Uri "$apiUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -ErrorAction Stop

    Write-Host "✅ Login SUCCESSFUL!" -ForegroundColor Green
    Write-Host ""

    if ($loginResponse.token) {
        Write-Host "Token received (first 50 chars): $($loginResponse.token.Substring(0, [Math]::Min(50, $loginResponse.token.Length)))..." -ForegroundColor Green

        # Save token
        $loginResponse.token | Out-File -FilePath ".test-token" -NoNewline -Encoding UTF8
        Write-Host "Token saved to .test-token" -ForegroundColor Green

        # Test token with /me endpoint
        Write-Host ""
        Write-Host "Test 4: Verifying token with /me endpoint..." -ForegroundColor Yellow

        $headers = @{
            "Authorization" = "Bearer $($loginResponse.token)"
        }

        try {
            $meResponse = Invoke-RestMethod -Uri "$apiUrl/auth/me" -Method GET -Headers $headers
            Write-Host "✅ Token is VALID!" -ForegroundColor Green
            Write-Host "Logged in as: $($meResponse.email)" -ForegroundColor Green
            Write-Host "User ID: $($meResponse.id)" -ForegroundColor Gray
            Write-Host "Roles: $($meResponse.roles -join ', ')" -ForegroundColor Gray

            # Check for SLA permissions
            Write-Host ""
            Write-Host "Checking SLA Permissions in token..." -ForegroundColor Yellow
            if ($meResponse.permissions) {
                $slaPermissions = $meResponse.permissions | Where-Object { $_ -like '*SLA*' }
                if ($slaPermissions) {
                    Write-Host "✅ SLA Permissions found:" -ForegroundColor Green
                    $slaPermissions | ForEach-Object { Write-Host "   - $_" -ForegroundColor Green }
                } else {
                    Write-Host "⚠️  NO SLA permissions found in token" -ForegroundColor Yellow
                    Write-Host "   You may need to logout and login again to get SLA permissions" -ForegroundColor Yellow
                }
            }

        } catch {
            Write-Host "❌ Token validation FAILED: $($_.Exception.Message)" -ForegroundColor Red
        }

    } else {
        Write-Host "⚠️  Login response received but NO TOKEN found" -ForegroundColor Yellow
        Write-Host "Response:" -ForegroundColor Gray
        Write-Host ($loginResponse | ConvertTo-Json -Depth 5) -ForegroundColor Gray
    }

} catch {
    Write-Host "❌ Login FAILED!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.ErrorDetails) {
        Write-Host ""
        Write-Host "Error Details:" -ForegroundColor Red
        Write-Host $_.ErrorDetails.Message -ForegroundColor Red
    }

    if ($_.Exception.Response) {
        Write-Host ""
        Write-Host "HTTP Status: $($_.Exception.Response.StatusCode.value__) - $($_.Exception.Response.StatusDescription)" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "Common Solutions:" -ForegroundColor Yellow
    Write-Host "1. Verify backend is running on port 5058" -ForegroundColor Gray
    Write-Host "2. Check database connection (user must exist in Users table)" -ForegroundColor Gray
    Write-Host "3. Verify credentials: admin@complaintmanagement.com / Admin@123" -ForegroundColor Gray
    Write-Host "4. Check backend logs for detailed error information" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== DIAGNOSTIC COMPLETE ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. If login successful via this script but browser login fails:" -ForegroundColor Gray
Write-Host "   - Open browser DevTools (F12)" -ForegroundColor Gray
Write-Host "   - Go to Console tab and look for errors" -ForegroundColor Gray
Write-Host "   - Go to Network tab and monitor /api/auth/login request" -ForegroundColor Gray
Write-Host ""
Write-Host "2. If login failed:" -ForegroundColor Gray
Write-Host "   - Check backend terminal for error details" -ForegroundColor Gray
Write-Host "   - Verify database has admin user with correct password hash" -ForegroundColor Gray
Write-Host "   - Try resetting admin password: powershell.exe -ExecutionPolicy Bypass -File reset-admin-password.ps1" -ForegroundColor Gray
