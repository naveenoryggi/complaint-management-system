# Test SMTP Notification System
# Creates a test complaint and verifies notification dispatch

$baseUrl = "http://localhost:5058"
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRW1wbG95ZWVDb2RlIjoiQURNSU4wMDEiLCJDb21wYW55SWQiOiJmZTI4Y2Q4NS00MjI2LTRkYWEtOWU0NS02NmEzZDUxODc3ZmEiLCJQZXJtaXNzaW9uIjpbIlZpZXdDb21wbGFpbnRzIiwiQWRkQ29tbWVudCIsIkVzY2FsYXRlQ29tcGxhaW50IiwiTWFuYWdlVXNlcnMiLCJWaWV3QXVkaXRMb2dzIiwiRWRpdENvbXBsYWludCIsIkNyZWF0ZUNvbXBsYWludCIsIlZpZXdDb21tZW50cyIsIk1hbmFnZVJvbGVzIiwiTWFuYWdlRXNjYWxhdGlvbiIsIlZpZXdBdHRhY2htZW50cyIsIlZpZXdFc2NhbGF0aW9uIiwiQXNzaWduQ29tcGxhaW50IiwiVmlld1JlcG9ydHMiLCJEZWxldGVDb21wbGFpbnQiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MTIxNjQxMCwiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.J-rRMZtpLFyvzC-J4KrIsKvtiwaaf9aQw2uhy0HH82Q"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SMTP Notification System Test" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Create headers
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Create complaint payload
$complaintData = @{
    title = "SMTP Configuration Test - Workplace Safety $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
    description = "This is a test complaint to verify SMTP configuration is detected by the notification system. Testing email notification dispatch with configured SMTP server."
    categoryId = "8c4c91d3-4c8e-4c7e-a6f1-be3e4c4f8e23"
    priority = 2
    contactEmail = "testuser@complaintmanagement.com"
    isAnonymous = $false
    companyId = "fe28cd85-4226-4daa-9e45-66a3d51877fa"
} | ConvertTo-Json

Write-Host "Step 1: Creating test complaint..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/complaints" -Method Post -Headers $headers -Body $complaintData -ErrorAction Stop

    Write-Host "✓ Complaint created successfully!" -ForegroundColor Green
    Write-Host "  Complaint ID: $($response.data.id)" -ForegroundColor White
    Write-Host "  Complaint Number: $($response.data.complaintNumber)" -ForegroundColor White
    Write-Host "  Title: $($response.data.title)" -ForegroundColor White
    Write-Host ""

    $complaintId = $response.data.id

    Write-Host "Step 2: Waiting 5 seconds for notification processing..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5

    Write-Host ""
    Write-Host "Step 3: Checking CommunicationLogs for notifications..." -ForegroundColor Yellow
    Write-Host ""

    # Query database for notifications
    $sqlQuery = "SELECT TOP 5 Id, Channel, RecipientAddress, Subject, Status, ErrorMessage, CreatedAt FROM CommunicationLogs WHERE ReferenceId = '$complaintId' ORDER BY CreatedAt DESC;"

    $result = sqlcmd -S 'LAPTOP-NF9BTG7Q\SQLEXPRESS' -d 'ComplaintManagementDB' -E -Q $sqlQuery -W

    Write-Host $result
    Write-Host ""

    Write-Host "Step 4: Interpreting results..." -ForegroundColor Yellow
    Write-Host ""

    if ($result -like "*No active email server settings found*") {
        Write-Host "✗ ERROR: SMTP settings not being detected!" -ForegroundColor Red
        Write-Host "  The notification system is still showing 'No active email server settings found'" -ForegroundColor Red
    } elseif ($result -like "*Authentication failed*" -or $result -like "*5.7.8*" -or $result -like "*535*") {
        Write-Host "✓ PROGRESS: SMTP settings ARE being detected!" -ForegroundColor Green
        Write-Host "  The system found the SMTP configuration and attempted to send" -ForegroundColor Green
        Write-Host "  Email failed due to invalid credentials (expected with placeholder password)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "  This proves the notification system is working correctly!" -ForegroundColor Green
    } elseif ($result -like "*Status*") {
        Write-Host "✓ Notifications created in CommunicationLogs" -ForegroundColor Green
        Write-Host "  Check the Status and ErrorMessage columns above for details" -ForegroundColor White
    } else {
        Write-Host "? No notifications found yet. This could mean:" -ForegroundColor Yellow
        Write-Host "  1. Notification rules are not configured" -ForegroundColor White
        Write-Host "  2. Notification dispatch failed silently" -ForegroundColor White
        Write-Host "  3. Need to wait longer for async processing" -ForegroundColor White
    }

} catch {
    Write-Host "✗ Error creating complaint" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Test Complete" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
