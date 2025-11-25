# Setup Handler User for Edit Testing

$token = (Get-Content ".working-token" -Raw).Trim()
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$baseUrl = "http://localhost:5000/api"

Write-Host "=== STEP 1: Creating Handler User ===" -ForegroundColor Cyan

# Create handler user
$handlerUser = @{
    userName = "handler.test"
    email = "handler.test@complaintmanagement.com"
    password = "Handler@123"
    firstName = "Test"
    lastName = "Handler"
    phoneNumber = "1234567890"
    employeeCode = "HANDLER001"
    jobTitle = "Complaint Handler"
    isActive = $true
    roleName = "Handler"
} | ConvertTo-Json

try {
    $createdUser = Invoke-RestMethod -Uri "$baseUrl/users" -Method Post -Headers $headers -Body $handlerUser
    Write-Host "Handler user created successfully!" -ForegroundColor Green
    Write-Host "User ID: $($createdUser.id)"
    Write-Host "Username: $($createdUser.userName)"
    Write-Host "Email: $($createdUser.email)"

    $handlerUserId = $createdUser.id

    # Save handler credentials for later use
    @{
        email = "handler.test@complaintmanagement.com"
        password = "Handler@123"
        userId = $handlerUserId
    } | ConvertTo-Json | Out-File ".handler-credentials.json"

} catch {
    Write-Host "Error creating user: $($_.Exception.Message)" -ForegroundColor Red

    # User might already exist, try to find them
    Write-Host "Attempting to find existing handler user..." -ForegroundColor Yellow
    $users = Invoke-RestMethod -Uri "$baseUrl/users" -Headers $headers -Method Get
    $existingHandler = $users | Where-Object { $_.email -eq "handler.test@complaintmanagement.com" }

    if ($existingHandler) {
        Write-Host "Found existing handler user!" -ForegroundColor Green
        $handlerUserId = $existingHandler.id
        Write-Host "User ID: $handlerUserId"

        # Save handler credentials
        @{
            email = "handler.test@complaintmanagement.com"
            password = "Handler@123"
            userId = $handlerUserId
        } | ConvertTo-Json | Out-File ".handler-credentials.json"
    } else {
        Write-Host "Could not find or create handler user" -ForegroundColor Red
        exit 1
    }
}

Write-Host "`n=== STEP 2: Getting a Complaint to Assign ===" -ForegroundColor Cyan

# Get first unassigned complaint
$complaints = Invoke-RestMethod -Uri "$baseUrl/complaints?pageSize=50" -Headers $headers -Method Get
$unassignedComplaint = $complaints | Where-Object { $null -eq $_.assignedTechnicianId } | Select-Object -First 1

if ($unassignedComplaint) {
    Write-Host "Found unassigned complaint:" -ForegroundColor Green
    Write-Host "Complaint ID: $($unassignedComplaint.id)"
    Write-Host "Complaint Number: $($unassignedComplaint.complaintNumber)"
    Write-Host "Title: $($unassignedComplaint.title)"

    Write-Host "`n=== STEP 3: Assigning Complaint to Handler ===" -ForegroundColor Cyan

    # Assign complaint to handler
    $assignPayload = @{
        complaintId = $unassignedComplaint.id
        assignedTechnicianId = $handlerUserId
        notes = "Assigned for handler edit testing"
    } | ConvertTo-Json

    try {
        # Try to assign via PUT endpoint
        $assignUrl = "$baseUrl/complaints/$($unassignedComplaint.id)/assign"
        $result = Invoke-RestMethod -Uri $assignUrl -Method Put -Headers $headers -Body $assignPayload

        Write-Host "Complaint assigned successfully!" -ForegroundColor Green
        Write-Host "Assigned to: $($result.assignedTechnicianName)"

        # Save test data
        @{
            handlerEmail = "handler.test@complaintmanagement.com"
            handlerPassword = "Handler@123"
            handlerUserId = $handlerUserId
            assignedComplaintId = $unassignedComplaint.id
            complaintNumber = $unassignedComplaint.complaintNumber
            complaintTitle = $unassignedComplaint.title
        } | ConvertTo-Json | Out-File ".handler-test-data.json"

        Write-Host "`n=== TEST DATA SAVED ===" -ForegroundColor Green
        Write-Host "Handler Login: handler.test@complaintmanagement.com"
        Write-Host "Handler Password: Handler@123"
        Write-Host "Assigned Complaint: $($unassignedComplaint.complaintNumber)"
        Write-Host "`nTest data saved to .handler-test-data.json" -ForegroundColor Yellow

    } catch {
        Write-Host "Error assigning complaint: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red

        # Try alternative: Update complaint directly
        Write-Host "`nTrying direct complaint update..." -ForegroundColor Yellow

        try {
            $updatePayload = @{
                assignedTechnicianId = $handlerUserId
            } | ConvertTo-Json

            $updateResult = Invoke-RestMethod -Uri "$baseUrl/complaints/$($unassignedComplaint.id)" -Method Put -Headers $headers -Body $updatePayload
            Write-Host "Complaint updated successfully via PUT!" -ForegroundColor Green

            # Save test data
            @{
                handlerEmail = "handler.test@complaintmanagement.com"
                handlerPassword = "Handler@123"
                handlerUserId = $handlerUserId
                assignedComplaintId = $unassignedComplaint.id
                complaintNumber = $unassignedComplaint.complaintNumber
                complaintTitle = $unassignedComplaint.title
            } | ConvertTo-Json | Out-File ".handler-test-data.json"

            Write-Host "`n=== TEST DATA SAVED ===" -ForegroundColor Green
            Write-Host "Handler Login: handler.test@complaintmanagement.com"
            Write-Host "Handler Password: Handler@123"
            Write-Host "Assigned Complaint: $($unassignedComplaint.complaintNumber)"

        } catch {
            Write-Host "Direct update also failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

} else {
    Write-Host "No unassigned complaints found!" -ForegroundColor Red
    Write-Host "You may need to create a complaint first or unassign an existing one."
}

Write-Host "`n=== SETUP COMPLETE ===" -ForegroundColor Green
