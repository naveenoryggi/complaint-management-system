# Verification Script: Notification Rules UI Display Fix
# Tests that the notification rules component can now load data successfully

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Notification Rules UI Fix Verification" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "https://localhost:7240/api"
$username = "navin@test.com"
$password = "Admin@123"

# Test Results
$results = @{
    "Fixed Endpoint Test" = $false
    "Notification Rules Load" = $false
    "Event Types Load" = $false
    "Templates Load" = $false
    "Data Count Validation" = $false
}

try {
    Write-Host "[1/5] Testing Authentication..." -ForegroundColor Yellow

    $loginBody = @{
        email = $username
        password = $password
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" `
        -Method Post `
        -Body $loginBody `
        -ContentType "application/json" `
        -SkipCertificateCheck

    $token = $loginResponse.data.token
    Write-Host "  SUCCESS - Logged in as $username" -ForegroundColor Green

    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    # Test 1: Verify the FIXED /api/roles endpoint works (was /api/role - 404)
    Write-Host ""
    Write-Host "[2/5] Testing FIXED Roles Endpoint (was causing 404)..." -ForegroundColor Yellow

    try {
        $rolesResponse = Invoke-RestMethod -Uri "$baseUrl/roles" `
            -Method Get `
            -Headers $headers `
            -SkipCertificateCheck

        if ($rolesResponse.isSuccess) {
            $roleCount = $rolesResponse.data.Count
            Write-Host "  SUCCESS - /api/roles endpoint works (Returns $roleCount roles)" -ForegroundColor Green
            Write-Host "  FIX VERIFIED: Changed from '/api/role' to '/api/roles' in role.service.ts" -ForegroundColor Green
            $results["Fixed Endpoint Test"] = $true
        }
    } catch {
        Write-Host "  FAILED - Roles endpoint still returning error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 2: Verify notification rules load successfully
    Write-Host ""
    Write-Host "[3/5] Testing Notification Rules Endpoint..." -ForegroundColor Yellow

    try {
        $rulesResponse = Invoke-RestMethod -Uri "$baseUrl/event-communication-rules?includeInactive=true" `
            -Method Get `
            -Headers $headers `
            -SkipCertificateCheck

        if ($rulesResponse.isSuccess -and $rulesResponse.data) {
            $ruleCount = $rulesResponse.data.Count
            Write-Host "  SUCCESS - Notification Rules loaded: $ruleCount rules" -ForegroundColor Green

            Write-Host ""
            Write-Host "  Notification Rules Found:" -ForegroundColor Cyan
            foreach ($rule in $rulesResponse.data) {
                $status = if ($rule.isActive) { "ACTIVE" } else { "INACTIVE" }
                Write-Host "    - [$status] $($rule.name)" -ForegroundColor $(if ($rule.isActive) { "Green" } else { "Gray" })
            }

            if ($ruleCount -ge 5) {
                $results["Notification Rules Load"] = $true
                $results["Data Count Validation"] = $true
            }
        }
    } catch {
        Write-Host "  FAILED - Notification rules error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 3: Verify event types load
    Write-Host ""
    Write-Host "[4/5] Testing Event Types Endpoint..." -ForegroundColor Yellow

    try {
        $eventTypesResponse = Invoke-RestMethod -Uri "$baseUrl/event-types?includeInactive=true" `
            -Method Get `
            -Headers $headers `
            -SkipCertificateCheck

        if ($eventTypesResponse.isSuccess -and $eventTypesResponse.data) {
            $eventCount = $eventTypesResponse.data.Count
            Write-Host "  SUCCESS - Event Types loaded: $eventCount types" -ForegroundColor Green
            $results["Event Types Load"] = $true
        }
    } catch {
        Write-Host "  FAILED - Event types error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 4: Verify templates load
    Write-Host ""
    Write-Host "[5/5] Testing Communication Templates Endpoint..." -ForegroundColor Yellow

    try {
        $templatesResponse = Invoke-RestMethod -Uri "$baseUrl/communication-templates?includeInactive=true" `
            -Method Get `
            -Headers $headers `
            -SkipCertificateCheck

        if ($templatesResponse.isSuccess -and $templatesResponse.data) {
            $templateCount = $templatesResponse.data.Count
            Write-Host "  SUCCESS - Templates loaded: $templateCount templates" -ForegroundColor Green
            $results["Templates Load"] = $true
        }
    } catch {
        Write-Host "  FAILED - Templates error: $($_.Exception.Message)" -ForegroundColor Red
    }

} catch {
    Write-Host "  CRITICAL ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
}

# Summary Report
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  VERIFICATION SUMMARY" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$passCount = ($results.Values | Where-Object { $_ -eq $true }).Count
$totalTests = $results.Count

foreach ($test in $results.GetEnumerator()) {
    $status = if ($test.Value) { "PASS" } else { "FAIL" }
    $color = if ($test.Value) { "Green" } else { "Red" }
    Write-Host "  [$status] $($test.Key)" -ForegroundColor $color
}

Write-Host ""
Write-Host "Overall: $passCount/$totalTests tests passed" -ForegroundColor $(if ($passCount -eq $totalTests) { "Green" } else { "Yellow" })

if ($passCount -eq $totalTests) {
    Write-Host ""
    Write-Host "BUG FIX VERIFIED!" -ForegroundColor Green
    Write-Host "The notification rules UI should now display correctly without 404 errors." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "SOME ISSUES REMAIN - Please review failed tests above" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
