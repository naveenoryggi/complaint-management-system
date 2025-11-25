# Email Ticketing Config Page Inspection Script
# Purpose: Diagnose why System Settings button/panel is not visible

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Email Ticketing Config Page Diagnostic Report" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Get Admin Token
Write-Host "[Step 1] Getting Admin Token..." -ForegroundColor Yellow
$loginUrl = "http://localhost:5000/api/auth/login"
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "✓ Admin login successful" -ForegroundColor Green
    Write-Host "  Token: $($token.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "✗ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host ""

# Step 2: Check System Configuration API Endpoint
Write-Host "[Step 2] Testing System Configuration API Endpoint..." -ForegroundColor Yellow
$systemConfigUrl = "http://localhost:5000/api/SystemConfiguration"

try {
    $systemConfigResponse = Invoke-RestMethod -Uri $systemConfigUrl -Method GET -Headers $headers
    Write-Host "✓ System Configuration API endpoint is working" -ForegroundColor Green
    Write-Host "  Response:" -ForegroundColor Gray
    $systemConfigResponse | ConvertTo-Json -Depth 3 | Write-Host -ForegroundColor White
} catch {
    Write-Host "✗ System Configuration API failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "  Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""

# Step 3: Check Email Server Settings API
Write-Host "[Step 3] Testing Email Server Settings API..." -ForegroundColor Yellow
$emailSettingsUrl = "http://localhost:5000/api/EmailConfiguration"

try {
    $emailSettingsResponse = Invoke-RestMethod -Uri $emailSettingsUrl -Method GET -Headers $headers
    Write-Host "✓ Email Configuration API endpoint is working" -ForegroundColor Green
    Write-Host "  Total configurations: $($emailSettingsResponse.Count)" -ForegroundColor Gray
    if ($emailSettingsResponse.Count -gt 0) {
        $emailSettingsResponse | Select-Object -First 1 | ConvertTo-Json -Depth 2 | Write-Host -ForegroundColor White
    }
} catch {
    Write-Host "✗ Email Configuration API failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Step 4: Check Angular Component Files
Write-Host "[Step 4] Checking Angular Component Files..." -ForegroundColor Yellow

$componentPath = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-angular\src\app\components\admin\email-ticketing-config"

if (Test-Path $componentPath) {
    Write-Host "✓ Component directory exists: $componentPath" -ForegroundColor Green

    $tsFile = Join-Path $componentPath "email-ticketing-config.component.ts"
    $htmlFile = Join-Path $componentPath "email-ticketing-config.component.html"

    if (Test-Path $tsFile) {
        Write-Host "✓ TypeScript file exists" -ForegroundColor Green

        # Check for System Settings related code
        $tsContent = Get-Content $tsFile -Raw

        Write-Host ""
        Write-Host "  Checking for System Settings related code:" -ForegroundColor Cyan

        if ($tsContent -match "showSystemSettings") {
            Write-Host "  ✓ Found 'showSystemSettings' variable" -ForegroundColor Green
        } else {
            Write-Host "  ✗ 'showSystemSettings' variable NOT found" -ForegroundColor Red
        }

        if ($tsContent -match "toggleSystemSettings") {
            Write-Host "  ✓ Found 'toggleSystemSettings' method" -ForegroundColor Green
        } else {
            Write-Host "  ✗ 'toggleSystemSettings' method NOT found" -ForegroundColor Red
        }

        if ($tsContent -match "SystemConfiguration") {
            Write-Host "  ✓ Found 'SystemConfiguration' reference" -ForegroundColor Green
        } else {
            Write-Host "  ✗ 'SystemConfiguration' reference NOT found" -ForegroundColor Red
        }

    } else {
        Write-Host "✗ TypeScript file NOT found" -ForegroundColor Red
    }

    if (Test-Path $htmlFile) {
        Write-Host "✓ HTML template file exists" -ForegroundColor Green

        # Check for System Settings UI elements
        $htmlContent = Get-Content $htmlFile -Raw

        Write-Host ""
        Write-Host "  Checking for System Settings UI elements:" -ForegroundColor Cyan

        if ($htmlContent -match "System Settings") {
            Write-Host "  ✓ Found 'System Settings' text" -ForegroundColor Green
        } else {
            Write-Host "  ✗ 'System Settings' text NOT found" -ForegroundColor Red
        }

        if ($htmlContent -match "system-settings-panel") {
            Write-Host "  ✓ Found 'system-settings-panel' element" -ForegroundColor Green
        } else {
            Write-Host "  ✗ 'system-settings-panel' element NOT found" -ForegroundColor Red
        }

        if ($htmlContent -match "btn-settings") {
            Write-Host "  ✓ Found 'btn-settings' button" -ForegroundColor Green
        } else {
            Write-Host "  ✗ 'btn-settings' button NOT found" -ForegroundColor Red
        }

        if ($htmlContent -match "fa-cog") {
            Write-Host "  ✓ Found gear icon (fa-cog)" -ForegroundColor Green
        } else {
            Write-Host "  ✗ Gear icon (fa-cog) NOT found" -ForegroundColor Red
        }

    } else {
        Write-Host "✗ HTML template file NOT found" -ForegroundColor Red
    }

} else {
    Write-Host "✗ Component directory NOT found: $componentPath" -ForegroundColor Red
}

Write-Host ""

# Step 5: Check if Angular is compiled
Write-Host "[Step 5] Checking Angular Compilation Status..." -ForegroundColor Yellow

$angularDistPath = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-angular\dist"

if (Test-Path $angularDistPath) {
    Write-Host "✓ Angular dist folder exists" -ForegroundColor Green
    $distFiles = Get-ChildItem -Path $angularDistPath -Recurse -File
    Write-Host "  Total files in dist: $($distFiles.Count)" -ForegroundColor Gray

    # Check for recent compilation
    $recentFiles = $distFiles | Where-Object { $_.LastWriteTime -gt (Get-Date).AddHours(-2) }
    if ($recentFiles.Count -gt 0) {
        Write-Host "  ✓ Recent compilation detected (within last 2 hours)" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ No recent compilation detected" -ForegroundColor Yellow
        Write-Host "    Last modified: $(($distFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1).LastWriteTime)" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ Angular dist folder NOT found (not compiled)" -ForegroundColor Red
}

Write-Host ""

# Step 6: Check Angular Routes
Write-Host "[Step 6] Checking Angular Routes..." -ForegroundColor Yellow

$routesFile = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-angular\src\app\app.routes.ts"

if (Test-Path $routesFile) {
    $routesContent = Get-Content $routesFile -Raw

    if ($routesContent -match "email-ticketing-config") {
        Write-Host "✓ Email Ticketing Config route found in app.routes.ts" -ForegroundColor Green
    } else {
        Write-Host "✗ Email Ticketing Config route NOT found in app.routes.ts" -ForegroundColor Red
    }
} else {
    Write-Host "✗ app.routes.ts file NOT found" -ForegroundColor Red
}

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Diagnostic Report Complete" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Summary
Write-Host "SUMMARY OF FINDINGS:" -ForegroundColor Yellow
Write-Host "--------------------" -ForegroundColor Yellow
Write-Host ""
Write-Host "API Endpoints:" -ForegroundColor Cyan
Write-Host "  • System Configuration API: " -NoNewline
if ($systemConfigResponse) { Write-Host "WORKING ✓" -ForegroundColor Green } else { Write-Host "FAILED ✗" -ForegroundColor Red }
Write-Host "  • Email Configuration API: " -NoNewline
if ($emailSettingsResponse) { Write-Host "WORKING ✓" -ForegroundColor Green } else { Write-Host "FAILED ✗" -ForegroundColor Red }

Write-Host ""
Write-Host "Frontend Code:" -ForegroundColor Cyan
Write-Host "  • Component directory: " -NoNewline
if (Test-Path $componentPath) { Write-Host "EXISTS ✓" -ForegroundColor Green } else { Write-Host "MISSING ✗" -ForegroundColor Red }

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Check the HTML template for System Settings button" -ForegroundColor White
Write-Host "  2. Verify Angular compilation is successful" -ForegroundColor White
Write-Host "  3. Check browser console for runtime errors" -ForegroundColor White
Write-Host "  4. Inspect the component TypeScript file for the toggleSystemSettings method" -ForegroundColor White
Write-Host ""
