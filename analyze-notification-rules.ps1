# Analyze the 22 existing notification rules

$token = Get-Content ".working-token" -Raw

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Analyzing 22 Notification Rules" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/event-communication-rules" `
        -Method GET `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json"

    if ($response.isSuccess) {
        Write-Host "`nTotal Rules: $($response.data.Count)" -ForegroundColor Green

        Write-Host "`nDetailed Rule Information:" -ForegroundColor Yellow
        $counter = 1
        foreach ($rule in $response.data) {
            Write-Host "`n[$counter] $($rule.ruleName)" -ForegroundColor Cyan
            Write-Host "    ID: $($rule.id)" -ForegroundColor Gray
            Write-Host "    EventTypeId: $($rule.eventTypeId)" -ForegroundColor White
            Write-Host "    EventType Code: $($rule.eventType.code)" -ForegroundColor Yellow
            Write-Host "    TemplateId: $($rule.templateId)" -ForegroundColor White
            Write-Host "    Template Name: $($rule.template.name)" -ForegroundColor Yellow
            Write-Host "    RecipientType: $($rule.recipientType)" -ForegroundColor White
            Write-Host "    Priority: $($rule.priority)" -ForegroundColor White
            Write-Host "    IsActive: $($rule.isActive)" -ForegroundColor $(if ($rule.isActive) { "Green" } else { "Red" })
            $counter++
        }

        # Save full details to JSON
        $response.data | ConvertTo-Json -Depth 10 | Out-File "notification-rules-full-details.json"
        Write-Host "`n================================================" -ForegroundColor Cyan
        Write-Host "Full details saved to: notification-rules-full-details.json" -ForegroundColor Green
        Write-Host "================================================" -ForegroundColor Cyan

    } else {
        Write-Host "Failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
