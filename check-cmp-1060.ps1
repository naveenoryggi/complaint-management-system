$token = Get-Content '.test-token' -Raw
$headers = @{ 'Authorization' = "Bearer $token" }

try {
    $complaints = Invoke-RestMethod -Uri 'http://localhost:5058/api/Complaints' -Headers $headers -Method Get

    Write-Host "Total complaints in database: $($complaints.Count)"

    $complaint1060 = $complaints | Where-Object { $_.complaintNumber -eq 'CMP-2025-1060' }
    if ($complaint1060) {
        Write-Host "CMP-2025-1060 EXISTS with ID: $($complaint1060.id)"
    } else {
        Write-Host "CMP-2025-1060 does NOT exist"
    }

    # Find the max complaint number
    $maxComplaint = $complaints |
        Where-Object { $_.complaintNumber -match 'CMP-2025-(\d+)' } |
        ForEach-Object {
            if ($_.complaintNumber -match 'CMP-2025-(\d+)') {
                [PSCustomObject]@{ Number = [int]$matches[1]; ComplaintNumber = $_.complaintNumber }
            }
        } |
        Sort-Object Number -Descending |
        Select-Object -First 1

    if ($maxComplaint) {
        Write-Host "Max complaint number: $($maxComplaint.ComplaintNumber) ($($maxComplaint.Number))"
        Write-Host "Next should be: CMP-2025-$(($maxComplaint.Number + 1).ToString('D4'))"
    }
} catch {
    Write-Host "Error: $_"
}
