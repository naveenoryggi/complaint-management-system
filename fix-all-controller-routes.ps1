# Fix all controller routes to match test expectations

$fixes = @(
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\EmailServerSettingsController.cs"
        Old = '[Route("api/communication/email-settings")]'
        New = '[Route("api/email-settings")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\SmsGatewaySettingsController.cs"
        Old = '[Route("api/communication/sms-settings")]'
        New = '[Route("api/sms-gateway")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\WhatsAppSettingsController.cs"
        Old = '[Route("api/communication/whatsapp-settings")]'
        New = '[Route("api/whatsapp-settings")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\CommunicationTemplatesController.cs"
        Old = '[Route("api/communication/templates")]'
        New = '[Route("api/communication-templates")]'
    },
    @{
        File = "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\EventCommunicationRulesController.cs"
        Old = '[Route("api/communication/notification-rules")]'
        New = '[Route("api/event-communication-rules")]'
    }
)

foreach ($fix in $fixes) {
    $filePath = $fix.File
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw
        $content = $content -replace [regex]::Escape($fix.Old), $fix.New
        Set-Content -Path $filePath -Value $content -NoNewline
        Write-Host "Fixed: $filePath" -ForegroundColor Green
    } else {
        Write-Host "File not found: $filePath" -ForegroundColor Red
    }
}

Write-Host "`nAll controller routes fixed!" -ForegroundColor Cyan
