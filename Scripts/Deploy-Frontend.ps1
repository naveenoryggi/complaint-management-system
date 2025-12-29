# Deploy Angular frontend to IIS WWW folder
$sourcePath = "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-angular\dist\complaint-system-angular\browser"
$destPath = "C:\Program Files\ComplaintManagement\WWW"

Write-Host "Deploying Angular frontend..." -ForegroundColor Yellow

# Copy all files
robocopy $sourcePath $destPath /E /R:1 /W:1 /NFL /NDL /NJH /NJS

Write-Host "Frontend deployed successfully!" -ForegroundColor Green
