# Check for ComplaintManagement service
Get-Service | Where-Object { $_.Name -like '*Complaint*' -or $_.DisplayName -like '*Complaint*' } | Format-Table Name, DisplayName, Status
