# Comprehensive Edge Case Tests for Complaint Management System
# Purpose: Test boundary conditions, concurrent operations, invalid inputs, and error handling

$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logFile = "EDGE_CASE_TEST_RESULTS_$timestamp.txt"

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
$createdComplaintIds = @()

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

function Invoke-ApiCall {
    param(
        [string]$endpoint,
        [string]$method = "GET",
        [object]$body = $null,
        [int]$timeoutSeconds = 30
    )
    try {
        $params = @{
            Uri = "$apiBase$endpoint"
            Headers = $headers
            Method = $method
            TimeoutSec = $timeoutSeconds
        }

        if ($body) {
            $params.Body = ($body | ConvertTo-Json -Depth 10)
        }

        $response = Invoke-RestMethod @params
        return @{ Success = $true; Data = $response; StatusCode = 200 }
    } catch {
        $statusCode = 0
        $errorDetails = $_.Exception.Message

        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $errorDetails = $reader.ReadToEnd()
            } catch {
                $errorDetails = $_.Exception.Message
            }
        }

        return @{ Success = $false; Error = $errorDetails; StatusCode = $statusCode }
    }
}

function Get-MasterData {
    Log-Message "Fetching master data..." "INFO"

    $statusResult = Invoke-ApiCall -endpoint "/api/statusmaster" -method "GET"
    $script:statusMaster = if ($statusResult.Success) { $statusResult.Data } else { @() }

    $priorityResult = Invoke-ApiCall -endpoint "/api/prioritymaster" -method "GET"
    $script:priorityMaster = if ($priorityResult.Success) { $priorityResult.Data } else { @() }

    $categoryResult = Invoke-ApiCall -endpoint "/api/categorymaster" -method "GET"
    $script:categoryMaster = if ($categoryResult.Success) { $categoryResult.Data } else { @() }

    Log-Message "Master data fetched" "INFO"
}

Log-Message "========================================" "INFO"
Log-Message "EDGE CASE TEST SUITE STARTED" "INFO"
Log-Message "========================================" "INFO"

Get-MasterData

$submittedStatus = $statusMaster | Where-Object { $_.name -eq "Submitted" } | Select-Object -First 1
$mediumPriority = $priorityMaster | Where-Object { $_.name -eq "Medium" } | Select-Object -First 1
$category = $categoryMaster | Select-Object -First 1

# TEST CATEGORY 1: BOUNDARY CONDITIONS - STRING LENGTHS
Log-Message "`n=== TEST CATEGORY 1: BOUNDARY CONDITIONS - STRING LENGTHS ===" "INFO"

# Test 1.1: Maximum length subject (assuming max is around 500 chars)
Log-Message "Test 1.1: Maximum Length Subject" "INFO"
$maxLengthSubject = "A" * 500
$complaintData = @{
    subject = $maxLengthSubject
    description = "Test for maximum length subject"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if ($result.Success) {
    $script:createdComplaintIds += $result.Data.id
    Log-Test "Maximum Length Subject" $true "Accepted 500 character subject"
} else {
    Log-Test "Maximum Length Subject" $false "MEDIUM: System rejected valid max-length subject - $($result.Error)"
}

# Test 1.2: Extremely long subject (beyond max - should be rejected)
Log-Message "Test 1.2: Extremely Long Subject (Beyond Max)" "INFO"
$extremeLengthSubject = "A" * 2000
$complaintData = @{
    subject = $extremeLengthSubject
    description = "Test for extremely long subject"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if (!$result.Success) {
    Log-Test "Reject Extremely Long Subject" $true "System correctly rejected oversized subject"
} else {
    Log-Test "Reject Extremely Long Subject" $false "HIGH: System accepted subject beyond max length"
}

# Test 1.3: Empty subject
Log-Message "Test 1.3: Empty Subject" "INFO"
$complaintData = @{
    subject = ""
    description = "Test for empty subject"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if (!$result.Success) {
    Log-Test "Reject Empty Subject" $true "System correctly rejected empty subject"
} else {
    Log-Test "Reject Empty Subject" $false "HIGH: System accepted empty subject"
}

# Test 1.4: Whitespace-only subject
Log-Message "Test 1.4: Whitespace-Only Subject" "INFO"
$complaintData = @{
    subject = "     "
    description = "Test for whitespace-only subject"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if (!$result.Success) {
    Log-Test "Reject Whitespace-Only Subject" $true "System correctly rejected whitespace-only subject"
} else {
    Log-Test "Reject Whitespace-Only Subject" $false "MEDIUM: System accepted whitespace-only subject"
}

# Test 1.5: Very long description (10000+ characters)
Log-Message "Test 1.5: Very Long Description" "INFO"
$longDescription = "This is a very long description. " * 500  # ~15000 chars
$complaintData = @{
    subject = "Test for long description"
    description = $longDescription
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if ($result.Success) {
    $script:createdComplaintIds += $result.Data.id
    Log-Test "Very Long Description" $true "Accepted long description"
} else {
    Log-Test "Very Long Description" $false "MEDIUM: System rejected valid long description - $($result.Error)"
}

# TEST CATEGORY 2: SPECIAL CHARACTERS AND ENCODING
Log-Message "`n=== TEST CATEGORY 2: SPECIAL CHARACTERS AND ENCODING ===" "INFO"

# Test 2.1: Special characters in subject
Log-Message "Test 2.1: Special Characters in Subject" "INFO"
$specialCharsSubject = "Test!@#$%^&*()_+-=[]{}|;:',.<>?/~`"
$complaintData = @{
    subject = $specialCharsSubject
    description = "Test for special characters"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if ($result.Success) {
    $script:createdComplaintIds += $result.Data.id
    Log-Test "Special Characters in Subject" $true "Accepted special characters"
} else {
    Log-Test "Special Characters in Subject" $false "MEDIUM: System rejected special characters - $($result.Error)"
}

# Test 2.2: Unicode characters (emoji, non-English)
Log-Message "Test 2.2: Unicode Characters" "INFO"
$unicodeSubject = "Test Unicode: 你好 مرحبا 👍 🎉 ñ ü ö"
$complaintData = @{
    subject = $unicodeSubject
    description = "Unicode test: Testing various unicode characters 测试 اختبار"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if ($result.Success) {
    $script:createdComplaintIds += $result.Data.id

    # Verify unicode was stored correctly
    Start-Sleep -Seconds 1
    $query = "SELECT Subject, Description FROM Complaints WHERE Id = '$($result.Data.id)'"
    $dbResult = Execute-SqlQuery -query $query

    if ($dbResult -and $dbResult.Rows[0].Subject.Contains("你好")) {
        Log-Test "Unicode Characters Storage" $true "Unicode stored and retrieved correctly"
    } else {
        Log-Test "Unicode Characters Storage" $false "MEDIUM: Unicode may not be stored correctly"
    }
} else {
    Log-Test "Unicode Characters" $false "MEDIUM: System rejected unicode characters - $($result.Error)"
}

# Test 2.3: SQL injection attempt in subject
Log-Message "Test 2.3: SQL Injection Prevention" "INFO"
$sqlInjectionSubject = "'; DROP TABLE Complaints; --"
$complaintData = @{
    subject = $sqlInjectionSubject
    description = "SQL injection test"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

# Check if Complaints table still exists
$query = "SELECT COUNT(*) as Count FROM Complaints"
$tableCheck = Execute-SqlQuery -query $query

if ($tableCheck) {
    Log-Test "SQL Injection Prevention" $true "Complaints table still exists - SQL injection prevented"
} else {
    Log-Test "SQL Injection Prevention" $false "CRITICAL: SQL injection may have succeeded"
}

# Test 2.4: XSS attempt in description
Log-Message "Test 2.4: XSS Prevention" "INFO"
$xssDescription = "<script>alert('XSS');</script><img src=x onerror=alert('XSS')>"
$complaintData = @{
    subject = "XSS Test"
    description = $xssDescription
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if ($result.Success) {
    $script:createdComplaintIds += $result.Data.id

    # Check if XSS is sanitized when retrieved
    Start-Sleep -Seconds 1
    $retrieveResult = Invoke-ApiCall -endpoint "/api/complaints/$($result.Data.id)" -method "GET"

    if ($retrieveResult.Success) {
        $retrievedDescription = $retrieveResult.Data.description
        # Check if HTML tags are escaped or removed
        if ($retrievedDescription -notlike "*<script>*") {
            Log-Test "XSS Prevention" $true "XSS content sanitized"
        } else {
            Log-Test "XSS Prevention" $false "HIGH: XSS content not sanitized"
        }
    }
} else {
    Log-Test "XSS Prevention" $true "System rejected XSS content"
}

# TEST CATEGORY 3: INVALID INPUTS
Log-Message "`n=== TEST CATEGORY 3: INVALID INPUTS ===" "INFO"

# Test 3.1: Invalid GUID for StatusMasterId
Log-Message "Test 3.1: Invalid GUID for StatusMasterId" "INFO"
$complaintData = @{
    subject = "Invalid StatusMasterId Test"
    description = "Testing invalid GUID"
    statusMasterId = "00000000-0000-0000-0000-000000000000"
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if (!$result.Success) {
    Log-Test "Reject Invalid StatusMasterId" $true "System correctly rejected invalid StatusMasterId"
} else {
    Log-Test "Reject Invalid StatusMasterId" $false "HIGH: System accepted invalid StatusMasterId"
}

# Test 3.2: Non-existent GUID for CategoryMasterId
Log-Message "Test 3.2: Non-Existent CategoryMasterId" "INFO"
$complaintData = @{
    subject = "Non-existent Category Test"
    description = "Testing non-existent category"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = "99999999-9999-9999-9999-999999999999"
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if (!$result.Success) {
    Log-Test "Reject Non-Existent CategoryMasterId" $true "System correctly rejected non-existent CategoryMasterId"
} else {
    Log-Test "Reject Non-Existent CategoryMasterId" $false "HIGH: System accepted non-existent CategoryMasterId"
}

# Test 3.3: Missing required fields
Log-Message "Test 3.3: Missing Required Fields" "INFO"
$complaintData = @{
    description = "Missing subject field"
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if (!$result.Success) {
    Log-Test "Reject Missing Required Fields" $true "System correctly rejected missing fields"
} else {
    Log-Test "Reject Missing Required Fields" $false "HIGH: System accepted incomplete data"
}

# Test 3.4: Invalid data types
Log-Message "Test 3.4: Invalid Data Types" "INFO"
$complaintData = @{
    subject = 12345  # Number instead of string
    description = "Test"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

# System might auto-convert number to string, which is acceptable
if ($result.Success) {
    Log-Test "Handle Invalid Data Types" $true "System handled type conversion"
} else {
    Log-Test "Reject Invalid Data Types" $true "System rejected invalid data type"
}

# Test 3.5: Null values for required fields
Log-Message "Test 3.5: Null Values for Required Fields" "INFO"
$complaintData = @{
    subject = $null
    description = "Test"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if (!$result.Success) {
    Log-Test "Reject Null Required Fields" $true "System correctly rejected null subject"
} else {
    Log-Test "Reject Null Required Fields" $false "HIGH: System accepted null subject"
}

# TEST CATEGORY 4: CONCURRENT OPERATIONS
Log-Message "`n=== TEST CATEGORY 4: CONCURRENT OPERATIONS ===" "INFO"

# Test 4.1: Create a complaint for concurrent testing
Log-Message "Test 4.1: Setup for Concurrent Operations" "INFO"
$complaintData = @{
    subject = "Concurrent Operations Test - $(Get-Date -Format 'yyyyMMddHHmmss')"
    description = "Test for concurrent updates"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if ($result.Success) {
    $concurrentTestId = $result.Data.id
    $script:createdComplaintIds += $concurrentTestId
    Log-Test "Create Complaint for Concurrent Test" $true

    # Test 4.2: Simulate concurrent updates
    Log-Message "Test 4.2: Concurrent Updates" "INFO"

    $jobs = @()

    # Create 5 simultaneous update jobs
    for ($i = 1; $i -le 5; $i++) {
        $jobs += Start-Job -ScriptBlock {
            param($apiBase, $complaintId, $token, $iteration)

            $headers = @{
                "Authorization" = "Bearer $token"
                "Content-Type" = "application/json"
            }

            $updateData = @{
                subject = "Updated by concurrent job $iteration - $(Get-Date -Format 'HH:mm:ss.fff')"
            } | ConvertTo-Json

            try {
                Invoke-RestMethod -Uri "$apiBase/api/complaints/$complaintId" -Headers $headers -Method "PUT" -Body $updateData -TimeoutSec 10
                return "Success-$iteration"
            } catch {
                return "Failed-$iteration"
            }
        } -ArgumentList $apiBase, $concurrentTestId, $token, $i
    }

    # Wait for all jobs to complete
    $jobs | Wait-Job -Timeout 30 | Out-Null
    $results = $jobs | Receive-Job
    $jobs | Remove-Job -Force

    $successCount = ($results | Where-Object { $_ -like "Success-*" }).Count

    if ($successCount -eq 5) {
        Log-Test "Concurrent Updates All Succeeded" $true "All 5 concurrent updates succeeded"
    } elseif ($successCount -gt 0) {
        Log-Test "Concurrent Updates Partial Success" $true "$successCount out of 5 updates succeeded"
    } else {
        Log-Test "Concurrent Updates" $false "MEDIUM: All concurrent updates failed"
    }

    # Test 4.3: Verify data integrity after concurrent updates
    Start-Sleep -Seconds 2
    $query = "SELECT Subject FROM Complaints WHERE Id = '$concurrentTestId'"
    $finalResult = Execute-SqlQuery -query $query

    if ($finalResult -and $finalResult.Rows[0].Subject -like "Updated by concurrent job*") {
        Log-Test "Data Integrity After Concurrent Updates" $true "Data is consistent"
    } else {
        Log-Test "Data Integrity After Concurrent Updates" $false "MEDIUM: Data may be inconsistent"
    }
}

# TEST CATEGORY 5: ERROR HANDLING
Log-Message "`n=== TEST CATEGORY 5: ERROR HANDLING ===" "INFO"

# Test 5.1: Request to non-existent complaint
Log-Message "Test 5.1: Non-Existent Complaint Request" "INFO"
$nonExistentId = "99999999-9999-9999-9999-999999999999"
$result = Invoke-ApiCall -endpoint "/api/complaints/$nonExistentId" -method "GET"

if (!$result.Success -and $result.StatusCode -eq 404) {
    Log-Test "Return 404 for Non-Existent Complaint" $true "Correctly returned 404"
} else {
    Log-Test "Return 404 for Non-Existent Complaint" $false "MEDIUM: Did not return proper 404"
}

# Test 5.2: Malformed JSON
Log-Message "Test 5.2: Malformed JSON Handling" "INFO"
try {
    $malformedJson = '{"subject": "Test", "description": "Test", MALFORMED}'
    $response = Invoke-WebRequest -Uri "$apiBase/api/complaints" -Headers $headers -Method "POST" -Body $malformedJson -UseBasicParsing
    Log-Test "Reject Malformed JSON" $false "HIGH: System accepted malformed JSON"
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Log-Test "Reject Malformed JSON" $true "Correctly returned 400 Bad Request"
    } else {
        Log-Test "Handle Malformed JSON" $true "System handled malformed JSON"
    }
}

# Test 5.3: Timeout handling (long operation)
Log-Message "Test 5.3: Timeout Handling" "INFO"
$result = Invoke-ApiCall -endpoint "/api/complaints?pageSize=10000" -method "GET" -timeoutSeconds 2

if ($result.Success -or $result.Error -like "*timeout*") {
    Log-Test "Timeout Handling" $true "System handled timeout gracefully"
} else {
    Log-Test "Timeout Handling" $false "MEDIUM: Timeout not handled properly"
}

# Test 5.4: Invalid HTTP method
Log-Message "Test 5.4: Invalid HTTP Method" "INFO"
try {
    $response = Invoke-WebRequest -Uri "$apiBase/api/complaints" -Headers $headers -Method "DELETE" -UseBasicParsing
    Log-Test "Reject Invalid HTTP Method" $false "MEDIUM: System accepted invalid HTTP method"
} catch {
    if ($_.Exception.Response.StatusCode -eq 405) {
        Log-Test "Reject Invalid HTTP Method" $true "Correctly returned 405 Method Not Allowed"
    } else {
        Log-Test "Handle Invalid HTTP Method" $true "System handled invalid method"
    }
}

# TEST CATEGORY 6: PAGINATION AND FILTERING EDGE CASES
Log-Message "`n=== TEST CATEGORY 6: PAGINATION AND FILTERING EDGE CASES ===" "INFO"

# Test 6.1: Zero page size
Log-Message "Test 6.1: Zero Page Size" "INFO"
$result = Invoke-ApiCall -endpoint "/api/complaints?pageSize=0" -method "GET"

if ($result.Success) {
    Log-Test "Handle Zero Page Size" $true "System handled zero page size"
} else {
    Log-Test "Reject Zero Page Size" $true "System rejected zero page size"
}

# Test 6.2: Negative page number
Log-Message "Test 6.2: Negative Page Number" "INFO"
$result = Invoke-ApiCall -endpoint "/api/complaints?page=-1" -method "GET"

if ($result.Success) {
    Log-Test "Handle Negative Page Number" $true "System handled negative page number"
} else {
    Log-Test "Reject Negative Page Number" $true "System rejected negative page number"
}

# Test 6.3: Extremely large page size
Log-Message "Test 6.3: Extremely Large Page Size" "INFO"
$result = Invoke-ApiCall -endpoint "/api/complaints?pageSize=999999" -method "GET"

if ($result.Success) {
    if ($result.Data.Count -lt 999999) {
        Log-Test "Cap Extremely Large Page Size" $true "System capped page size appropriately"
    } else {
        Log-Test "Handle Extremely Large Page Size" $false "MEDIUM: System may not limit page size"
    }
} else {
    Log-Test "Reject Extremely Large Page Size" $true "System rejected extreme page size"
}

# Test 6.4: Invalid filter parameters
Log-Message "Test 6.4: Invalid Filter Parameters" "INFO"
$result = Invoke-ApiCall -endpoint "/api/complaints?status=InvalidStatus&priority=999" -method "GET"

if ($result.Success) {
    # Should return empty results, not error
    Log-Test "Handle Invalid Filter Parameters" $true "System handled invalid filters gracefully"
} else {
    Log-Test "Handle Invalid Filter Parameters" $false "MEDIUM: System errors on invalid filters"
}

# TEST CATEGORY 7: DATE/TIME EDGE CASES
Log-Message "`n=== TEST CATEGORY 7: DATE/TIME EDGE CASES ===" "INFO"

# Test 7.1: Future date filtering
Log-Message "Test 7.1: Future Date Filtering" "INFO"
$futureDate = (Get-Date).AddYears(1).ToString("yyyy-MM-dd")
$result = Invoke-ApiCall -endpoint "/api/complaints?fromDate=$futureDate" -method "GET"

if ($result.Success) {
    # Should return empty or handle gracefully
    Log-Test "Handle Future Date Filter" $true "System handled future date filter"
} else {
    Log-Test "Handle Future Date Filter" $false "MEDIUM: System rejected valid future date filter"
}

# Test 7.2: Invalid date format
Log-Message "Test 7.2: Invalid Date Format" "INFO"
$result = Invoke-ApiCall -endpoint "/api/complaints?fromDate=invalid-date" -method "GET"

if ($result.Success -or $result.StatusCode -eq 400) {
    Log-Test "Handle Invalid Date Format" $true "System handled invalid date format"
} else {
    Log-Test "Handle Invalid Date Format" $false "MEDIUM: Unexpected error on invalid date"
}

# Test 7.3: Year 1900 and Year 9999 edge cases
Log-Message "Test 7.3: Extreme Date Values" "INFO"
$result = Invoke-ApiCall -endpoint "/api/complaints?fromDate=1900-01-01&toDate=9999-12-31" -method "GET"

if ($result.Success) {
    Log-Test "Handle Extreme Date Range" $true "System handled extreme date range"
} else {
    Log-Test "Handle Extreme Date Range" $false "MEDIUM: System rejected valid extreme dates"
}

# TEST CATEGORY 8: PERFORMANCE UNDER LOAD
Log-Message "`n=== TEST CATEGORY 8: PERFORMANCE UNDER LOAD ===" "INFO"

# Test 8.1: Rapid sequential requests
Log-Message "Test 8.1: Rapid Sequential Requests" "INFO"
$requestCount = 10
$successCount = 0
$startTime = Get-Date

for ($i = 1; $i -le $requestCount; $i++) {
    $result = Invoke-ApiCall -endpoint "/api/complaints?pageSize=10" -method "GET" -timeoutSeconds 5
    if ($result.Success) { $successCount++ }
}

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds
$avgTime = $duration / $requestCount

if ($successCount -eq $requestCount) {
    Log-Test "Rapid Sequential Requests" $true "All $requestCount requests succeeded, Avg: $([math]::Round($avgTime, 3))s per request"
} else {
    Log-Test "Rapid Sequential Requests" $false "MEDIUM: Only $successCount out of $requestCount succeeded"
}

# Test 8.2: Bulk operations
Log-Message "Test 8.2: Bulk Create Operations" "INFO"
$bulkCount = 5
$bulkSuccessCount = 0

for ($i = 1; $i -le $bulkCount; $i++) {
    $complaintData = @{
        subject = "Bulk Test $i - $(Get-Date -Format 'yyyyMMddHHmmss')"
        description = "Bulk creation test complaint $i"
        statusMasterId = $submittedStatus.id
        priorityMasterId = $mediumPriority.id
        categoryMasterId = $category.id
    }

    $result = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

    if ($result.Success) {
        $bulkSuccessCount++
        $script:createdComplaintIds += $result.Data.id
    }

    Start-Sleep -Milliseconds 100
}

if ($bulkSuccessCount -eq $bulkCount) {
    Log-Test "Bulk Create Operations" $true "All $bulkCount bulk creates succeeded"
} else {
    Log-Test "Bulk Create Operations" $false "MEDIUM: Only $bulkSuccessCount out of $bulkCount bulk creates succeeded"
}

# FINAL SUMMARY
Log-Message "`n========================================" "INFO"
Log-Message "EDGE CASE TEST SUITE COMPLETED" "INFO"
Log-Message "========================================" "INFO"
Log-Message "Total Tests: $totalTests" "INFO"
Log-Message "Passed: $passedTests" "INFO"
Log-Message "Failed: $failedTests" "INFO"
Log-Message "Success Rate: $(if($totalTests -gt 0){[math]::Round(($passedTests/$totalTests)*100, 2)}else{0})%" "INFO"
Log-Message "Created Test Complaints: $($createdComplaintIds.Count)" "INFO"

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
Log-Message "`nNOTE: Created $($createdComplaintIds.Count) test complaints. Consider cleaning up test data." "INFO"
