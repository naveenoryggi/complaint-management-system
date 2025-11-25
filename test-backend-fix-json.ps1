# Test backend fix for Unknown Unknown bug - RAW JSON
$token = Get-Content '.fresh-token' -Raw
$token = $token.Trim()
$headers = @{
    'Authorization' = "Bearer $token"
}

Write-Host "Testing complaint CMP-2025-1110 (ID: dc5f95da-92d1-40f9-8ed3-1b91f0b70c34)..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5058/api/complaints/dc5f95da-92d1-40f9-8ed3-1b91f0b70c34' -Headers $headers -Method Get

    Write-Host "`nRAW JSON Response:" -ForegroundColor Yellow
    $response | ConvertTo-Json -Depth 5

    Write-Host "`n`nChecking Status and Priority fields:" -ForegroundColor Cyan

    # Try different property access patterns
    if ($response.PSObject.Properties['status']) {
        Write-Host "Status property exists: $($response.status)"
    } else {
        Write-Host "Status property NOT found"
    }

    if ($response.PSObject.Properties['priority']) {
        Write-Host "Priority property exists: $($response.priority)"
    } else {
        Write-Host "Priority property NOT found"
    }

    # List all properties
    Write-Host "`nAll properties:" -ForegroundColor Yellow
    $response.PSObject.Properties | ForEach-Object { Write-Host "  $($_.Name): $($_.Value)" }

} catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $reader.DiscardBufferedData()
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}
