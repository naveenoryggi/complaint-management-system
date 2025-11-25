# Fix remaining controller routes to use lowercase/kebab-case

$fixes = @(
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\AuthController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/auth")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\BranchesController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/branches")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\CategoriesController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/categories")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\CompanyController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/company")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\ComplaintsController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/complaints")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\DashboardController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/dashboard")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\DepartmentsController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/departments")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\EscalationController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/escalation")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\SectionsController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/sections")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\UsersController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/users")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\EventTypesController.cs"
        Old = '[Route("api/communication/event-types")]'
        New = '[Route("api/event-types")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\ResourcePoolController.cs"
        Old = '[Route("api/escalation/resource-pools")]'
        New = '[Route("api/resource-pools")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\OryggiConnectionSettingsController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/oryggi-connection-settings")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\OryggiSyncController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/oryggi-sync")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\ComplaintInfoSettingsController.cs"
        Old = '[Route("api/[controller]")]'
        New = '[Route("api/complaint-info-settings")]'
    }
)

$fixedCount = 0
$notFoundCount = 0

foreach ($fix in $fixes) {
    $filePath = $fix.File
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw
        if ($content -match [regex]::Escape($fix.Old)) {
            $content = $content -replace [regex]::Escape($fix.Old), $fix.New
            Set-Content -Path $filePath -Value $content -NoNewline
            Write-Host "[OK] Fixed: $($filePath | Split-Path -Leaf)" -ForegroundColor Green
            $fixedCount++
        } else {
            Write-Host "[SKIP] Already fixed or pattern not found: $($filePath | Split-Path -Leaf)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "[NOT FOUND] File not found: $filePath" -ForegroundColor Red
        $notFoundCount++
    }
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Fixed: $fixedCount controllers" -ForegroundColor Green
Write-Host "Skipped: $($fixes.Count - $fixedCount - $notFoundCount) controllers" -ForegroundColor Yellow
Write-Host "Not found: $notFoundCount controllers" -ForegroundColor Red
Write-Host "=============================================" -ForegroundColor Cyan
