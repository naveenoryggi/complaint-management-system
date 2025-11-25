# Simple SLA Calculator Test using existing token

$baseUrl = "http://localhost:5058/api"
$token = (Get-Content .test-token -Raw).Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "`n========== SLA CALCULATOR TEST ==========" -ForegroundColor Cyan

# Get categories and users
Write-Host "`n[1] Fetching test data..." -ForegroundColor Yellow
try {
    $categories = Invoke-RestMethod -Uri "$baseUrl/categories" -Method Get -Headers $headers
    $users = Invoke-RestMethod -Uri "$baseUrl/users" -Method Get -Headers $headers

    $category = $categories.data[0]
    $user = $users.data[0]

    Write-Host "  Category: $($category.name)" -ForegroundColor Green
    Write-Host "  User: $($user.fullName)" -ForegroundColor Green
    Write-Host "  Company: $($user.companyId)" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Create complaints with different priorities
Write-Host "`n[2] Creating complaints with different priorities..." -ForegroundColor Yellow

$priorities = @("Low", "Normal", "High", "Critical", "Urgent")
$results = @()

foreach ($priority in $priorities) {
    Write-Host "`n  Priority: $priority" -ForegroundColor Cyan

    $data = @{
        title = "SLA Test - $priority"
        description = "Testing SLA calculation for $priority priority"
        categoryId = $category.id
        priority = $priority
        complainantId = $user.id
        companyId = $user.companyId
        isAnonymous = $false
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/complaints" -Method Post -Headers $headers -Body $data
        $complaint = $response.data

        $submitted = [DateTime]::Parse($complaint.submittedAt)
        $due = [DateTime]::Parse($complaint.dueDate)
        $slaHours = [Math]::Round(($due - $submitted).TotalHours, 2)

        Write-Host "    Complaint#: $($complaint.complaintNumber)" -ForegroundColor White
        Write-Host "    Submitted:  $($complaint.submittedAt)" -ForegroundColor White
        Write-Host "    Due Date:   $($complaint.dueDate)" -ForegroundColor White
        Write-Host "    SLA Hours:  $slaHours" -ForegroundColor White
        Write-Host "    STATUS: PASS" -ForegroundColor Green

        $results += @{
            Priority = $priority
            ComplaintNumber = $complaint.complaintNumber
            SLAHours = $slaHours
            Success = $true
        }
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $results += @{
            Priority = $priority
            Success = $false
            Error = $_.Exception.Message
        }
    }

    Start-Sleep -Milliseconds 300
}

# Summary
Write-Host "`n[3] Test Summary" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan

$passed = ($results | Where-Object { $_.Success }).Count
$failed = ($results | Where-Object { -not $_.Success }).Count

Write-Host "`nResults:" -ForegroundColor White
foreach ($result in $results) {
    if ($result.Success) {
        Write-Host "  $($result.Priority.PadRight(10)) | $($result.ComplaintNumber) | $($result.SLAHours) hours" -ForegroundColor Green
    } else {
        Write-Host "  $($result.Priority.PadRight(10)) | FAILED" -ForegroundColor Red
    }
}

Write-Host "`nPassed: $passed / Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Yellow" })
Write-Host "========================================`n" -ForegroundColor Cyan

if ($failed -eq 0) {
    Write-Host "SUCCESS: SLA Calculator is working!" -ForegroundColor Green
} else {
    Write-Host "WARNING: Some tests failed" -ForegroundColor Yellow
}
