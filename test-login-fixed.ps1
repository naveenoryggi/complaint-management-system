# Fixed Login Test Script
# Issue: LoginRequest expects "Email" and "Password" properties, not "identifier"

Write-Host "=== CORRECTED LOGIN TEST ===" -ForegroundColor Cyan
Write-Host ""

$apiUrl = "http://localhost:5058/api"

# CORRECT format with Email and Password (capital E and P)
$loginBody = @{
    Email = "admin@complaintmanagement.com"
    Password = "Admin@123"
} | ConvertTo-Json

Write-Host "Testing login with CORRECT property names..." -ForegroundColor Yellow
Write-Host "Request Body:" -ForegroundColor Gray
Write-Host $loginBody -ForegroundColor Gray
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri "$apiUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -ErrorAction Stop

    Write-Host "✅ LOGIN SUCCESSFUL!" -ForegroundColor Green
    Write-Host ""

    if ($response.token) {
        Write-Host "Token (first 50 chars): $($response.token.Substring(0, [Math]::Min(50, $response.token.Length)))..." -ForegroundColor Green

        # Save token
        $response.token | Out-File -FilePath ".test-token" -NoNewline -Encoding UTF8
        Write-Host "✅ Token saved to .test-token" -ForegroundColor Green
        Write-Host ""

        # Test token with /me endpoint
        Write-Host "Verifying token..." -ForegroundColor Yellow
        $headers = @{
            "Authorization" = "Bearer $($response.token)"
        }

        try {
            $meResponse = Invoke-RestMethod -Uri "$apiUrl/auth/me" -Method GET -Headers $headers
            Write-Host "✅ Token is VALID!" -ForegroundColor Green
            Write-Host ""
            Write-Host "User Details:" -ForegroundColor Cyan
            Write-Host "  Email: $($meResponse.email)" -ForegroundColor White
            Write-Host "  Name: $($meResponse.firstName) $($meResponse.lastName)" -ForegroundColor White
            Write-Host "  User ID: $($meResponse.id)" -ForegroundColor Gray

            if ($meResponse.roles) {
                Write-Host "  Roles: $($meResponse.roles -join ', ')" -ForegroundColor White
            }

            # Check for SLA permissions
            Write-Host ""
            Write-Host "SLA Permissions:" -ForegroundColor Cyan
            if ($meResponse.permissions) {
                $slaPermissions = $meResponse.permissions | Where-Object { $_ -like '*SLA*' }
                if ($slaPermissions) {
                    $slaPermissions | ForEach-Object { Write-Host "  ✅ $_" -ForegroundColor Green }
                } else {
                    Write-Host "  ⚠️  No SLA permissions found" -ForegroundColor Yellow
                    Write-Host "  Note: Logout and login via browser to get fresh token with SLA permissions" -ForegroundColor Gray
                }
            } else {
                Write-Host "  No permissions data in response" -ForegroundColor Gray
            }

        } catch {
            Write-Host "❌ Token validation failed: $($_.Exception.Message)" -ForegroundColor Red
        }

    } else {
        Write-Host "⚠️  Login successful but no token in response" -ForegroundColor Yellow
    }

} catch {
    Write-Host "❌ LOGIN FAILED!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.ErrorDetails) {
        Write-Host ""
        Write-Host "Error Details:" -ForegroundColor Red
        try {
            $errorObj = $_.ErrorDetails.Message | ConvertFrom-Json
            Write-Host ($errorObj | ConvertTo-Json -Depth 5) -ForegroundColor Red
        } catch {
            Write-Host $_.ErrorDetails.Message -ForegroundColor Red
        }
    }

    if ($_.Exception.Response) {
        Write-Host ""
        Write-Host "HTTP Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== TEST COMPLETE ===" -ForegroundColor Cyan
