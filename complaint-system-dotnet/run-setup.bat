@echo off
echo ============================================
echo  Running .NET Setup with Logging
echo ============================================
echo.

powershell -ExecutionPolicy Bypass -File setup-solution-with-logging.ps1

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
