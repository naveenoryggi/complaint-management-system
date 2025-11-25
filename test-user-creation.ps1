$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get complainant role
$rolesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/roles" -Method Get -Headers $headers
if ($rolesResponse.data) {
    $roles = $rolesResponse.data
} else {
    $roles = $rolesResponse
}

$complainantRole = $roles | Where-Object { $_.name -like "*Complainant*" }
Write-Host "Complainant Role ID: $($complainantRole.id)"

# Try to create user
$newUser = @{
    email = "nav_nainital@yahoo.com"
    firstName = "Nav"
    lastName = "Nainital"
    employeeCode = "NAV001"
    password = "Nav@12345"
    roleIds = @($complainantRole.id)
    isActive = $true
}

Write-Host "`nRequest payload:"
$newUser | ConvertTo-Json | Write-Host

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/users" -Method Post -Headers $headers -Body ($newUser | ConvertTo-Json)
    Write-Host "`nSuccess!" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5 | Write-Host
} catch {
    Write-Host "`nError:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response body: $responseBody"
    }
}
