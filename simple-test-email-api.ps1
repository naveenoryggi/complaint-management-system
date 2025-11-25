# Simple test - Does the email endpoint return isOutbound?

$ErrorActionPreference = "Stop"

Write-Host "Testing Email API..." -ForegroundColor Cyan

# Login
$loginBody = '{"email":"admin@complaintmanagement.com","password":"Admin@123"}'
$login = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $login.data.token
Write-Host "✓ Logged in" -ForegroundColor Green

# Get a complaint
$headers = @{ Authorization = "Bearer $token" }
$complaints = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints?pageSize=5" -Headers $headers
$complaintId = $complaints.data.items[0].id
Write-Host "✓ Found complaint: $complaintId" -ForegroundColor Green

# Get emails for complaint
Write-Host ""
Write-Host "Calling: GET /api/complaints/$complaintId/emails" -ForegroundColor Yellow

try {
    $emails = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints/$complaintId/emails" -Headers $headers

    if ($emails.isSuccess) {
        Write-Host "✓ API call successful" -ForegroundColor Green
        Write-Host "  Email count: $($emails.data.Count)" -ForegroundColor Gray

        if ($emails.data.Count -gt 0) {
            $first = $emails.data[0]
            Write-Host ""
            Write-Host "First Email:" -ForegroundColor Cyan
            Write-Host "  Subject: $($first.subject)" -ForegroundColor Gray
            Write-Host "  From: $($first.fromEmail)" -ForegroundColor Gray

            if ($null -ne $first.isOutbound) {
                Write-Host "  ✓ isOutbound: $($first.isOutbound) (type: $($first.isOutbound.GetType().Name))" -ForegroundColor Green
            } else {
                Write-Host "  ✗ isOutbound: MISSING" -ForegroundColor Red
            }

            if ($null -ne $first.direction) {
                Write-Host "  ⚠ direction: $($first.direction) (OLD FORMAT)" -ForegroundColor Yellow
            }

            Write-Host ""
            Write-Host "Full object:" -ForegroundColor Cyan
            $first | ConvertTo-Json -Depth 2
        } else {
            Write-Host "  No emails in this complaint" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ API returned error: $($emails.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ API ERROR" -ForegroundColor Red
    Write-Host $_ -ForegroundColor Red
}
