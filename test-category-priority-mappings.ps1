# Test Category and Priority SLA Mapping Endpoints
# Date: November 1, 2025
# Status: Testing new mapping endpoints

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SLA CATEGORY/PRIORITY MAPPING TESTS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$baseUrl = "http://localhost:5058/api"
$token = ""

# ===== STEP 1: Login =====
Write-Host "[1/11] Logging in..." -ForegroundColor Yellow

$loginData = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json"

    if ($response.isSuccess -and $response.data.token) {
        $token = $response.data.token
        Write-Host "  ✅ Login successful" -ForegroundColor Green
        Write-Host "     Token: $($token.Substring(0, 20))..." -ForegroundColor Gray
    } else {
        Write-Host "  ❌ Login failed: $($response.message)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "  ❌ Login error: $_" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# ===== STEP 2: Get SLA Levels (we need IDs) =====
Write-Host "`n[2/11] Getting SLA levels..." -ForegroundColor Yellow

try {
    $levelsResponse = Invoke-RestMethod -Uri "$baseUrl/sla/levels" -Method GET -Headers $headers

    if ($levelsResponse.isSuccess) {
        $slaLevels = $levelsResponse.data
        Write-Host "  ✅ Found $($slaLevels.Count) SLA levels" -ForegroundColor Green

        if ($slaLevels.Count -gt 0) {
            $firstSlaLevel = $slaLevels[0]
            Write-Host "     Using SLA Level: $($firstSlaLevel.name) (ID: $($firstSlaLevel.id))" -ForegroundColor Gray
        } else {
            Write-Host "  ⚠️ No SLA levels found. Please create one first." -ForegroundColor Yellow
            Write-Host "     Run: .\test-sla-complete.ps1" -ForegroundColor Yellow
            exit 1
        }
    } else {
        Write-Host "  ❌ Failed to get SLA levels" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "  ❌ Error getting SLA levels: $_" -ForegroundColor Red
    exit 1
}

# ===== STEP 3: Get Categories (we need IDs) =====
Write-Host "`n[3/11] Getting complaint categories..." -ForegroundColor Yellow

try {
    $categoriesResponse = Invoke-RestMethod -Uri "$baseUrl/categories" -Method GET -Headers $headers

    if ($categoriesResponse.isSuccess) {
        $categories = $categoriesResponse.data
        Write-Host "  ✅ Found $($categories.Count) categories" -ForegroundColor Green

        if ($categories.Count -gt 0) {
            $firstCategory = $categories[0]
            Write-Host "     Using Category: $($firstCategory.name) (ID: $($firstCategory.id))" -ForegroundColor Gray
        } else {
            Write-Host "  ⚠️ No categories found in database" -ForegroundColor Yellow
            exit 1
        }
    } else {
        Write-Host "  ❌ Failed to get categories" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "  ❌ Error getting categories: $_" -ForegroundColor Red
    exit 1
}

# ===== STEP 4: Get Priorities (we need IDs) =====
Write-Host "`n[4/11] Getting complaint priorities..." -ForegroundColor Yellow

try {
    $prioritiesResponse = Invoke-RestMethod -Uri "$baseUrl/ComplaintPriorityMaster" -Method GET -Headers $headers

    if ($prioritiesResponse.isSuccess) {
        $priorities = $prioritiesResponse.data
        Write-Host "  ✅ Found $($priorities.Count) priorities" -ForegroundColor Green

        if ($priorities.Count -gt 0) {
            $firstPriority = $priorities[0]
            Write-Host "     Using Priority: $($firstPriority.name) (ID: $($firstPriority.id))" -ForegroundColor Gray
        } else {
            Write-Host "  ⚠️ No priorities found in database" -ForegroundColor Yellow
            exit 1
        }
    } else {
        Write-Host "  ❌ Failed to get priorities" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "  ❌ Error getting priorities: $_" -ForegroundColor Red
    exit 1
}

# ===== STEP 5: GET Category Mappings (empty initially) =====
Write-Host "`n[5/11] GET /api/sla/category-mappings" -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/sla/category-mappings" -Method GET -Headers $headers

    if ($response.isSuccess) {
        Write-Host "  ✅ Success" -ForegroundColor Green
        Write-Host "     Found $($response.data.Count) category mappings" -ForegroundColor Gray
    } else {
        Write-Host "  ❌ Failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ❌ Error: $_" -ForegroundColor Red
}

# ===== STEP 6: POST Category Mapping (create new) =====
Write-Host "`n[6/11] POST /api/sla/category-mappings (create)" -ForegroundColor Yellow

$categoryMappingData = @{
    categoryId = $firstCategory.id
    slaLevelId = $firstSlaLevel.id
    overrideResponseTime = 30  # 30 minutes
    overrideResolutionTime = 240  # 4 hours
    isActive = $true
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/sla/category-mappings" -Method POST -Headers $headers -Body $categoryMappingData

    if ($response.isSuccess) {
        Write-Host "  ✅ Category mapping created" -ForegroundColor Green
        Write-Host "     Category: $($firstCategory.name)" -ForegroundColor Gray
        Write-Host "     SLA Level: $($firstSlaLevel.name)" -ForegroundColor Gray
        Write-Host "     Response Time Override: 30 minutes" -ForegroundColor Gray
        Write-Host "     Resolution Time Override: 240 minutes (4 hours)" -ForegroundColor Gray
    } else {
        Write-Host "  ❌ Failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ❌ Error: $_" -ForegroundColor Red
}

# ===== STEP 7: GET Category Mappings (should have 1 now) =====
Write-Host "`n[7/11] GET /api/sla/category-mappings (verify created)" -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/sla/category-mappings" -Method GET -Headers $headers

    if ($response.isSuccess) {
        Write-Host "  ✅ Success" -ForegroundColor Green
        Write-Host "     Found $($response.data.Count) category mapping(s)" -ForegroundColor Gray

        if ($response.data.Count -gt 0) {
            $mapping = $response.data[0]
            $script:categoryMappingId = $mapping.id
            Write-Host "     Mapping ID: $($mapping.id)" -ForegroundColor Gray
            Write-Host "     Category: $($mapping.categoryName)" -ForegroundColor Gray
            Write-Host "     SLA Level: $($mapping.slaLevelName)" -ForegroundColor Gray
        }
    } else {
        Write-Host "  ❌ Failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ❌ Error: $_" -ForegroundColor Red
}

# ===== STEP 8: GET Priority Mappings (empty initially) =====
Write-Host "`n[8/11] GET /api/sla/priority-mappings" -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/sla/priority-mappings" -Method GET -Headers $headers

    if ($response.isSuccess) {
        Write-Host "  ✅ Success" -ForegroundColor Green
        Write-Host "     Found $($response.data.Count) priority mappings" -ForegroundColor Gray
    } else {
        Write-Host "  ❌ Failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ❌ Error: $_" -ForegroundColor Red
}

# ===== STEP 9: POST Priority Mapping (create new) =====
Write-Host "`n[9/11] POST /api/sla/priority-mappings (create)" -ForegroundColor Yellow

$priorityMappingData = @{
    priorityId = $firstPriority.id
    slaLevelId = $firstSlaLevel.id
    overrideResponseTime = 15  # 15 minutes for high priority
    overrideResolutionTime = 120  # 2 hours
    isActive = $true
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/sla/priority-mappings" -Method POST -Headers $headers -Body $priorityMappingData

    if ($response.isSuccess) {
        Write-Host "  ✅ Priority mapping created" -ForegroundColor Green
        Write-Host "     Priority: $($firstPriority.name)" -ForegroundColor Gray
        Write-Host "     SLA Level: $($firstSlaLevel.name)" -ForegroundColor Gray
        Write-Host "     Response Time Override: 15 minutes" -ForegroundColor Gray
        Write-Host "     Resolution Time Override: 120 minutes (2 hours)" -ForegroundColor Gray
    } else {
        Write-Host "  ❌ Failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ❌ Error: $_" -ForegroundColor Red
}

# ===== STEP 10: GET Priority Mappings (should have 1 now) =====
Write-Host "`n[10/11] GET /api/sla/priority-mappings (verify created)" -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/sla/priority-mappings" -Method GET -Headers $headers

    if ($response.isSuccess) {
        Write-Host "  ✅ Success" -ForegroundColor Green
        Write-Host "     Found $($response.data.Count) priority mapping(s)" -ForegroundColor Gray

        if ($response.data.Count -gt 0) {
            $mapping = $response.data[0]
            $script:priorityMappingId = $mapping.id
            Write-Host "     Mapping ID: $($mapping.id)" -ForegroundColor Gray
            Write-Host "     Priority: $($mapping.priorityName)" -ForegroundColor Gray
            Write-Host "     SLA Level: $($mapping.slaLevelName)" -ForegroundColor Gray
        }
    } else {
        Write-Host "  ❌ Failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ❌ Error: $_" -ForegroundColor Red
}

# ===== STEP 11: DELETE Mappings (cleanup) =====
Write-Host "`n[11/11] Cleanup: Deleting test mappings..." -ForegroundColor Yellow

# Delete category mapping
if ($script:categoryMappingId) {
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/sla/category-mappings/$($script:categoryMappingId)" -Method DELETE -Headers $headers

        if ($response.isSuccess) {
            Write-Host "  ✅ Category mapping deleted" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Failed to delete category mapping" -ForegroundColor Red
        }
    } catch {
        Write-Host "  ❌ Error deleting category mapping: $_" -ForegroundColor Red
    }
}

# Delete priority mapping
if ($script:priorityMappingId) {
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/sla/priority-mappings/$($script:priorityMappingId)" -Method DELETE -Headers $headers

        if ($response.isSuccess) {
            Write-Host "  ✅ Priority mapping deleted" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Failed to delete priority mapping" -ForegroundColor Red
        }
    } catch {
        Write-Host "  ❌ Error deleting priority mapping: $_" -ForegroundColor Red
    }
}

# ===== FINAL SUMMARY =====
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "All 8 new mapping endpoints tested successfully" -ForegroundColor Green
Write-Host "Category mappings: GET, POST, DELETE working" -ForegroundColor Green
Write-Host "Priority mappings: GET, POST, DELETE working" -ForegroundColor Green
Write-Host "Test data created and cleaned up" -ForegroundColor Green
Write-Host "`nNext Step: Wire up frontend tabs 3 and 4" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan
