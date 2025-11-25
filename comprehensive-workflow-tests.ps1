# Comprehensive Business Workflow Tests for Complaint Management System
# Purpose: Test complete business workflows and permission validation

$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logFile = "WORKFLOW_TEST_RESULTS_$timestamp.txt"

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
        [object]$body = $null
    )
    try {
        $params = @{
            Uri = "$apiBase$endpoint"
            Headers = $headers
            Method = $method
        }

        if ($body) {
            $params.Body = ($body | ConvertTo-Json -Depth 10)
        }

        $response = Invoke-RestMethod @params
        return @{ Success = $true; Data = $response }
    } catch {
        $errorDetails = $_.Exception.Message
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errorDetails = $reader.ReadToEnd()
        }
        return @{ Success = $false; Error = $errorDetails }
    }
}

function Get-MasterData {
    Log-Message "Fetching master data..." "INFO"

    # Get Status Master
    $statusResult = Invoke-ApiCall -endpoint "/api/statusmaster" -method "GET"
    $script:statusMaster = if ($statusResult.Success) { $statusResult.Data } else { @() }

    # Get Priority Master
    $priorityResult = Invoke-ApiCall -endpoint "/api/prioritymaster" -method "GET"
    $script:priorityMaster = if ($priorityResult.Success) { $priorityResult.Data } else { @() }

    # Get Category Master
    $categoryResult = Invoke-ApiCall -endpoint "/api/categorymaster" -method "GET"
    $script:categoryMaster = if ($categoryResult.Success) { $categoryResult.Data } else { @() }

    # Get Users
    $usersResult = Invoke-ApiCall -endpoint "/api/users" -method "GET"
    $script:users = if ($usersResult.Success) { $usersResult.Data } else { @() }

    Log-Message "Master data fetched: Status=$($statusMaster.Count), Priority=$($priorityMaster.Count), Category=$($categoryMaster.Count), Users=$($users.Count)" "INFO"
}

Log-Message "========================================" "INFO"
Log-Message "BUSINESS WORKFLOW TEST SUITE STARTED" "INFO"
Log-Message "========================================" "INFO"

# Fetch master data
Get-MasterData

# TEST CATEGORY 1: COMPLETE LIFECYCLE WORKFLOW
Log-Message "`n=== TEST CATEGORY 1: COMPLETE LIFECYCLE WORKFLOW (Create → Assign → Resolve → Close) ===" "INFO"

# Test 1.1: Create a new complaint
Log-Message "Test 1.1: Create New Complaint" "INFO"
$submittedStatus = $statusMaster | Where-Object { $_.name -eq "Submitted" } | Select-Object -First 1
$mediumPriority = $priorityMaster | Where-Object { $_.name -eq "Medium" } | Select-Object -First 1
$category = $categoryMaster | Select-Object -First 1

$complaintData = @{
    subject = "Workflow Test - Complete Lifecycle - $(Get-Date -Format 'yyyyMMddHHmmss')"
    description = "This is a test complaint for complete lifecycle workflow testing"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$createResult = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if ($createResult.Success) {
    $complaintId = $createResult.Data.id
    $script:createdComplaintIds += $complaintId
    Log-Test "Create New Complaint" $true "Created complaint ID: $complaintId"

    # Verify in database
    Start-Sleep -Seconds 1
    $query = "SELECT * FROM Complaints WHERE Id = '$complaintId'"
    $dbResult = Execute-SqlQuery -query $query

    if ($dbResult -and $dbResult.Rows.Count -gt 0) {
        Log-Test "Complaint Exists in Database" $true
        $complaintRow = $dbResult.Rows[0]

        # Test 1.2: Verify initial status is Submitted
        if ($complaintRow.Status -eq "Submitted") {
            Log-Test "Initial Status is Submitted" $true
        } else {
            Log-Test "Initial Status is Submitted" $false "MEDIUM: Status is '$($complaintRow.Status)' instead of 'Submitted'"
        }

        # Test 1.3: Assign the complaint
        Log-Message "Test 1.3: Assign Complaint" "INFO"
        $assignUser = $users | Where-Object { $_.id -ne $null } | Select-Object -First 1

        if ($assignUser) {
            $assignData = @{
                assignedTo = $assignUser.id
            }

            $assignResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/assign" -method "PUT" -body $assignData

            if ($assignResult.Success) {
                Log-Test "Assign Complaint" $true "Assigned to user: $($assignUser.fullName)"

                # Verify assignment in database
                Start-Sleep -Seconds 1
                $query = "SELECT AssignedTo FROM Complaints WHERE Id = '$complaintId'"
                $dbResult = Execute-SqlQuery -query $query

                if ($dbResult -and $dbResult.Rows[0].AssignedTo -eq $assignUser.id) {
                    Log-Test "Assignment Persisted in Database" $true
                } else {
                    Log-Test "Assignment Persisted in Database" $false "HIGH: AssignedTo not updated in database"
                }

                # Test 1.4: Update status to InProgress
                Log-Message "Test 1.4: Update Status to InProgress" "INFO"
                $inProgressStatus = $statusMaster | Where-Object { $_.name -eq "InProgress" } | Select-Object -First 1

                $updateData = @{
                    statusMasterId = $inProgressStatus.id
                }

                $updateResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/status" -method "PUT" -body $updateData

                if ($updateResult.Success) {
                    Log-Test "Update Status to InProgress" $true

                    # Test 1.5: Verify status history was created
                    Start-Sleep -Seconds 1
                    $query = "SELECT COUNT(*) as Count FROM ComplaintStatusHistory WHERE ComplaintId = '$complaintId'"
                    $historyResult = Execute-SqlQuery -query $query

                    if ($historyResult -and $historyResult.Rows[0].Count -gt 0) {
                        Log-Test "Status History Created" $true "History count: $($historyResult.Rows[0].Count)"
                    } else {
                        Log-Test "Status History Created" $false "HIGH: No status history found"
                    }

                    # Test 1.6: Add a comment
                    Log-Message "Test 1.6: Add Comment" "INFO"
                    $commentData = @{
                        comment = "This is a test comment for workflow testing"
                    }

                    $commentResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/comments" -method "POST" -body $commentData

                    if ($commentResult.Success) {
                        Log-Test "Add Comment" $true

                        # Verify comment in database
                        Start-Sleep -Seconds 1
                        $query = "SELECT COUNT(*) as Count FROM ComplaintComments WHERE ComplaintId = '$complaintId'"
                        $commentDbResult = Execute-SqlQuery -query $query

                        if ($commentDbResult -and $commentDbResult.Rows[0].Count -gt 0) {
                            Log-Test "Comment Persisted in Database" $true
                        } else {
                            Log-Test "Comment Persisted in Database" $false "HIGH: Comment not found in database"
                        }
                    } else {
                        Log-Test "Add Comment" $false "MEDIUM: $($commentResult.Error)"
                    }

                    # Test 1.7: Resolve the complaint
                    Log-Message "Test 1.7: Resolve Complaint" "INFO"
                    $resolvedStatus = $statusMaster | Where-Object { $_.name -eq "Resolved" } | Select-Object -First 1

                    $resolveData = @{
                        statusMasterId = $resolvedStatus.id
                        resolutionNotes = "Complaint resolved through workflow testing"
                    }

                    $resolveResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/resolve" -method "PUT" -body $resolveData

                    if ($resolveResult.Success) {
                        Log-Test "Resolve Complaint" $true

                        # Verify resolved status and resolution notes
                        Start-Sleep -Seconds 1
                        $query = "SELECT Status, ResolutionNotes FROM Complaints WHERE Id = '$complaintId'"
                        $resolveDbResult = Execute-SqlQuery -query $query

                        if ($resolveDbResult) {
                            $row = $resolveDbResult.Rows[0]
                            if ($row.Status -eq "Resolved" -and $row.ResolutionNotes -ne "") {
                                Log-Test "Resolution Data Persisted" $true
                            } else {
                                Log-Test "Resolution Data Persisted" $false "HIGH: Status='$($row.Status)', ResolutionNotes='$($row.ResolutionNotes)'"
                            }
                        }

                        # Test 1.8: Close the complaint
                        Log-Message "Test 1.8: Close Complaint" "INFO"
                        $closedStatus = $statusMaster | Where-Object { $_.name -eq "Closed" } | Select-Object -First 1

                        $closeData = @{
                            statusMasterId = $closedStatus.id
                        }

                        $closeResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/close" -method "PUT" -body $closeData

                        if ($closeResult.Success) {
                            Log-Test "Close Complaint" $true

                            # Verify closed status
                            Start-Sleep -Seconds 1
                            $query = "SELECT Status, ClosedDate FROM Complaints WHERE Id = '$complaintId'"
                            $closeDbResult = Execute-SqlQuery -query $query

                            if ($closeDbResult) {
                                $row = $closeDbResult.Rows[0]
                                if ($row.Status -eq "Closed" -and $row.ClosedDate -ne [DBNull]::Value) {
                                    Log-Test "Closure Data Persisted" $true
                                } else {
                                    Log-Test "Closure Data Persisted" $false "HIGH: Status='$($row.Status)', ClosedDate='$($row.ClosedDate)'"
                                }
                            }
                        } else {
                            Log-Test "Close Complaint" $false "HIGH: $($closeResult.Error)"
                        }
                    } else {
                        Log-Test "Resolve Complaint" $false "HIGH: $($resolveResult.Error)"
                    }
                } else {
                    Log-Test "Update Status to InProgress" $false "HIGH: $($updateResult.Error)"
                }
            } else {
                Log-Test "Assign Complaint" $false "HIGH: $($assignResult.Error)"
            }
        } else {
            Log-Test "Assign Complaint" $false "CRITICAL: No users available for assignment"
        }
    } else {
        Log-Test "Complaint Exists in Database" $false "CRITICAL: Complaint not found in database"
    }
} else {
    Log-Test "Create New Complaint" $false "CRITICAL: $($createResult.Error)"
}

# TEST CATEGORY 2: ESCALATION WORKFLOW
Log-Message "`n=== TEST CATEGORY 2: ESCALATION WORKFLOW (Create → Escalate → Resolve) ===" "INFO"

# Test 2.1: Create a new complaint for escalation
Log-Message "Test 2.1: Create Complaint for Escalation" "INFO"
$complaintData = @{
    subject = "Workflow Test - Escalation - $(Get-Date -Format 'yyyyMMddHHmmss')"
    description = "This is a test complaint for escalation workflow testing"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$createResult = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if ($createResult.Success) {
    $complaintId = $createResult.Data.id
    $script:createdComplaintIds += $complaintId
    Log-Test "Create Complaint for Escalation" $true "Created complaint ID: $complaintId"

    # Test 2.2: Escalate the complaint
    Log-Message "Test 2.2: Escalate Complaint" "INFO"
    $escalatedStatus = $statusMaster | Where-Object { $_.name -eq "Escalated" } | Select-Object -First 1
    $escalateToUser = $users | Where-Object { $_.id -ne $null } | Select-Object -First 1

    $escalationData = @{
        reason = "Test escalation for workflow validation"
        escalatedTo = $escalateToUser.id
        statusMasterId = $escalatedStatus.id
    }

    $escalateResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/escalate" -method "POST" -body $escalationData

    if ($escalateResult.Success) {
        Log-Test "Escalate Complaint" $true

        # Test 2.3: Verify escalation in database
        Start-Sleep -Seconds 1
        $query = "SELECT * FROM ComplaintEscalations WHERE ComplaintId = '$complaintId' AND IsActive = 1"
        $escalationDbResult = Execute-SqlQuery -query $query

        if ($escalationDbResult -and $escalationDbResult.Rows.Count -gt 0) {
            Log-Test "Escalation Record Created" $true

            $escalationRow = $escalationDbResult.Rows[0]
            if ($escalationRow.Reason -eq "Test escalation for workflow validation") {
                Log-Test "Escalation Reason Persisted" $true
            } else {
                Log-Test "Escalation Reason Persisted" $false "MEDIUM: Reason mismatch"
            }

            # Test 2.4: Verify complaint status changed to Escalated
            $query = "SELECT Status FROM Complaints WHERE Id = '$complaintId'"
            $statusDbResult = Execute-SqlQuery -query $query

            if ($statusDbResult -and $statusDbResult.Rows[0].Status -eq "Escalated") {
                Log-Test "Status Changed to Escalated" $true
            } else {
                Log-Test "Status Changed to Escalated" $false "HIGH: Status is '$($statusDbResult.Rows[0].Status)'"
            }

            # Test 2.5: Resolve escalated complaint
            Log-Message "Test 2.5: Resolve Escalated Complaint" "INFO"
            $resolvedStatus = $statusMaster | Where-Object { $_.name -eq "Resolved" } | Select-Object -First 1

            $resolveData = @{
                statusMasterId = $resolvedStatus.id
                resolutionNotes = "Escalated complaint resolved"
            }

            $resolveResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/resolve" -method "PUT" -body $resolveData

            if ($resolveResult.Success) {
                Log-Test "Resolve Escalated Complaint" $true

                # Verify escalation is still active but complaint is resolved
                Start-Sleep -Seconds 1
                $query = @"
                SELECT c.Status, ce.IsActive
                FROM Complaints c
                LEFT JOIN ComplaintEscalations ce ON c.Id = ce.ComplaintId AND ce.IsActive = 1
                WHERE c.Id = '$complaintId'
"@
                $verifyResult = Execute-SqlQuery -query $query

                if ($verifyResult -and $verifyResult.Rows[0].Status -eq "Resolved") {
                    Log-Test "Escalated Complaint Status Updated" $true
                } else {
                    Log-Test "Escalated Complaint Status Updated" $false "HIGH: Status not updated"
                }
            } else {
                Log-Test "Resolve Escalated Complaint" $false "HIGH: $($resolveResult.Error)"
            }
        } else {
            Log-Test "Escalation Record Created" $false "CRITICAL: No escalation record found in database"
        }
    } else {
        Log-Test "Escalate Complaint" $false "HIGH: $($escalateResult.Error)"
    }
} else {
    Log-Test "Create Complaint for Escalation" $false "CRITICAL: $($createResult.Error)"
}

# TEST CATEGORY 3: REOPEN WORKFLOW
Log-Message "`n=== TEST CATEGORY 3: REOPEN WORKFLOW (Close → Reopen → Resolve) ===" "INFO"

# Test 3.1: Create and close a complaint
Log-Message "Test 3.1: Create and Close Complaint for Reopen Test" "INFO"
$complaintData = @{
    subject = "Workflow Test - Reopen - $(Get-Date -Format 'yyyyMMddHHmmss')"
    description = "This is a test complaint for reopen workflow testing"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$createResult = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if ($createResult.Success) {
    $complaintId = $createResult.Data.id
    $script:createdComplaintIds += $complaintId
    Log-Test "Create Complaint for Reopen Test" $true "Created complaint ID: $complaintId"

    # Resolve first
    $resolvedStatus = $statusMaster | Where-Object { $_.name -eq "Resolved" } | Select-Object -First 1
    $resolveData = @{
        statusMasterId = $resolvedStatus.id
        resolutionNotes = "Initial resolution for reopen test"
    }

    Start-Sleep -Seconds 1
    $resolveResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/resolve" -method "PUT" -body $resolveData

    if ($resolveResult.Success) {
        # Close the complaint
        $closedStatus = $statusMaster | Where-Object { $_.name -eq "Closed" } | Select-Object -First 1
        $closeData = @{
            statusMasterId = $closedStatus.id
        }

        Start-Sleep -Seconds 1
        $closeResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/close" -method "PUT" -body $closeData

        if ($closeResult.Success) {
            Log-Test "Close Complaint for Reopen Test" $true

            # Test 3.2: Reopen the complaint
            Log-Message "Test 3.2: Reopen Complaint" "INFO"
            $reopenedStatus = $statusMaster | Where-Object { $_.name -eq "Reopened" } | Select-Object -First 1

            $reopenData = @{
                statusMasterId = $reopenedStatus.id
                reason = "Reopening for additional testing"
            }

            Start-Sleep -Seconds 1
            $reopenResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/reopen" -method "PUT" -body $reopenData

            if ($reopenResult.Success) {
                Log-Test "Reopen Complaint" $true

                # Test 3.3: Verify complaint is reopened in database
                Start-Sleep -Seconds 1
                $query = "SELECT Status, ReopenedDate FROM Complaints WHERE Id = '$complaintId'"
                $reopenDbResult = Execute-SqlQuery -query $query

                if ($reopenDbResult) {
                    $row = $reopenDbResult.Rows[0]
                    if ($row.Status -eq "Reopened" -and $row.ReopenedDate -ne [DBNull]::Value) {
                        Log-Test "Reopen Data Persisted" $true
                    } else {
                        Log-Test "Reopen Data Persisted" $false "HIGH: Status='$($row.Status)', ReopenedDate='$($row.ReopenedDate)'"
                    }
                }

                # Test 3.4: Resolve reopened complaint
                Log-Message "Test 3.4: Resolve Reopened Complaint" "INFO"
                $resolveData = @{
                    statusMasterId = $resolvedStatus.id
                    resolutionNotes = "Resolved after reopening"
                }

                Start-Sleep -Seconds 1
                $resolveResult2 = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/resolve" -method "PUT" -body $resolveData

                if ($resolveResult2.Success) {
                    Log-Test "Resolve Reopened Complaint" $true

                    # Verify resolution
                    Start-Sleep -Seconds 1
                    $query = "SELECT Status FROM Complaints WHERE Id = '$complaintId'"
                    $finalResult = Execute-SqlQuery -query $query

                    if ($finalResult -and $finalResult.Rows[0].Status -eq "Resolved") {
                        Log-Test "Reopened Complaint Final Status" $true
                    } else {
                        Log-Test "Reopened Complaint Final Status" $false "HIGH: Final status is '$($finalResult.Rows[0].Status)'"
                    }
                } else {
                    Log-Test "Resolve Reopened Complaint" $false "HIGH: $($resolveResult2.Error)"
                }
            } else {
                Log-Test "Reopen Complaint" $false "HIGH: $($reopenResult.Error)"
            }
        } else {
            Log-Test "Close Complaint for Reopen Test" $false "HIGH: $($closeResult.Error)"
        }
    }
} else {
    Log-Test "Create Complaint for Reopen Test" $false "CRITICAL: $($createResult.Error)"
}

# TEST CATEGORY 4: PERMISSION VALIDATION
Log-Message "`n=== TEST CATEGORY 4: PERMISSION VALIDATION ===" "INFO"

# Test 4.1: Verify permission-based endpoints (with valid token)
Log-Message "Test 4.1: Access Protected Endpoints with Valid Token" "INFO"
$protectedEndpoints = @(
    @{ Endpoint = "/api/complaints"; Method = "GET"; Permission = "ViewComplaints" }
    @{ Endpoint = "/api/users"; Method = "GET"; Permission = "ManageUsers" }
    @{ Endpoint = "/api/roles"; Method = "GET"; Permission = "ManageRoles" }
    @{ Endpoint = "/api/auditlogs"; Method = "GET"; Permission = "ViewAuditLogs" }
)

foreach ($endpoint in $protectedEndpoints) {
    $result = Invoke-ApiCall -endpoint $endpoint.Endpoint -method $endpoint.Method

    if ($result.Success) {
        Log-Test "Access $($endpoint.Permission) Endpoint" $true "$($endpoint.Endpoint)"
    } else {
        Log-Test "Access $($endpoint.Permission) Endpoint" $false "HIGH: Cannot access $($endpoint.Endpoint) - $($result.Error)"
    }
}

# Test 4.2: Verify endpoints without token (should fail)
Log-Message "Test 4.2: Access Protected Endpoints Without Token" "INFO"
$noAuthHeaders = @{
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri "$apiBase/api/complaints" -Headers $noAuthHeaders -Method Get
    Log-Test "Reject Unauthenticated Request" $false "CRITICAL: Endpoint accessible without authentication"
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Log-Test "Reject Unauthenticated Request" $true "401 Unauthorized returned"
    } else {
        Log-Test "Reject Unauthenticated Request" $false "MEDIUM: Unexpected status code: $($_.Exception.Response.StatusCode)"
    }
}

# TEST CATEGORY 5: AUDIT TRAIL VALIDATION
Log-Message "`n=== TEST CATEGORY 5: AUDIT TRAIL VALIDATION ===" "INFO"

if ($createdComplaintIds.Count -gt 0) {
    $testComplaintId = $createdComplaintIds[0]

    # Test 5.1: Verify audit logs were created for complaint operations
    Log-Message "Test 5.1: Audit Logs for Complaint Operations" "INFO"
    $query = @"
    SELECT COUNT(*) as Count
    FROM AuditLogs
    WHERE EntityType = 'Complaint' AND EntityId = '$testComplaintId'
"@
    $auditResult = Execute-SqlQuery -query $query

    if ($auditResult -and $auditResult.Rows[0].Count -gt 0) {
        Log-Test "Audit Logs Created" $true "Found $($auditResult.Rows[0].Count) audit log entries"
    } else {
        Log-Test "Audit Logs Created" $false "HIGH: No audit logs found for complaint operations"
    }

    # Test 5.2: Verify audit log details
    Log-Message "Test 5.2: Audit Log Details Validation" "INFO"
    $query = @"
    SELECT Action, UserId, Timestamp, Details
    FROM AuditLogs
    WHERE EntityType = 'Complaint' AND EntityId = '$testComplaintId'
    ORDER BY Timestamp
"@
    $auditDetails = Execute-SqlQuery -query $query

    if ($auditDetails) {
        $hasCreateAction = $false
        $hasUpdateAction = $false
        $allHaveUserId = $true
        $allHaveTimestamp = $true

        foreach ($row in $auditDetails.Rows) {
            if ($row.Action -like "*Create*") { $hasCreateAction = $true }
            if ($row.Action -like "*Update*") { $hasUpdateAction = $true }
            if ([string]::IsNullOrEmpty($row.UserId)) { $allHaveUserId = $false }
            if ($row.Timestamp -eq [DBNull]::Value) { $allHaveTimestamp = $false }
        }

        if ($hasCreateAction) {
            Log-Test "Audit Log Contains Create Action" $true
        } else {
            Log-Test "Audit Log Contains Create Action" $false "MEDIUM: No create action found"
        }

        if ($allHaveUserId) {
            Log-Test "All Audit Logs Have UserId" $true
        } else {
            Log-Test "All Audit Logs Have UserId" $false "HIGH: Some audit logs missing UserId"
        }

        if ($allHaveTimestamp) {
            Log-Test "All Audit Logs Have Timestamp" $true
        } else {
            Log-Test "All Audit Logs Have Timestamp" $false "HIGH: Some audit logs missing Timestamp"
        }
    }
}

# TEST CATEGORY 6: WORKFLOW STATE VALIDATION
Log-Message "`n=== TEST CATEGORY 6: WORKFLOW STATE VALIDATION ===" "INFO"

# Test 6.1: Verify invalid state transitions are prevented
Log-Message "Test 6.1: Invalid State Transition Prevention" "INFO"

# Create a new complaint in Submitted status
$complaintData = @{
    subject = "Workflow Test - State Validation - $(Get-Date -Format 'yyyyMMddHHmmss')"
    description = "Test for invalid state transitions"
    statusMasterId = $submittedStatus.id
    priorityMasterId = $mediumPriority.id
    categoryMasterId = $category.id
}

$createResult = Invoke-ApiCall -endpoint "/api/complaints" -method "POST" -body $complaintData

if ($createResult.Success) {
    $complaintId = $createResult.Data.id
    $script:createdComplaintIds += $complaintId

    # Try to close a complaint that hasn't been resolved (invalid transition)
    $closedStatus = $statusMaster | Where-Object { $_.name -eq "Closed" } | Select-Object -First 1
    $closeData = @{
        statusMasterId = $closedStatus.id
    }

    Start-Sleep -Seconds 1
    $closeResult = Invoke-ApiCall -endpoint "/api/complaints/$complaintId/close" -method "PUT" -body $closeData

    # This should fail or the system should handle it gracefully
    if (!$closeResult.Success) {
        Log-Test "Prevent Invalid State Transition (Submitted→Closed)" $true "System correctly prevented invalid transition"
    } else {
        # Check if complaint is actually closed
        $query = "SELECT Status FROM Complaints WHERE Id = '$complaintId'"
        $statusResult = Execute-SqlQuery -query $query

        if ($statusResult -and $statusResult.Rows[0].Status -eq "Closed") {
            Log-Test "Prevent Invalid State Transition (Submitted→Closed)" $false "MEDIUM: System allowed invalid state transition"
        } else {
            Log-Test "Prevent Invalid State Transition (Submitted→Closed)" $true "System handled transition gracefully"
        }
    }
}

# FINAL SUMMARY
Log-Message "`n========================================" "INFO"
Log-Message "BUSINESS WORKFLOW TEST SUITE COMPLETED" "INFO"
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
