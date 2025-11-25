# ============================================
# USER AUTHENTICATION DEBUG TEST
# Tests login and shows raw response
# ============================================

$ErrorActionPreference = "Continue"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "USER AUTHENTICATION DEBUG TEST" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000/api"
$loginUrl = "$baseUrl/auth/login"

# ============================================
# TEST 1: Complainant Login
# ============================================

Write-Host "TEST 1: Complainant Login" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow
Write-Host "Email: nav_nainital@yahoo.com"
Write-Host "Password: Nav@12345"
Write-Host ""

$complainantBody = @{
    email = "nav_nainital@yahoo.com"
    password = "Nav@12345"
} | ConvertTo-Json

Write-Host "Request Body: $complainantBody" -ForegroundColor Gray
Write-Host ""

try {
    $response = Invoke-WebRequest -Uri $loginUrl -Method Post -Body $complainantBody -ContentType "application/json" -UseBasicParsing

    Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Content Type: $($response.Headers['Content-Type'])" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Raw Response:" -ForegroundColor Green
    Write-Host $response.Content
    Write-Host ""

    # Try to parse as JSON
    try {
        $jsonResponse = $response.Content | ConvertFrom-Json
        Write-Host "Parsed Response:" -ForegroundColor Green
        $jsonResponse | Format-List
    } catch {
        Write-Host "Could not parse as JSON" -ForegroundColor Yellow
    }

} catch {
    Write-Host "FAILED: Login failed!" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errorBody = $reader.ReadToEnd()
            Write-Host "Response Body: $errorBody" -ForegroundColor Red
        } catch {
            Write-Host "Could not read error body" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# ============================================
# TEST 2: Handler Login
# ============================================

Write-Host "TEST 2: Handler Login" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow
Write-Host "Email: naveen.chandra@oryggitech.com"
Write-Host "Password: Naveen@12345"
Write-Host ""

$handlerBody = @{
    email = "naveen.chandra@oryggitech.com"
    password = "Naveen@12345"
} | ConvertTo-Json

Write-Host "Request Body: $handlerBody" -ForegroundColor Gray
Write-Host ""

try {
    $response = Invoke-WebRequest -Uri $loginUrl -Method Post -Body $handlerBody -ContentType "application/json" -UseBasicParsing

    Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Content Type: $($response.Headers['Content-Type'])" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Raw Response:" -ForegroundColor Green
    Write-Host $response.Content
    Write-Host ""

    # Try to parse as JSON
    try {
        $jsonResponse = $response.Content | ConvertFrom-Json
        Write-Host "Parsed Response:" -ForegroundColor Green
        $jsonResponse | Format-List
    } catch {
        Write-Host "Could not parse as JSON" -ForegroundColor Yellow
    }

} catch {
    Write-Host "FAILED: Login failed!" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errorBody = $reader.ReadToEnd()
            Write-Host "Response Body: $errorBody" -ForegroundColor Red
        } catch {
            Write-Host "Could not read error body" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
