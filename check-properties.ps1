$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "=== PRIORITY MASTERS ===" -ForegroundColor Cyan
$prioritiesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/ComplaintPriorityMaster" -Method Get -Headers $headers
if ($prioritiesResponse.data) {
    $priorities = $prioritiesResponse.data
} else {
    $priorities = $prioritiesResponse
}

Write-Host "Count: $($priorities.Count)"
Write-Host "First priority properties:"
$priorities[0] | Format-List *

Write-Host "`n=== CATEGORIES ===" -ForegroundColor Cyan
$categoriesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/categories" -Method Get -Headers $headers
if ($categoriesResponse.data) {
    $categories = $categoriesResponse.data
} else {
    $categories = $categoriesResponse
}

Write-Host "Count: $($categories.Count)"
Write-Host "First category properties:"
$categories[0] | Format-List *
