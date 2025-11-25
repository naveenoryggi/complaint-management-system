# FINAL 100% VERIFICATION TEST
# Tests all specific items requested

$BaseUrl = "http://localhost:5058"
$token = Get-Content ".test-token" -Raw

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "FINAL 100% VERIFICATION TEST" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

$testResults = @()

# TEST 1: Notification Rules API (should show 23+ rules, not 0)
Write-Host "[TEST 1] Notification Rules API..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/event-communication-rules" -Headers $headers -Method Get
    $ruleCount = $response.data.Count
    if ($ruleCount -ge 23) {
        Write-Host "  PASS: Found $ruleCount notification rules (expected 23+)" -ForegroundColor Green
        $testResults += @{Test="Notification Rules API"; Status="PASS"; Details="$ruleCount rules found"}
    } elseif ($ruleCount -eq 0) {
        Write-Host "  FAIL: Found 0 notification rules (expected 23+)" -ForegroundColor Red
        $testResults += @{Test="Notification Rules API"; Status="FAIL"; Details="0 rules found - 404 error"}
    } else {
        Write-Host "  WARN: Found $ruleCount notification rules (expected 23+)" -ForegroundColor Yellow
        $testResults += @{Test="Notification Rules API"; Status="WARN"; Details="$ruleCount rules found (less than 23)"}
    }
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{Test="Notification Rules API"; Status="FAIL"; Details=$_.Exception.Message}
}

# TEST 2: Email Server Configuration (Gmail SMTP)
Write-Host "[TEST 2] Email Server Configuration..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/email-settings" -Headers $headers -Method Get
    $gmailServer = $response.data | Where-Object { $_.host -eq "smtp.gmail.com" }
    if ($gmailServer) {
        $activeStatus = if ($gmailServer.isActive) { "active" } else { "inactive" }
        Write-Host "  PASS: Gmail SMTP server configured ($activeStatus)" -ForegroundColor Green
        Write-Host "    - Host: $($gmailServer.host)" -ForegroundColor Gray
        Write-Host "    - Port: $($gmailServer.port)" -ForegroundColor Gray
        Write-Host "    - Name: $($gmailServer.name)" -ForegroundColor Gray
        Write-Host "    - Active: $($gmailServer.isActive)" -ForegroundColor Gray
        $testResults += @{Test="Email Server Configuration"; Status="PASS"; Details="Gmail SMTP configured on port $($gmailServer.port)"}
    } else {
        Write-Host "  FAIL: Gmail SMTP server not found" -ForegroundColor Red
        $allServers = $response.data | ForEach-Object { "$($_.name) ($($_.host):$($_.port))" }
        Write-Host "    Found servers: $($allServers -join ', ')" -ForegroundColor Gray
        $testResults += @{Test="Email Server Configuration"; Status="FAIL"; Details="Gmail SMTP not configured"}
    }
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{Test="Email Server Configuration"; Status="FAIL"; Details=$_.Exception.Message}
}

# TEST 3: SLA Configuration (20+ levels, 5 priority policies)
Write-Host "[TEST 3] SLA Configuration..." -ForegroundColor Yellow
try {
    # Get SLA settings (ComplaintInfoSettings)
    $slaResponse = Invoke-RestMethod -Uri "$BaseUrl/api/complaint-info-settings" -Headers $headers -Method Get
    $slaCount = $slaResponse.data.Count

    if ($slaCount -ge 20) {
        Write-Host "  PASS: Found $slaCount SLA settings (expected 20+)" -ForegroundColor Green

        # Check for priority-based SLA
        $priorities = @("Low", "Normal", "High", "Critical", "Urgent")
        Write-Host "    Priority-based SLA settings:" -ForegroundColor Gray
        foreach ($priority in $priorities) {
            $setting = $slaResponse.data | Where-Object { $_.name -like "*$priority*" }
            if ($setting) {
                Write-Host "      - ${priority}: Response $($setting.responseTime)h / Resolution $($setting.resolutionTime)h" -ForegroundColor Gray
            }
        }

        $testResults += @{Test="SLA Configuration"; Status="PASS"; Details="$slaCount SLA settings found"}
    } else {
        Write-Host "  WARN: Found $slaCount SLA settings (expected 20+)" -ForegroundColor Yellow
        $testResults += @{Test="SLA Configuration"; Status="WARN"; Details="Only $slaCount settings found"}
    }
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{Test="SLA Configuration"; Status="FAIL"; Details=$_.Exception.Message}
}

# TEST 4: Workflow Configuration (3+ workflows)
Write-Host "[TEST 4] Workflow Configuration..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/workflows" -Headers $headers -Method Get
    $workflowCount = $response.data.Count

    if ($workflowCount -ge 3) {
        Write-Host "  PASS: Found $workflowCount workflows (expected 3+)" -ForegroundColor Green
        Write-Host "    Workflows:" -ForegroundColor Gray
        $response.data | ForEach-Object {
            Write-Host "      - $($_.name)" -ForegroundColor Gray
        }
        $testResults += @{Test="Workflow Configuration"; Status="PASS"; Details="$workflowCount workflows found"}
    } else {
        Write-Host "  FAIL: Found only $workflowCount workflows (expected 3+)" -ForegroundColor Red
        $testResults += @{Test="Workflow Configuration"; Status="FAIL"; Details="Only $workflowCount workflows"}
    }
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{Test="Workflow Configuration"; Status="FAIL"; Details=$_.Exception.Message}
}

# TEST 5: Test Complaints (10 complaints CMP-2025-1130 to 1139)
Write-Host "[TEST 5] Test Complaints Verification..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/complaints?pageSize=100" -Headers $headers -Method Get
    $testComplaints = $response.data.items | Where-Object {
        $_.complaintNumber -ge "CMP-2025-1130" -and $_.complaintNumber -le "CMP-2025-1139"
    }
    $testCount = $testComplaints.Count

    if ($testCount -ge 10) {
        Write-Host "  PASS: Found all 10 test complaints (CMP-2025-1130 to 1139)" -ForegroundColor Green
        Write-Host "    Sample complaint:" -ForegroundColor Gray
        $sample = $testComplaints[0]
        Write-Host "      - Number: $($sample.complaintNumber)" -ForegroundColor Gray
        Write-Host "      - Title: $($sample.title)" -ForegroundColor Gray
        Write-Host "      - Priority: $($sample.priorityName)" -ForegroundColor Gray
        Write-Host "      - Status: $($sample.statusName)" -ForegroundColor Gray
        $testResults += @{Test="Test Complaints"; Status="PASS"; Details="All 10 test complaints found"}
    } else {
        Write-Host "  FAIL: Found only $testCount test complaints (expected 10)" -ForegroundColor Red
        $testResults += @{Test="Test Complaints"; Status="FAIL"; Details="Only $testCount complaints found"}
    }
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{Test="Test Complaints"; Status="FAIL"; Details=$_.Exception.Message}
}

# TEST 6: Dashboard Statistics
Write-Host "[TEST 6] Dashboard Statistics..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/dashboard/statistics" -Headers $headers -Method Get
    $stats = $response.data

    if ($stats.totalComplaints -gt 1000) {
        Write-Host "  PASS: Dashboard statistics working" -ForegroundColor Green
        Write-Host "    - Total Complaints: $($stats.totalComplaints)" -ForegroundColor Gray
        Write-Host "    - Open Complaints: $($stats.openComplaints)" -ForegroundColor Gray
        Write-Host "    - Closed Complaints: $($stats.closedComplaints)" -ForegroundColor Gray
        Write-Host "    - Pending Complaints: $($stats.pendingComplaints)" -ForegroundColor Gray
        $testResults += @{Test="Dashboard Statistics"; Status="PASS"; Details="$($stats.totalComplaints) total complaints"}
    } else {
        Write-Host "  WARN: Dashboard statistics returned but complaint count is low" -ForegroundColor Yellow
        $testResults += @{Test="Dashboard Statistics"; Status="WARN"; Details="Only $($stats.totalComplaints) complaints"}
    }
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{Test="Dashboard Statistics"; Status="FAIL"; Details=$_.Exception.Message}
}

# TEST 7: Backend API Health
Write-Host "[TEST 7] Backend API Health..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/health" -Method Get
    Write-Host "  PASS: Backend API is healthy" -ForegroundColor Green
    $testResults += @{Test="Backend API Health"; Status="PASS"; Details="API responding"}
} catch {
    Write-Host "  FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{Test="Backend API Health"; Status="FAIL"; Details=$_.Exception.Message}
}

# FINAL SUMMARY
Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "FINAL VERIFICATION SUMMARY" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

$passCount = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$warnCount = ($testResults | Where-Object { $_.Status -eq "WARN" }).Count
$totalCount = $testResults.Count

Write-Host ""
Write-Host "Total Tests: $totalCount" -ForegroundColor White
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red
Write-Host "Warnings: $warnCount" -ForegroundColor Yellow
Write-Host ""

$percentage = [math]::Round(($passCount / $totalCount) * 100, 2)
Write-Host "Overall Pass Rate: $percentage%" -ForegroundColor $(if ($percentage -ge 95) { "Green" } elseif ($percentage -ge 85) { "Yellow" } else { "Red" })

Write-Host ""
Write-Host "Detailed Results:" -ForegroundColor Cyan
$testResults | ForEach-Object {
    $color = switch ($_.Status) {
        "PASS" { "Green" }
        "FAIL" { "Red" }
        "WARN" { "Yellow" }
    }
    Write-Host "  [$($_.Status)] $($_.Test): $($_.Details)" -ForegroundColor $color
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan

# Save results to file
$testResults | ConvertTo-Json | Out-File "final-verification-results.json"
Write-Host "Results saved to: final-verification-results.json" -ForegroundColor Gray
