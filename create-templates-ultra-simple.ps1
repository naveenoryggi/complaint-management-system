# Ultra Simple Template Creation

Write-Host "Creating Templates..." -ForegroundColor Cyan

# Get token
$token = Get-Content ".working-token" -Raw -ErrorAction SilentlyContinue
if (-not $token) {
    $loginBody = '{"email":"admin@complaintmanagement.com","password":"Admin@123"}'
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    $token | Out-File -FilePath ".working-token" -NoNewline
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get company ID
$companies = Invoke-RestMethod -Uri "http://localhost:5000/api/companies" -Headers $headers
$companyId = $companies.data[0].id

Write-Host "Company ID: $companyId" -ForegroundColor Green

# Load and create templates
$templatesJson = Get-Content "templates-payload.json" -Raw
$templates = $templatesJson | ConvertFrom-Json

foreach ($template in $templates) {
    $template | Add-Member -NotePropertyName "companyId" -NotePropertyValue $companyId -Force
    $templateJson = $template | ConvertTo-Json -Depth 10 -Compress

    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/templates" -Method POST -Headers $headers -Body $templateJson
        if ($response.isSuccess) {
            Write-Host "Created: $($template.code)" -ForegroundColor Green
            if ($template.code -eq "AUTO_ACK_NEW_TICKET") {
                $response.data.id | Out-File -FilePath ".template-id.txt" -NoNewline
                Write-Host "Template ID: $($response.data.id)" -ForegroundColor Cyan
            }
        }
    } catch {
        Write-Host "Template $($template.code) might already exist" -ForegroundColor Yellow
    }
}

# Ensure we have the template ID
$templatesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/templates" -Headers $headers
$autoAckTemplate = $templatesResponse.data | Where-Object { $_.code -eq "AUTO_ACK_NEW_TICKET" } | Select-Object -First 1

if ($autoAckTemplate) {
    $autoAckTemplate.id | Out-File -FilePath ".template-id.txt" -NoNewline
    Write-Host "Template ID saved: $($autoAckTemplate.id)" -ForegroundColor Cyan
}

Write-Host "Done!" -ForegroundColor Green
