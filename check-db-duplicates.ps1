# Check for duplicates in ComplaintManagementDb
$connectionString = "Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected to ComplaintManagementDb successfully" -ForegroundColor Green
    Write-Host ""

    # Check for duplicate OryggiEmployeeId in Employees table
    Write-Host "=== DUPLICATE ORYGGIEMPLOYEEID IN EMPLOYEES TABLE ===" -ForegroundColor Yellow
    $cmd1 = $connection.CreateCommand()
    $cmd1.CommandText = @"
SELECT OryggiEmployeeId, COUNT(*) as DuplicateCount
FROM Employees
WHERE OryggiEmployeeId IS NOT NULL
GROUP BY OryggiEmployeeId
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC
"@
    $reader1 = $cmd1.ExecuteReader()
    $duplicateCount = 0
    while ($reader1.Read()) {
        Write-Host "OryggiEmployeeId: $($reader1['OryggiEmployeeId']) - Count: $($reader1['DuplicateCount'])" -ForegroundColor Red
        $duplicateCount++
    }
    $reader1.Close()
    if ($duplicateCount -eq 0) {
        Write-Host "No duplicates found in Employees table" -ForegroundColor Green
    } else {
        Write-Host "Found $duplicateCount duplicate OryggiEmployeeId values in Employees" -ForegroundColor Red
    }
    Write-Host ""

    # Check for duplicate OryggiEmployeeId in Users table
    Write-Host "=== DUPLICATE ORYGGIEMPLOYEEID IN USERS TABLE ===" -ForegroundColor Yellow
    $cmd2 = $connection.CreateCommand()
    $cmd2.CommandText = @"
SELECT OryggiEmployeeId, COUNT(*) as DuplicateCount
FROM Users
WHERE OryggiEmployeeId IS NOT NULL
GROUP BY OryggiEmployeeId
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC
"@
    $reader2 = $cmd2.ExecuteReader()
    $duplicateCount = 0
    while ($reader2.Read()) {
        Write-Host "OryggiEmployeeId: $($reader2['OryggiEmployeeId']) - Count: $($reader2['DuplicateCount'])" -ForegroundColor Red
        $duplicateCount++
    }
    $reader2.Close()
    if ($duplicateCount -eq 0) {
        Write-Host "No duplicates found in Users table" -ForegroundColor Green
    } else {
        Write-Host "Found $duplicateCount duplicate OryggiEmployeeId values in Users" -ForegroundColor Red
    }
    Write-Host ""

    # Check for NULL values and totals
    Write-Host "=== NULL VALUES AND TOTALS ===" -ForegroundColor Yellow
    $cmd3 = $connection.CreateCommand()
    $cmd3.CommandText = @"
SELECT 'Employees with NULL OryggiEmployeeId' as Info, COUNT(*) as Count
FROM Employees
WHERE OryggiEmployeeId IS NULL
UNION ALL
SELECT 'Users with NULL OryggiEmployeeId', COUNT(*)
FROM Users
WHERE OryggiEmployeeId IS NULL
UNION ALL
SELECT 'Total Employees (not deleted)', COUNT(*)
FROM Employees
WHERE IsDeleted = 0
UNION ALL
SELECT 'Total Users (not deleted)', COUNT(*)
FROM Users
WHERE IsDeleted = 0
UNION ALL
SELECT 'Total Employees (including deleted)', COUNT(*)
FROM Employees
UNION ALL
SELECT 'Total Users (including deleted)', COUNT(*)
FROM Users
"@
    $reader3 = $cmd3.ExecuteReader()
    while ($reader3.Read()) {
        $info = $reader3['Info']
        $count = $reader3['Count']
        if ($info -like "*NULL*") {
            Write-Host "$info : $count" -ForegroundColor Cyan
        } else {
            Write-Host "$info : $count" -ForegroundColor White
        }
    }
    $reader3.Close()

    $connection.Close()
    Write-Host ""
    Write-Host "Database check completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}
