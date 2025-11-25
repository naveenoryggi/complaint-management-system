# Check Email Configuration via API

$tokenFile = ".working-token"
if (Test-Path $tokenFile) {
    $token = Get-Content $tokenFile -Raw
    Write-Host "Using existing token"

    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/email-configuration" `
            -Method GET `
            -Headers @{Authorization="Bearer $token"} `
            -ContentType "application/json"

        Write-Host "`nEmail Configuration Response:"
        $response | ConvertTo-Json -Depth 5
    }
    catch {
        Write-Host "Error: $_" -ForegroundColor Red
        Write-Host $_.Exception.Response.StatusCode
    }
}
else {
    Write-Host "No token found - need to login first" -ForegroundColor Yellow
}
