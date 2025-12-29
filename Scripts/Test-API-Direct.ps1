# Run API directly to see errors
Set-Location "C:\Program Files\ComplaintManagement\API"
Write-Host "Starting API directly to capture errors..." -ForegroundColor Yellow

# Set environment variables
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:DOTNET_NOLOGO = "1"

# Run the API and capture output
& ".\ComplaintManagement.API.exe" 2>&1 | ForEach-Object {
    Write-Host $_
}
