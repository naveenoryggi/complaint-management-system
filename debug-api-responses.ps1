# Debug API Responses
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "=== ROLES ===" -ForegroundColor Cyan
try {
    $roles = Invoke-RestMethod -Uri "http://localhost:5000/api/roles" -Method Get -Headers $headers
    Write-Host "Type: $($roles.GetType().Name)"
    Write-Host "Count: $($roles.Count)"
    $roles | ConvertTo-Json -Depth 3 | Write-Host
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host "`n=== USERS ===" -ForegroundColor Cyan
try {
    $users = Invoke-RestMethod -Uri "http://localhost:5000/api/users" -Method Get -Headers $headers
    Write-Host "Type: $($users.GetType().Name)"
    Write-Host "Count: $($users.Count)"
    $users | ConvertTo-Json -Depth 3 | Write-Host
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host "`n=== PRIORITY MASTERS ===" -ForegroundColor Cyan
try {
    $priorities = Invoke-RestMethod -Uri "http://localhost:5000/api/ComplaintPriorityMaster" -Method Get -Headers $headers
    Write-Host "Type: $($priorities.GetType().Name)"
    if ($priorities.PSObject.Properties.Name -contains 'Count') {
        Write-Host "Count: $($priorities.Count)"
    }
    $priorities | ConvertTo-Json -Depth 3 | Write-Host
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host "`n=== CATEGORIES ===" -ForegroundColor Cyan
try {
    $categories = Invoke-RestMethod -Uri "http://localhost:5000/api/categories" -Method Get -Headers $headers
    Write-Host "Type: $($categories.GetType().Name)"
    if ($categories.PSObject.Properties.Name -contains 'Count') {
        Write-Host "Count: $($categories.Count)"
    }
    $categories | ConvertTo-Json -Depth 3 | Write-Host
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
