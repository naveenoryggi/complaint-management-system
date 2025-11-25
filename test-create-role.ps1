$TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlVwZGF0ZWQgQWRtaW4iLCJFbXBsb3llZUNvZGUiOiJBRE1JTjAwMSIsIkNvbXBhbnlJZCI6ImZlMjhjZDg1LTQyMjYtNGRhYS05ZTQ1LTY2YTNkNTE4NzdmYSIsIlBlcm1pc3Npb24iOlsiVmlld0NvbXBsYWludHMiLCJBZGRDb21tZW50IiwiRXNjYWxhdGVDb21wbGFpbnQiLCJNYW5hZ2VVc2VycyIsIlZpZXdBdWRpdExvZ3MiLCJFZGl0Q29tcGxhaW50IiwiQ3JlYXRlQ29tcGxhaW50IiwiVmlld0NvbW1lbnRzIiwiTWFuYWdlUm9sZXMiLCJNYW5hZ2VFc2NhbGF0aW9uIiwiVmlld0F0dGFjaG1lbnRzIiwiVmlld0VzY2FsYXRpb24iLCJBc3NpZ25Db21wbGFpbnQiLCJWaWV3UmVwb3J0cyIsIkRlbGV0ZUNvbXBsYWludCIsIkFkZEF0dGFjaG1lbnQiLCJNYW5hZ2VDYXRlZ29yaWVzIiwiQ2xvc2VDb21wbGFpbnQiLCJNYW5hZ2VTZXR0aW5ncyIsIk1hbmFnZUNvbXBhbnkiLCJSZW9wZW5Db21wbGFpbnQiXSwiZXhwIjoxNzYxNDkwNDMxLCJpc3MiOiJDb21wbGFpbnRNYW5hZ2VtZW50U3lzdGVtIiwiYXVkIjoiQ29tcGxhaW50TWFuYWdlbWVudEFQSSJ9.R76auz-LIBRiuQ1U1NMUDrW2Fu315txFbbBcP0g4pmA"

$body = @{
    name = "Test Role 12345"
    code = "TEST_ROLE_12345"
    description = "Test role"
    roleType = 15
    escalationLevel = 3
    permissions = @()
} | ConvertTo-Json

Write-Host "Testing Create Role endpoint..."
Write-Host "Body: $body"

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5058/api/roles" -Method POST -Headers @{
        "Authorization" = "Bearer $TOKEN"
        "Content-Type" = "application/json"
    } -Body $body -UseBasicParsing

    Write-Host "SUCCESS!"
    Write-Host "Status: $($response.StatusCode)"
    Write-Host "Response: $($response.Content)"
} catch {
    Write-Host "FAILED!"
    Write-Host "Error Status: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "Error Message: $($_.Exception.Message)"

    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    $reader.BaseStream.Position = 0
    $errorResponse = $reader.ReadToEnd()
    Write-Host "Error Response Body:"
    Write-Host $errorResponse
}
