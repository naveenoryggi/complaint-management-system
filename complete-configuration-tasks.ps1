# Complete Configuration Tasks Script
# This script creates test users, verifies handler users, and creates SLA policies

$ErrorActionPreference = "Stop"
$baseUrl = "http://localhost:5000"

# Read token
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Extract company ID from JWT token
$tokenParts = $token.Split(".")
$payload = $tokenParts[1]
$padding = 4 - ($payload.Length % 4)
if ($padding -ne 4) {
    $payload += "=" * $padding
}
$payloadJson = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($payload))
$payloadObj = $payloadJson | ConvertFrom-Json
$companyId = $payloadObj.CompanyId

Write-Host "Using Company ID: $companyId" -ForegroundColor Cyan

$report = @{
    TaskResults = @()
    Summary = @{
        TotalTasks = 4
        Successful = 0
        Failed = 0
        Warnings = @()
    }
}

function Write-Report {
    param($TaskName, $Status, $Details)

    $taskResult = @{
        TaskName = $TaskName
        Status = $Status
        Details = $Details
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }

    $report.TaskResults += $taskResult

    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "TASK: $TaskName" -ForegroundColor Yellow
    Write-Host "STATUS: $Status" -ForegroundColor $(if($Status -eq "SUCCESS") {"Green"} elseif($Status -eq "WARNING") {"Yellow"} else {"Red"})
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host $Details
}

# ============================================
# TASK 1: Create Test User (nav_nainital@yahoo.com)
# ============================================
try {
    Write-Host "`n`n=== TASK 1: Creating Test User ===" -ForegroundColor Magenta

    # First, get all roles to find Complainant role
    Write-Host "Fetching roles..." -ForegroundColor Gray
    $rolesResponse = Invoke-RestMethod -Uri "$baseUrl/api/roles" -Method Get -Headers $headers

    # Extract data from response
    if ($rolesResponse.data) {
        $roles = $rolesResponse.data
    } else {
        $roles = $rolesResponse
    }

    Write-Host "Found $($roles.Count) roles" -ForegroundColor Gray

    $complainantRole = $roles | Where-Object { $_.name -like "*Complainant*" -or $_.roleType -eq 3 }

    if (-not $complainantRole) {
        $roleNames = ($roles | ForEach-Object { $_.name }) -join ', '
        throw "Complainant role not found. Available roles: $roleNames"
    }

    Write-Host "Found Complainant Role: $($complainantRole.name) (ID: $($complainantRole.id))" -ForegroundColor Green

    # Check if user already exists
    Write-Host "Checking if user already exists..." -ForegroundColor Gray
    $usersResponse = Invoke-RestMethod -Uri "$baseUrl/api/users" -Method Get -Headers $headers

    # Extract data from response
    if ($usersResponse.data) {
        $existingUsers = $usersResponse.data
    } else {
        $existingUsers = $usersResponse
    }

    $existingUser = $existingUsers | Where-Object { $_.email -eq "nav_nainital@yahoo.com" }

    if ($existingUser) {
        $details = @"
User already exists:
- User ID: $($existingUser.id)
- Email: $($existingUser.email)
- Full Name: $($existingUser.fullName)
- Employee Code: $($existingUser.employeeCode)
- Is Active: $($existingUser.isActive)
- Role IDs: $($existingUser.roleIds -join ', ')
"@
        Write-Report "Task 1: Create Test User" "WARNING" $details
        $report.Summary.Warnings += "User nav_nainital@yahoo.com already exists"
    } else {
        # Create new user
        $newUser = @{
            email = "nav_nainital@yahoo.com"
            firstName = "Nav"
            lastName = "Nainital"
            employeeCode = "NAV001"
            password = "Nav@12345"
            companyId = $companyId
            roleIds = @($complainantRole.id)
            isActive = $true
        }

        Write-Host "Creating user..." -ForegroundColor Gray
        $createdUser = Invoke-RestMethod -Uri "$baseUrl/api/users" -Method Post -Headers $headers -Body ($newUser | ConvertTo-Json)

        $details = @"
User created successfully:
- User ID: $($createdUser.id)
- Email: $($createdUser.email)
- Full Name: $($createdUser.fullName)
- Employee Code: $($createdUser.employeeCode)
- Role: Complainant (ID: $($complainantRole.id))
- Is Active: $($createdUser.isActive)
"@
        Write-Report "Task 1: Create Test User" "SUCCESS" $details
        $report.Summary.Successful++
    }
} catch {
    $details = "Error creating user: $($_.Exception.Message)"
    Write-Report "Task 1: Create Test User" "FAILED" $details
    $report.Summary.Failed++
}

# ============================================
# TASK 2: Verify Handler User
# ============================================
try {
    Write-Host "`n`n=== TASK 2: Verifying Handler User ===" -ForegroundColor Magenta

    $usersResponse = Invoke-RestMethod -Uri "$baseUrl/api/users" -Method Get -Headers $headers

    # Extract data from response
    if ($usersResponse.data) {
        $users = $usersResponse.data
    } else {
        $users = $usersResponse
    }

    $handlerUser = $users | Where-Object { $_.email -eq "Naveen.chandra@oryggitech.com" }

    if ($handlerUser) {
        # Get role details
        $rolesResponse = Invoke-RestMethod -Uri "$baseUrl/api/roles" -Method Get -Headers $headers

        # Extract data from response
        if ($rolesResponse.data) {
            $roles = $rolesResponse.data
        } else {
            $roles = $rolesResponse
        }

        $userRoles = $roles | Where-Object { $handlerUser.roleIds -contains $_.id }

        $roleDetails = $userRoles | ForEach-Object { "  - $($_.name) (ID: $($_.id), Type: $($_.roleType))" }

        $details = @"
Handler user found and verified:
- User ID: $($handlerUser.id)
- Email: $($handlerUser.email)
- Full Name: $($handlerUser.fullName)
- Employee Code: $($handlerUser.employeeCode)
- Is Active: $($handlerUser.isActive)
- Role Assignments:
$($roleDetails -join "`n")
- Has Handler Role: $(if($userRoles | Where-Object { $_.roleType -eq 2 -or $_.name -like "*Handler*" }) {"YES"} else {"NO - WARNING!"})
"@
        Write-Report "Task 2: Verify Handler User" "SUCCESS" $details
        $report.Summary.Successful++
    } else {
        $userEmails = ($users | ForEach-Object { $_.email }) -join ', '
        $details = "Handler user 'Naveen.chandra@oryggitech.com' not found in the system.`n`nAvailable users: $userEmails"
        Write-Report "Task 2: Verify Handler User" "FAILED" $details
        $report.Summary.Failed++
    }
} catch {
    $details = "Error verifying handler user: $($_.Exception.Message)"
    Write-Report "Task 2: Verify Handler User" "FAILED" $details
    $report.Summary.Failed++
}

# ============================================
# TASK 3: Create Priority-Based SLA Policies
# ============================================
try {
    Write-Host "`n`n=== TASK 3: Creating Priority-Based SLA Policies ===" -ForegroundColor Magenta

    # Get all priority masters
    Write-Host "Fetching priority masters..." -ForegroundColor Gray
    $prioritiesResponse = Invoke-RestMethod -Uri "$baseUrl/api/ComplaintPriorityMaster" -Method Get -Headers $headers

    # Extract data from response
    if ($prioritiesResponse.data) {
        $priorities = $prioritiesResponse.data
    } elseif ($prioritiesResponse -is [Array]) {
        $priorities = $prioritiesResponse
    } else {
        $priorities = @($prioritiesResponse)
    }

    Write-Host "Found $($priorities.Count) priority masters" -ForegroundColor Gray

    # Map priority names to IDs
    $priorityMap = @{}
    foreach ($priority in $priorities) {
        $priorityMap[$priority.name] = $priority.id
        Write-Host "  - $($priority.name): $($priority.id)" -ForegroundColor Gray
    }

    # Step 1: Create SLA Levels
    $slaLevels = @(
        @{
            name = "Low Priority Level"
            description = "SLA for Low priority complaints"
            order = 1
            colorCode = "#4CAF50"
            defaultResponseTime = 48
            defaultResolutionTime = 120
            responseTimeUnit = "Hours"
            resolutionTimeUnit = "Hours"
            isActive = $true
        },
        @{
            name = "Normal Priority Level"
            description = "SLA for Normal priority complaints"
            order = 2
            colorCode = "#2196F3"
            defaultResponseTime = 24
            defaultResolutionTime = 72
            responseTimeUnit = "Hours"
            resolutionTimeUnit = "Hours"
            isActive = $true
        },
        @{
            name = "High Priority Level"
            description = "SLA for High priority complaints"
            order = 3
            colorCode = "#FF9800"
            defaultResponseTime = 8
            defaultResolutionTime = 24
            responseTimeUnit = "Hours"
            resolutionTimeUnit = "Hours"
            isActive = $true
        },
        @{
            name = "Critical Priority Level"
            description = "SLA for Critical priority complaints"
            order = 4
            colorCode = "#F44336"
            defaultResponseTime = 4
            defaultResolutionTime = 12
            responseTimeUnit = "Hours"
            resolutionTimeUnit = "Hours"
            isActive = $true
        },
        @{
            name = "Urgent Priority Level"
            description = "SLA for Urgent priority complaints"
            order = 5
            colorCode = "#9C27B0"
            defaultResponseTime = 2
            defaultResolutionTime = 8
            responseTimeUnit = "Hours"
            resolutionTimeUnit = "Hours"
            isActive = $true
        }
    )

    $createdLevels = @()
    $failedLevels = @()

    Write-Host "Creating SLA Levels..." -ForegroundColor Gray
    foreach ($level in $slaLevels) {
        try {
            Write-Host "  Creating level: $($level.name)..." -ForegroundColor Gray
            $levelResponse = Invoke-RestMethod -Uri "$baseUrl/api/sla/levels" -Method Post -Headers $headers -Body ($level | ConvertTo-Json)

            if ($levelResponse.isSuccess) {
                $createdLevels += @{ name = $level.name; id = $levelResponse.data.id; response = $level.defaultResponseTime; resolution = $level.defaultResolutionTime }
                Write-Host "    Created: $($levelResponse.data.id)" -ForegroundColor Green
            } else {
                $failedLevels += "- $($level.name): $($levelResponse.message)"
            }
        } catch {
            $errorMsg = $_.Exception.Message
            if ($errorMsg -like "*already exists*" -or $errorMsg -like "*duplicate*") {
                Write-Host "    Already exists" -ForegroundColor Yellow
                $createdLevels += @{ name = $level.name; id = "existing"; response = $level.defaultResponseTime; resolution = $level.defaultResolutionTime }
            } else {
                $failedLevels += "- $($level.name): $errorMsg"
                Write-Host "    Failed: $errorMsg" -ForegroundColor Red
            }
        }
    }

    # Step 2: Map priorities to SLA levels
    Write-Host "Mapping priorities to SLA levels..." -ForegroundColor Gray

    # Get existing levels
    $levelsResponse = Invoke-RestMethod -Uri "$baseUrl/api/sla/levels" -Method Get -Headers $headers
    $existingLevels = if ($levelsResponse.data) { $levelsResponse.data } else { $levelsResponse }

    $priorityMappings = @(
        @{ priorityName = "Low"; levelName = "Low Priority Level" },
        @{ priorityName = "Normal"; levelName = "Normal Priority Level" },
        @{ priorityName = "High"; levelName = "High Priority Level" },
        @{ priorityName = "Critical"; levelName = "Critical Priority Level" },
        @{ priorityName = "Urgent"; levelName = "Urgent Priority Level" }
    )

    $createdMappings = @()
    $failedMappings = @()

    foreach ($mapping in $priorityMappings) {
        try {
            $priorityId = $priorityMap[$mapping.priorityName]
            $slaLevel = $existingLevels | Where-Object { $_.name -eq $mapping.levelName } | Select-Object -First 1

            if (-not $priorityId) {
                $failedMappings += "- $($mapping.priorityName): Priority not found"
                continue
            }

            if (-not $slaLevel) {
                $failedMappings += "- $($mapping.priorityName): SLA Level '$($mapping.levelName)' not found"
                continue
            }

            $mappingData = @{
                priorityId = $priorityId
                slaLevelId = $slaLevel.id
                isActive = $true
            }

            Write-Host "  Mapping $($mapping.priorityName) to $($mapping.levelName)..." -ForegroundColor Gray
            $mappingResponse = Invoke-RestMethod -Uri "$baseUrl/api/sla/priority-mappings" -Method Post -Headers $headers -Body ($mappingData | ConvertTo-Json)

            if ($mappingResponse.isSuccess) {
                $createdMappings += "- $($mapping.priorityName) -> $($mapping.levelName)"
                Write-Host "    Mapped successfully" -ForegroundColor Green
            } else {
                $failedMappings += "- $($mapping.priorityName): $($mappingResponse.message)"
            }
        } catch {
            $errorMsg = $_.Exception.Message
            if ($errorMsg -like "*already exists*" -or $errorMsg -like "*duplicate*") {
                $createdMappings += "- $($mapping.priorityName) -> $($mapping.levelName) (Already exists)"
                Write-Host "    Already exists" -ForegroundColor Yellow
            } else {
                $failedMappings += "- $($mapping.priorityName): $errorMsg"
                Write-Host "    Failed: $errorMsg" -ForegroundColor Red
            }
        }
    }

    $createdSLAs = @()
    $failedSLAs = @()

    # Combine results
    foreach ($level in $createdLevels) {
        $createdSLAs += "- $($level.name) (ID: $($level.id)) - Response: $($level.response)h, Resolution: $($level.resolution)h"
    }

    foreach ($mapping in $createdMappings) {
        $createdSLAs += $mapping
    }

    $failedSLAs = $failedLevels + $failedMappings

    $details = @"
Priority-Based SLA Policies Created/Verified:

Successfully Created/Existing ($($createdSLAs.Count)):
$($createdSLAs -join "`n")

$(if($failedSLAs.Count -gt 0) {"Failed ($($failedSLAs.Count)):`n$($failedSLAs -join "`n")"} else {"No failures"})
"@

    if ($failedSLAs.Count -eq 0) {
        Write-Report "Task 3: Create Priority-Based SLA Policies" "SUCCESS" $details
        $report.Summary.Successful++
    } else {
        Write-Report "Task 3: Create Priority-Based SLA Policies" "WARNING" $details
        $report.Summary.Warnings += "$($failedSLAs.Count) SLA policies failed to create"
    }
} catch {
    $details = "Error creating priority-based SLA policies: $($_.Exception.Message)"
    Write-Report "Task 3: Create Priority-Based SLA Policies" "FAILED" $details
    $report.Summary.Failed++
}

# ============================================
# TASK 4: Create Category-Based SLA Policies
# ============================================
try {
    Write-Host "`n`n=== TASK 4: Creating Category-Based SLA Policies ===" -ForegroundColor Magenta

    # Get all categories
    Write-Host "Fetching categories..." -ForegroundColor Gray
    $categoriesResponse = Invoke-RestMethod -Uri "$baseUrl/api/categories" -Method Get -Headers $headers

    # Extract data from response
    if ($categoriesResponse.data) {
        $categories = $categoriesResponse.data
    } elseif ($categoriesResponse -is [Array]) {
        $categories = $categoriesResponse
    } else {
        $categories = @($categoriesResponse)
    }

    Write-Host "Found $($categories.Count) categories" -ForegroundColor Gray

    # Map category names to IDs
    $categoryMap = @{}
    foreach ($category in $categories) {
        $categoryMap[$category.name] = $category.id
        Write-Host "  - $($category.name): $($category.id)" -ForegroundColor Gray
    }

    # Get existing SLA levels to map categories to
    Write-Host "Fetching existing SLA levels..." -ForegroundColor Gray
    $levelsResponse = Invoke-RestMethod -Uri "$baseUrl/api/sla/levels" -Method Get -Headers $headers
    $existingLevels = if ($levelsResponse.data) { $levelsResponse.data } else { $levelsResponse }

    Write-Host "Found $($existingLevels.Count) SLA levels" -ForegroundColor Gray

    # Find appropriate SLA level for categories (we'll use High Priority Level for technical, Normal for billing)
    $highLevel = $existingLevels | Where-Object { $_.name -like "*High*" } | Select-Object -First 1
    $normalLevel = $existingLevels | Where-Object { $_.name -like "*Normal*" } | Select-Object -First 1

    # Define category-based SLA mappings
    $categoryMappings = @(
        @{
            categoryName = "Technical Issues"
            slaLevel = $highLevel
            overrideResponseTimeMinutes = 240  # 4 hours
            overrideResolutionTimeMinutes = 960  # 16 hours
        },
        @{
            categoryName = "Billing Problems"
            slaLevel = $normalLevel
            overrideResponseTimeMinutes = 360  # 6 hours
            overrideResolutionTimeMinutes = 1440  # 24 hours
        }
    )

    $createdSLAs = @()
    $failedSLAs = @()

    foreach ($mapping in $categoryMappings) {
        try {
            # Find category ID (try exact match first, then partial)
            $categoryId = $categoryMap[$mapping.categoryName]
            $actualCategoryName = $mapping.categoryName

            if (-not $categoryId) {
                # Try partial match
                $matchingCategory = $categories | Where-Object { $_.name -like "*$($mapping.categoryName)*" } | Select-Object -First 1
                if ($matchingCategory) {
                    $categoryId = $matchingCategory.id
                    $actualCategoryName = $matchingCategory.name
                } else {
                    $failedSLAs += "- $($mapping.categoryName): Category not found. Available: $($categoryMap.Keys -join ', ')"
                    continue
                }
            }

            if (-not $mapping.slaLevel) {
                $failedSLAs += "- $($actualCategoryName): No suitable SLA level found"
                continue
            }

            # Create category SLA mapping
            $mappingData = @{
                categoryId = $categoryId
                slaLevelId = $mapping.slaLevel.id
                overrideResponseTime = $mapping.overrideResponseTimeMinutes
                overrideResolutionTime = $mapping.overrideResolutionTimeMinutes
                isActive = $true
            }

            Write-Host "Mapping $actualCategoryName to $($mapping.slaLevel.name)..." -ForegroundColor Gray
            $mappingResponse = Invoke-RestMethod -Uri "$baseUrl/api/sla/category-mappings" -Method Post -Headers $headers -Body ($mappingData | ConvertTo-Json)

            if ($mappingResponse.isSuccess) {
                $responseHours = [math]::Round($mapping.overrideResponseTimeMinutes / 60, 1)
                $resolutionHours = [math]::Round($mapping.overrideResolutionTimeMinutes / 60, 1)
                $createdSLAs += "- $actualCategoryName -> $($mapping.slaLevel.name) (Response: ${responseHours}h, Resolution: ${resolutionHours}h)"
                Write-Host "  Mapped successfully" -ForegroundColor Green
            } else {
                $failedSLAs += "- ${actualCategoryName}: $($mappingResponse.message)"
            }
        } catch {
            $errorMsg = $_.Exception.Message
            if ($errorMsg -like "*already exists*" -or $errorMsg -like "*duplicate*") {
                $createdSLAs += "- $actualCategoryName (Already exists)"
                $report.Summary.Warnings += "Category SLA mapping for '$actualCategoryName' already exists"
                Write-Host "  Already exists" -ForegroundColor Yellow
            } else {
                $failedSLAs += "- ${actualCategoryName}: $errorMsg"
                Write-Host "  Failed: $errorMsg" -ForegroundColor Red
            }
        }
    }

    $details = @"
Category-Based SLA Policies Created/Verified:

Successfully Created/Existing ($($createdSLAs.Count)):
$($createdSLAs -join "`n")

$(if($failedSLAs.Count -gt 0) {"Failed ($($failedSLAs.Count)):`n$($failedSLAs -join "`n")"} else {"No failures"})

Note: If categories don't exist with exact names, the script attempts partial matching.
"@

    if ($failedSLAs.Count -eq 0) {
        Write-Report "Task 4: Create Category-Based SLA Policies" "SUCCESS" $details
        $report.Summary.Successful++
    } else {
        Write-Report "Task 4: Create Category-Based SLA Policies" "WARNING" $details
        $report.Summary.Warnings += "$($failedSLAs.Count) category SLA policies failed to create"
    }
} catch {
    $details = "Error creating category-based SLA policies: $($_.Exception.Message)"
    Write-Report "Task 4: Create Category-Based SLA Policies" "FAILED" $details
    $report.Summary.Failed++
}

# ============================================
# FINAL SUMMARY
# ============================================
Write-Host "`n`n" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "     FINAL CONFIGURATION REPORT         " -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`nTotal Tasks: $($report.Summary.TotalTasks)" -ForegroundColor White
Write-Host "Successful: $($report.Summary.Successful)" -ForegroundColor Green
Write-Host "Failed: $($report.Summary.Failed)" -ForegroundColor Red
Write-Host "Warnings: $($report.Summary.Warnings.Count)" -ForegroundColor Yellow

if ($report.Summary.Warnings.Count -gt 0) {
    Write-Host "`nWarnings:" -ForegroundColor Yellow
    foreach ($warning in $report.Summary.Warnings) {
        Write-Host "  - $warning" -ForegroundColor Yellow
    }
}

Write-Host "`n========================================`n" -ForegroundColor Cyan

# Save report to file
$reportJson = $report | ConvertTo-Json -Depth 10
$reportFile = "configuration-tasks-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$reportJson | Out-File $reportFile -Encoding UTF8

Write-Host "Detailed report saved to: $reportFile" -ForegroundColor Cyan

# Also save a readable text report
$textReport = @"
=================================================
COMPLAINT MANAGEMENT SYSTEM - CONFIGURATION TASKS
COMPLETION REPORT
=================================================
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

SUMMARY
-------
Total Tasks: $($report.Summary.TotalTasks)
Successful: $($report.Summary.Successful)
Failed: $($report.Summary.Failed)
Warnings: $($report.Summary.Warnings.Count)

TASK DETAILS
------------
$($report.TaskResults | ForEach-Object { @"

Task: $($_.TaskName)
Status: $($_.Status)
Timestamp: $($_.Timestamp)
Details:
$($_.Details)
----------------------------------------
"@ } | Out-String)

WARNINGS
--------
$($report.Summary.Warnings | ForEach-Object { "- $_" } | Out-String)

=================================================
END OF REPORT
=================================================
"@

$textReportFile = "configuration-tasks-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
$textReport | Out-File $textReportFile -Encoding UTF8

Write-Host "Text report saved to: $textReportFile" -ForegroundColor Cyan
Write-Host "`nConfiguration tasks completed!`n" -ForegroundColor Green
