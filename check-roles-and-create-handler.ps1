$token = (Get-Content ".working-token" -Raw).Trim()
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "=== Checking Available Roles ===" -ForegroundColor Cyan
try {
    $roles = Invoke-RestMethod -Uri "http://localhost:5000/api/roles" -Headers $headers -Method Get
    Write-Host "Available Roles:" -ForegroundColor Green
    $roles | Select-Object id, name, description | Format-Table -AutoSize

    # Find Handler or Technician role
    $handlerRole = $roles | Where-Object { $_.name -eq "Handler" -or $_.name -eq "Technician" -or $_.name -eq "Support Staff" } | Select-Object -First 1

    if ($handlerRole) {
        Write-Host "`nFound suitable role: $($handlerRole.name) (ID: $($handlerRole.id))" -ForegroundColor Green
        $roleId = $handlerRole.id
    } else {
        Write-Host "`nNo Handler/Technician role found. Using first available role." -ForegroundColor Yellow
        $roleId = $roles[0].id
        Write-Host "Using role: $($roles[0].name) (ID: $roleId)"
    }

    # Save role ID
    $roleId | Out-File ".handler-role-id.txt"

} catch {
    Write-Host "Error getting roles: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Checking for Existing Handler User ===" -ForegroundColor Cyan
$users = Invoke-RestMethod -Uri "http://localhost:5000/api/users" -Headers $headers -Method Get
$existingHandler = $users | Where-Object { $_.email -like "*handler*" -or $_.userName -like "*handler*" } | Select-Object -First 1

if ($existingHandler) {
    Write-Host "Found existing handler-like user:" -ForegroundColor Green
    Write-Host "ID: $($existingHandler.id)"
    Write-Host "Username: $($existingHandler.userName)"
    Write-Host "Email: $($existingHandler.email)"
    Write-Host "Role: $($existingHandler.roleName)"

    # Use this user
    $handlerUserId = $existingHandler.id
    $handlerEmail = $existingHandler.email

    # Try to determine password (use default)
    $handlerPassword = "Handler@123"

    @{
        email = $handlerEmail
        password = $handlerPassword
        userId = $handlerUserId
    } | ConvertTo-Json | Out-File ".handler-credentials.json"

} else {
    Write-Host "No existing handler user found. Will use admin for testing." -ForegroundColor Yellow

    # Get admin user
    $adminUser = $users | Where-Object { $_.email -eq "admin@complaintmanagement.com" } | Select-Object -First 1

    if ($adminUser) {
        Write-Host "Using admin user for testing" -ForegroundColor Yellow
        $handlerUserId = $adminUser.id
        $handlerEmail = $adminUser.email
        $handlerPassword = "Admin@123"

        @{
            email = $handlerEmail
            password = $handlerPassword
            userId = $handlerUserId
            note = "Using admin because no handler exists"
        } | ConvertTo-Json | Out-File ".handler-credentials.json"
    }
}

Write-Host "`n=== Assigning Complaint ===" -ForegroundColor Cyan
$complaints = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints?pageSize=50" -Headers $headers -Method Get
$targetComplaint = $complaints | Select-Object -First 1

if ($targetComplaint) {
    Write-Host "Selected complaint for testing:" -ForegroundColor Green
    Write-Host "ID: $($targetComplaint.id)"
    Write-Host "Number: $($targetComplaint.complaintNumber)"
    Write-Host "Title: $($targetComplaint.title)"
    Write-Host "Current assigned to: $($targetComplaint.assignedTechnicianName)"

    # Save test data
    @{
        handlerEmail = $handlerEmail
        handlerPassword = $handlerPassword
        handlerUserId = $handlerUserId
        complaintId = $targetComplaint.id
        complaintNumber = $targetComplaint.complaintNumber
        complaintTitle = $targetComplaint.title
    } | ConvertTo-Json | Out-File ".handler-test-data.json"

    Write-Host "`n=== TEST DATA READY ===" -ForegroundColor Green
    Write-Host "Login Email: $handlerEmail"
    Write-Host "Password: $handlerPassword"
    Write-Host "Test Complaint: $($targetComplaint.complaintNumber)"
    Write-Host "Test data saved to .handler-test-data.json"
}
