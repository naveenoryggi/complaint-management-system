$filePath = "complaint-system-dotnet/src/ComplaintManagement.Infrastructure/Data/Seed/DbSeeder.cs"
$content = Get-Content $filePath
$filtered = $content | Where-Object { $_ -notmatch '^\s+DefaultSlaHours\s*=' }
$filtered | Set-Content $filePath
Write-Host "Removed DefaultSlaHours lines from DbSeeder.cs"
