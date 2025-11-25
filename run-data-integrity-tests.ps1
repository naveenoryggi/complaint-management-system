$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logFile = "DATA_INTEGRITY_TEST_RESULTS_$timestamp.txt"

$dbServer = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
$dbName = "ComplaintManagementDB"

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
        Log-Message "PASS: $testName" "PASS"
    } else {
        $script:failedTests++
        Log-Message "FAIL: $testName - $details" "FAIL"
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
Log-Message "" "INFO"
Log-Message "=== TEST CATEGORY 1: STATUS SYNCHRONIZATION ===" "INFO"

# Test 1.1: Check for Status/StatusMasterId mismatch
Log-Message "Test 1.1: Status/StatusMasterId Synchronization Check" "INFO"
$query = "SELECT c.Id, c.ComplaintNumber, c.Status, c.StatusMasterId, sm.Name as StatusMasterName FROM Complaints c LEFT JOIN StatusMaster sm ON c.StatusMasterId = sm.Id WHERE c.Status != sm.Name OR c.StatusMasterId IS NULL"
$mismatches = Execute-SqlQuery -query $query
if ($mismatches -and $mismatches.Rows.Count -gt 0) {
    Log-Test "Status/StatusMasterId Synchronization" $false "CRITICAL: Found $($mismatches.Rows.Count) complaints with status mismatch"
    foreach ($row in $mismatches.Rows | Select-Object -First 5) {
        Log-Message "  - Complaint $($row.ComplaintNumber): Status='$($row.Status)', StatusMasterName='$($row.StatusMasterName)'" "ERROR"
    }
} else {
    Log-Test "Status/StatusMasterId Synchronization" $true
}

# Test 1.2: Check for invalid status values
Log-Message "Test 1.2: Invalid Status Values Check" "INFO"
$query = "SELECT c.Id, c.ComplaintNumber, c.Status FROM Complaints c WHERE c.Status NOT IN (SELECT Name FROM StatusMaster)"
$invalidStatuses = Execute-SqlQuery -query $query
if ($invalidStatuses -and $invalidStatuses.Rows.Count -gt 0) {
    Log-Test "Invalid Status Values" $false "HIGH: Found $($invalidStatuses.Rows.Count) complaints with invalid status values"
} else {
    Log-Test "Invalid Status Values" $true
}

# TEST CATEGORY 2: NULL FOREIGN KEY DETECTION
Log-Message "" "INFO"
Log-Message "=== TEST CATEGORY 2: NULL FOREIGN KEY DETECTION ===" "INFO"

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
Log-Message "" "INFO"
Log-Message "=== TEST CATEGORY 3: ORPHANED RECORDS DETECTION ===" "INFO"

# Test 3.1: Orphaned Comments
Log-Message "Test 3.1: Orphaned Comments Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM ComplaintComments cc WHERE NOT EXISTS (SELECT 1 FROM Complaints c WHERE c.Id = cc.ComplaintId)"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Orphaned Comments" $false "MEDIUM: Found $count orphaned comments"
} else {
    Log-Test "Orphaned Comments" $true
}

# Test 3.2: Orphaned Attachments
Log-Message "Test 3.2: Orphaned Attachments Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM ComplaintAttachments ca WHERE NOT EXISTS (SELECT 1 FROM Complaints c WHERE c.Id = ca.ComplaintId)"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Orphaned Attachments" $false "MEDIUM: Found $count orphaned attachments"
} else {
    Log-Test "Orphaned Attachments" $true
}

# Test 3.3: Orphaned Escalations
Log-Message "Test 3.3: Orphaned Escalations Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM ComplaintEscalations ce WHERE NOT EXISTS (SELECT 1 FROM Complaints c WHERE c.Id = ce.ComplaintId)"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Orphaned Escalations" $false "MEDIUM: Found $count orphaned escalations"
} else {
    Log-Test "Orphaned Escalations" $true
}

# Test 3.4: Orphaned Status History
Log-Message "Test 3.4: Orphaned Status History Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM ComplaintStatusHistory csh WHERE NOT EXISTS (SELECT 1 FROM Complaints c WHERE c.Id = csh.ComplaintId)"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Orphaned Status History" $false "MEDIUM: Found $count orphaned status history records"
} else {
    Log-Test "Orphaned Status History" $true
}

# Test 3.5: Invalid User References in AssignedTo
Log-Message "Test 3.5: Invalid AssignedTo User References" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints c WHERE c.AssignedTo IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Users u WHERE u.Id = c.AssignedTo)"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Invalid AssignedTo References" $false "HIGH: Found $count complaints with invalid AssignedTo user references"
} else {
    Log-Test "Invalid AssignedTo References" $true
}

# TEST CATEGORY 4: AUDIT TRAIL COMPLETENESS
Log-Message "" "INFO"
Log-Message "=== TEST CATEGORY 4: AUDIT TRAIL COMPLETENESS ===" "INFO"

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
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE UpdatedDate IS NOT NULL AND UpdatedDate < CreatedDate"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "UpdatedDate Before CreatedDate" $false "HIGH: Found $count complaints where UpdatedDate is before CreatedDate"
} else {
    Log-Test "UpdatedDate Before CreatedDate" $true
}

# TEST CATEGORY 5: REFERENTIAL INTEGRITY
Log-Message "" "INFO"
Log-Message "=== TEST CATEGORY 5: REFERENTIAL INTEGRITY ===" "INFO"

# Test 5.1: Invalid Company References
Log-Message "Test 5.1: Invalid Company References" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints c WHERE NOT EXISTS (SELECT 1 FROM Companies co WHERE co.Id = c.CompanyId)"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Invalid Company References" $false "CRITICAL: Found $count complaints with invalid company references"
} else {
    Log-Test "Invalid Company References" $true
}

# Test 5.2: Invalid Category References
Log-Message "Test 5.2: Invalid Category References" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints c WHERE c.CategoryMasterId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM CategoryMaster cm WHERE cm.Id = c.CategoryMasterId)"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Invalid Category References" $false "HIGH: Found $count complaints with invalid category references"
} else {
    Log-Test "Invalid Category References" $true
}

# Test 5.3: Invalid Priority References
Log-Message "Test 5.3: Invalid Priority References" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints c WHERE c.PriorityMasterId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PriorityMaster pm WHERE pm.Id = c.PriorityMasterId)"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Invalid Priority References" $false "HIGH: Found $count complaints with invalid priority references"
} else {
    Log-Test "Invalid Priority References" $true
}

# Test 5.4: Invalid Status References
Log-Message "Test 5.4: Invalid Status References" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints c WHERE c.StatusMasterId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM StatusMaster sm WHERE sm.Id = c.StatusMasterId)"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Invalid Status References" $false "CRITICAL: Found $count complaints with invalid status references"
} else {
    Log-Test "Invalid Status References" $true
}

# TEST CATEGORY 6: DATA CONSISTENCY
Log-Message "" "INFO"
Log-Message "=== TEST CATEGORY 6: DATA CONSISTENCY ===" "INFO"

# Test 6.1: Duplicate ComplaintNumber
Log-Message "Test 6.1: Duplicate ComplaintNumber Check" "INFO"
$query = "SELECT ComplaintNumber, COUNT(*) as Count FROM Complaints GROUP BY ComplaintNumber HAVING COUNT(*) > 1"
$result = Execute-SqlQuery -query $query
if ($result -and $result.Rows.Count -gt 0) {
    Log-Test "Duplicate ComplaintNumber" $false "CRITICAL: Found $($result.Rows.Count) duplicate complaint numbers"
    foreach ($row in $result.Rows | Select-Object -First 3) {
        Log-Message "  - ComplaintNumber: $($row.ComplaintNumber) appears $($row.Count) times" "ERROR"
    }
} else {
    Log-Test "Duplicate ComplaintNumber" $true
}

# Test 6.2: Empty or whitespace-only Subject
Log-Message "Test 6.2: Empty Subject Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE Subject IS NULL OR LTRIM(RTRIM(Subject)) = ''"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Empty Subject" $false "HIGH: Found $count complaints with empty or whitespace-only subject"
} else {
    Log-Test "Empty Subject" $true
}

# Test 6.3: Empty or whitespace-only Description
Log-Message "Test 6.3: Empty Description Check" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE Description IS NULL OR LTRIM(RTRIM(Description)) = ''"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Empty Description" $false "HIGH: Found $count complaints with empty or whitespace-only description"
} else {
    Log-Test "Empty Description" $true
}

# Test 6.4: Closed complaints without ResolutionNotes
Log-Message "Test 6.4: Closed Complaints Without Resolution Notes" "INFO"
$query = "SELECT COUNT(*) as Count FROM Complaints WHERE Status IN ('Closed', 'Resolved') AND (ResolutionNotes IS NULL OR LTRIM(RTRIM(ResolutionNotes)) = '')"
$result = Execute-SqlQuery -query $query
$count = $result.Rows[0].Count
if ($count -gt 0) {
    Log-Test "Closed Without Resolution Notes" $false "MEDIUM: Found $count closed/resolved complaints without resolution notes"
} else {
    Log-Test "Closed Without Resolution Notes" $true
}

# TEST CATEGORY 7: MASTER DATA INTEGRITY
Log-Message "" "INFO"
Log-Message "=== TEST CATEGORY 7: MASTER DATA INTEGRITY ===" "INFO"

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

# FINAL SUMMARY
Log-Message "" "INFO"
Log-Message "========================================" "INFO"
Log-Message "DATA INTEGRITY TEST SUITE COMPLETED" "INFO"
Log-Message "========================================" "INFO"
Log-Message "Total Tests: $totalTests" "INFO"
Log-Message "Passed: $passedTests" "INFO"
Log-Message "Failed: $failedTests" "INFO"
$successRate = if($totalTests -gt 0){[math]::Round(($passedTests/$totalTests)*100, 2)}else{0}
Log-Message "Success Rate: $successRate%" "INFO"

if ($issues.Count -gt 0) {
    Log-Message "" "ERROR"
    Log-Message "=== ISSUES FOUND ===" "ERROR"
    $criticalIssues = $issues | Where-Object { $_.Severity -eq "CRITICAL" }
    $highIssues = $issues | Where-Object { $_.Severity -eq "HIGH" }
    $mediumIssues = $issues | Where-Object { $_.Severity -eq "MEDIUM" }

    if ($criticalIssues.Count -gt 0) {
        Log-Message "" "ERROR"
        Log-Message "CRITICAL Issues ($($criticalIssues.Count)):" "ERROR"
        foreach ($issue in $criticalIssues) {
            Log-Message "  - $($issue.Test): $($issue.Details)" "ERROR"
        }
    }

    if ($highIssues.Count -gt 0) {
        Log-Message "" "ERROR"
        Log-Message "HIGH Priority Issues ($($highIssues.Count)):" "ERROR"
        foreach ($issue in $highIssues) {
            Log-Message "  - $($issue.Test): $($issue.Details)" "ERROR"
        }
    }

    if ($mediumIssues.Count -gt 0) {
        Log-Message "" "ERROR"
        Log-Message "MEDIUM Priority Issues ($($mediumIssues.Count)):" "ERROR"
        foreach ($issue in $mediumIssues) {
            Log-Message "  - $($issue.Test): $($issue.Details)" "ERROR"
        }
    }
} else {
    Log-Message "" "INFO"
    Log-Message "No issues found - All tests passed!" "INFO"
}

Log-Message "" "INFO"
Log-Message "Results saved to: $logFile" "INFO"

# Output issues for parsing
$issues | ConvertTo-Json -Depth 3 | Out-File "data-integrity-issues.json"
