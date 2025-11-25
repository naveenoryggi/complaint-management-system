# Create fresh test complaints with correct complainant
$token = Get-Content ".complainant-token" -Raw
$token = $token.Trim()

Write-Host "Creating test complaints as complainant user..." -ForegroundColor Cyan
Write-Host ""

# Get required master data IDs
$categoriesUrl = "http://localhost:5000/api/categories"
$prioritiesUrl = "http://localhost:5000/api/master-data/priorities"
$statusesUrl = "http://localhost:5000/api/master-data/statuses"

Write-Host "Fetching master data..." -ForegroundColor Yellow
$categories = Invoke-RestMethod -Uri $categoriesUrl -Method GET -Headers @{ "Authorization" = "Bearer $token" }
$priorities = Invoke-RestMethod -Uri $prioritiesUrl -Method GET -Headers @{ "Authorization" = "Bearer $token" }

$category = $categories.data | Where-Object { $_.name -eq "IT Support" } | Select-Object -First 1
$priorityHigh = $priorities.data | Where-Object { $_.name -eq "High" } | Select-Object -First 1
$priorityNormal = $priorities.data | Where-Object { $_.name -eq "Normal" } | Select-Object -First 1

Write-Host "Category ID: $($category.id)"
Write-Host "High Priority ID: $($priorityHigh.id)"
Write-Host "Normal Priority ID: $($priorityNormal.id)"
Write-Host ""

# Create 5 test complaints
$complaints = @(
    @{
        title = "ROLE TEST - Cannot access HRMS portal"
        description = "Getting timeout errors when trying to log into the HRMS system. This is affecting my ability to submit leave requests."
        priorityId = $priorityHigh.id
    },
    @{
        title = "ROLE TEST - Payroll discrepancy in salary"
        description = "My salary for this month is less than expected. Please verify the payroll calculation."
        priorityId = $priorityHigh.id
    },
    @{
        title = "ROLE TEST - Office WiFi not working"
        description = "Unable to connect to office WiFi network. Need IT support to resolve this issue."
        priorityId = $priorityNormal.id
    },
    @{
        title = "ROLE TEST - Request for new laptop"
        description = "Current laptop is 5 years old and very slow. Requesting upgrade to newer model."
        priorityId = $priorityNormal.id
    },
    @{
        title = "ROLE TEST - Email not receiving attachments"
        description = "Email attachments are not downloading properly. Getting error message when trying to open attachments."
        priorityId = $priorityNormal.id
    }
)

$createdCount = 0
$failedCount = 0

foreach ($complaintData in $complaints) {
    Write-Host "Creating: $($complaintData.title)..." -ForegroundColor Yellow

    $body = @{
        title = $complaintData.title
        description = $complaintData.description
        categoryId = $category.id
        priorityMasterId = $complaintData.priorityId
        contactEmail = "nav_nainital@yahoo.com"
        contactPhone = "9876543210"
        preferredContactMethod = "Email"
        isAnonymous = $false
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" -Method POST -Body $body -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

        if ($response.isSuccess) {
            Write-Host "  SUCCESS: Created $($response.data.complaintNumber)" -ForegroundColor Green
            $createdCount++
        } else {
            Write-Host "  FAILED: $($response.message)" -ForegroundColor Red
            $failedCount++
        }
    } catch {
        Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $failedCount++
    }

    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "  Created: $createdCount complaints" -ForegroundColor Green
Write-Host "  Failed: $failedCount complaints" -ForegroundColor $(if ($failedCount -gt 0) { "Red" } else { "White" })
Write-Host ""

# Verify
Write-Host "Verifying..." -ForegroundColor Cyan
$verifyUrl = "http://localhost:5000/api/complaints?page=1" + "&" + "pageSize=20"
$verifyResponse = Invoke-RestMethod -Uri $verifyUrl -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

Write-Host "Total complaints now visible to complainant: $($verifyResponse.data.totalCount)" -ForegroundColor $(if ($verifyResponse.data.totalCount -le 10) { "Green" } else { if ($verifyResponse.data.totalCount -gt 1000) { "Red" } else { "Yellow" }})

if ($verifyResponse.data.totalCount -le 10) {
    Write-Host "SUCCESS: Role-based filtering is working!" -ForegroundColor Green
} else {
    Write-Host "PROBLEM: Still seeing too many complaints. Backend filtering may not be working." -ForegroundColor Red
}
