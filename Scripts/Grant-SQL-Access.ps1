# Grant SQL Server access to IIS App Pool identity
$serverInstance = "LAPTOP-NF9BTG7Q\SQLEXPRESS"
$databaseName = "ComplaintManagementDB"
$appPoolIdentity = "IIS APPPOOL\ComplaintManagementAPIPool"

Write-Host "Granting SQL Server access to: $appPoolIdentity" -ForegroundColor Yellow
Write-Host "Server: $serverInstance" -ForegroundColor Cyan
Write-Host "Database: $databaseName" -ForegroundColor Cyan

$sql = @"
-- Create login for IIS App Pool if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'$appPoolIdentity')
BEGIN
    CREATE LOGIN [$appPoolIdentity] FROM WINDOWS WITH DEFAULT_DATABASE=[$databaseName]
    PRINT 'Created login: $appPoolIdentity'
END
ELSE
BEGIN
    PRINT 'Login already exists: $appPoolIdentity'
END

-- Switch to the database
USE [$databaseName]

-- Create user if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$appPoolIdentity')
BEGIN
    CREATE USER [$appPoolIdentity] FOR LOGIN [$appPoolIdentity]
    PRINT 'Created user: $appPoolIdentity'
END
ELSE
BEGIN
    PRINT 'User already exists: $appPoolIdentity'
END

-- Grant db_owner role (full access to the database)
ALTER ROLE db_owner ADD MEMBER [$appPoolIdentity]
PRINT 'Granted db_owner role to: $appPoolIdentity'
"@

# Save the SQL to a temp file
$sqlFile = "C:\Users\Navin Chandra\Pictures\Complaint management system\grant-sql-access.sql"
$sql | Out-File -FilePath $sqlFile -Encoding UTF8

Write-Host ""
Write-Host "Executing SQL script..." -ForegroundColor Yellow

# Execute using sqlcmd
$result = & sqlcmd -S $serverInstance -d "master" -i $sqlFile -E 2>&1
Write-Host $result

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "SQL access granted successfully!" -ForegroundColor Green

    # Recycle the app pool
    Write-Host ""
    Write-Host "Recycling IIS App Pool..." -ForegroundColor Yellow
    & "C:\Windows\System32\inetsrv\appcmd.exe" recycle apppool /apppool.name:"ComplaintManagementAPIPool"

    Write-Host ""
    Write-Host "Done! Please test the API at http://localhost:11020/api/auth/login" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Failed to grant SQL access. Error code: $LASTEXITCODE" -ForegroundColor Red
}
