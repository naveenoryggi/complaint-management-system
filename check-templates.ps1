# Check if templates exist in database via API

$token = Get-Content .working-token -Raw -ErrorAction SilentlyContinue
if (-not $token) {
    Write-Host "No token found, logging in..." -ForegroundColor Yellow
    $loginBody = '{"email":"admin@complaintmanagement.com","password":"Admin@123"}'
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/templates" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
}

$headers = @{
    "Authorization" = "Bearer $token"
}

Write-Host "`nChecking templates in database..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/templates" -Headers $headers

    if ($response.isSuccess) {
        Write-Host "✅ API call successful" -ForegroundColor Green
        Write-Host "Total templates: $($response.data.Count)" -ForegroundColor Cyan

        if ($response.data.Count -gt 0) {
            Write-Host "`nTemplates found:" -ForegroundColor Green
            $response.data | Select-Object name, code, isActive | Format-Table -AutoSize

            # Check for AUTO_ACK template
            $autoAck = $response.data | Where-Object { $_.code -eq "AUTO_ACK_NEW_TICKET" }
            if ($autoAck) {
                Write-Host "`n✅ AUTO_ACK_NEW_TICKET template found!" -ForegroundColor Green
                Write-Host "Template ID: $($autoAck.id)" -ForegroundColor Cyan
                Write-Host "Name: $($autoAck.name)" -ForegroundColor Gray

                # Save template ID to file
                $autoAck.id | Out-File -FilePath ".template-id.txt" -NoNewline
                Write-Host "`n📋 Template ID saved to .template-id.txt" -ForegroundColor Cyan
            } else {
                Write-Host "`n⚠️  AUTO_ACK_NEW_TICKET template not found" -ForegroundColor Yellow
            }
        } else {
            Write-Host "`n⚠️  No templates found in database" -ForegroundColor Yellow
            Write-Host "You need to run the SQL script to insert templates" -ForegroundColor Yellow
        }
    } else {
        Write-Host "❌ API returned error: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error calling API: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
