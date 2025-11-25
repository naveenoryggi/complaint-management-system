# Fix test complaint ownership
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

# Correct complainant ID for nav_nainital@yahoo.com
$correctComplainantId = "fd0073b8-fc95-4a49-867c-6ffb38b7d177"

# Handler ID for naveen.chandra@oryggitech.com
$handlerId = "94c91ae3-72ef-4b53-8057-08de0e0582b5"

Write-Host "Fixing test complaint ownership..." -ForegroundColor Cyan
Write-Host ""

# Get all complaints
$allUrl = "http://localhost:5000/api/complaints?page=1" + "&" + "pageSize=1093"
$allResponse = Invoke-RestMethod -Uri $allUrl -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

# Find the test complaints (CMP-2025-1130 through CMP-2025-1139)
$testComplaints = $allResponse.data.items | Where-Object {
    $_.complaintNumber -match "CMP-2025-11(3[0-9])"
} | Sort-Object complaintNumber

Write-Host "Found $($testComplaints.Count) test complaints" -ForegroundColor Yellow
Write-Host ""

if ($testComplaints.Count -eq 0) {
    Write-Host "No test complaints found matching pattern CMP-2025-11**" -ForegroundColor Red
    Write-Host "Showing first 10 complaints:"
    $allResponse.data.items | Select-Object -First 10 | ForEach-Object {
        Write-Host "  - $($_.complaintNumber): $($_.title) (Complainant: $($_.complainantName))"
    }
    exit 1
}

# Display current ownership
Write-Host "Current ownership:" -ForegroundColor White
$testComplaints | ForEach-Object {
    Write-Host "  - $($_.complaintNumber): Complainant=$($_.complainantName) (ID: $($_.complainantId))"
}
Write-Host ""

# Fix each complaint
$fixedCount = 0
$failedCount = 0

foreach ($complaint in $testComplaints) {
    Write-Host "Fixing $($complaint.complaintNumber)..." -ForegroundColor Yellow

    try {
        # Get full complaint details
        $detailUrl = "http://localhost:5000/api/complaints/$($complaint.id)"
        $detailResponse = Invoke-RestMethod -Uri $detailUrl -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

        $complaintData = $detailResponse.data

        # Update the complainant ID
        $updateData = @{
            title = $complaintData.title
            description = $complaintData.description
            categoryId = $complaintData.categoryId
            complainantId = $correctComplainantId
            companyId = $complaintData.companyId
            branchId = $complaintData.branchId
            departmentId = $complaintData.departmentId
            sectionId = $complaintData.sectionId
            contactEmail = $complaintData.contactEmail
            contactPhone = $complaintData.contactPhone
            alternatePhone = $complaintData.alternatePhone
            preferredContactMethod = $complaintData.preferredContactMethod
            priorityMasterId = $complaintData.priorityId
            isAnonymous = $complaintData.isAnonymous
            tags = $complaintData.tags
        } | ConvertTo-Json -Depth 10

        # Update complaint
        $updateUrl = "http://localhost:5000/api/complaints/$($complaint.id)"
        $updateResponse = Invoke-RestMethod -Uri $updateUrl -Method PUT -Body $updateData -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"

        if ($updateResponse.isSuccess) {
            Write-Host "  SUCCESS: Updated complainant to Nav Nainital" -ForegroundColor Green
            $fixedCount++

            # Also assign to handler if not already assigned
            if ([string]::IsNullOrEmpty($complaint.assignedToId)) {
                $assignUrl = "http://localhost:5000/api/complaints/$($complaint.id)/assign/$handlerId"
                $assignResponse = Invoke-RestMethod -Uri $assignUrl -Method POST -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"
                if ($assignResponse.isSuccess) {
                    Write-Host "  SUCCESS: Assigned to handler Naveen Chandra" -ForegroundColor Green
                }
            }
        } else {
            Write-Host "  FAILED: $($updateResponse.message)" -ForegroundColor Red
            $failedCount++
        }
    } catch {
        Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $failedCount++
    }

    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "  Fixed: $fixedCount complaints" -ForegroundColor Green
Write-Host "  Failed: $failedCount complaints" -ForegroundColor $(if ($failedCount -gt 0) { "Red" } else { "White" })
Write-Host ""

# Verify the fix
Write-Host "Verifying fix..." -ForegroundColor Cyan
$verifyUrl = "http://localhost:5000/api/complaints?page=1" + "&" + "pageSize=50" + "&" + "complainantId=$correctComplainantId"
$verifyResponse = Invoke-RestMethod -Uri $verifyUrl -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"
Write-Host "Complaints now owned by Nav Nainital: $($verifyResponse.data.totalCount)" -ForegroundColor $(if ($verifyResponse.data.totalCount -gt 0) { "Green" } else { "Red" })

if ($verifyResponse.data.totalCount -gt 0) {
    Write-Host "Owned complaint numbers:"
    $verifyResponse.data.items | ForEach-Object {
        Write-Host "  - $($_.complaintNumber): $($_.title)"
    }
}
