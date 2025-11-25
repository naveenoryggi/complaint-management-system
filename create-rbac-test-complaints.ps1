# RBAC Test - Create Test Complaints for Complainant User
# Creates 4 more complaints for nav_nainital@yahoo.com to test role-based filtering

$baseUrl = "http://localhost:5000/api"

# Login as complainant
$loginBody = @{
    email = "nav_nainital@yahoo.com"
    password = "Nav@12345"
} | ConvertTo-Json

Write-Host "Logging in as complainant..." -ForegroundColor Cyan
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.token
Write-Host "Login successful! Token: $($token.Substring(0,20))..." -ForegroundColor Green

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get complainant user ID
$userInfo = Invoke-RestMethod -Uri "$baseUrl/auth/me" -Headers $headers
$complainantId = $userInfo.id
Write-Host "Complainant ID: $complainantId" -ForegroundColor Yellow
Write-Host "Complainant Name: $($userInfo.fullName)" -ForegroundColor Yellow

# Get master data for categories and priorities
Write-Host "`nFetching master data..." -ForegroundColor Cyan
$categories = Invoke-RestMethod -Uri "$baseUrl/categories?companyId=fe2bb7e2-c432-4cb1-93ea-b8eaafd8fa93" -Headers $headers
$priorities = Invoke-RestMethod -Uri "$baseUrl/ComplaintPriorities" -Headers $headers
$statuses = Invoke-RestMethod -Uri "$baseUrl/ComplaintStatuses" -Headers $headers

Write-Host "Categories loaded: $($categories.Count)" -ForegroundColor Green
Write-Host "Priorities loaded: $($priorities.Count)" -ForegroundColor Green

# Find specific categories
$itCategory = ($categories | Where-Object { $_.name -like "*IT*Technical*" })[0]
$hrCategory = ($categories | Where-Object { $_.name -like "*Salary*" -or $_.name -like "*Payroll*" })[0]
$facilitiesCategory = ($categories | Where-Object { $_.name -like "*Facilities*" })[0]
$generalCategory = ($categories | Where-Object { $_.name -like "*General*" })[0]

# Find priorities
$highPriority = ($priorities | Where-Object { $_.name -eq "High" })[0]
$normalPriority = ($priorities | Where-Object { $_.name -eq "Normal" })[0]
$lowPriority = ($priorities | Where-Object { $_.name -eq "Low" })[0]

# Find submitted status
$submittedStatus = ($statuses | Where-Object { $_.name -eq "Submitted" })[0]

Write-Host "`nMaster Data IDs:" -ForegroundColor Cyan
Write-Host "IT Category: $($itCategory.id)" -ForegroundColor Yellow
Write-Host "HR Category: $($hrCategory.id)" -ForegroundColor Yellow
Write-Host "Facilities Category: $($facilitiesCategory.id)" -ForegroundColor Yellow
Write-Host "High Priority: $($highPriority.id)" -ForegroundColor Yellow
Write-Host "Normal Priority: $($normalPriority.id)" -ForegroundColor Yellow
Write-Host "Submitted Status: $($submittedStatus.id)" -ForegroundColor Yellow

# Define the 4 remaining complaints
$complaints = @(
    @{
        title = "Payroll discrepancy"
        description = "My salary for November is showing incorrect amount. Please verify and correct the payroll calculation."
        categoryId = $hrCategory.id
        priorityId = $highPriority.id
        statusId = $submittedStatus.id
    },
    @{
        title = "Office AC not working"
        description = "Air conditioning in Floor 3 has been broken for 2 days. Temperature is very uncomfortable for working."
        categoryId = $facilitiesCategory.id
        priorityId = $normalPriority.id
        statusId = $submittedStatus.id
    },
    @{
        title = "Printer issues"
        description = "Network printer on Floor 2 is not responding. Unable to print important documents."
        categoryId = $itCategory.id
        priorityId = $lowPriority.id
        statusId = $submittedStatus.id
    },
    @{
        title = "Parking pass request"
        description = "Need parking pass for visitor coming next week on November 15th. Please arrange."
        categoryId = $generalCategory.id
        priorityId = $normalPriority.id
        statusId = $submittedStatus.id
    }
)

$createdComplaints = @()

foreach ($complaint in $complaints) {
    Write-Host "`nCreating complaint: $($complaint.title)" -ForegroundColor Cyan

    $complaintBody = @{
        title = $complaint.title
        description = $complaint.description
        categoryId = $complaint.categoryId
        priorityId = $complaint.priorityId
        statusId = $complaint.statusId
        complainantId = $complainantId
        companyId = "fe2bb7e2-c432-4cb1-93ea-b8eaafd8fa93"
        contactEmail = "nav_nainital@yahoo.com"
        isAnonymous = $false
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method Post -Headers $headers -Body $complaintBody
        $createdComplaints += $response
        Write-Host "  Created: CMP-$($response.complaintNumber) (ID: $($response.id))" -ForegroundColor Green
    } catch {
        Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "  Response: $($_.Exception.Response)" -ForegroundColor Red
    }
}

Write-Host "`n==================================" -ForegroundColor Cyan
Write-Host "COMPLAINT CREATION SUMMARY" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Complainant: Nav Nainital (nav_nainital@yahoo.com)" -ForegroundColor Yellow
Write-Host "Total Complaints Created: $($createdComplaints.Count)" -ForegroundColor Green
Write-Host ""
Write-Host "Complaint Details:" -ForegroundColor Cyan
foreach ($c in $createdComplaints) {
    Write-Host "  - CMP-$($c.complaintNumber): $($c.title)" -ForegroundColor White
}
Write-Host "==================================" -ForegroundColor Cyan

# Save results
$results = @{
    complainantId = $complainantId
    complainantEmail = "nav_nainital@yahoo.com"
    complainantName = $userInfo.fullName
    totalCreated = $createdComplaints.Count
    complaints = $createdComplaints
    timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
} | ConvertTo-Json -Depth 10

$results | Out-File "rbac-test-complaints-created.json" -Encoding UTF8
Write-Host "`nResults saved to: rbac-test-complaints-created.json" -ForegroundColor Green
