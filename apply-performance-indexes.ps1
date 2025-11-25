$script = Get-Content 'C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.Infrastructure\AddPerformanceIndexes.sql' -Raw
Invoke-Sqlcmd -ServerInstance "LAPTOP-NF9BTG7Q\SQLEXPRESS" -Database "ComplaintManagementDB" -Query $script -QueryTimeout 120 -Verbose
