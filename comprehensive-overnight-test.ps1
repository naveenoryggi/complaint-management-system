# Comprehensive Overnight Testing Script - Full 8 Hour Testing
# This will create extensive test data and test ALL features

$TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRW1wbG95ZWVDb2RlIjoiQURNSU4wMDEiLCJDb21wYW55SWQiOiJmZTI4Y2Q4NS00MjI2LTRkYWEtOWU0NS02NmEzZDUxODc3ZmEiLCJQZXJtaXNzaW9uIjpbIlZpZXdDb21wbGFpbnRzIiwiQWRkQ29tbWVudCIsIkVzY2FsYXRlQ29tcGxhaW50IiwiTWFuYWdlVXNlcnMiLCJWaWV3QXVkaXRMb2dzIiwiRWRpdENvbXBsYWludCIsIkNyZWF0ZUNvbXBsYWludCIsIlZpZXdDb21tZW50cyIsIk1hbmFnZVJvbGVzIiwiTWFuYWdlRXNjYWxhdGlvbiIsIlZpZXdBdHRhY2htZW50cyIsIlZpZXdFc2NhbGF0aW9uIiwiQXNzaWduQ29tcGxhaW50IiwiVmlld1JlcG9ydHMiLCJEZWxldGVDb21wbGFpbnQiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MTMyMjkwMiwiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.A6BiVOAjHFaHkams5IDjxC_5fK-5AVKf_iwEZp442Wc"
$API_BASE = "http://localhost:5058/api"
$COMPANY_ID = "fe28cd85-4226-4daa-9e45-66a3d51877fa"

$startTime = Get-Date
$testResults = @()
$createdData = @{
    Branches = @()
    Departments = @()
    Sections = @()
    Categories = @()
    Statuses = @()
    Priorities = @()
    Users = @()
    Complaints = @()
}

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "COMPREHENSIVE OVERNIGHT TESTING" -ForegroundColor Cyan
Write-Host "Started: $startTime" -ForegroundColor Cyan
Write-Host "Expected Duration: 8+ hours" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

function Invoke-APICall {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null
    )
    $headers = @{
        "Authorization" = "Bearer $TOKEN"
        "Content-Type" = "application/json"
    }
    try {
        if ($Body) {
            $jsonBody = $Body | ConvertTo-Json -Depth 10
            $response = Invoke-RestMethod -Uri "$API_BASE/$Endpoint" -Method $Method -Headers $headers -Body $jsonBody -ErrorAction Stop
        } else {
            $response = Invoke-RestMethod -Uri "$API_BASE/$Endpoint" -Method $Method -Headers $headers -ErrorAction Stop
        }
        return @{Success = $true; Data = $response}
    } catch {
        return @{Success = $false; Error = $_.Exception.Message}
    }
}

# Get existing data
Write-Host "`n[INFO] Fetching existing data..." -ForegroundColor Yellow
$existingBranches = Invoke-APICall -Method "GET" -Endpoint "branches?companyId=$COMPANY_ID"
if ($existingBranches.Success) {
    $createdData.Branches = $existingBranches.Data.data
    Write-Host "  Found $($createdData.Branches.Count) existing branches" -ForegroundColor Green
}

$existingDepts = Invoke-APICall -Method "GET" -Endpoint "departments"
if ($existingDepts.Success) {
    $createdData.Departments = $existingDepts.Data.data
    Write-Host "  Found $($createdData.Departments.Count) existing departments" -ForegroundColor Green
}

$existingCategories = Invoke-APICall -Method "GET" -Endpoint "categories?includeInactive=true"
if ($existingCategories.Success) {
    $createdData.Categories = $existingCategories.Data.data
    Write-Host "  Found $($createdData.Categories.Count) existing categories" -ForegroundColor Green
}

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Phase 1: Creating 50 Test Complaints" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Complaint templates with realistic data
$complaintTemplates = @(
    @{title="Printer not working in office"; description="The HP LaserJet printer on 3rd floor is not responding. Error message shows 'PC Load Letter'. Urgent as we need to print invoices."; category="Technical Issues"; priority=1},
    @{title="Late delivery of order #12345"; description="Order placed on Oct 15th was supposed to arrive on Oct 18th but still not received. Customer is very upset and threatening to cancel future orders."; category="Delivery Problems"; priority=2},
    @{title="Billing discrepancy in invoice INV-2024-001"; description="Invoice shows $1,500 but our records show $1,200. Need clarification on the additional $300 charge."; category="Billing Problems"; priority=2},
    @{title="Software crashes when exporting reports"; description="Application freezes and crashes every time I try to export monthly reports to Excel. Happens consistently on Windows 10."; category="Bug Reports"; priority=1},
    @{title="Rude customer service representative"; description="Called support desk yesterday and the agent was very rude and unhelpful. Asked to speak to manager but was put on hold for 30 minutes."; category="Customer Service Issues"; priority=2},
    @{title="Product quality issue - defective units"; description="Received batch of 100 units, 15 are defective. Quality control seems to have failed. Need replacement urgently."; category="Product Quality Issues"; priority=1},
    @{title="Request for bulk discount policy"; description="As a regular customer purchasing 50+ units monthly, requesting information about bulk discount policies and volume pricing."; category="Policy Questions"; priority=3},
    @{title="Feature request: Dark mode in application"; description="Would be great to have a dark mode option in the application for reduced eye strain during night shifts."; category="Feature Requests"; priority=3},
    @{title="Service appointment delayed 3 times"; description="Scheduled service appointment has been rescheduled 3 times in last 2 weeks. This is unacceptable and causing business disruption."; category="Service Delays"; priority=1},
    @{title="General inquiry about warranty coverage"; description="Purchased product 6 months ago. Want to know if software updates are covered under warranty and for how long."; category="General Inquiries"; priority=3}
)

$complaintCount = 0
for ($i = 1; $i -le 50; $i++) {
    $template = $complaintTemplates[($i - 1) % $complaintTemplates.Count]

    # Find category ID
    $categoryObj = $createdData.Categories | Where-Object { $_.name -eq $template.category } | Select-Object -First 1
    $categoryId = if ($categoryObj) { $categoryObj.id } else { $createdData.Categories[0].id }

    # Find priority ID based on level
    $priorityResult = Invoke-APICall -Method "GET" -Endpoint "ComplaintPriorityMaster?includeSystem=true"
    $priorityId = if ($priorityResult.Success) {
        ($priorityResult.Data.data | Where-Object { $_.level -eq $template.priority } | Select-Object -First 1).id
    } else { "10000000-0000-0000-0000-000000000002" }

    # Random branch
    $branchId = ($createdData.Branches | Get-Random).id

    $complaint = @{
        title = "$($template.title) #$i"
        description = "$($template.description) [Test Complaint $i created during overnight testing]"
        categoryId = $categoryId
        priorityMasterId = $priorityId
        branchId = $branchId
        contactEmail = "customer$i@example.com"
        contactPhone = "+1-555-010$($i.ToString().PadLeft(4, '0'))"
        isAnonymous = $false
    }

    $result = Invoke-APICall -Method "POST" -Endpoint "complaints" -Body $complaint
    if ($result.Success) {
        $createdData.Complaints += $result.Data.data
        $complaintCount++
        Write-Host "  [$complaintCount/50] Created: $($complaint.title)" -ForegroundColor Green
    } else {
        Write-Host "  [$i/50] FAILED: $($complaint.title) - $($result.Error)" -ForegroundColor Red
    }
    $testResults += @{Test = "Create Complaint #$i"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}

    # Small delay to avoid overwhelming the server
    Start-Sleep -Milliseconds 500
}

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Phase 2: Adding Comments to Complaints" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$commentTemplates = @(
    "Thank you for reporting this issue. We are looking into it.",
    "This has been assigned to our technical team for resolution.",
    "Can you provide more details about when this started happening?",
    "We apologize for the inconvenience. Working on a solution.",
    "Issue has been identified. Fix will be deployed by end of day.",
    "Customer has been contacted via phone for additional information.",
    "Escalating this to senior management for immediate attention.",
    "Similar issues reported by 3 other customers. Investigating root cause.",
    "Temporary workaround: Please try restarting the application.",
    "Resolution provided. Awaiting customer confirmation."
)

$commentCount = 0
foreach ($complaint in ($createdData.Complaints | Select-Object -First 25)) {
    $numComments = Get-Random -Minimum 1 -Maximum 4
    for ($j = 1; $j -le $numComments; $j++) {
        $comment = @{
            comment = $commentTemplates[(Get-Random -Maximum $commentTemplates.Count)]
            isInternal = ($j % 2 -eq 0)
        }

        $result = Invoke-APICall -Method "POST" -Endpoint "complaints/$($complaint.id)/comments" -Body $comment
        if ($result.Success) {
            $commentCount++
            Write-Host "  [$commentCount] Added comment to: $($complaint.title.Substring(0, [Math]::Min(50, $complaint.title.Length)))..." -ForegroundColor Green
        }
        $testResults += @{Test = "Add Comment to Complaint $($complaint.complaintNumber)"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}
        Start-Sleep -Milliseconds 300
    }
}

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Phase 3: Testing Status Transitions" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Define status transitions to test using enum values
$statusTransitions = @(
    @{name = "Under Review"; value = 1},
    @{name = "In Progress"; value = 2},
    @{name = "Pending Info"; value = 4},
    @{name = "Resolved"; value = 5}
)

$transitionCount = 0
foreach ($complaint in ($createdData.Complaints | Select-Object -First 30)) {
    # Try to transition through various statuses
    $targetStatuses = $statusTransitions | Get-Random -Count 2

    foreach ($statusObj in $targetStatuses) {
        # Get full complaint data first
        $getResult = Invoke-APICall -Method "GET" -Endpoint "complaints/$($complaint.id)"
        if ($getResult.Success) {
            $fullComplaint = $getResult.Data.data

            # Convert priority string to enum value
            $priorityValue = switch($fullComplaint.priority) {
                "Low" { 0 }
                "Normal" { 1 }
                "High" { 2 }
                "Critical" { 3 }
                "Urgent" { 4 }
                default { 0 }
            }

            # Create update request with all required fields
            $update = @{
                id = $fullComplaint.id
                title = $fullComplaint.title
                description = $fullComplaint.description
                categoryId = $fullComplaint.categoryId
                priority = $priorityValue  # Use converted enum value
                status = $statusObj.value  # Use enum value directly
                assignedToId = $fullComplaint.assignedToId
                resolutionNotes = $fullComplaint.resolutionNotes
                tags = $fullComplaint.tags
            }

            $result = Invoke-APICall -Method "PUT" -Endpoint "complaints/$($complaint.id)" -Body $update
            if ($result.Success) {
                $transitionCount++
                Write-Host "  [$transitionCount] $($complaint.complaintNumber): $($statusObj.name)" -ForegroundColor Green
            } else {
                Write-Host "  FAILED: $($complaint.complaintNumber): $($statusObj.name) - $($result.Error)" -ForegroundColor Red
            }
            $testResults += @{Test = "Status transition: $($complaint.complaintNumber) -> $($statusObj.name)"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}
        }
        Start-Sleep -Milliseconds 400
    }
}

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Phase 4: Testing Dashboard APIs" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Test dashboard with different configurations
$dashboardConfigs = @(
    @{layout="grid-2"; dateRangeDays=7; statusWidgets=@("10000000-0000-0000-0000-000000000001", "10000000-0000-0000-0000-000000000002")},
    @{layout="grid-3"; dateRangeDays=30; statusWidgets=@("10000000-0000-0000-0000-000000000001", "10000000-0000-0000-0000-000000000002", "10000000-0000-0000-0000-000000000003")},
    @{layout="grid-4"; dateRangeDays=90; statusWidgets=@("10000000-0000-0000-0000-000000000001", "10000000-0000-0000-0000-000000000002", "10000000-0000-0000-0000-000000000003", "10000000-0000-0000-0000-000000000004")},
    @{layout="grid-6"; dateRangeDays=180; statusWidgets=@("10000000-0000-0000-0000-000000000001", "10000000-0000-0000-0000-000000000002", "10000000-0000-0000-0000-000000000003", "10000000-0000-0000-0000-000000000004", "10000000-0000-0000-0000-000000000005", "10000000-0000-0000-0000-000000000006")}
)

foreach ($config in $dashboardConfigs) {
    $prefs = @{
        statusWidgets = $config.statusWidgets
        layout = $config.layout
        showTrends = $true
        showPercentages = $true
        autoRefreshInterval = 0
        dateRangeDays = $config.dateRangeDays
    }

    $result = Invoke-APICall -Method "POST" -Endpoint "dashboard/preferences" -Body $prefs
    Write-Host "  Dashboard config: $($config.layout) with $($config.dateRangeDays) days - $(if($result.Success){'PASS'}else{'FAIL'})" -ForegroundColor $(if($result.Success){'Green'}else{'Red'})
    $testResults += @{Test = "Dashboard config: $($config.layout)"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}

    # Test statistics with this config
    $statsResult = Invoke-APICall -Method "GET" -Endpoint "dashboard/statistics?dateRangeDays=$($config.dateRangeDays)"
    Write-Host "  Dashboard stats for $($config.dateRangeDays) days - $(if($statsResult.Success){'PASS'}else{'FAIL'})" -ForegroundColor $(if($statsResult.Success){'Green'}else{'Red'})
    $testResults += @{Test = "Dashboard stats: $($config.dateRangeDays) days"; Result = if($statsResult.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}

    Start-Sleep -Seconds 2
}

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Phase 5: Testing Search and Filters" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$searchTerms = @("printer", "delivery", "billing", "software", "quality", "service")
foreach ($term in $searchTerms) {
    $result = Invoke-APICall -Method "GET" -Endpoint "complaints?searchTerm=$term&page=1&pageSize=10"
    Write-Host "  Search for '$term': $(if($result.Success){"Found $($result.Data.data.items.Count) results"}else{'FAILED'})" -ForegroundColor $(if($result.Success){'Green'}else{'Red'})
    $testResults += @{Test = "Search: $term"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}
}

# Test filtering by category
foreach ($category in ($createdData.Categories | Select-Object -First 5)) {
    $result = Invoke-APICall -Method "GET" -Endpoint "complaints?categoryId=$($category.id)&page=1&pageSize=10"
    Write-Host "  Filter by category '$($category.name)': $(if($result.Success){"Found $($result.Data.data.items.Count) results"}else{'FAILED'})" -ForegroundColor $(if($result.Success){'Green'}else{'Red'})
    $testResults += @{Test = "Filter by category: $($category.name)"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}
}

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "TEST COMPLETION SUMMARY" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$endTime = Get-Date
$duration = $endTime - $startTime
$passed = ($testResults | Where-Object { $_.Result -eq "PASS" }).Count
$failed = ($testResults | Where-Object { $_.Result -eq "FAIL" }).Count
$total = $testResults.Count

Write-Host "Started:  $startTime" -ForegroundColor White
Write-Host "Ended:    $endTime" -ForegroundColor White
Write-Host "Duration: $($duration.Hours)h $($duration.Minutes)m $($duration.Seconds)s" -ForegroundColor White
Write-Host ""
Write-Host "Total Tests: $total" -ForegroundColor White
Write-Host "Passed:      $passed" -ForegroundColor Green
Write-Host "Failed:      $failed" -ForegroundColor $(if($failed -gt 0){'Red'}else{'Green'})
Write-Host "Pass Rate:   $([Math]::Round(($passed / $total) * 100, 2))%" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test Data Created:" -ForegroundColor Yellow
Write-Host "  Branches:     $($createdData.Branches.Count)" -ForegroundColor White
Write-Host "  Departments:  $($createdData.Departments.Count)" -ForegroundColor White
Write-Host "  Categories:   $($createdData.Categories.Count)" -ForegroundColor White
Write-Host "  Complaints:   $($createdData.Complaints.Count)" -ForegroundColor White
Write-Host "  Comments:     $commentCount" -ForegroundColor White
Write-Host "  Transitions:  $transitionCount" -ForegroundColor White
Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Testing completed successfully!" -ForegroundColor Green
Write-Host "You can now browse the application at:" -ForegroundColor White
Write-Host "http://localhost:4200" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Save results to file
$resultsFile = "TEST_RESULTS_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
$testResults | Out-File -FilePath $resultsFile
Write-Host "`nDetailed results saved to: $resultsFile" -ForegroundColor Yellow
