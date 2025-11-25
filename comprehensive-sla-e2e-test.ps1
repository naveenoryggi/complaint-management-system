# ==================================================================================
# COMPREHENSIVE SLA END-TO-END TEST SUITE
# ==================================================================================
# Tests ALL SLA-related features exhaustively across the entire application
# ==================================================================================

$BaseUrl = "http://localhost:5000/api"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$resultsFile = "COMPREHENSIVE_SLA_E2E_TEST_$timestamp.txt"
$passedTests = 0
$failedTests = 0
$totalTests = 0
$testData = @{
    Categories = @()
    Workflows = @()
    Complaints = @()
    Statuses = @()
    Priorities = @()
}

function Write-TestLog {
    param([string]$Message, [string]$Level = "INFO")
    $ts = Get-Date -Format "HH:mm:ss"
    $logMessage = "[$ts] [$Level] $Message"
    switch ($Level) {
        "PASS" { Write-Host $logMessage -ForegroundColor Green }
        "FAIL" { Write-Host $logMessage -ForegroundColor Red }
        "INFO" { Write-Host $logMessage -ForegroundColor Cyan }
        "WARN" { Write-Host $logMessage -ForegroundColor Yellow }
    }
    Add-Content -Path $resultsFile -Value $logMessage
}

Write-TestLog "========================================" "INFO"
Write-TestLog "COMPREHENSIVE SLA E2E TESTING STARTING" "INFO"
Write-TestLog "========================================" "INFO"

# Get Authentication Token
Write-TestLog "Getting authentication token..." "INFO"
$token = (Get-Content ".test-token" -Raw -ErrorAction SilentlyContinue).Trim()
if (-not $token -or $token -eq "") {
    Write-TestLog "No token found. Please run get-sla-token.ps1 first!" "FAIL"
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-TestLog "Token loaded successfully" "PASS"

# ==================================================================================
# PHASE 1: DISCOVER CURRENT SLA CONFIGURATION
# ==================================================================================
Write-TestLog "`n===== PHASE 1: SLA CONFIGURATION DISCOVERY =====" "INFO"

# Test 1.1: Category-Level SLA
Write-TestLog "`n[1.1] Testing Category-Level SLA Configuration..." "INFO"
$totalTests++
try {
    $categories = Invoke-RestMethod -Uri "$BaseUrl/categories" -Headers $headers -Method Get
    Write-TestLog "Retrieved $($categories.Count) categories" "PASS"
    $passedTests++
    $testData.Categories = $categories

    # Check for SLA fields
    $sampleCat = $categories[0]
    $slaFields = @($sampleCat.PSObject.Properties | Where-Object { $_.Name -match 'sla|SLA' })

    if ($slaFields.Count -gt 0) {
        Write-TestLog "Found SLA fields in categories: $($slaFields.Name -join ', ')" "PASS"
        $passedTests++
    } else {
        Write-TestLog "No SLA fields found in category objects" "WARN"
    }
    $totalTests++

    # Count categories with SLA
    $catsWithSLA = @($categories | Where-Object { $_.defaultSlaHours -or $_.slaHours })
    Write-TestLog "Categories with SLA configured: $($catsWithSLA.Count)" "INFO"

} catch {
    Write-TestLog "Failed to retrieve categories: $($_.Exception.Message)" "FAIL"
    $failedTests++
}

# Test 1.2: Workflow Status-Level SLA
Write-TestLog "`n[1.2] Testing Workflow Status-Level SLA Configuration..." "INFO"
$totalTests++
try {
    $workflows = Invoke-RestMethod -Uri "$BaseUrl/workflows" -Headers $headers -Method Get
    Write-TestLog "Retrieved $($workflows.Count) workflows" "PASS"
    $passedTests++
    $testData.Workflows = $workflows

    $totalTests++
    $statusesWithSLA = 0
    foreach ($wf in $workflows) {
        if ($wf.statuses) {
            $slaStatuses = @($wf.statuses | Where-Object { $_.defaultSLAHours -or $_.escalationHours })
            if ($slaStatuses.Count -gt 0) {
                $statusesWithSLA += $slaStatuses.Count
                Write-TestLog "Workflow '$($wf.name)' has $($slaStatuses.Count) statuses with SLA config" "INFO"
            }
        }
    }

    if ($statusesWithSLA -gt 0) {
        Write-TestLog "Found $statusesWithSLA workflow statuses with SLA configuration" "PASS"
        $passedTests++
    } else {
        Write-TestLog "No workflow statuses with SLA configuration found" "WARN"
    }
    $totalTests++

} catch {
    Write-TestLog "Failed to retrieve workflows: $($_.Exception.Message)" "FAIL"
    $failedTests++
}

# Test 1.3: SLA Management Endpoints
Write-TestLog "`n[1.3] Testing SLA Management Endpoints..." "INFO"
$slaEndpoints = @(
    @{ Path = "/sla"; Description = "SLA Policies" },
    @{ Path = "/sla/policies"; Description = "SLA Policy List" },
    @{ Path = "/sla/calculator"; Description = "SLA Calculator" },
    @{ Path = "/sla/compliance"; Description = "SLA Compliance" },
    @{ Path = "/dashboard/sla"; Description = "SLA Dashboard" }
)

foreach ($endpoint in $slaEndpoints) {
    $totalTests++
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl$($endpoint.Path)" -Headers $headers -Method Get -ErrorAction Stop
        Write-TestLog "✅ $($endpoint.Description) endpoint found at $($endpoint.Path)" "PASS"
        $passedTests++
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 404) {
            Write-TestLog "❌ $($endpoint.Description) endpoint not found at $($endpoint.Path)" "INFO"
        } else {
            Write-TestLog "⚠️ $($endpoint.Description) endpoint error: $($_.Exception.Message)" "WARN"
        }
        $failedTests++
    }
}

# Get Status and Priority Masters for test data creation
Write-TestLog "`n[1.4] Loading master data..." "INFO"
try {
    $testData.Statuses = Invoke-RestMethod -Uri "$BaseUrl/status-master" -Headers $headers -Method Get
    $testData.Priorities = Invoke-RestMethod -Uri "$BaseUrl/priority-master" -Headers $headers -Method Get
    Write-TestLog "Loaded $($testData.Statuses.Count) statuses and $($testData.Priorities.Count) priorities" "PASS"
} catch {
    Write-TestLog "Failed to load master data: $($_.Exception.Message)" "FAIL"
}

# ==================================================================================
# PHASE 2: CREATE COMPREHENSIVE TEST DATA
# ==================================================================================
Write-TestLog "`n===== PHASE 2: CREATE TEST DATA =====" "INFO"

# Test 2.1: Create Categories with SLA
Write-TestLog "`n[2.1] Creating Test Categories with SLA..." "INFO"
$testCategories = @(
    @{ name = "SLA-TEST-Hardware-48h"; description = "Hardware support - 48h SLA"; defaultSlaHours = 48; slaHours = 48 },
    @{ name = "SLA-TEST-Software-24h"; description = "Software support - 24h SLA"; defaultSlaHours = 24; slaHours = 24 },
    @{ name = "SLA-TEST-Critical-4h"; description = "Critical systems - 4h SLA"; defaultSlaHours = 4; slaHours = 4 },
    @{ name = "SLA-TEST-General-72h"; description = "General inquiry - 72h SLA"; defaultSlaHours = 72; slaHours = 72 }
)

$createdCategories = @()
foreach ($catData in $testCategories) {
    $totalTests++
    try {
        $body = $catData | ConvertTo-Json
        $created = Invoke-RestMethod -Uri "$BaseUrl/categories" -Headers $headers -Method Post -Body $body
        $createdCategories += $created
        Write-TestLog "Created category: $($catData.name) with $($catData.defaultSlaHours)h SLA" "PASS"
        $passedTests++
    } catch {
        Write-TestLog "Failed to create category $($catData.name): $($_.Exception.Message)" "FAIL"
        $failedTests++
    }
}

# Test 2.2: Create Complaints for SLA Testing
Write-TestLog "`n[2.2] Creating Test Complaints..." "INFO"
$complaintsCreated = 0

foreach ($cat in $createdCategories) {
    # Create 2 complaints per category
    for ($i = 1; $i -le 2; $i++) {
        $totalTests++
        try {
            if ($testData.Priorities.Count -gt 0 -and $testData.Statuses.Count -gt 0) {
                $priority = $testData.Priorities | Get-Random
                $status = $testData.Statuses | Where-Object { $_.name -match 'New|Submitted' } | Select-Object -First 1

                if (-not $status) {
                    $status = $testData.Statuses[0]
                }

                $complaintData = @{
                    title = "SLA-TEST-$($cat.name)-C$i"
                    description = "Test complaint for SLA testing in $($cat.name)"
                    categoryId = $cat.id
                    priorityMasterId = $priority.id
                    statusMasterId = $status.id
                }

                $body = $complaintData | ConvertTo-Json
                $created = Invoke-RestMethod -Uri "$BaseUrl/complaints" -Headers $headers -Method Post -Body $body
                $testData.Complaints += $created
                $complaintsCreated++
                Write-TestLog "Created complaint: SLA-TEST-$($cat.name)-C$i" "PASS"
                $passedTests++

                Start-Sleep -Milliseconds 300
            }
        } catch {
            Write-TestLog "Failed to create complaint: $($_.Exception.Message)" "FAIL"
            $failedTests++
        }
    }
}

Write-TestLog "Total test complaints created: $complaintsCreated" "INFO"

# ==================================================================================
# PHASE 3: TEST SLA VISIBILITY & CALCULATIONS
# ==================================================================================
Write-TestLog "`n===== PHASE 3: SLA VISIBILITY & CALCULATIONS =====" "INFO"

# Test 3.1: SLA in Complaint List
Write-TestLog "`n[3.1] Testing SLA Visibility in Complaint List..." "INFO"
$totalTests++
try {
    $allComplaints = Invoke-RestMethod -Uri "$BaseUrl/complaints" -Headers $headers -Method Get
    $testComplaints = @($allComplaints | Where-Object { $_.title -like "SLA-TEST-*" })

    Write-TestLog "Retrieved $($allComplaints.Count) total complaints, $($testComplaints.Count) test complaints" "PASS"
    $passedTests++

    # Check for SLA fields in complaint list
    $totalTests++
    if ($testComplaints.Count -gt 0) {
        $sample = $testComplaints[0]
        $slaFields = @($sample.PSObject.Properties | Where-Object { $_.Name -match 'sla|SLA|due|remaining|elapsed|breach' })

        if ($slaFields.Count -gt 0) {
            Write-TestLog "SLA fields found in complaint list: $($slaFields.Name -join ', ')" "PASS"
            $passedTests++
        } else {
            Write-TestLog "WARNING: No SLA fields in complaint list objects" "WARN"
            $failedTests++
        }
    }

} catch {
    Write-TestLog "Failed to test complaint list SLA: $($_.Exception.Message)" "FAIL"
    $failedTests++
}

# Test 3.2: SLA in Complaint Detail
Write-TestLog "`n[3.2] Testing SLA Visibility in Complaint Detail..." "INFO"
if ($testData.Complaints.Count -gt 0) {
    $testComplaint = $testData.Complaints[0]
    $totalTests++

    try {
        $detail = Invoke-RestMethod -Uri "$BaseUrl/complaints/$($testComplaint.id)" -Headers $headers -Method Get
        Write-TestLog "Retrieved complaint detail for: $($detail.title)" "PASS"
        $passedTests++

        # Analyze SLA fields in detail
        $totalTests++
        $allSLAFields = @($detail.PSObject.Properties | Where-Object {
            $_.Name -match 'sla|SLA|due|remaining|elapsed|breach|urgency|progress'
        })

        if ($allSLAFields.Count -gt 0) {
            Write-TestLog "SLA fields in detail view: $($allSLAFields.Name -join ', ')" "PASS"
            $passedTests++

            # Log specific SLA values
            if ($detail.slaHours) {
                Write-TestLog "  SLA Hours: $($detail.slaHours)" "INFO"
            }
            if ($detail.timeRemaining) {
                Write-TestLog "  Time Remaining: $($detail.timeRemaining) hours" "INFO"
            }
            if ($detail.timeElapsed) {
                Write-TestLog "  Time Elapsed: $($detail.timeElapsed) hours" "INFO"
            }
        } else {
            Write-TestLog "WARNING: No SLA fields in complaint detail" "WARN"
            $failedTests++
        }

    } catch {
        Write-TestLog "Failed to get complaint detail: $($_.Exception.Message)" "FAIL"
        $failedTests++
    }
}

# Test 3.3: SLA Calculation Accuracy
Write-TestLog "`n[3.3] Testing SLA Calculation Accuracy..." "INFO"
foreach ($complaint in ($testData.Complaints | Select-Object -First 3)) {
    $totalTests++
    try {
        $detail = Invoke-RestMethod -Uri "$BaseUrl/complaints/$($complaint.id)" -Headers $headers -Method Get

        if ($detail.createdAt -and $detail.slaHours) {
            $createdTime = [DateTime]::Parse($detail.createdAt)
            $now = Get-Date
            $elapsedHours = ($now - $createdTime).TotalHours
            $expectedRemaining = $detail.slaHours - $elapsedHours

            Write-TestLog "Complaint: $($detail.title)" "INFO"
            Write-TestLog "  Created: $($detail.createdAt)" "INFO"
            Write-TestLog "  SLA Hours: $($detail.slaHours)" "INFO"
            Write-TestLog "  Calculated Elapsed: $([Math]::Round($elapsedHours, 2))h" "INFO"
            Write-TestLog "  API Elapsed: $($detail.timeElapsed)h" "INFO"
            Write-TestLog "  Calculated Remaining: $([Math]::Round($expectedRemaining, 2))h" "INFO"
            Write-TestLog "  API Remaining: $($detail.timeRemaining)h" "INFO"

            # Check accuracy (within 0.1 hour tolerance)
            if ($detail.timeRemaining -and [Math]::Abs($detail.timeRemaining - $expectedRemaining) -lt 0.1) {
                Write-TestLog "  ✅ SLA calculation is accurate" "PASS"
                $passedTests++
            } else {
                Write-TestLog "  ⚠️ SLA calculation may be inaccurate" "WARN"
                $failedTests++
            }
        }
    } catch {
        Write-TestLog "SLA calculation test failed: $($_.Exception.Message)" "FAIL"
        $failedTests++
    }
}

# ==================================================================================
# PHASE 4: TEST SLA BREACH SCENARIOS
# ==================================================================================
Write-TestLog "`n===== PHASE 4: SLA BREACH SCENARIOS =====" "INFO"

# Test 4.1: SLA Inheritance from Category
Write-TestLog "`n[4.1] Testing SLA Inheritance..." "INFO"
$totalTests++
try {
    $catWithSLA = $createdCategories | Where-Object { $_.defaultSlaHours -gt 0 } | Select-Object -First 1

    if ($catWithSLA -and $testData.Priorities.Count -gt 0 -and $testData.Statuses.Count -gt 0) {
        $inheritanceData = @{
            title = "SLA-INHERITANCE-TEST"
            description = "Testing SLA inheritance from category"
            categoryId = $catWithSLA.id
            priorityMasterId = $testData.Priorities[0].id
            statusMasterId = $testData.Statuses[0].id
        }

        $body = $inheritanceData | ConvertTo-Json
        $created = Invoke-RestMethod -Uri "$BaseUrl/complaints" -Headers $headers -Method Post -Body $body

        # Check inherited SLA
        $detail = Invoke-RestMethod -Uri "$BaseUrl/complaints/$($created.id)" -Headers $headers -Method Get

        Write-TestLog "Category SLA: $($catWithSLA.defaultSlaHours)h" "INFO"
        Write-TestLog "Complaint SLA: $($detail.slaHours)h" "INFO"

        if ($detail.slaHours -eq $catWithSLA.defaultSlaHours) {
            Write-TestLog "✅ SLA correctly inherited from category" "PASS"
            $passedTests++
        } else {
            Write-TestLog "⚠️ SLA inheritance mismatch (Expected: $($catWithSLA.defaultSlaHours), Got: $($detail.slaHours))" "WARN"
            $failedTests++
        }
    }
} catch {
    Write-TestLog "SLA inheritance test failed: $($_.Exception.Message)" "FAIL"
    $failedTests++
}

# ==================================================================================
# PHASE 5: EDGE CASES
# ==================================================================================
Write-TestLog "`n===== PHASE 5: EDGE CASES =====" "INFO"

# Test 5.1: Category without SLA
Write-TestLog "`n[5.1] Testing Category Without SLA..." "INFO"
$totalTests++
try {
    $noSLACat = @{
        name = "SLA-TEST-No-SLA"
        description = "Category without SLA configuration"
    }

    $body = $noSLACat | ConvertTo-Json
    $created = Invoke-RestMethod -Uri "$BaseUrl/categories" -Headers $headers -Method Post -Body $body

    # Create complaint in this category
    $complaintData = @{
        title = "SLA-TEST-No-SLA-Complaint"
        description = "Testing complaint with no SLA"
        categoryId = $created.id
        priorityMasterId = $testData.Priorities[0].id
        statusMasterId = $testData.Statuses[0].id
    }

    $body = $complaintData | ConvertTo-Json
    $complaint = Invoke-RestMethod -Uri "$BaseUrl/complaints" -Headers $headers -Method Post -Body $body

    $detail = Invoke-RestMethod -Uri "$BaseUrl/complaints/$($complaint.id)" -Headers $headers -Method Get

    Write-TestLog "Complaint SLA Hours: $($detail.slaHours)" "INFO"

    if ($null -eq $detail.slaHours -or $detail.slaHours -eq 0) {
        Write-TestLog "✅ No-SLA category handled correctly (SLA is null/0)" "PASS"
        $passedTests++
    } else {
        Write-TestLog "⚠️ No-SLA category has SLA value: $($detail.slaHours)" "WARN"
        $failedTests++
    }

} catch {
    Write-TestLog "No-SLA test failed: $($_.Exception.Message)" "FAIL"
    $failedTests++
}

# Test 5.2: Very Short SLA (30 minutes)
Write-TestLog "`n[5.2] Testing Very Short SLA (0.5 hours)..." "INFO"
$totalTests++
try {
    $shortSLACat = @{
        name = "SLA-TEST-Ultra-Short"
        description = "Category with 30-minute SLA"
        defaultSlaHours = 0.5
        slaHours = 0.5
    }

    $body = $shortSLACat | ConvertTo-Json
    $created = Invoke-RestMethod -Uri "$BaseUrl/categories" -Headers $headers -Method Post -Body $body

    Write-TestLog "✅ Created category with 0.5 hour (30 min) SLA" "PASS"
    $passedTests++

} catch {
    Write-TestLog "Short SLA test failed: $($_.Exception.Message)" "FAIL"
    $failedTests++
}

# Test 5.3: Very Long SLA (999 hours)
Write-TestLog "`n[5.3] Testing Very Long SLA (999 hours)..." "INFO"
$totalTests++
try {
    $longSLACat = @{
        name = "SLA-TEST-Very-Long"
        description = "Category with 999-hour SLA"
        defaultSlaHours = 999
        slaHours = 999
    }

    $body = $longSLACat | ConvertTo-Json
    $created = Invoke-RestMethod -Uri "$BaseUrl/categories" -Headers $headers -Method Post -Body $body

    Write-TestLog "✅ Created category with 999 hour SLA" "PASS"
    $passedTests++

} catch {
    Write-TestLog "Long SLA test failed: $($_.Exception.Message)" "FAIL"
    $failedTests++
}

# ==================================================================================
# FINAL SUMMARY
# ==================================================================================
Write-TestLog "`n========================================" "INFO"
Write-TestLog "TEST EXECUTION COMPLETE" "INFO"
Write-TestLog "========================================" "INFO"
Write-TestLog "" "INFO"
Write-TestLog "Total Tests: $totalTests" "INFO"
Write-TestLog "Passed: $passedTests" "PASS"
Write-TestLog "Failed: $failedTests" "FAIL"
$successRate = if ($totalTests -gt 0) { [Math]::Round(($passedTests / $totalTests) * 100, 2) } else { 0 }
Write-TestLog "Success Rate: $successRate%" "INFO"
Write-TestLog "" "INFO"
Write-TestLog "Results saved to: $resultsFile" "INFO"

if ($failedTests -eq 0) {
    Write-TestLog "🎉 ALL TESTS PASSED!" "PASS"
    exit 0
} else {
    Write-TestLog "⚠️ Some tests failed. Review the log for details." "WARN"
    exit 1
}
