@echo off
echo ============================================
echo  Running .NET Setup (FIXED VERSION)
echo ============================================
echo.

powershell -ExecutionPolicy Bypass -File setup-solution-fixed.ps1

echo.
echo ============================================
echo  Setup completed!
echo ============================================
echo.
echo Log files created:
echo   - setup-log.txt (full log)
echo   - setup-errors.txt (errors only, if any)
echo.
echo To view errors, run: type setup-errors.txt
echo.
pause
