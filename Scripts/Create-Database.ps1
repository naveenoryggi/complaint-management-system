# Create-Database.ps1
# Database creation and setup script for Complaint Management System

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
    [switch]$WindowsAuth
)

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Database Setup Script                    " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Server: $Server"
Write-Host "Database: $Database"
Write-Host "Authentication: $(if ($WindowsAuth) { 'Windows' } else { 'SQL Server' })"
Write-Host ""

# Build connection string for master database
if ($WindowsAuth) {
    $masterConnStr = "Server=$Server;Database=master;Integrated Security=True;TrustServerCertificate=True;"
    $dbConnStr = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
} else {
    $masterConnStr = "Server=$Server;Database=master;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
    $dbConnStr = "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
}

try {
    Write-Host "Connecting to SQL Server..." -ForegroundColor Yellow

    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = $masterConnStr
    $conn.Open()

    Write-Host "[OK] Connected to SQL Server" -ForegroundColor Green

    # Check if database exists
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$Database'"
    $exists = $cmd.ExecuteScalar()

    if ($exists -eq 0) {
        Write-Host "Creating database '$Database'..." -ForegroundColor Yellow

        # First, get the default data path for SQL Server
        $cmd.CommandText = "SELECT SERVERPROPERTY('InstanceDefaultDataPath')"
        $dataPath = $cmd.ExecuteScalar()

        if ($dataPath) {
            $mdfPath = Join-Path $dataPath "$Database.mdf"
            $ldfPath = Join-Path $dataPath "${Database}_log.ldf"

            # Check if orphaned files exist and remove them
            if ((Test-Path $mdfPath) -or (Test-Path $ldfPath)) {
                Write-Host "[WARN] Found orphaned database files. Cleaning up..." -ForegroundColor Yellow

                # Try to delete orphaned files
                try {
                    if (Test-Path $mdfPath) {
                        Remove-Item $mdfPath -Force -ErrorAction Stop
                        Write-Host "  Removed: $mdfPath" -ForegroundColor Gray
                    }
                    if (Test-Path $ldfPath) {
                        Remove-Item $ldfPath -Force -ErrorAction Stop
                        Write-Host "  Removed: $ldfPath" -ForegroundColor Gray
                    }
                    Write-Host "[OK] Orphaned files cleaned up" -ForegroundColor Green
                } catch {
                    Write-Host "[WARN] Could not remove orphaned files: $($_.Exception.Message)" -ForegroundColor Yellow
                    Write-Host "[INFO] Trying to attach existing database instead..." -ForegroundColor Yellow

                    # Try to attach the existing database files
                    try {
                        $attachSql = @"
CREATE DATABASE [$Database] ON
(FILENAME = '$mdfPath'),
(FILENAME = '$ldfPath')
FOR ATTACH;
"@
                        $cmd.CommandText = $attachSql
                        $cmd.ExecuteNonQuery() | Out-Null
                        Write-Host "[OK] Attached existing database files" -ForegroundColor Green
                        $conn.Close()
                        exit 0
                    } catch {
                        Write-Host "[ERROR] Could not attach database: $($_.Exception.Message)" -ForegroundColor Red
                        Write-Host "[INFO] Please manually delete these files and retry:" -ForegroundColor Yellow
                        Write-Host "  $mdfPath" -ForegroundColor Gray
                        Write-Host "  $ldfPath" -ForegroundColor Gray
                        $conn.Close()
                        exit 1
                    }
                }
            }
        }

        # Create the database
        $cmd.CommandText = "CREATE DATABASE [$Database]"
        $cmd.ExecuteNonQuery() | Out-Null

        Write-Host "[OK] Database created successfully" -ForegroundColor Green
    } else {
        Write-Host "[SKIP] Database '$Database' already exists" -ForegroundColor Yellow
    }

    $conn.Close()

    Write-Host ""
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "  Database setup complete!                 " -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green

    exit 0

} catch {
    Write-Host "[ERROR] $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting tips:" -ForegroundColor Yellow
    Write-Host "  1. Ensure SQL Server is running" -ForegroundColor Gray
    Write-Host "  2. Verify the server name is correct" -ForegroundColor Gray
    Write-Host "  3. Check your authentication credentials" -ForegroundColor Gray
    Write-Host "  4. If files already exist, manually delete them from SQL data folder" -ForegroundColor Gray
    exit 1
}
