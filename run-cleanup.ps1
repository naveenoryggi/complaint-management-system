# ============================================================================
# EXECUTE DATABASE CLEANUP SCRIPT
# Removes all fake/test GUIDs from the database
# ============================================================================

Write-Host "=== Database Cleanup Script ===" -ForegroundColor Cyan
Write-Host ""

# Get connection string from appsettings.json
$appsettingsPath = ".\complaint-system-dotnet\src\ComplaintManagement.API\appsettings.json"
$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
$connectionString = $appsettings.ConnectionStrings.DefaultConnection

Write-Host "Connection: $($connectionString.Substring(0, 50))..." -ForegroundColor Gray
Write-Host ""

# Execute the cleanup SQL script
$sqlFile = ".\cleanup-fake-data.sql"

Write-Host "Executing cleanup script..." -ForegroundColor Yellow

try {
    # Use Invoke-Sqlcmd if available (requires SQL Server module)
    if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
        Invoke-Sqlcmd -ConnectionString $connectionString -InputFile $sqlFile -Verbose
        Write-Host ""
        Write-Host "✅ Cleanup completed successfully!" -ForegroundColor Green
    }
    else {
        # Fallback: Use sqlcmd command line tool
        Write-Host "Using sqlcmd..." -ForegroundColor Gray

        # Parse connection string to get server and database
        if ($connectionString -match "Server=([^;]+).*Database=([^;]+)") {
            $server = $matches[1]
            $database = $matches[2]

            # Check if using Windows Authentication or SQL Auth
            if ($connectionString -match "Integrated Security=true") {
                sqlcmd -S $server -d $database -E -i $sqlFile
            }
            else {
                # Extract username and password
                if ($connectionString -match "User Id=([^;]+).*Password=([^;]+)") {
                    $userId = $matches[1]
                    $password = $matches[2]
                    sqlcmd -S $server -d $database -U $userId -P $password -i $sqlFile
                }
            }

            if ($LASTEXITCODE -eq 0) {
                Write-Host ""
                Write-Host "✅ Cleanup completed successfully!" -ForegroundColor Green
            }
            else {
                Write-Host ""
                Write-Host "❌ Cleanup failed with exit code: $LASTEXITCODE" -ForegroundColor Red
            }
        }
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Error executing cleanup: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative: Run cleanup-fake-data.sql manually in SQL Server Management Studio" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Refresh your browser (F5)" -ForegroundColor White
Write-Host "2. Dashboard will automatically load with all 11 real status widgets" -ForegroundColor White
Write-Host ""
