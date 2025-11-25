# Debug complaint creation with detailed error output
$ErrorActionPreference = "Continue"

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Debugging Complaint Creation Error" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Login first
Write-Host "1. Logging in..." -ForegroundColor Yellow
$loginUrl = "http://localhost:5058/api/auth/login"
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "   Login successful!" -ForegroundColor Green
    Write-Host "   User: $($loginResponse.data.user.fullName)" -ForegroundColor Green
    Write-Host "   Email: $($loginResponse.data.user.email)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "   Login failed: $_" -ForegroundColor Red
    exit 1
}

# Get categories
Write-Host "2. Fetching categories..." -ForegroundColor Yellow
$categoriesUrl = "http://localhost:5058/api/categories"
$headers = @{
    "Authorization" = "Bearer $token"
}

try {
    $categoriesResponse = Invoke-RestMethod -Uri $categoriesUrl -Method Get -Headers $headers
    if ($categoriesResponse.data.Count -gt 0) {
        $categoryId = $categoriesResponse.data[0].id
        Write-Host "   Category ID: $categoryId" -ForegroundColor Green
    }
} catch {
    Write-Host "   Failed: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "3. Testing with MINIMAL data (like Angular might send)..." -ForegroundColor Yellow
Write-Host ""

# Try with minimal data
$testCases = @(
    @{
        name = "Test 1: Minimal required fields only"
        body = @{
            title = "Test Complaint"
            description = "Test Description"
            categoryId = $categoryId
            priority = 1
            isAnonymous = $false
        }
    },
    @{
        name = "Test 2: With null optional fields"
        body = @{
            title = "Test Complaint 2"
            description = "Test Description 2"
            categoryId = $categoryId
            priority = 1
            isAnonymous = $false
            branchId = $null
            departmentId = $null
            sectionId = $null
            tags = $null
            employeeCode = $null
            contactEmail = $null
            contactPhone = $null
            alternatePhone = $null
            preferredContactMethod = $null
        }
    },
    @{
        name = "Test 3: With empty strings"
        body = @{
            title = "Test Complaint 3"
            description = "Test Description 3"
            categoryId = $categoryId
            priority = 1
            isAnonymous = $false
            tags = ""
            employeeCode = ""
            contactEmail = ""
            contactPhone = ""
        }
    }
)

foreach ($test in $testCases) {
    Write-Host "---" -ForegroundColor Gray
    Write-Host $test.name -ForegroundColor Cyan
    Write-Host "Request Body:" -ForegroundColor Gray
    Write-Host ($test.body | ConvertTo-Json) -ForegroundColor DarkGray
    Write-Host ""

    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5058/api/complaints" -Method Post -Body ($test.body | ConvertTo-Json) -ContentType "application/json" -Headers $headers
        Write-Host "✅ SUCCESS!" -ForegroundColor Green
        Write-Host "   Complaint Number: $($response.data.complaintNumber)" -ForegroundColor Green
        Write-Host ""
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "❌ FAILED - Status: $statusCode" -ForegroundColor Red

        # Try to get the error response body
        try {
            $errorStream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($errorStream)
            $errorBody = $reader.ReadToEnd()
            $reader.Close()

            Write-Host ""
            Write-Host "Error Response Body:" -ForegroundColor Red
            Write-Host $errorBody -ForegroundColor Yellow

            # Try to parse as JSON
            try {
                $errorJson = $errorBody | ConvertFrom-Json
                Write-Host ""
                Write-Host "Parsed Validation Errors:" -ForegroundColor Red
                $errorJson | ConvertTo-Json -Depth 5 | Write-Host -ForegroundColor Yellow
            } catch {
                # Not JSON
            }
        } catch {
            Write-Host "Could not read error details" -ForegroundColor Red
        }
        Write-Host ""
    }
}

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Debug Complete" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
