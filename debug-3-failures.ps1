# Debug script for 3 failing tests
$baseUrl = "http://localhost:5058"

# Test-APIEndpoint function
function Test-APIEndpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Endpoint,
        [string]$Body = $null,
        [hashtable]$Headers = @{}
    )

    try {
        $uri = "$baseUrl$Endpoint"
        Write-Host "`n===========================================`n"  -ForegroundColor Cyan
        Write-Host "[TEST] $Name" -ForegroundColor Yellow
        Write-Host "Method: $Method" -ForegroundColor Gray
        Write-Host "URI: $uri" -ForegroundColor Gray
        if ($Body) {
            Write-Host "Body: $Body" -ForegroundColor Gray
        }

        $params = @{
            Uri = $uri
            Method = $Method
            Headers = $Headers
            ContentType = "application/json"
        }

        if ($Body) {
            $params.Body = $Body
        }

        $response = Invoke-RestMethod @params -ErrorAction Stop

        Write-Host "[PASS] Status: Success" -ForegroundColor Green
        Write-Host "Response: $($response | ConvertTo-Json -Depth 3)" -ForegroundColor Green

        return $response
    }
    catch {
        Write-Host "[FAIL] Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

        try {
            $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
            Write-Host "Error Details: $($errorDetails | ConvertTo-Json -Depth 3)" -ForegroundColor Red
        }
        catch {
            if ($_.ErrorDetails) {
                Write-Host "Error Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
            }
        }

        return $null
    }
}

Write-Host "===========================================
" -ForegroundColor Cyan
Write-Host "DEBUG SCRIPT FOR 3 FAILING TESTS" -ForegroundColor Cyan
Write-Host "===========================================`n" -ForegroundColor Cyan

# Step 1: Authenticate
Write-Host "[STEP 1] Authenticating..." -ForegroundColor Cyan
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123456"
} | ConvertTo-Json

$loginResponse = Test-APIEndpoint -Name "Login" -Method "POST" -Endpoint "/api/auth/login" -Body $loginBody

if (-not $loginResponse) {
    Write-Host "`n[ERROR] Authentication failed. Cannot proceed." -ForegroundColor Red
    exit 1
}

$token = $loginResponse.data.token
$authHeaders = @{
    "Authorization" = "Bearer $token"
}

Write-Host "`n[SUCCESS] Authentication successful. Token obtained." -ForegroundColor Green

# Step 2: Get Categories (for CategoryId)
Write-Host "`n[STEP 2] Getting categories for CategoryId..." -ForegroundColor Cyan
$categories = Test-APIEndpoint -Name "Get Categories" -Method "GET" -Endpoint "/api/categories?activeOnly=true" -Headers $authHeaders

$CategoryId = if ($categories -and $categories.data -and $categories.data.Count -gt 0) {
    $categories.data[0].id
} else {
    Write-Host "[WARNING] No categories found. Using default GUID." -ForegroundColor Yellow
    [guid]::NewGuid().ToString()
}

Write-Host "CategoryId: $CategoryId" -ForegroundColor Cyan

# Step 3: TEST 1 - Create Complaint
Write-Host "`n[STEP 3] TEST 1 - Create Complaint" -ForegroundColor Cyan
$randomComplaintNumber = Get-Random -Minimum 100000 -Maximum 999999
$createComplaintBody = @{
    title = "Debug Test Complaint $randomComplaintNumber"
    description = "This is a debug test complaint"
    categoryId = $CategoryId
    priority = 1
    isAnonymous = $false
    tags = "debug,test"
    contactEmail = "debug@test.com"
    contactPhone = "1234567890"
    preferredContactMethod = 0
} | ConvertTo-Json -Depth 5

$createdComplaint = Test-APIEndpoint -Name "Create Complaint" -Method "POST" -Endpoint "/api/complaints" -Body $createComplaintBody -Headers $authHeaders

# Step 4: Get Template for Event Rule
Write-Host "`n[STEP 4] Getting template for Event Rule test..." -ForegroundColor Cyan
$templates = Test-APIEndpoint -Name "Get Templates" -Method "GET" -Endpoint "/api/communication-templates?page=1&pageSize=10" -Headers $authHeaders

$templateId = if ($templates -and $templates.data -and $templates.data.items -and $templates.data.items.Count -gt 0) {
    $templates.data.items[0].id
} else {
    Write-Host "[WARNING] No templates found. Event Rule test may fail." -ForegroundColor Yellow
    $null
}

Write-Host "TemplateId: $templateId" -ForegroundColor Cyan

# Step 5: Get Event Types
Write-Host "`n[STEP 5] Getting event types..." -ForegroundColor Cyan
$eventTypes = Test-APIEndpoint -Name "Get Event Types" -Method "GET" -Endpoint "/api/event-types" -Headers $authHeaders

$eventTypeId = if ($eventTypes -and $eventTypes.Count -gt 0) {
    $eventTypes[0].id
} else {
    Write-Host "[WARNING] No event types found. Event Rule test will fail." -ForegroundColor Yellow
    [guid]::Empty.ToString()
}

Write-Host "EventTypeId: $eventTypeId" -ForegroundColor Cyan

# Step 6: TEST 2 - Create Event Rule
Write-Host "`n[STEP 6] TEST 2 - Create Event Rule" -ForegroundColor Cyan
$randomEventRuleNumber = Get-Random -Minimum 100000 -Maximum 999999
$createEventRuleBody = @{
    name = "Debug Test Event Rule $randomEventRuleNumber"
    eventTypeId = $eventTypeId
    channel = 0  # Email
    recipientType = 0  # Complainant
    templateId = $templateId
    isActive = $true
    priority = 1
} | ConvertTo-Json

$createdEventRule = Test-APIEndpoint -Name "Create Event Rule" -Method "POST" -Endpoint "/api/event-communication-rules" -Body $createEventRuleBody -Headers $authHeaders

# Step 7: Get Escalation Matrix for Add Level test
Write-Host "`n[STEP 7] Getting/Creating escalation matrix..." -ForegroundColor Cyan
$escalationMatrices = Test-APIEndpoint -Name "Get Escalation Matrices" -Method "GET" -Endpoint "/api/escalation/matrices" -Headers $authHeaders

$matrixId = $null
if ($escalationMatrices -and $escalationMatrices.Count -gt 0) {
    $matrixId = $escalationMatrices[0].id
    Write-Host "Using existing matrix: $matrixId" -ForegroundColor Cyan
} else {
    Write-Host "Creating new escalation matrix..." -ForegroundColor Cyan
    $createMatrixBody = @{
        name = "Debug Test Matrix $(Get-Random -Minimum 100000 -Maximum 999999)"
        description = "Debug test escalation matrix"
        categoryId = $CategoryId
        isActive = $true
    } | ConvertTo-Json

    $createdMatrix = Test-APIEndpoint -Name "Create Escalation Matrix" -Method "POST" -Endpoint "/api/escalation/matrices" -Body $createMatrixBody -Headers $authHeaders

    if ($createdMatrix -and $createdMatrix.data) {
        $matrixId = $createdMatrix.data.id
    }
}

# Step 8: Get UserId from token response
$UserId = $loginResponse.data.userId

# Step 9: TEST 3 - Add Escalation Level
if ($matrixId) {
    Write-Host "`n[STEP 8] TEST 3 - Add Escalation Level" -ForegroundColor Cyan
    $addLevelBody = @{
        level = 2
        name = "Debug Level 2 Escalation"
        assignmentStrategy = 3  # SpecificUser
        assignToUserId = $UserId
        triggerAfterValue = 48
        triggerTimeUnit = 0  # Hours
        sendNotification = $true
    } | ConvertTo-Json -Depth 5

    $addedLevel = Test-APIEndpoint -Name "Add Escalation Level" -Method "POST" -Endpoint "/api/escalation/matrices/$matrixId/levels" -Body $addLevelBody -Headers $authHeaders
} else {
    Write-Host "`n[SKIP] TEST 3 - Add Escalation Level (No matrix ID available)" -ForegroundColor Yellow
}

Write-Host "`n===========================================
" -ForegroundColor Cyan
Write-Host "DEBUG SCRIPT COMPLETED" -ForegroundColor Cyan
Write-Host "===========================================`n" -ForegroundColor Cyan
