$token = Get-Content '.test-token' -Raw
$headers = @{ 'Authorization' = "Bearer $token" }

try {
    Write-Host "=== Checking for test data from previous runs ==="
    Write-Host ""

    # Check for complaints with CMP-2025-1060
    Write-Host "1. Checking complaints..."
    $complaints = Invoke-RestMethod -Uri 'http://localhost:5058/api/Complaints' -Headers $headers -Method Get
    if ($complaints.data) {
        $allComplaints = $complaints.data
    } else {
        $allComplaints = $complaints
    }

    $complaint1060 = $allComplaints | Where-Object { $_.complaintNumber -eq 'CMP-2025-1060' }
    if ($complaint1060) {
        Write-Host "   FOUND CMP-2025-1060: ID = $($complaint1060.id)"
        Write-Host "   ComplainantId: $($complaint1060.complainantId)"
        Write-Host "   ContactEmail: $($complaint1060.contactEmail)"
    } else {
        Write-Host "   CMP-2025-1060 NOT FOUND in complaints table"
    }

    Write-Host ""
    Write-Host "2. Checking users with test emails..."
    $users = Invoke-RestMethod -Uri 'http://localhost:5058/api/users' -Headers $headers -Method Get
    if ($users.data) {
        $allUsers = $users.data
    } else {
        $allUsers = $users
    }

    $testUsers = $allUsers | Where-Object { $_.email -like 'testuser*@test.com' }
    if ($testUsers) {
        Write-Host "   FOUND $($testUsers.Count) test users:"
        $testUsers | ForEach-Object {
            Write-Host "   - $($_.email) (ID: $($_.id))"
        }
    } else {
        Write-Host "   No test users found"
    }

    Write-Host ""
    Write-Host "3. Max complaint number:"
    $maxComplaint = $allComplaints |
        Where-Object { $_.complaintNumber -match 'CMP-2025-(\d+)' } |
        ForEach-Object {
            if ($_.complaintNumber -match 'CMP-2025-(\d+)') {
                [PSCustomObject]@{ Number = [int]$matches[1]; ComplaintNumber = $_.complaintNumber }
            }
        } |
        Sort-Object Number -Descending |
        Select-Object -First 1

    if ($maxComplaint) {
        Write-Host "   Current max: $($maxComplaint.ComplaintNumber)"
        Write-Host "   Next should be: CMP-2025-$(($maxComplaint.Number + 1).ToString('D4'))"
    } else {
        Write-Host "   No complaints found, next should be: CMP-2025-0001"
    }

} catch {
    Write-Host "Error: $_"
    Write-Host $_.Exception.Message
}
