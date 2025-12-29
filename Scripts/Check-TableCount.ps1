$conn = New-Object System.Data.SqlClient.SqlConnection('Server=.\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;')
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
$result = $cmd.ExecuteScalar()
Write-Host "Tables in database: $result"
$conn.Close()
