# Simple SMTP Notification Test
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRW1wbG95ZWVDb2RlIjoiQURNSU4wMDEiLCJDb21wYW55SWQiOiJmZTI4Y2Q4NS00MjI2LTRkYWEtOWU0NS02NmEzZDUxODc3ZmEiLCJQZXJtaXNzaW9uIjpbIlZpZXdDb21wbGFpbnRzIiwiQWRkQ29tbWVudCIsIkVzY2FsYXRlQ29tcGxhaW50IiwiTWFuYWdlVXNlcnMiLCJWaWV3QXVkaXRMb2dzIiwiRWRpdENvbXBsYWludCIsIkNyZWF0ZUNvbXBsYWludCIsIlZpZXdDb21tZW50cyIsIk1hbmFnZVJvbGVzIiwiTWFuYWdlRXNjYWxhdGlvbiIsIlZpZXdBdHRhY2htZW50cyIsIlZpZXdFc2NhbGF0aW9uIiwiQXNzaWduQ29tcGxhaW50IiwiVmlld1JlcG9ydHMiLCJEZWxldGVDb21wbGFpbnQiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MTIxNjQxMCwiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.J-rRMZtpLFyvzC-J4KrIsKvtiwaaf9aQw2uhy0HH82Q"

Write-Host "Creating test complaint..." -ForegroundColor Cyan

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    title = "SMTP Test - $(Get-Date -Format 'HH:mm:ss')"
    description = "Testing SMTP notification system with configured server"
    categoryId = "7EC22A28-F757-4133-8152-22400FC4627A"
    priority = 2
    contactEmail = "testuser@complaintmanagement.com"
    isAnonymous = $false
    companyId = "fe28cd85-4226-4daa-9e45-66a3d51877fa"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5058/api/complaints" -Method Post -Headers $headers -Body $body

    Write-Host "Complaint created: $($response.data.complaintNumber)" -ForegroundColor Green
    $complaintId = $response.data.id

    Write-Host "Waiting 3 seconds..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3

    Write-Host "`nChecking notifications..." -ForegroundColor Cyan

    $query = "SELECT TOP 5 Channel, RecipientAddress, Subject, Status, ErrorMessage FROM CommunicationLogs WHERE ReferenceId = '$complaintId' ORDER BY CreatedAt DESC"
    $result = sqlcmd -S 'LAPTOP-NF9BTG7Q\SQLEXPRESS' -d 'ComplaintManagementDB' -E -Q $query -W

    Write-Host $result -ForegroundColor White

} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
