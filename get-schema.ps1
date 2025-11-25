$connectionString = 'Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;'
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()

# Get all tables
$command = $connection.CreateCommand()
$command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
$dataset = New-Object System.Data.DataSet
$adapter.Fill($dataset) | Out-Null

Write-Host "=== DATABASE TABLES ===" -ForegroundColor Cyan
foreach ($row in $dataset.Tables[0].Rows) {
    Write-Host $row.TABLE_NAME
}

# Get Complaints table schema
Write-Host "`n=== COMPLAINTS TABLE COLUMNS ===" -ForegroundColor Cyan
$command.CommandText = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Complaints' ORDER BY ORDINAL_POSITION"
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
$dataset = New-Object System.Data.DataSet
$adapter.Fill($dataset) | Out-Null

foreach ($row in $dataset.Tables[0].Rows) {
    Write-Host "$($row.COLUMN_NAME) ($($row.DATA_TYPE))"
}

$connection.Close()
