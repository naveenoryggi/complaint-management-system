# Fix-Schema.ps1
# Ensures all required columns exist in the database after migrations

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
Write-Host "  Schema Fix Script                        " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Server: $Server"
Write-Host "Database: $Database"
Write-Host ""

# Build connection string
if ($WindowsAuth) {
    $connStr = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
} else {
    $connStr = "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
}

try {
    Write-Host "Connecting to database..." -ForegroundColor Yellow

    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = $connStr
    $conn.Open()

    Write-Host "[OK] Connected to database" -ForegroundColor Green

    # SQL to add missing columns if they don't exist
    $sql = @"
    -- Add ManagerId column if not exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Companies' AND COLUMN_NAME = 'ManagerId')
    BEGIN
        ALTER TABLE Companies ADD ManagerId uniqueidentifier NULL;
        PRINT 'Added ManagerId column';
    END

    -- Add SecondaryManagerId column if not exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Companies' AND COLUMN_NAME = 'SecondaryManagerId')
    BEGIN
        ALTER TABLE Companies ADD SecondaryManagerId uniqueidentifier NULL;
        PRINT 'Added SecondaryManagerId column';
    END

    -- Add HrResponsibleId column if not exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Companies' AND COLUMN_NAME = 'HrResponsibleId')
    BEGIN
        ALTER TABLE Companies ADD HrResponsibleId uniqueidentifier NULL;
        PRINT 'Added HrResponsibleId column';
    END

    PRINT 'Schema check complete';
"@

    Write-Host "Checking and fixing schema..." -ForegroundColor Yellow

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    $cmd.ExecuteNonQuery() | Out-Null

    Write-Host "[OK] Schema verified and fixed" -ForegroundColor Green

    $conn.Close()

    Write-Host ""
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "  Schema fix complete!                     " -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green

    exit 0

} catch {
    Write-Host "[ERROR] $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
