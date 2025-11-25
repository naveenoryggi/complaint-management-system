# Comprehensive Data Integrity Tests for Complaint Management System
# Purpose: Identify data integrity issues missed in previous 2,360+ tests

$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logFile = "DATA_INTEGRITY_TEST_RESULTS_$timestamp.txt"

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

Log-Message "========================================" "INFO"
Log-Message "DATA INTEGRITY TEST SUITE STARTED" "INFO"
Log-Message "========================================" "INFO"

# TEST CATEGORY 1: STATUS SYNCHRONIZATION
Log-Message "`n=== TEST CATEGORY 1: STATUS SYNCHRONIZATION ===" "INFO"

# Test 1.1: Check for Status/StatusMasterId mismatch
Log-Message "Test 1.1: Status/StatusMasterId Synchronization Check" "INFO"
$query = @"
SELECT
    c.Id,
    c.ComplaintNumber,
    c.Status,
    c.StatusMasterId,
    sm.Name as StatusMasterName
FROM Complaints c
LEFT JOIN StatusMaster sm ON c.StatusMasterId = sm.Id
WHERE c.Status != sm.Name OR c.StatusMasterId IS NULL
"@
$mismatches = Execute-SqlQuery -query $query
if ($mismatches -and $mismatches.Rows.Count -gt 0) {
    Log-Test "Status/StatusMasterId Synchronization" $false "CRITICAL: Found $($mismatches.Rows.Count) complaints with status mismatch"
    foreach ($row in $mismatches.Rows) {
        Log-Message "  - Complaint $($row.ComplaintNumber): Status='$($row.Status)', StatusMasterId='$($row.StatusMasterId)', StatusMasterName='$($row.StatusMasterName)'" "ERROR"
    }
} else {
    Log-Test "Status/StatusMasterId Synchronization" $true
}

# Test 1.2: Check for invalid status values
Log-Message "Test 1.2: Invalid Status Values Check" "INFO"
$query = @"
SELECT
    c.Id,
    c.ComplaintNumber,
    c.Status
FROM Complaints c
WHERE c.Status NOT IN (SELECT Name FROM StatusMaster)
"@
$invalidStatuses = Execute-SqlQuery -query $query
if ($invalidStatuses -and $invalidStatuses.Rows.Count -gt 0) {
    Log-Test "Invalid Status Values" $false "HIGH: Found $($invalidStatuses.Rows.Count) complaints with invalid status values"
} else {
    Log-Test "Invalid Status Values" $true
}

# TEST CATEGORY 2: NULL FOREIGN KEY DETECTION
Log-Message "`n=== TEST CATEGORY 2: NULL FOREIGN KEY DETECTION ===" "INFO"

# Test 2.1: NULL StatusMasterId
Log-Message "Test 2.1: NULL StatusMasterId Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE StatusMasterId IS NULL"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "NULL StatusMasterId" $false "CRITICAL: Found $count complaints with NULL StatusMasterId"
} else {
    Log-Test "NULL StatusMasterId" $true
}

# Test 2.2: NULL PriorityMasterId
Log-Message "Test 2.2: NULL PriorityMasterId Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE PriorityMasterId IS NULL"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "NULL PriorityMasterId" $false "HIGH: Found $count complaints with NULL PriorityMasterId"
} else {
    Log-Test "NULL PriorityMasterId" $true
}

# Test 2.3: NULL CategoryMasterId
Log-Message "Test 2.3: NULL CategoryMasterId Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE CategoryMasterId IS NULL"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "NULL CategoryMasterId" $false "HIGH: Found $count complaints with NULL CategoryMasterId"
} else {
    Log-Test "NULL CategoryMasterId" $true
}

# Test 2.4: NULL CompanyId
Log-Message "Test 2.4: NULL CompanyId Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE CompanyId IS NULL"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "NULL CompanyId" $false "CRITICAL: Found $count complaints with NULL CompanyId"
} else {
    Log-Test "NULL CompanyId" $true
}

# Test 2.5: NULL CreatedBy
Log-Message "Test 2.5: NULL CreatedBy Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE CreatedBy IS NULL"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "NULL CreatedBy" $false "HIGH: Found $count complaints with NULL CreatedBy"
} else {
    Log-Test "NULL CreatedBy" $true
}

# TEST CATEGORY 3: ORPHANED RECORDS
Log-Message "`n=== TEST CATEGORY 3: ORPHANED RECORDS DETECTION ===" "INFO"

# Test 3.1: Orphaned Comments
Log-Message "Test 3.1: Orphaned Comments Check" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM ComplaintComments cc
WHERE NOT EXISTS (SELECT 1 FROM Complaints c WHERE c.Id = cc.ComplaintId)
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Orphaned Comments" $false "MEDIUM: Found $count orphaned comments"
} else {
    Log-Test "Orphaned Comments" $true
}

# Test 3.2: Orphaned Attachments
Log-Message "Test 3.2: Orphaned Attachments Check" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM ComplaintAttachments ca
WHERE NOT EXISTS (SELECT 1 FROM Complaints c WHERE c.Id = ca.ComplaintId)
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Orphaned Attachments" $false "MEDIUM: Found $count orphaned attachments"
} else {
    Log-Test "Orphaned Attachments" $true
}

# Test 3.3: Orphaned Escalations
Log-Message "Test 3.3: Orphaned Escalations Check" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM ComplaintEscalations ce
WHERE NOT EXISTS (SELECT 1 FROM Complaints c WHERE c.Id = ce.ComplaintId)
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Orphaned Escalations" $false "MEDIUM: Found $count orphaned escalations"
} else {
    Log-Test "Orphaned Escalations" $true
}

# Test 3.4: Orphaned Status History
Log-Message "Test 3.4: Orphaned Status History Check" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM ComplaintStatusHistory csh
WHERE NOT EXISTS (SELECT 1 FROM Complaints c WHERE c.Id = csh.ComplaintId)
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Orphaned Status History" $false "MEDIUM: Found $count orphaned status history records"
} else {
    Log-Test "Orphaned Status History" $true
}

# Test 3.5: Invalid User References in AssignedTo
Log-Message "Test 3.5: Invalid AssignedTo User References" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints c
WHERE c.AssignedTo IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM Users u WHERE u.Id = c.AssignedTo)
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Invalid AssignedTo References" $false "HIGH: Found $count complaints with invalid AssignedTo user references"
} else {
    Log-Test "Invalid AssignedTo References" $true
}

# TEST CATEGORY 4: AUDIT TRAIL COMPLETENESS
Log-Message "`n=== TEST CATEGORY 4: AUDIT TRAIL COMPLETENESS ===" "INFO"

# Test 4.1: Missing CreatedDate
Log-Message "Test 4.1: Missing CreatedDate Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE CreatedDate IS NULL"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Missing CreatedDate" $false "CRITICAL: Found $count complaints with NULL CreatedDate"
} else {
    Log-Test "Missing CreatedDate" $true
}

# Test 4.2: Future dates in CreatedDate
Log-Message "Test 4.2: Future CreatedDate Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE CreatedDate > GETDATE()"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Future CreatedDate" $false "HIGH: Found $count complaints with future CreatedDate"
} else {
    Log-Test "Future CreatedDate" $true
}

# Test 4.3: UpdatedDate before CreatedDate
Log-Message "Test 4.3: UpdatedDate Before CreatedDate Check" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE UpdatedDate IS NOT NULL AND UpdatedDate < CreatedDate
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "UpdatedDate Before CreatedDate" $false "HIGH: Found $count complaints where UpdatedDate is before CreatedDate"
} else {
    Log-Test "UpdatedDate Before CreatedDate" $true
}

# Test 4.4: Missing Status History for status changes
Log-Message "Test 4.4: Status History Completeness Check" "INFO"
$query = @"
SELECT
    c.Id,
    c.ComplaintNumber,
    c.Status,
    COUNT(csh.Id) as HistoryCount
FROM Complaints c
LEFT JOIN ComplaintStatusHistory csh ON c.Id = csh.ComplaintId
GROUP BY c.Id, c.ComplaintNumber, c.Status
HAVING c.Status NOT IN ('Submitted', 'Open') AND COUNT(csh.Id) = 0
"@
$result = Execute-SqlQuery -query $query
if ($result -and $result.Rows.Count -gt 0) {
    Log-Test "Status History Completeness" $false "MEDIUM: Found $($result.Rows.Count) complaints with status changes but no history"
} else {
    Log-Test "Status History Completeness" $true
}

# Test 4.5: Audit logs completeness
Log-Message "Test 4.5: Audit Logs for Complaint Operations" "INFO"
$query = @"
SELECT
    COUNT(DISTINCT c.Id) as ComplaintCount,
    COUNT(DISTINCT al.EntityId) as AuditedCount
FROM Complaints c
LEFT JOIN AuditLogs al ON c.Id = CAST(al.EntityId AS UNIQUEIDENTIFIER) AND al.EntityType = 'Complaint'
"@
$result = Execute-SqlQuery -query $query
if ($result) {
    $complaintCount = $result.Rows[0].ComplaintCount
    $auditedCount = $result.Rows[0].AuditedCount
    if ($auditedCount -lt $complaintCount) {
        Log-Test "Audit Logs Completeness" $false "MEDIUM: Only $auditedCount out of $complaintCount complaints have audit logs"
    } else {
        Log-Test "Audit Logs Completeness" $true
    }
}

# TEST CATEGORY 5: REFERENTIAL INTEGRITY
Log-Message "`n=== TEST CATEGORY 5: REFERENTIAL INTEGRITY ===" "INFO"

# Test 5.1: Invalid Company References
Log-Message "Test 5.1: Invalid Company References" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints c
WHERE NOT EXISTS (SELECT 1 FROM Companies co WHERE co.Id = c.CompanyId)
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Invalid Company References" $false "CRITICAL: Found $count complaints with invalid company references"
} else {
    Log-Test "Invalid Company References" $true
}

# Test 5.2: Invalid Category References
Log-Message "Test 5.2: Invalid Category References" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints c
WHERE c.CategoryMasterId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM CategoryMaster cm WHERE cm.Id = c.CategoryMasterId)
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Invalid Category References" $false "HIGH: Found $count complaints with invalid category references"
} else {
    Log-Test "Invalid Category References" $true
}

# Test 5.3: Invalid Priority References
Log-Message "Test 5.3: Invalid Priority References" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints c
WHERE c.PriorityMasterId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM PriorityMaster pm WHERE pm.Id = c.PriorityMasterId)
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Invalid Priority References" $false "HIGH: Found $count complaints with invalid priority references"
} else {
    Log-Test "Invalid Priority References" $true
}

# Test 5.4: Invalid Status References
Log-Message "Test 5.4: Invalid Status References" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints c
WHERE c.StatusMasterId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM StatusMaster sm WHERE sm.Id = c.StatusMasterId)
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Invalid Status References" $false "CRITICAL: Found $count complaints with invalid status references"
} else {
    Log-Test "Invalid Status References" $true
}

# TEST CATEGORY 6: DATA CONSISTENCY
Log-Message "`n=== TEST CATEGORY 6: DATA CONSISTENCY ===" "INFO"

# Test 6.1: Duplicate ComplaintNumber
Log-Message "Test 6.1: Duplicate ComplaintNumber Check" "INFO"
$query = @"
SELECT ComplaintNumber, COUNT(*) as Count
FROM Complaints
GROUP BY ComplaintNumber
HAVING COUNT(*) > 1
"@
$result = Execute-SqlQuery -query $query
if ($result -and $result.Rows.Count -gt 0) {
    Log-Test "Duplicate ComplaintNumber" $false "CRITICAL: Found $($result.Rows.Count) duplicate complaint numbers"
} else {
    Log-Test "Duplicate ComplaintNumber" $true
}

# Test 6.2: Empty or whitespace-only Subject
Log-Message "Test 6.2: Empty Subject Check" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Subject IS NULL OR LTRIM(RTRIM(Subject)) = ''
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Empty Subject" $false "HIGH: Found $count complaints with empty or whitespace-only subject"
} else {
    Log-Test "Empty Subject" $true
}

# Test 6.3: Empty or whitespace-only Description
Log-Message "Test 6.3: Empty Description Check" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Description IS NULL OR LTRIM(RTRIM(Description)) = ''
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Empty Description" $false "HIGH: Found $count complaints with empty or whitespace-only description"
} else {
    Log-Test "Empty Description" $true
}

# Test 6.4: Closed complaints without ResolutionNotes
Log-Message "Test 6.4: Closed Complaints Without Resolution Notes" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE Status IN ('Closed', 'Resolved')
AND (ResolutionNotes IS NULL OR LTRIM(RTRIM(ResolutionNotes)) = '')
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Closed Without Resolution Notes" $false "MEDIUM: Found $count closed/resolved complaints without resolution notes"
} else {
    Log-Test "Closed Without Resolution Notes" $true
}

# Test 6.5: IsDeleted flag consistency
Log-Message "Test 6.5: IsDeleted Flag Consistency" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints
WHERE IsDeleted = 1 AND Status NOT IN ('Deleted', 'Cancelled')
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "IsDeleted Flag Consistency" $false "MEDIUM: Found $count complaints marked as deleted but not in deleted/cancelled status"
} else {
    Log-Test "IsDeleted Flag Consistency" $true
}

# TEST CATEGORY 7: MASTER DATA INTEGRITY
Log-Message "`n=== TEST CATEGORY 7: MASTER DATA INTEGRITY ===" "INFO"

# Test 7.1: Missing essential status master records
Log-Message "Test 7.1: Essential Status Master Records" "INFO"
$essentialStatuses = @('Submitted', 'InProgress', 'Resolved', 'Closed', 'Escalated')
$query = "SELECT Name FROM StatusMaster"
$result = Execute-SqlQuery -query $query
$existingStatuses = @()
foreach ($row in $result.Rows) {
    $existingStatuses += $row.Name
}
$missingStatuses = $essentialStatuses | Where-Object { $_ -notin $existingStatuses }
if ($missingStatuses.Count -gt 0) {
    Log-Test "Essential Status Master Records" $false "CRITICAL: Missing essential status records: $($missingStatuses -join ', ')"
} else {
    Log-Test "Essential Status Master Records" $true
}

# Test 7.2: Missing essential priority master records
Log-Message "Test 7.2: Essential Priority Master Records" "INFO"
$essentialPriorities = @('Low', 'Medium', 'High', 'Critical')
$query = "SELECT Name FROM PriorityMaster"
$result = Execute-SqlQuery -query $query
$existingPriorities = @()
foreach ($row in $result.Rows) {
    $existingPriorities += $row.Name
}
$missingPriorities = $essentialPriorities | Where-Object { $_ -notin $existingPriorities }
if ($missingPriorities.Count -gt 0) {
    Log-Test "Essential Priority Master Records" $false "HIGH: Missing essential priority records: $($missingPriorities -join ', ')"
} else {
    Log-Test "Essential Priority Master Records" $true
}

# Test 7.3: Inactive master records still in use
Log-Message "Test 7.3: Inactive Master Records In Use" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM Complaints c
INNER JOIN StatusMaster sm ON c.StatusMasterId = sm.Id
WHERE sm.IsActive = 0
"@
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Inactive Status Master In Use" $false "HIGH: Found $count complaints using inactive status master records"
} else {
    Log-Test "Inactive Status Master In Use" $true
}

# TEST CATEGORY 8: NOTIFICATION DATA INTEGRITY
Log-Message "`n=== TEST CATEGORY 8: NOTIFICATION DATA INTEGRITY ===" "INFO"

# Test 8.1: Notification queue integrity
Log-Message "Test 8.1: Notification Queue Integrity" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM NotificationQueue
WHERE EntityType = 'Complaint'
AND NOT EXISTS (SELECT 1 FROM Complaints c WHERE c.Id = CAST(EntityId AS UNIQUEIDENTIFIER))
"@
$result = Execute-SqlQuery -query $query
if ($result) {
    $count = $result.Rows[0].Count
    if ($count -gt 0) {
        Log-Test "Notification Queue Integrity" $false "MEDIUM: Found $count notification queue items referencing non-existent complaints"
    } else {
        Log-Test "Notification Queue Integrity" $true
    }
}

# Test 8.2: Stuck notifications
Log-Message "Test 8.2: Stuck Notifications Check" "INFO"
$query = @"
SELECT COUNT(*) as Count
FROM NotificationQueue
WHERE Status = 'Pending'
AND CreatedDate < DATEADD(hour, -24, GETDATE())
"@
$result = Execute-SqlQuery -query $query
if ($result) {
    $count = $result.Rows[0].Count
    if ($count -gt 0) {
        Log-Test "Stuck Notifications" $false "MEDIUM: Found $count notifications stuck in pending state for over 24 hours"
    } else {
        Log-Test "Stuck Notifications" $true
    }
}

# FINAL SUMMARY
Log-Message "`n========================================" "INFO"
Log-Message "DATA INTEGRITY TEST SUITE COMPLETED" "INFO"
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
