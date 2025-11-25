# Comprehensive Email Buttons Issue Investigation
# Tests the actual API response and frontend behavior

$baseUrl = "http://localhost:5000/api"
$frontendUrl = "http://localhost:4200"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "EMAIL BUTTONS INVESTIGATION" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login to get fresh token
Write-Host "[STEP 1] Logging in to get fresh auth token..." -ForegroundColor Yellow
$loginUrl = "$baseUrl/auth/login"
$loginBody = @{
    employeeCodeOrPhoneOrEmail = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "  SUCCESS: Got auth token" -ForegroundColor Green
    Write-Host "  Token preview: $($token.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "  FAILED: Login failed - $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Find a complaint with emails
Write-Host ""
Write-Host "[STEP 2] Finding complaint with emails..." -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $complaintsResponse = Invoke-RestMethod -Uri "$baseUrl/complaints?page=1&pageSize=10" -Method Get -Headers $headers
    $complaintWithEmails = $complaintsResponse.data.items | Select-Object -First 1
    $complaintId = $complaintWithEmails.id
    Write-Host "  SUCCESS: Using complaint ID: $complaintId" -ForegroundColor Green
} catch {
    Write-Host "  FAILED: Could not get complaints - $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Test the EMAIL THREAD endpoint (the one being called by frontend)
Write-Host ""
Write-Host "[STEP 3] Testing /api/complaints/{id}/emails endpoint..." -ForegroundColor Yellow
$emailThreadUrl = "$baseUrl/complaints/$complaintId/emails"

try {
    $emailResponse = Invoke-RestMethod -Uri $emailThreadUrl -Method Get -Headers $headers
    Write-Host "  SUCCESS: Got email thread response" -ForegroundColor Green
    Write-Host "  Response structure:" -ForegroundColor Cyan
    Write-Host "    - isSuccess: $($emailResponse.isSuccess)" -ForegroundColor Gray
    Write-Host "    - Email count: $($emailResponse.data.Count)" -ForegroundColor Gray

    if ($emailResponse.data.Count -gt 0) {
        $firstEmail = $emailResponse.data[0]
        Write-Host "    - First email properties:" -ForegroundColor Gray
        $firstEmail.PSObject.Properties | ForEach-Object {
            Write-Host "      - $($_.Name): $($_.Value)" -ForegroundColor DarkGray
        }

        Write-Host ""
        Write-Host "  CRITICAL CHECKS:" -ForegroundColor Magenta
        if ($null -ne $firstEmail.isOutbound) {
            Write-Host "    ✓ HAS isOutbound property: $($firstEmail.isOutbound)" -ForegroundColor Green
        } else {
            Write-Host "    ✗ MISSING isOutbound property!" -ForegroundColor Red
        }

        if ($null -ne $firstEmail.direction) {
            Write-Host "    ✗ STILL HAS direction property (enum): $($firstEmail.direction)" -ForegroundColor Red
        } else {
            Write-Host "    ✓ NO direction property (good)" -ForegroundColor Green
        }

        # Save full response for inspection
        $emailResponse | ConvertTo-Json -Depth 10 | Out-File "email-thread-api-response.json"
        Write-Host ""
        Write-Host "  Full response saved to: email-thread-api-response.json" -ForegroundColor Cyan
    } else {
        Write-Host "    - No emails in thread" -ForegroundColor Yellow
    }

} catch {
    Write-Host "  FAILED: API call failed" -ForegroundColor Red
    Write-Host "  Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.Exception.Response.StatusCode.value__ -eq 401) {
        Write-Host ""
        Write-Host "  ⚠ 401 UNAUTHORIZED ERROR DETECTED!" -ForegroundColor Red
        Write-Host "  This means the token is not being accepted by the API" -ForegroundColor Red
    }
}

# Step 4: Check if IEmailThreadingService exists
Write-Host ""
Write-Host "[STEP 4] Checking backend controller registration..." -ForegroundColor Yellow
Write-Host "  EmailThreadController should be registered at:" -ForegroundColor Gray
Write-Host "    Route: /api/complaints/{complaintId}/emails" -ForegroundColor Gray
Write-Host "    File: EmailThreadController.cs" -ForegroundColor Gray

# Step 5: Summary
Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "INVESTIGATION SUMMARY" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Issue Reported: Email action buttons (Reply, Forward) not showing in UI" -ForegroundColor Yellow
Write-Host ""
Write-Host "Checks Performed:" -ForegroundColor Cyan
Write-Host "  1. Login & Auth Token - PASSED" -ForegroundColor Green
Write-Host "  2. Complaint List API - PASSED" -ForegroundColor Green
Write-Host "  3. Email Thread API - See results above" -ForegroundColor Yellow
Write-Host ""
Write-Host "Expected Behavior:" -ForegroundColor Cyan
Write-Host "  - Email objects should have 'isOutbound' property (boolean)" -ForegroundColor Gray
Write-Host "  - Email objects should NOT have 'direction' property (enum)" -ForegroundColor Gray
Write-Host "  - Frontend shows buttons when: !isExpanded && showActions && (for inbound emails)" -ForegroundColor Gray
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Check email-thread-api-response.json for exact API structure" -ForegroundColor Gray
Write-Host "  2. Verify EmailThreadController is properly registered in DI" -ForegroundColor Gray
Write-Host "  3. Check browser DevTools Network tab for the actual frontend request" -ForegroundColor Gray
Write-Host ""
