# Complete Template System Setup - One Script Does Everything
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "COMPLETE TEMPLATE SYSTEM SETUP" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Kill old processes
Write-Host "Step 1: Cleaning up old processes..." -ForegroundColor Yellow
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process node -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "Done`n" -ForegroundColor Green

# Step 2: Start Backend
Write-Host "Step 2: Starting backend..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.API'; dotnet run" -WindowStyle Minimized
Start-Sleep -Seconds 15
Write-Host "Done`n" -ForegroundColor Green

# Step 3: Start Frontend
Write-Host "Step 3: Starting frontend..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-angular'; npm start" -WindowStyle Minimized
Start-Sleep -Seconds 10
Write-Host "Done`n" -ForegroundColor Green

# Step 4: Get Auth Token
Write-Host "Step 4: Getting authentication token..." -ForegroundColor Yellow
$loginBody = '{"email":"admin@complaintmanagement.com","password":"Admin@123"}'
try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -TimeoutSec 30
    $token = $loginResponse.data.token
    $token | Out-File -FilePath ".working-token" -NoNewline
    Write-Host "Token obtained successfully" -ForegroundColor Green
} catch {
    Write-Host "Waiting for backend to fully start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    $token | Out-File -FilePath ".working-token" -NoNewline
    Write-Host "Token obtained successfully (retry)" -ForegroundColor Green
}
Write-Host "Done`n" -ForegroundColor Green

# Step 5: Get Company ID
Write-Host "Step 5: Getting company ID..." -ForegroundColor Yellow
$headers = @{"Authorization" = "Bearer $token"}
$companies = Invoke-RestMethod -Uri "http://localhost:5000/api/companies" -Headers $headers
$companyId = $companies.data[0].id
Write-Host "Company ID: $companyId" -ForegroundColor Green
Write-Host "Done`n" -ForegroundColor Green

# Step 6: Check if templates exist
Write-Host "Step 6: Checking templates..." -ForegroundColor Yellow
try {
    $templatesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/communication-templates" -Headers $headers
    $existingTemplates = $templatesResponse.data.Count
    Write-Host "Found $existingTemplates templates" -ForegroundColor Green
} catch {
    $existingTemplates = 0
    Write-Host "No templates found or endpoint not available" -ForegroundColor Yellow
}

# Step 7: Create templates if needed
if ($existingTemplates -eq 0) {
    Write-Host "`nStep 7: Creating templates..." -ForegroundColor Yellow

    $templates = Get-Content "templates-payload.json" -Raw | ConvertFrom-Json
    foreach ($template in $templates) {
        $template | Add-Member -NotePropertyName "companyId" -NotePropertyValue $companyId -Force
        $templateJson = $template | ConvertTo-Json -Depth 10 -Compress

        try {
            $response = Invoke-RestMethod -Uri "http://localhost:5000/api/communication-templates" -Method POST -Headers $headers -Body $templateJson -ContentType "application/json"
            Write-Host "  Created: $($template.code)" -ForegroundColor Green
        } catch {
            Write-Host "  Skip: $($template.code)" -ForegroundColor Gray
        }
    }
    Write-Host "Done`n" -ForegroundColor Green
} else {
    Write-Host "Templates already exist, skipping creation`n" -ForegroundColor Gray
}

# Step 8: Get AUTO_ACK template ID
Write-Host "Step 8: Getting AUTO_ACK template ID..." -ForegroundColor Yellow
$templatesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/communication-templates" -Headers $headers
$autoAckTemplate = $templatesResponse.data | Where-Object { $_.code -eq "AUTO_ACK_NEW_TICKET" }
if ($autoAckTemplate) {
    $templateId = $autoAckTemplate.id
    $templateId | Out-File -FilePath ".template-id.txt" -NoNewline
    Write-Host "Template ID: $templateId" -ForegroundColor Green
    Write-Host "Saved to .template-id.txt" -ForegroundColor Gray
} else {
    Write-Host "AUTO_ACK template not found!" -ForegroundColor Red
}
Write-Host "Done`n" -ForegroundColor Green

# Step 9: Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SETUP COMPLETE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nServers Running:" -ForegroundColor Yellow
Write-Host "  Backend: http://localhost:5000" -ForegroundColor White
Write-Host "  Frontend: http://localhost:4200" -ForegroundColor White
Write-Host "`nTemplate ID saved to: .template-id.txt" -ForegroundColor Yellow
Write-Host "Template ID: $templateId" -ForegroundColor White
Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "  1. Open browser: http://localhost:4200" -ForegroundColor White
Write-Host "  2. Go to: Admin Panel > Communication > Email Ticketing" -ForegroundColor White
Write-Host "  3. Click 'Add Email Configuration'" -ForegroundColor White
Write-Host "  4. Configure with OAuth and paste template ID" -ForegroundColor White
Write-Host "  5. Set polling to 120 seconds (2 minutes)" -ForegroundColor White
Write-Host "`nPress any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
