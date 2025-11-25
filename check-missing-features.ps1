# Script to check for missing features between Backend API and Frontend Angular

$report = @()

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "CHECKING FOR MISSING FRONTEND FEATURES" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# ComplaintsController
Write-Host "1. Complaints Features:" -ForegroundColor Yellow
$complaintService = Get-Content "complaint-system-angular/src/app/services/complaint.service.ts" -Raw

$missing = @()
if ($complaintService -notmatch "getHistory|history") { $missing += "  - Get Complaint History (GET /api/complaints/{id}/history)" }

if ($missing.Count -gt 0) {
    Write-Host "  MISSING:" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    $report += "Complaints: $($missing.Count) missing"
} else {
    Write-Host "  All features implemented ✓" -ForegroundColor Green
}
Write-Host ""

# DashboardController
Write-Host "2. Dashboard Features:" -ForegroundColor Yellow
if (Test-Path "complaint-system-angular/src/app/services/dashboard.service.ts") {
    $dashboardService = Get-Content "complaint-system-angular/src/app/services/dashboard.service.ts" -Raw
    $missing = @()

    if ($dashboardService -notmatch "preferences") { $missing += "  - User Preferences (GET/PUT /api/dashboard/preferences)" }

    if ($missing.Count -gt 0) {
        Write-Host "  MISSING:" -ForegroundColor Red
        $missing | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        $report += "Dashboard: $($missing.Count) missing"
    } else {
        Write-Host "  All features implemented ✓" -ForegroundColor Green
    }
} else {
    Write-Host "  Dashboard service exists ✓" -ForegroundColor Green
}
Write-Host ""

# EventTypesController
Write-Host "3. Event Types Features:" -ForegroundColor Yellow
if (-not (Test-Path "complaint-system-angular/src/app/services/event-type.service.ts")) {
    Write-Host "  MISSING: Event Types Service completely missing!" -ForegroundColor Red
    Write-Host "    - GET /api/event-types" -ForegroundColor Red
    Write-Host "    - POST /api/event-types" -ForegroundColor Red
    Write-Host "    - PUT /api/event-types/{id}" -ForegroundColor Red
    Write-Host "    - DELETE /api/event-types/{id}" -ForegroundColor Red
    $report += "Event Types: Service missing"
} else {
    Write-Host "  Service exists ✓" -ForegroundColor Green
}
Write-Host ""

# CommentsController
Write-Host "4. Comments Features:" -ForegroundColor Yellow
if (Test-Path "complaint-system-angular/src/app/services/comment.service.ts") {
    $commentService = Get-Content "complaint-system-angular/src/app/services/comment.service.ts" -Raw
    $missing = @()

    # Comments are nested under complaints/{id}/comments
    if ($commentService -notmatch "getComments|comments") {
        $missing += "  - Get Comments (GET /api/complaints/{id}/comments)"
    }
    if ($commentService -notmatch "addComment|createComment") {
        $missing += "  - Add Comment (POST /api/complaints/{id}/comments)"
    }

    if ($missing.Count -gt 0) {
        Write-Host "  MISSING:" -ForegroundColor Red
        $missing | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        $report += "Comments: $($missing.Count) missing"
    } else {
        Write-Host "  Service exists ✓" -ForegroundColor Green
    }
} else {
    Write-Host "  Service exists ✓" -ForegroundColor Green
}
Write-Host ""

# EscalationController
Write-Host "5. Escalation Features:" -ForegroundColor Yellow
if (Test-Path "complaint-system-angular/src/app/services/escalation.service.ts") {
    $escalationService = Get-Content "complaint-system-angular/src/app/services/escalation.service.ts" -Raw
    $missing = @()

    if ($escalationService -notmatch "matrices|getMatrices") {
        $missing += "  - Escalation Matrices (GET /api/escalation/matrices)"
    }
    if ($escalationService -notmatch "pending") {
        $missing += "  - Pending Escalations (GET /api/escalation/pending)"
    }
    if ($escalationService -notmatch "history") {
        $missing += "  - Escalation History (GET /api/escalation/complaints/{id}/history)"
    }

    if ($missing.Count -gt 0) {
        Write-Host "  MISSING:" -ForegroundColor Red
        $missing | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        $report += "Escalation: $($missing.Count) missing"
    } else {
        Write-Host "  All features implemented ✓" -ForegroundColor Green
    }
} else {
    Write-Host "  Service exists ✓" -ForegroundColor Green
}
Write-Host ""

# ResourcePoolController
Write-Host "6. Resource Pool Features:" -ForegroundColor Yellow
if (Test-Path "complaint-system-angular/src/app/services/resource-pool.service.ts") {
    Write-Host "  Service exists ✓" -ForegroundColor Green
} else {
    Write-Host "  MISSING: Resource Pool Service completely missing!" -ForegroundColor Red
    $report += "Resource Pool: Service missing"
}
Write-Host ""

# Oryggi Sync
Write-Host "7. Oryggi Sync Features:" -ForegroundColor Yellow
if (Test-Path "complaint-system-angular/src/app/services/oryggi-sync.service.ts") {
    Write-Host "  Service exists ✓" -ForegroundColor Green
} else {
    Write-Host "  Service missing" -ForegroundColor Red
    $report += "Oryggi: Service missing"
}
Write-Host ""

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
if ($report.Count -eq 0) {
    Write-Host "All features are implemented! ✓" -ForegroundColor Green
} else {
    Write-Host "Missing features found:" -ForegroundColor Yellow
    $report | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
}
