$token = Get-Content ".oauth-fix-token" -Raw | ForEach-Object { $_.Trim() }
$headers = @{
    "Authorization" = "Bearer $token"
}
$configId = "4a1b41ef-cbc5-4858-a6a5-02b1c147a80a"

Write-Host "Getting OAuth authorization URL..." -ForegroundColor Cyan

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/oauth/authorize/$configId" `
        -Headers $headers `
        -Method Get `
        -MaximumRedirection 0 `
        -ErrorAction SilentlyContinue

    Write-Host "`nResponse Status: $($response.StatusCode)" -ForegroundColor Yellow

    if ($response.Headers.Location) {
        Write-Host "`nOAuth Authorization URL:" -ForegroundColor Green
        Write-Host $response.Headers.Location -ForegroundColor Cyan
        $response.Headers.Location | Out-File -FilePath ".oauth-url.txt" -Encoding UTF8
        Write-Host "`nURL saved to .oauth-url.txt" -ForegroundColor Green
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "Status Code: $statusCode" -ForegroundColor Yellow

        if ($statusCode -eq 302 -or $statusCode -eq 307) {
            $location = $_.Exception.Response.Headers.Location
            Write-Host "`nOAuth Authorization URL:" -ForegroundColor Green
            Write-Host $location -ForegroundColor Cyan
            $location | Out-File -FilePath ".oauth-url.txt" -Encoding UTF8
            Write-Host "`nURL saved to .oauth-url.txt" -ForegroundColor Green
        }
    }
}
