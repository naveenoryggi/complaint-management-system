# List all controller routes

Get-ChildItem "complaint-system-dotnet\src\ComplaintManagement.API\Controllers\*.cs" | ForEach-Object {
    $file = $_.Name
    $content = Get-Content $_.FullName
    $routeLine = $content | Where-Object { $_ -match '\[Route\("api/' } | Select-Object -First 1
    $className = $content | Where-Object { $_ -match 'public class .* : ControllerBase' } | Select-Object -First 1

    if ($routeLine) {
        $route = ($routeLine -replace '.*\[Route\("([^"]+)"\)\].*', '$1').Trim()
        $class = ($className -replace '.*public class ([^ ]+).*', '$1').Trim()
        Write-Host "$class -> /$route" -ForegroundColor Cyan
    }
} | Sort-Object
