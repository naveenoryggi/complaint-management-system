# Reset Dashboard Preferences with Proper Authentication
# This script respects role-based access control and authentication

Write-Host "Step 1: Authenticating as Admin..." -ForegroundColor Cyan

$loginBody = @{
    Email = "admin@complaintmanagement.com"
    Password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

    if ($loginResponse.isSuccess -or $loginResponse.data.token) {
        $token = if ($loginResponse.data.token) { $loginResponse.data.token } elseif ($loginResponse.token) { $loginResponse.token } else { $null }

        if (-not $token) {
            Write-Host "[ERROR] No token received" -ForegroundColor Red
            exit 1
        }

        $userName = if ($loginResponse.data.user.fullName) { $loginResponse.data.user.fullName } else { "Admin" }
        $userRole = if ($loginResponse.data.user.role) { $loginResponse.data.user.role } else { "Administrator" }

        Write-Host "[SUCCESS] Login successful" -ForegroundColor Green
        Write-Host "  User: $userName" -ForegroundColor Gray
        Write-Host "  Role: $userRole" -ForegroundColor Gray
        Write-Host ""

        Write-Host "Step 2: Deleting dashboard preferences via authenticated API..." -ForegroundColor Cyan

        $headers = @{
            "Authorization" = "Bearer $token"
        }

        $deleteResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/preferences" -Method DELETE -Headers $headers

        Write-Host "[SUCCESS] Dashboard preferences deleted successfully" -ForegroundColor Green
        Write-Host ""
        Write-Host "Response:" -ForegroundColor Gray
        $deleteResponse | ConvertTo-Json -Depth 5

        Write-Host ""
        Write-Host "[SUCCESS] All fake statusIds have been cleared from the database" -ForegroundColor Green
        Write-Host "  The DELETE operation was executed with proper authentication and RBAC" -ForegroundColor Gray
        Write-Host "  Next: Refresh the dashboard page to see all status widgets" -ForegroundColor Yellow

    } else {
        Write-Host "[FAILED] Login failed" -ForegroundColor Red
        $loginResponse | ConvertTo-Json -Depth 5
    }
} catch {
    Write-Host "[ERROR] $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}
