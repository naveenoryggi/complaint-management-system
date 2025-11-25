# Email Buttons Issue - Quick Diagnosis Script
# Tests both endpoints and checks database format

$complaintId = "e9dc50f7-493c-4e13-a5a0-dc42085d4fca"
$baseUrl = "http://localhost:5000/api"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EMAIL BUTTONS ISSUE - DIAGNOSIS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Get fresh token
Write-Host "[1] Getting auth token..." -ForegroundColor Yellow
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "  ✓ Login successful" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Step 2: Test EmailThreadController endpoint
Write-Host ""
Write-Host "[2] Testing EmailThreadController: /api/complaints/{id}/emails" -ForegroundColor Yellow
$endpoint1 = "$baseUrl/complaints/$complaintId/emails"

try {
    $response1 = Invoke-RestMethod -Uri $endpoint1 -Method Get -Headers $headers
    Write-Host "  ✓ SUCCESS - Got response" -ForegroundColor Green
    Write-Host "    Email count: $($response1.data.Count)" -ForegroundColor Gray

    if ($response1.data.Count -gt 0) {
        $email1 = $response1.data[0]
        Write-Host "    First email:" -ForegroundColor Gray
        Write-Host "      - id: $($email1.id)" -ForegroundColor DarkGray
        Write-Host "      - subject: $($email1.subject)" -ForegroundColor DarkGray
        Write-Host "      - isOutbound: $($email1.isOutbound)" -ForegroundColor $(if ($null -ne $email1.isOutbound) { "Green" } else { "Red" })
        Write-Host "      - direction: $($email1.direction)" -ForegroundColor $(if ($null -eq $email1.direction) { "Green" } else { "Red" })
        Write-Host "      - toRecipients count: $($email1.toRecipients.Count)" -ForegroundColor DarkGray
        Write-Host "      - ccRecipients count: $($email1.ccRecipients.Count)" -ForegroundColor DarkGray

        # Save full response
        $response1 | ConvertTo-Json -Depth 10 | Out-File "endpoint1-response.json"
        Write-Host "    Full response saved to: endpoint1-response.json" -ForegroundColor Cyan
    }
} catch {
    Write-Host "  ✗ FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "    Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    }
}

# Step 3: Test EmailTicketingController endpoint
Write-Host ""
Write-Host "[3] Testing EmailTicketingController: /api/email-ticketing/complaint/{id}/emails" -ForegroundColor Yellow
$endpoint2 = "$baseUrl/email-ticketing/complaint/$complaintId/emails"

try {
    $response2 = Invoke-RestMethod -Uri $endpoint2 -Method Get -Headers $headers
    Write-Host "  ✓ SUCCESS - Got response" -ForegroundColor Green
    Write-Host "    Email count: $($response2.data.Count)" -ForegroundColor Gray

    if ($response2.data.Count -gt 0) {
        $email2 = $response2.data[0]
        Write-Host "    First email:" -ForegroundColor Gray
        Write-Host "      - id: $($email2.id)" -ForegroundColor DarkGray
        Write-Host "      - subject: $($email2.subject)" -ForegroundColor DarkGray
        Write-Host "      - isOutbound: $($email2.isOutbound)" -ForegroundColor $(if ($null -ne $email2.isOutbound) { "Green" } else { "Red" })
        Write-Host "      - direction: $($email2.direction)" -ForegroundColor $(if ($null -eq $email2.direction) { "Green" } else { "Red" })
        Write-Host "      - toRecipients count: $($email2.toRecipients.Count)" -ForegroundColor DarkGray
        Write-Host "      - ccRecipients count: $($email2.ccRecipients.Count)" -ForegroundColor DarkGray

        # Save full response
        $response2 | ConvertTo-Json -Depth 10 | Out-File "endpoint2-response.json"
        Write-Host "    Full response saved to: endpoint2-response.json" -ForegroundColor Cyan
    }
} catch {
    Write-Host "  ✗ FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "    Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    }
}

# Step 4: Check database
Write-Host ""
Write-Host "[4] Checking database email format..." -ForegroundColor Yellow
Write-Host "  Please run this SQL query manually:" -ForegroundColor Gray
Write-Host ""
Write-Host "  SELECT TOP 1" -ForegroundColor DarkGray
Write-Host "    Id, MessageId, Subject, Direction," -ForegroundColor DarkGray
Write-Host "    ToEmail, ToName, ToRecipientsJson," -ForegroundColor DarkGray
Write-Host "    CcEmails, CcRecipientsJson," -ForegroundColor DarkGray
Write-Host "    FromEmail, FromName," -ForegroundColor DarkGray
Write-Host "    IsInternal, ReceivedAt" -ForegroundColor DarkGray
Write-Host "  FROM EmailMessages" -ForegroundColor DarkGray
Write-Host "  WHERE ComplaintId = '$complaintId'" -ForegroundColor DarkGray
Write-Host "  ORDER BY ReceivedAt DESC" -ForegroundColor DarkGray
Write-Host ""

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DIAGNOSIS SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Expected Results:" -ForegroundColor Yellow
Write-Host "  ✓ Both endpoints should return isOutbound: true/false" -ForegroundColor Gray
Write-Host "  ✓ Both endpoints should NOT have direction property" -ForegroundColor Gray
Write-Host "  ✓ toRecipients and ccRecipients should be arrays with objects" -ForegroundColor Gray
Write-Host ""
Write-Host "If buttons are still missing, check:" -ForegroundColor Yellow
Write-Host "  1. Which endpoint is the frontend actually calling?" -ForegroundColor Gray
Write-Host "  2. Are recipients arrays empty?" -ForegroundColor Gray
Write-Host "  3. Is showActions prop set to true in component?" -ForegroundColor Gray
Write-Host "  4. Is email expanded? (buttons only show when collapsed)" -ForegroundColor Gray
Write-Host ""
Write-Host "Check the JSON files created for full API responses." -ForegroundColor Cyan
Write-Host ""
