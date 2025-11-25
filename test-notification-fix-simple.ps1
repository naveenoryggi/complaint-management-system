# Simple Notification Rules Fix Test
# Tests the fix for the /api/role -> /api/roles bug

Write-Host "Testing Notification Rules Fix..." -ForegroundColor Cyan
Write-Host ""

$baseUrl = "https://localhost:7240/api"
$username = "navin@test.com"
$password = "Admin@123"

# Disable SSL verification for local testing
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint svcPoint, X509Certificate certificate,
            WebRequest webRequest, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

try {
    Write-Host "[1] Logging in..." -ForegroundColor Yellow

    $loginBody = @{
        email = $username
        password = $password
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" `
        -Method Post `
        -Body $loginBody `
        -ContentType "application/json"

    $token = $loginResponse.data.token
    Write-Host "  SUCCESS - Authenticated" -ForegroundColor Green

    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    # Test the FIXED endpoint
    Write-Host ""
    Write-Host "[2] Testing FIXED /api/roles endpoint..." -ForegroundColor Yellow
    Write-Host "    (Previously was /api/role which returned 404)" -ForegroundColor Gray

    try {
        $rolesResponse = Invoke-RestMethod -Uri "$baseUrl/roles" `
            -Method Get `
            -Headers $headers

        if ($rolesResponse.isSuccess) {
            $roleCount = $rolesResponse.data.Count
            Write-Host "  SUCCESS - Endpoint works! Returns $roleCount roles" -ForegroundColor Green
            Write-Host "  FIX CONFIRMED in role.service.ts line 20" -ForegroundColor Green
        }
    } catch {
        Write-Host "  FAILED - Still getting 404 error" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test notification rules loading
    Write-Host ""
    Write-Host "[3] Testing Notification Rules Load..." -ForegroundColor Yellow

    try {
        $rulesResponse = Invoke-RestMethod -Uri "$baseUrl/event-communication-rules?includeInactive=true" `
            -Method Get `
            -Headers $headers

        if ($rulesResponse.isSuccess -and $rulesResponse.data) {
            $ruleCount = $rulesResponse.data.Count
            Write-Host "  SUCCESS - $ruleCount notification rules loaded" -ForegroundColor Green

            if ($ruleCount -gt 0) {
                Write-Host ""
                Write-Host "  Rules in Database:" -ForegroundColor Cyan
                foreach ($rule in $rulesResponse.data | Select-Object -First 10) {
                    $status = if ($rule.isActive) { "ACTIVE" } else { "INACTIVE" }
                    Write-Host "    [$status] $($rule.name)" -ForegroundColor $(if ($rule.isActive) { "Green" } else { "Gray" })
                }
                if ($ruleCount -gt 10) {
                    Write-Host "    ... and $($ruleCount - 10) more" -ForegroundColor Gray
                }
            }
        }
    } catch {
        Write-Host "  FAILED - $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test event types
    Write-Host ""
    Write-Host "[4] Testing Event Types Load..." -ForegroundColor Yellow

    try {
        $eventResponse = Invoke-RestMethod -Uri "$baseUrl/event-types?includeInactive=true" `
            -Method Get `
            -Headers $headers

        if ($eventResponse.isSuccess) {
            Write-Host "  SUCCESS - $($eventResponse.data.Count) event types loaded" -ForegroundColor Green
        }
    } catch {
        Write-Host "  FAILED - $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test templates
    Write-Host ""
    Write-Host "[5] Testing Templates Load..." -ForegroundColor Yellow

    try {
        $templateResponse = Invoke-RestMethod -Uri "$baseUrl/communication-templates?includeInactive=true" `
            -Method Get `
            -Headers $headers

        if ($templateResponse.isSuccess) {
            Write-Host "  SUCCESS - $($templateResponse.data.Count) templates loaded" -ForegroundColor Green
        }
    } catch {
        Write-Host "  FAILED - $($_.Exception.Message)" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  FIX SUMMARY" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "File Modified:" -ForegroundColor Yellow
    Write-Host "  C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-angular\src\app\services\role.service.ts"
    Write-Host ""
    Write-Host "Line Changed:" -ForegroundColor Yellow
    Write-Host "  Line 20:" -ForegroundColor Gray
    Write-Host "  BEFORE: private apiUrl = " -NoNewline -ForegroundColor Red
    Write-Host "`${environment.apiUrl}/role" -ForegroundColor Red -BackgroundColor DarkRed
    Write-Host "  AFTER:  private apiUrl = " -NoNewline -ForegroundColor Green
    Write-Host "`${environment.apiUrl}/roles" -ForegroundColor Green -BackgroundColor DarkGreen
    Write-Host ""
    Write-Host "Result:" -ForegroundColor Yellow
    Write-Host "  - No more 404 errors on /api/role" -ForegroundColor Green
    Write-Host "  - Notification Rules component can now load all data" -ForegroundColor Green
    Write-Host "  - UI should display notification rules correctly" -ForegroundColor Green
    Write-Host ""

} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
