# Test Email Buttons Endpoint - Verify isOutbound Property

Write-Host "=== EMAIL BUTTONS ENDPOINT TEST ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login and get fresh token
Write-Host "Step 1: Getting fresh token..." -ForegroundColor Yellow
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
        -Method POST `
        -Body $loginBody `
        -ContentType "application/json"

    if ($loginResponse.isSuccess) {
        $token = $loginResponse.data.token
        Write-Host "✓ Login successful" -ForegroundColor Green
        Write-Host "Token (first 50 chars): $($token.Substring(0,50))..." -ForegroundColor Gray
    } else {
        Write-Host "✗ Login failed: $($loginResponse.message)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Login error: $_" -ForegroundColor Red
    exit 1
}

# Step 2: Get list of complaints to find one with emails
Write-Host ""
Write-Host "Step 2: Finding complaint with emails..." -ForegroundColor Yellow

$headers = @{
    "Authorization" = "Bearer $token"
}

try {
    $complaintsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints?pageSize=10" `
        -Method GET `
        -Headers $headers

    if ($complaintsResponse.isSuccess -and $complaintsResponse.data.items.Count -gt 0) {
        $complaintId = $complaintsResponse.data.items[0].id
        Write-Host "✓ Found complaint: $complaintId" -ForegroundColor Green
    } else {
        Write-Host "✗ No complaints found" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Error getting complaints: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Test EMAIL THREAD CONTROLLER endpoint
Write-Host ""
Write-Host "Step 3: Testing /api/complaints/{id}/emails endpoint..." -ForegroundColor Yellow
Write-Host "Endpoint: http://localhost:5000/api/complaints/$complaintId/emails" -ForegroundColor Gray

try {
    $emailsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$complaintId/emails" `
        -Method GET `
        -Headers $headers

    if ($emailsResponse.isSuccess) {
        Write-Host "✓ API call successful" -ForegroundColor Green
        Write-Host "  Email count: $($emailsResponse.data.Count)" -ForegroundColor Gray

        if ($emailsResponse.data.Count -gt 0) {
            $firstEmail = $emailsResponse.data[0]

            Write-Host ""
            Write-Host "=== FIRST EMAIL OBJECT INSPECTION ===" -ForegroundColor Cyan
            Write-Host "  ID: $($firstEmail.id)" -ForegroundColor Gray
            Write-Host "  From: $($firstEmail.fromEmail)" -ForegroundColor Gray
            Write-Host "  Subject: $($firstEmail.subject)" -ForegroundColor Gray

            # CRITICAL CHECK: Does it have isOutbound property?
            if ($null -ne $firstEmail.isOutbound) {
                Write-Host "  ✓ isOutbound property EXISTS: $($firstEmail.isOutbound)" -ForegroundColor Green
                Write-Host "  ✓ Type: $(if ($firstEmail.isOutbound -is [bool]) { 'BOOLEAN' } else { $firstEmail.isOutbound.GetType().Name })" -ForegroundColor Green
            } else {
                Write-Host "  ✗ isOutbound property MISSING" -ForegroundColor Red
            }

            # Check if it has Direction enum instead
            if ($null -ne $firstEmail.direction) {
                Write-Host "  ⚠ direction property EXISTS: $($firstEmail.direction)" -ForegroundColor Yellow
                Write-Host "  ⚠ This is the OLD format (should be isOutbound boolean)" -ForegroundColor Yellow
            }

            Write-Host ""
            Write-Host "=== FULL FIRST EMAIL OBJECT ===" -ForegroundColor Cyan
            $firstEmail | ConvertTo-Json -Depth 3 | Write-Host

        } else {
            Write-Host "  ⚠ No emails found for this complaint" -ForegroundColor Yellow
            Write-Host "  Trying another complaint..." -ForegroundColor Yellow

            # Try second complaint
            if ($complaintsResponse.data.items.Count -gt 1) {
                $complaintId = $complaintsResponse.data.items[1].id
                Write-Host "  Testing complaint: $complaintId" -ForegroundColor Gray

                $emailsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$complaintId/emails" `
                    -Method GET `
                    -Headers $headers

                if ($emailsResponse.isSuccess -and $emailsResponse.data.Count -gt 0) {
                    $firstEmail = $emailsResponse.data[0]
                    Write-Host "  ✓ Found email in second complaint" -ForegroundColor Green
                    Write-Host "  isOutbound: $($firstEmail.isOutbound)" -ForegroundColor Gray
                    $firstEmail | ConvertTo-Json -Depth 3 | Write-Host
                }
            }
        }

    } else {
        Write-Host "✗ API returned error: $($emailsResponse.message)" -ForegroundColor Red
    }
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $statusDesc = $_.Exception.Response.StatusDescription
    Write-Host "✗ API call failed" -ForegroundColor Red
    Write-Host "  Status Code: $statusCode $statusDesc" -ForegroundColor Red
    Write-Host "  Error: $_" -ForegroundColor Red

    if ($statusCode -eq 401) {
        Write-Host ""
        Write-Host "  401 UNAUTHORIZED - This means:" -ForegroundColor Yellow
        Write-Host "  - Token might have expired" -ForegroundColor Yellow
        Write-Host "  - Endpoint requires special permissions" -ForegroundColor Yellow
        Write-Host "  - RBAC rules are blocking access" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== TEST COMPLETE ===" -ForegroundColor Cyan
