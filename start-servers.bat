@echo off
echo Killing old processes...
taskkill /F /IM node.exe >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1
timeout /t 2 >nul

echo Starting Backend...
cd "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet\src\ComplaintManagement.API"
start "BACKEND" cmd /k "dotnet run"

echo Waiting 20 seconds for backend...
timeout /t 20 >nul

echo Starting Frontend...
cd "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-angular"
start "FRONTEND" cmd /k "npm start"

echo Waiting 40 seconds for frontend...
timeout /t 40 >nul

echo Servers should be ready!
echo Backend: http://localhost:5000
echo Frontend: http://localhost:4200
pause
