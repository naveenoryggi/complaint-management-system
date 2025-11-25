# Check Existing Notification Rules

$token = Get-Content ".working-token" -Raw
Write-Host "Checking notification rules..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/event-communication-rules" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($response.isSuccess) {
        $ruleCount = $response.data.Count
        Write-Host "`nTotal Notification Rules: $ruleCount" -ForegroundColor Green

        if ($ruleCount -gt 0) {
            Write-Host "`nNotification Rules Summary:" -ForegroundColor Yellow
            $response.data | Select-Object Id, RuleName, EventType, IsActive, Priority | Format-Table -AutoSize

            # Group by event type
            Write-Host "`nRules by Event Type:" -ForegroundColor Yellow
            $response.data | Group-Object EventType | Select-Object Name, Count | Format-Table -AutoSize
        }
        else {
            Write-Host "No notification rules found" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Failed: $($response.message)" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Check templates
Write-Host "`n=== Checking Templates ===" -ForegroundColor Cyan
try {
    $templatesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/communication-templates" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($templatesResponse.isSuccess) {
        $templateCount = $templatesResponse.data.Count
        Write-Host "Total Templates: $templateCount" -ForegroundColor Green

        if ($templateCount -gt 0) {
            Write-Host "`nTemplates Summary:" -ForegroundColor Yellow
            $templatesResponse.data | Select-Object Id, Name, TemplateType, IsActive | Format-Table -AutoSize
        }
    }
}
catch {
    Write-Host "Error checking templates: $_" -ForegroundColor Red
}
