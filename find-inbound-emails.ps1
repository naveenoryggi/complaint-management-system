# Find complaints with inbound emails
$token = Get-Content ".fresh-token" -ErrorAction SilentlyContinue
if (-not $token) {
    $token = Get-Content ".admin-token" -ErrorAction SilentlyContinue
}

if (-not $token) {
    Write-Host "No token found"
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get first 20 complaints
$complaintsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints?page=1&pageSize=20" -Headers $headers -Method GET

Write-Host "Checking $($complaintsResponse.data.items.Count) complaints for inbound emails..." -ForegroundColor Cyan

$complaintsWithInbound = @()

foreach ($complaint in $complaintsResponse.data.items) {
    try {
        $emailsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$($complaint.id)/emails" -Headers $headers -Method GET

        if ($emailsResponse.isSuccess -and $emailsResponse.data) {
            $inboundCount = ($emailsResponse.data | Where-Object { -not $_.isOutbound }).Count
            if ($inboundCount -gt 0) {
                $obj = [PSCustomObject]@{
                    ComplaintNumber = $complaint.complaintNumber
                    ComplaintId = $complaint.id
                    Title = $complaint.title.Substring(0, [Math]::Min(50, $complaint.title.Length))
                    TotalEmails = $emailsResponse.data.Count
                    InboundEmails = $inboundCount
                    OutboundEmails = ($emailsResponse.data | Where-Object { $_.isOutbound }).Count
                }
                $complaintsWithInbound += $obj
                Write-Host "FOUND: $($complaint.complaintNumber) - $inboundCount inbound emails" -ForegroundColor Green
            }
        }
    } catch {
        # Skip if no emails
    }
}

Write-Host "`nSummary:" -ForegroundColor Yellow
Write-Host "Total complaints checked: $($complaintsResponse.data.items.Count)"
Write-Host "Complaints with inbound emails: $($complaintsWithInbound.Count)"

if ($complaintsWithInbound.Count -gt 0) {
    Write-Host "`nComplaintswith inbound emails:" -ForegroundColor Green
    $complaintsWithInbound | Format-Table -AutoSize

    Write-Host "`nFirst complaint with inbound email:"
    Write-Host "Complaint Number: $($complaintsWithInbound[0].ComplaintNumber)"
    Write-Host "Complaint ID: $($complaintsWithInbound[0].ComplaintId)"
    Write-Host "URL: http://localhost:4200/complaints/$($complaintsWithInbound[0].ComplaintId)"
} else {
    Write-Host "No complaints with inbound emails found in the first 20 complaints" -ForegroundColor Red
}
