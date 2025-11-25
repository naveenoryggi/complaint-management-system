# Comprehensive Dashboard Validation Tests for Complaint Management System
# Purpose: Validate dashboard widget counts against actual database queries

$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logFile = "DASHBOARD_TEST_RESULTS_$timestamp.txt"

# Configuration
$dbServer = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
$dbName = "ComplaintManagementDB"
$apiBase = "http://localhost:5058"
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRW1wbG95ZWVDb2RlIjoiQURNSU4wMDEiLCJDb21wYW55SWQiOiJmZTI4Y2Q4NS00MjI2LTRkYWEtOWU0NS02NmEzZDUxODc3ZmEiLCJQZXJtaXNzaW9uIjpbIlZpZXdDb21wbGFpbnRzIiwiQWRkQ29tbWVudCIsIkVzY2FsYXRlQ29tcGxhaW50IiwiTWFuYWdlVXNlcnMiLCJWaWV3QXVkaXRMb2dzIiwiRWRpdENvbXBsYWludCIsIkNyZWF0ZUNvbXBsYWludCIsIlZpZXdDb21tZW50cyIsIk1hbmFnZVJvbGVzIiwiTWFuYWdlRXNjYWxhdGlvbiIsIlZpZXdBdHRhY2htZW50cyIsIlZpZXdFc2NhbGF0aW9uIiwiQXNzaWduQ29tcGxhaW50IiwiVmlld1JlcG9ydHMiLCJEZWxldGVDb21wbGFpbnQiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MTI4MDg4NSwiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.B4JHfPaF_IBhd7DsYoUxIg4TcdkRiXry7nIcfTKGJuo"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Test counters
$totalTests = 0
$passedTests = 0
$failedTests = 0
$issues = @()

function Log-Message {
    param([string]$message, [string]$level = "INFO")
    $logMessage = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [$level] $message"
    Write-Host $logMessage
    Add-Content -Path $logFile -Value $logMessage
}

function Log-Test {
    param([string]$testName, [bool]$passed, [string]$details = "")
    $script:totalTests++
    if ($passed) {
        $script:passedTests++
        Log-Message "✓ PASS: $testName" "PASS"
    } else {
        $script:failedTests++
        Log-Message "✗ FAIL: $testName - $details" "FAIL"
        $script:issues += @{
            Test = $testName
            Details = $details
            Severity = if ($details -match "CRITICAL") { "CRITICAL" } elseif ($details -match "HIGH") { "HIGH" } else { "MEDIUM" }
        }
    }
}

function Execute-SqlQuery {
    param([string]$query)
    try {
        $connectionString = "Server=$dbServer;Database=$dbName;Integrated Security=True;TrustServerCertificate=True;"
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()

        $command = $connection.CreateCommand()
        $command.CommandText = $query
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
        $dataset = New-Object System.Data.DataSet
        $adapter.Fill($dataset) | Out-Null

        $connection.Close()
        return $dataset.Tables[0]
    } catch {
        Log-Message "SQL Error: $_" "ERROR"
        return $null
    }
}

function Get-ApiData {
    param([string]$endpoint)
    try {
        $response = Invoke-RestMethod -Uri "$apiBase$endpoint" -Headers $headers -Method Get
        return $response
    } catch {
        Log-Message "API Error on $endpoint : $_" "ERROR"
        return $null
    }
}

Log-Message "========================================" "INFO"
Log-Message "DASHBOARD VALIDATION TEST SUITE STARTED" "INFO"
Log-Message "========================================" "INFO"

# TEST CATEGORY 1: STATUS-BASED COUNTS
Log-Message "`n=== TEST CATEGORY 1: STATUS-BASED COMPLAINT COUNTS ===" "INFO"

# Get dashboard data from API
Log-Message "Fetching dashboard data from API..." "INFO"
$dashboardData = Get-ApiData "/api/dashboard/stats"

if ($dashboardData) {
    Log-Message "Dashboard API Response received" "INFO"
} else {
    Log-Message "Failed to fetch dashboard data from API" "ERROR"
}

# Test 1.1: Submitted Count
Log-Message "Test 1.1: Submitted Complaints Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Status = 'Submitted' AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.statusCounts) {
    ($dashboardData.statusCounts | Where-Object { $_.status -eq 'Submitted' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Submitted Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Submitted Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 1.2: InProgress Count
Log-Message "Test 1.2: InProgress Complaints Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Status = 'InProgress' AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.statusCounts) {
    ($dashboardData.statusCounts | Where-Object { $_.status -eq 'InProgress' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "InProgress Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "InProgress Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 1.3: UnderReview Count
Log-Message "Test 1.3: UnderReview Complaints Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Status = 'UnderReview' AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.statusCounts) {
    ($dashboardData.statusCounts | Where-Object { $_.status -eq 'UnderReview' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "UnderReview Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "UnderReview Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 1.4: PendingInfo Count
Log-Message "Test 1.4: PendingInfo Complaints Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Status = 'PendingInfo' AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.statusCounts) {
    ($dashboardData.statusCounts | Where-Object { $_.status -eq 'PendingInfo' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "PendingInfo Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "PendingInfo Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 1.5: Resolved Count
Log-Message "Test 1.5: Resolved Complaints Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Status = 'Resolved' AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.statusCounts) {
    ($dashboardData.statusCounts | Where-Object { $_.status -eq 'Resolved' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Resolved Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Resolved Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 1.6: Closed Count
Log-Message "Test 1.6: Closed Complaints Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Status = 'Closed' AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.statusCounts) {
    ($dashboardData.statusCounts | Where-Object { $_.status -eq 'Closed' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Closed Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Closed Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 1.7: Escalated Count
Log-Message "Test 1.7: Escalated Complaints Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Status = 'Escalated' AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.statusCounts) {
    ($dashboardData.statusCounts | Where-Object { $_.status -eq 'Escalated' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Escalated Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Escalated Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 1.8: Reopened Count
Log-Message "Test 1.8: Reopened Complaints Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Status = 'Reopened' AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.statusCounts) {
    ($dashboardData.statusCounts | Where-Object { $_.status -eq 'Reopened' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Reopened Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Reopened Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# TEST CATEGORY 2: PRIORITY BREAKDOWN
Log-Message "`n=== TEST CATEGORY 2: PRIORITY BREAKDOWN VALIDATION ===" "INFO"

# Test 2.1: Low Priority Count
Log-Message "Test 2.1: Low Priority Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints c
INNER JOIN PriorityMaster pm ON c.PriorityMasterId = pm.Id
WHERE pm.Name = 'Low' AND c.IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.priorityCounts) {
    ($dashboardData.priorityCounts | Where-Object { $_.priority -eq 'Low' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Low Priority Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Low Priority Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 2.2: Medium Priority Count
Log-Message "Test 2.2: Medium Priority Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints c
INNER JOIN PriorityMaster pm ON c.PriorityMasterId = pm.Id
WHERE pm.Name = 'Medium' AND c.IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.priorityCounts) {
    ($dashboardData.priorityCounts | Where-Object { $_.priority -eq 'Medium' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Medium Priority Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Medium Priority Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 2.3: High Priority Count
Log-Message "Test 2.3: High Priority Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints c
INNER JOIN PriorityMaster pm ON c.PriorityMasterId = pm.Id
WHERE pm.Name = 'High' AND c.IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.priorityCounts) {
    ($dashboardData.priorityCounts | Where-Object { $_.priority -eq 'High' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "High Priority Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "High Priority Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 2.4: Critical Priority Count
Log-Message "Test 2.4: Critical Priority Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints c
INNER JOIN PriorityMaster pm ON c.PriorityMasterId = pm.Id
WHERE pm.Name = 'Critical' AND c.IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.priorityCounts) {
    ($dashboardData.priorityCounts | Where-Object { $_.priority -eq 'Critical' }).count
} else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Critical Priority Count Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Critical Priority Count Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# TEST CATEGORY 3: CATEGORY DISTRIBUTION
Log-Message "`n=== TEST CATEGORY 3: CATEGORY DISTRIBUTION VALIDATION ===" "INFO"

# Test 3.1: All categories count match
Log-Message "Test 3.1: Category Distribution Accuracy" "INFO"
$query = @"
SELECT
    cm.Name as CategoryName,
    COUNT(c.Id) as Count
FROM CategoryMaster cm
LEFT JOIN Complaints c ON cm.Id = c.CategoryMasterId AND c.IsDeleted = 0
GROUP BY cm.Name
ORDER BY cm.Name
"@
$dbResult = Execute-SqlQuery -query $query

if ($dbResult -and $dashboardData -and $dashboardData.categoryCounts) {
    $mismatchCount = 0
    foreach ($row in $dbResult.Rows) {
        $categoryName = $row.CategoryName
        $dbCount = $row.Count
        $apiItem = $dashboardData.categoryCounts | Where-Object { $_.category -eq $categoryName }
        $apiCount = if ($apiItem) { $apiItem.count } else { 0 }

        if ($dbCount -ne $apiCount) {
            $mismatchCount++
            Log-Message "  Category '$categoryName': DB=$dbCount, API=$apiCount" "ERROR"
        }
    }

    if ($mismatchCount -eq 0) {
        Log-Test "Category Distribution Accuracy" $true "All categories match"
    } else {
        Log-Test "Category Distribution Accuracy" $false "HIGH: $mismatchCount categories have mismatched counts"
    }
} else {
    Log-Test "Category Distribution Accuracy" $false "CRITICAL: Unable to retrieve category data"
}

# TEST CATEGORY 4: TOTAL COMPLAINT COUNTS
Log-Message "`n=== TEST CATEGORY 4: TOTAL COMPLAINT COUNTS ===" "INFO"

# Test 4.1: Total Active Complaints
Log-Message "Test 4.1: Total Active Complaints" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE IsDeleted = 0"
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.totalComplaints) { $dashboardData.totalComplaints } else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Total Active Complaints Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Total Active Complaints Match" $false "CRITICAL: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 4.2: Total Open Complaints (not closed/resolved)
Log-Message "Test 4.2: Total Open Complaints" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Status NOT IN ('Closed', 'Resolved') AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.openComplaints) { $dashboardData.openComplaints } else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Total Open Complaints Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Total Open Complaints Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# TEST CATEGORY 5: TIME-BASED METRICS
Log-Message "`n=== TEST CATEGORY 5: TIME-BASED METRICS ===" "INFO"

# Test 5.1: Complaints Created Today
Log-Message "Test 5.1: Complaints Created Today" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE CAST(CreatedDate AS DATE) = CAST(GETDATE() AS DATE) AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.complaintsToday) { $dashboardData.complaintsToday } else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Complaints Today Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Complaints Today Match" $false "MEDIUM: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 5.2: Complaints Created This Week
Log-Message "Test 5.2: Complaints Created This Week" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE CreatedDate >= DATEADD(day, -7, GETDATE()) AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.complaintsThisWeek) { $dashboardData.complaintsThisWeek } else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Complaints This Week Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Complaints This Week Match" $false "MEDIUM: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 5.3: Complaints Created This Month
Log-Message "Test 5.3: Complaints Created This Month" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE MONTH(CreatedDate) = MONTH(GETDATE())
AND YEAR(CreatedDate) = YEAR(GETDATE())
AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.complaintsThisMonth) { $dashboardData.complaintsThisMonth } else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Complaints This Month Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Complaints This Month Match" $false "MEDIUM: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# TEST CATEGORY 6: ASSIGNMENT METRICS
Log-Message "`n=== TEST CATEGORY 6: ASSIGNMENT METRICS ===" "INFO"

# Test 6.1: Unassigned Complaints
Log-Message "Test 6.1: Unassigned Complaints Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE AssignedTo IS NULL AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.unassignedComplaints) { $dashboardData.unassignedComplaints } else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Unassigned Complaints Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Unassigned Complaints Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# Test 6.2: Assigned Complaints
Log-Message "Test 6.2: Assigned Complaints Count" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE AssignedTo IS NOT NULL AND IsDeleted = 0
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.assignedComplaints) { $dashboardData.assignedComplaints } else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Assigned Complaints Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Assigned Complaints Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# TEST CATEGORY 7: ESCALATION METRICS
Log-Message "`n=== TEST CATEGORY 7: ESCALATION METRICS ===" "INFO"

# Test 7.1: Total Escalated Complaints
Log-Message "Test 7.1: Total Escalated Complaints" "INFO"
$query = @"
SELECT COUNT(DISTINCT ComplaintId) as Count
FROM ComplaintEscalations
WHERE IsActive = 1
"@
$dbResult = Execute-SqlQuery -query $query
$dbCount = $dbResult.Rows[0].Count
$apiCount = if ($dashboardData -and $dashboardData.escalatedCount) { $dashboardData.escalatedCount } else { 0 }

if ($dbCount -eq $apiCount) {
    Log-Test "Total Escalated Complaints Match" $true "DB: $dbCount, API: $apiCount"
} else {
    Log-Test "Total Escalated Complaints Match" $false "HIGH: Mismatch - DB: $dbCount, API: $apiCount, Difference: $($dbCount - $apiCount)"
}

# TEST CATEGORY 8: CROSS-VALIDATION
Log-Message "`n=== TEST CATEGORY 8: CROSS-VALIDATION CHECKS ===" "INFO"

# Test 8.1: Sum of status counts equals total
Log-Message "Test 8.1: Status Counts Sum Validation" "INFO"
if ($dashboardData -and $dashboardData.statusCounts) {
    $statusSum = ($dashboardData.statusCounts | Measure-Object -Property count -Sum).Sum
    $totalCount = if ($dashboardData.totalComplaints) { $dashboardData.totalComplaints } else { 0 }

    if ($statusSum -eq $totalCount) {
        Log-Test "Status Counts Sum Validation" $true "Sum: $statusSum, Total: $totalCount"
    } else {
        Log-Test "Status Counts Sum Validation" $false "CRITICAL: Status sum ($statusSum) does not match total ($totalCount), Difference: $($statusSum - $totalCount)"
    }
}

# Test 8.2: Sum of priority counts equals total
Log-Message "Test 8.2: Priority Counts Sum Validation" "INFO"
if ($dashboardData -and $dashboardData.priorityCounts) {
    $prioritySum = ($dashboardData.priorityCounts | Measure-Object -Property count -Sum).Sum
    $totalCount = if ($dashboardData.totalComplaints) { $dashboardData.totalComplaints } else { 0 }

    if ($prioritySum -eq $totalCount) {
        Log-Test "Priority Counts Sum Validation" $true "Sum: $prioritySum, Total: $totalCount"
    } else {
        Log-Test "Priority Counts Sum Validation" $false "CRITICAL: Priority sum ($prioritySum) does not match total ($totalCount), Difference: $($prioritySum - $totalCount)"
    }
}

# Test 8.3: Assigned + Unassigned equals Total
Log-Message "Test 8.3: Assignment Counts Sum Validation" "INFO"
if ($dashboardData) {
    $assignedCount = if ($dashboardData.assignedComplaints) { $dashboardData.assignedComplaints } else { 0 }
    $unassignedCount = if ($dashboardData.unassignedComplaints) { $dashboardData.unassignedComplaints } else { 0 }
    $totalCount = if ($dashboardData.totalComplaints) { $dashboardData.totalComplaints } else { 0 }
    $sum = $assignedCount + $unassignedCount

    if ($sum -eq $totalCount) {
        Log-Test "Assignment Counts Sum Validation" $true "Assigned: $assignedCount + Unassigned: $unassignedCount = Total: $totalCount"
    } else {
        Log-Test "Assignment Counts Sum Validation" $false "CRITICAL: Assignment sum ($sum) does not match total ($totalCount), Difference: $($sum - $totalCount)"
    }
}

# FINAL SUMMARY
Log-Message "`n========================================" "INFO"
Log-Message "DASHBOARD VALIDATION TEST SUITE COMPLETED" "INFO"
Log-Message "========================================" "INFO"
Log-Message "Total Tests: $totalTests" "INFO"
Log-Message "Passed: $passedTests" "INFO"
Log-Message "Failed: $failedTests" "INFO"
Log-Message "Success Rate: $(if($totalTests -gt 0){[math]::Round(($passedTests/$totalTests)*100, 2)}else{0})%" "INFO"

if ($issues.Count -gt 0) {
    Log-Message "`n=== ISSUES FOUND ===" "ERROR"
    $criticalIssues = $issues | Where-Object { $_.Severity -eq "CRITICAL" }
    $highIssues = $issues | Where-Object { $_.Severity -eq "HIGH" }
    $mediumIssues = $issues | Where-Object { $_.Severity -eq "MEDIUM" }

    if ($criticalIssues.Count -gt 0) {
        Log-Message "`nCRITICAL Issues ($($criticalIssues.Count)):" "ERROR"
        foreach ($issue in $criticalIssues) {
            Log-Message "  - $($issue.Test): $($issue.Details)" "ERROR"
        }
    }

    if ($highIssues.Count -gt 0) {
        Log-Message "`nHIGH Priority Issues ($($highIssues.Count)):" "ERROR"
        foreach ($issue in $highIssues) {
            Log-Message "  - $($issue.Test): $($issue.Details)" "ERROR"
        }
    }

    if ($mediumIssues.Count -gt 0) {
        Log-Message "`nMEDIUM Priority Issues ($($mediumIssues.Count)):" "ERROR"
        foreach ($issue in $mediumIssues) {
            Log-Message "  - $($issue.Test): $($issue.Details)" "ERROR"
        }
    }
}

Log-Message "`nResults saved to: $logFile" "INFO"
