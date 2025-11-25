# Simple Template Creation via API

Write-Host "🚀 Creating Templates via API..." -ForegroundColor Cyan

# Get token
$token = if (Test-Path ".working-token") { Get-Content ".working-token" -Raw } else {
    $loginBody = @{ email = "admin@complaintmanagement.com"; password = "Admin@123" } | ConvertTo-Json
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $loginResponse.data.token
}

$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }

# Get company ID
$companies = Invoke-RestMethod -Uri "http://localhost:5000/api/companies" -Headers $headers
$companyId = $companies.data[0].id

Write-Host "✅ Company ID: $companyId" -ForegroundColor Green

# Load template JSON from file (we'll create a JSON file next)
$templatesJson = Get-Content "templates-payload.json" -Raw
$templates = $templatesJson | ConvertFrom-Json

$created = 0
foreach ($template in $templates) {
    $template.companyId = $companyId
    $templateJson = $template | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/templates" -Method POST -Headers $headers -Body $templateJson
        if ($response.isSuccess) {
            Write-Host "✅ Created: $($template.code) (ID: $($response.data.id))" -ForegroundColor Green
            if ($template.code -eq "AUTO_ACK_NEW_TICKET") {
                $response.data.id | Out-File -FilePath ".template-id.txt" -NoNewline
            }
            $created++
        }
    } catch {
        Write-Host "⚠️  Template $($template.code) might already exist" -ForegroundColor Yellow
    }
}

Write-Host "`n✅ Created $created templates" -ForegroundColor Green

# Get the AUTO_ACK template ID
$templatesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/templates" -Headers $headers
$autoAckTemplate = $templatesResponse.data | Where-Object { $_.code -eq "AUTO_ACK_NEW_TICKET" } | Select-Object -First 1

if ($autoAckTemplate) {
    $autoAckTemplate.id | Out-File -FilePath ".template-id.txt" -NoNewline
    Write-Host "`n📋 Template ID saved: $($autoAckTemplate.id)" -ForegroundColor Cyan
    Write-Host "   File: .template-id.txt" -ForegroundColor Gray
}
