# Run-DatabaseMigration.ps1
# Executes the EF Core migration SQL script to create all database tables
param(
    [Parameter(Mandatory=$true)]
    [string]$Server,

    [Parameter(Mandatory=$true)]
    [string]$Database,

    [Parameter(Mandatory=$false)]
    [string]$Username,

    [Parameter(Mandatory=$false)]
    [string]$Password,

    [Parameter(Mandatory=$false)]
    [switch]$WindowsAuth,

    [Parameter(Mandatory=$true)]
    [string]$ScriptPath
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Running Database Migration Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Server: $Server" -ForegroundColor Gray
Write-Host "Database: $Database" -ForegroundColor Gray
Write-Host "Script: $ScriptPath" -ForegroundColor Gray
Write-Host ""

# Build connection string
if ($WindowsAuth) {
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=60;"
    Write-Host "Using Windows Authentication" -ForegroundColor Gray
} else {
    $connectionString = "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=True;Connection Timeout=60;"
    Write-Host "Using SQL Server Authentication (User: $Username)" -ForegroundColor Gray
}

try {
    # Check if script file exists
    if (-not (Test-Path $ScriptPath)) {
        Write-Host "ERROR: Migration script not found at: $ScriptPath" -ForegroundColor Red
        exit 1
    }

    Write-Host ""
    Write-Host "Reading migration script..." -ForegroundColor Yellow
    $script = Get-Content $ScriptPath -Raw

    if ([string]::IsNullOrWhiteSpace($script)) {
        Write-Host "ERROR: Migration script is empty!" -ForegroundColor Red
        exit 1
    }

    Write-Host "Script size: $($script.Length) characters" -ForegroundColor Gray

    # Connect to database
    Write-Host ""
    Write-Host "Connecting to database..." -ForegroundColor Yellow
    $conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $conn.Open()
    Write-Host "Connected successfully!" -ForegroundColor Green

    # Split script by GO statements and execute each batch
    Write-Host ""
    Write-Host "Executing migration batches..." -ForegroundColor Yellow

    # Split by GO statements (case insensitive, on its own line)
    $batches = $script -split '(?m)^\s*GO\s*$'

    $batchCount = 0
    $successCount = 0
    $errorCount = 0

    foreach ($batch in $batches) {
        $batch = $batch.Trim()
        if ($batch -ne '' -and $batch.Length -gt 5) {
            $batchCount++
            try {
                $cmd = $conn.CreateCommand()
                $cmd.CommandTimeout = 300  # 5 minute timeout per batch
                $cmd.CommandText = $batch
                $result = $cmd.ExecuteNonQuery()
                $successCount++

                # Show progress every 10 batches
                if ($batchCount % 10 -eq 0) {
                    Write-Host "  Executed $batchCount batches..." -ForegroundColor Gray
                }
            } catch {
                $errorCount++
                $errorMsg = $_.Exception.Message

                # Skip "already exists" type errors (idempotent script)
                if ($errorMsg -like "*already exists*" -or
                    $errorMsg -like "*already an object*" -or
                    $errorMsg -like "*duplicate key*") {
                    Write-Host "  [SKIP] Object already exists (batch $batchCount)" -ForegroundColor DarkYellow
                    $successCount++
                    $errorCount--
                } else {
                    Write-Host "  [ERROR] Batch $batchCount`: $errorMsg" -ForegroundColor Red
                    # Show first 200 chars of problematic batch for debugging
                    $batchPreview = if ($batch.Length -gt 200) { $batch.Substring(0, 200) + "..." } else { $batch }
                    Write-Host "  Batch preview: $batchPreview" -ForegroundColor DarkGray
                }
            }
        }
    }

    $conn.Close()

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Migration Complete!" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Total batches processed: $batchCount" -ForegroundColor White
    Write-Host "Successful: $successCount" -ForegroundColor Green
    if ($errorCount -gt 0) {
        Write-Host "Errors: $errorCount" -ForegroundColor Red
    }

    # Verify tables were created
    Write-Host ""
    Write-Host "Verifying database tables..." -ForegroundColor Yellow

    $conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $conn.Open()

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
    $tableCount = $cmd.ExecuteScalar()

    Write-Host "Total tables in database: $tableCount" -ForegroundColor Cyan

    if ($tableCount -ge 50) {
        Write-Host "Database schema created successfully!" -ForegroundColor Green
    } else {
        Write-Host "WARNING: Expected 50+ tables, found $tableCount. Migration may be incomplete." -ForegroundColor Yellow
    }

    $conn.Close()

    Write-Host ""
    Write-Host "Press any key to continue..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

    exit 0

} catch {
    Write-Host ""
    Write-Host "FATAL ERROR during migration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check:" -ForegroundColor Yellow
    Write-Host "  1. SQL Server is running" -ForegroundColor White
    Write-Host "  2. Database '$Database' exists" -ForegroundColor White
    Write-Host "  3. Connection credentials are correct" -ForegroundColor White
    Write-Host ""
    Write-Host "Press any key to continue..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}
