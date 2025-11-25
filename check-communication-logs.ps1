# Check Communication Logs for the test complaint

$token = Get-Content ".working-token" -Raw
$complaintId = "50c975e9-3f7a-4250-9df5-74f631586b1b"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Checking Communication Logs" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Complaint ID: $complaintId" -ForegroundColor White

try {
    # Try to get communication logs if endpoint exists
    $logsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/communication-logs?complaintId=$complaintId" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json" `
        -ErrorAction Stop

    if ($logsResponse.isSuccess) {
        Write-Host "`nFound $($logsResponse.data.Count) communication log(s)" -ForegroundColor Green

        foreach ($log in $logsResponse.data) {
            Write-Host "`nLog Entry:" -ForegroundColor Cyan
            Write-Host "  Type: $($log.type)" -ForegroundColor White
            Write-Host "  To: $($log.recipientEmail)" -ForegroundColor White
            Write-Host "  Subject: $($log.subject)" -ForegroundColor White
            Write-Host "  Status: $($log.status)" -ForegroundColor $(if ($log.status -eq "Sent") { "Green" } else { "Red" })
            Write-Host "  Sent At: $($log.sentAt)" -ForegroundColor White
            if ($log.errorMessage) {
                Write-Host "  Error: $($log.errorMessage)" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "`nNo communication logs found" -ForegroundColor Yellow
    }
} catch {
    Write-Host "`nCommunication logs endpoint not available: $_" -ForegroundColor Yellow

    # Try alternative endpoint
    Write-Host "`nTrying alternative API path..." -ForegroundColor Yellow
    try {
        $logsResponse2 = Invoke-RestMethod -Uri "http://localhost:5000/api/communications?complaintId=$complaintId" `
            -Method GET `
            -Headers @{Authorization="Bearer $token"} `
            -ContentType "application/json"

        Write-Host "Found logs at alternative endpoint" -ForegroundColor Green
        $logsResponse2 | ConvertTo-Json -Depth 5
    } catch {
        Write-Host "Alternative endpoint also not available: $_" -ForegroundColor Red

        # Check if NotificationDispatcher is registered
        Write-Host "`nDiagnostic: Let's verify the system configuration..." -ForegroundColor Yellow
        Write-Host "  1. Complaint was created successfully: YES" -ForegroundColor Green
        Write-Host "  2. CreateComplaintCommandHandler calls NotificationDispatcher: YES (line 118)" -ForegroundColor Green
        Write-Host "  3. Event type COMPLAINT_CREATED exists: YES (verified earlier)" -ForegroundColor Green
        Write-Host "  4. Notification rules exist for COMPLAINT_CREATED: YES (Rules 10-14)" -ForegroundColor Green
        Write-Host "`nPossible Issues:" -ForegroundColor Yellow
        Write-Host "  - Exception being swallowed in CreateComplaintCommandHandler (line 141-145)" -ForegroundColor Red
        Write-Host "  - NotificationDispatcher may not be finding matching rules" -ForegroundColor Yellow
        Write-Host "  - Communication logs table may not exist or endpoint not implemented" -ForegroundColor Yellow
    }
}

Write-Host "`n================================================" -ForegroundColor Cyan
